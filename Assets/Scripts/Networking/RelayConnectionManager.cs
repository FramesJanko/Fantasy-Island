using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Scenes;
using UnityEngine;

namespace FantasyIsland.Networking
{
    /// Drives cross-network multiplayer for the Netcode-for-Entities game via
    /// Unity Gaming Services (Relay + Lobby). This is the M8 replacement for the old
    /// localhost auto-connect: because all traffic tunnels through Unity Relay, two
    /// players on different home networks connect with no port forwarding.
    ///
    /// Model: one player Hosts (their process runs the Netcode *server* world plus a
    /// local *client* world). Everyone else Joins as a pure client. Discovery is a
    /// live lobby browser backed by UGS Lobby; the Relay join code is stored in the
    /// lobby's data so joiners never type a code by hand.
    ///
    /// UI (see RelayLobbyUI) subscribes to the events below; this class owns no UI.
    public class RelayConnectionManager : MonoBehaviour
    {
        public static RelayConnectionManager Instance { get; private set; }

        [Tooltip("Total players per session, host included (2 = host + 1 joiner).")]
        [SerializeField] int maxPlayers = 2;

        [Tooltip("Relay connection encryption. 'dtls' is the recommended secure default " +
                 "for desktop; use 'wss' only for WebGL builds.")]
        [SerializeField] string relayConnectionType = "dtls";

        [Tooltip("The gameplay SubScene(s) to stream into each Netcode server/client world " +
                 "on connect. A SubScene's auto-load only targets worlds that exist when the " +
                 "scene first opens, and our netcode worlds are created later (on Host/Join), " +
                 "so they must be loaded explicitly here. Drag the Gameplay SubScene(s) in.")]
        [SerializeField] SubScene[] gameplaySubScenes;

        // Lobby data key under which the Relay join code is published to lobby members.
        const string JoinCodeKey = "joinCode";
        // Lobbies expire after ~30s without a heartbeat, so the host pings well inside that.
        const float HeartbeatInterval = 15f;

        public enum State { Offline, SigningIn, Hosting, Joining, Connected }
        public State CurrentState { get; private set; } = State.Offline;

        /// True once this process is the session host (runs the server world).
        public bool IsHost { get; private set; }

        // ── Events for the UI ──────────────────────────────────────────────────
        /// Human-readable status line ("Signing in…", "Hosting 'Rheck's game'", errors).
        public event Action<string> OnStatusChanged;
        /// Result of a lobby-browser refresh.
        public event Action<List<Lobby>> OnLobbyListUpdated;
        /// Fired when a session is up (hosted or joined). Argument is the Relay join code
        /// (handy to display when hosting; empty for a pure joiner if unknown).
        public event Action<string> OnSessionStarted;

        Lobby m_CurrentLobby;
        Coroutine m_Heartbeat;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void SetStatus(string message)
        {
            Debug.Log($"[Relay] {message}");
            OnStatusChanged?.Invoke(message);
        }

        // ── Services / auth ────────────────────────────────────────────────────

        /// Initialize UGS and sign in anonymously. Safe to call repeatedly.
        public async Task EnsureSignedInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                CurrentState = State.SigningIn;
                SetStatus("Initializing Unity Services…");
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                SetStatus("Signing in…");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            if (CurrentState == State.SigningIn)
                CurrentState = State.Offline;
        }

        // ── Host ───────────────────────────────────────────────────────────────

        /// Create a Relay allocation, publish it in a named public lobby, then bring up
        /// the Netcode server world and this process's local client — both through Relay.
        public async Task HostAsync(string lobbyName)
        {
            try
            {
                await EnsureSignedInAsync();
                CurrentState = State.Hosting;
                if (string.IsNullOrWhiteSpace(lobbyName))
                    lobbyName = $"{AuthenticationService.Instance.PlayerId}'s game";
                SetStatus($"Allocating Relay for \"{lobbyName}\"…");

                // maxConnections = peers that connect to the Relay host. In this
                // host-as-server model the host's OWN local client also connects
                // through Relay, so budget one slot per player (host + joiners).
                int maxConnections = Mathf.Max(1, maxPlayers);
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // Server binds as the Relay host; the local client joins its own code.
                RelayServerData serverData = allocation.ToRelayServerData(relayConnectionType);
                JoinAllocation localJoin = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData clientData = localJoin.ToRelayServerData(relayConnectionType);

                // Publish the join code to lobby members so the browser can join hands-free.
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                };
                m_CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                m_Heartbeat = StartCoroutine(HeartbeatLoop(m_CurrentLobby.Id));

                StartServerAndLocalClient(serverData, clientData);

                IsHost = true;
                CurrentState = State.Connected;
                SetStatus($"Hosting \"{lobbyName}\" (join code {joinCode})");
                OnSessionStarted?.Invoke(joinCode);
            }
            catch (Exception e)
            {
                CurrentState = State.Offline;
                SetStatus($"Host failed: {e.Message}");
                Debug.LogException(e);
            }
        }

        IEnumerator HeartbeatLoop(string lobbyId)
        {
            var wait = new WaitForSecondsRealtime(HeartbeatInterval);
            while (true)
            {
                yield return wait;
                // Fire-and-forget; a failed ping shouldn't spam or stop the loop.
                _ = SafeHeartbeatAsync(lobbyId);
            }
        }

        async Task SafeHeartbeatAsync(string lobbyId)
        {
            try { await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId); }
            catch (Exception e) { Debug.LogWarning($"[Relay] Heartbeat failed: {e.Message}"); }
        }

        // ── Browse / join ────────────────────────────────────────────────────

        /// Query open lobbies for the browser. Fires OnLobbyListUpdated with the results.
        public async Task<List<Lobby>> RefreshLobbiesAsync()
        {
            try
            {
                await EnsureSignedInAsync();
                var options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        // Only lobbies with at least one open slot.
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    }
                };
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                SetStatus($"Found {response.Results.Count} lobby(s)");
                OnLobbyListUpdated?.Invoke(response.Results);
                return response.Results;
            }
            catch (Exception e)
            {
                SetStatus($"Refresh failed: {e.Message}");
                Debug.LogException(e);
                return new List<Lobby>();
            }
        }

        /// Join a lobby (from the browser), read its Relay join code, and bring up the
        /// Netcode client world through Relay.
        public async Task JoinLobbyAsync(string lobbyId)
        {
            try
            {
                await EnsureSignedInAsync();
                CurrentState = State.Joining;
                SetStatus("Joining lobby…");

                m_CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                if (!m_CurrentLobby.Data.TryGetValue(JoinCodeKey, out DataObject joinCodeData)
                    || string.IsNullOrEmpty(joinCodeData.Value))
                    throw new Exception("Lobby has no Relay join code.");

                string joinCode = joinCodeData.Value;
                SetStatus("Connecting through Relay…");
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData clientData = joinAllocation.ToRelayServerData(relayConnectionType);

                StartClient(clientData);

                IsHost = false;
                CurrentState = State.Connected;
                SetStatus("Connected");
                OnSessionStarted?.Invoke(joinCode);
            }
            catch (Exception e)
            {
                CurrentState = State.Offline;
                SetStatus($"Join failed: {e.Message}");
                Debug.LogException(e);
            }
        }

        // ── Netcode world creation ───────────────────────────────────────────

        void StartServerAndLocalClient(RelayServerData serverData, RelayServerData clientData)
        {
            DestroyNetworkWorlds();

            // The driver is built during world creation, so install Relay data first.
            NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(serverData, clientData);

            World serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            // Restore the default so any later world isn't accidentally Relay-bound.
            NetworkStreamReceiveSystem.DriverConstructor = DefaultDriverBuilder.DefaultDriverConstructor;

            // For Relay the bind endpoint is irrelevant (the Relay endpoint lives in the
            // driver settings), so AnyIpv4 is fine.
            using (var q = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>()))
                q.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.AnyIpv4);

            using (var q = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>()))
                q.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, clientData.Endpoint);

            LoadGameplaySubScenes(serverWorld);
            LoadGameplaySubScenes(clientWorld);

            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        void StartClient(RelayServerData clientData)
        {
            DestroyNetworkWorlds();

            NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(null, clientData);
            World clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            NetworkStreamReceiveSystem.DriverConstructor = DefaultDriverBuilder.DefaultDriverConstructor;

            using (var q = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>()))
                q.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, clientData.Endpoint);

            LoadGameplaySubScenes(clientWorld);

            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        /// Stream the configured gameplay SubScene(s) into a freshly created netcode world.
        /// The server needs them so GameSpawner/spawn systems have data to act on; the
        /// client needs them for matching baked/pre-spawned entities.
        void LoadGameplaySubScenes(World world)
        {
            if (gameplaySubScenes == null)
                return;
            foreach (var subScene in gameplaySubScenes)
            {
                if (subScene == null)
                    continue;
                if (subScene.SceneGUID != default)
                    SceneSystem.LoadSceneAsync(world.Unmanaged, subScene.SceneGUID);
                else
                    Debug.LogWarning($"[Relay] SubScene '{subScene.name}' has no valid GUID; skipping.");
            }
        }

        /// Tear down any existing Netcode client/server worlds (e.g. before re-hosting).
        /// The local/default bootstrap world is left alone.
        static void DestroyNetworkWorlds()
        {
            var toDispose = new List<World>();
            foreach (var world in World.All)
            {
                if (IsNetcodeWorld(world))
                    toDispose.Add(world);
            }
            foreach (var world in toDispose)
                world.Dispose();
        }

        // GameServer/GameClient/GameThinClient each OR in the shared `Live` bit that the
        // local `Game` world also carries, so a plain `& (mask) != 0` test would match — and
        // wrongly dispose — the local menu world. Test each flag in full instead.
        static bool IsNetcodeWorld(World world)
        {
            return (world.Flags & WorldFlags.GameServer) == WorldFlags.GameServer
                || (world.Flags & WorldFlags.GameClient) == WorldFlags.GameClient
                || (world.Flags & WorldFlags.GameThinClient) == WorldFlags.GameThinClient;
        }

        // ── Leave / cleanup ──────────────────────────────────────────────────

        /// Leave the current session: tear down worlds and remove/delete the lobby.
        public async Task LeaveAsync()
        {
            if (m_Heartbeat != null)
            {
                StopCoroutine(m_Heartbeat);
                m_Heartbeat = null;
            }

            DestroyNetworkWorlds();

            try
            {
                if (m_CurrentLobby != null)
                {
                    if (IsHost)
                        await LobbyService.Instance.DeleteLobbyAsync(m_CurrentLobby.Id);
                    else
                        await LobbyService.Instance.RemovePlayerAsync(m_CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                }
            }
            catch (Exception e) { Debug.LogWarning($"[Relay] Lobby cleanup: {e.Message}"); }

            m_CurrentLobby = null;
            IsHost = false;
            CurrentState = State.Offline;
            SetStatus("Left session");
        }

        async void OnApplicationQuit()
        {
            if (CurrentState == State.Connected)
                await LeaveAsync();
        }
    }
}

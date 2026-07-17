using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyIsland.Networking
{
    /// Minimal lobby-browser UI over <see cref="RelayConnectionManager"/>.
    ///
    /// Wire in the Inspector:
    ///  • Lobby Name Input – TMP_InputField the host types a name into.
    ///  • Host Button      – calls HostAsync.
    ///  • Refresh Button   – re-queries the lobby list.
    ///  • Lobby List Content – a Vertical Layout Group container the list buttons spawn into.
    ///  • Lobby Button Prefab – a Button with a child TMP_Text; one is spawned per lobby.
    ///  • Status Text      – shows connection status / errors.
    ///  • Menu Root        – (optional) panel hidden once a session starts.
    public class RelayLobbyUI : MonoBehaviour
    {
        [SerializeField] RelayConnectionManager manager;

        [Header("Host")]
        [SerializeField] TMP_InputField lobbyNameInput;
        [SerializeField] Button hostButton;

        [Header("Browse")]
        [SerializeField] Button refreshButton;
        [SerializeField] Transform lobbyListContent;
        [SerializeField] Button lobbyButtonPrefab;

        [Header("Feedback")]
        [SerializeField] TMP_Text statusText;
        [SerializeField] GameObject menuRoot;

        readonly List<Button> m_SpawnedButtons = new List<Button>();

        void Awake()
        {
            if (manager == null)
                manager = RelayConnectionManager.Instance ?? FindObjectOfType<RelayConnectionManager>();
        }

        void OnEnable()
        {
            if (manager == null)
                return;
            manager.OnStatusChanged += HandleStatus;
            manager.OnLobbyListUpdated += HandleLobbyList;
            manager.OnSessionStarted += HandleSessionStarted;

            if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
            if (refreshButton != null) refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        void OnDisable()
        {
            if (manager != null)
            {
                manager.OnStatusChanged -= HandleStatus;
                manager.OnLobbyListUpdated -= HandleLobbyList;
                manager.OnSessionStarted -= HandleSessionStarted;
            }
            if (hostButton != null) hostButton.onClick.RemoveListener(OnHostClicked);
            if (refreshButton != null) refreshButton.onClick.RemoveListener(OnRefreshClicked);
        }

        async void OnHostClicked()
        {
            string name = lobbyNameInput != null ? lobbyNameInput.text : null;
            await manager.HostAsync(name);
        }

        async void OnRefreshClicked()
        {
            await manager.RefreshLobbiesAsync();
        }

        void HandleStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        void HandleLobbyList(List<Lobby> lobbies)
        {
            foreach (var button in m_SpawnedButtons)
                if (button != null)
                    Destroy(button.gameObject);
            m_SpawnedButtons.Clear();

            if (lobbyListContent == null || lobbyButtonPrefab == null)
                return;

            foreach (var lobby in lobbies)
            {
                Button button = Instantiate(lobbyButtonPrefab, lobbyListContent);
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = $"{lobby.Name}  ({lobby.MaxPlayers - lobby.AvailableSlots}/{lobby.MaxPlayers})";

                string lobbyId = lobby.Id; // capture for the closure
                button.onClick.AddListener(async () => await manager.JoinLobbyAsync(lobbyId));
                m_SpawnedButtons.Add(button);
            }
        }

        void HandleSessionStarted(string joinCode)
        {
            // Hide the menu once we're in a session so gameplay is unobstructed.
            if (menuRoot != null)
                menuRoot.SetActive(false);
        }
    }
}

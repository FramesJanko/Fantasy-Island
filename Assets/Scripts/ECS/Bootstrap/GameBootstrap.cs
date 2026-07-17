using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Netcode world bootstrap.
    ///
    /// M8 (UGS Relay + Lobby): we no longer auto-create the client/server worlds or
    /// auto-connect to localhost. Only the local (default) world is created at startup
    /// so the menu/lobby UI can run. RelayConnectionManager creates the server/client
    /// worlds on demand — when a player Hosts or Joins a lobby — and connects them
    /// through Unity Relay, which is what lets players on different home networks meet
    /// without port forwarding.
    ///
    /// Offline/local iteration: to temporarily restore the old localhost flow (e.g. for
    /// quick single-machine testing with PlayMode Tools set to Client & Server), set
    /// AutoConnectPort = 7979 and return base.Initialize(defaultWorldName) instead.
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 0; // no localhost auto-connect; Relay drives connections
            CreateLocalWorld(defaultWorldName);
            return true;
        }
    }
}

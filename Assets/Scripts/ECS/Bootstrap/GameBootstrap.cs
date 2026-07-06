using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Netcode world bootstrap. For local iteration (M2-M7) we auto-create the client
    /// and server worlds and auto-connect to localhost, so pressing Play in the Editor
    /// (with PlayMode Tools set to Client & Server) just works.
    ///
    /// M8 (UGS Relay + Lobby) will replace the auto-connect: set AutoConnectPort = 0
    /// here and drive connection/listen through the Relay-configured transport instead.
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979; // localhost auto-connect for local testing
            return base.Initialize(defaultWorldName);
        }
    }
}

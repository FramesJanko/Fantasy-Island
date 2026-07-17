using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport.Relay;

namespace FantasyIsland.Networking
{
    /// Feeds UGS Relay data into Netcode for Entities' driver creation.
    ///
    /// Netcode builds each world's transport driver by calling the currently
    /// installed <see cref="INetworkStreamDriverConstructor"/> during world creation,
    /// so <see cref="RelayConnectionManager"/> assigns an instance of this to
    /// <c>NetworkStreamReceiveSystem.DriverConstructor</c> *before* creating the
    /// server/client worlds. That swaps the default raw-UDP socket for a driver that
    /// tunnels through Relay — which is what lets two players on different home
    /// networks reach each other without port forwarding.
    ///
    /// A host supplies both sets of data: <paramref name="serverData"/> for the server
    /// world (it binds as the Relay host) and <paramref name="clientData"/> for the
    /// host's own local client (it joins its own allocation through Relay). A pure
    /// joiner supplies only <c>clientData</c>.
    public sealed class RelayDriverConstructor : INetworkStreamDriverConstructor
    {
        RelayServerData m_ServerData;
        RelayServerData m_ClientData;
        readonly bool m_HasServer;
        readonly bool m_HasClient;

        public RelayDriverConstructor(RelayServerData? serverData, RelayServerData? clientData)
        {
            if (serverData.HasValue)
            {
                m_ServerData = serverData.Value;
                m_HasServer = true;
            }
            if (clientData.HasValue)
            {
                m_ClientData = clientData.Value;
                m_HasClient = true;
            }
        }

        public void CreateServerDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug)
        {
            if (!m_HasServer)
            {
                // No relay server data was provided (e.g. a pure client). Fall back to
                // the default so world creation doesn't throw; this driver just won't
                // be used to listen.
                DefaultDriverBuilder.RegisterServerDriver(world, ref driver, netDebug);
                return;
            }
            DefaultDriverBuilder.RegisterServerDriver(world, ref driver, netDebug, ref m_ServerData);
        }

        public void CreateClientDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug)
        {
            if (!m_HasClient)
            {
                DefaultDriverBuilder.RegisterClientDriver(world, ref driver, netDebug);
                return;
            }
            DefaultDriverBuilder.RegisterClientDriver(world, ref driver, netDebug, ref m_ClientData);
        }
    }
}

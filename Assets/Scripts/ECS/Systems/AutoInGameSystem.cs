using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Puts every established connection "in game" automatically on both the client and
    /// the server, which is what actually starts ghost replication. For v1 there is no
    /// menu/handshake gating this, so we flip it on as soon as a connection exists.
    /// (M8 will gate this behind the lobby flow.)
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    partial struct AutoInGameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (id, entity) in SystemAPI
                         .Query<RefRO<NetworkId>>()
                         .WithNone<NetworkStreamInGame>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

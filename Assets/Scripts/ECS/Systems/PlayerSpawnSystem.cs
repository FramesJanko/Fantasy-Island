using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace FantasyIsland
{
    /// Server-only: spawns one player ghost per connection the first time it is in-game,
    /// stamps it with that connection's NetworkId as its GhostOwner, and offsets the
    /// spawn a little per client so they don't overlap.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct PlayerSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawner>();
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().Prefab;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (id, connection) in SystemAPI
                         .Query<RefRO<NetworkId>>()
                         .WithAll<NetworkStreamInGame>()
                         .WithNone<PlayerSpawned>()
                         .WithEntityAccess())
            {
                // Guard so each connection only ever spawns a single player.
                ecb.AddComponent<PlayerSpawned>(connection);

                var player = ecb.Instantiate(prefab);
                ecb.SetComponent(player, new GhostOwner { NetworkId = id.ValueRO.Value });
                ecb.SetComponent(player, LocalTransform.FromPosition(new float3(id.ValueRO.Value * 2f, 0f, 0f)));
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

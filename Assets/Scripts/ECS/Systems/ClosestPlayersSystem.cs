using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct ClosestPlayersSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach(var (transform, distances, npcEntity) in SystemAPI
                        .Query<RefRO<LocalTransform>, DynamicBuffer<DistanceToPlayers>>()
                        .WithAll<NpcTag>()
                        .WithEntityAccess())
            {
                distances.Clear();

                foreach( var (playerTransform, playerEntity) in SystemAPI
                            .Query<RefRO<LocalTransform>>()
                            .WithAll<PlayerTag>()
                            .WithEntityAccess())
                {
                    float dist = math.distance(transform.ValueRO.Position, playerTransform.ValueRO.Position);
                    distances.Add(new DistanceToPlayers {Player = playerEntity, Value = dist});
                }
            }
        }
    }
}
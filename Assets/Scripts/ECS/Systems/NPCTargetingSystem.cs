using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct NpcTargetingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (transform, distances, distanceToTarget, target, npcAi, entity) in SystemAPI
                    .Query<RefRO<LocalTransform>, DynamicBuffer<DistanceToPlayers>, RefRW<DistanceToTarget>, RefRW<TargetRef>, RefRO<NpcAi>>()
                    .WithAll<NpcTag>()
                    .WithEntityAccess())
            {

                float distFromHome = math.distance(transform.ValueRO.Position, npcAi.ValueRO.Home);
                if(target.ValueRO.Value != Entity.Null)
                {
                    if(distFromHome > npcAi.ValueRO.LeashRange)
                    {
                        target.ValueRW.Value = Entity.Null;
                        // ecb.SetComponentEnabled<TargetRef>(entity, false);
                    }
                    continue;
                }
                if(distFromHome < npcAi.ValueRO.AggroRange)
                {
                    
                    Entity closest = Entity.Null;
                    float closestDist = npcAi.ValueRO.HuntingDistance;

                    foreach( var distance in distances)
                    {
                        if(distance.Value < closestDist)
                        {
                            closest = distance.Player;
                            closestDist = distance.Value;
                        }
                    }
                    target.ValueRW.Value = closest;
                    // ecb.SetComponentEnabled<TargetRef>(entity, true);
                }

            }
        }
    }
}
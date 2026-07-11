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
            foreach (var (distances, distanceToTarget, target, npcAi, entity) in SystemAPI
                    .Query<DynamicBuffer<DistanceToPlayers>, RefRW<DistanceToTarget>, RefRW<TargetRef>, RefRO<NpcAi>>()
                    .WithAll<NpcTag>()
                    .WithEntityAccess())
            {
                if(target.ValueRO.Value != Entity.Null)
                {
                    if(distanceToTarget.ValueRO.Value > npcAi.ValueRO.LeashRange)
                    {
                        target.ValueRW.Value = Entity.Null;
                    }
                    break;
                }

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
            }
        }
    }
}
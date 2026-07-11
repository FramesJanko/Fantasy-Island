using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct CalculateDistanceToTargetSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (distance, target, transform, entity) in SystemAPI
                    .Query<RefRW<DistanceToTarget>, RefRW<TargetRef>, RefRO<LocalTransform>>()
                    .WithEntityAccess())
            {
                if(target.ValueRO.Value != Entity.Null && SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value))
                {
                    var targetTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.Value);
                    float dist = math.distance(transform.ValueRO.Position, targetTransform.Position);
                    distance.ValueRW.Value = dist;
                }
            }
        }
    }
}
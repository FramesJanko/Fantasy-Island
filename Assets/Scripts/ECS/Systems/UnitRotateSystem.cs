using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FantasyIsland
{
    partial struct UnitRotateSystem : ISystem
    {
        void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, target, mode, entity) in SystemAPI
                    .Query<RefRW<LocalTransform>, RefRO<TargetRef>, RefRO<UnitMode>>()
                    .WithEntityAccess())
            {
                if(mode.ValueRO.CurrentMode == UnitMode.Mode.Attacking)
                {
                    
                    if(target.ValueRO.Value == Entity.Null) continue;
                    var targetTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.Value);
                    transform.ValueRW.Rotation = quaternion.LookRotationSafe(targetTransform.Position - transform.ValueRO.Position, math.up());
                }
            }
        }
    }
}
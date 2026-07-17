using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct UnitSetMoveDestinationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {

            foreach (var (target, unitMode, moveDest, entity) in SystemAPI
                    .Query<RefRO<TargetRef>, RefRO<UnitMode>, RefRW<MoveDestination>>()
                    .WithPresent<MoveDestination>()
                    .WithEntityAccess())
            {
                
                if(target.ValueRO.Value != Entity.Null)
                {
                    Entity targetEntity = target.ValueRO.Value;
                    if(unitMode.ValueRO.CurrentMode != UnitMode.Mode.Attacking && SystemAPI.HasComponent<LocalTransform>(targetEntity))
                    {
                        moveDest.ValueRW.Value = SystemAPI.GetComponentRO<LocalTransform>(targetEntity).ValueRO.Position;
                        SystemAPI.SetComponentEnabled<MoveDestination>(entity, true);
                    }
                }
                
            }
        }
    }
}
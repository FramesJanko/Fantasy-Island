
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct NpcReturnHomeSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {

            foreach (var (transform, target, npcAi, moveDest, entity) in SystemAPI
                    .Query<RefRO<LocalTransform>, RefRO<TargetRef>, RefRO<NpcAi>, RefRW<MoveDestination>>()
                    .WithPresent<MoveDestination>()
                    .WithEntityAccess())
            {
                
                if(target.ValueRO.Value == Entity.Null)
                {
                    float dist = math.distance(transform.ValueRO.Position, npcAi.ValueRO.Home);
                    if(dist > 0.15f)
                    {
                        moveDest.ValueRW.Value = npcAi.ValueRO.Home;
                        SystemAPI.SetComponentEnabled<MoveDestination>(entity, true);

                    }
                }
                
            }
        }
    }
}
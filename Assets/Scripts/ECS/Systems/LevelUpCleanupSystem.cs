using Unity.Entities;

namespace FantasyIsland
{

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    partial struct LevelUpCleanupSystem : ISystem
    {
        void OnUpdate(ref SystemState state)
        {
            foreach(var leveledup in SystemAPI.Query<EnabledRefRW<LeveledUp>>())
            {
                leveledup.ValueRW = false;
            }
        }
    }
}
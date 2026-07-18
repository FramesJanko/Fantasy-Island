using Unity.Collections;
using Unity.Entities;

namespace FantasyIsland
{

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct ExperienceSystem : ISystem
    {
        public FixedList128Bytes<int> Thresholds;
        void OnCreate(ref SystemState state)
        {
            Thresholds = new FixedList128Bytes<int> {0, 200, 500, 900, 1400, 2000};
        }
        void OnUpdate(ref SystemState state)
        {
            foreach(var (experience, level, ap, apEnabled, leveledUp, leveledUpEnabled) in SystemAPI
                    .Query<RefRO<Experience>, RefRW<UnitLevel>, RefRW<AbilityPoints>, EnabledRefRW<AbilityPoints>, RefRW<LeveledUp>, EnabledRefRW<LeveledUp>>()
                    .WithPresent<AbilityPoints>()
                    .WithPresent<LeveledUp>())   // include entities whose LeveledUp is disabled
            {
                int gained = 0;

                // Keep leveling while the XP clears the next level's threshold, so a big
                // single XP award can grant several levels in one tick. Stop at max level.
                while(level.ValueRO.Value < Thresholds.Length &&
                      experience.ValueRO.TotalExperience >= Thresholds[level.ValueRO.Value])
                {
                    level.ValueRW.Value += 1;
                    if(apEnabled.ValueRO == false) apEnabled.ValueRW = true;
                    ap.ValueRW.Value += 1;
                    gained++;
                }

                if(gained > 0)
                {
                    leveledUp.ValueRW.LevelsGained = gained;
                    leveledUpEnabled.ValueRW = true;   // "fire" the event for this tick
                }
            }
        }
    }
}
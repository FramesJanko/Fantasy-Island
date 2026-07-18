using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct CalculateStatsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (combat, speed, health, entity) in SystemAPI
                    .Query<RefRW<CombatStats>, RefRW<MoveSpeed>, RefRW<Health>>()
                    .WithEntityAccess())
            {
                combat.ValueRW.Damage = combat.ValueRO.BaseDamage + combat.ValueRO.Strength;
                combat.ValueRW.AttackTime = combat.ValueRO.BaseAttackTime * 25 / (25 + combat.ValueRO.Agility);
                speed.ValueRW.Movespeed = speed.ValueRW.BaseMovespeed + (combat.ValueRO.Strength + combat.ValueRO.Agility)/25;
                var old = health.ValueRO.Max;
                health.ValueRW.Max = health.ValueRO.BaseHp + combat.ValueRO.Strength * 5;
                if(health.ValueRO.Max != old) health.ValueRW.Current += health.ValueRO.Max - old;
            }
        }
    }
}
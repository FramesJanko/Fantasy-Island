using Unity.Entities;
using Unity.Mathematics;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct UnitAttackingSystem : ISystem
    {
        ComponentLookup<Health> _healthLookup;

        public void OnCreate(ref SystemState state)
        {
            _healthLookup = state.GetComponentLookup<Health>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            _healthLookup.Update(ref state);

            foreach(var (distance, target, combat, mode, attack, health, entity) in SystemAPI
                    .Query<RefRO<DistanceToTarget>, RefRO<TargetRef>, RefRO<CombatStats>, RefRW<UnitMode>, RefRW<AttackState>, RefRO<Health>>()
                    .WithEntityAccess())
            {
                if(target.ValueRO.Value == Entity.Null)
                {
                    attack.ValueRW.Progress = 0;
                    continue;

                }
                if(!health.ValueRO.Dead && distance.ValueRO.Value <= combat.ValueRO.BaseRange)
                {
                    mode.ValueRW.CurrentMode = UnitMode.Mode.Attacking;
                    SystemAPI.SetComponentEnabled<MoveDestination>(entity, false);
                }
                else if(mode.ValueRO.CurrentMode == UnitMode.Mode.Attacking && distance.ValueRO.Value > combat.ValueRO.AttackRange)
                {
                    mode.ValueRW.CurrentMode = UnitMode.Mode.Idle;
                    attack.ValueRW.Progress = 0;
                }
                if(mode.ValueRO.CurrentMode != UnitMode.Mode.Attacking)
                {
                    attack.ValueRW.Progress = 0;
                }
                if(mode.ValueRO.CurrentMode == UnitMode.Mode.Attacking)
                {
                    attack.ValueRW.Progress += dt;
                    if(attack.ValueRW.Progress > combat.ValueRO.AttackTime)
                    {
                        attack.ValueRW.Progress = 0;
                        attack.ValueRW.AttackSucceeded = true;
                        mode.ValueRW.CurrentMode = UnitMode.Mode.Idle;

                        Entity targetEntity = target.ValueRO.Value;
                        if(_healthLookup.HasComponent(targetEntity))
                        {
                            RefRW<Health> targetHealth = _healthLookup.GetRefRW(targetEntity);
                            targetHealth.ValueRW.Current = math.max(0f, targetHealth.ValueRO.Current - combat.ValueRO.Damage);
                        }
                    }
                }
            }
        }
    }
}
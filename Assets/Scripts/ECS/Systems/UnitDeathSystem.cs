using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Server-only: turns depleted Health into entity death. Runs after combat so damage
    /// dealt this tick is accounted for. Agnostic to unit type - anything with a Health
    /// component dies the same way (players and NPCs alike). Because ghosts despawn on the
    /// clients automatically when the server entity is destroyed, no explicit RPC is needed.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UnitAttackingSystem))]
    partial struct UnitDeathSystem : ISystem
    {
        ComponentLookup<Health> _healthLookup;

        public void OnCreate(ref SystemState state)
        {
            _healthLookup = state.GetComponentLookup<Health>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _healthLookup.Update(ref state);
            var em = state.EntityManager;

            // 1) Drop any target that is dead or already gone, so attackers and NPCs stop
            //    chasing a corpse and never hold a dangling Entity reference. WithPresent
            //    covers disabled TargetRefs too. The codebase treats Entity.Null as "no
            //    target", so clearing Value is enough - mirrors NpcTargetingSystem's leash.
            foreach (var target in SystemAPI
                        .Query<RefRW<TargetRef>>()
                        .WithPresent<TargetRef>())
            {
                Entity t = target.ValueRO.Value;
                if (t == Entity.Null)
                {
                    continue;
                }

                bool dead = !em.Exists(t) ||
                            (_healthLookup.HasComponent(t) && _healthLookup[t].Current <= 0f);
                if (dead)
                {
                    target.ValueRW.Value = Entity.Null;
                }
            }

            // 2) Destroy depleted units. Deferred via ECB so we don't invalidate the
            //    ComponentLookup / query while still iterating this tick.
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (health, entity) in SystemAPI
                        .Query<RefRW<Health>>()
                        .WithNone<PlayerTag>()
                        .WithEntityAccess())
            {
                if (health.ValueRO.Dead)
                {
                    ecb.DestroyEntity(entity);
                }
                if (health.ValueRO.Current <= 0f)
                {
                    health.ValueRW.Dead = true;
                }
            }
            ecb.Playback(em);
            ecb.Dispose();
            foreach(var (health, entity) in SystemAPI
                    .Query<RefRW<Health>>()
                    .WithAll<PlayerTag>()
                    .WithEntityAccess())
            {
                if(health.ValueRO.Current <= 0)
                {
                    health.ValueRW.Dead = true;
                }
            }
        }
    }
}

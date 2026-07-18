using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

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
        const float ExperienceRange = 10f;   // radius around a corpse that shares its XP award

        ComponentLookup<Health> _healthLookup;
        ComponentLookup<Experience> _experienceLookup;

        public void OnCreate(ref SystemState state)
        {
            _healthLookup = state.GetComponentLookup<Health>(isReadOnly: true);
            _experienceLookup = state.GetComponentLookup<Experience>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _healthLookup.Update(ref state);
            _experienceLookup.Update(ref state);
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
                            (_healthLookup.HasComponent(t) && _healthLookup[t].Dead);
                if (dead)
                {
                    target.ValueRW.Value = Entity.Null;
                }
            }

            // 2) Destroy depleted units. Deferred via ECB so we don't invalidate the
            //    ComponentLookup / query while still iterating this tick.
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (health, transform, entity) in SystemAPI
                        .Query<RefRW<Health>, RefRO<LocalTransform>>()
                        .WithNone<PlayerTag>()
                        .WithEntityAccess())
            {
                if (health.ValueRO.Dead)
                {
                    AwardNearbyExperience(ref state, transform.ValueRO.Position, entity);
                    ecb.DestroyEntity(entity);
                }
                if (health.ValueRO.Current <= 0f)
                {
                    health.ValueRW.Dead = true;
                }
            }
            ecb.Playback(em);
            ecb.Dispose();
            foreach(var (health, target, spawnStatus, transform, entity) in SystemAPI
                    .Query<RefRW<Health>, RefRW<TargetRef>, RefRW<SpawnStatus>, RefRO<LocalTransform>>()
                    .WithAll<PlayerTag>()
                    .WithEntityAccess())
            {
                if(health.ValueRO.Current <= 0)
                {
                    AwardNearbyExperience(ref state, transform.ValueRO.Position, entity);
                    health.ValueRW.Dead = true;
                    target.ValueRW.Value = Entity.Null;
                    spawnStatus.ValueRW.DeathCount += 1;
                    spawnStatus.ValueRW.SpawnTimer = spawnStatus.ValueRO.DeathCount * spawnStatus.ValueRO.BaseSpawnTimer;
                    health.ValueRW.Current = health.ValueRW.Max;
                }
            }

        }

        /// Splits a dying unit's Experience.Award evenly among every Experience holder within
        /// ExperienceRange (the dying unit excluded). Agnostic to unit type - receivers are
        /// picked purely by having an Experience component, so players and NPCs level the same
        /// way. Cost is only paid per death, so the nested range scan stays cheap.
        void AwardNearbyExperience(ref SystemState state, float3 center, Entity dying)
        {
            if (!_experienceLookup.HasComponent(dying))
            {
                return;
            }

            int pool = _experienceLookup[dying].Award;
            if (pool <= 0)
            {
                return;
            }

            const float rangeSq = ExperienceRange * ExperienceRange;

            // Pass 1: count in-range receivers so the pool can be divided evenly.
            int receivers = 0;
            foreach (var (transform, entity) in SystemAPI
                        .Query<RefRO<LocalTransform>>()
                        .WithAll<Experience>()
                        .WithEntityAccess())
            {
                if (entity == dying)
                {
                    continue;
                }
                if (math.distancesq(center, transform.ValueRO.Position) <= rangeSq)
                {
                    receivers++;
                }
            }

            if (receivers == 0)
            {
                return;
            }

            int award = pool / receivers;

            // Pass 2: hand each in-range receiver its even share.
            foreach (var (transform, exp, entity) in SystemAPI
                        .Query<RefRO<LocalTransform>, RefRW<Experience>>()
                        .WithEntityAccess())
            {
                if (entity == dying)
                {
                    continue;
                }
                if (math.distancesq(center, transform.ValueRO.Position) <= rangeSq)
                {
                    exp.ValueRW.TotalExperience += award;
                }
            }
        }
    }
}

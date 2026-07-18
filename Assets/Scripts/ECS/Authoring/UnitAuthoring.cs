using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Add to both the Player and NPC ghost prefabs. Bakes the shared unit state
    /// (health, speed, combat stats, target/path scaffolding). MoveDestination starts
    /// disabled so freshly spawned units stay put until commanded.
    public class UnitAuthoring : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;

        [Header("Movement")]
        public float moveSpeed = 5f;

        [Header("Experience")]
        public int startingLevel = 1;
        public int experienceAward = 25;

        [Header("Combat")]
        public float baseAttackRange = 2f;
        public float attackRange = 2.5f;
        public float baseDamage = 10f;
        public float baseAttackTime = 1f;
        public float strength = 1f;
        public float agility = 1f;
        public float intelligence= 1f;

        class UnitBaker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health { BaseHp = authoring.maxHealth, Max = authoring.maxHealth, Current = authoring.maxHealth });
                AddComponent(entity, new MoveSpeed { BaseMovespeed = authoring.moveSpeed });
                AddComponent(entity, new CombatStats
                {
                    BaseRange = authoring.baseAttackRange,
                    AttackRange = authoring.attackRange,
                    BaseDamage = authoring.baseDamage,
                    BaseAttackTime = authoring.baseAttackTime,
                    AttackTime = authoring.baseAttackTime,
                    Strength = authoring.strength,
                    Agility = authoring.agility,
                    Intelligence = authoring.intelligence
                });
                AddComponent(entity, new AbilityPoints());
                AddComponent(entity, new SkillPoints());
                AddComponent(entity, new LeveledUp());
                AddComponent(entity, new UnitLevel { Value = authoring.startingLevel });
                AddComponent(entity, new Experience { Award = authoring.experienceAward });
                AddComponent(entity, new AttackState());
                AddComponent(entity, new UnitMode {CurrentMode = UnitMode.Mode.Idle});
                AddComponent(entity, new TargetRef());
                AddComponent(entity, new DistanceToTarget());
                AddComponent(entity, new UnitName { Value = authoring.gameObject.name });

                AddComponent(entity, new MoveDestination());
                SetComponentEnabled<MoveDestination>(entity, false); // idle until commanded

                AddComponent(entity, new PathCursor());
                AddBuffer<PathWaypoint>(entity);
                AddBuffer<DistanceToPlayers>(entity);

                AddComponent<UnitTag>(entity);
            }
        }
    }
}

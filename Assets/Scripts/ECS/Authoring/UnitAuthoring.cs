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

        [Header("Combat")]
        public float baseAttackRange = 2f;
        public float attackRange = 2.5f;
        public float damage = 10f;
        public float attackTime = 1f;

        class UnitBaker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health { Max = authoring.maxHealth, Current = authoring.maxHealth });
                AddComponent(entity, new MoveSpeed { Value = authoring.moveSpeed });
                AddComponent(entity, new CombatStats
                {
                    BaseRange = authoring.baseAttackRange,
                    AttackRange = authoring.attackRange,
                    Damage = authoring.damage,
                    AttackTime = authoring.attackTime
                });
                AddComponent(entity, new AttackState());
                AddComponent(entity, new TargetRef());
                AddComponent(entity, new DistanceToTarget());
                AddComponent(entity, new UnitName { Value = authoring.gameObject.name });

                AddComponent(entity, new MoveDestination());
                SetComponentEnabled<MoveDestination>(entity, false); // idle until commanded

                AddComponent(entity, new PathCursor());
                AddBuffer<PathWaypoint>(entity);

                AddComponent<UnitTag>(entity);
            }
        }
    }
}

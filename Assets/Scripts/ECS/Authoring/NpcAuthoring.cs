using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Add to the NPC ghost prefab (alongside UnitAuthoring). Tags it as an NPC and
    /// bakes its hunting/leash tuning. Home is filled in at spawn time from the
    /// spawn point (see NpcSpawnSystem).
    public class NpcAuthoring : MonoBehaviour
    {
        [Tooltip("A player closer than this (and inside leash) is acquired as a target.")]
        public float huntingDistance = 10f;

        [Tooltip("If the NPC strays further than this from home, it returns and drops its target.")]
        public float leashRange = 20f;

        class NpcBaker : Baker<NpcAuthoring>
        {
            public override void Bake(NpcAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<NpcTag>(entity);
                AddComponent(entity, new NpcAi
                {
                    HuntingDistance = authoring.huntingDistance,
                    LeashRange = authoring.leashRange
                    // Home is set when the NPC is spawned.
                });
            }
        }
    }
}

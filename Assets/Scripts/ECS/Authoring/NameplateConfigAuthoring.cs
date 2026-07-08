using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Put anywhere in the SubScene (e.g. alongside GameSpawnerAuthoring). Points
    /// at the world-space nameplate prefab; baked into a singleton NameplateSpawnSystem reads.
    public class NameplateConfigAuthoring : MonoBehaviour
    {
        [Tooltip("World-space Canvas prefab with a NameplateViewAuthoring component. " +
                 "Must be a plain Prefab asset, NOT placed inside the SubScene itself.")]
        public GameObject nameplatePrefab;

        class NameplateConfigBaker : Baker<NameplateConfigAuthoring>
        {
            public override void Bake(NameplateConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NameplateConfig { Prefab = authoring.nameplatePrefab });
            }
        }
    }
}

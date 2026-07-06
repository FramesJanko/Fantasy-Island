using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Put on a single empty GameObject inside the SubScene. Holds references to the
    /// Player and NPC ghost prefabs and the NPC spawn points, and bakes them into
    /// singleton components the server spawn systems read.
    public class GameSpawnerAuthoring : MonoBehaviour
    {
        [Tooltip("Player ghost prefab (must have GhostAuthoringComponent with 'Has Owner').")]
        public GameObject playerPrefab;

        [Tooltip("NPC ghost prefab (must have GhostAuthoringComponent).")]
        public GameObject npcPrefab;

        [Tooltip("One NPC is spawned at each of these transforms at server start.")]
        public Transform[] npcSpawnPoints;

        class SpawnerBaker : Baker<GameSpawnerAuthoring>
        {
            public override void Bake(GameSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PlayerSpawner
                {
                    Prefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
                });
                AddComponent(entity, new NpcSpawner
                {
                    Prefab = GetEntity(authoring.npcPrefab, TransformUsageFlags.Dynamic)
                });

                var points = AddBuffer<NpcSpawnPoint>(entity);
                if (authoring.npcSpawnPoints != null)
                {
                    foreach (var t in authoring.npcSpawnPoints)
                    {
                        if (t != null)
                        {
                            points.Add(new NpcSpawnPoint { Position = t.position });
                        }
                    }
                }
            }
        }
    }
}

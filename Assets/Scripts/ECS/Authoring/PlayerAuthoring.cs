using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Add to the Player ghost prefab (alongside UnitAuthoring). Tags it as a player.
    /// Ownership itself comes from the GhostAuthoringComponent ("Has Owner"), which the
    /// server stamps with the connecting client's NetworkId in PlayerSpawnSystem.
    public class PlayerAuthoring : MonoBehaviour
    {
        class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
            }
        }
    }
}

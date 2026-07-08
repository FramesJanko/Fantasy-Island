using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Destroys the nameplate GameObject once its owning unit entity is gone.
    /// NameplateView is a cleanup component, so it survives DestroyEntity - entities
    /// land here still carrying it but missing UnitTag, which is what despawn looks like.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    partial struct NameplateCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (view, entity) in SystemAPI.Query<RefRO<NameplateView>>()
                         .WithNone<UnitTag>()
                         .WithEntityAccess())
            {
                Object.Destroy(view.ValueRO.Root.Value.gameObject);
                ecb.RemoveComponent<NameplateView>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

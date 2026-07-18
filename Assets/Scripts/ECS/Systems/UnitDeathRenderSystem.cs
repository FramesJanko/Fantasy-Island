using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Rendering;

namespace FantasyIsland
{
    /// Client-only: hides the rendered mesh of any unit the server has flagged dead and
    /// shows it again on respawn. Death is server-authoritative but Health.Dead is a
    /// GhostField, so it replicates here for the client to react to. Rendering is a client
    /// concern (the server world never draws), which is why this can't live in the
    /// server-side UnitDeathSystem. Agnostic to unit type.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    partial struct UnitDeathRenderSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (health, entity) in SystemAPI
                         .Query<RefRO<Health>>()
                         .WithEntityAccess())
            {
                SetVisible(em, ecb, entity, visible: !health.ValueRO.Dead);
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        /// A baked prefab hierarchy renders through child entities (one per MeshRenderer),
        /// linked to the root by LinkedEntityGroup - so we toggle every renderable in the
        /// group, not just the root, or multi-mesh models would only half-hide.
        void SetVisible(EntityManager em, EntityCommandBuffer ecb, Entity root, bool visible)
        {
            if (em.HasBuffer<LinkedEntityGroup>(root))
            {
                var group = em.GetBuffer<LinkedEntityGroup>(root);
                for (int i = 0; i < group.Length; i++)
                {
                    Toggle(em, ecb, group[i].Value, visible);
                }
            }
            else
            {
                Toggle(em, ecb, root, visible);
            }
        }

        void Toggle(EntityManager em, EntityCommandBuffer ecb, Entity e, bool visible)
        {
            // Only entities that actually render carry MaterialMeshInfo; skip the rest so we
            // don't add a dangling tag to transform-only nodes.
            if (!em.HasComponent<MaterialMeshInfo>(e))
            {
                return;
            }

            bool hidden = em.HasComponent<DisableRendering>(e);
            if (visible && hidden)
            {
                ecb.RemoveComponent<DisableRendering>(e);
            }
            else if (!visible && !hidden)
            {
                ecb.AddComponent<DisableRendering>(e);
            }
        }
    }
}

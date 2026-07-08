using FantasyIsland;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    partial struct NameplateSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<NameplateConfig>().Prefab.Value;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach(var (_, entity) in SystemAPI.Query<RefRO<UnitTag>>()
                .WithNone<NameplateView>()
                .WithEntityAccess())
            {
                var go = Object.Instantiate(prefab);
                var view = go.GetComponent<NameplateViewAuthoring>();

                ecb.AddComponent(entity, new NameplateView
                {
                    Root = go.transform,
                    NameText = view.NameText,
                    TargetText = view.TargetText
                });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

}
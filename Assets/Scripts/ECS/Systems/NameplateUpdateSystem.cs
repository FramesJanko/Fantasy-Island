using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FantasyIsland
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    partial struct NameplateUpdateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var cam = Camera.main;
            if (cam == null) return;

            float3 camPosition = cam.transform.position;

            foreach (var (view, transform, name, target) in SystemAPI
                         .Query<RefRO<NameplateView>, RefRO<LocalTransform>, RefRO<UnitName>, RefRO<TargetRef>>())
            {
                var root = view.ValueRO.Root.Value;
                var position = transform.ValueRO.Position + new float3(0, 2f, 0);

                root.position = position;
                root.forward = position - camPosition;

                view.ValueRO.NameText.Value.text = name.ValueRO.Value.ToString();

                var targetEntity = target.ValueRO.Value;
                view.ValueRO.TargetText.Value.text = SystemAPI.HasComponent<UnitName>(targetEntity)
                    ? SystemAPI.GetComponent<UnitName>(targetEntity).Value.ToString()
                    : "-";
            }
        }
    }
}
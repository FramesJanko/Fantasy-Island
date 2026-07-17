using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FantasyIsland
{
    /// Client-only: on right-click, raycast the mouse against the world (classic PhysX)
    /// and fire a MoveCommandRpc to the server with the hit point. The server decides
    /// which unit that connection owns and moves it.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Only send commands once we're actually connected to a server.
            RequireForUpdate<NetworkId>();
        }

        protected override void OnUpdate()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.wasPressedThisFrame)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            var ray = cam.ScreenPointToRay(mouse.position.ReadValue());

            // Right-click a unit -> target it. The pick manager stands up transient proxy
            // colliders only for this raycast, so we get the clicked entity's ghost id back.
            var picker = UnitPickManager.Instance;
            if (picker != null && picker.TryPick(ray, out _, out var targetGhostId))
            {
                var targetRequest = EntityManager.CreateEntity();
                EntityManager.AddComponentData(targetRequest, new SetTargetCommandRpc { TargetGhostId = targetGhostId });
                EntityManager.AddComponentData(targetRequest, new SendRpcCommandRequest());
                return;
            }

            // Otherwise right-click the ground -> move there. Exclude the pick layer so this
            // ray can never catch a stray proxy collider.
            int groundMask = picker != null && picker.HasPickLayer ? ~picker.PickLayerMask : ~0;
            if (!Physics.Raycast(ray, out var hit, 1000f, groundMask))
            {
                return;
            }

            // Create a one-shot RPC request entity; Netcode ships it to the server and
            // deletes the client-side copy for us.
            var request = EntityManager.CreateEntity();
            EntityManager.AddComponentData(request, new MoveCommandRpc { Destination = hit.point });
            EntityManager.AddComponentData(request, new SendRpcCommandRequest());
        }
    }
}

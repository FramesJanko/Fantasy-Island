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
            if (!Physics.Raycast(ray, out var hit, 1000f))
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

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Server-only: consumes MoveCommandRpc requests. Maps the sending connection's
    /// NetworkId to the player unit it owns (via GhostOwner) and points that unit at
    /// the requested destination.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct MoveCommandReceiveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Build NetworkId -> owned player entity lookup once per tick.
            var owners = new NativeHashMap<int, Entity>(16, Allocator.Temp);
            foreach (var (owner, entity) in SystemAPI
                         .Query<RefRO<GhostOwner>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                owners[owner.ValueRO.NetworkId] = entity;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (rpc, req, requestEntity) in SystemAPI
                         .Query<RefRO<MoveCommandRpc>, RefRO<ReceiveRpcCommandRequest>>()
                         .WithEntityAccess())
            {
                var source = req.ValueRO.SourceConnection;
                if (SystemAPI.HasComponent<NetworkId>(source))
                {
                    int senderId = SystemAPI.GetComponent<NetworkId>(source).Value;
                    if (owners.TryGetValue(senderId, out var player))
                    {
                        SystemAPI.GetComponentRW<MoveDestination>(player).ValueRW.Value = rpc.ValueRO.Destination;
                        ecb.SetComponentEnabled<MoveDestination>(player, true);

                        // A manual move cancels any combat target, otherwise
                        // UnitSetMoveDestinationSystem would override this destination with
                        // the target's position on the next tick and the unit couldn't leave.
                        if (SystemAPI.HasComponent<TargetRef>(player))
                        {
                            SystemAPI.GetComponentRW<TargetRef>(player).ValueRW.Value = Entity.Null;
                        }
                    }
                }
                ecb.DestroyEntity(requestEntity); // consume the request
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            owners.Dispose();
        }
    }
}

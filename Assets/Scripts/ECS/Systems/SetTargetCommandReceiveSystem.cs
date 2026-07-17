using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Server-only: consumes SetTargetCommandRpc. Resolves the clicked ghost id back to a
    /// server entity and points the sender's owned player unit at it (TargetRef), mirroring
    /// how MoveCommandReceiveSystem routes move commands. Only runs on ticks where a target
    /// command actually arrived, so the ghost lookup isn't built when idle.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct SetTargetCommandReceiveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<SetTargetCommandRpc>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // NetworkId -> owned player entity (who is issuing the command).
            var owners = new NativeHashMap<int, Entity>(16, Allocator.Temp);
            foreach (var (owner, entity) in SystemAPI
                         .Query<RefRO<GhostOwner>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                owners[owner.ValueRO.NetworkId] = entity;
            }

            // ghostId -> server entity, so we can resolve the clicked target. Every ghost
            // carries a GhostInstance whose ghostId matches the client's.
            var ghosts = new NativeHashMap<int, Entity>(256, Allocator.Temp);
            foreach (var (ghost, entity) in SystemAPI
                         .Query<RefRO<GhostInstance>>()
                         .WithEntityAccess())
            {
                ghosts[ghost.ValueRO.ghostId] = entity;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (rpc, req, requestEntity) in SystemAPI
                         .Query<RefRO<SetTargetCommandRpc>, RefRO<ReceiveRpcCommandRequest>>()
                         .WithEntityAccess())
            {
                var source = req.ValueRO.SourceConnection;
                if (SystemAPI.HasComponent<NetworkId>(source))
                {
                    int senderId = SystemAPI.GetComponent<NetworkId>(source).Value;
                    if (owners.TryGetValue(senderId, out var player) &&
                        ghosts.TryGetValue(rpc.ValueRO.TargetGhostId, out var targetEntity) &&
                        targetEntity != player && // ignore clicking your own unit
                        SystemAPI.HasComponent<TargetRef>(player))
                    {
                        SystemAPI.GetComponentRW<TargetRef>(player).ValueRW.Value = targetEntity;
                        ecb.SetComponentEnabled<TargetRef>(player, true);
                    }
                }
                ecb.DestroyEntity(requestEntity); // consume the request
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            owners.Dispose();
            ghosts.Dispose();
        }
    }
}

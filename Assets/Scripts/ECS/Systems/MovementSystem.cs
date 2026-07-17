using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace FantasyIsland
{
    /// Server-only: steers each moving unit along its waypoint buffer. When it reaches
    /// the final waypoint it disables MoveDestination (goes idle). Position is written
    /// to LocalTransform, which is a ghost field, so clients receive it via interpolation.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct MovementSystem : ISystem
    {
        // Distance at which we consider a waypoint "reached".
        const float k_ArriveThreshold = 0.15f;

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, speed, waypoints, cursor, mode, entity) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRO<MoveSpeed>, DynamicBuffer<PathWaypoint>, RefRW<PathCursor>, RefRW<UnitMode>>()
                         .WithAll<MoveDestination>() // enabled == moving
                         .WithEntityAccess())
            {
                if (waypoints.Length == 0)
                {
                    ecb.SetComponentEnabled<MoveDestination>(entity, false);
                    continue;
                }

                int index = math.clamp(cursor.ValueRO.Index, 0, waypoints.Length - 1);
                float3 position = transform.ValueRO.Position;
                float3 toTarget = waypoints[index].Position - position;
                toTarget.y = 0f; // keep movement on the ground plane
                float distance = math.length(toTarget);

                if (distance <= k_ArriveThreshold)
                {
                    if (index + 1 >= waypoints.Length)
                    {
                        mode.ValueRW.CurrentMode = UnitMode.Mode.Idle;
                        // Clear the consumed route on arrival. If left in place, the next move
                        // command re-enables MoveDestination while the cursor still sits on this
                        // already-reached final waypoint, so MovementSystem "arrives" instantly
                        // and disables it again - swallowing the command regardless of distance.
                        waypoints.Clear();
                        cursor.ValueRW.Index = 0;
                        ecb.SetComponentEnabled<MoveDestination>(entity, false); // arrived
                    }
                    else
                    {
                        cursor.ValueRW.Index = index + 1;
                    }
                    continue;
                }
                mode.ValueRW.CurrentMode = UnitMode.Mode.Moving;
                float3 direction = toTarget / distance;
                float step = math.min(speed.ValueRO.Value * dt, distance);
                transform.ValueRW.Position = position + direction * step;
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

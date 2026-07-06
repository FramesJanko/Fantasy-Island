using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace FantasyIsland
{
    /// Server-only: turns a unit's MoveDestination into a path of waypoints.
    ///
    /// v1 is "direct steering": the path is just the destination itself (one waypoint).
    /// This is the single seam where A* plugs in (M4): replace the buffer-fill below
    /// with a call into the managed pathfinder to produce a real multi-waypoint route.
    /// MovementSystem is agnostic to which one filled the buffer.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(MovementSystem))]
    partial struct PathRequestSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // MoveDestination is enableable; including it in the query matches only
            // units that currently have somewhere to go.
            foreach (var (dest, waypoints, cursor) in SystemAPI
                         .Query<RefRO<MoveDestination>, DynamicBuffer<PathWaypoint>, RefRW<PathCursor>>())
            {
                float3 target = dest.ValueRO.Value;
                bool alreadyPathed = waypoints.Length > 0 &&
                                     waypoints[waypoints.Length - 1].Position.Equals(target);
                if (alreadyPathed)
                {
                    continue;
                }

                waypoints.Clear();
                waypoints.Add(new PathWaypoint { Position = target }); // v1: single waypoint
                cursor.ValueRW.Index = 0;
            }
        }
    }
}

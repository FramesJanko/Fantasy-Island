using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FantasyIsland
{
    /// Server-only: turns a unit's MoveDestination into a path of waypoints by calling
    /// into the managed A* pathfinder (Grid / Pathfinding / PathRequestManager on a scene
    /// GameObject). MovementSystem is agnostic to how the buffer got filled.
    ///
    /// The A* pathfinder is async (coroutine + callback), so this is a managed SystemBase
    /// rather than a Burst ISystem: it holds the in-flight bookkeeping and receives the
    /// callbacks. On a new destination it fills a single *direct* waypoint immediately so
    /// the unit starts moving with zero latency, then replaces it with the real route when
    /// the callback lands (typically next frame). If no A* object is in the scene, or a
    /// path can't be found, it degrades to that same direct-steering behaviour.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(MovementSystem))]
    public partial class PathRequestSystem : SystemBase
    {
        // Re-request a path only once the destination has moved at least this far from
        // what we last pathed to. Keeps chasing a moving target from spamming A* every tick.
        const float k_RepathThreshold = 1f;

        // Draw each moving unit's remaining route with Debug.DrawLine (Scene view always,
        // Game view with Gizmos on). Green == a real A* route, yellow == direct fallback.
        const bool k_DrawPaths = false;

        // Destination we last requested a path toward, per unit (keyed by Entity, whose
        // version guards against destroyed-then-recycled entities matching stale entries).
        readonly Dictionary<Entity, float3> _lastRequested = new();

        // Units with an A* request currently in flight (one at a time each).
        readonly HashSet<Entity> _pending = new();

        // Whether each unit's current buffer is a real A* route (true) or the direct
        // fallback (false). Drives the path visualization colour.
        readonly Dictionary<Entity, bool> _isAstarRoute = new();

        // Completed paths, delivered from the A* callback, drained on the next update.
        readonly Queue<PathResult> _results = new();

        struct PathResult
        {
            public Entity Entity;
            public float3 Target;      // exact destination we pathed to (appended for a precise stop)
            public Vector3[] Waypoints;
            public bool Success;
        }

        protected override void OnUpdate()
        {
            ApplyCompletedPaths();

            foreach (var (dest, transform, waypoints, cursor, entity) in SystemAPI
                         .Query<RefRO<MoveDestination>, RefRO<LocalTransform>, DynamicBuffer<PathWaypoint>, RefRW<PathCursor>>()
                         .WithEntityAccess())
            {
                float3 target = dest.ValueRO.Value;

                // Already waiting on a path for this unit — let it finish.
                if (_pending.Contains(entity))
                {
                    continue;
                }

                // Already pathed to (roughly) here and still have a route — nothing to do.
                if (waypoints.Length > 0 &&
                    _lastRequested.TryGetValue(entity, out var last) &&
                    math.distancesq(last, target) < k_RepathThreshold * k_RepathThreshold)
                {
                    continue;
                }

                _lastRequested[entity] = target;
                float3 start = transform.ValueRO.Position;

                // Steer straight when A* isn't set up, or when nothing unwalkable lies between
                // the unit and its destination - no point routing around obstacles that aren't
                // there, and it avoids the async A* round-trip entirely.
                if (!PathRequestManager.IsReady ||
                    PathRequestManager.IsDirectPathClear((Vector3)start, (Vector3)target))
                {
                    waypoints.Clear();
                    waypoints.Add(new PathWaypoint { Position = target });
                    cursor.ValueRW.Index = 0;
                    _isAstarRoute[entity] = false;
                    continue;
                }

                // An obstacle is in the way - request an A* route. Only beeline when the unit
                // has no route at all (a fresh command): it moves straight at the target for the
                // one frame until the result lands, so there's zero input latency. If it's
                // already following a route (e.g. chasing and re-pathing), keep steering that
                // one until the new path arrives - otherwise it would briefly cut through a wall.
                if (waypoints.Length == 0)
                {
                    waypoints.Add(new PathWaypoint { Position = target });
                    cursor.ValueRW.Index = 0;
                    _isAstarRoute[entity] = false;
                }

                Entity requester = entity;
                _pending.Add(requester);
                PathRequestManager.RequestPath(
                    (Vector3)start,
                    (Vector3)target,
                    (path, success) => _results.Enqueue(new PathResult
                    {
                        Entity = requester,
                        Target = target,
                        Waypoints = path,
                        Success = success,
                    }));
            }

            if (k_DrawPaths)
            {
                DrawPaths();
            }
        }

        void ApplyCompletedPaths()
        {
            while (_results.Count > 0)
            {
                var result = _results.Dequeue();
                _pending.Remove(result.Entity);

                if (!EntityManager.Exists(result.Entity) ||
                    !EntityManager.HasBuffer<PathWaypoint>(result.Entity))
                {
                    _lastRequested.Remove(result.Entity);
                    _isAstarRoute.Remove(result.Entity);
                    continue;
                }

                // A* couldn't reach the target (even after snapping endpoints to walkable
                // nodes): leave the unit on whatever route it's already following rather than
                // overwriting it with a straight line that cuts through obstacles.
                if (!result.Success || result.Waypoints == null || result.Waypoints.Length == 0)
                {
                    _isAstarRoute[result.Entity] = false;
                    continue;
                }

                _isAstarRoute[result.Entity] = true;

                var buffer = EntityManager.GetBuffer<PathWaypoint>(result.Entity);
                buffer.Clear();
                foreach (var wp in result.Waypoints)
                {
                    buffer.Add(new PathWaypoint { Position = wp });
                }

                // Always finish on the exact clicked point: A* turn-points sit at node
                // centres, so without this the unit stops short of where it was told to go.
                buffer.Add(new PathWaypoint { Position = result.Target });

                EntityManager.SetComponentData(result.Entity, new PathCursor { Index = 0 });
            }
        }

        /// Draws each moving unit's remaining route: a line from the unit through every
        /// waypoint it has yet to reach, plus an X at each waypoint. Green == A* route,
        /// yellow == direct fallback (no A* object, or A* couldn't reach the target).
        void DrawPaths()
        {
            foreach (var (transform, waypoints, cursor, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, DynamicBuffer<PathWaypoint>, RefRO<PathCursor>>()
                         .WithAll<MoveDestination>() // enabled == currently moving
                         .WithEntityAccess())
            {
                if (waypoints.Length == 0)
                {
                    continue;
                }

                Color color = _isAstarRoute.TryGetValue(entity, out var astar) && astar
                    ? Color.green
                    : Color.yellow;

                float3 previous = transform.ValueRO.Position;
                for (int i = math.clamp(cursor.ValueRO.Index, 0, waypoints.Length - 1); i < waypoints.Length; i++)
                {
                    float3 point = waypoints[i].Position;
                    Debug.DrawLine(previous, point, color);
                    DrawMarker(point, color);
                    previous = point;
                }
            }
        }

        static void DrawMarker(float3 p, Color color)
        {
            const float s = 0.3f;
            Debug.DrawLine(p + new float3(-s, 0f, -s), p + new float3(s, 0f, s), color);
            Debug.DrawLine(p + new float3(-s, 0f, s), p + new float3(s, 0f, -s), color);
        }
    }
}

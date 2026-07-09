using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace FantasyIsland
{
    // -----------------------------------------------------------------------
    // Tags
    // -----------------------------------------------------------------------

    /// Marks any controllable/combat unit (players and NPCs alike).
    public struct UnitTag : IComponentData { }

    /// Marks a player-controlled unit (has a GhostOwner, driven by move commands).
    public struct PlayerTag : IComponentData { }

    /// Marks a server-driven NPC unit.
    public struct NpcTag : IComponentData { }

    // -----------------------------------------------------------------------
    // Shared unit state
    // -----------------------------------------------------------------------

    /// Replicated health. Current is a ghost field so clients see damage/heals.
    public struct Health : IComponentData
    {
        public float Max;
        [GhostField] public float Current;
    }

    /// Movement speed in units/second (server-authoritative movement).
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    /// The unit's current move destination. Enableable: enabled == "has somewhere to go".
    /// Disabled means the unit is idle (arrived / never commanded).
    public struct MoveDestination : IComponentData, IEnableableComponent
    {
        public float3 Value;
    }

    /// The unit's current combat target (Entity.Null == none). Populated by AI/commands.
    public struct TargetRef : IComponentData
    {
        [GhostField] public Entity Value;
    }

    /// Display name baked from the prefab's GameObject name. Same for every instance of
    /// a given unit type, so it's static tuning data (like CombatStats) - no GhostField needed.
    public struct UnitName : IComponentData
    {
        public FixedString64Bytes Value;
    }

    /// Cached distance to the current target, refreshed each server tick.
    public struct DistanceToTarget : IComponentData
    {
        public float Value;
    }

    /// Static combat tuning baked from authoring.
    public struct CombatStats : IComponentData
    {
        public float BaseRange;
        public float AttackRange;
        public float Damage;
        public float AttackTime;
    }

    /// Live attack progress (server-driven). Progress counts up to AttackTime.
    public struct AttackState : IComponentData
    {
        public float Progress;
        public bool Attacking;
    }

    /// NPC behaviour tuning + leash anchor. Home is set at spawn time.
    public struct NpcAi : IComponentData
    {
        public float3 Home;
        public float HuntingDistance;
        public float LeashRange;
    }

    // -----------------------------------------------------------------------
    // Pathing (v1: single waypoint == direct steering; A* fills this later)
    // -----------------------------------------------------------------------

    /// Ordered path the unit steers through. In v1 it holds a single point (the
    /// destination); the A* integration (M4) replaces PathRequestSystem's body to
    /// fill this with a real route without touching the movement system.
    [InternalBufferCapacity(16)]
    public struct PathWaypoint : IBufferElementData
    {
        public float3 Position;
    }

    /// Index of the waypoint the unit is currently steering toward.
    public struct PathCursor : IComponentData
    {
        public int Index;
    }

    // -----------------------------------------------------------------------
    // Networking: spawn config + commands
    // -----------------------------------------------------------------------

    /// Singleton (baked on the spawner object) holding the player ghost prefab.
    public struct PlayerSpawner : IComponentData
    {
        public Entity Prefab;
    }

    /// Singleton (baked on the spawner object) holding the NPC ghost prefab.
    public struct NpcSpawner : IComponentData
    {
        public Entity Prefab;
    }

    /// Where NPCs should spawn (baked from the spawner's spawn-point transforms).
    public struct NpcSpawnPoint : IBufferElementData
    {
        public float3 Position;
    }

    /// Marker added to a connection once its player has been spawned (spawn-once guard).
    public struct PlayerSpawned : IComponentData { }

    /// Client -> server "move my unit here" command. Discrete event (interpolated
    /// ghosts, no prediction), so an RPC is the right tool rather than streamed input.
    public struct MoveCommandRpc : IRpcCommand
    {
        public float3 Destination;
    }
}

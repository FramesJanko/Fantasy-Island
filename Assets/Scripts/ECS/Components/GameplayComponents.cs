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

    public struct Health : IComponentData
    {
        [GhostField]public float BaseHp;
        [GhostField]public float Max;
        [GhostField]public float Current;
        [GhostField]public bool Dead;
    }
    public struct LeveledUp : IComponentData, IEnableableComponent
    {
        public int LevelsGained;
    }
    public struct UnitLevel : IComponentData
    {
        [GhostField]public int Value;
        [GhostField]public int NeededForLevel;
    }
    [GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct Experience : IComponentData
    {
        [GhostField]public int TotalExperience;
        /// How much experience this unit grants when it dies, split evenly among nearby
        /// Experience holders by UnitDeathSystem.
        public int Award;
    }

    public struct MoveSpeed : IComponentData
    {
        public float BaseMovespeed;
        [GhostField]public float Movespeed;
    }

    /// The unit's current move destination. Enableable: enabled == "has somewhere to go".
    /// Disabled means the unit is idle (arrived / never commanded).
    public struct MoveDestination : IComponentData, IEnableableComponent
    {
        public float3 Value;
    }

    /// The unit's current combat target (Entity.Null == none). Populated by AI/commands.
    public struct TargetRef : IComponentData, IEnableableComponent
    {
        [GhostField] public Entity Value;
    }

    /// Display name baked from the prefab's GameObject name. Same for every instance of
    /// a given unit type, so it's static tuning data (like CombatStats) - no GhostField needed.
    public struct UnitName : IComponentData
    {
        public FixedString64Bytes Value;
    }

    [InternalBufferCapacity(8)]
    public struct DistanceToPlayers : IBufferElementData
    {
        public Entity Player;       
        public float Value;
    }

    /// Cached distance to the current target, refreshed each server tick.
    public struct DistanceToTarget : IComponentData
    {
        public float Value;
    }

    /// Static combat tuning baked from authoring.
    public struct CombatStats : IComponentData
    {
        [GhostField] public float BaseRange;
        [GhostField] public float AttackRange;
        [GhostField] public float Damage;
        [GhostField] public float BaseDamage;
        [GhostField] public float AttackTime;
        [GhostField] public float BaseAttackTime;
        [GhostField] public float Strength;
        [GhostField] public float Agility;
        [GhostField] public float Intelligence;
    }
    [GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct AbilityPoints : IComponentData, IEnableableComponent
    {
        [GhostField]public int Value;
    }
    [GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
    public struct SkillPoints : IComponentData, IEnableableComponent
    {
        [GhostField]public int Value;
    }

    /// Live attack progress (server-driven). Progress counts up to AttackTime.
    public struct AttackState : IComponentData
    {
        [GhostField]public float Progress;
        public bool AttackSucceeded;
    }

    /// NPC behaviour tuning + leash anchor. Home is set at spawn time.
    public struct NpcAi : IComponentData
    {
        public float3 Home;
        public float HuntingDistance;
        public float AggroRange;
        public float LeashRange;
    }
    public struct SpawnStatus : IComponentData
    {
        public float BaseSpawnTimer;
        [GhostField]public float SpawnTimer;
        public int DeathCount;
    }

    public struct UnitMode : IComponentData
    {
        public enum Mode
        {
            Idle,
            Moving,
            Attacking
        }

        [GhostField]public Mode CurrentMode;
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

    /// Client -> server "target this unit" command. Carries the clicked ghost's networked
    /// id (resolved client-side from GhostInstance); the server maps it back to a server
    /// entity and points the sender's owned unit at it. Ghost ids are stable across the
    /// wire, unlike Entity, which is only valid within the world it came from.
    public struct SetTargetCommandRpc : IRpcCommand
    {
        public int TargetGhostId;
    }
    public struct SpendAbilityPointCommandRpc : IRpcCommand
    {
        public int Ability;
    }
    public struct SpendSkillPointCommandRpc : IRpcCommand
    {
        public int Skill;
    }
}

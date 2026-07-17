using System;
using Unity.Entities;
using UnityEngine;

namespace FantasyIsland
{
    /// Runtime-only marker placed on a pooled pick-proxy collider. Holds the client-world
    /// entity the proxy currently stands in for, plus that ghost's networked id (used to
    /// tell the server which unit was clicked). Set fresh on every pick - never serialized.
    [RequireComponent(typeof(BoxCollider))]
    public class UnitPickProxy : MonoBehaviour
    {
        [NonSerialized] public Entity Entity;
        [NonSerialized] public int GhostId;
    }
}

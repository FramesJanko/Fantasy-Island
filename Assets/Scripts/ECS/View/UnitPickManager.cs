using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace FantasyIsland
{
    /// Client-only picking service. On demand it projects the client world's unit entities
    /// into a pool of collider-only proxy GameObjects, raycasts the supplied ray against
    /// them, and reports which entity was hit. The proxies exist only for the duration of a
    /// single pick - they are positioned, queried, then deactivated - so there is no
    /// per-frame collider upkeep. The cost is paid only when something actually clicks.
    ///
    /// Setup:
    ///   1. Add a dedicated physics layer (default name "UnitPick") under
    ///      Project Settings > Tags and Layers.
    ///   2. Put this component on a plain client-side manager GameObject and set the layer
    ///      name + collider size/center to roughly match your unit.
    /// Nothing references this directly; PlayerInputSystem finds it via the static Instance.
    public class UnitPickManager : MonoBehaviour
    {
        public static UnitPickManager Instance { get; private set; }

        [Tooltip("Dedicated physics layer the proxy colliders live on. Create it under " +
                 "Project Settings > Tags and Layers so the ground raycast can exclude it.")]
        [SerializeField] string _pickLayerName = "UnitPick";

        [Tooltip("Size of each proxy's box collider (roughly the unit's bounds).")]
        [SerializeField] Vector3 _proxyColliderSize = new Vector3(1f, 2f, 1f);

        [Tooltip("Local center of each proxy's box collider, relative to the unit's transform origin.")]
        [SerializeField] Vector3 _proxyColliderCenter = new Vector3(0f, 1f, 0f);

        [Tooltip("Maximum pick ray distance.")]
        [SerializeField] float _maxPickDistance = 1000f;

        [Tooltip("Proxy colliders to allocate up front so the first click doesn't hitch. " +
                 "Set roughly to your expected peak on-screen unit count; the pool still " +
                 "grows on demand if a pick needs more.")]
        [SerializeField] int _prewarmCount = 32;

        int _pickLayer = -1;

        World _clientWorld;
        EntityQuery _unitQuery;
        bool _queryBuilt;

        readonly List<UnitPickProxy> _pool = new();

        /// Layer mask matching just the proxy layer - use for the pick raycast, and its
        /// inverse for the ground raycast so the two never hit each other.
        public int PickLayerMask => _pickLayer >= 0 ? 1 << _pickLayer : 0;

        /// True once a valid pick layer was resolved at Awake.
        public bool HasPickLayer => _pickLayer >= 0;

        void Awake()
        {
            Instance = this;
            _pickLayer = LayerMask.NameToLayer(_pickLayerName);
            if (_pickLayer < 0)
            {
                Debug.LogError($"[UnitPickManager] Layer \"{_pickLayerName}\" does not exist. " +
                               "Add it under Project Settings > Tags and Layers - unit picking is disabled until then.");
            }
        }

        void Start()
        {
            // Allocate the proxy pool up front so the first pick doesn't pay for a burst of
            // GameObject instantiation. Layer is resolved in Awake, which runs before Start.
            if (_pickLayer >= 0 && _prewarmCount > 0) EnsurePool(_prewarmCount);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            foreach (var proxy in _pool)
            {
                if (proxy != null) Destroy(proxy.gameObject);
            }
            _pool.Clear();
            if (_queryBuilt && _clientWorld is { IsCreated: true }) _unitQuery.Dispose();
        }

        /// Projects the current unit entities onto proxy colliders and raycasts them.
        /// Returns true (with the hit entity + ghost id) if the ray struck a unit. Intended
        /// to be called on demand - e.g. the frame a click happens - not every frame.
        public bool TryPick(Ray ray, out Entity entity, out int ghostId)
        {
            entity = Entity.Null;
            ghostId = 0;

            if (_pickLayer < 0 || !EnsureWorld()) return false;

            var entities = _unitQuery.ToEntityArray(Allocator.Temp);
            var transforms = _unitQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var ghosts = _unitQuery.ToComponentDataArray<GhostInstance>(Allocator.Temp);

            // Stand up one proxy per unit at its current position.
            EnsurePool(entities.Length);
            for (int i = 0; i < entities.Length; i++)
            {
                var proxy = _pool[i];
                proxy.Entity = entities[i];
                proxy.GhostId = ghosts[i].ghostId;
                proxy.transform.position = transforms[i].Position;
                proxy.gameObject.SetActive(true);
            }
            for (int i = entities.Length; i < _pool.Count; i++)
            {
                if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
            }

            // Push the just-written transforms into PhysX before we query it - the proxies
            // were moved this frame, so an implicit sync isn't guaranteed to have happened.
            Physics.SyncTransforms();

            bool hit = false;
            if (Physics.Raycast(ray, out var rayHit, _maxPickDistance, PickLayerMask) &&
                rayHit.collider.TryGetComponent<UnitPickProxy>(out var picked))
            {
                entity = picked.Entity;
                ghostId = picked.GhostId;
                hit = true;
            }

            // Tear the proxies back down - picking is on-demand, nothing should linger.
            for (int i = 0; i < entities.Length; i++)
            {
                _pool[i].gameObject.SetActive(false);
            }

            entities.Dispose();
            transforms.Dispose();
            ghosts.Dispose();
            return hit;
        }

        void EnsurePool(int count)
        {
            while (_pool.Count < count)
            {
                // Unparented (scene root) so a non-identity parent scale can't distort the
                // box collider - its effective size is size * lossyScale.
                var go = new GameObject("UnitPickProxy") { layer = _pickLayer };
                var box = go.AddComponent<BoxCollider>();
                box.size = _proxyColliderSize;
                box.center = _proxyColliderCenter;
                var body = go.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                var proxy = go.AddComponent<UnitPickProxy>();
                go.SetActive(false);
                _pool.Add(proxy);
            }
        }

        bool EnsureWorld()
        {
            if (_clientWorld is { IsCreated: true }) return true;

            _clientWorld = null;
            foreach (var world in World.All)
            {
                if (world.IsClient() && !world.IsThinClient())
                {
                    _clientWorld = world;
                    break;
                }
            }
            if (_clientWorld == null) return false;

            _unitQuery = _clientWorld.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<UnitTag>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<GhostInstance>());
            _queryBuilt = true;
            return true;
        }
    }
}

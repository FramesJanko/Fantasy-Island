using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace FantasyIsland
{
    /// Drives every unit's nameplate from a single Screen Space - Overlay Canvas.
    ///
    /// This is a plain MonoBehaviour, NOT an ECS system: each frame it polls the
    /// client world's unit entities (read-only) and pools UI elements to match.
    /// Nameplates are never entities and have no ECS lifecycle coupling - when a
    /// unit entity disappears from the query, its pooled element is recycled.
    ///
    /// Put this on the UI Canvas and assign a NameplateElement prefab.
    public class NameplateManager : MonoBehaviour
    {
        [Tooltip("UI prefab with a NameplateElement component (RectTransform + two TMP_Text).")]
        public NameplateElement NameplatePrefab;

        [Tooltip("Camera used to project world positions to screen. Defaults to Camera.main.")]
        public Camera WorldCamera;

        [Tooltip("World-space offset from the unit's transform where the nameplate anchors (use Y to sit above the head).")]
        public Vector3 WorldOffset = new Vector3(0f, 2f, 0f);

        [Tooltip("Screen-space pixel offset applied after projection. X = right, Y = up. Camera-angle independent.")]
        public Vector2 ScreenOffset = new Vector2(0f, 0f);

        World _clientWorld;
        EntityQuery _unitQuery;
        bool _queryBuilt;

        readonly Dictionary<Entity, NameplateElement> _active = new();
        readonly List<Entity> _stale = new();
        readonly Stack<NameplateElement> _pool = new();

        void Awake()
        {
            if (WorldCamera == null) WorldCamera = Camera.main;
        }

        void LateUpdate()
        {
            if (WorldCamera == null)
            {
                WorldCamera = Camera.main;
                if (WorldCamera == null) return;
            }

            if (!EnsureWorld()) return;

            var em = _clientWorld.EntityManager;

            var entities = _unitQuery.ToEntityArray(Allocator.Temp);
            var transforms = _unitQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var names = _unitQuery.ToComponentDataArray<UnitName>(Allocator.Temp);
            var targets = _unitQuery.ToComponentDataArray<TargetRef>(Allocator.Temp);

            // Anything currently shown is stale until this frame's query re-touches it.
            _stale.Clear();
            foreach (var key in _active.Keys) _stale.Add(key);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                _stale.Remove(entity);

                if (!_active.TryGetValue(entity, out var element))
                {
                    element = Rent();
                    _active[entity] = element;
                }

                float3 world = transforms[i].Position + (float3)WorldOffset;
                Vector3 screen = WorldCamera.WorldToScreenPoint(world);

                // z <= 0 means the point is behind the camera - hide rather than flip.
                if (screen.z <= 0f)
                {
                    element.SetVisible(false);
                    continue;
                }

                var targetEntity = targets[i].Value;
                bool hasTarget = em.Exists(targetEntity) && em.HasComponent<UnitName>(targetEntity);
                bool hasHealth = em.Exists(entity) && em.HasComponent<Health>(entity);
                bool hasAttackState = em.Exists(entity) && em.HasComponent<AttackState>(entity);
                bool hasCombat = em.Exists(entity) && em.HasComponent<CombatStats>(entity);
                var health = em.GetComponentData<Health>(entity);

                screen.x += ScreenOffset.x;
                screen.y += ScreenOffset.y;

                
                element.SetScreenPosition(screen);
                element.SetName(names[i].Value.ToString());

                element.SetVisible(!health.Dead);
                if (hasHealth) element.SetHealth(health.Current/health.Max);
                if (hasAttackState && hasCombat) element.SetAttackProgress(em.GetComponentData<AttackState>(entity).Progress/em.GetComponentData<CombatStats>(entity).AttackTime);
                element.SetTarget(hasTarget
                    ? em.GetComponentData<UnitName>(targetEntity).Value.ToString()
                    : "-");
            }

            // Recycle elements whose entity vanished this frame.
            foreach (var gone in _stale)
            {
                if (_active.TryGetValue(gone, out var element))
                {
                    Recycle(element);
                    _active.Remove(gone);
                }
            }

            entities.Dispose();
            transforms.Dispose();
            names.Dispose();
            targets.Dispose();
        }

        void OnDestroy()
        {
            if (_queryBuilt && _clientWorld is { IsCreated: true })
                _unitQuery.Dispose();
        }

        /// Locates the Netcode client world and builds the unit query once.
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
                ComponentType.ReadOnly<UnitName>(),
                ComponentType.ReadOnly<TargetRef>());
            _queryBuilt = true;
            return true;
        }

        NameplateElement Rent()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                pooled.SetVisible(true);
                return pooled;
            }
            return Instantiate(NameplatePrefab, transform);
        }

        void Recycle(NameplateElement element)
        {
            element.SetVisible(false);
            _pool.Push(element);
        }
    }
}

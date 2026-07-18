using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using TMPro;

namespace FantasyIsland
{
    public class InfoPlateData : MonoBehaviour
    {
        World _clientWorld;
        EntityQuery _unitQuery;
        EntityQuery _ownerQuery;
        bool _queryBuilt;
        public TMP_Text unitName;
        public TMP_Text strengthText;
        public TMP_Text agilityText;
        public TMP_Text intelligenceText;
        public TMP_Text hpText;
        public TMP_Text attackspeedText;
        public TMP_Text attackdamageText;
        public TMP_Text movespeedText;
        public TMP_Text spawntimerText;
        public bool working;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            working = false;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if(!EnsureWorld()) return;

            working = true;
            var em = _clientWorld.EntityManager;
            // var ownerGhost = Entity.Null;
            if(_ownerQuery.CalculateEntityCount() != 1) return;
            // ownerGhost = _ownerQuery.ToEntityArray(Allocator.Temp)[0];
            var combat = _ownerQuery.ToComponentDataArray<CombatStats>(Allocator.Temp);
            var movespeed = _ownerQuery.ToComponentDataArray<MoveSpeed>(Allocator.Temp);
            var spawnStats= _ownerQuery.ToComponentDataArray<SpawnStatus>(Allocator.Temp);
            var health = _ownerQuery.ToComponentDataArray<Health>(Allocator.Temp);
            var ownerName = _ownerQuery.ToComponentDataArray<UnitName>(Allocator.Temp);
            unitName.text = ownerName[0].Value.ToString();
            strengthText.text = "Str: " + combat[0].Strength.ToString();
            agilityText.text = "Agi: " + combat[0].Agility.ToString();
            intelligenceText.text = "Int: " + combat[0].Intelligence.ToString();
            attackspeedText.text = "Atk Speed: " + combat[0].AttackTime.ToString();
            attackdamageText.text = "Atk Damage: " + combat[0].Damage.ToString();
            hpText.text = "HP: " + health[0].Current.ToString() + " / " + health[0].Max.ToString();
            movespeedText.text = "Movespeed: " + movespeed[0].Movespeed.ToString();
            spawntimerText.text = spawnStats[0].SpawnTimer < 0f ? "" : "Respawn in: " + spawnStats[0].SpawnTimer.ToString("0.00");


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

            // _unitQuery = _clientWorld.EntityManager.CreateEntityQuery(
            //     ComponentType.ReadOnly<UnitTag>(),
            //     ComponentType.ReadOnly<UnitName>(),
            //     ComponentType.ReadOnly<CombatStats>(),
            //     ComponentType.ReadOnly<MoveSpeed>(),
            //     ComponentType.ReadOnly<SpawnStatus>(),
            //     ComponentType.ReadOnly<Health>());

            _ownerQuery = _clientWorld.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<GhostOwnerIsLocal>(),
                ComponentType.ReadOnly<UnitTag>(),
                ComponentType.ReadOnly<UnitName>(),
                ComponentType.ReadOnly<CombatStats>(),
                ComponentType.ReadOnly<MoveSpeed>(),
                ComponentType.ReadOnly<SpawnStatus>(),
                ComponentType.ReadOnly<Health>());
            _queryBuilt = true;
            return true;
        }
    }
}
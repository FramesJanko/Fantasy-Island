using System;
using FantasyIsland;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FantasyIsland
{

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct PlayerRespawnSystem : ISystem
    {   
        void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            foreach(var (spawn, health, transform, entity) in SystemAPI
                    .Query<RefRW<SpawnStatus>, RefRW<Health>, RefRW<LocalTransform>>()
                    .WithAll<PlayerTag>()
                    .WithEntityAccess())
            {
                spawn.ValueRW.SpawnTimer -= dt;
                if (health.ValueRO.Dead)
                {
                    if(spawn.ValueRO.SpawnTimer <= 0)
                    {
                        health.ValueRW.Dead = false;
                        transform.ValueRW.Position = new float3(0f, 0f, 0f);
                    }
                }
            }
        }
    }
}

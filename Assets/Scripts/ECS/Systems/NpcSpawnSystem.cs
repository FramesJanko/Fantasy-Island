using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace FantasyIsland
{
    /// Server-only: spawns one NPC ghost at each baked spawn point, once, at startup.
    /// Hunting/leash tuning comes from the prefab's NpcAuthoring; only Home is set here
    /// (to the spawn position) so the NPC knows where to leash back to.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    partial struct NpcSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NpcSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Spawn a single wave, then stop running.
            state.Enabled = false;

            var prefab = SystemAPI.GetSingleton<NpcSpawner>().Prefab;
            var baseAi = SystemAPI.GetComponent<NpcAi>(prefab);
            var points = SystemAPI.GetSingletonBuffer<NpcSpawnPoint>(true);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < points.Length; i++)
            {
                var pos = points[i].Position;
                var npc = ecb.Instantiate(prefab);
                ecb.SetComponent(npc, LocalTransform.FromPosition(pos));

                var ai = baseAi;
                ai.Home = pos;
                ecb.SetComponent(npc, ai);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

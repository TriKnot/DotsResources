using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Biods.Spawner
{
    [BurstCompile]
    public partial struct BoidSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidSpawnerConfig>();   
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            
            EntityManager entityManager = state.EntityManager;
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            
            BoidSpawnerConfig boidSpawnerConfig = SystemAPI.GetSingleton<BoidSpawnerConfig>();
            
            for (int i = 0; i < boidSpawnerConfig.Count; i++)
            {
                Entity entity = commandBuffer.Instantiate(boidSpawnerConfig.Prefab);
                commandBuffer.SetComponent(entity, new LocalTransform
                {
                    Position = new float3(
                        UnityEngine.Random.Range(-boidSpawnerConfig.Bounds.x, boidSpawnerConfig.Bounds.x),
                        UnityEngine.Random.Range(-boidSpawnerConfig.Bounds.y, boidSpawnerConfig.Bounds.y),
                        UnityEngine.Random.Range(-boidSpawnerConfig.Bounds.z, boidSpawnerConfig.Bounds.z)
                        ),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
            
            commandBuffer.Playback(entityManager);
        }
    }
}
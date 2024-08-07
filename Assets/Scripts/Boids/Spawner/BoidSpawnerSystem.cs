﻿using Biods.Movement;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Biods.Spawner
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BoidSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidMovementConfig>();
            state.RequireForUpdate<BoidSpawnerConfig>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false; // Disable the system after the first update
            
            EntityManager entityManager = state.EntityManager;
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            
            BoidSpawnerConfig boidSpawnerConfig = SystemAPI.GetSingleton<BoidSpawnerConfig>();
            BoidMovementConfig boidMovementConfig = SystemAPI.GetSingleton<BoidMovementConfig>();
            
            for (int i = 0; i < boidSpawnerConfig.Count; i++)
            {
                Entity entity = commandBuffer.Instantiate(boidSpawnerConfig.Prefab);
                commandBuffer.SetComponent(entity, new LocalTransform
                {
                    Position = new float3(
                        UnityEngine.Random.Range(-boidMovementConfig.Bounds.x, boidMovementConfig.Bounds.x),
                        UnityEngine.Random.Range(-boidMovementConfig.Bounds.y, boidMovementConfig.Bounds.y),
                        UnityEngine.Random.Range(-boidMovementConfig.Bounds.z, boidMovementConfig.Bounds.z)
                        ) * 0.75f + boidMovementConfig.BoundsCenter,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
            
            commandBuffer.Playback(entityManager);
        }
    }
}
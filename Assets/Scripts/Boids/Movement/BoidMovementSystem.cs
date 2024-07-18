using Biods.Spawner;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Biods.Movement
{
    [BurstCompile]
    public partial struct BoidMovementSystem : ISystem
    {
        private EntityQuery _boidQuery;
        private JobHandle _jobHandle;
    
        private NativeArray<float3> _positions;
        private NativeArray<float3> _velocities;
        private NativeArray<float3> _newVelocities;
        private NativeArray<LocalTransform> _transforms;
        private NativeArray<Boid> _boids;
        private NativeArray<Entity> _entities;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidMovementConfig>();
            _boidQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadWrite<Boid>());
        }
        
       [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int count = _boidQuery.CalculateEntityCount();
            
            if(count == 0)
                return;
            
            InitializeArrays(ref state, count);
            ScheduleBoidSimulationJob(ref state, count);            
            UpdateComponents(ref state, count);
            DisposeArrays();
        }

        private void DisposeArrays()
        {
            _positions.Dispose();
            _velocities.Dispose();
            _newVelocities.Dispose();
            _transforms.Dispose();
            _entities.Dispose();
            _boids.Dispose();
        }

        private void ScheduleBoidSimulationJob(ref SystemState state, int count)
        {
            BoidMovementConfig config = SystemAPI.GetSingleton<BoidMovementConfig>();

            BoidSimulationJob job = new BoidSimulationJob
            {
                Positions = _positions,
                Velocities = _velocities,
                NewVelocities = _newVelocities,
                Config = config,
            };

            _jobHandle = job.ScheduleParallel(count, 128, new JobHandle());
            _jobHandle.Complete();
        }
        
        private void UpdateComponents(ref SystemState state, int count)
        {
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            for (int i = 0; i < count; i++)
            {
                commandBuffer.SetComponent(_entities[i], new LocalTransform
                {
                    Position = _transforms[i].Position + _newVelocities[i] * SystemAPI.Time.DeltaTime,
                    Rotation = _transforms[i].Rotation,
                    Scale = _transforms[i].Scale
                });
                commandBuffer.SetComponent(_entities[i], new Boid { Velocity = _newVelocities[i] });
            }
            
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }

        private void InitializeArrays(ref SystemState state, int count)
        {

            _positions = new NativeArray<float3>(count, Allocator.TempJob);
            _velocities = new NativeArray<float3>(count, Allocator.TempJob);
            _newVelocities = new NativeArray<float3>(count, Allocator.TempJob);
            
            _boids = _boidQuery.ToComponentDataArray<Boid>(Allocator.TempJob);
            _entities = _boidQuery.ToEntityArray(Allocator.TempJob);
            _transforms = _boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
    
            for (int i = 0; i < count; i++)
            {
                _positions[i] = _transforms[i].Position;
                _velocities[i] = _boids[i].Velocity;
            }

        }
        
    }
}
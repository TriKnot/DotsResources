using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

namespace Biods.Movement
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BoidMovementSystem : ISystem
    {
        private EntityQuery _boidQuery;
        private JobHandle _jobHandle;
        private BoidMovementConfig _config;
    
        private NativeArray<float3> _positions;
        private NativeArray<LocalTransform> _transforms;
        private NativeArray<Boid> _boids;
        private NativeArray<Entity> _entities;
        
        private NativeList<BoidAspect> _boidsAspects;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidMovementConfig>();
        }
        
       [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
           
            _boidsAspects = new NativeList<BoidAspect>(Allocator.Persistent);
            
            _config = SystemAPI.GetSingleton<BoidMovementConfig>();
            
            ScheduleBoidSimulationJob(ref state);        
            
            _jobHandle.Complete();
            _boidsAspects.Dispose();
        }
        
        [BurstCompile]
        private void ScheduleBoidSimulationJob(ref SystemState state)
        {
            _boidsAspects.Clear();

            foreach (BoidAspect aspect in SystemAPI.Query<BoidAspect>())
            {
                _boidsAspects.Add(aspect);
            }
            
            BoidSimulationJob job = new BoidSimulationJob
            {
                RandomSeed = (uint) Random.Range(0, 100000),
                DeltaTime = SystemAPI.Time.DeltaTime,
                Config = _config,
                BoidsAspects = _boidsAspects.AsArray()
            };

            _jobHandle = job.ScheduleParallel(_boidsAspects.Length, 64, new JobHandle());
        }
        
        // [BurstCompile]
        // private void DisposeArrays()
        // {
        //     _positions.Dispose();
        //     _velocities.Dispose();
        //     _newVelocities.Dispose();
        //     _transforms.Dispose();
        //     _entities.Dispose();
        //     _boids.Dispose();
        // }
        
        // [BurstCompile]
        // private void UpdateComponents(ref SystemState state, int count)
        // {
        //     EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        //     
        //     for (int i = 0; i < count; i++)
        //     {
        //         commandBuffer.SetComponent(_entities[i], new LocalTransform
        //         {
        //             Position = _transforms[i].Position + _newVelocities[i] * SystemAPI.Time.DeltaTime,
        //             Rotation = _transforms[i].Rotation,
        //             Scale = _transforms[i].Scale
        //         });
        //         commandBuffer.SetComponent(_entities[i], new Boid { Velocity = _newVelocities[i] });
        //     }
        //     
        //     commandBuffer.Playback(state.EntityManager);
        //     commandBuffer.Dispose();
        // }

        // [BurstCompile]
        // private void InitializeArrays(ref SystemState state, int count)
        // {
        //
        //     _positions = new NativeArray<float3>(count, Allocator.TempJob);
        //     _newVelocities = new NativeArray<MoveComponent>(count, Allocator.TempJob);
        //     
        //     _boids = _boidQuery.ToComponentDataArray<Boid>(Allocator.TempJob);
        //     _velocities = _boidQuery.ToComponentDataArray<MoveComponent>(Allocator.TempJob);
        //     _entities = _boidQuery.ToEntityArray(Allocator.TempJob);
        //     _transforms = _boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        //
        //     for (int i = 0; i < count; i++)
        //     {
        //         _positions[i] = _transforms[i].Position;
        //     }
        //
        // }
        
    }
}
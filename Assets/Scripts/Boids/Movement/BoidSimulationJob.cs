using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Biods.Movement
{
    [BurstCompile]
    public struct BoidSimulationJob : IJobFor
    {
        [ReadOnly] public uint RandomSeed;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public BoidMovementConfig Config;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<BoidAspect> BoidsAspects;
        
        [BurstCompile]
        public void Execute(int index)
        {
            NativeList<int> neighborIndexes = BoidsAspects[index].FindNeighborIndexes(index, BoidsAspects, Config.MaxNeighborDistance);
            NativeArray<BoidAspect> neighborAspects = new NativeArray<BoidAspect>(neighborIndexes.Length, Allocator.Temp);

            for (int i = 0; i < neighborIndexes.Length; i++)
            {
                neighborAspects[i] = BoidsAspects[neighborIndexes[i]];
            }
            
            Random random = new Random((uint) (RandomSeed * (index + 1)));
            float3 randomDirection = random.NextFloat3(new float3(-1,-1,-1), new float3(1,1,1));

            BoidsAspects[index].Simulate(DeltaTime, randomDirection, neighborAspects, Config);
            
            // NativeList<int> neighborIndexes = aspect.FindNeighbors(index, BoidsAspects, Config.MaxNeighborDistance);
            //
            // float3 separation = aspect.CalculateSeparation(position, Config, BoidsAspects, neighborIndexes);
            // float3 alignment = aspect.CalculateAlignment(position, Config, BoidsAspects, neighborIndexes);
            // float3 cohesion = aspect.CalculateCohesion(position, Config, BoidsAspects, neighborIndexes);
            //
            // Random random = new Random((uint) (RandomSeed * (index + 1)));
            // float3 randomDirection = random.NextFloat3(new float3(-1,-1,-1), new float3(1,1,1)) * Config.RandomScatterWeight;
            //
            // float3 boundsSteer = aspect.SteerInBounds(position, velocity, Config);
            //
            // float3 newVelocity = velocity + separation + alignment + cohesion + randomDirection + boundsSteer;
            // newVelocity = aspect.ClampVelocity(newVelocity, Config);
            //
            // aspect.Boid.ValueRW.Velocity = newVelocity;
            // aspect.Transform.ValueRW.Position += newVelocity * DeltaTime;
        }

        // private NativeList<int> FindNeighbors(int index, float3 position, float maxNeighborDistance)
        // {
        //     NativeList<int> neighbors = new NativeList<int>(Allocator.Temp);
        //
        //     for (int i = 0; i < Positions.Length; i++)
        //     {
        //         if (i == index) continue;
        //
        //         float3 otherPosition = Positions[i];
        //         float distance = math.distance(position, otherPosition);
        //         if (distance < maxNeighborDistance)
        //         {
        //             neighbors.Add(i);
        //         }
        //     }
        //
        //     return neighbors;
        // }

        // private float3 CalculateSeparation(float3 position, NativeList<int> neighbors)
        // {
        //     float3 separation = float3.zero;
        //     int neighborCount = 0;
        //
        //     for (int i = 0; i < neighbors.Length; i++)
        //     {
        //         int index = neighbors[i];
        //         float3 otherPosition = Positions[index];
        //         float distance = math.distance(position, otherPosition);
        //         if (distance < Config.SeparationRange)
        //         {
        //             float3 diff = position - otherPosition;
        //             float3 direction = math.normalize(diff);
        //             float3 weightedDiff = direction / distance;
        //             
        //             separation += weightedDiff;
        //             neighborCount++;
        //         }
        //     }
        //
        //     if (neighborCount > 0)
        //     {
        //         separation /= neighborCount;
        //         separation *= Config.SeparationWeight;
        //     }
        //
        //     return separation;
        // }

        // private float3 CalculateAlignment(float3 position, NativeList<int> neighbors)
        // {
        //     float3 alignment = float3.zero;
        //     int neighborCount = 0;
        //
        //     for (int i = 0; i < neighbors.Length; i++)
        //     {
        //         int index = neighbors[i];
        //         float3 otherPosition = Positions[index];
        //         float distance = math.distance(position, otherPosition);
        //         if (distance < Config.AlignmentRange)
        //         {
        //             float3 otherVelocity = Velocities[index].Velocity;
        //             
        //             alignment += otherVelocity;
        //             neighborCount++;
        //         }
        //     }
        //
        //     if (neighborCount > 0)
        //     {
        //         alignment /= neighborCount;
        //         alignment *= Config.AlignmentWeight;
        //     }
        //
        //     return alignment;
        // }

        // private float3 CalculateCohesion(float3 position, NativeList<int> neighbors)
        // {
        //     float3 cohesion = float3.zero;
        //     int neighborCount = 0;
        //
        //     for (int i = 0; i < neighbors.Length; i++)
        //     {
        //         int index = neighbors[i];
        //         float3 otherPosition = Positions[index];
        //         float distance = math.distance(position, otherPosition);
        //         if (distance < Config.CohesionRange)
        //         {
        //             cohesion += otherPosition;
        //             neighborCount++;
        //         }
        //     }
        //
        //     if (neighborCount > 0)
        //     {
        //         cohesion /= neighborCount;
        //         float3 cohesionDirection = cohesion - position;
        //         cohesion = math.normalize(cohesionDirection);
        //         
        //         cohesion *= Config.CohesionWeight;
        //     }
        //
        //     return cohesion;
        // }
        //
        // private float3 SteerInBounds(float3 position, float3 velocity)
        // {
        //     float3 steer = float3.zero;
        //     float3 relativePosition = position - Config.BoundsCenter;
        //
        //     float3 bounds = Config.Bounds;
        //
        //     steer.x = math.select(0, -Config.MinSpeed, relativePosition.x > bounds.x) + math.select(0, Config.MinSpeed, relativePosition.x < -bounds.x);
        //     steer.y = math.select(0, -Config.MinSpeed, relativePosition.y > bounds.y) + math.select(0, Config.MinSpeed, relativePosition.y < -bounds.y);
        //     steer.z = math.select(0, -Config.MinSpeed, relativePosition.z > bounds.z) + math.select(0, Config.MinSpeed, relativePosition.z < -bounds.z);
        //
        //     return velocity + steer;
        // }
        //
        // private float3 ClampVelocity(float3 velocity)
        // {
        //     float speed = math.length(velocity);
        //     
        //     if (speed > Config.MaxSpeed)
        //     {
        //         velocity = math.normalize(velocity) * Config.MaxSpeed;
        //     }
        //     else if(speed < Config.MinSpeed)
        //     {
        //         velocity = math.normalize(velocity) * Config.MinSpeed;
        //     }
        //     
        //     return velocity;
        // }
    }
}
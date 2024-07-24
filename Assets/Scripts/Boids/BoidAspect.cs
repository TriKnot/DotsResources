using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Biods.Movement
{
    [BurstCompile]
    public readonly partial struct BoidAspect : IAspect
    {
        public readonly RefRW<Boid> Boid;
        public readonly RefRW<LocalTransform> Transform;
        
        [BurstCompile]
        public void Simulate( float deltaTime, float3 randomDir, NativeArray<BoidAspect> neighbors, BoidMovementConfig config)
        {
            float3 position = Transform.ValueRO.Position;
            float3 velocity = Boid.ValueRO.Velocity;
            
            float3 separation = CalculateSeparation(position, config, neighbors);
            float3 alignment = CalculateAlignment(position, config, neighbors);
            float3 cohesion = CalculateCohesion(position, config, neighbors);

            float3 boundsSteer = SteerInBounds(position, velocity, config);
            
            float3 randomVelocity = randomDir * config.RandomScatterWeight;

            float3 newVelocity = velocity + separation + alignment + cohesion + boundsSteer + randomVelocity;
            newVelocity = ClampVelocity(newVelocity, config);

            Boid.ValueRW.Velocity = newVelocity;
            Transform.ValueRW.Position += newVelocity * deltaTime;
        }
        
        [BurstCompile]
        public NativeList<int> FindNeighborIndexes(int index, NativeArray<BoidAspect> aspects, float maxNeighborDistance)
        {
            NativeList<int> neighbors = new NativeList<int>(Allocator.Temp);
            float3 position = aspects[index].Transform.ValueRO.Position;
            
            for (int i = 0; i < aspects.Length; i++)
            {
                if (i == index) continue;

                float3 otherPosition = aspects[i].Transform.ValueRO.Position;
                float distance = math.distance(position, otherPosition);
                if (distance < maxNeighborDistance)
                {
                    neighbors.Add(i);
                }
            }

            return neighbors;
        }
        
        [BurstCompile]
        public float3 CalculateSeparation(float3 position, BoidMovementConfig config, NativeArray<BoidAspect> aspects)
        {
            float3 separation = float3.zero;
            int neighborCount = 0;

            foreach (BoidAspect aspect in aspects)
            {
                float3 otherPosition = aspect.Transform.ValueRO.Position;
                float distance = math.distance(position, otherPosition);
                if (distance < config.SeparationRange)
                {
                    float3 diff = position - otherPosition;
                    float3 direction = math.normalize(diff);
                    float3 scaledDiff = direction / distance;
                    
                    separation += scaledDiff;
                    neighborCount++;
                }
            }

            if(neighborCount > 0)
            {
                separation /= neighborCount;
            }

            return separation * config.SeparationWeight;
        }
        
        [BurstCompile]
        public float3 CalculateAlignment(float3 position, BoidMovementConfig config, NativeArray<BoidAspect> aspects)
        {
            float3 alignment = float3.zero;
            int neighborCount = 0;

            foreach (BoidAspect aspect in aspects)
            {
                float3 otherPosition = aspect.Transform.ValueRO.Position;
                float distance = math.distance(position, otherPosition);
                if (distance < config.AlignmentRange)
                {
                    float3 otherVelocity = aspect.Boid.ValueRW.Velocity;
                    
                    alignment += otherVelocity;
                    neighborCount++;
                }
            }

            if(neighborCount > 0)
            {
                alignment /= neighborCount;
            }

            return alignment * config.AlignmentWeight;
        }
        
        [BurstCompile]
        public float3 CalculateCohesion(float3 position, BoidMovementConfig config, NativeArray<BoidAspect> aspects)
        {
            float3 cohesion = float3.zero;
            int neighborCount = 0;

            foreach (BoidAspect aspect in aspects)
            {
                float3 otherPosition = aspect.Transform.ValueRO.Position;
                float distance = math.distance(position, otherPosition);
                if (distance < config.CohesionRange)
                {
                    cohesion += otherPosition;
                    neighborCount++;
                }
            }

            if(neighborCount > 0)
            {
                cohesion /= neighborCount;
                float3 cohesionDirection = cohesion - position;
                cohesion = math.normalize(cohesionDirection);
            }

            return cohesion * config.CohesionWeight;
        }

        [BurstCompile]
        public float3 SteerInBounds(float3 position, float3 velocity, BoidMovementConfig config)
        {
            float3 steer = float3.zero;
            float3 relativePosition = position - config.BoundsCenter;
            float speed = math.length(velocity);

            float3 bounds = config.Bounds;

            steer.x = math.select(0, -speed, relativePosition.x > bounds.x) + math.select(0, speed, relativePosition.x < -bounds.x);
            steer.y = math.select(0, -speed, relativePosition.y > bounds.y) + math.select(0, speed, relativePosition.y < -bounds.y);
            steer.z = math.select(0, -speed, relativePosition.z > bounds.z) + math.select(0, speed, relativePosition.z < -bounds.z);

            return velocity + steer;
        }
        
        public float3 ClampVelocity(float3 velocity, BoidMovementConfig config)
        {
            float speed = math.length(velocity);
            if (speed > config.MaxSpeed)
            {
                float3 direction = math.normalize(velocity);
                velocity = direction * config.MaxSpeed;
                return velocity;
            }
            
            if (speed < config.MinSpeed)
            {
                float3 direction = math.normalize(velocity);
                velocity = direction * config.MinSpeed;
                return velocity;
            }

            return velocity;
        }

    }
}
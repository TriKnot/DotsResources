using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Biods.Movement
{
    [BurstCompile]
    public partial struct BoidSimulationJob : IJobFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<float3> Velocities;
        
        public NativeArray<float3> NewVelocities;
        public BoidMovementConfig Config;

        public void Execute(int index)
        {
            float3 position = Positions[index];
            float3 velocity = Velocities[index];

            velocity += CalculateSeparation(index, position);
            velocity += CalculateAlignment(index, position);
            velocity += CalculateCohesion(index, position);
            
            velocity = KeepInBounds(position, velocity);
            
            velocity = ClampVelocity(velocity);

            NewVelocities[index] = velocity;
        }

        private float3 CalculateSeparation(int index, float3 position)
        {
            float3 separation = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == index) continue;

                float3 otherPosition = Positions[i];
                float distance = math.distance(position, otherPosition);
                if (distance < Config.SeparationRange)
                {
                    float3 diff = position - otherPosition;
                    float3 direction = math.normalize(diff);
                    float3 weightedDiff = direction / distance;
                    
                    separation += weightedDiff;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separation /= neighborCount;
                separation *= Config.SeparationWeight;
            }

            return separation;
        }

        private float3 CalculateAlignment(int index, float3 position)
        {
            float3 alignment = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == index) continue;

                float3 otherPosition = Positions[i];
                float distance = math.distance(position, otherPosition);
                if (distance < Config.AlignmentRange)
                {
                    float3 otherVelocity = Velocities[i];
                    
                    alignment += otherVelocity;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                alignment /= neighborCount;
                alignment *= Config.AlignmentWeight;
            }

            return alignment;
        }

        private float3 CalculateCohesion(int index, float3 position)
        {
            float3 cohesion = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == index) continue;

                float3 otherPosition = Positions[i];
                float distance = math.distance(position, otherPosition);
                if (distance < Config.CohesionRange)
                {
                    cohesion += otherPosition;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                cohesion /= neighborCount;
                float3 cohesionDirection = cohesion - position;
                cohesion = math.normalize(cohesionDirection);
                
                cohesion *= Config.CohesionWeight;
            }

            return cohesion;
        }

        private float3 KeepInBounds(float3 position, float3 velocity)
        {
            float3 steer = float3.zero;
            float3 desired = float3.zero;

            desired.x = math.select(0, 1, position.x < -Config.Bounds.x) + math.select(0, -1, position.x > Config.Bounds.x);
            desired.y = math.select(0, 1, position.y < -Config.Bounds.y) + math.select(0, -1, position.y > Config.Bounds.y);
            desired.z = math.select(0, 1, position.z < -Config.Bounds.z) + math.select(0, -1, position.z > Config.Bounds.z);
            
            if(!math.all(desired == float3.zero))
            {
                desired = math.normalize(desired) * math.length(velocity);
                steer = desired - velocity;
            }

            return velocity + steer;
        }
        
        
        private float3 ClampVelocity(float3 velocity)
        {
            float speed = math.length(velocity);
            
            if (speed > Config.MaxSpeed)
            {
                velocity = math.normalize(velocity) * Config.MaxSpeed;
            }
            else if(speed < Config.MinSpeed)
            {
                velocity = math.normalize(velocity) * Config.MinSpeed;
            }
            
            return velocity;
        }
    }
}
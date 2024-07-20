﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Biods.Movement
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, CompileSynchronously = true)]
    public struct BoidSimulationJob : IJobFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<float3> Velocities;
        [ReadOnly] public uint RandomSeed;
        
        public NativeArray<float3> NewVelocities;
        public BoidMovementConfig Config;
        public void Execute(int index)
        {
            float3 position = Positions[index];
            float3 velocity = Velocities[index];
            
            NativeList<int> neighbors = FindNeighbors(index, position, Config.MaxNeighborDistance);

            velocity += CalculateSeparation( position, neighbors);
            velocity += CalculateAlignment( position, neighbors);
            velocity += CalculateCohesion( position, neighbors);
            
            Random random = new Random((uint) (RandomSeed * (index + 1)));
            velocity += random.NextFloat3(new float3(-1,-1,-1), new float3(1,1,1)) * Config.RandomScatterWeight;
            
            velocity = SteerInBounds(position, velocity);
            
            velocity = ClampVelocity(velocity);

            NewVelocities[index] = velocity;
            
        }

        private NativeList<int> FindNeighbors(int index, float3 position, float maxNeighborDistance)
        {
            NativeList<int> neighbors = new NativeList<int>(Allocator.Temp);

            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == index) continue;

                float3 otherPosition = Positions[i];
                float distance = math.distance(position, otherPosition);
                if (distance < maxNeighborDistance)
                {
                    neighbors.Add(i);
                }
            }

            return neighbors;
        }

        private float3 CalculateSeparation(float3 position, NativeList<int> neighbors)
        {
            float3 separation = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < neighbors.Length; i++)
            {
                int index = neighbors[i];
                float3 otherPosition = Positions[index];
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

        private float3 CalculateAlignment(float3 position, NativeList<int> neighbors)
        {
            float3 alignment = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < neighbors.Length; i++)
            {
                int index = neighbors[i];
                float3 otherPosition = Positions[index];
                float distance = math.distance(position, otherPosition);
                if (distance < Config.AlignmentRange)
                {
                    float3 otherVelocity = Velocities[index];
                    
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

        private float3 CalculateCohesion(float3 position, NativeList<int> neighbors)
        {
            float3 cohesion = float3.zero;
            int neighborCount = 0;

            for (int i = 0; i < neighbors.Length; i++)
            {
                int index = neighbors[i];
                float3 otherPosition = Positions[index];
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
        
        private float3 SteerInBounds(float3 position, float3 velocity)
        {
            float3 steer = float3.zero;
            float3 relativePosition = position - Config.BoundsCenter;

            float3 bounds = Config.Bounds;

            steer.x = math.select(0, -Config.MinSpeed, relativePosition.x > bounds.x) + math.select(0, Config.MinSpeed, relativePosition.x < -bounds.x);
            steer.y = math.select(0, -Config.MinSpeed, relativePosition.y > bounds.y) + math.select(0, Config.MinSpeed, relativePosition.y < -bounds.y);
            steer.z = math.select(0, -Config.MinSpeed, relativePosition.z > bounds.z) + math.select(0, Config.MinSpeed, relativePosition.z < -bounds.z);

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
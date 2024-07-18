using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Biods.Movement
{
    [BurstCompile]
    public readonly partial struct BoidsAspect : IAspect
    {
        public readonly RefRW<Boid> Boid;
        public readonly RefRW<LocalTransform> Transform;

        public float3 Position
        {
            get => Transform.ValueRO.Position;
            set => Transform.ValueRW.Position = value;
        }
        
        public float3 Velocity
        {
            get => Boid.ValueRW.Velocity;
            set => Boid.ValueRW.Velocity = value;
        }
    }
}
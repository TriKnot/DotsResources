using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Biods
{
    [BurstCompile]
    public struct Boid : IComponentData
    {
        public float3 Velocity;
    }

    [BurstCompile]
    public class BoidAuthoring : MonoBehaviour
    {
        public float3 StartVelocity;
        public class BoidBaker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new Boid { Velocity = authoring.StartVelocity } );
            }
        }
    }
}
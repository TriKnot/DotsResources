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
        public float MinStartSpeed;
        public float MaxStartSpeed;

        public class BoidBaker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                float3 direction = new float3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                float3 velocity = math.normalize(direction) * UnityEngine.Random.Range(authoring.MinStartSpeed, authoring.MaxStartSpeed);
                
                AddComponent(entity, new Boid { Velocity = velocity });
            }
        }
    }
}
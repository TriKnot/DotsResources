using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Biods.Spawner
{
    [BurstCompile]
    public struct BoidSpawnerConfig : IComponentData
    {
        public Entity Prefab;
        public int Count;
        public float3 Bounds;
    }
    
    [BurstCompile]
    public class BoidSpawnerConfigAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public int Count;
        public float3 Bounds;

        public class BoidSpawnerBaker : Baker<BoidSpawnerConfigAuthoring>
        {
            public override void Bake(BoidSpawnerConfigAuthoring configAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new BoidSpawnerConfig
                    {
                        Prefab = GetEntity(configAuthoring.Prefab, TransformUsageFlags.Dynamic), 
                        Count = configAuthoring.Count,
                        Bounds = configAuthoring.Bounds
                    });
            }
        }
    }
}
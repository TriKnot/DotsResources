using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Biods.Movement
{
    [BurstCompile]
    public struct BoidMovementConfig : IComponentData
    {
        public float3 Bounds;
        public float3 BoundsCenter;
        
        public float SeparationRange;
        public float AlignmentRange;
        public float CohesionRange;
        
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
        
        public float MaxSpeed;
        public float MinSpeed;
    }

    public class BoidMovementConfigAuthoring : MonoBehaviour
    {
        public float3 Bounds;
        public float3 BoundsCenter;
        
        public float SeparationRange;
        public float AlignmentRange;
        public float CohesionRange;
        
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
        
        public float MaxSpeed;
        public float MinSpeed;

        public class BoidMovementConfigBaker : Baker<BoidMovementConfigAuthoring>
        {
            public override void Bake(BoidMovementConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new BoidMovementConfig
                    {
                        Bounds = authoring.Bounds,
                        BoundsCenter = authoring.BoundsCenter,
                        
                        SeparationRange = authoring.SeparationRange,
                        AlignmentRange = authoring.AlignmentRange,
                        CohesionRange = authoring.CohesionRange,
                        
                        SeparationWeight = authoring.SeparationWeight,
                        AlignmentWeight = authoring.AlignmentWeight,
                        CohesionWeight = authoring.CohesionWeight,
                        
                        MaxSpeed = authoring.MaxSpeed,
                        MinSpeed = authoring.MinSpeed,
                    });
            }
        }
    }
}
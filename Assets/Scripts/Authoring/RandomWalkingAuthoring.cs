using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

class RandomWalkingAuthoring : MonoBehaviour
{
    public float3 targetPosition;
    public float3 originPosition;
    public float distanceMin;
    public float distanceMax;
    public uint randomSeed;
    
    class RandomWalkingAuthoringBaker : Baker<RandomWalkingAuthoring>
    {
        public override void Bake(RandomWalkingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomWalking
            {
                targetPosition = authoring.targetPosition,
                originPosition = authoring.originPosition,
                distanceMin = authoring.distanceMin,
                distanceMax = authoring.distanceMax,
                random = new Random(authoring.randomSeed),
            });
        }
    }
}

public struct RandomWalking : IComponentData
{
    public float3 targetPosition;
    public float3 originPosition;
    public float distanceMin;
    public float distanceMax;
    public Random random;
}
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class ShootVictionAuthoring : MonoBehaviour
{
    public Transform hitPositionTransform;
    
    class ShootVictionAuthoringBaker : Baker<ShootVictionAuthoring>
    {
        public override void Bake(ShootVictionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {
                hitLocalPosition = authoring.hitPositionTransform.localPosition,
            });
        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 hitLocalPosition;
}
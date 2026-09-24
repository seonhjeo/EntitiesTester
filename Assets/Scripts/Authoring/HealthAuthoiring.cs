using Unity.Entities;
using UnityEngine;

class HealthAuthoiring : MonoBehaviour
{
    public int healthAmount;
    
    class HealthAuthoiringBaker : Baker<HealthAuthoiring>
    {
        public override void Bake(HealthAuthoiring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                healthAmount = authoring.healthAmount,
            });
        }
    }
}

public struct Health : IComponentData
{
    public int healthAmount;
}


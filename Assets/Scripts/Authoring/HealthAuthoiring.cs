using Unity.Entities;
using UnityEngine;

class HealthAuthoiring : MonoBehaviour
{
    public int healthAmount;
    public int healthAmountMax;
    
    class HealthAuthoiringBaker : Baker<HealthAuthoiring>
    {
        public override void Bake(HealthAuthoiring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                healthAmount = authoring.healthAmount,
                healthAmountMax = authoring.healthAmountMax,
                onHealthChanged = true
            });
        }
    }
}

public struct Health : IComponentData
{
    public int healthAmount;
    public int healthAmountMax;
    public bool onHealthChanged;
}


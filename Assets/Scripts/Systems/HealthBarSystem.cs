using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct HealthBarSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        
        foreach (RefRO<HealthBar> healthBar in SystemAPI.Query<RefRO<HealthBar>>())
        {
            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);
            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
    }
}

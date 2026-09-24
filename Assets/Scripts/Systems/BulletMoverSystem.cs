using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach ((
                     RefRW<LocalTransform> localTransform,
                     RefRO<Bullet> bullet,
                     RefRO<Target> target,
                     Entity entity)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRO<Bullet>,
                     RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }
            
            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            
            float distanceBeforeSq = math.distancesq(localTransform.ValueRW.Position, targetLocalTransform.Position);
            
            float3 moveDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            moveDirection = math.normalizesafe(moveDirection);
            
            localTransform.ValueRW.Position += bullet.ValueRO.speed * SystemAPI.Time.DeltaTime * moveDirection;

            float distanceAfterSq = math.distancesq(localTransform.ValueRW.Position, targetLocalTransform.Position);

            if (distanceBeforeSq < distanceAfterSq)
            {
                localTransform.ValueRW.Position = targetLocalTransform.Position;
            }
            
            float destroyDistanceSq = 0.2f;
            if (math.distancesq(localTransform.ValueRW.Position, targetLocalTransform.Position) < destroyDistanceSq)
            {
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}

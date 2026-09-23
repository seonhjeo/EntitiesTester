using Unity.Burst;
using Unity.Entities;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
                    (RefRW<ShootAttack> shootAttack,
                     RefRO<Target> target)
                 in SystemAPI.Query<
                     RefRW<ShootAttack>,
                     RefRO<Target>>()
                 )
        {
            
        }
    }
}

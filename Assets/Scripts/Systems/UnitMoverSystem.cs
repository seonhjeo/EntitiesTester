using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// 유닛의 목표 위치를 따라 이동과 회전을 갱신하는 잡을 매 업데이트마다 병렬로 예약하는 ECS 시스템이다.
/// </summary>
partial struct UnitMoverSystem : ISystem
{
    /// <summary>
    /// 현재 시간 간격을 이동 잡에 전달하고, 필요한 컴포넌트를 가진 엔티티들의 처리를 예약한다.
    /// </summary>
    /// <param name="state">Unity가 전달하는 현재 ECS 시스템의 상태.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 시간 간격을 전달하고 유닛 이동 잡을 병렬로 예약한다.
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
        };
        
        unitMoverJob.ScheduleParallel();

        // 잡으로 전환하기 전의 직접 순회 예제다. 현재는 실행되지 않는다.
        // foreach ((
        //              RefRW<LocalTransform> localTransform,
        //              RefRO<UnitMover> unitMover,
        //              RefRW<PhysicsVelocity> physicsVelocity)
        //          in SystemAPI.Query<
        //              RefRW<LocalTransform>,
        //              RefRO<UnitMover>,
        //             RefRW<PhysicsVelocity>>())
        // {
        //     float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
        //     moveDirection = math.normalizesafe(moveDirection);
        //     
        //     localTransform.ValueRW.Rotation =
        //         math.slerp(localTransform.ValueRO.Rotation, quaternion.LookRotationSafe(moveDirection, math.up()),
        //             SystemAPI.Time.DeltaTime * unitMover.ValueRO.RotationSpeed);
        //     
        //     physicsVelocity.ValueRW.Linear = unitMover.ValueRO.MoveSpeed * moveDirection;
        //     physicsVelocity.ValueRW.Angular = float3.zero;
        // }
    }
}


/// <summary>
/// LocalTransform, UnitMover, PhysicsVelocity를 가진 각 엔티티에 목표 이동을 적용하는 Burst 잡이다.
/// 선택 여부로 필터링하지 않으므로 선택을 해제해도 마지막으로 지정된 목표를 향해 이동한다.
/// </summary>
[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    // 시스템에서 전달받은 업데이트 간격(초)으로, 목표 방향을 향한 회전 보간에 사용한다.
    public float DeltaTime;
    
    /// <summary>
    /// 유닛이 목표 근처에 도착하면 정지시키고, 그렇지 않으면 목표 방향으로 회전과 선속도를 설정한다.
    /// </summary>
    /// <param name="localTransform">현재 위치를 읽고 회전을 수정할 유닛의 로컬 변환 데이터.</param>
    /// <param name="unitMover">이동 속도, 회전 보간 계수, 목표 위치를 제공하는 읽기 전용 이동 데이터.</param>
    /// <param name="physicsVelocity">물리 시뮬레이션에 전달할 선속도와 각속도를 기록하는 데이터.</param>
    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        // 목표까지의 제곱 거리를 비교하고 도착 범위 안이면 정지한다.
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = 2f;
        if (math.lengthsq(moveDirection) < reachedTargetDistanceSq)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        
        // 이동 방향을 정규화하고 해당 방향을 바라보도록 회전한다.
        moveDirection = math.normalizesafe(moveDirection);
            
        localTransform.Rotation =
            math.slerp(localTransform.Rotation, quaternion.LookRotationSafe(moveDirection, math.up()),
                DeltaTime * unitMover.RotationSpeed);
            
        // 이동 속도를 적용하고 물리 각속도를 초기화한다.
        physicsVelocity.Linear = unitMover.MoveSpeed * moveDirection;
        physicsVelocity.Angular = float3.zero;
    }
}

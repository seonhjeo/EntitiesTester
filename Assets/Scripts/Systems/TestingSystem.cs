using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// ECS 쿼리의 필터링 동작을 실험하기 위한 시스템이다. 현재 테스트 코드는 주석 처리되어 실행되지 않는다.
/// </summary>
partial struct TestingSystem : ISystem
{
    /// <summary>
    /// 선택되지 않은 이동 가능 엔티티 수를 세는 예제를 보관한다. 현재는 실행할 로직이 없다.
    /// </summary>
    /// <param name="state">Unity가 전달하는 현재 ECS 시스템의 상태.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 선택되지 않은 이동 가능 엔티티 수를 세는 비활성 테스트 예제다.
        int unitCount = 0;
        foreach ( RefRW<Friendly> friendly
                 in SystemAPI.Query<
                     RefRW<Friendly>>())
        {
            unitCount++;
        }
        
        Debug.Log("unitCount : " + unitCount);
    }
}

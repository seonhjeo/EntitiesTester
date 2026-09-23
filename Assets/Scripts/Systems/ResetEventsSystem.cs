using Unity.Burst;
using Unity.Entities;

/// <summary>
/// LateSimulationSystemGroup에서 선택 관련 이벤트 플래그를 초기화한다.
/// SelectedVisualSystem이 먼저 플래그를 처리하도록 두 시스템의 실행 순서가 지정되어 있다.
/// </summary>
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ResetEventsSystem : ISystem
{
    /// <summary>
    /// 선택 컴포넌트의 활성화 여부와 관계없이 선택 및 선택 해제 이벤트를 모두 초기화한다.
    /// </summary>
    /// <param name="state">Unity가 전달하는 현재 ECS 시스템의 상태.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 비활성화된 Selected도 포함해 선택·해제 이벤트를 초기화한다.
        foreach (RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            selected.ValueRW.onSelected = false;
            selected.ValueRW.onDeselected = false;
        }
    }
}

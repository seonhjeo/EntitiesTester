using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// 선택 이벤트에 따라 표시용 엔티티의 크기를 바꾸는 시스템이다.
/// LateSimulationSystemGroup에서 ResetEventsSystem보다 먼저 실행하여 초기화 전의 이벤트를 읽는다.
/// </summary>
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventsSystem))]
partial struct SelectedVisualSystem : ISystem
{
    /// <summary>
    /// 선택된 유닛의 표시를 켜고 선택 해제된 유닛의 표시를 숨긴다.
    /// 두 이벤트 플래그가 모두 켜져 있으면 뒤에서 처리하는 선택 해제 결과가 적용된다.
    /// </summary>
    /// <param name="state">Unity가 전달하는 현재 ECS 시스템의 상태.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 선택 상태와 관계없이 표시 갱신 이벤트를 확인한다.
        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
        {
            // 선택 시 지정한 크기로 표시한다.
            if (selected.ValueRO.onSelected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = selected.ValueRO.ShowScale;
            }

            // 선택 해제 시 크기를 0으로 줄여 숨긴다.
            if (selected.ValueRO.onDeselected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = 0f;
            }
        }
    }
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 유닛 이동 잡이 참조하는 이동 속도, 회전 보간 속도, 목표 위치를 보관하는 ECS 컴포넌트다.
/// </summary>
public struct UnitMover : IComponentData
{
    // 목표 방향으로 적용할 선속도의 크기로, 초당 이동하는 월드 거리 단위다.
    public float MoveSpeed;
    // 목표 방향을 바라보도록 회전을 보간할 때 DeltaTime에 곱하는 계수다. 초당 회전 각도가 아니다.
    public float RotationSpeed;
    // 이동 명령으로 지정하는 월드 공간 목표 위치다. 현재 이동 코드는 부모 없는 유닛을 전제로 한다.
    public float3 targetPosition;
}

/// <summary>
/// 인스펙터에서 유닛의 이동 및 회전 속도를 설정하고 이를 ECS 이동 데이터로 변환한다.
/// </summary>
public class UnitMoverAuthoring : MonoBehaviour
{
    // 베이킹 시 UnitMover.MoveSpeed로 복사할 유닛의 이동 속도다.
    public float MoveSpeed;
    // 베이킹 시 UnitMover.RotationSpeed로 복사할 회전 보간 계수다.
    public float RotationSpeed;

    /// <summary>
    /// 게임 오브젝트의 이동 설정을 <see cref="UnitMover"/> 컴포넌트로 변환하는 베이커다.
    /// </summary>
    public class Baker : Baker<UnitMoverAuthoring>
    {
        /// <summary>
        /// 이동 가능한 엔티티에 인스펙터의 속도 설정을 담은 이동 컴포넌트를 추가한다.
        /// </summary>
        /// <param name="authoring">이동 및 회전 속도를 제공하는 저작 컴포넌트.</param>
        public override void Bake(UnitMoverAuthoring authoring)
        {
            // 인스펙터의 이동 및 회전 속도를 엔티티에 등록한다.
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                MoveSpeed = authoring.MoveSpeed,
                RotationSpeed = authoring.RotationSpeed,
            });
        }
    }
}


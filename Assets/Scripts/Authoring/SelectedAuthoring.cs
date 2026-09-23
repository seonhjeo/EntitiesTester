using Unity.Entities;
using UnityEngine;

/// <summary>
/// 유닛의 선택 표시 오브젝트와 표시 크기를 설정하고, 베이킹 시 선택 상태 컴포넌트를 생성한다.
/// </summary>
public class SelectedAuthoring : MonoBehaviour
{
    // 유닛을 선택했을 때 보여 줄 표시용 게임 오브젝트로, 베이킹 시 엔티티 참조로 변환된다.
    public GameObject visualObject;
    // 선택 표시를 켤 때 표시용 엔티티의 LocalTransform.Scale에 적용할 균일 배율이다.
    public float showScale;
    
    /// <summary>
    /// 선택 표시 설정을 <see cref="Selected"/> 컴포넌트로 옮기는 베이커다.
    /// </summary>
    public class Baker : Baker<SelectedAuthoring>
    {
        /// <summary>
        /// 선택 표시 엔티티와 배율을 등록하고, 유닛을 선택되지 않은 상태로 초기화한다.
        /// </summary>
        /// <param name="authoring">선택 표시 오브젝트와 배율을 제공하는 저작 컴포넌트.</param>
        public override void Bake(SelectedAuthoring authoring)
        {
            // 선택 표시 정보를 등록하고 초기 선택 상태를 비활성화한다.
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Selected
            {
                VisualEntity = GetEntity(authoring.visualObject, TransformUsageFlags.Dynamic),
                ShowScale = authoring.showScale,
            });
            SetComponentEnabled<Selected>(entity, false);
        }
    }
}


/// <summary>
/// 유닛의 선택 상태와 표시 설정을 보관한다. 컴포넌트의 활성화 여부가 현재 선택 여부를 나타내며,
/// 활성화 전환은 엔티티의 컴포넌트 구성을 바꾸는 구조적 변경 없이 수행할 수 있다.
/// </summary>
public struct Selected : IComponentData, IEnableableComponent
{
    // 선택 여부에 따라 LocalTransform.Scale을 변경할 표시용 엔티티다.
    public Entity VisualEntity;
    // 선택 시 표시용 엔티티에 적용하는 배율이며, 선택 해제 시에는 0으로 숨긴다.
    public float ShowScale;

    // 선택 발생을 표시 시스템에 알리는 플래그로, 처리 후 ResetEventsSystem에서 false로 초기화한다.
    public bool onSelected;
    // 선택 해제 발생을 알리는 플래그로, Selected가 비활성화되어도 읽어야 한다.
    public bool onDeselected;
}

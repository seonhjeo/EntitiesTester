using Unity.Entities;
using UnityEngine;

/// <summary>
/// 게임 오브젝트가 RTS 유닛임을 나타내며, 베이킹 시 엔티티에 유닛 태그를 추가한다.
/// </summary>
public class UnitAuthoring : MonoBehaviour
{
    public Faction faction;
    
    /// <summary>
    /// 인스펙터에 배치한 <see cref="UnitAuthoring"/>을 ECS의 <see cref="Unit"/> 컴포넌트로 변환한다.
    /// </summary>
    public class Baker : Baker<UnitAuthoring>
    {
        /// <summary>
        /// 움직일 수 있는 엔티티를 가져와 유닛을 식별하는 태그 컴포넌트를 등록한다.
        /// </summary>
        /// <param name="authoring">베이킹 대상 게임 오브젝트에 부착된 저작 컴포넌트.</param>
        public override void Bake(UnitAuthoring authoring)
        {
            // 유닛 엔티티에 Unit 태그를 추가한다.
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit
            {
                faction = authoring.faction,
            });
        }
    }
}

/// <summary>
/// 엔티티가 RTS 유닛임을 표시하는 데이터 없는 태그 컴포넌트로, 선택 쿼리와 클릭 판정에 사용한다.
/// </summary>
public struct Unit : IComponentData
{
    public Faction faction;
}

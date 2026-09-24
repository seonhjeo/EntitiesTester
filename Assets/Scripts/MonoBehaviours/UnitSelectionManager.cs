using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// 마우스 입력을 ECS 유닛 선택 및 이동 명령으로 연결한다.
/// 왼쪽 클릭은 단일 선택, 왼쪽 드래그는 영역 선택, 오른쪽 클릭은 선택한 유닛들의 이동을 처리한다.
/// </summary>
public class UnitSelectionManager : MonoBehaviour
{
    // 선택 UI 등 다른 컴포넌트가 접근하는 공용 인스턴스이며, 외부에서는 값을 변경할 수 없다.
    public static UnitSelectionManager Instance { get; private set; }
    
    // 왼쪽 버튼을 눌러 선택 영역 지정을 시작했음을 UI에 알리는 이벤트다.
    public event EventHandler OnSelectionAreaStart;
    // 왼쪽 버튼을 놓고 선택 처리를 마쳤음을 UI에 알리는 이벤트다.
    public event EventHandler OnSelectionAreaEnd;
    
    // 드래그 시작 시점의 마우스 화면 좌표(픽셀)로, 현재 좌표와 함께 선택 사각형을 계산한다.
    private Vector2 _selectionStartMousePosition;

    /// <summary>
    /// 최초 선택 매니저를 공용 인스턴스로 등록하고 중복된 게임 오브젝트를 제거한다.
    /// </summary>
    private void Awake()
    {
        // 공용 인스턴스를 등록하고 중복 오브젝트를 제거한다.
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 매 프레임 마우스 버튼의 상태 변화를 확인해 선택 영역 시작, 유닛 선택 확정, 이동 목표 지정을 처리한다.
    /// </summary>
    void Update()
    {
        // 드래그 시작 좌표를 저장하고 선택 UI를 표시한다.
        if (Input.GetMouseButtonDown(0))
        {
            _selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        // 왼쪽 버튼을 놓으면 클릭 또는 영역 선택을 확정한다.
        if (Input.GetMouseButtonUp(0))
        {
            var cam = Camera.main;

            // 기존 선택을 모두 해제하고 해제 이벤트를 기록한다.
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);
            }

            // 선택 사각형의 너비와 높이 합으로 클릭과 드래그를 구분한다.
            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                // 선택 상태와 관계없이 유닛을 조회해 사각형 안의 유닛을 선택한다.
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = cam.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }
                }
            }
            else
            {
                // ECS 물리 월드에서 Units 레이어를 검사할 마우스 광선을 준비한다.
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex = 0
                    }
                };
                // 광선에 맞은 엔티티가 유닛이면 선택 상태와 이벤트를 기록한다.
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }

            // 선택 처리를 마치고 선택 UI를 숨긴다.
            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }
        
        // 우클릭 지점과 현재 선택된 이동 가능 유닛을 조회한다.
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager =  World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMovers = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            // 유닛별 목표 위치를 생성하고 이동 컴포넌트에 일괄 반영한다.
            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mousePosition, unitMovers.Length);
            GenerateMovePositionArray(mousePosition, entityArray.Length);
            for (int i = 0; i < unitMovers.Length; i++)
            {
                UnitMover unitMover = unitMovers[i];
                unitMover.targetPosition = movePositionArray[i];
                unitMovers[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMovers);
        }
    }


    /// <summary>
    /// 드래그 방향과 관계없이 너비와 높이가 음수가 되지 않는 화면 공간 선택 사각형을 계산한다.
    /// </summary>
    /// <returns>화면 왼쪽 아래를 원점으로 하는 픽셀 단위의 선택 사각형.</returns>
    public Rect GetSelectionAreaRect()
    {
        // 드래그 양 끝의 최솟값과 최댓값으로 사각형의 모서리를 구한다.
        Vector2 selectionEndMousePosition = Input.mousePosition;
        Vector2 lowerLeftCorner = new Vector2
        (
            Mathf.Min(_selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(_selectionStartMousePosition.y, selectionEndMousePosition.y)
        );
        
        Vector2 upperRightCorner = new Vector2
        (
            Mathf.Max(_selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(_selectionStartMousePosition.y, selectionEndMousePosition.y)
        );

        // 왼쪽 아래 좌표와 두 모서리의 차이로 선택 사각형을 만든다.
        return new Rect
        (
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }

    /// <summary>
    /// 중심점 하나와 그 주위의 동심원에 유닛별 이동 목표를 배치한다.
    /// 바깥쪽 원으로 갈수록 반지름과 배치할 위치 수를 늘린다.
    /// </summary>
    /// <param name="targetPosition">이동 대형의 중심이 되는 월드 좌표.</param>
    /// <param name="positionCount">생성할 목표 위치 수로, 이동할 유닛 수에 대응하는 0 이상의 값.</param>
    /// <returns>요청한 수의 목표 좌표를 담은 Allocator.Temp 배열. 현재 프레임의 임시 처리에 사용한다.</returns>
    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        // 목표 배열과 중심점을 준비하고 유닛이 0~1개면 바로 반환한다.
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        if (positionCount == 0) return positionArray;

        positionArray[0] = targetPosition;
        if (positionCount == 1) return positionArray;

        // 원 사이 간격과 목표를 기록할 인덱스를 초기화한다.
        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        // 바깥쪽 원으로 확장하며 같은 각도 간격으로 목표 위치를 채운다.
        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;
            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPosition + ringVector;
                
                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount) break;
            }
            ring++;
        }

        return positionArray;
    }
}

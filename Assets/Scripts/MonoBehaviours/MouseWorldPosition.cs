using UnityEngine;

/// <summary>
/// 마우스 화면 좌표를 높이 0인 수평면 위의 월드 좌표로 변환하여 유닛 이동 명령에 제공한다.
/// </summary>
public class MouseWorldPosition : MonoBehaviour
{
    // 다른 스크립트에서 좌표 변환 기능에 접근하기 위한 현재 인스턴스다. 설정은 이 클래스 내부에서만 가능하다.
    public static MouseWorldPosition Instance { get; private set; }

    /// <summary>
    /// 최초 인스턴스를 등록하고 중복 생성된 게임 오브젝트를 제거한다.
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
    /// 메인 카메라에서 마우스 방향으로 광선을 쏘아 XZ 평면과 만나는 위치를 계산한다.
    /// 실제 지형 콜라이더 대신 수학적 평면을 사용하므로 지형 높낮이는 반영하지 않는다.
    /// </summary>
    /// <returns>광선과 y = 0 평면의 교점. 교차하지 않으면 월드 원점을 반환한다.</returns>
    public Vector3 GetPosition()
    {
        // 마우스 광선과 지면(y = 0)의 교점을 구한다.
        Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(mouseCameraRay, out float distance))
        {
            return mouseCameraRay.GetPoint(distance);
        }

        return Vector3.zero;
    }
}

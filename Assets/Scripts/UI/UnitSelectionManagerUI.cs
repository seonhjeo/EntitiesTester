using System;
using UnityEngine;

/// <summary>
/// 유닛 선택 매니저의 이벤트를 받아 드래그 선택 사각형을 표시하고 마우스 움직임에 맞춰 갱신한다.
/// </summary>
public class UnitSelectionManagerUI : MonoBehaviour
{
    // 화면에 그릴 선택 사각형의 RectTransform이다. 비공개 필드지만 인스펙터에서 연결할 수 있다.
    [SerializeField] private RectTransform selectionAreaRectTransform;
    // 선택 사각형이 속한 캔버스로, 화면 픽셀 좌표를 UI 좌표로 환산할 때 배율을 참조한다.
    [SerializeField] private Canvas canvas;

    /// <summary>
    /// 선택 시작 및 종료 이벤트를 구독하고, 입력이 시작되기 전에는 선택 사각형을 숨긴다.
    /// </summary>
    private void Start()
    {
        // 선택 이벤트를 구독하고 초기 UI를 숨긴다.
        UnitSelectionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;
        
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    /// <summary>
    /// 선택 사각형이 활성화된 동안 현재 드래그 범위에 맞게 위치와 크기를 갱신한다.
    /// </summary>
    private void Update()
    {
        // 선택 UI가 표시 중이면 드래그 범위를 갱신한다.
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }
    
    /// <summary>
    /// 선택 영역 지정이 시작되면 사각형을 표시하고 시작 위치를 즉시 반영한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 유닛 선택 매니저.</param>
    /// <param name="e">이벤트 데이터. 현재는 추가 정보가 없는 EventArgs.Empty가 전달된다.</param>
    private void UnitSelectionManager_OnSelectionAreaStart(object sender, EventArgs e)
    {
        // 선택 UI를 표시하고 현재 드래그 범위를 반영한다.
        selectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }
    
    /// <summary>
    /// 선택 처리가 끝나면 드래그 사각형을 숨긴다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 유닛 선택 매니저.</param>
    /// <param name="e">이벤트 데이터. 현재는 추가 정보가 없는 EventArgs.Empty가 전달된다.</param>
    private void UnitSelectionManager_OnSelectionAreaEnd(object sender, EventArgs e)
    {
        // 선택이 끝나면 UI를 숨긴다.
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    /// <summary>
    /// 화면 공간 선택 사각형을 캔버스 배율로 환산해 UI의 위치와 크기에 적용한다.
    /// 화면 왼쪽 아래 좌표와 맞는 앵커 및 피벗, 균일한 캔버스 배율을 사용하는 배치를 전제로 한다.
    /// </summary>
    private void UpdateVisual()
    {
        // 선택 범위를 캔버스 배율로 환산해 UI 위치와 크기에 적용한다.
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

        float canvasScale = canvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
    }
}

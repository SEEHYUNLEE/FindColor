using UnityEngine;
using UnityEngine.EventSystems;

public class SaveUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject savePanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 실제 세이브창을 클릭한 경우에는 닫지 않음
        if (RectTransformUtility.RectangleContainsScreenPoint(
            savePanel.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            return;
        }

        // 세이브창 바깥을 클릭하면 닫기
        gameObject.SetActive(false);
    }
}
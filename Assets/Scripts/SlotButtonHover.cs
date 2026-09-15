using UnityEngine;
using UnityEngine.EventSystems;

public class SlotButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator targetAnimator;

    // SaveSlotUI에서 생성된 플레이어 프리팹의 Animator를 전달받는 함수
    public void Setup(GameObject playerInstance)
    {
        if (playerInstance != null)
        {
            targetAnimator = playerInstance.GetComponentInChildren<Animator>();
        }
    }

    // 마우스를 올렸을 때 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetBool("IsHovered", true);
        }
    }

    // 마우스를 뗐을 때 실행
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetBool("IsHovered", false);
        }
    }
}
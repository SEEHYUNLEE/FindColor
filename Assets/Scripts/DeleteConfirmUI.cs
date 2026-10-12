using UnityEngine;
using UnityEngine.UI;

public class DeleteConfirmUI : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private int targetSlotIndex = -1;
    private SaveSlotUI ownerSlotUI;

    private void Awake()
    {
        if (yesButton != null) yesButton.onClick.AddListener(OnClickYes);
        if (noButton != null) noButton.onClick.AddListener(OnClickNo);
    }

    public void OpenPopup(int slotIndex, SaveSlotUI slotUI)
    {
        targetSlotIndex = slotIndex;
        ownerSlotUI = slotUI;
        gameObject.SetActive(true);
    }

    private void OnClickYes()
    {
        if (targetSlotIndex != -1)
        {
            DataManager.Instance.DeleteSlot(targetSlotIndex);

            // 삭제 후 슬롯 UI 텍스트 및 X버튼 상태 갱신
            if (ownerSlotUI != null)
            {
                ownerSlotUI.UpdateSlotUI();
            }
        }
        AudioManager.Instance.PlaySFX(SFXType.Click);
        ClosePopup();
    }

    private void OnClickNo()
    {
        AudioManager.Instance.PlaySFX(SFXType.Click);
        ClosePopup();
    }

    private void ClosePopup()
    {
        targetSlotIndex = -1;
        AudioManager.Instance.PlaySFX(SFXType.Click);
        gameObject.SetActive(false);
    }
}
using UnityEngine;

public class RepairShop : MonoBehaviour
{
    [SerializeField] private GameObject upgradeUIPanel; // 켜고 끌 강화 UI Panel 오브젝트

    private void Start()
    {
        // 시작 시 UI 비활성화
        if (upgradeUIPanel != null)
        {
            upgradeUIPanel.SetActive(false);
        }
    }

    // 플레이어가 영역 내로 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (upgradeUIPanel != null)
            {
                upgradeUIPanel.SetActive(true);

                // UI가 열릴 때 최신 골드 및 레벨 정보로 갱신
                UpgradeUI upgradeUI = upgradeUIPanel.GetComponent<UpgradeUI>();
                if (upgradeUI != null)
                {
                    upgradeUI.UpdateUI();
                }
            }
        }
    }

    // 플레이어가 영역 밖으로 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (upgradeUIPanel != null)
            {
                upgradeUIPanel.SetActive(false);
            }
        }
    }
}
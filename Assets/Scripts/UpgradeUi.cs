using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용할 경우 (일반 Text 사용 시 UnityEngine.UI.Text로 변경)

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private Button hpUpgradeButton;
    [SerializeField] private Button speedUpgradeButton;
    [SerializeField] private Button damageUpgradeButton;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI hpLevelText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI damageLevelText;

    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        hpUpgradeButton.onClick.AddListener(OnClickUpgradeHp);
        speedUpgradeButton.onClick.AddListener(OnClickUpgradeSpeed);
        damageUpgradeButton.onClick.AddListener(OnClickUpgradeDamage);

        UpdateUI();
    }

    // --- [버튼 클릭 이벤트 함수들] ---

    // 1. HP 강화 버튼에 연결할 함수
    public void OnClickUpgradeHp()
    {
        if (DataManager.Instance == null) return;

        // DataManager를 통해 강화 시도
        if (DataManager.Instance.UpgradeStat(StatType.Hp))
        {
            // 스탯 즉시 반영
            playerController?.ApplyUpgradeStats();
            // UI 텍스트 갱신
            UpdateUI();
            Debug.Log("[Upgrade] HP 강화 성공!");
        }
    }

    // 2. Speed 강화 버튼에 연결할 함수
    public void OnClickUpgradeSpeed()
    {
        if (DataManager.Instance == null) return;

        if (DataManager.Instance.UpgradeStat(StatType.Speed))
        {
            // 스탯 즉시 반영
            playerController?.ApplyUpgradeStats();
            UpdateUI();
            Debug.Log("[Upgrade] Speed 강화 성공!");
        }
    }

    // 3. Damage 강화 버튼에 연결할 함수
    public void OnClickUpgradeDamage()
    {
        if (DataManager.Instance == null) return;

        if (DataManager.Instance.UpgradeStat(StatType.Damage))
        {
            // AttackEffect는 생성이 시점(Start)에 데이터를 읽으므로 별도 호출 필요 없음
            UpdateUI();
            Debug.Log("[Upgrade] Damage 강화 성공!");
        }
    }

    // --- UI 텍스트 업데이트 ---
    public void UpdateUI()
    {
        if (DataManager.Instance == null || DataManager.Instance.currentData == null) return;

        var data = DataManager.Instance.currentData;

        if (goldText != null)
            goldText.text = $"Gold: {data.gold}";

        if (hpLevelText != null)
            hpLevelText.text = data.hpLevel >= 7 ? "HP: MAX" : $"HP Lv: {data.hpLevel}";

        if (speedLevelText != null)
            speedLevelText.text = data.speedLevel >= 7 ? "Speed: MAX" : $"Speed Lv: {data.speedLevel}";

        if (damageLevelText != null)
            damageLevelText.text = data.damageLevel >= 7 ? "Damage: MAX" : $"Damage Lv: {data.damageLevel}";
    }
}
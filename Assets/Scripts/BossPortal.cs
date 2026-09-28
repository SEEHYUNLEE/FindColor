using UnityEngine;

public class BossPortal : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Stage가 8일 때만 활성화
        if (GameManager.Instance.GetCurrentStage() == 8)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player가 아니면 무시
        if (!collision.CompareTag("Player"))
            return;

        GameManager.Instance.LoadBossStage();
    }
}
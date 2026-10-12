using UnityEngine;
public class AttackEffect : MonoBehaviour
{
    private int baseDamage = 10;
    private int damagePerLevel = 10;

    private Vector3 maxLevelScale = new Vector3(3f, 3f, 1f);
    private int finalDamage;

    private void Start()
    {
        AudioManager.Instance.PlaySFX(SFXType.PlayerAttack);
        if (DataManager.Instance != null && DataManager.Instance.currentData != null)
        {
            int damageLevel = DataManager.Instance.currentData.damageLevel;

            // [Damage 만렙 특수효과]
            if (damageLevel >= 7)
            {
                finalDamage = 1000;
                transform.localScale = Vector3.Scale(transform.localScale, maxLevelScale); // 이펙트 거대화
            }
            else
            {
                finalDamage = baseDamage + (damageLevel * damagePerLevel);
            }
        }
        else
        {
            finalDamage = baseDamage;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 대상에게 SlimeController가 있는지 확인
        if (collision.TryGetComponent<SlimeController>(out SlimeController slime))
        {
            // 슬라임이 살아있는 상태일 때만 데미지 부여
            if (!slime.IsDead())
            {
                slime.TakeDamage(finalDamage);
                Destroy(gameObject);
            }
        }

        if (collision.TryGetComponent<Boss>(out Boss boss))
        {
            if (!boss.IsDead())
            {
                boss.TakeDamage(finalDamage);
                Destroy(gameObject);
            }
        }
    }

    // 애니메이션의 마지막 프레임에 등록
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}

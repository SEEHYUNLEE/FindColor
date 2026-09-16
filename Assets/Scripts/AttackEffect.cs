using UnityEngine;
public class AttackEffect : MonoBehaviour
{
    private float maxLifetime = 5f; // 이벤트 미발생 대비 안전용 최대 수명
    private int baseDamage = 10;
    private int damagePerLevel = 10;

    private Vector3 maxLevelScale = new Vector3(3f, 3f, 1f);
    private int finalDamage;

    private void Start()
    {
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
        // 만약 애니메이션 이벤트가 어떤 이유로 호출되지 않더라도 
        // 일정 시간이 지나면 메모리 누수를 방지하기 위해 자동으로 삭제
        Destroy(gameObject, maxLifetime);
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
    }

    // [애니메이션 이벤트용 함수]
    // 애니메이션의 마지막 프레임에 이 함수를 등록해둡니다.
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}

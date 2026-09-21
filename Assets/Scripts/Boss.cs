using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    private int maxHp = 1000;
    private int currentHp;

    private bool isDead;

    private int missileSpeed = 5;

    private float jumpTime = 1.4f;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Transform player;

    private int flashCount = 2;
    private float flashInterval = 0.05f;
    private Color flashColor1 = Color.black;
    private Color flashColor2 = Color.gray;

    private Coroutine flashCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHp = maxHp;

        StartCoroutine(MissileStart());
        StartCoroutine(JumpMove());
    }

    IEnumerator MissileStart()
    {
        while (true)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Mathf.PI * 2f * i / 8f;

                Vector2 direction = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ).normalized;

                BossMissile missile = MissilePool.Instance.GetMissile();
                
                // 중앙에서 거리를 두고 생성
                missile.transform.position = transform.position + (Vector3)(direction);

                missile.Initialize(
                    direction,
                    missileSpeed,
                    true
                );
            }

            yield return new WaitForSeconds(2f);
        }
    }
    public void Shot()
    {
        if (isDead)
            return;

        for (int i = 0; i < 5; i++)
        {
            float angle = Mathf.PI * 2f * i / 5f;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ).normalized;

            BossMissile missile = MissilePool.Instance.GetMissile();

            // 보스 중심에서 1만큼 떨어진 곳에서 생성
            missile.transform.position =
                transform.position + (Vector3)direction;

            // 휘지 않고 직선으로 발사
            missile.Initialize(
                direction,
                missileSpeed,
                false
            );
        }
    }

    IEnumerator JumpMove()
    {
        while (true)
        {
            // 점프 시작 시 플레이어 위치 저장
            Vector3 targetPosition = player.position;

            // 이동 방향에 따라 보스 방향 변경
            if (targetPosition.x > transform.position.x)
            {
                // 오른쪽으로 이동
                spriteRenderer.flipX = true;
            }
            else
            {
                // 왼쪽으로 이동
                spriteRenderer.flipX = false;
            }

            // 점프 애니메이션 실행
            animator.SetTrigger("Jump");

            Vector3 startPosition = transform.position;

            float elapsedTime = 0f;

            // 1.4초 동안 이동(애니메이션 시간)
            while (elapsedTime < jumpTime)
            {
                elapsedTime += Time.deltaTime;

                float t = elapsedTime / jumpTime;

                transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

                yield return null;
            }

            // 정확하게 목표 위치로 이동
            transform.position = targetPosition;

            // 랜덤 대기
            float waitTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(waitTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerController playerController =
                collision.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.TakeDamage(50f);

                Vector2 knockbackDirection =
                    (collision.transform.position - transform.position).normalized;

                playerController.KnockBack(
                    knockbackDirection,
                    10f
                );
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        // 중복 피격 제어
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashColorRoutine());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void Die()
    {
        isDead = true;

        StopAllCoroutines();

        animator.SetTrigger("Die");
    }

    private IEnumerator FlashColorRoutine()
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor1; // 검은색

            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.color = flashColor2; // 회색

            yield return new WaitForSeconds(flashInterval);
        }

        // 원래 색상으로 복구
        spriteRenderer.color = originalColor;

        flashCoroutine = null;
    }
}
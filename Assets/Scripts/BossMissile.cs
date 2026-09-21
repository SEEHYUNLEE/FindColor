using UnityEngine;

public class BossMissile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;

    private float lifeTime = 3f;
    private float curveSpeed = 60f;

    private float timer;

    private bool isCurve;

    private Animator animator;

    private bool hasHit;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // 미사일 풀에 들어갔다가 꺼내는 것이므로 초기화 중요
    public void Initialize(Vector2 direction, float speed, bool isCurve)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.isCurve = isCurve;

        timer = 0f;
        hasHit = false;

        animator.Rebind();
        animator.Update(0f);

        UpdateRotation();
    }

    private void Update()
    {
        if (hasHit)
            return;
        // 휘어지는 미사일만 방향을 회전시킨다.
        if (isCurve)
        {
            direction = RotateVector(
                direction,
                curveSpeed * Time.deltaTime
            );
        }

        // 계속 앞으로 이동
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        UpdateRotation();

        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            MissilePool.Instance.ReturnMissile(this);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        ).normalized;
    }

    private void UpdateRotation()
    {
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        if (collision.CompareTag("Player"))
        {
            hasHit = true;

            // 플레이어 데미지
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(10);
            }

            // 폭발 애니메이션 실행
            animator.SetTrigger("Explosion");
        }
    }

    // 폭발 애니메이션 마지막 프레임의 Animation Event에서 호출
    public void ReturnToPool()
    {
        MissilePool.Instance.ReturnMissile(this);
    }
}
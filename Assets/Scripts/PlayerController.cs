using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private Transform firePoint;

    [SerializeField] private float baseWalkSpeed = 2f;
    [SerializeField] private float baseRunSpeed = 4f;
    [SerializeField] private float speedPerLevel = 1f;

    [SerializeField] private float baseMaxHp = 100f;
    [SerializeField] private float hpPerLevel = 100f;

    private float currentWalkSpeed;
    private float currentRunSpeed;

    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isFacingRight = true;
    private bool isAttacking = false;
    private Vector2 attackDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (floorTilemap == null)
        {
            GameObject floorObj = GameObject.Find("Floor");
            if (floorObj != null)
                floorTilemap = floorObj.GetComponent<Tilemap>();

            if (floorTilemap == null)
                Debug.LogWarning("Floor Tilemap을 찾을 수 없습니다!");
        }

        // 게임 시작 시 저장 데이터 기반 스탯 적용
        ApplyUpgradeStats();
    }

    // 강화 단계 데이터를 읽어와서 이동 속도 재계산
    public void ApplyUpgradeStats()
    {
        if (DataManager.Instance != null && DataManager.Instance.currentData != null)
        {
            var data = DataManager.Instance.currentData;

            if (data.speedLevel >= 7)
            {
                // [Speed 만렙 특수효과]
                currentWalkSpeed = 30f;
                currentRunSpeed = 60f;
            }
            else
            {
                float additionalSpeed = data.speedLevel * speedPerLevel;
                currentWalkSpeed = baseWalkSpeed + additionalSpeed;
                currentRunSpeed = currentWalkSpeed * 2;
            }

            if (data.hpLevel >= 7)
            {
                // [HP 만렙 특수효과]
                MaxHp = 10000f;
                CurrentHp = MaxHp;

                float scaleX = Mathf.Abs(transform.localScale.x) * 2f;
                float scaleY = Mathf.Abs(transform.localScale.y) * 2f;

                transform.localScale = new Vector3(
                    isFacingRight ? scaleX : -scaleX,
                    scaleY,
                    transform.localScale.z
                );
            }
            else
            {
                MaxHp = baseMaxHp + (data.hpLevel * hpPerLevel);
                CurrentHp = MaxHp;
            }
            
        }
        else
        {
            currentWalkSpeed = baseWalkSpeed;
            currentRunSpeed = baseRunSpeed;
            MaxHp = baseMaxHp;
            CurrentHp = MaxHp;
        }
    }

    void Update()
    {
        if (isAttacking)
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }

        movement = new Vector2(moveX, moveY).normalized;

        bool isShiftPressed = Keyboard.current != null &&
                              (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        bool isMoving = movement.sqrMagnitude > 0.01f;

        // 강화 데이터가 계산된 속도 변수 사용
        float moveSpeed = isShiftPressed ? currentRunSpeed : currentWalkSpeed;

        if (moveX > 0f && !isFacingRight)
            Flip();
        else if (moveX < 0f && isFacingRight)
            Flip();

        if (animator != null)
        {
            bool isWalking = isMoving && !isShiftPressed;
            bool isRunning = isMoving && isShiftPressed;

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (animator != null && !isAttacking)
            {
                isAttacking = true;
                movement = Vector2.zero;
                animator.SetTrigger("Attack");

                Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                mouseWorldPosition.z = 0f;

                Vector3 originPosition = firePoint != null ? firePoint.position : transform.position;
                attackDirection = (mouseWorldPosition - originPosition).normalized;
            }
        }

        if (floorTilemap == null) return;

        // 강화 스탯이 반영된 moveSpeed 적용
        Vector2 nextPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        Vector3Int cellPos = floorTilemap.WorldToCell(nextPosition);

        if (floorTilemap.HasTile(cellPos))
        {
            rb.MovePosition(nextPosition);
        }
    }

    public void SpawnAttackEffect()
    {
        if (attackEffectPrefab == null)
        {
            Debug.LogWarning("공격 이펙트 프리팹이 할당되지 않았습니다!");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, Quaternion.identity);

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D effectRb = effect.GetComponent<Rigidbody2D>();
        if (effectRb != null)
        {
            effectRb.linearVelocity = attackDirection * projectileSpeed;
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}
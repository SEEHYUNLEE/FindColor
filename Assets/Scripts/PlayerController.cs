using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Tilemap floorTilemap;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            // A/D 또는 좌/우 화살표
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;

            // W/S 또는 상/하 화살표
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }

        movement = new Vector2(moveX, moveY).normalized;

        if (floorTilemap == null) return;

        Vector2 nextPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // 바닥 타일맵 위인지 검사
        Vector3Int cellPos = floorTilemap.WorldToCell(nextPosition);

        if (floorTilemap.HasTile(cellPos))
        {
            rb.MovePosition(nextPosition);
        }
    }
}
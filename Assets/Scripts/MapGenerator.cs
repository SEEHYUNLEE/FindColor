using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private float minimumDevideRate;
    [SerializeField] private float maximumDivideRate;
    [SerializeField] private int maximumDepth;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Tilemap wallBackTilemap;
    [SerializeField] private Tilemap wallFrontTilemap;
    [SerializeField] private Tilemap backgroundTilemap;

    [Header("Tiles")]
    [SerializeField] private Tile roomTile;
    [SerializeField] private Tile wallTile;
    [SerializeField] private Tile outTile;

    [Header("UI")]
    [SerializeField] private Button generateButton;

    [HideInInspector]
    public List<Vector3Int> roomCenters =
        new List<Vector3Int>();


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(CreateNewMap);
        }

        CreateNewMap();
    }


    // =========================================================
    // 새로운 맵 생성
    // =========================================================

    public void CreateNewMap()
    {
        if (tileMap != null)
            tileMap.ClearAllTiles();

        if (wallBackTilemap != null)
            wallBackTilemap.ClearAllTiles();

        if (wallFrontTilemap != null)
            wallFrontTilemap.ClearAllTiles();

        if (backgroundTilemap != null)
            backgroundTilemap.ClearAllTiles();

        roomCenters.Clear();

        FillBackground();

        Node root =
            new Node(
                new RectInt(
                    0,
                    0,
                    mapSize.x,
                    mapSize.y
                )
            );

        Divide(root, 0);
        GenerateRoom(root, 0);
        GenerateLoad(root, 0);
        FillWall();
    }


    // =========================================================
    // BSP 공간 분할
    // =========================================================

    private void Divide(Node tree, int depth)
    {
        if (depth == maximumDepth)
            return;

        int maxLength =
            Mathf.Max(
                tree.nodeRect.width,
                tree.nodeRect.height
            );

        int split =
            Mathf.RoundToInt(
                Random.Range(
                    maxLength * minimumDevideRate,
                    maxLength * maximumDivideRate
                )
            );

        split =
            Mathf.Clamp(
                split,
                1,
                maxLength - 1
            );


        // 가로가 더 길면 좌우 분할
        if (tree.nodeRect.width >= tree.nodeRect.height)
        {
            tree.leftNode =
                new Node(
                    new RectInt(
                        tree.nodeRect.x,
                        tree.nodeRect.y,
                        split,
                        tree.nodeRect.height
                    )
                );

            tree.rightNode =
                new Node(
                    new RectInt(
                        tree.nodeRect.x + split,
                        tree.nodeRect.y,
                        tree.nodeRect.width - split,
                        tree.nodeRect.height
                    )
                );
        }
        // 세로가 더 길면 상하 분할
        else
        {
            tree.leftNode =
                new Node(
                    new RectInt(
                        tree.nodeRect.x,
                        tree.nodeRect.y,
                        tree.nodeRect.width,
                        split
                    )
                );

            tree.rightNode =
                new Node(
                    new RectInt(
                        tree.nodeRect.x,
                        tree.nodeRect.y + split,
                        tree.nodeRect.width,
                        tree.nodeRect.height - split
                    )
                );
        }


        tree.leftNode.parNode = tree;
        tree.rightNode.parNode = tree;

        Divide(
            tree.leftNode,
            depth + 1
        );

        Divide(
            tree.rightNode,
            depth + 1
        );
    }


    // =========================================================
    // 방 생성
    // =========================================================

    private RectInt GenerateRoom(
        Node tree,
        int depth)
    {
        RectInt rect;

        if (depth == maximumDepth)
        {
            rect = tree.nodeRect;

            int width =
                Random.Range(
                    rect.width / 2,
                    rect.width - 1
                );

            int height =
                Random.Range(
                    rect.height / 2,
                    rect.height - 1
                );

            int x =
                rect.x +
                Random.Range(
                    1,
                    rect.width - width
                );

            int y =
                rect.y +
                Random.Range(
                    1,
                    rect.height - height
                );

            rect =
                new RectInt(
                    x,
                    y,
                    width,
                    height
                );

            FillRoom(rect);

            Vector2Int center =
                new Vector2Int(
                    rect.x + rect.width / 2,
                    rect.y + rect.height / 2
                );

            roomCenters.Add(
                new Vector3Int(
                    center.x - mapSize.x / 2,
                    center.y - mapSize.y / 2,
                    0
                )
            );

            tree.roomRect = rect;
        }
        else
        {
            tree.leftNode.roomRect =
                GenerateRoom(
                    tree.leftNode,
                    depth + 1
                );

            tree.rightNode.roomRect =
                GenerateRoom(
                    tree.rightNode,
                    depth + 1
                );

            rect =
                tree.leftNode.roomRect;

            tree.roomRect = rect;
        }

        return rect;
    }


    // =========================================================
    // 방 연결
    // =========================================================

    private void GenerateLoad(
        Node tree,
        int depth)
    {
        if (depth == maximumDepth)
            return;

        Vector2Int leftNodeCenter =
            tree.leftNode.center;

        Vector2Int rightNodeCenter =
            tree.rightNode.center;


        // 가로 통로
        for (
            int i =
                Mathf.Min(
                    leftNodeCenter.x,
                    rightNodeCenter.x
                );
            i <=
                Mathf.Max(
                    leftNodeCenter.x,
                    rightNodeCenter.x
                );
            i++
        )
        {
            SetRoomTile(
                new Vector3Int(
                    i - mapSize.x / 2,
                    leftNodeCenter.y - mapSize.y / 2,
                    0
                )
            );

            SetRoomTile(
                new Vector3Int(
                    i - mapSize.x / 2,
                    leftNodeCenter.y - mapSize.y / 2 + 1,
                    0
                )
            );
        }


        // 세로 통로
        for (
            int j =
                Mathf.Min(
                    leftNodeCenter.y,
                    rightNodeCenter.y
                );
            j <=
                Mathf.Max(
                    leftNodeCenter.y,
                    rightNodeCenter.y
                );
            j++
        )
        {
            SetRoomTile(
                new Vector3Int(
                    rightNodeCenter.x - mapSize.x / 2,
                    j - mapSize.y / 2,
                    0
                )
            );

            SetRoomTile(
                new Vector3Int(
                    rightNodeCenter.x - mapSize.x / 2 + 1,
                    j - mapSize.y / 2,
                    0
                )
            );
        }


        GenerateLoad(
            tree.leftNode,
            depth + 1
        );

        GenerateLoad(
            tree.rightNode,
            depth + 1
        );
    }


    // =========================================================
    // 방 바닥 생성
    // =========================================================

    private void FillRoom(RectInt rect)
    {
        for (
            int x = rect.x;
            x < rect.x + rect.width;
            x++
        )
        {
            for (
                int y = rect.y;
                y < rect.y + rect.height;
                y++
            )
            {
                SetRoomTile(
                    new Vector3Int(
                        x - mapSize.x / 2,
                        y - mapSize.y / 2,
                        0
                    )
                );
            }
        }
    }


    // =========================================================
    // Floor Tile 생성
    // =========================================================

    private void SetRoomTile(Vector3Int cellPos)
    {
        if (tileMap != null)
        {
            tileMap.SetTile(
                cellPos,
                roomTile
            );
        }
    }


    // =========================================================
    // 배경 생성
    // =========================================================

    private void FillBackground()
    {
        for (
            int x = -10;
            x < mapSize.x + 10;
            x++
        )
        {
            for (
                int y = -10;
                y < mapSize.y + 10;
                y++
            )
            {
                Vector3Int cellPos =
                    new Vector3Int(
                        x - mapSize.x / 2,
                        y - mapSize.y / 2,
                        0
                    );

                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(
                        cellPos,
                        outTile
                    );
                }
            }
        }
    }


    // =========================================================
    // 벽 생성
    //
    // Floor 기준
    //
    // 오른쪽 → Back
    // 위쪽   → Back
    //
    // 왼쪽   → Front
    // 아래쪽 → Front
    // =========================================================

    private void FillWall()
    {
        HashSet<Vector3Int> processedBackWalls =
            new HashSet<Vector3Int>();

        HashSet<Vector3Int> processedFrontWalls =
            new HashSet<Vector3Int>();


        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3Int floorPos =
                    new Vector3Int(
                        x - mapSize.x / 2,
                        y - mapSize.y / 2,
                        0
                    );

                if (!IsFloor(floorPos))
                    continue;


                // 오른쪽 → Back
                Vector3Int rightPos =
                    floorPos + Vector3Int.right;

                if (!IsFloor(rightPos))
                {
                    if (IsInsideMap(rightPos) &&
                        processedBackWalls.Add(rightPos))
                    {
                        SetBackWall(rightPos);
                    }
                }


                // 왼쪽 → Front
                Vector3Int leftPos =
                    floorPos + Vector3Int.left;

                if (!IsFloor(leftPos))
                {
                    if (IsInsideMap(leftPos) &&
                        processedFrontWalls.Add(leftPos))
                    {
                        SetFrontWall(leftPos);
                    }
                }


                // 위쪽 → Back
                Vector3Int upPos =
                    floorPos + Vector3Int.up;

                if (!IsFloor(upPos))
                {
                    if (IsInsideMap(upPos) &&
                        processedBackWalls.Add(upPos))
                    {
                        SetBackWall(upPos);
                    }
                }


                // 아래쪽 → Front
                Vector3Int downPos =
                    floorPos + Vector3Int.down;

                if (!IsFloor(downPos))
                {
                    if (IsInsideMap(downPos) &&
                        processedFrontWalls.Add(downPos))
                    {
                        SetFrontWall(downPos);
                    }
                }
            }
        }
    }


    // =========================================================
    // Floor인지 확인
    // =========================================================

    private bool IsFloor(Vector3Int cellPos)
    {
        if (tileMap == null)
            return false;

        return tileMap.GetTile(cellPos) == roomTile;
    }


    // =========================================================
    // 맵 범위 확인
    // =========================================================

    private bool IsInsideMap(Vector3Int cellPos)
    {
        int minX =
            -mapSize.x / 2;

        int maxX =
            minX + mapSize.x - 1;

        int minY =
            -mapSize.y / 2;

        int maxY =
            minY + mapSize.y - 1;

        return
            cellPos.x >= minX &&
            cellPos.x <= maxX &&
            cellPos.y >= minY &&
            cellPos.y <= maxY;
    }


    // =========================================================
    // Back Wall
    // =========================================================

    private void SetBackWall(Vector3Int cellPos)
    {
        if (wallFrontTilemap != null)
        {
            wallFrontTilemap.SetTile(
                cellPos,
                null
            );
        }

        if (wallBackTilemap != null)
        {
            wallBackTilemap.SetTile(
                cellPos,
                wallTile
            );
        }
    }


    // =========================================================
    // Front Wall
    // =========================================================

    private void SetFrontWall(Vector3Int cellPos)
    {
        if (wallBackTilemap != null)
        {
            wallBackTilemap.SetTile(
                cellPos,
                null
            );
        }

        if (wallFrontTilemap != null)
        {
            wallFrontTilemap.SetTile(
                cellPos,
                wallTile
            );
        }
    }
}
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

    // =========================================================
    // 추가된 엔티티 생성 관련 설정
    // =========================================================
    [Header("Spawning")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject slimePrefab;
    //[SerializeField] private int slimeCount = 3; // 생성할 슬라임 개수

    private List<GameObject> spawnedObjects = new List<GameObject>();


    [HideInInspector]
    public List<Vector3Int> roomCenters = new List<Vector3Int>();


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

        // 기존에 소환된 플레이어/슬라임 제거
        ClearSpawnedEntities();

        roomCenters.Clear();

        FillBackground();

        Node root = new Node(new RectInt(0, 0, mapSize.x, mapSize.y));

        Divide(root, 0);
        GenerateRoom(root, 0);
        GenerateLoad(root, 0);
        FillWall();

        // 맵 생성이 완료된 후 플레이어와 슬라임 생성
        SpawnEntities();
    }


    // =========================================================
    // 엔티티(플레이어, 슬라임) 생성
    // =========================================================

    private void SpawnEntities()
    {
        if (roomCenters.Count == 0) return;

        // roomCenters 복사본 생성 후 무작위 셔플
        List<Vector3Int> availableCenters = new List<Vector3Int>(roomCenters);
        ShuffleList(availableCenters);

        // 1. 플레이어 생성 (첫 번째 무작위 방)
        if (playerPrefab != null && availableCenters.Count > 0)
        {
            Vector3 spawnPos = tileMap.CellToWorld(availableCenters[0]) + new Vector3(0.5f, 0.5f, 0f);
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            spawnedObjects.Add(player);

            // 플레이어가 생성된 방은 목록에서 제거
            availableCenters.RemoveAt(0);
        }

        // 2. 슬라임 생성 (남은 방 중 무작위 1개 방에만 1마리 생성)
        if (slimePrefab != null && availableCenters.Count > 0)
        {
            Vector3 spawnPos = tileMap.CellToWorld(availableCenters[0]) + new Vector3(0.5f, 0.5f, 0f);
            GameObject slime = Instantiate(slimePrefab, spawnPos, Quaternion.identity);
            spawnedObjects.Add(slime);
        }
    }

    // 기존 생성물 삭제
    private void ClearSpawnedEntities()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    // 리스트 무작위 셔플 (Fisher-Yates 알고리즘)
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }


    // =========================================================
    // BSP 공간 분할
    // =========================================================

    private void Divide(Node tree, int depth)
    {
        if (depth == maximumDepth)
            return;

        int maxLength = Mathf.Max(tree.nodeRect.width, tree.nodeRect.height);

        int split = Mathf.RoundToInt(Random.Range(maxLength * minimumDevideRate, maxLength * maximumDivideRate));
        split = Mathf.Clamp(split, 1, maxLength - 1);

        if (tree.nodeRect.width >= tree.nodeRect.height)
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, split, tree.nodeRect.height));
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x + split, tree.nodeRect.y, tree.nodeRect.width - split, tree.nodeRect.height));
        }
        else
        {
            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, tree.nodeRect.width, split));
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y + split, tree.nodeRect.width, tree.nodeRect.height - split));
        }

        tree.leftNode.parNode = tree;
        tree.rightNode.parNode = tree;

        Divide(tree.leftNode, depth + 1);
        Divide(tree.rightNode, depth + 1);
    }


    // =========================================================
    // 방 생성
    // =========================================================

    private RectInt GenerateRoom(Node tree, int depth)
    {
        RectInt rect;

        if (depth == maximumDepth)
        {
            rect = tree.nodeRect;

            int width = Random.Range(rect.width / 2, rect.width - 1);
            int height = Random.Range(rect.height / 2, rect.height - 1);

            int x = rect.x + Random.Range(1, rect.width - width);
            int y = rect.y + Random.Range(1, rect.height - height);

            rect = new RectInt(x, y, width, height);

            FillRoom(rect);

            Vector2Int center = new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

            roomCenters.Add(new Vector3Int(center.x - mapSize.x / 2, center.y - mapSize.y / 2, 0));

            tree.roomRect = rect;
        }
        else
        {
            tree.leftNode.roomRect = GenerateRoom(tree.leftNode, depth + 1);
            tree.rightNode.roomRect = GenerateRoom(tree.rightNode, depth + 1);

            rect = tree.leftNode.roomRect;
            tree.roomRect = rect;
        }

        return rect;
    }


    // =========================================================
    // 방 연결
    // =========================================================

    private void GenerateLoad(Node tree, int depth)
    {
        if (depth == maximumDepth)
            return;

        Vector2Int leftNodeCenter = tree.leftNode.center;
        Vector2Int rightNodeCenter = tree.rightNode.center;

        for (int i = Mathf.Min(leftNodeCenter.x, rightNodeCenter.x); i <= Mathf.Max(leftNodeCenter.x, rightNodeCenter.x); i++)
        {
            SetRoomTile(new Vector3Int(i - mapSize.x / 2, leftNodeCenter.y - mapSize.y / 2, 0));
            SetRoomTile(new Vector3Int(i - mapSize.x / 2, leftNodeCenter.y - mapSize.y / 2 + 1, 0));
        }

        for (int j = Mathf.Min(leftNodeCenter.y, rightNodeCenter.y); j <= Mathf.Max(leftNodeCenter.y, rightNodeCenter.y); j++)
        {
            SetRoomTile(new Vector3Int(rightNodeCenter.x - mapSize.x / 2, j - mapSize.y / 2, 0));
            SetRoomTile(new Vector3Int(rightNodeCenter.x - mapSize.x / 2 + 1, j - mapSize.y / 2, 0));
        }

        GenerateLoad(tree.leftNode, depth + 1);
        GenerateLoad(tree.rightNode, depth + 1);
    }


    // =========================================================
    // 방 바닥 생성
    // =========================================================

    private void FillRoom(RectInt rect)
    {
        for (int x = rect.x; x < rect.x + rect.width; x++)
        {
            for (int y = rect.y; y < rect.y + rect.height; y++)
            {
                SetRoomTile(new Vector3Int(x - mapSize.x / 2, y - mapSize.y / 2, 0));
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
            tileMap.SetTile(cellPos, roomTile);
        }
    }


    // =========================================================
    // 배경 생성
    // =========================================================

    private void FillBackground()
    {
        for (int x = -10; x < mapSize.x + 10; x++)
        {
            for (int y = -10; y < mapSize.y + 10; y++)
            {
                Vector3Int cellPos = new Vector3Int(x - mapSize.x / 2, y - mapSize.y / 2, 0);

                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(cellPos, outTile);
                }
            }
        }
    }


    // =========================================================
    // 벽 생성
    // =========================================================

    private void FillWall()
    {
        HashSet<Vector3Int> processedBackWalls = new HashSet<Vector3Int>();
        HashSet<Vector3Int> processedFrontWalls = new HashSet<Vector3Int>();

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3Int floorPos = new Vector3Int(x - mapSize.x / 2, y - mapSize.y / 2, 0);

                if (!IsFloor(floorPos))
                    continue;

                Vector3Int rightPos = floorPos + Vector3Int.right;
                if (!IsFloor(rightPos) && IsInsideMap(rightPos) && processedBackWalls.Add(rightPos))
                {
                    SetBackWall(rightPos);
                }

                Vector3Int leftPos = floorPos + Vector3Int.left;
                if (!IsFloor(leftPos) && IsInsideMap(leftPos) && processedFrontWalls.Add(leftPos))
                {
                    SetFrontWall(leftPos);
                }

                Vector3Int upPos = floorPos + Vector3Int.up;
                if (!IsFloor(upPos) && IsInsideMap(upPos) && processedBackWalls.Add(upPos))
                {
                    SetBackWall(upPos);
                }

                Vector3Int downPos = floorPos + Vector3Int.down;
                if (!IsFloor(downPos) && IsInsideMap(downPos) && processedFrontWalls.Add(downPos))
                {
                    SetFrontWall(downPos);
                }
            }
        }
    }


    private bool IsFloor(Vector3Int cellPos)
    {
        if (tileMap == null)
            return false;

        return tileMap.GetTile(cellPos) == roomTile;
    }


    private bool IsInsideMap(Vector3Int cellPos)
    {
        int minX = -mapSize.x / 2;
        int maxX = minX + mapSize.x - 1;
        int minY = -mapSize.y / 2;
        int maxY = minY + mapSize.y - 1;

        return cellPos.x >= minX && cellPos.x <= maxX && cellPos.y >= minY && cellPos.y <= maxY;
    }


    private void SetBackWall(Vector3Int cellPos)
    {
        if (wallFrontTilemap != null)
            wallFrontTilemap.SetTile(cellPos, null);

        if (wallBackTilemap != null)
            wallBackTilemap.SetTile(cellPos, wallTile);
    }


    private void SetFrontWall(Vector3Int cellPos)
    {
        if (wallBackTilemap != null)
            wallBackTilemap.SetTile(cellPos, null);

        if (wallFrontTilemap != null)
            wallFrontTilemap.SetTile(cellPos, wallTile);
    }
}
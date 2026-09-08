using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Tilemap floorTilemap;
    public Tilemap wallBackTilemap;
    public Tilemap wallFrontTilemap;

    [Header("Gizmo")]
    public float gizmoLineWidth = 0.02f;

    private Node[,] grid;
    private int gridSizeX;
    private int gridSizeY;
    private BoundsInt bounds;

    public bool isGridCreated { get; private set; }

    // PathfindingTest에서 사용하는 현재 경로
    public List<Node> path = new List<Node>();


    // =========================================================
    // Start & 자동 오브젝트 검색
    // =========================================================

    private void Start()
    {
        // 이름으로 타일맵 및 플레이어 자동 할당
        FindAndAssignTilemaps();

        if (floorTilemap == null)
        {
            Debug.LogError("Grid : floorTilemap을 찾을 수 없습니다.");
            return;
        }

        CreateGrid();
    }

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void FindAndAssignTilemaps()
    {
        if (floorTilemap == null)
        {
            GameObject floorObj = GameObject.Find("Floor");
            if (floorObj != null) floorTilemap = floorObj.GetComponent<Tilemap>();
        }

        if (wallBackTilemap == null)
        {
            GameObject wallBackObj = GameObject.Find("WallBack");
            if (wallBackObj != null) wallBackTilemap = wallBackObj.GetComponent<Tilemap>();
        }

        if (wallFrontTilemap == null)
        {
            GameObject wallFrontObj = GameObject.Find("WallFront");
            if (wallFrontObj != null) wallFrontTilemap = wallFrontObj.GetComponent<Tilemap>();
        }
    }


    // =========================================================
    // Grid 생성
    // =========================================================

    public void CreateGrid()
    {
        isGridCreated = false;

        // -----------------------------------------------------
        // Floor Tilemap을 Grid의 기준으로 사용
        // -----------------------------------------------------
        floorTilemap.CompressBounds();
        bounds = floorTilemap.cellBounds;

        gridSizeX = bounds.size.x;
        gridSizeY = bounds.size.y;

        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            Debug.LogError("Grid : Floor Tilemap에 타일이 없습니다.");
            return;
        }

        grid = new Node[gridSizeX, gridSizeY];


        // -----------------------------------------------------
        // Node 생성
        // -----------------------------------------------------
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int cellPos = new Vector3Int(
                    bounds.xMin + x,
                    bounds.yMin + y,
                    0
                );


                // 1. Floor가 존재해야 기본적으로 이동 가능
                bool walkable = floorTilemap.HasTile(cellPos);


                // 2. WallBack 또는 WallFront 타일이 하나라도 존재하면 이동 불가능 처리
                if (wallBackTilemap != null && wallBackTilemap.HasTile(cellPos))
                {
                    walkable = false;
                }

                if (wallFrontTilemap != null && wallFrontTilemap.HasTile(cellPos))
                {
                    walkable = false;
                }


                // Floor Tilemap의 실제 Cell 중앙 월드 좌표
                Vector3 nodePosition = floorTilemap.GetCellCenterWorld(cellPos);


                grid[x, y] = new Node(
                    walkable,
                    nodePosition,
                    x,
                    y
                );
            }
        }


        isGridCreated = true;

        Debug.Log(
            $"Grid 생성 완료 : {gridSizeX} x {gridSizeY}"
        );
    }


    // =========================================================
    // 이웃 Node
    // =========================================================

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();


        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;


                int checkX = node.gridX + x;
                int checkY = node.gridY + y;


                // Grid 범위 체크
                if (checkX < 0 ||
                    checkX >= gridSizeX ||
                    checkY < 0 ||
                    checkY >= gridSizeY)
                {
                    continue;
                }


                Node targetNode = grid[checkX, checkY];


                // 이동 불가능한 칸
                if (!targetNode.isWalkable)
                    continue;


                // -------------------------------------------------
                // 대각선 이동
                // -------------------------------------------------
                if (x != 0 && y != 0)
                {
                    Node horizontalNode = grid[node.gridX + x, node.gridY];
                    Node verticalNode = grid[node.gridX, node.gridY + y];


                    // 벽 사이 대각선 통과 방지
                    if (!horizontalNode.isWalkable || !verticalNode.isWalkable)
                    {
                        continue;
                    }
                }


                neighbours.Add(targetNode);
            }
        }


        return neighbours;
    }


    // =========================================================
    // 월드 좌표 → Node
    // =========================================================

    public Node GetNodeFromPosition(Vector3 worldPosition)
    {
        if (grid == null || floorTilemap == null)
            return null;

        Vector3Int cellPosition = floorTilemap.WorldToCell(worldPosition);


        int x = cellPosition.x - bounds.xMin;
        int y = cellPosition.y - bounds.yMin;


        if (x < 0 ||
            x >= gridSizeX ||
            y < 0 ||
            y >= gridSizeY)
        {
            return null;
        }


        return grid[x, y];
    }


    // =========================================================
    // Player 설정
    // =========================================================

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }


    // =========================================================
    // 전체 Node 가져오기
    // =========================================================

    public IEnumerable<Node> GetAllNodes()
    {
        if (grid == null)
            yield break;


        foreach (Node node in grid)
        {
            yield return node;
        }
    }


    // =========================================================
    // 실제 Floor Tilemap의 Cell 4개 꼭짓점
    // =========================================================

    private void GetCellCorners(
        Vector3Int cellPosition,
        out Vector3 bottomLeft,
        out Vector3 bottomRight,
        out Vector3 topRight,
        out Vector3 topLeft)
    {
        bottomLeft = floorTilemap.CellToWorld(cellPosition);
        bottomRight = floorTilemap.CellToWorld(cellPosition + new Vector3Int(1, 0, 0));
        topRight = floorTilemap.CellToWorld(cellPosition + new Vector3Int(1, 1, 0));
        topLeft = floorTilemap.CellToWorld(cellPosition + new Vector3Int(0, 1, 0));
    }


    // =========================================================
    // Gizmo
    // =========================================================

    private void OnDrawGizmos()
    {
        if (floorTilemap == null || grid == null)
            return;

        Node playerNode = null;

        if (player != null)
        {
            playerNode = GetNodeFromPosition(player.position);
        }

        foreach (Node node in grid)
        {
            if (node == null)
                continue;

            Vector3Int cellPosition = new Vector3Int(
                bounds.xMin + node.gridX,
                bounds.yMin + node.gridY,
                0
            );

            Vector3 center = floorTilemap.GetCellCenterWorld(cellPosition);

            // 색상 지정
            if (!node.isWalkable)
            {
                Gizmos.color = Color.red; // 장애물/벽
            }
            else
            {
                Gizmos.color = Color.white; // 이동 가능
            }

            // Player 위치
            if (playerNode == node)
            {
                Gizmos.color = Color.green;
            }
            // 현재 Path
            else if (path != null && path.Contains(node))
            {
                Gizmos.color = Color.blue;
            }

            float pointSize = 0.1f;
            Gizmos.DrawSphere(center, pointSize);
        }
    }
}
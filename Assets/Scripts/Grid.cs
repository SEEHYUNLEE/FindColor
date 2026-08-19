using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour
{
    public Transform player;
    public Tilemap floorTilemap;
    public Tilemap WallTilemap;
    public Tilemap obstacleTilemap;
    public float nodeSize = 1f;

    Node[,] grid;
    int gridSizeX, gridSizeY;
    BoundsInt bounds;

    // 타일맵을 받아와서 그리드를 생성하므로 Awake 대신 Start 사용
    void Start()
    {
        if (WallTilemap == null)
        {
            return;
        }

        // 바닥 타일맵의 그려진 영역 경계를 기준으로 그리드 크기 자동 설정
        WallTilemap.CompressBounds();
        bounds = WallTilemap.cellBounds;

        gridSizeX = bounds.size.x;
        gridSizeY = bounds.size.y;

        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            return;
        }

        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY]; // 배열 크기 지정

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // 경계선 내부 기준에서 타일 위치
                Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                // 바닥 있는지 확인
                bool walkable = floorTilemap.HasTile(cellPos);

                // 장애물 확인
                if (obstacleTilemap != null)
                {
                    if (obstacleTilemap.HasTile(cellPos))
                    {
                        walkable = false;
                    }
                }

                // 노드 객체 생성 및 배열에 저장
                Vector3 nodePosition = floorTilemap.GetCellCenterWorld(cellPos); // 타일 중앙으로 위치 설정
                grid[x, y] = new Node(walkable, nodePosition, x, y);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // 맵 범위를 벗어나지 않는지 먼저 확인
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    Node targetNode = grid[checkX, checkY];

                    // 기본적으로 갈 수 있는 노드여야 함
                    if (!targetNode.isWalkable) continue;

                    // 대각선 이동 제어
                    if (x != 0 && y != 0)
                    {
                        // 이동할 칸 주변이 벽이나 장애물일 때 대각선 이동 금지
                        Node horizontalNeighbor = grid[node.gridX + x, node.gridY];
                        Node verticalNeighbor = grid[node.gridX, node.gridY + y];

                        if (!horizontalNeighbor.isWalkable || !verticalNeighbor.isWalkable)
                        {
                            continue;
                        }
                    }

                    neighbours.Add(targetNode);
                }
            }
        }

        return neighbours;
    }

    public Node GetNodeFromPosition(Vector3 position)
    {
        // 월드 좌표를 타일맵 칸 번호로 변환
        Vector3Int cellPos = floorTilemap.WorldToCell(position);

        // 경계선으로부터 시작점을 잡았기 때문에 빼주기
        int x = cellPos.x - bounds.xMin;
        int y = cellPos.y - bounds.yMin;

        // 범위 초과 방지 : Mathf.Clamp(값, 최소값, 최대값)
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
        {
            return null; // 범위를 벗어나면 크래시 대신 null 반환
        }

        return grid[x, y];
    }

    // 찾은 경로 저장할 리스트
    public List<Node> path;
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    void OnDrawGizmos()
    {
        // 경계선 그리기
        if (floorTilemap != null)
        {
            Bounds mapBounds = floorTilemap.localBounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(floorTilemap.transform.TransformPoint(mapBounds.center), mapBounds.size);
        }

        if (grid != null)
        {
            Node playernode = null;
            if (player != null)
            {
                playernode = GetNodeFromPosition(player.position);
            }

            foreach (Node n in grid)
            {
                Gizmos.color = (n.isWalkable) ? Color.white : Color.red;

                if (playernode == n)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    if (path != null && path.Contains(n))
                        Gizmos.color = Color.blue;
                }

                Gizmos.DrawCube(n.position, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }
}
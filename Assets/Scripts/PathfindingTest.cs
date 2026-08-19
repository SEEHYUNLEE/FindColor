using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;     // 플레이어 프리팹
    [SerializeField] private GameObject destinationPrefab;// 도착지 프리팹
    private GameObject currentPlayer;
    private GameObject currentDestination;

    private Transform start, destination;
    public MapGenerator mapGenerator; // 맵 제너레이터 연결
    public Tilemap floorTilemap;      // 월드 좌표 변환용 타일맵

    private Vector3 cacheStart, cacheDest;
    private Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    void Start()
    {
        // 게임 시작 시 최초 맵 생성 및 배치
        GenerateAndSetup();
    }

    void Update()
    {
        // 테스트용: 스페이스바를 누를 때마다 새로운 맵 생성 후 랜덤 배치 및 길찾기
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GenerateAndSetup();
        }

        if (start == null || destination == null) return;

        if (start.position != cacheStart || destination.position != cacheDest)
        {
            FindPath(start.position, destination.position);

            cacheStart = start.position;
            cacheDest = destination.position;
        }
    }

    // 맵을 새로 만들고 플레이어와 도착지를 랜덤 방에 배치하는 함수
    public void GenerateAndSetup()
    {
        if (mapGenerator == null)
        {
            return;
        }

        if (floorTilemap == null)
        {
            return;
        }

        if (mapGenerator != null)
        {
            mapGenerator.CreateNewMap(); // 1. 새로운 맵 생성
        }

        // 맵이 바뀐 후, Grid 데이터 새로 갱신
        if (grid != null)
        {
            grid.CreateGrid(); 
        }

        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentDestination != null) Destroy(currentDestination);

        if (mapGenerator != null && mapGenerator.roomCenters.Count >= 2 && floorTilemap != null)
        {
            List<Vector3Int> availableRooms = new List<Vector3Int>(mapGenerator.roomCenters);

            int index1 = Random.Range(0, availableRooms.Count);
            Vector3Int startCell = availableRooms[index1];
            availableRooms.RemoveAt(index1);

            int index2 = Random.Range(0, availableRooms.Count);
            Vector3Int destCell = availableRooms[index2];

            // 소환 위치를 미리 계산
            Vector3 startPos = floorTilemap.GetCellCenterWorld(startCell);
            Vector3 destPos = floorTilemap.GetCellCenterWorld(destCell);

            // 프리팹 먼저 소환 후, 그 Transform을 start/destination 변수에 대입
            if (playerPrefab != null)
            {
                currentPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
                start = currentPlayer.transform; // 여기서 할당됨
                grid.SetPlayer(start);
            }

            if (destinationPrefab != null)
            {
                currentDestination = Instantiate(destinationPrefab, destPos, Quaternion.identity);
                destination = currentDestination.transform; // 여기서 할당됨
            }
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // grid가 비어있는지 체크
        if (grid == null)
        {
            grid = GetComponent<Grid>();
            if (grid == null) return;
        }

        Node startNode = grid.GetNodeFromPosition(startPos);
        Node targetNode = grid.GetNodeFromPosition(targetPos);

        if (startNode == null || targetNode == null)
        {
            return;
        }

        MinPriorityQueue<Node> openSet = new MinPriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Enqueue(startNode);
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            closedSet.Add(currentNode);

            foreach (Node n in grid.GetNeighbours(currentNode))
            {
                if (n == null || !n.isWalkable || closedSet.Contains(n))
                {
                    continue;
                }

                int g = currentNode.gCost + GetDistance(currentNode, n);
                int h = GetDistance(n, targetNode);
                int f = g + h;

                if (!openSet.Contains(n))
                {
                    n.gCost = g;
                    n.hCost = h;
                    n.parent = currentNode;
                    openSet.Enqueue(n);
                }
                else
                {
                    if (n.fCost > f)
                    {
                        n.gCost = g;
                        n.parent = currentNode;
                        openSet.Reposition(n);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }
}
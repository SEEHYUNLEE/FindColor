using System.Collections.Generic;
using UnityEngine;

public class SlimePathfinding : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float reachThreshold = 0.05f;

    // 플레이어가 이 거리 안으로 들어오면 도망 시작
    public float dangerDistance = 1.5f;

    private Grid grid;

    private bool isInitialized;
    private bool isMoving;

    private bool wasInDanger;

    private int targetIndex;

    private Node currentDestination;


    void Awake()
    {
        grid = GetComponent<Grid>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }


    void Update()
    {
        if (grid == null || player == null)
            return;

        if (!grid.isGridCreated)
            return;

        if (!isInitialized)
        {
            isInitialized = true;

            wasInDanger = false;

            return;
        }

        // 플레이어와의 거리
        float distance = Vector3.Distance(transform.position, player.position);
        bool inDanger = distance <= dangerDistance;

        if (inDanger && !wasInDanger)
        {
            StartEscape();
        }
        wasInDanger = inDanger;

        if (isMoving)
        {
            MoveAlongPath();
        }
        else if (inDanger)
        {
            StartEscape();
        }
    }

    public void StartEscape()
    {
        Node destination = FindEscapeDestination();

        if (destination == null)
        {
            isMoving = false;
            return;
        }

        currentDestination = destination;
        bool success = FindPath(transform.position, destination);
        isMoving = success;
    }

    // 다익스트라 알고리즘으로 플레이어로부터 가장 먼 곳 확인
    Node FindEscapeDestination()
    {
        Node playerNode = grid.GetNodeFromPosition(player.position);

        if (playerNode == null)
            return null;

        // 1. 모든 Node의 경로 비용 초기화
        foreach (Node node in grid.GetAllNodes())
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }

        // 2. 플레이어 기준 다익스트라 실행
        MinPriorityQueue<Node> openSet = new MinPriorityQueue<Node>();

        playerNode.gCost = 0;
        openSet.Enqueue(playerNode);

        while (openSet.Count > 0)
        {
            Node current = openSet.Dequeue();

            foreach (Node neighbor in grid.GetNeighbours(current))
            {
                if (!neighbor.isWalkable)
                    continue;

                int newCost = current.gCost + GetDistance(current, neighbor);

                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.parent = current;

                    openSet.Enqueue(neighbor);
                }
            }
        }

        // 3. 플레이어로부터 가장 먼 Node 선택
        Node bestNode = null;
        int maxDistance = -1;

        foreach (Node node in grid.GetAllNodes())
        {
            if (!node.isWalkable)
                continue;

            // 플레이어에게 도달할 수 없는 Node 제외
            if (node.gCost == int.MaxValue)
                continue;

            // 플레이어가 있는 Node는 목적지에서 제외
            if (node == playerNode)
                continue;

            if (node.gCost > maxDistance)
            {
                maxDistance = node.gCost;
                bestNode = node;
            }
        }

        return bestNode;
    }

    // A* 알고리즘으로 경로 확인
    bool FindPath(Vector3 startPos, Node targetNode)
    {
        Node startNode = grid.GetNodeFromPosition(startPos);

        if (startNode == null || targetNode == null)
        {
            return false;
        }

        if (startNode == targetNode)
        {
            grid.path.Clear();

            targetIndex = 0;

            return false;
        }

        foreach (Node node in grid.GetAllNodes())
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }

        MinPriorityQueue<Node> openSet = new MinPriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;

        startNode.hCost = GetDistance(startNode, targetNode);

        openSet.Enqueue(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet.Dequeue();

            if (current == targetNode)
            {
                RetracePath(startNode, targetNode);

                return true;
            }

            closedSet.Add(current);

            foreach (Node neighbor in grid.GetNeighbours(current))
            {
                if (!neighbor.isWalkable)
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                int newGCost = current.gCost + GetDistance(current, neighbor);

                if (!openSet.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;
                    openSet.Enqueue(neighbor);
                }
                else if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;
                    openSet.Reposition(neighbor);
                }
            }
        }
        grid.path.Clear();
        targetIndex = 0;
        return false;
    }

    // 경로 생성
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> newPath = new List<Node>();
        Node current = endNode;

        while (current != startNode && current != null)
        {
            newPath.Add(current);
            current = current.parent;
        }
        newPath.Reverse();
        grid.path = newPath;
        targetIndex = 0;
    }

    // 이동
    void MoveAlongPath()
    {
        if (grid.path == null || grid.path.Count == 0)
        {
            isMoving = false;
            return;
        }

        if (targetIndex >= grid.path.Count)
        {
            isMoving = false;
            return;
        }

        Node targetNode = grid.path[targetIndex];
        Vector3 targetPos = targetNode.position;
        targetPos.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position,targetPos) <= reachThreshold)
        {
            targetIndex++;
            if (targetIndex >= grid.path.Count)
            {
                isMoving = false;
            }
        }
    }

    // 이동 비용
    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        if (dx > dy)
        {
            return 14 * dy + 10 * (dx - dy);
        }
        return 14 * dx + 10 * (dy - dx);
    }

    void OnDrawGizmos()
    {
        if (currentDestination != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentDestination.position,0.4f);
        }
    }
}
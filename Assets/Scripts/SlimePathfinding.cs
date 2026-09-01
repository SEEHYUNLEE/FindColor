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

    [Header("Escape Region")]
    [Range(0f, 1f)]
    public float oppositeDirectionWeight = 0.7f;

    // 플레이어로부터 최소 몇 % 이상 먼 지역만 후보로 사용할지
    [Range(0f, 1f)]
    public float minimumFarDistanceRatio = 0.65f;

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
            GameObject playerObject =
                GameObject.FindWithTag("Player");

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


        // =====================================================
        // 최초 초기화
        //
        // 게임 시작 시에는 절대로 도망가지 않음
        // =====================================================

        if (!isInitialized)
        {
            isInitialized = true;

            wasInDanger = false;

            return;
        }


        // =====================================================
        // 플레이어와의 거리
        // =====================================================

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );


        bool inDanger =
            distance <= dangerDistance;


        // =====================================================
        // 플레이어가 처음 dangerDistance 안으로 들어왔을 때
        // =====================================================

        if (inDanger && !wasInDanger)
        {
            StartEscape();
        }


        wasInDanger = inDanger;


        // =====================================================
        // 이동 중
        // =====================================================

        if (isMoving)
        {
            MoveAlongPath();
        }


        // =====================================================
        // 이동이 끝났는데 플레이어가 아직 가까이 있다면
        // 다시 도망
        // =====================================================

        else if (inDanger)
        {
            StartEscape();
        }
    }


    // =========================================================
    // 도망 시작
    // =========================================================

    public void StartEscape()
    {
        Node destination =
            FindEscapeDestination();


        if (destination == null)
        {
            isMoving = false;
            return;
        }


        currentDestination =
            destination;


        bool success =
            FindPath(
                transform.position,
                destination
            );


        isMoving =
            success;
    }


    // =========================================================
    // 도망 목적지 탐색
    //
    // 1. 플레이어 기준 다익스트라
    // 2. 플레이어로부터 충분히 먼 지역
    // 3. 플레이어 → 적 방향 우선
    // 4. 적으로부터도 먼 위치 우선
    // =========================================================

    Node FindEscapeDestination()
    {
        Node playerNode =
            grid.GetNodeFromPosition(
                player.position
            );


        Node enemyNode =
            grid.GetNodeFromPosition(
                transform.position
            );


        if (playerNode == null ||
            enemyNode == null)
        {
            return null;
        }


        // =====================================================
        // 1. 플레이어 기준 다익스트라
        // =====================================================

        foreach (Node node in grid.GetAllNodes())
        {
            node.gCost = int.MaxValue;
            node.parent = null;
        }


        MinPriorityQueue<Node> openSet =
            new MinPriorityQueue<Node>();


        playerNode.gCost = 0;

        openSet.Enqueue(playerNode);


        while (openSet.Count > 0)
        {
            Node current =
                openSet.Dequeue();


            foreach (Node neighbor
                     in grid.GetNeighbours(current))
            {
                if (!neighbor.isWalkable)
                    continue;


                int newCost =
                    current.gCost +
                    GetDistance(
                        current,
                        neighbor
                    );


                if (newCost < neighbor.gCost)
                {
                    neighbor.gCost = newCost;
                    neighbor.parent = current;

                    openSet.Enqueue(neighbor);
                }
            }
        }


        // =====================================================
        // 2. 플레이어 기준 가장 먼 거리
        // =====================================================

        int maxPlayerDistance = 0;


        foreach (Node node in grid.GetAllNodes())
        {
            if (!node.isWalkable)
                continue;


            if (node.gCost == int.MaxValue)
                continue;


            if (node.gCost > maxPlayerDistance)
            {
                maxPlayerDistance =
                    node.gCost;
            }
        }


        if (maxPlayerDistance <= 0)
            return null;


        int minimumDistance =
            Mathf.RoundToInt(
                maxPlayerDistance *
                minimumFarDistanceRatio
            );


        // =====================================================
        // 3. 플레이어 → 적 방향
        // =====================================================

        Vector2 playerToEnemy =
            enemyNode.position -
            playerNode.position;


        if (playerToEnemy.sqrMagnitude <= 0.01f)
        {
            playerToEnemy =
                Vector2.right;
        }


        playerToEnemy.Normalize();


        // =====================================================
        // 4. 후보 탐색
        // =====================================================

        Node bestNode = null;

        float bestScore =
            float.MinValue;


        foreach (Node node in grid.GetAllNodes())
        {
            if (!node.isWalkable)
                continue;


            if (node.gCost == int.MaxValue)
                continue;


            // 플레이어에게서 충분히 먼 곳만 후보
            if (node.gCost < minimumDistance)
                continue;


            // 직전 목적지와 같은 곳은 제외
            if (node == currentDestination)
                continue;


            Vector2 playerToNode =
                node.position -
                playerNode.position;


            if (playerToNode.sqrMagnitude <= 0.01f)
                continue;


            playerToNode.Normalize();


            // =================================================
            // 방향 비교
            // =================================================

            float directionScore =
                Vector2.Dot(
                    playerToEnemy,
                    playerToNode
                );


            float oppositeScore =
                (directionScore + 1f) * 0.5f;


            // =================================================
            // 적으로부터 목적지까지 거리
            // =================================================

            float enemyDistance =
                Vector3.Distance(
                    enemyNode.position,
                    node.position
                );


            // =================================================
            // 최종 점수
            // =================================================

            float normalizedPlayerDistance =
                (float)node.gCost /
                maxPlayerDistance;


            float normalizedEnemyDistance =
                enemyDistance /
                (maxPlayerDistance * 0.1f + 1f);


            float score =
                normalizedPlayerDistance * 5f +
                oppositeScore *
                oppositeDirectionWeight *
                10f +
                normalizedEnemyDistance * 2f;


            if (score > bestScore)
            {
                bestScore = score;
                bestNode = node;
            }
        }


        // =====================================================
        // 후보가 없으면 가장 먼 Node
        // =====================================================

        if (bestNode == null)
        {
            int maxCost = -1;


            foreach (Node node in grid.GetAllNodes())
            {
                if (!node.isWalkable)
                    continue;


                if (node.gCost == int.MaxValue)
                    continue;


                if (node == currentDestination)
                    continue;


                if (node.gCost > maxCost)
                {
                    maxCost =
                        node.gCost;

                    bestNode =
                        node;
                }
            }
        }


        return bestNode;
    }


    // =========================================================
    // A*
    // =========================================================

    bool FindPath(
        Vector3 startPos,
        Node targetNode)
    {
        Node startNode =
            grid.GetNodeFromPosition(
                startPos
            );


        if (startNode == null ||
            targetNode == null)
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


        MinPriorityQueue<Node> openSet =
            new MinPriorityQueue<Node>();


        HashSet<Node> closedSet =
            new HashSet<Node>();


        startNode.gCost = 0;


        startNode.hCost =
            GetDistance(
                startNode,
                targetNode
            );


        openSet.Enqueue(startNode);


        while (openSet.Count > 0)
        {
            Node current =
                openSet.Dequeue();


            if (current == targetNode)
            {
                RetracePath(
                    startNode,
                    targetNode
                );

                return true;
            }


            closedSet.Add(current);


            foreach (Node neighbor
                     in grid.GetNeighbours(current))
            {
                if (!neighbor.isWalkable)
                    continue;


                if (closedSet.Contains(neighbor))
                    continue;


                int newGCost =
                    current.gCost +
                    GetDistance(
                        current,
                        neighbor
                    );


                if (!openSet.Contains(neighbor))
                {
                    neighbor.gCost =
                        newGCost;


                    neighbor.hCost =
                        GetDistance(
                            neighbor,
                            targetNode
                        );


                    neighbor.parent =
                        current;


                    openSet.Enqueue(
                        neighbor
                    );
                }
                else if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost =
                        newGCost;


                    neighbor.hCost =
                        GetDistance(
                            neighbor,
                            targetNode
                        );


                    neighbor.parent =
                        current;


                    openSet.Reposition(
                        neighbor
                    );
                }
            }
        }


        grid.path.Clear();

        targetIndex = 0;

        return false;
    }


    // =========================================================
    // 경로 생성
    // =========================================================

    void RetracePath(
        Node startNode,
        Node endNode)
    {
        List<Node> newPath =
            new List<Node>();


        Node current =
            endNode;


        while (current != startNode &&
               current != null)
        {
            newPath.Add(current);

            current =
                current.parent;
        }


        newPath.Reverse();


        grid.path =
            newPath;


        targetIndex = 0;
    }


    // =========================================================
    // 이동
    // =========================================================

    void MoveAlongPath()
    {
        if (grid.path == null ||
            grid.path.Count == 0)
        {
            isMoving = false;
            return;
        }


        if (targetIndex >= grid.path.Count)
        {
            isMoving = false;
            return;
        }


        Node targetNode =
            grid.path[targetIndex];


        Vector3 targetPos =
            targetNode.position;


        targetPos.z =
            transform.position.z;


        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed *
                Time.deltaTime
            );


        if (Vector3.Distance(
                transform.position,
                targetPos
            ) <= reachThreshold)
        {
            targetIndex++;


            if (targetIndex >= grid.path.Count)
            {
                isMoving = false;
            }
        }
    }


    // =========================================================
    // 이동 비용
    // =========================================================

    int GetDistance(
        Node a,
        Node b)
    {
        int dx =
            Mathf.Abs(
                a.gridX -
                b.gridX
            );


        int dy =
            Mathf.Abs(
                a.gridY -
                b.gridY
            );


        if (dx > dy)
        {
            return
                14 * dy +
                10 * (dx - dy);
        }


        return
            14 * dx +
            10 * (dy - dx);
    }


    // =========================================================
    // Gizmo
    // =========================================================

    void OnDrawGizmos()
    {
        if (currentDestination != null)
        {
            Gizmos.color =
                Color.red;


            Gizmos.DrawSphere(
                currentDestination.position,
                0.4f
            );
        }
    }
}
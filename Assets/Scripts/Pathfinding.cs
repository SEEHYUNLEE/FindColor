using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

    public Transform start, destination;

    private Vector3 cacheStart, cacheDest;
    private Grid grid;


    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    void Update()
    {
        if (start.position != cacheStart || destination.position != cacheDest)
        {
            FindPath(start.position, destination.position);

            cacheStart = start.position;
            cacheDest = destination.position;
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.GetNodeFromPosition(startPos);
        Node targetNode = grid.GetNodeFromPosition(targetPos);

        MinPriorityQueue<Node> openSet = new MinPriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Enqueue(startNode);
        while (openSet.Count > 0)
        {
            // 가장 낮은 값을 가진 노드 선택
            Node currentNode = openSet.Dequeue();

            // 가장 낮은 값을 가진 노드가 종착노드면 탐색 종료
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            // 클로즈드 셋에 저장
            closedSet.Add(currentNode);

            // 이웃노드 값을 계산한 후 오픈 셋에 추가
            foreach (Node n in grid.GetNeighbours(currentNode))
            {
                if (!n.isWalkable || closedSet.Contains(n))
                {
                    continue;
                }

                int g = currentNode.gCost + GetDistance(currentNode, n);
                int h = GetDistance(n, targetNode);
                int f = g + h;

                // 오픈 셋에 이미 중복 노드가 있는 경우 값이 작은 쪽으로 변경
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

    // 대각방향 14, 상하좌우 10 (정수 연산이 빠름)
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // (0,0)에서 (3,2)로 갈 때 대각 2번, 가로 1번
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }
}

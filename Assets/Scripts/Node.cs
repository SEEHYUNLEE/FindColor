using System;
using UnityEngine;

[Serializable]
public class Node : IComparable<Node>
{
    public bool isWalkable;
    public Vector3 position;

    public int gridX;
    public int gridY;

    // Pathfinding
    public int gCost;
    public int hCost;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public Node parent;

    public Node(
        bool walkable,
        Vector3 position,
        int gridX,
        int gridY)
    {
        this.isWalkable = walkable;
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int CompareTo(Node otherNode)
    {
        if (this.fCost < otherNode.fCost)
            return -1;

        if (this.fCost > otherNode.fCost)
            return 1;

        return 0;
    }

    // BSP용 데이터
    public Node leftNode;
    public Node rightNode;
    public Node parNode;

    public RectInt nodeRect;
    public RectInt roomRect;

    public Vector2Int center
    {
        get
        {
            return new Vector2Int(
                roomRect.x + roomRect.width / 2,
                roomRect.y + roomRect.height / 2
            );
        }
    }

    public Node(RectInt rect)
    {
        nodeRect = rect;
    }
}
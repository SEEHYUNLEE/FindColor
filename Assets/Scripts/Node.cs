using System;
using UnityEngine;

[Serializable] // 직렬화 : 내부 데이터 저장 및 유니티 인스펙터에서 확인 가능
public class Node : IComparable<Node> // 크기를 비교하는 규칙만들기, CompareTo 함수 구현 필수
{

    public bool isWalkable;
    public Vector3 position;
    public int gridX;
    public int gridY;

    public int gCost; // 시작 노드에서 현재 노드까지 이동한 총 비용
    public int hCost; // 현재 노드에서 목적지까지 직선 거리
    public int fCost { get { return gCost + hCost; } }
    public Node parent;

    public Node(bool walkable, Vector3 position, int gridX, int gridY)
    {
        this.isWalkable = walkable;
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int CompareTo(Node otherNode)
    {
        if (this.fCost < otherNode.fCost) return -1;
        else if (this.fCost > otherNode.fCost) return 1;
        else return 0;
    }

    public Node leftNode;
    public Node rightNode;
    public Node parNode;
    public RectInt nodeRect; //분리된 공간의 rect정보
    public RectInt roomRect; //분리된 공간 속 방의 rect정보
    public Vector2Int center
    {
        get
        {
            return new Vector2Int(roomRect.x + roomRect.width / 2, roomRect.y + roomRect.height / 2);
        }
        //방의 가운데 점. 방과 방을 이을 때 사용
    }
    public Node(RectInt rect)
    {
        this.nodeRect = rect;
    }
}

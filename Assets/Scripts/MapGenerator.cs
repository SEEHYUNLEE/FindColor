using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int mapSize;
    [SerializeField] float minimumDevideRate; //공간이 나눠지는 최소 비율
    [SerializeField] float maximumDivideRate; //공간이 나눠지는 최대 비율
    [SerializeField] private GameObject line; //lineRenderer를 사용해서 공간이 나눠진걸 시작적으로 보여주기 위함
    [SerializeField] private GameObject map; //lineRenderer를 사용해서 첫 맵의 사이즈를 보여주기 위함
    [SerializeField] private GameObject roomLine; //lineRenderer를 사용해서 방의 사이즈를 보여주기 위함
    [SerializeField] private int maximumDepth; //트리의 높이, 높을 수록 방을 더 자세히 나누게 됨
    [SerializeField] Tilemap tileMap;
    [SerializeField] Tilemap wallTileMap;
    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] Tile roomTile; //방을 구성하는 타일
    [SerializeField] Tile wallTile; //방과 외부를 구분지어줄 벽 타일
    [SerializeField] Tile outTile; //방 외부의 타일

    [Header("UI")]
    [SerializeField] private Button generateButton; // 맵 재생성 버튼
    [HideInInspector] public List<Vector3Int> roomCenters = new List<Vector3Int>();

    void Start()
    {
        // 버튼 이벤트 연결 (인스펙터에서 직접 연결하지 않아도 코드로 자동 등록)
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(CreateNewMap);
        }

        // 게임 시작 시 최초 맵 생성
        CreateNewMap();
    }

    // 외부 버튼 클릭이나 시작 시 호출되는 맵 생성 통합 함수
    public void CreateNewMap()
    {
        tileMap.ClearAllTiles(); // 기존에 그려진 타일들을 전부 초기화 (안 지우면 겹침)
        wallTileMap.ClearAllTiles();
        backgroundTilemap.ClearAllTiles();
        roomCenters.Clear(); // 맵 재생성 시 기존 방 중심점 리스트 초기화

        FillBackground(); //신 로드 시 전부다 바깥타일로 덮음
        Node root = new Node(new RectInt(0, 0, mapSize.x, mapSize.y));
        Divide(root, 0);
        GenerateRoom(root, 0);
        GenerateLoad(root, 0);
        FillWall(); //바깥과 방이 만나는 지점을 벽으로 칠해주는 함수
    }

    void Divide(Node tree, int n)
    {
        if (n == maximumDepth) return; //내가 원하는 높이에 도달하면 더 나눠주지 않는다.
                                       //그 외의 경우에는

        int maxLength = Mathf.Max(tree.nodeRect.width, tree.nodeRect.height);
        //가로와 세로중 더 긴것을 구한후, 가로가 길다면 위 좌, 우로 세로가 더 길다면 위, 아래로 나눠주게 될 것이다.
        int split = Mathf.RoundToInt(Random.Range(maxLength * minimumDevideRate, maxLength * maximumDivideRate));
        //나올 수 있는 최대 길이와 최소 길이중에서 랜덤으로 한 값을 선택
        if (tree.nodeRect.width >= tree.nodeRect.height) //가로가 더 길었던 경우에는 좌 우로 나누게 될 것이며, 이 경우에는 세로 길이는 변하지 않는다.
        {

            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, split, tree.nodeRect.height));
            //왼쪽 노드에 대한 정보다 
            //위치는 좌측 하단 기준이므로 변하지 않으며, 가로 길이는 위에서 구한 랜덤값을 넣어준다.
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x + split, tree.nodeRect.y, tree.nodeRect.width - split, tree.nodeRect.height));
            //우측 노드에 대한 정보다 
            //위치는 좌측 하단에서 오른쪽으로 가로 길이만큼 이동한 위치이며, 가로 길이는 기존 가로길이에서 새로 구한 가로값을 뺀 나머지 부분이 된다. 
        }
        else
        {

            tree.leftNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y, tree.nodeRect.width, split));
            tree.rightNode = new Node(new RectInt(tree.nodeRect.x, tree.nodeRect.y + split, tree.nodeRect.width, tree.nodeRect.height - split));
            //DrawLine(new Vector2(tree.nodeRect.x , tree.nodeRect.y+ split), new Vector2(tree.nodeRect.x + tree.nodeRect.width, tree.nodeRect.y  + split));
        }
        tree.leftNode.parNode = tree; //자식노드들의 부모노드를 나누기전 노드로 설정
        tree.rightNode.parNode = tree;
        Divide(tree.leftNode, n + 1); //왼쪽, 오른쪽 자식 노드들도 나눠준다.
        Divide(tree.rightNode, n + 1);//왼쪽, 오른쪽 자식 노드들도 나눠준다.
    }
    private RectInt GenerateRoom(Node tree, int n)
    {
        RectInt rect;
        if (n == maximumDepth) //해당 노드가 리프노드라면 방을 만들어 줄 것이다.
        {
            rect = tree.nodeRect;
            int width = Random.Range(rect.width / 2, rect.width - 1);
            //방의 가로 최소 크기는 노드의 가로길이의 절반, 최대 크기는 가로길이보다 1 작게 설정한 후 그 사이 값중 랜덤한 값을 구해준다.
            int height = Random.Range(rect.height / 2, rect.height - 1);
            //높이도 위와 같다.
            int x = rect.x + Random.Range(1, rect.width - width);
            //방의 x좌표이다. 만약 0이 된다면 붙어 있는 방과 합쳐지기 때문에,최솟값은 1로 해주고, 최댓값은 기존 노드의 가로에서 방의 가로길이를 빼 준 값이다.
            int y = rect.y + Random.Range(1, rect.height - height);
            //y좌표도 위와 같다.
            rect = new RectInt(x, y, width, height);
            FillRoom(rect);

            // 생성된 방의 중심 타일 좌표를 리스트에 저장
            Vector2Int center = new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);
            roomCenters.Add(new Vector3Int(center.x - mapSize.x / 2, center.y - mapSize.y / 2, 0));
        }
        else
        {
            tree.leftNode.roomRect = GenerateRoom(tree.leftNode, n + 1);
            tree.rightNode.roomRect = GenerateRoom(tree.rightNode, n + 1);
            rect = tree.leftNode.roomRect;
        }
        return rect;
    }
    private void GenerateLoad(Node tree, int n)
    {
        if (n == maximumDepth) // 리프 노드라면 이을 자식이 없다.
            return;

        Vector2Int leftNodeCenter = tree.leftNode.center;
        Vector2Int rightNodeCenter = tree.rightNode.center;

        // 1. 가로 통로 (두께 2로 확장)
        for (int i = Mathf.Min(leftNodeCenter.x, rightNodeCenter.x); i <= Mathf.Max(leftNodeCenter.x, rightNodeCenter.x); i++)
        {
            // 기본 1칸
            tileMap.SetTile(new Vector3Int(i - mapSize.x / 2, leftNodeCenter.y - mapSize.y / 2, 0), roomTile);
            // 바로 위쪽 칸을 같이 채워서 두께를 2로 만듦
            tileMap.SetTile(new Vector3Int(i - mapSize.x / 2, leftNodeCenter.y - mapSize.y / 2 + 1, 0), roomTile);
        }

        // 2. 세로 통로 (두께 2로 확장)
        for (int j = Mathf.Min(leftNodeCenter.y, rightNodeCenter.y); j <= Mathf.Max(leftNodeCenter.y, rightNodeCenter.y); j++)
        {
            // 기본 1칸
            tileMap.SetTile(new Vector3Int(rightNodeCenter.x - mapSize.x / 2, j - mapSize.y / 2, 0), roomTile);
            // 바로 오른쪽 칸을 같이 채워서 두께를 2로 만듦
            tileMap.SetTile(new Vector3Int(rightNodeCenter.x - mapSize.x / 2 + 1, j - mapSize.y / 2, 0), roomTile);
        }

        // 자식 노드들도 탐색
        GenerateLoad(tree.leftNode, n + 1);
        GenerateLoad(tree.rightNode, n + 1);
    }
    void FillBackground()
    {
        for (int i = -10; i < mapSize.x + 10; i++)
        {
            for (int j = -10; j < mapSize.y + 10; j++)
            {
                Vector3Int cellPos = new Vector3Int(i - mapSize.x / 2, j - mapSize.y / 2, 0);

                if (backgroundTilemap != null)
                {
                    backgroundTilemap.SetTile(cellPos, outTile);
                }
            }
        }
    }
    void FillWall()
    {
        // 1단계: 1차 벽(wallTileMap) 채우기 (기존 로직)
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3Int cellPos = new Vector3Int(i - mapSize.x / 2, j - mapSize.y / 2, 0);

                // 만약 지금 탐색하는 자리가 이미 방/통로(roomTile)라면 벽을 그리면 안됨
                if (tileMap.GetTile(cellPos) == roomTile) continue;

                // 배경 타일맵(backgroundTilemap)을 기준으로 바깥(outTile)인 공간을 탐색
                if (backgroundTilemap != null && backgroundTilemap.GetTile(cellPos) == outTile)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0) continue;

                            Vector3Int neighborPos = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);

                            // 그 주변에 roomTile(바닥)이 있다면 wallTilemap에 벽을 그림
                            if (tileMap.GetTile(neighborPos) == roomTile)
                            {
                                if (wallTileMap != null)
                                {
                                    wallTileMap.SetTile(cellPos, wallTile);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    private void FillRoom(RectInt rect)
    { //room의 rect정보를 받아서 tile을 set해주는 함수
        for (int i = rect.x; i < rect.x + rect.width; i++)
        {
            for (int j = rect.y; j < rect.y + rect.height; j++)
            {
                tileMap.SetTile(new Vector3Int(i - mapSize.x / 2, j - mapSize.y / 2, 0), roomTile);
            }
        }
    }
}
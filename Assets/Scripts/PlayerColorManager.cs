using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    [System.Serializable]
    public struct BodyPart
    {
        public SpriteRenderer renderer; // 해당 부위의 SpriteRenderer
    }

    [Header("Player Body Parts (Total 7)")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    // 이미 색상이 변경된 부위의 인덱스를 저장하는 리스트 (HashSet 대신 List 사용)
    private List<int> coloredParts = new List<int>();

    private void Start()
    {
        // 씬 시작 시 저장된 색상 데이터 자동 복원
        if (DataManager.Instance != null && DataManager.Instance.currentData != null)
        {
            LoadColorData(DataManager.Instance.currentData);
        }
    }

    /// <summary>
    /// 외부(Item)에서 호출해 아이템 색상을 7개 부위 중 미색상 부위에 적용
    /// </summary>
    public void ApplyColorToRandomPart(Color newColor)
    {
        // 1. 아직 색상이 안 바뀐 부위의 인덱스 목록 추출
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (!coloredParts.Contains(i) && bodyParts[i].renderer != null)
            {
                availableIndices.Add(i);
            }
        }

        // 2. 만약 모든 부위가 이미 색상이 칠해졌다면 처리 중단
        if (availableIndices.Count == 0)
        {
            Debug.Log("모든 신체 부위의 색상이 이미 변경되었습니다!");
            return;
        }

        // 3. 남은 부위 중 무작위 하나 선택
        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        // 4. 선택된 부위에 색상 적용 및 칠해진 부위로 등록
        bodyParts[randomIndex].renderer.color = newColor;
        coloredParts.Add(randomIndex);

        // 5. 색상 변경 즉시 DataManager 데이터 갱신
        if (DataManager.Instance != null)
        {
            SaveColorData(DataManager.Instance.currentData);
        }
    }

    // --- JSON 저장 시 호출 ---
    public void SaveColorData(PlayerData data)
    {
        if (data.bodyPartHexColors == null)
            data.bodyPartHexColors = new List<string>();

        data.bodyPartHexColors.Clear();
        // List를 그대로 복사하여 전달
        data.coloredParts = new List<int>(coloredParts);

        // 부위별 Color를 HEX 문자열("#RRGGBBAA") 형태로 전환하여 저장
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].renderer != null)
            {
                string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(bodyParts[i].renderer.color);
                data.bodyPartHexColors.Add(hexColor);
            }
            else
            {
                data.bodyPartHexColors.Add("#FFFFFF"); // null 예외 대비 기본값
            }
        }
    }

    // --- JSON 로드 시 호출 ---
    public void LoadColorData(PlayerData data)
    {
        if (data == null || data.bodyPartHexColors == null) return;

        // 저장된 List를 그대로 불러오기
        coloredParts = new List<int>(data.coloredParts);

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (i < data.bodyPartHexColors.Count && bodyParts[i].renderer != null)
            {
                // 저장된 HEX 문자열을 Color 타입으로 재변환해 SpriteRenderer에 적용
                if (ColorUtility.TryParseHtmlString(data.bodyPartHexColors[i], out Color restoredColor))
                {
                    bodyParts[i].renderer.color = restoredColor;
                }
            }
        }
    }
}
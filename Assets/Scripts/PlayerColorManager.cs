using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    [System.Serializable]
    public struct BodyPart
    {
        public SpriteRenderer renderer;  // 해당 부위의 SpriteRenderer
    }

    [Header("Player Body Parts (Total 7)")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    // 이미 색상이 변경된 부위의 인덱스를 저장하는 집합
    private HashSet<int> coloredPartIndices = new HashSet<int>();

    /// <summary>
    /// 외부(Item)에서 호출해 아이템 색상을 7개 부위 중 미색상 부위에 적용
    /// </summary>
    public void ApplyColorToRandomPart(Color newColor)
    {
        // 1. 아직 색상이 안 바뀐 부위의 인덱스 목록 추출
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (!coloredPartIndices.Contains(i) && bodyParts[i].renderer != null)
            {
                availableIndices.Add(i);
            }
        }

        // 2. 만약 모든 부위가 이미 색상이 칠해졌다면 처리 중단 (또는 예외 처리)
        if (availableIndices.Count == 0)
        {
            Debug.Log("모든 신체 부위의 색상이 이미 변경되었습니다!");
            return;
        }

        // 3. 남은 부위 중 무작위 하나 선택
        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        // 4. 색상 적용 및 칠해진 부위로 등록
        bodyParts[randomIndex].renderer.color = newColor;
        coloredPartIndices.Add(randomIndex);
    }
}
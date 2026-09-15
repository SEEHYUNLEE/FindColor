using System.Collections.Generic;
using UnityEngine;

public class PlayerUi : MonoBehaviour
{
    [System.Serializable]
    public struct BodyPart
    {
        public SpriteRenderer renderer;
    }

    [Header("Player Body Parts")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    /// <summary>
    /// SaveSlotUI에서 불러온 PlayerData를 직접 전달받아 색상만 입힙니다.
    /// </summary>
    public void SetColorData(PlayerData data)
    {
        if (data == null || data.bodyPartHexColors == null) return;

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (i < data.bodyPartHexColors.Count && bodyParts[i].renderer != null)
            {
                if (ColorUtility.TryParseHtmlString(data.bodyPartHexColors[i], out Color restoredColor))
                {
                    bodyParts[i].renderer.color = restoredColor;
                }
            }
        }
    }
}
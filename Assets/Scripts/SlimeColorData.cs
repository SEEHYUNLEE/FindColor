using System.Collections.Generic;
using UnityEngine;

// 1. 색상 종류 Enum
public enum SlimeColorType
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Indigo,
    Violet
}

// 2. 색상 데이터 구조체
[System.Serializable]
public struct SlimeColorData
{
    public SlimeColorType colorType;
    public Color color;

    public SlimeColorData(SlimeColorType colorType, Color color)
    {
        this.colorType = colorType;
        this.color = color;
    }
}

// 3. HEX 데이터 적용 중앙 관리 팔레트
public static class SlimeColorPalette
{
    // HEX 코드 기반의 데이터 구조체
    private struct SlimeHexData
    {
        public SlimeColorType colorType;
        public string hexCode;

        public SlimeHexData(SlimeColorType colorType, string hexCode)
        {
            this.colorType = colorType;
            this.hexCode = hexCode;
        }
    }

    // HEX 문자열 매핑 배열
    private static readonly SlimeHexData[] HexColors = new SlimeHexData[]
    {
        new SlimeHexData(SlimeColorType.Red,    "#FF3333"),
        new SlimeHexData(SlimeColorType.Orange, "#FF801A"),
        new SlimeHexData(SlimeColorType.Yellow, "#FFE633"),
        new SlimeHexData(SlimeColorType.Green,  "#33CC4D"),
        new SlimeHexData(SlimeColorType.Blue,   "#3399FF"),
        new SlimeHexData(SlimeColorType.Indigo, "#1A1A99"),
        new SlimeHexData(SlimeColorType.Violet, "#9933CC")
    };

    // 정적 생성자: 최초 호출 시 HEX 문자열을 Color로 변환하여 Colors 배열 초기화
    public static readonly SlimeColorData[] Colors;

    static SlimeColorPalette()
    {
        Colors = new SlimeColorData[HexColors.Length];

        for (int i = 0; i < HexColors.Length; i++)
        {
            // HEX 문자열 Color로 변환
            if (ColorUtility.TryParseHtmlString(HexColors[i].hexCode, out Color parsedColor))
            {
                Colors[i] = new SlimeColorData(HexColors[i].colorType, parsedColor);
            }
            else
            {
                Colors[i] = new SlimeColorData(HexColors[i].colorType, Color.white);
            }
        }
    }

    // 플레이어 몸에 아직 적용되지 않은 색상 중 무작위 1개 반환
    public static SlimeColorData GetRandomColorData(PlayerData playerData)
    {
        // 데이터가 없거나 아직 입혀진 색상이 없다면 전체 중 순수 무작위 반환
        if (playerData == null || playerData.bodyPartHexColors == null || playerData.bodyPartHexColors.Count == 0)
        {
            int randomIndex = Random.Range(0, Colors.Length);
            return Colors[randomIndex];
        }

        List<SlimeColorData> availableColors = new List<SlimeColorData>();

        foreach (var colorData in Colors)
        {
            bool isAlreadyApplied = false;

            // bodyPartHexColors에 있는 HEX 문자열과 RGB 값 세부 비교
            foreach (string bodyHex in playerData.bodyPartHexColors)
            {
                if (ColorUtility.TryParseHtmlString(bodyHex, out Color bodyColor))
                {
                    if (Mathf.Approximately(colorData.color.r, bodyColor.r) &&
                        Mathf.Approximately(colorData.color.g, bodyColor.g) &&
                        Mathf.Approximately(colorData.color.b, bodyColor.b))
                    {
                        isAlreadyApplied = true;
                        break;
                    }
                }
            }

            if (!isAlreadyApplied)
            {
                availableColors.Add(colorData);
            }
        }

        // 7개 색상이 이미 플레이어 신체에 모두 적용된 경우 전체 색상 중에서 재선택
        if (availableColors.Count == 0)
        {
            availableColors.AddRange(Colors);
        }

        int finalIndex = Random.Range(0, availableColors.Count);
        return availableColors[finalIndex];
    }
}
using UnityEngine;

// 1. 색상 종류 Enum (동일)
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

// 2. 색상 데이터 구조체 (동일)
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

    // 랜덤 색상 데이터 반환
    public static SlimeColorData GetRandomColorData()
    {
        int randomIndex = Random.Range(0, Colors.Length);
        return Colors[randomIndex];
    }
}
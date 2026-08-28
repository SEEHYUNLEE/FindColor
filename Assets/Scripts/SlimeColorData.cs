using UnityEngine;

// 1. 색상 종류 Enum (아이템, 플레이어 부위 변경 시 식별자로 활용)
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

// 3. 색상 데이터 중앙 관리 팔레트
public static class SlimeColorPalette
{
    public static readonly SlimeColorData[] Colors = new SlimeColorData[]
    {
        new SlimeColorData(SlimeColorType.Red,    new Color(1f, 0.2f, 0.2f)),
        new SlimeColorData(SlimeColorType.Orange, new Color(1f, 0.5f, 0.1f)),
        new SlimeColorData(SlimeColorType.Yellow, new Color(1f, 0.9f, 0.2f)),
        new SlimeColorData(SlimeColorType.Green,  new Color(0.2f, 0.8f, 0.3f)),
        new SlimeColorData(SlimeColorType.Blue,   new Color(0.2f, 0.6f, 1f)),
        new SlimeColorData(SlimeColorType.Indigo, new Color(0.1f, 0.1f, 0.6f)),
        new SlimeColorData(SlimeColorType.Violet, new Color(0.6f, 0.2f, 0.8f))
    };

    // 랜덤 색상 데이터 반환
    public static SlimeColorData GetRandomColorData()
    {
        int randomIndex = Random.Range(0, Colors.Length);
        return Colors[randomIndex];
    }
}
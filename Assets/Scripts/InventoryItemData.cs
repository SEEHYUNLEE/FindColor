using System;

[Serializable]
public class InventoryItemData
{
    public SlimeColorType colorType;
    public int amount;

    public InventoryItemData(SlimeColorType colorType, int amount = 1)
    {
        this.colorType = colorType;
        this.amount = amount;
    }
}
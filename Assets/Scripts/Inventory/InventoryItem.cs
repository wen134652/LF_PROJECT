using UnityEngine;

[System.Serializable]
//背包内物品实例
public class InventoryItem
{
    public ItemSO item;
    public int count;

    //左上角
    public int originX;
    public int originY;

    //物品是否旋转
    public bool rotated;

    //当前占用宽高
    public int Width => rotated ? item.gridHeight : item.gridWidth;
    public int Height => rotated ? item.gridWidth : item.gridHeight;

    public InventoryItem(ItemSO item,int count,int originX,int originY, bool rotated)
    {
        this.item = item;
        this.count = count;
        this.originX = originX;
        this.originY = originY;
        this.rotated = rotated;
    }
}

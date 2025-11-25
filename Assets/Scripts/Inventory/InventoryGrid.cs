using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    [Header("背包尺寸")]
    public int width = 6;
    public int height = 10;

    private InventoryItem[,] cells;
    [SerializeField]
    private List<InventoryItem> items = new List<InventoryItem>();
    
    private InventoryGridView inventory;
    public IReadOnlyList<InventoryItem> Items => items;
    public int Width => width;
    public int Height => height;

    
    private void Awake()
    {
        inventory = GetComponentInParent<InventoryGridView>();
        cells = new InventoryItem[width, height];
    }
    public InventoryItem GetItemAt(int x,int y)
    {
        if (!InBounds(x, y)) return null;
        return cells[x, y];
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    
    //位置空余检查
    public bool CanPlace(ItemSO item, int x,int y, bool rotated, InventoryItem ignoreItem = null)
    {
        if (item == null)
        {
            return false;
        }
        int w = rotated ? item.gridHeight : item.gridWidth;
        int h = rotated ? item.gridWidth : item.gridHeight;

        if (x < 0 || y < 0 || x + w > width || y + h > height)
            return false;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (cells[x+i,y+j]!=null && cells[x+i,y+j]!= ignoreItem)
                {
                    return false;
                }
            }
        }
        return true;
    }
    //背包中放入实例
    public InventoryItem PlaceNewItem(ItemSO item, int count, int x, int y, bool rotated)
    {
        
        if (!CanPlace(item,x,y,rotated))
        {
            Debug.Log("进入了cannot place");
            return null;
        }
        InventoryItem inst = new InventoryItem(item, Mathf.Max(1, count), x, y, rotated);
        Debug.Log($"成功创建{inst.item.name}");
        items.Add(inst);
        
        FillCells(inst, inst.originX, inst.originY, true);
        inventory.RefreshAllItems();
        return inst;
    }
    private void FillCells(InventoryItem inst, int originX, int originY, bool occupy)
    {
        if (inst == null) return;
        int w = inst.Width;
        int h = inst.Height;

        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                int gx = originX + ix;
                int gy = originY + iy;

                if (!InBounds(gx, gy)) continue;

                cells[gx, gy] = occupy ? inst : null;
            }
        }
    }
    public bool MoveItem(InventoryItem inst, int newX, int newY, bool newRotated)
    {
        if (inst==null)
        {
            return false;
        }
        FillCells(inst, inst.originX, inst.originY, false);
        bool canPlace = CanPlace(inst.item, newX, newY, newRotated, null);
        if (!canPlace)
        {
            FillCells(inst, inst.originX, inst.originY, true);
            return false;
        }
        inst.originX = newX;
        inst.originY = newY;
        inst.rotated = newRotated;
        FillCells(inst, inst.originX, inst.originY, true);
        return true;
    }
    public void RemoveItem(InventoryItem inst)
    {
        if (inst == null)
        {
            return;
        }
        FillCells(inst, inst.originX, inst.originY, false);
        items.Remove(inst);
    }
    public void MovingItem(InventoryItem inst)
    {
        if (inst == null)
        {
            return;
        }
        FillCells(inst, inst.originX, inst.originY, false);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ManufactureGrid : MonoBehaviour
{
    [Header("材料区尺寸(最大)")]
    public int width = 3;
    public int height = 3;

    private InventoryItem[,] cells;
    public List<InventoryItem> items = new List<InventoryItem>();

    public int Width => width;
    public int Height => height;

    public System.Action OnChanged;

    private void OnEnable()
    {
        cells = new InventoryItem[3, 3];
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public InventoryItem GetItemAt(int x, int y)
    {
        if (!InBounds(x, y)) return null;
        return cells[x, y];
    }

    // ========= 和背包一样的 CanPlace / PlaceNewItem =========

    public bool CanPlace(ItemSO item, int x, int y, bool rotated, InventoryItem ignoreItem = null)
    {
        if (item == null) return false;

        int w = rotated ? item.gridHeight : item.gridWidth;
        int h = rotated ? item.gridWidth : item.gridHeight;

        if (x < 0 || y < 0 || x + w > width || y + h > height)
            return false;

        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                var cellItem = cells[x + ix, y + iy];
                if (cellItem != null && cellItem != ignoreItem)
                    return false;
            }
        }
        return true;
    }

    public InventoryItem PlaceNewItem(ItemSO item, int count, int x, int y, bool rotated)
    {
        if (!CanPlace(item, x, y, rotated))
            return null;

        InventoryItem inst = new InventoryItem(item, Mathf.Max(1, count), x, y, rotated);
        items.Add(inst);
        FillCells(inst, inst.originX, inst.originY, true);

        OnChanged?.Invoke();
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

    public void CleanGrid()
    {
        foreach (var item in items)
        {
            FillCells(item, item.originX, item.originY, false);
        }
        items.Clear();
        OnChanged?.Invoke();
    }
    public void RemoveItem(InventoryItem inst)
    {
        if (inst == null) return;

        FillCells(inst, inst.originX, inst.originY, false);
        items.Remove(inst);

        OnChanged?.Invoke();
    }

    public void MovingItem(InventoryItem inst)
    {
        if (inst == null)
        {
            return;
        }
        FillCells(inst, inst.originX, inst.originY, false);
    }
    // 从某个格子删掉整件物品
    public void RemoveItemAt(int x, int y)
    {
        var inst = GetItemAt(x, y);
        if (inst != null)
            RemoveItem(inst);
    }

    public void ClearAll()
    {
        cells = new InventoryItem[width, height];
        items.Clear();
        OnChanged?.Invoke();
    }
}

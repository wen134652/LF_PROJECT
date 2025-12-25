using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManufactureGridView : MonoBehaviour
{
    [Header("逻辑")]
    public ManufactureGrid materialGrid;
    public ManufactureManager manager;
    public InventoryGridView inventoryView;   // 左边背包，用来读拖拽状态

    [Header("格子层（和背包一样）")]
    public RectTransform cellsRoot;           // 有 GridLayoutGroup
    public GameObject cellPrefab;             // 单格 prefab，挂 ManufactureCellUI

    [Header("图标层（直接复用背包的 icon prefab）")]
    public RectTransform itemsRoot;           // 和 cellsRoot 对齐
    public GameObject itemIconPrefab;         // 用背包里那一个

    private ManufactureCellUI[,] cellUIs;
    private Dictionary<InventoryItem, RectTransform> itemIcons =
        new Dictionary<InventoryItem, RectTransform>();

    private InventoryItem draggingItem;
    private RectTransform draggingIcon;
    private RectTransform originIconForDrag;

    private GridLayoutGroup layout;
    private Canvas rootCanvas;

    private void Awake()
    {
        if (cellsRoot != null)
            layout = cellsRoot.GetComponent<GridLayoutGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (materialGrid == null)
        {
            Debug.LogError("ManufactureGridView: materialGrid 未设置");
            return;
        }
        manager.RefreshContainer += BuildCells;
        BuildCells();
        RefreshAllItems();

        materialGrid.OnChanged += RefreshAllItems;
    }

    private void OnDisable()
    {
        if (materialGrid != null)
            materialGrid.OnChanged -= RefreshAllItems;
    }
    // ========= 生成材料格子 =========
    public void BuildCells()
    {
        int w = materialGrid.Width;
        int h = materialGrid.Height;

        cellUIs = new ManufactureCellUI[w, h];

        foreach (Transform child in cellsRoot)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                GameObject go = Instantiate(cellPrefab, cellsRoot);
                ManufactureCellUI cell = go.GetComponent<ManufactureCellUI>();
                cell.x = x;
                cell.y = y;
                cell.owner = this;
                cellUIs[x, y] = cell;
            }
        }
    }

    // ========= 重绘所有物品图标（和背包一样） =========
    public void RefreshAllItems()
    {
        // 清图标
        foreach (var kv in itemIcons)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }
        itemIcons.Clear();

        // 为材料区里的每件物品生成一张 icon
        foreach (var inst in materialGrid.items)
        {
            CreateIconForItem(inst);
        }
    }

    private void CreateIconForItem(InventoryItem inst)
    {
        if (inst?.item == null) return;

        GameObject go = Instantiate(itemIconPrefab, itemsRoot);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 图片
        var img = go.GetComponent<Image>();
        if (img != null)
            img.sprite = inst.item.icon;

        // 数量文本（可选）
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = inst.count > 1 ? inst.count.ToString() : "";

        // ===== 1. 尺寸：覆盖 w×h 个格子（完全照抄 InventoryGridView） =====
        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        int w = inst.Width;
        int h = inst.Height;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        rt.sizeDelta = inst.rotated ? new Vector2(height, width) : new Vector2(width, height);
        rt.rotation = inst.rotated ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, 0f);

        // ===== 2. 位置：用左上格和右下格的世界坐标算中心 =====
        ManufactureCellUI cellLT = cellUIs[inst.originX, inst.originY];
        ManufactureCellUI cellRB = cellUIs[inst.originX + w - 1, inst.originY + h - 1];

        RectTransform rtLT = cellLT.GetComponent<RectTransform>();
        RectTransform rtRB = cellRB.GetComponent<RectTransform>();

        Vector3 worldCenterLT = rtLT.TransformPoint(rtLT.rect.center);
        Vector3 worldCenterRB = rtRB.TransformPoint(rtRB.rect.center);
        Vector3 worldCenter = (worldCenterLT + worldCenterRB) * 0.5f;

        Vector3 localCenter = itemsRoot.InverseTransformPoint(worldCenter);
        rt.anchoredPosition = localCenter;

        itemIcons[inst] = rt;
    }

    // ========= 当格子被点击 =========
    public void OnCellClicked(ManufactureCellUI cell)
    {
        int x = cell.x;
        int y = cell.y;

        // 情况 1：背包正在拖拽物品 → 尝试把这件物品放到材料区
        if (inventoryView != null && inventoryView.IsDraggingItem)
        {
            ItemSO item = inventoryView.GetDraggingItemSO();
            if (item == null) return;

            bool rotated = inventoryView.DraggingItemRotated;

            if (!materialGrid.CanPlace(item, x, y, rotated))
            {
                Debug.Log("材料格位置放不下该物品");
                return;
            }

            InventoryItem inst = materialGrid.PlaceNewItem(item, 1, x, y, rotated);
            if (inst == null) return;

            bool ok = inventoryView.ConsumeOneFromDraggingForExternal();
            if (!ok)
            {
                materialGrid.RemoveItem(inst);
                return;
            }
            Debug.Log("2222");
            // PlaceNewItem 里已经触发 OnChanged，这里不需要再手动 Refresh
            return;
        }
        // 情况 2：没有在拖东西,拖起来
        InventoryItem existing = materialGrid.GetItemAt(x, y);
        if (existing != null)
        {
            materialGrid.MovingItem(existing);
            inventoryView.StartDrag(existing);
            inventoryView.otherSystemDragging = true;
            originIconForDrag = null;
            if (itemIcons.TryGetValue(existing, out RectTransform originIconRT))
            {
                originIconForDrag = originIconRT;
                itemIcons.Remove(existing);
                materialGrid.items.Remove(existing);
                Destroy(originIconForDrag.gameObject);
            }
            return;
        }
        Debug.Log("空格子");
        //materialGrid.OnChanged();
    }
}

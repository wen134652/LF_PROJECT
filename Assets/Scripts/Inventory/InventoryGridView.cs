using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGridView : MonoBehaviour
{
    [Header("逻辑背包")]
    public InventoryGrid inventoryGrid;   

    [Header("UI 绑定")]
    public RectTransform cellsRoot;       
    public RectTransform itemsRoot;       
    public GameObject cellPrefab;         
    public GameObject itemIconPrefab;    

    private InventoryCellUI[,] cellUIs;
    private Dictionary<InventoryItem, RectTransform> itemIcons =
        new Dictionary<InventoryItem, RectTransform>();

    private GridLayoutGroup layout;

    private InventoryItem selectedItem;
    private Color selectedColor = new Color(1f, 1f, 0.6f);
    private Color normalColor = Color.white;

    private readonly Color iconNormalColor = Color.white;
    private readonly Color iconDimColor = new Color(1f, 1f, 1f, 0.35f); // 半透明

    private InventoryItem draggingItem;
    private RectTransform draggingIcon;
    private RectTransform originIconForDrag;
    private Canvas rootCanvas;

    // 预览高亮
    private InventoryCellUI hoveredCell;
    [Header("预览颜色")]
    public Color previewOkColor = new Color(0f, 1f, 0f, 0.35f); // 绿色半透明
    public Color previewBadColor = new Color(1f, 0f, 0f, 0.35f); // 红色半透明

    public ItemSO testItem;
    private void Awake()
    {
        layout = cellsRoot.GetComponent<GridLayoutGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (inventoryGrid == null)
        {
            Debug.LogError("InventoryGridView: 未设置 inventoryGrid！");
            return;
        }

        BuildCells();
        
        StartCoroutine(DelayedRefresh());
        RefreshAllItems();
    }
    private void Update()
    {
        if (draggingItem != null && draggingIcon != null)
        {
            // 跟随鼠标
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemsRoot,
                Input.mousePosition,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out localPos
            );
            draggingIcon.anchoredPosition = localPos;

            // 右键旋转
            if (Input.GetMouseButtonDown(1))
            {
                RotateDraggingItem();
                // 旋转后宽高变了，预览也要重算
                if (hoveredCell != null)
                    UpdatePreview();
            }

            // 鼠标移出背包区域时清除预览
            bool inGrid = RectTransformUtility.RectangleContainsScreenPoint(
                cellsRoot,
                Input.mousePosition,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera
            );
            if (!inGrid)
            {
                hoveredCell = null;
                ClearPreview();
            }
        }

    }


    // ========= 生成所有格子 =========
    private void BuildCells()
    {
        int w = inventoryGrid.Width;
        int h = inventoryGrid.Height;

        cellUIs = new InventoryCellUI[w, h];

        // 清空旧的
        foreach (Transform child in cellsRoot)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                GameObject go = Instantiate(cellPrefab, cellsRoot);
                InventoryCellUI cell = go.GetComponent<InventoryCellUI>();
                cell.x = x;
                cell.y = y;
                cell.owner = this;
                cellUIs[x, y] = cell;
            }
        }
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var cell =cellUIs[x, y] ;
                Debug.Log(itemsRoot.InverseTransformPoint(cell.GetComponent<RectTransform>().TransformPoint(cell.GetComponent<RectTransform>().rect.center)));
            }
        }
    }

    // ========= 重新绘制所有物品图标 =========
    public void RefreshAllItems()
    {
        // 1. 清空旧图标
        foreach (var kv in itemIcons)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }
        itemIcons.Clear();

        // 2. 根据逻辑背包里的 Items 逐个创建
        foreach (var inst in inventoryGrid.Items)
        {
            CreateIconForItem(inst);
        }
    }

    private void CreateIconForItem(InventoryItem inst)
    {
        if (inst?.item == null) return;

        GameObject go = Instantiate(itemIconPrefab, itemsRoot);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 设置图片
        var img = go.GetComponent<Image>();
        if (img != null)
            img.sprite = inst.item.icon;

        // 数量文本（可选）
        var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
            text.text = inst.count > 1 ? inst.count.ToString() : "";

        // ===== 1. 设置尺寸：覆盖 w×h 个格子 =====
        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        int w = inst.Width;
        int h = inst.Height;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        rt.sizeDelta = new Vector2(width, height);

        // ===== 2. 用格子的世界坐标来算“多格区域的中心” =====

        // 左上那个格子
        RectTransform cellLT = cellUIs[inst.originX, inst.originY]
                               .GetComponent<RectTransform>();

        // 右下那个格子（占 w×h，所以是 originX + w - 1, originY + h - 1）
        RectTransform cellRB = cellUIs[inst.originX + w - 1, inst.originY + h - 1]
                               .GetComponent<RectTransform>();

        // 取每个格子的“世界空间中心点”
        Vector3 worldCenterLT = cellLT.TransformPoint(cellLT.rect.center);
        Vector3 worldCenterRB = cellRB.TransformPoint(cellRB.rect.center);

        // 多格区域的中心 = 左上格中心和右下格中心的中点
        Vector3 worldCenter = (worldCenterLT + worldCenterRB) * 0.5f;

        // 把这个世界坐标转换成 ItemsRoot 本地坐标
        Vector3 localCenter = itemsRoot.InverseTransformPoint(worldCenter);
        // 把图标的锚点位置设成这个中心
        rt.anchoredPosition = localCenter;

        itemIcons[inst] = rt;
    }



    // ========= 点击格子回调 =========
    public void OnCellClicked(InventoryCellUI cell)
    {
        int x = cell.x;
        int y = cell.y;

        var inst = inventoryGrid.GetItemAt(x, y);

        // 情况 1：当前没有在拖东西
        if (draggingItem == null)
        {
            if (inst != null)
            {
                inventoryGrid.MovingItem(inst);
                StartDrag(inst);
                Debug.Log($"开始拖动物品：{inst.item.displayName}");
            }
            else
            {
                Debug.Log($"格子 ({x},{y}) 是空的");
            }
            return;
        }

        // 情况 2：正在拖动物品（draggingItem 不为 null）

        // 2.1 再点到同一件物品原来的区域：认为是“放回去 / 取消”
        if (inst == draggingItem)
        {
            StopDrag();
            RefreshAllItems();
            Debug.Log("取消拖拽");
            return;
        }

        // 2.2 点到其他非空格子：暂时不支持交换，就是无法放下
        if (inst != null && inst != draggingItem)
        {
            Debug.Log("这个位置放不下该物品");
            return;
        }

        // 2.3 点到空格子 → 尝试把正在拖的物品移动到这里
        if (inst == null)
        {
            bool ok = inventoryGrid.MoveItem(draggingItem, x, y, draggingItem.rotated);

            if (ok)
            {
                Debug.Log($"放下物品：{draggingItem.item.displayName} 到 ({x},{y})");

                StopDrag();         // 先结束拖拽
                RefreshAllItems();  // 再重绘一遍物品位置
            }
            else
            {
                Debug.Log("这个位置放不下该物品");
                // 如果你想“放不下就自动取消拖拽”，可以在这里加 StopDrag();
            }
        }
    }


    private void SelectItem(InventoryItem inst)
    {
        // 先清除之前的
        ClearSelection();

        if (inst == null) return;

        selectedItem = inst;

        if (itemIcons.TryGetValue(inst, out RectTransform iconRT))
        {
            var img = iconRT.GetComponent<Image>();
            if (img != null)
                img.color = selectedColor;
        }
    }

    // 清除当前选中状态
    private void ClearSelection()
    {
        if (selectedItem != null && itemIcons.TryGetValue(selectedItem, out RectTransform iconRT))
        {
            var img = iconRT.GetComponent<Image>();
            if (img != null)
                img.color = normalColor;
        }

        selectedItem = null;
    }

    private void StartDrag(InventoryItem inst)
    {
        ClearPreview();
        hoveredCell = null;

        draggingItem = inst;

        // 1) 原位置那个图标删掉
        originIconForDrag = null;
        if (itemIcons.TryGetValue(inst, out RectTransform originIconRT))
        {
            originIconForDrag = originIconRT;
            originIconForDrag.gameObject.SetActive(false);  
        }

        // 2) 清掉旧的拖拽图标
        if (draggingIcon != null)
        {
            Destroy(draggingIcon.gameObject);
            draggingIcon = null;
        }

        // 3) 创建跟随鼠标的图标
        GameObject go = Instantiate(itemIconPrefab, itemsRoot);
        draggingIcon = go.GetComponent<RectTransform>();

        var img = go.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = inst.item.icon;
            img.raycastTarget = false;  // 不挡点击
        }

        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = inst.count > 1 ? inst.count.ToString() : "";
            text.raycastTarget = false;
        }

        // 4) 设置拖拽图标尺寸（和占用格子一致）
        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        int w = inst.Width;
        int h = inst.Height;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        draggingIcon.sizeDelta = new Vector2(width, height);

        // 初始位置放在鼠标下
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemsRoot,
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPos
        );
        draggingIcon.anchoredPosition = localPos;
    }

    private void StopDrag()
    {
        ClearPreview();
        hoveredCell = null;
        // 恢复原格子里的图标颜色
        if (draggingItem != null && itemIcons.TryGetValue(draggingItem, out RectTransform originIconRT))
        {
            var originImg = originIconRT.GetComponent<Image>();
            if (originImg != null)
            {
                originImg.color = iconNormalColor;
            }
        }

        draggingItem = null;

        if (draggingIcon != null)
        {
            Destroy(draggingIcon.gameObject);
            draggingIcon = null;
        }
    }

    private void RotateDraggingItem()
    {
        if (draggingItem == null || draggingIcon == null) return;

        // 切换 rotated 状态
        draggingItem.rotated = !draggingItem.rotated;

        // 根据新的宽高调整拖拽图标尺寸
        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        int w = draggingItem.Width;
        int h = draggingItem.Height;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        draggingIcon.sizeDelta = new Vector2(width, height);
    }

    // 清除所有格子的高亮（恢复原色）
    private void ClearPreview()
    {
        if (cellUIs == null || inventoryGrid == null) return;

        int w = inventoryGrid.Width;
        int h = inventoryGrid.Height;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var cell = cellUIs[x, y];
                if (cell != null)
                    cell.ResetColor();
            }
        }
    }

    // 根据当前 hoveredCell + draggingItem 更新红/绿预览
    private void UpdatePreview()
    {
        ClearPreview();

        if (draggingItem == null || hoveredCell == null || inventoryGrid == null)
            return;

        int startX = hoveredCell.x;
        int startY = hoveredCell.y;

        // 能不能放：忽略自己占的格子（移动时很重要）
        bool canPlace = inventoryGrid.CanPlace(
            draggingItem.item,
            startX,
            startY,
            draggingItem.rotated,
            draggingItem
        );

        int w = draggingItem.Width;
        int h = draggingItem.Height;

        int gridW = inventoryGrid.Width;
        int gridH = inventoryGrid.Height;

        Color c = canPlace ? previewOkColor : previewBadColor;

        // 给覆盖区域内的格子上色（注意不要越界）
        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                int gx = startX + ix;
                int gy = startY + iy;

                if (gx < 0 || gy < 0 || gx >= gridW || gy >= gridH)
                    continue;

                var cell = cellUIs[gx, gy];
                if (cell != null)
                    cell.SetColor(c);
            }
        }
    }

    public void OnCellHover(InventoryCellUI cell)
    {
        hoveredCell = cell;

        if (draggingItem != null)
        {
            UpdatePreview();
        }
    }
    //测试用
    private IEnumerator DelayedRefresh()
    {
        // 等一帧，让 GridLayoutGroup 自己排版
        yield return null;
        inventoryGrid.PlaceNewItem(testItem,1,0,0,false);
        inventoryGrid.PlaceNewItem(testItem, 1, 0, 4, false);
        RefreshAllItems();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManufactureResultGridView : MonoBehaviour
{
    [Header("逻辑")]
    public ManufactureManager manager;   // 制作管理器，里面有 GetPreviewResult()

    [Header("UI 绑定")]
    public RectTransform cellsRoot;      // 3×3 GridLayoutGroup
    public RectTransform itemsRoot;      // 覆盖在上面的空节点
    public GameObject cellPrefab;        // 小格子 prefab（可以和材料格用同一个或复制一份）
    public GameObject itemIconPrefab;    // 物品图标 prefab（直接用背包那个）

    private const int GRID_SIZE = 3;     // 3×3
    private RectTransform[,] cellRTs;    // 保存每个结果格子的 RectTransform
    private GridLayoutGroup layout;

    private void Awake()
    {
        if (cellsRoot != null)
            layout = cellsRoot.GetComponent<GridLayoutGroup>();
    }

    private void OnEnable()
    {
        BuildCells();
        RefreshPreview();

        if (manager != null)
        {
            manager.RefreshPreviewRequested += RefreshPreview;
        }
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.RefreshPreviewRequested -= RefreshPreview;
        }
    }

    // 生成 3×3 小格子，只负责画背景
    private void BuildCells()
    {
        if (cellsRoot == null || cellPrefab == null) return;

        foreach (Transform child in cellsRoot)
            Destroy(child.gameObject);

        cellRTs = new RectTransform[GRID_SIZE, GRID_SIZE];

        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                var go = Instantiate(cellPrefab, cellsRoot);
                var rt = go.GetComponent<RectTransform>();
                cellRTs[x, y] = rt;
            }
        }
    }

    // 刷新结果预览：按照物品自身占格大小画一块大图标
    public void RefreshPreview()
    {
        Debug.Log("开始生成大结果图");
        if (manager == null || itemsRoot == null || layout == null || cellRTs == null)
            return;

        // 1. 清掉旧图标
        foreach (Transform child in itemsRoot)
            Destroy(child.gameObject);

        // 2. 拿到当前预览结果
        var preview = manager.GetPreviewResult(); 
        if (preview.item == null || preview.count <= 0)
            return;

        ItemSO item = preview.item;
        int count = preview.count;

        // 用 InventoryItem 来复用“多格图标”的算法
        var inst = new InventoryItem(item, count, 0, 0, false); // originX=0,originY=0，从左上角开始

        // === 生成图标（几乎就是 InventoryGridView.CreateIconForItem 的复制品） ===
        GameObject iconGO = Instantiate(itemIconPrefab, itemsRoot);
        RectTransform rt = iconGO.GetComponent<RectTransform>();

        // 图片
        var img = iconGO.GetComponent<Image>();
        if (img != null)
            img.sprite = inst.item.icon;

        // 数量文本（可选）
        var txt = iconGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
            txt.text = inst.count > 1 ? inst.count.ToString() : "";

        // 1) 尺寸：根据物品占格数拉伸
        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        int w = inst.Width;
        int h = inst.Height;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        rt.sizeDelta = new Vector2(width, height);

        // 2) 位置：在 3×3 里占据 w×h 个格子，从左上角开始
        RectTransform cellLT = cellRTs[0, 0];              // 左上格
        RectTransform cellRB = cellRTs[w - 1, h - 1];      // 右下格

        Vector3 worldCenterLT = cellLT.TransformPoint(cellLT.rect.center);
        Vector3 worldCenterRB = cellRB.TransformPoint(cellRB.rect.center);
        Vector3 worldCenter = (worldCenterLT + worldCenterRB) * 0.5f;

        Vector3 localCenter = itemsRoot.InverseTransformPoint(worldCenter);
        rt.anchoredPosition = localCenter;
    }
}

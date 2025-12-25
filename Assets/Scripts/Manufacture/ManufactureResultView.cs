using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ManufactureResultView : MonoBehaviour
{
    [Header("逻辑")]
    public ManufactureManager manager;      // 里面有 GetPreviewResult() / CraftIfPossible()

    [Header("UI 引用")]
    public RectTransform cellsRoot;         // 小格子父节点（有 GridLayoutGroup）
    public RectTransform itemsRoot;         // 图标父节点（和 cellsRoot 完全重合）
    public GameObject cellPrefab;           // 结果区的单个格子 prefab（背景用）
    public GameObject itemIconPrefab;       // 物品图标 prefab（用背包里的那一个）

    private GridLayoutGroup layout;
    private RectTransform[,] cellRTs;       // 记录生成的格子，给图标定位用
    public Button putIntoBagButton;
    public Button throwOutButton;
    private void Awake()
    {
        if (cellsRoot != null)
            layout = cellsRoot.GetComponent<GridLayoutGroup>();
        putIntoBagButton.onClick.AddListener(PutIntoBag);
        throwOutButton.onClick.AddListener(ThrowOut);
    }

    private void OnEnable()
    {
        BuildEmptyGrid(1, 1);   // 先生成一个 1×1，避免空引用
                                // 关键：动态生成格子后，强制让 GridLayoutGroup 立刻布局完成
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cellsRoot);
        Canvas.ForceUpdateCanvases();

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

    // ================== 刷新预览 ==================

    public void RefreshPreview()
    {
        if (manager == null || layout == null || cellsRoot == null || itemsRoot == null)
            return;

        // 清掉旧图标
        foreach (Transform child in itemsRoot)
            Destroy(child.gameObject);

        var preview = manager.GetPreviewResult();
        if (preview.item == null || preview.count <= 0)
        {
            // 没有产物就不画
            BuildEmptyGrid(1, 1);
            return;
        }

        ItemSO item = preview.item;
        int count = preview.count;

        int w = Mathf.Max(1, item.gridWidth);
        int h = Mathf.Max(1, item.gridHeight);
        Debug.Log($"{w},{h}");
        // 重新生成 w×h 个小格子
        BuildEmptyGrid(w, h);
        
        // 生成一张大图标
        CreateIconForResult(item, count, w, h);
    }

    // ================== 生成格子 ==================

    private void BuildEmptyGrid(int w, int h)
    {
        if (cellsRoot == null || cellPrefab == null || layout == null)
            return;

        // 清掉旧格子
        foreach (Transform child in cellsRoot)
            Destroy(child.gameObject);

        cellRTs = new RectTransform[w, h];

        // 让 GridLayout 按列数来排布
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = w;

        for (int iy = 0; iy < h; iy++)
        {
            for (int ix = 0; ix < w; ix++)
            {
                GameObject go = Instantiate(cellPrefab, cellsRoot);
                Button button = go.GetComponent<Button>();
                //button.onClick.AddListener(OnPointerClick);
                RectTransform rt = go.GetComponent<RectTransform>();
                cellRTs[ix, iy] = rt;
            }
        }
    }

    // ================== 生成图标 ==================

    private void CreateIconForResult(ItemSO item, int count, int w, int h)
    {
        if (item == null) return;
        if (itemIconPrefab == null || itemsRoot == null || layout == null) return;

        GameObject iconGO = Instantiate(itemIconPrefab, itemsRoot);
        RectTransform rt = iconGO.GetComponent<RectTransform>();
        if (rt == null) return;

        // sprite
        var img = iconGO.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.sprite = item.icon;
            img.raycastTarget = false;
        }

        Vector2 cellSize = layout.cellSize;
        Vector2 spacing = layout.spacing;

        float width = cellSize.x * w + spacing.x * (w - 1);
        float height = cellSize.y * h + spacing.y * (h - 1);
        rt.sizeDelta = new Vector2(width, height);

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }




    // ================== 点击：尝试把产物放进背包 ==================

    public void PutIntoBag()
    {
        if (manager == null)
            return;
        /*
        // 沿用你之前的逻辑：成功则消耗材料并放入背包，预览会通过事件重新刷新
        if (manager.CraftIfPossible())
        {
            Debug.Log("创造");
            RefreshPreview();
        }*/
        if (manager.TryGiveOutputToInventory(manager.result,1))
        {
            manager.SetPanel(true, false);
        }
        else
        {
            Debug.Log("放不进哦！");
        }
    }

    public void ThrowOut()
    {

    }
}

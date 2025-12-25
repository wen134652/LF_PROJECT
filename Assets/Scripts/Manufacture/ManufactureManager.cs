using System;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureManager : MonoBehaviour
{
    [Header("配方列表")]
    public List<ManufactureRecipeSO> recipes = new List<ManufactureRecipeSO>();

    [Header("引用")]
    public ManufactureGrid materialGrid;
    public InventoryGrid playerInventory;

    [Header("当前容器 / 工具（UI 改这两个就行）")]
    public ItemSO currentContainer;
    public ItemSO currentTool;

    public ItemSO defaultContainer;
    // 给 UI 用：需要刷新预览时回调
    public System.Action RefreshPreviewRequested;
    public Action RefreshContainer;

    [Header("材料面板/结果面板")]
    public GameObject materialPanel;
    public GameObject resultPanel;

    [Header("默认制作结果")]
    public ManufactureRecipeSO defaultResult;
    public ItemSO result;

    private ManufactureRecipeSO currentRecipe;
    private void Awake()
    {
        SetContainer(defaultContainer);
        if (materialGrid != null)
        {
            materialGrid.width = currentContainer.containerWidth;
            materialGrid.height = currentContainer.containerHeight;
            materialGrid.OnChanged += () =>
            {
                RefreshPreviewRequested?.Invoke();
            };
        }
        result = defaultResult.outputItem;
    }

    private void OnDisable()
    {
        SetContainer(defaultContainer);
    }

    // ====== 容器 / 工具接口 ======

    public void SetContainer(ItemSO container)
    {
        currentContainer = container;
        materialGrid.width = currentContainer.containerWidth;
        materialGrid.height = currentContainer.containerHeight;
        RefreshContainer?.Invoke();
        RefreshPreviewRequested?.Invoke();
    }

    public void ClearContainer()
    {
        currentContainer = defaultContainer;
        RefreshPreviewRequested?.Invoke();
    }

    public void SetTool(ItemSO tool)
    {
        currentTool = tool;

        RefreshPreviewRequested?.Invoke();
    }

    public void ClearTool()
    {
        currentTool = null;
        RefreshPreviewRequested?.Invoke();
    }

    // ====== 1. 找匹配配方 ======

    public ManufactureRecipeSO FindMatchingRecipe()
    {
        if (materialGrid == null) return null;

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            if (IsRecipeMatch(recipe))
                return recipe;
        }

        return null;
    }

    private bool IsRecipeMatch(ManufactureRecipeSO recipe)
    {
        // 1) 容器
        if (recipe.requiredContainer != null &&
            recipe.requiredContainer != currentContainer)
            return false;

        // 2) 工具
        if (recipe.requiredTool != null &&
            recipe.requiredTool != currentTool)
            return false;

        // 3) 材料图案（允许在材料区里滑动）
        int rw = recipe.width;
        int rh = recipe.height;

        int gw = materialGrid.width;
        int gh = materialGrid.height;

        if (rw > gw || rh > gh)
            return false;

        for (int offsetY = 0; offsetY <= gh - rh; offsetY++)
        {
            for (int offsetX = 0; offsetX <= gw - rw; offsetX++)
            {
                if (PatternMatchesAtOffset(recipe, offsetX, offsetY) &&
                    OtherCellsEmptyOutsidePattern(recipe, offsetX, offsetY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool PatternMatchesAtOffset(ManufactureRecipeSO recipe, int offsetX, int offsetY)
    {
        int rw = recipe.width;
        int rh = recipe.height;

        for (int y = 0; y < rh; y++)
        {
            for (int x = 0; x < rw; x++)
            {
                ItemSO need = recipe.GetRequiredItem(x, y);

                var inst = materialGrid.GetItemAt(offsetX + x, offsetY + y);
                ItemSO have = inst != null ? inst.item : null;

                if (need == null && have == null) continue;
                if (need == null && have != null) return false;
                if (need != null && have == null) return false;
                if (need != null && have != need) return false;
            }
        }

        return true;
    }

    // 检查“配方区域之外”的格子是不是全空
    private bool OtherCellsEmptyOutsidePattern(ManufactureRecipeSO recipe, int offsetX, int offsetY)
    {
        int rw = recipe.width;
        int rh = recipe.height;

        int gw = materialGrid.width;
        int gh = materialGrid.height;

        for (int y = 0; y < gh; y++)
        {
            for (int x = 0; x < gw; x++)
            {
                bool insidePattern =
                    (x >= offsetX && x < offsetX + rw &&
                     y >= offsetY && y < offsetY + rh);

                if (insidePattern) continue;

                var inst = materialGrid.GetItemAt(x, y);
                if (inst != null)
                    return false;
            }
        }

        return true;
    }

    // ====== 2. 预览结果 ======

    [System.Serializable]
    public struct ManufacturePreview
    {
        public ItemSO item;
        public int count;
    }

    public ManufacturePreview GetPreviewResult()
    {

        return new ManufacturePreview
        {
            item = currentRecipe.outputItem,
            count = currentRecipe.outputCount
        };
    }

    // ====== 3. 执行合成 ======

    public bool CraftIfPossible()
    {
        if (materialGrid == null || playerInventory == null)
        {
            Debug.LogWarning("ManufactureManager: materialGrid 或 playerInventory 未设置");
            return false;
        }
        if (materialGrid.items.Count==0)
        {
            Debug.Log("当前材料格为空，巧妇难为无米之炊");
            return false;
        }
        var recipe = FindMatchingRecipe();
        if (recipe == null)
        {
            Debug.Log("当前排列不匹配任何配方,消耗材料制作出垃圾");
            recipe = defaultResult;
        }
        /*
        // 把产物放进背包
        bool ok = TryGiveOutputToInventory(recipe.outputItem, recipe.outputCount);
        if (!ok)
        {
            Debug.Log("背包装不下制作结果");
            return false;
        }
        */
        ConsumeMaterials();
        result = recipe.outputItem;
        currentRecipe = recipe;
        // 刷新预览
        RefreshPreviewRequested?.Invoke();

        Debug.Log($"制作成功：{recipe.displayName}");
        return true;
    }

    public bool TryGiveOutputToInventory(ItemSO item, int count)
    {
        if (item == null || count <= 0) return false;

        /*// 1x1 且可堆叠：用背包里的堆叠接口
        if (item.gridWidth == 1 && item.gridHeight == 1 && item.maxStack > 1)
        {
            int left = playerInventory.TryAddStackable(item, count);
            return left == 0;
        }
        else
        {
            // 多格物品：简单起见，先尝试放在 (0,0)
            var inst = playerInventory.PlaceNewItem(item, count, 0, 0, false);
            return inst != null;
        }*/
        var inst = playerInventory.PlaceNewItemWithNoPosition(item);
        return inst != null;
    }
    /*
    private void ConsumeMaterials(ManufactureRecipeSO recipe)
    {
        int rw = recipe.width;
        int rh = recipe.height;

        int gw = materialGrid.width;
        int gh = materialGrid.height;

        // 先找到匹配的偏移
        int matchOffsetX = -1;
        int matchOffsetY = -1;

        for (int offsetY = 0; offsetY <= gh - rh; offsetY++)
        {
            for (int offsetX = 0; offsetX <= gw - rw; offsetX++)
            {
                if (PatternMatchesAtOffset(recipe, offsetX, offsetY) &&
                    OtherCellsEmptyOutsidePattern(recipe, offsetX, offsetY))
                {
                    matchOffsetX = offsetX;
                    matchOffsetY = offsetY;
                    break;
                }
            }

            if (matchOffsetX != -1) break;
        }

        if (matchOffsetX == -1)
            return;

        var removed = new HashSet<InventoryItem>();

        for (int y = 0; y < rh; y++)
        {
            for (int x = 0; x < rw; x++)
            {
                ItemSO need = recipe.GetRequiredItem(x, y);
                if (need == null) continue;

                var inst = materialGrid.GetItemAt(matchOffsetX + x, matchOffsetY + y);
                if (inst == null) continue;

                if (!removed.Contains(inst))
                {
                    materialGrid.RemoveItem(inst);
                    removed.Add(inst);
                }
            }
        }
    }*/
    private void ConsumeMaterials()
    {
        materialGrid.CleanGrid();
        currentRecipe = defaultResult;
        result = currentRecipe.outputItem;
    }
    public void SetPanel(bool manufacturePanel,bool reusltPanelActavie)
    {
        materialPanel.SetActive(manufacturePanel);
        resultPanel.SetActive(reusltPanelActavie);
    }
}

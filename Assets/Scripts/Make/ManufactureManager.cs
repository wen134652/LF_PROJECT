using System.Collections.Generic;
using UnityEngine;

public class ManufactureManager : MonoBehaviour
{
    [Header("配方列表")]
    public List<ManufactureRecipeSO> recipes = new List<ManufactureRecipeSO>();

    [Header("引用")]
    public ManufactureGrid materialGrid;
    public InventoryGrid playerInventory;

    [Header("当前放入的容器 / 工具")]
    public ItemSO currentContainer;
    public ItemSO currentTool;

    // ========== 供 UI 调用：放/取容器、工具 ==========
    public void SetContainer(ItemSO container)
    {
        currentContainer = container;
        // 容器变化后，材料格大小后续可以在这里调整（当前版本先不改 grid 尺寸）
        RefreshPreviewRequested?.Invoke();
    }

    public void ClearContainer()
    {
        currentContainer = null;
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

    // 给 UI 用：当需要刷新预览时回调
    public System.Action RefreshPreviewRequested;

    private void Awake()
    {
        if (materialGrid != null)
        {
            // 材料变化时也刷新预览
            materialGrid.OnChanged += () =>
            {
                RefreshPreviewRequested?.Invoke();
            };
        }
    }

    // ========== 1. 找到当前匹配的配方 ==========
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
        // 1) 检查容器
        if (recipe.requiredContainer != null &&
            recipe.requiredContainer != currentContainer)
            return false;

        // 2) 检查工具
        if (recipe.requiredTool != null &&
            recipe.requiredTool != currentTool)
            return false;

        // 3) 滑动匹配材料图案
        int rw = recipe.width;
        int rh = recipe.height;

        int gw = materialGrid.width;
        int gh = materialGrid.height;

        // 如果配方比材料格还大，肯定不行
        if (rw > gw || rh > gh)
            return false;

        // 尝试所有可能的偏移位置
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

    // 在给定偏移 (offsetX, offsetY) 处检查配方区域是否完全匹配
    private bool PatternMatchesAtOffset(ManufactureRecipeSO recipe, int offsetX, int offsetY)
    {
        int rw = recipe.width;
        int rh = recipe.height;

        for (int y = 0; y < rh; y++)
        {
            for (int x = 0; x < rw; x++)
            {
                ItemSO need = recipe.GetRequiredItem(x, y);
                InventoryItem inventoryItem = materialGrid.GetItemAt(offsetX + x, offsetY + y);
                ItemSO have = inventoryItem?.item; // 从 InventoryItem 获取 ItemSO

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

                InventoryItem inventoryItem = materialGrid.GetItemAt(x, y);
                ItemSO have = inventoryItem?.item; // 从 InventoryItem 获取 ItemSO
                if (have != null)
                    return false; // 图案外不能有额外材料
            }
        }

        return true;
    }


    // ========== 2. 给 UI 用的预览结果 ==========
    [System.Serializable]
    public struct ManufacturePreview
    {
        public ItemSO item;
        public int count;
    }

    public ManufacturePreview GetPreviewResult()
    {
        var recipe = FindMatchingRecipe();
        if (recipe == null || recipe.outputItem == null || recipe.outputCount <= 0)
        {
            return new ManufacturePreview { item = null, count = 0 };
        }

        return new ManufacturePreview
        {
            item = recipe.outputItem,
            count = recipe.outputCount
        };
    }

    // ========== 3. 真正执行一次合成 ==========
    public bool CraftIfPossible()
    {
        if (materialGrid == null || playerInventory == null)
        {
            Debug.LogWarning("ManufactureManager: materialGrid 或 playerInventory 未设置");
            return false;
        }

        var recipe = FindMatchingRecipe();
        if (recipe == null)
        {
            Debug.Log("当前排列不匹配任何配方");
            return false;
        }

        // 把产物放进背包
        bool ok = TryGiveOutputToInventory(recipe.outputItem, recipe.outputCount);
        if (!ok)
        {
            Debug.Log("背包装不下制作结果");
            return false;
        }

        // 消耗材料（当前简化为每格 1 个，后面想做数量再扩展）
        ConsumeMaterials(recipe);

        // 刷新预览
        RefreshPreviewRequested?.Invoke();

        Debug.Log($"制作成功：{recipe.displayName}");
        return true;
    }

    private bool TryGiveOutputToInventory(ItemSO item, int count)
    {
        if (item == null || count <= 0) return false;


        // 多格物品：简单起见，先尝试放在 (0,0)
        var inst = playerInventory.PlaceNewItem(item, count, 0, 0, false);
        return inst != null;
        
    }

    private void ConsumeMaterials(ManufactureRecipeSO recipe)
    {
        int rw = recipe.width;
        int rh = recipe.height;

        // 需要找到匹配的配方位置（与 FindMatchingRecipe 中的逻辑一致）
        // 这里简化处理：假设配方从 (0,0) 开始，实际应该使用匹配时的偏移量
        // 为了正确实现，应该保存匹配时的偏移量，或者重新查找匹配位置
        
        // 临时方案：遍历所有可能的偏移位置，找到匹配的位置后消耗材料
        int gw = materialGrid.width;
        int gh = materialGrid.height;
        
        for (int offsetY = 0; offsetY <= gh - rh; offsetY++)
        {
            for (int offsetX = 0; offsetX <= gw - rw; offsetX++)
            {
                if (PatternMatchesAtOffset(recipe, offsetX, offsetY) &&
                    OtherCellsEmptyOutsidePattern(recipe, offsetX, offsetY))
                {
                    // 找到匹配位置，消耗材料
                    for (int y = 0; y < rh; y++)
                    {
                        for (int x = 0; x < rw; x++)
                        {
                            ItemSO need = recipe.GetRequiredItem(x, y);
                            if (need != null)
                            {
                                // 删除该位置的物品（会删除整个 InventoryItem）
                                materialGrid.RemoveItemAtCell(offsetX + x, offsetY + y);
                            }
                        }
                    }
                    return; // 只消耗一次
                }
            }
        }
    }
}

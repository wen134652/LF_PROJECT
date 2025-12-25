using UnityEngine;

[CreateAssetMenu(menuName = "配方")]
public class ManufactureRecipeSO : ScriptableObject
{
    [Header("基础信息")]
    public string id;
    public string displayName;

    [Header("容器 / 工具要求（可为空）")]
    [Tooltip("如果为 null，表示任何容器都可以")]
    public ItemSO requiredContainer;

    [Tooltip("如果为 null，表示不要求工具")]
    public ItemSO requiredTool;

    [Header("材料格子图案（位置有要求）")]
    [Range(1, 3)] public int width = 2;   // 配方占用格子宽（最多 3）
    [Range(1, 3)] public int height = 2;  // 配方占用格子高（最多 3）

    // 长度 >= width * height，按行存：index = y * width + x
    [Tooltip("按行从上到下，从左到右：index = y * width + x")]
    public ItemSO[] pattern = new ItemSO[9];

    [Header("输出")]
    public ItemSO outputItem;
    public int outputCount = 1;

    public ItemSO GetRequiredItem(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        int index = y * width + x;
        if (index < 0 || index >= pattern.Length) return null;
        return pattern[index];
    }
}

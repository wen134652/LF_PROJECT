using UnityEngine;

public enum ItemType
{
    Food,
    Material,
    Tool,
    Other//有其他需要之后再改
}
[CreateAssetMenu(
    fileName = "NewItem",
    menuName ="Item",
    order = 0
    )]
public class ItemSO : ScriptableObject
{
    [Header("基础信息")]
    public string id;
    public string displayName;
    [TextArea] public string description;

    [Header("外观")]
    public Sprite icon;

    [Header("类型")]
    public ItemType itemType = ItemType.Material;

    [Tooltip("最大堆叠数量")]
    public int maxStack = 1;

    [Header("背包占用格子大小")]
    [Min(1)] public int gridWidth = 1;
    [Min(1)] public int gridHeight = 1;
    public bool canRotate = true;

    // =============== 腐败相关（时间） ===============
    [Header("腐败 / 变质设置")]
    [Tooltip("是否会随时间腐败（例如：生肉、熟食）")]
    public bool perishable = false;

    [Tooltip("从新鲜到完全腐败的时间（游戏内秒数）")]
    [Min(0f)] public float shelfLifeSeconds = 0f;

    [Tooltip("腐败完成后是否直接销毁")]
    public bool destroyWhenRotten = true;

    [Tooltip("若不销毁，则腐败后变成的物品（例如 RottenMeat）")]
    public ItemSO rottenResultItem;

    [Tooltip("腐败后的Icon")]
    public Sprite rottenIcon;

    [Tooltip("X=腐败进度(0~1)，Y=品质(0~1)")]//之后可能可以用来做腐败曲线这样
    public AnimationCurve freshnessCurve;

    // =============== 耐久相关（使用次数） ===============
    [Header("耐久 / 使用损坏设置")]
    [Tooltip("是否有耐久度（使用次数限制），例如工具、武器")]
    public bool hasDurability = false;

    [Tooltip("最大耐久（总使用次数或耐久值上限）")]
    [Min(1)] public int maxDurability = 100;

    [Tooltip("每次使用掉多少耐久，一般是1")]
    [Min(1)] public int durabilityCostPerUse = 1;

    [Tooltip("耐久归零时是否直接消失")]
    public bool destroyWhenBroken = true;

    [Tooltip("若不直接消失，则损坏时变成的物品，例如：破碎工具、废铁")]
    public ItemSO brokenResultItem;

}

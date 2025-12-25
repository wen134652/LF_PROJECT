using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ContainerSlot: MonoBehaviour, IPointerClickHandler
{
    [Header("逻辑引用")]
    public InventoryGrid playerInventory;      // 左边背包逻辑
    public InventoryGridView inventoryView;    // 用来读拖拽状态
    public ManufactureManager manager;          //记录现在使用的容器

    [Header("UI")]
    public RectTransform iconRoot;            // 图标的父节点（可以就是自己）
    public GameObject itemIconPrefab;         // 和背包用的一样的 icon prefab
    public Image icon;

    private ItemSO equippedItem;              // 当前装备的容器
    private RectTransform iconRT;             // 显示容器图标的 RT



    public bool HasTool => equippedItem != null;
    public ItemSO CurrentTool => equippedItem;

    private void OnEnable()
    {
        icon.gameObject.SetActive(false);
        manager = gameObject.GetComponentInParent<ManufactureManager>();
        manager.ClearContainer();
        ClearSlot();
        Equip(manager.defaultContainer);
    }
    private void OnDisable()
    {
        if (equippedItem!=manager.defaultContainer)
        {
            InventoryItem backOk = playerInventory.PlaceNewItemWithNoPosition(equippedItem);
        }
        
    }
    bool CanAccept(ItemSO so)
    {
        if (so == null) return false;
        // 你根据自己 ItemSO 的定义改这一行判断
        return so.itemType == ItemType.Container;//只能放进容器
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("点一下");
        if (inventoryView != null && inventoryView.IsDraggingItem)// 正在从背包拖一个物品过来
        {
            ItemSO so = inventoryView.GetDraggingItemSO();
            if (!CanAccept(so))
                return;

            // 如果已经有容器，就放不进去
            if (manager.currentContainer != manager.defaultContainer && playerInventory != null)
            {
                
                Debug.Log("已经装备了容器了");
                return;
            }
            /*
            // 消耗拖拽堆里的一件
            bool consumed = inventoryView.ConsumeOneFromDraggingForExternal();
            if (!consumed) return;
            */
            Equip(so);
        }
        else
        {
            // 没有拖东西，点击容器格：如果有容器就尝试放回背包
            if (manager.currentContainer != manager.defaultContainer && playerInventory != null)
            {
                InventoryItem backOk = playerInventory.PlaceNewItemWithNoPosition(equippedItem);
                if (backOk!=null)
                {
                    ClearSlot();
                }
                else
                {
                    Debug.Log("背包放不下容器，无法收回");
                }
            }
        }
    }

    private void Equip(ItemSO so)
    {
        icon.gameObject.SetActive(true);
        Debug.Log("放入容器！");
        inventoryView.ConsumeOneFromDraggingForExternal();
        equippedItem = so;
        manager.SetContainer(so);
        // 建一个 icon
        if (iconRoot == null) iconRoot = GetComponent<RectTransform>();

        if (iconRT != null)
        {
            Destroy(iconRT.gameObject);
            iconRT = null;
        }

        GameObject go = Instantiate(itemIconPrefab, iconRoot);
        iconRT = go.GetComponent<RectTransform>();

        var img = go.GetComponent<Image>();
        if (img != null)
            img.sprite = so.icon;
        icon.sprite = so.icon;
        // 让图标充满整个容器格
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        iconRT.localRotation = Quaternion.identity;
    }

    private void ClearSlot()
    {
        equippedItem = null;
        if (iconRT != null)
        {
            Equip(manager.defaultContainer);
            //Destroy(iconRT.gameObject);
            //iconRT = null;
            //icon.sprite = null;
            //icon.gameObject.SetActive(false);
        }
    }
}

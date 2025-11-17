using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class InventoryCellUI : MonoBehaviour,IPointerEnterHandler
{
    public int x;
    public int y;

    [HideInInspector]
    public InventoryGridView owner;

    private Button button;
    private Image image;
    private Color baseColor;
    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        if (image != null)
            baseColor = image.color;

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (owner != null)
            owner.OnCellClicked(this);
    }

    // 鼠标移入格子时，通知 View
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null)
            owner.OnCellHover(this);
    }

    // 给格子上色（红/绿）
    public void SetColor(Color c)
    {
        if (image != null)
            image.color = c;
    }

    // 恢复原始颜色
    public void ResetColor()
    {
        if (image != null)
            image.color = baseColor;
    }
}

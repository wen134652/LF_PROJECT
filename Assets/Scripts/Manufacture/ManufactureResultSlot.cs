using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManufactureResultSlot : MonoBehaviour
{
    [Header("引用")]
    public ManufactureManager manager;

    [Header("UI 元素")]
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClickResult);
    }

    private void OnEnable()
    {
        RefreshPreview();
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.RefreshPreviewRequested -= RefreshPreview;
    }

    private void Start()
    {
        //if (manager != null)
            //manager.RefreshPreviewRequested += RefreshPreview;
    }

    public void RefreshPreview()
    {
        if (manager == null || iconImage == null || countText == null) return;

        var preview = manager.GetPreviewResult();

        if (preview.item == null || preview.count <= 0)
        {
            iconImage.enabled = false;
            countText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = preview.item.icon;
            countText.text = preview.count > 1 ? preview.count.ToString() : "";
        }
    }

    private void OnClickResult()
    {
        if (manager == null) return;

        bool ok = manager.CraftIfPossible();
        // 成功与否都刷新一下（材料/结果可能改变）
        RefreshPreview();
    }
}

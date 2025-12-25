using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManufactureItemIconView : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    public void SetData(Sprite sprite, int count)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }

        if (countText != null)
        {
            countText.text = count > 1 ? count.ToString() : "";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeButton : MonoBehaviour
{
    private Button button;
    public ManufactureManager manufactureManager;
    public GameObject resultPanel;
    public GameObject manufacturePanel;
    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (manufactureManager.CraftIfPossible())
        {
            manufactureManager.SetPanel(false, true);
        }
    }
}

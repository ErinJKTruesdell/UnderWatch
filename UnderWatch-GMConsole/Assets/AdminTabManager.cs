using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminTabManager : MonoBehaviour
{
    // Start is called before the first frame update

    public Dictionary<string, GameObject> tabs;
    TabButton currentlySelected;
    public Color selectedColor;
    public Color deselectedColor;
    public TabButton selectFirst;

    void Start()
    {
        TabButton[] buttons = GameObject.FindObjectsOfType<TabButton>();
        tabs = new Dictionary<string, GameObject>();
        foreach (TabButton button in buttons)
        {
            tabs.Add(button.tabName, button.tabObject);
            button.tabImage.color = deselectedColor;
        }
        switchTab(selectFirst);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void switchTab(TabButton tabButton)
    {

        if(currentlySelected != null)
        {
            tabs[currentlySelected.tabName].SetActive(false);
            currentlySelected.tabImage.color = deselectedColor;

        }

        currentlySelected = tabButton;
        currentlySelected.sendActiveEvent();
        tabs[currentlySelected.tabName].SetActive(true);
        currentlySelected.tabImage.color = selectedColor;
    }
}

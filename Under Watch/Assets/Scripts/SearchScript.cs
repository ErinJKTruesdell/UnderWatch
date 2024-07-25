using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mopsicus.InfiniteScroll;

public class SearchScript : MonoBehaviour
{
    public GameObject ContentHolder;
    //change to a list for infinite space
    public GameObject[] Element;

    public GameObject SearchBar;

    public InfiniteScroll infiniteScroll;

    public int totalElements;
      
    void Start()
    {
        totalElements = ContentHolder.transform.childCount;

        Element = new GameObject[totalElements];

        for (int i = 0; i < totalElements; i++)
        {
            Element[i] = ContentHolder.transform.GetChild(i).gameObject;
        }
    }

    public void Search()
    {
        string SearchText = SearchBar.GetComponent<TMP_InputField>().text;
        int searchTxtLength = SearchText.Length;

        int searchedElements = 0;

        foreach (GameObject ele in Element)
        {
            searchedElements++;

            if (ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Length >= searchTxtLength)
            {
                if (SearchText.ToLower() == ele.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Substring(0, searchTxtLength).ToLower())
                {
                    ele.SetActive(true);
                    //scroll to top
                }
                else
                {
                    ele.SetActive(false);
                }
            }
        }
    }
}

using System.Collections.Generic;
using Mopsicus.InfiniteScroll;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Demo2 : MonoBehaviour
{

    [SerializeField]
    private InfiniteScroll Scroll;

    [SerializeField]
    private int Count = 7;

    [SerializeField]
    private int PullCount = 7;

    private List<int> _list = new List<int>();

    public Texture sampleTex;

    public SocialFeedDatabase sfd;

    public int screenWidth = 500;

    void Start()
    {

        screenWidth = Screen.width;
        //Debug.Log("SCREEN WITDTH: " + screenWidth);
        Scroll.OnFill += OnFillItem;
        Scroll.OnHeight += OnHeightItem;
        Scroll.OnPull += OnPullItem;
        for (int i = 0; i < Count; i++)
        {
            _list.Add(i);
        }
        Scroll.InitData(_list.Count);
        if (sfd == null)
        {
            sfd = FindObjectOfType<SocialFeedDatabase>();
        }
            if (sfd == null)
        {
            sfd = new SocialFeedDatabase();
        }
    }

    void OnFillItem(int index, GameObject item)
    {
        //database code here
        //this may be where we put in the emoji react stuff?
        ListItem li = item.GetComponentInChildren<ListItem>();
        sfd.getNextPost(li.photoImg, li.profImage, li.unText);
        //item.GetComponentInChildren<Text>().text = sfo.username;
        //item.GetComponentInChildren<RawImage>().texture = sfo.photo;
    }

    int OnHeightItem(int index)
    {
        return 509;
    }

    void OnPullItem(InfiniteScroll.Direction direction)
    {
        int index = _list.Count;
        if (direction == InfiniteScroll.Direction.Top)
        {
            for (int i = 0; i < PullCount; i++)
            {
                _list.Insert(0, index);
                index++;
            }
        }
        else
        {
            for (int i = 0; i < PullCount; i++)
            {
                _list.Add(index);
                index++;
            }
        }
        Scroll.ApplyDataTo(_list.Count, PullCount, direction);
    }

    public void SceneLoad(int index)
    {
        SceneManager.LoadScene(index);
    }

}
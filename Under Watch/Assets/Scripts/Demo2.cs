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

    SocialFeedDatabase sfd;

    void Start()
    {
        Scroll.OnFill += OnFillItem;
        Scroll.OnHeight += OnHeightItem;
        Scroll.OnPull += OnPullItem;
        for (int i = 0; i < Count; i++)
        {
            _list.Add(i);
        }
        Scroll.InitData(_list.Count);

        sfd = GameObject.FindObjectOfType<SocialFeedDatabase>();
        if(sfd == null)
        {
            sfd = new SocialFeedDatabase();
        }
    }

    void OnFillItem(int index, GameObject item)
    {
        //database code here
        SocialFeedObject sfo = sfd.getNextPost();
        item.GetComponentInChildren<Text>().text = sfo.username;
        item.GetComponentInChildren<RawImage>().texture = sfo.photo;
    }

    int OnHeightItem(int index)
    {
        return 500;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBarButton : MonoBehaviour
{
    public string sceneName = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveToScene()
    {
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        gm.ProgressToScene(sceneName);
        
    }
}

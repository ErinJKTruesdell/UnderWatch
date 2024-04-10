using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBarButton : MonoBehaviour
{
    public int sceneIndex = 0;

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
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

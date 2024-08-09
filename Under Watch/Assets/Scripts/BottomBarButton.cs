using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BottomBarButton : MonoBehaviour
{
    public string sceneName = "";

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;

    /* private void Update()
     {
         if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
         {
             startTouchPosition = Input.GetTouch(0).position;
         }

         if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
         {
             endTouchPosition = Input.GetTouch(0).position;

             if (endTouchPosition.x < startTouchPosition.x)
             {
                 swipeScene(SceneManager.GetActiveScene().buildIndex, 1);
             }

             if (endTouchPosition.x > startTouchPosition.x)
             {
                 swipeScene(SceneManager.GetActiveScene().buildIndex, -1);
             }
         }
     }
    */

    public void moveToScene()
    {
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        gm.ProgressToScene(sceneName);       
    }

    public void swipeScene(int scene, int isLeftOrRight)
    {
        scene += isLeftOrRight;

        if (scene >= 3 && scene <= 7)
        {
            GameManager gm = GameObject.FindObjectOfType<GameManager>();
            SceneManager.LoadScene(scene);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SC_LoginSystem scls;
    // Start is called before the first frame update
    void Start()
    {
        scls = GameObject.FindObjectOfType<SC_LoginSystem>();
        if (scls == null)
        {
            scls = new SC_LoginSystem();
        }

        scls.gm = this;

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProgressToScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ForgotPassword()
    {
        ProgressToScene("ForgotPassword");
    }
}

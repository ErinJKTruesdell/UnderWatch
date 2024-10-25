using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class LoginUITween : MonoBehaviour
{
    public GameObject loginButton;
    public GameObject registerButton;
    public GameObject snapGramLogo;

    private void Awake()
    {
        //can change any settings inside the init
        DOTween.Init();
    }

    void Start()
    {
        loginButton.transform.DOLocalMoveY(-900f, .7f).From().SetEase(Ease.OutQuad);
        registerButton.transform.DOLocalMoveY((-900f - 138f), .7f).From().SetEase(Ease.OutQuad); 
        snapGramLogo.transform.DOLocalMoveY(900f, .7f).From().SetEase(Ease.OutQuad); 
    }


}

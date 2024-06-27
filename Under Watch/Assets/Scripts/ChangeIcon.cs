using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Member;
using UnityEditor.iOS;
using UnityEditor.Build;
using System;



public class ChangeIcon : MonoBehaviour
{
    public static Texture2D sourceTex; 
    public static Texture2D destinationTex;
    public static Texture2D sourceTex2;

    public Texture2D[] appleTexs;

    public string nameIcon = "default";
    //TODO: make this save to the server.
    public int day = 0;
    //remove for prod
    public bool testing = true;

    public DateTime quitDate;
    System.DateTime moment = new();

    bool isPaused = false;

    public void Awake()
    {
        destinationTex = (Texture2D)Resources.Load("iconImage");
        //source = Resources.Load<Texture2D>("iconimage");
        //destination = Resources.Load<Texture2D>("day2icon");
    }

    public void Start()
    {
        //remove for prod
        //needs testing on apple device.... uh....
        if (moment.Day > quitDate.Day || (testing))
        {
            day++;
            nameIcon = $"v{day}";
            SetAppleIconAlt(nameIcon);
            Debug.Log($"iconVer: {nameIcon}");
            Debug.Log($"A new day dawns ({moment.Day})");    
        }
    }

    public void Update()
    {
        if (isPaused)
        {
            quitDate = DateTime.Now;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }

    // Setting all `App` icons for iOS
    public void SetAppleIconAlt(string iconName)
    {
        AppIconChanger.iOS.SetAlternateIconName(iconName);
    }
    public void SetAppleIcons(Texture2D[] textures)
    {
        NamedBuildTarget platform = NamedBuildTarget.iOS;
        PlatformIconKind kind = iOSPlatformIconKind.Application;

        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, kind);

        //Assign textures to each available icon slot.
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].SetTextures(textures[i]);
        }
        PlayerSettings.SetPlatformIcons(platform, kind, icons);
    }

    [MenuItem("AssetDatabase/CopyAndroid")]
    public static void CopyAndroid()
    {
        //delete old asset
        if (AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(destinationTex)))
        {
            Debug.Log("Deleted the old texture");
        }

        // Copy next asset in line into old asset's filepath
        if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(sourceTex), "Assets/Resources/iconImage1.png"))
        {
            Debug.Log("texture asset copied as Assets/Resources/day2Icon.png");
        }
        else
        {
            Debug.Log("Couldn't copy the texture");
        }

        //Texture2D destinationTex = AssetDatabase.LoadAssetAtPath("Assets/Resources/day2Icon.png", typeof(Texture2D)) as Texture2D;
        // Manually refresh the Database to inform of a change
        AssetDatabase.Refresh();
    }
}

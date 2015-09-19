/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// Showcase GUI for navigating between scenes.
/// <summary>
public class ShowcaseGUI : MonoBehaviour
{
    private static ShowcaseGUI instance;
    private int levels = 8;

    void Start()
    {
        if (instance)
            Destroy(gameObject);

        instance = this;
        DontDestroyOnLoad(gameObject);
        OnLevelWasLoaded(0);
    }


    void OnLevelWasLoaded(int level)
    {
        GameObject floor = GameObject.Find("Floor_Tile");
        if (floor)
        {
            foreach (Transform trans in floor.transform)
            {
                trans.gameObject.SetActive(true);
            }
        }
    }


    void OnGUI()
    {
        int width = Screen.width;
        int buttonW = 30;
        int buttonH = 40;

        Rect leftRect = new Rect(width - buttonW * 2 - 70, 10, buttonW, buttonH);
        if (Application.loadedLevel > 0 && GUI.Button(leftRect, "<"))
            Application.LoadLevel(Application.loadedLevel - 1);
        else if (GUI.Button(new Rect(leftRect), "<"))
            Application.LoadLevel(levels - 1);

        GUI.Box(new Rect(width - buttonW - 70, 10, 60, buttonH),
                "Scene:\n" + (Application.loadedLevel + 1) + " / " + levels);

        Rect rightRect = new Rect(width - buttonW - 10, 10, buttonW, buttonH);
        if (Application.loadedLevel < levels - 1 && GUI.Button(new Rect(rightRect), ">"))
            Application.LoadLevel(Application.loadedLevel + 1);
        else if (GUI.Button(new Rect(rightRect), ">"))
            Application.LoadLevel(0);
    }
}

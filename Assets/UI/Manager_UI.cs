using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_UI : MonoBehaviour
{
    [Header("Settings")]
    public float BlackScreen_FadeTime_Total = 2f;
    public float BlackScreen_FadeTime_RemainBlack = 0.33f;
    private float BlackScreen_FadeTime_Timer = -1f;
    private float BlackScreen_FadeTime_Half { get { return (BlackScreen_FadeTime_Total - BlackScreen_FadeTime_RemainBlack) / 2; } }

    [Header("Assign")]
    public Image BlackScreen;


    // Start is called before the first frame update
    void Start()
    {
        BlackScreen.enabled = true;
        BlackScreen.CrossFadeAlpha(0, 0, false);

    }

    // Update is called once per frame
    void Update()
    {

        if (BlackScreen_FadeTime_Timer != -1) // All of the timer shenanigans
        {
            BlackScreen_FadeTime_Timer -= Time.deltaTime;

            if (BlackScreen_FadeTime_Timer < 0)
            {
                BlackScreen_FadeTime_Timer = -1;
                BlackScreen.CrossFadeAlpha(0, BlackScreen_FadeTime_Half, false);
            }
        }
    }

    public void AdjustScreen()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");


        int playerCount = Mathf.Min(players.Length, 4);




        for (int i = 0; i < playerCount; i++)
        {
            Transform currentPlayer = players[i].transform;

            Camera playerCamera = currentPlayer.GetComponentInChildren<Camera>();

            int everyOther = Mathf.RoundToInt((float)i+1 / 2);
            int everySecond = Mathf.RoundToInt((float)i+1 / 4);

            playerCamera.targetDisplay = 0;

            Rect rect = playerCamera.rect;

            if (playerCount == 1)
            {
                rect.x = 0;
                rect.y = 0;
            }

            if (playerCount == 2)
            {
                rect.x = 0;
                rect.y = 0.5f * -everyOther;
            }

            if (playerCount > 2)
            {
                rect.x = 0.5f * everyOther;
                rect.y = 0.5f * -everySecond;
            }


            playerCamera.rect = rect;


        }
    }

    // / Returns the time (+ X frames) until the screen is 100% black, in seconds.
    public float Fade_Black() 
    {
        BlackScreen_FadeTime_Timer = BlackScreen_FadeTime_Half + BlackScreen_FadeTime_RemainBlack;
        BlackScreen.CrossFadeAlpha(1, BlackScreen_FadeTime_Half, false);
        return BlackScreen_FadeTime_Half + Time.deltaTime * 1; // The X frames serves as simply as a buffer, so that no instant changes will ever be visible
    }


    public static Manager_UI Get()
    {
        return GameObject.Find("Canvas - Ingame").GetComponent<Manager_UI>();
    }
}

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
    public Color[] colors;

    [Header("Assign")]
    public Image BlackScreen;
    private FadeScreen[] FadeScreens;

    public struct FadeScreen
				{
        public Image BlackScreen;
        public float timer;
    }


    // Start is called before the first frame update
    void Start()
    {
        BlackScreen.enabled = true;
        BlackScreen.CrossFadeAlpha(0, 0, false);


        FadeScreens = new FadeScreen[BlackScreen.transform.childCount + 1];

        FadeScreens[0].BlackScreen = BlackScreen;
        FadeScreens[0].timer = -1;

        for (int i = 1; i < FadeScreens.Length; i++)
        {
            FadeScreens[i].BlackScreen = BlackScreen.transform.GetChild(i-1).GetComponent<Image>();
            FadeScreens[i].timer = -1;

            FadeScreens[i].BlackScreen.enabled = true;
            FadeScreens[i].BlackScreen.CrossFadeAlpha(0, 0, false);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (false && BlackScreen_FadeTime_Timer != -1) // All of the timer shenanigans
        {
            BlackScreen_FadeTime_Timer -= Time.deltaTime;

            if (BlackScreen_FadeTime_Timer < 0)
            {
                BlackScreen_FadeTime_Timer = -1;
                BlackScreen.CrossFadeAlpha(0, BlackScreen_FadeTime_Half, false);
            }
        }


        for (int i = 0; i < FadeScreens.Length; i++)
        {
            if(FadeScreens[i].timer != -1)
												{
                FadeScreens[i].timer -= Time.deltaTime;

                if(FadeScreens[i].timer < 0)
																{
                    FadeScreens[i].timer = -1;

                    FadeScreens[i].BlackScreen.CrossFadeAlpha(0, BlackScreen_FadeTime_Half, false);
                }
												}
        }
    }

    public void AdjustScreen()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Fade_Black();

        int playerCount = Mathf.Min(players.Length, 4);
        bool everyOther = false;

        for (int i = 0; i < playerCount; i++)
        {
            Transform currentPlayer = players[i].transform;

            if (playerCount > 1) // Multiplayer Options Only:
            {
                currentPlayer.GetComponentInChildren<MeshRenderer>().material.color = colors[i];

                float playerWidth = currentPlayer.GetComponent<BoxCollider>().size.x;

                if (Vector3.Distance(currentPlayer.position, players[0].transform.position) < playerWidth)
                {
                    currentPlayer.position += currentPlayer.right * playerWidth * 1.1f * i;
                }
            }


            Controller_Vehicle playerController = currentPlayer.GetComponent<Controller_Vehicle>();

            playerController.currentInput = playerController.Inputs[i];
            playerController.playerIndex = i;

            Camera playerCamera = currentPlayer.GetComponentInChildren<Camera>();


            

          
            bool aboveThree = i > 1;

            //playerCamera.targetDisplay = i;

            Rect rect = playerCamera.rect;

            Image SubFadeScreen = BlackScreen.transform.GetChild(i).GetComponent<Image>();
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.one;


            if (playerCount == 1)
            {
                rect.x = 0;
                rect.y = 0;
            }

            if (playerCount == 2)
            {
                rect.x = 0;
                rect.y = 0.5f * (everyOther ? -1 : 1);

                min.x = (i == 0 ? 0 : 0f);
                min.y = (i == 0 ? 0.5f : 0f);

                max.x = (i == 0 ? 1f : 1f);
                max.y = (i == 0 ? 1f : 0.5f);
            }

            if (playerCount > 2)
            {
                rect.x = 0.5f * (everyOther ? 1 : -1);
                rect.y = 0.5f * (aboveThree ? -1 : 1);
            }


            playerCamera.rect = rect;
            everyOther = !everyOther;

            SubFadeScreen.enabled = true;
            SubFadeScreen.rectTransform.anchorMin = min;
            SubFadeScreen.rectTransform.anchorMax = max;
        }
    }

    /// Returns the time (+ X frames) until the screen is 100% black, in seconds.
    public float Fade_Black(int playerIndex = -1) 
    {
        playerIndex++; // makes it easier for others to use

        FadeScreens[playerIndex].timer = BlackScreen_FadeTime_Half + BlackScreen_FadeTime_RemainBlack;
        FadeScreens[playerIndex].BlackScreen.CrossFadeAlpha(1, BlackScreen_FadeTime_Half, false);
        return BlackScreen_FadeTime_Half + Time.deltaTime * 1; // The X frames serves as simply as a buffer, so that no instant changes will ever be visible
    }


    public static Manager_UI Get()
    {
        return GameObject.Find("Canvas - Ingame").GetComponent<Manager_UI>();
    }
}

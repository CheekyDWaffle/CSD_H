using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Builder_UI_Manager : MonoBehaviour
{

    public RawImage activeImage, topImage, botImage;


    public GameObject displayUI;
    public List<Texture> displayImage;
    [Header("All available hazards AT the current lap")]
    public List<int> allHazards; //all hazards is updated at the end of a lap, it recives a new list that contains every track the player can place, reqardless of selected track pice.

    public int currentEnum;
    //[HideInInspector]
    public List<int> usableHazards;
    //[HideInInspector]
    public int currentDisplay; // can be shown in inspector for debuging

    public void Update()
    {
        //debug code for testing tile change. 
        #region
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    allHazards = new List<int>();
        //    allHazards.Add(1);
        //    //allHazards.Add(2);
        //    allHazards.Add(0);
        //    TileChange(allHazards);
        //}
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    allHazards = new List<int>();
        //    allHazards.Add(1);
        //    allHazards.Add(2);
        //    allHazards.Add(0);
        //    TileChange(allHazards);
        //}
        #endregion 
    }
    public void TileChange(List<int> inputHazards)
    {
        usableHazards = new List<int>();
        foreach (int i in allHazards)
        {
            //chekcs the given list of ints (inputhazards) contains any maching ints from allHazards
            if (inputHazards.Contains(i))
                usableHazards.Add(i);
        }

        if (usableHazards.Count == 0)
        {
            displayUI.SetActive(false);
        }
        else
        {
            displayUI.SetActive(true);
            DisplayChange(0);
        }
    }

    //recives a int of 0, 1 or -1.
    public void DisplayChange(int changeValue)
    {
        if (usableHazards.Count == 0) return;
        currentDisplay = ValueCheck(currentDisplay, changeValue);

        //change image on mddle, top and botom.
        activeImage.texture = displayImage[usableHazards[currentDisplay]];
        topImage.texture = displayImage[usableHazards[ValueCheck(currentDisplay, 1)]];
        botImage.texture = displayImage[usableHazards[ValueCheck(currentDisplay, -1)]];
        currentEnum = usableHazards[currentDisplay];
    }

    // allows "looping" list, for middle, top and botom image. 
    int ValueCheck(int currentValue, int changeValue)
    {
        if (usableHazards.Count == 0)
        {
            Debug.LogError("available hazards is empty");
            return 0;
        }
        currentValue += changeValue;

        if (currentValue >= usableHazards.Count) currentValue = 0;
        if (currentValue < 0) currentValue = usableHazards.Count - 1;

        return currentValue;
    }
}

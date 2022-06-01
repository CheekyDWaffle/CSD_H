using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menue_Builder_Navigation : MonoBehaviour
{

    public RawImage activeImage, topImage, botImage;


    public List<Texture> displayImage;
    [Header("All available AT the current time")]
    public List<int> allHazards;
    //public List<string> displayText;

    [HideInInspector]
    public List<int> usableHazards;
    //[HideInInspector]
    public int currentDisplay;
    private void Start()
    {
        currentDisplay = 0;
        allHazards.Add(1);
        allHazards.Add(2);
        allHazards.Add(0);
        TileChange(allHazards);

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            DisplayChange(1);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            DisplayChange(-1);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            allHazards = new List<int>();
            allHazards.Add(1);
            //allHazards.Add(2);
            allHazards.Add(0);
            TileChange(allHazards);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            allHazards = new List<int>();
            allHazards.Add(1);
            allHazards.Add(2);
            allHazards.Add(0);
            TileChange(allHazards);
        }
    }
    public void TileChange(List<int> inputHazards)
    {
        usableHazards = new List<int>();
        foreach (int i in allHazards)
        {
            if (inputHazards.Contains(i))
                usableHazards.Add(i);
        }
        //if a player cant place anything this is where the error will occur. 
        DisplayChange(0);
    }
    public int DisplayChange(int changeValue)
    {
        currentDisplay = ValueCheck(currentDisplay, changeValue);

        activeImage.texture = displayImage[usableHazards[currentDisplay]];
        topImage.texture = displayImage[usableHazards[ValueCheck(currentDisplay, 1)]];
        botImage.texture = displayImage[usableHazards[ValueCheck(currentDisplay, -1)]];

        return usableHazards[currentDisplay];
    }
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
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Menue_Builder_Navigation : MonoBehaviour
//{

//    public RawImage activeImage, topImage, botImage;


//    public List<int> returnValue;
//    public List<Texture> displayImage;
//    //public List<string> displayText;

//    [HideInInspector]
//    public List<int> avialbeHazards;
//    //[HideInInspector]
//    public int currentDisplay, topDisplay, botDisplay;
//    private void Start()
//    {
//        currentDisplay = 0;
//        topDisplay = 1;
//        botDisplay = displayImage.Count - 1;
//        DisplayChange(0);

//    }
//    public void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.W))
//        {
//            DisplayChange(1);
//        }
//        if (Input.GetKeyDown(KeyCode.S))
//        {
//            DisplayChange(-1);
//        }
//    }
//    public int DisplayChange(int changeValue)
//    {
//        currentDisplay += changeValue;
//        topDisplay += changeValue;
//        botDisplay += changeValue;

//        if (currentDisplay >= displayImage.Count) currentDisplay = 0;
//        if (topDisplay >= displayImage.Count) topDisplay = 0;
//        if (botDisplay >= displayImage.Count) botDisplay = 0;


//        if (currentDisplay < 0) currentDisplay = displayImage.Count - 1;
//        if (topDisplay < 0) topDisplay = displayImage.Count - 1;
//        if (botDisplay < 0) botDisplay = displayImage.Count - 1;


//        activeImage.texture = displayImage[currentDisplay];
//        topImage.texture = displayImage[topDisplay];
//        botImage.texture = displayImage[botDisplay];




//        return returnValue[currentDisplay];
//    }
//}

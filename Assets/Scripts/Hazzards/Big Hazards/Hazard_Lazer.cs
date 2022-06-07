using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard_Lazer : MonoBehaviour
{
    [Header("Attributes")]
    public float lazerLenght;
    public float scaleFactor;
    public float warningTime;
    public float coolDown;

    [Header("Must be assigned")]
    public GameObject lazerPrefab;
    public GameObject warningPrefab;

    [HideInInspector]
    public bool fireLazer;
    [HideInInspector]
    public bool isActive;
    [HideInInspector]
    public Vector3 originalTransform;
    [HideInInspector]
    public Vector3 orignalScale;


    bool retractLazer;
    bool drawRay = true;
    private void Start()
    {
        drawRay = false;
        fireLazer = true;
        lazerLenght = lazerLenght * 100 + 50;
        originalTransform = lazerPrefab.transform.position;
        orignalScale = lazerPrefab.transform.localScale;
        float loopBreak = 1000;
        while (warningPrefab.transform.lossyScale.z <= lazerLenght * 2)
        {
            loopBreak--;
            warningPrefab.transform.localScale += new Vector3(0, 0, 1);
            warningPrefab.transform.localPosition += new Vector3(0, 0, .5f);
            if (loopBreak <= 0)
            {
                break;
            }
        }

    }

    //delay between shots in seconds
    IEnumerator fireCooldown()
    {
        yield return new WaitForSeconds(coolDown);
        StartCoroutine("fireWarning");
    }
    //How long the warning is visable before the shot is fired
    IEnumerator fireWarning()
    {
        warningPrefab.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        fireLazer = true;
    }
    //Retracts lazer, resets transform and restarts fire prosses
    void disepateLazer()
    {
        lazerPrefab.transform.position = originalTransform;
        lazerPrefab.transform.localScale = orignalScale;
        fireLazer = false;
        retractLazer = false;
        StartCoroutine("fireCooldown");
    }
    private void Update()
    {

        if (isActive)
        {

            if (fireLazer && !retractLazer)
            {
                //moves lazer forwards and scales the lazer in the forward direction
                lazerPrefab.transform.localScale += new Vector3(0, scaleFactor * Time.deltaTime, 0);
                lazerPrefab.transform.localPosition += new Vector3(0, 0, scaleFactor * Time.deltaTime);

                if (lazerPrefab.transform.lossyScale.y >= lazerLenght)
                {
                    retractLazer = true;
                    warningPrefab.SetActive(false);
                }
            }
            if (retractLazer)
            {
                //scaling down the lazer, whils tcontinuing movment in the forwards direction.
                lazerPrefab.transform.localScale -= new Vector3(0, (scaleFactor * 2) * Time.deltaTime, 0);
                lazerPrefab.transform.localPosition += new Vector3(0, 0, (scaleFactor * 2) * Time.deltaTime);

                if (lazerPrefab.transform.lossyScale.y <= 0)
                {
                    disepateLazer();
                }
            }
        }

    }


    private void OnDrawGizmos()
    {
        if (drawRay)
        {
            float raylenght = lazerLenght * 200 + 100;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * raylenght);
        }

    }
}

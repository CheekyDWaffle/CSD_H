using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard_Lazer : MonoBehaviour
{
    [Header("Attributes")]
    public float lazerLenght;
    public float scaleFactor;
    public float warningTime;

    [Header("Must be assigned")]
    public GameObject lazerPrefab;
    public GameObject warningPrefab;

    [HideInInspector]
    public bool fireLazer;
    [HideInInspector]
    bool retractLazer;
    [HideInInspector]
    public Vector3 originalTransform;
    [HideInInspector]
    public Vector3 orignalScale;
    
    private void Start()
    {
        originalTransform = lazerPrefab.transform.position;
        orignalScale = lazerPrefab.transform.localScale;
        float loopBreak = 1000;
        while (warningPrefab.transform.lossyScale.z <= lazerLenght*2)
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine("fireWarning");
        }

        if (fireLazer && !retractLazer)
        {
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
            lazerPrefab.transform.localScale -= new Vector3(0, (scaleFactor * 2) * Time.deltaTime, 0);
            lazerPrefab.transform.localPosition += new Vector3(0, 0, (scaleFactor * 2) * Time.deltaTime);

            if (lazerPrefab.transform.lossyScale.y <= 0)
            {
                disepateLazer();
            }
        }

    }

    void disepateLazer()
    {
        lazerPrefab.transform.position = originalTransform;
        lazerPrefab.transform.localScale = orignalScale;
        fireLazer = false;
        retractLazer = false;
    }
    IEnumerator fireWarning()
    {
        warningPrefab.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        fireLazer = true;
    }
    private void OnDrawGizmos()
    {
        float raylenght = lazerLenght * 2 - 2.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * raylenght);
    }
}

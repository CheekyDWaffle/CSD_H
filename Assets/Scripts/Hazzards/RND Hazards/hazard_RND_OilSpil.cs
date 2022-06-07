using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hazard_RND_OilSpil : MonoBehaviour
{
    public float tractionLoss, stearingLoss;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Controller_Vehicle>().grip_Multiplier -= tractionLoss;
            other.GetComponent<Controller_Vehicle>().turnSpeed_Modifier -= stearingLoss;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name + "exited");
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Controller_Vehicle>().grip_Multiplier += tractionLoss;
            other.GetComponent<Controller_Vehicle>().turnSpeed_Modifier += stearingLoss;
        }
    }
}

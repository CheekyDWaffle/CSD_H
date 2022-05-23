using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSpill : MonoBehaviour
{
    public float tractionLoss, stearingLoss;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + "entered");
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Controller_Vehicle>().friction_Modifier -= tractionLoss;
            other.GetComponent<Controller_Vehicle>().turnSpeed -= stearingLoss;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name + "exited");
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Controller_Vehicle>().friction_Modifier += tractionLoss;
            other.GetComponent<Controller_Vehicle>().turnSpeed += stearingLoss;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hazard_RND_Rock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Controller_Vehicle>().OnDeath();
        }
    }
}

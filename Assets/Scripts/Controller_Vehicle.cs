using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Vehicle : MonoBehaviour
{
    [Header("Attributes")]
    public float speed_Base_kmh = 6;
    public float acceleration = 5;
    public float driftModifier = 0.5f;
    public float turnSpeed = 1;

    public float speed_Modifier = 1;


    [Header("Read-Only")]
    public Vector3 velocity;
    public string currentSpeed;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float timeStep = Time.deltaTime;

        Move(timeStep);

        transform.position += velocity * timeStep;
    }

				void Move(float timeStep)
				{
        int forwardModifier = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        int sidewayModifier = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);
      
        float frictionModifier = Input.GetKey(KeyCode.Space) ? driftModifier : 1f;

        if (frictionModifier != 1)
            forwardModifier = 0;

        transform.localEulerAngles += new Vector3(0, sidewayModifier * timeStep * turnSpeed, 0);

        sidewayModifier = 0; // Temp

        float speed_Base = speed_Base_kmh / 6 / 6 * 10;

        Vector3 newVelocity = (transform.forward * forwardModifier + transform.right * sidewayModifier).normalized * speed_Base * speed_Modifier;

        float frictionStep = acceleration * timeStep * frictionModifier;

        velocity -= velocity * frictionStep;
        velocity += newVelocity * frictionStep;

								#region current Speed string
								float speed = Mathf.Floor(velocity.magnitude * 100) / 100;
        float speed_kmh = Mathf.Round(speed * 60 * 60 / 1000);

        currentSpeed = "Current speed is: " + speed + " m/s. (" + speed_kmh + " km/h)";
								#endregion
				}
}

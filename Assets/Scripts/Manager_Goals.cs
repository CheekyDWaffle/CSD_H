using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Goals : MonoBehaviour
{
    public Vector3 goalPosition;
    public float goalForward = 0;
    public float goalWidth = 10;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawGoal();
    }

    public void DrawGoal()
    {
        Vector3 forward = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.forward;
        Vector3 right = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.right;

        Debug.DrawRay(goalPosition, forward * goalWidth / 2, Color.red);
        Debug.DrawRay(goalPosition, right * goalWidth / 2, Color.blue);
    }

    public bool isPassingGoal(Transform target, Vector3 velocity, bool wasGoingReverse, int lastLapCount, out bool isGoingReverse, out int lapCount)
				{

        /// Note: Right now it checks the center of the car, not front. I'll get on that eventually.
        bool goOnCooldown = false;
        float distance = Vector3.Distance(target.position, goalPosition);

        isGoingReverse = wasGoingReverse;
        lapCount = lastLapCount;

        if (distance > goalWidth)
            return false;

        Vector3 forward = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.forward;
        Vector3 right = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.right;

        Vector3 directionToGoal = target.position - goalPosition;

        float positionDot = Vector3.Dot(directionToGoal.normalized, forward);
        float velocityDot = Vector3.Dot(velocity, forward);

        bool isInfrontOfGoal = positionDot > 0 && positionDot < 0.5f;
        bool isMovingForward = velocityDot > 0;

        if(isInfrontOfGoal)
								{
            if(isMovingForward && !wasGoingReverse)
												{
                lapCount++;
                isGoingReverse = false;
                goOnCooldown = true;
												}

            if (isMovingForward && wasGoingReverse)
            {
                isGoingReverse = false;
                goOnCooldown = true;
            }

            if (!isMovingForward)
												{
                isGoingReverse = true;

												}

								}

        if (lapCount > 3)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<Controller_Vehicle>().Reset();

            }
        }

        return goOnCooldown;
    }
}

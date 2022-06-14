using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Goals : MonoBehaviour
{
    [Header("Game Settings")]
    public int totalLapCount = 3;

    [Header("Goal Settings")]
    public Vector3 goalPosition;
    public float goalForward = 0;
    public float goalWidth = 10;

    [Header("scene to load, int")]
    public int endScene;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DrawGoal();
        DrawGoal(1);
    }

    public void DrawGoal(int i = 0)
    {
        Vector3 forward = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.forward;
        Vector3 right = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.right;

        Debug.DrawRay(goalPosition + forward * i * goalWidth, forward * goalWidth / 2, Color.red);
        Debug.DrawRay(goalPosition + forward * i * goalWidth, right * goalWidth / 2, Color.blue);
        Debug.DrawRay(goalPosition + forward * i * goalWidth, -right * goalWidth / 2, Color.cyan);
    }

    public void isPassingGoal(Transform target, Vector3 velocity, bool wasGoingReverse, int lastLapCount, out bool isGoingReverse, out int lapCount)
    {
        /// Note: Right now it checks the center of the car, not front. I'll get on that eventually.
        float distance = Vector3.Distance(target.position, goalPosition);

        isGoingReverse = wasGoingReverse;
        lapCount = lastLapCount;

        if (distance > goalWidth * 3)
            return;

        Controller_Vehicle carScript = target.GetComponent<Controller_Vehicle>();


        bool isInfrontOfGoal = false;
        bool isMovingForward = false;

        int currentPosition = 0;
        int PreviousPosition = carScript.previousGoal;

        for (int i = 0; i < 2; i++)
        {
            Vector3 forward = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.forward;
            Vector3 right = Quaternion.AngleAxis(goalForward, Vector3.up) * Vector3.right;

            Vector3 directionToGoal = target.position - (goalPosition + forward * i * goalWidth);

            float positionDot = Vector3.Dot(directionToGoal.normalized, forward);
            float velocityDot = Vector3.Dot(velocity, forward);

            isInfrontOfGoal = positionDot > 0;
            isMovingForward = velocityDot > 0;


            if(isInfrontOfGoal)
                currentPosition = 1 + i;
        }

        if (!carScript.isGoingReverse && currentPosition == 0 && PreviousPosition == 2)
            PreviousPosition = 0;
        
        if (currentPosition < PreviousPosition)
            carScript.isGoingReverse = true;
        
        if (currentPosition > PreviousPosition)
        {
            if (currentPosition == 2 && !carScript.isGoingReverse) // Index 1 would be the first real line. This is only triggered on the tick currentPosition increases.
            {
                carScript.isGoingReverse = false;

                Debug.Log("New Lap");

                lapCount++;
                OnPassGoal(carScript, lapCount);
            }

            if (currentPosition == 2)
                carScript.isGoingReverse = false;
        }

        carScript.previousGoal = currentPosition;
    }

    private void OnPassGoal(Controller_Vehicle player, int lapCount)
    {

        if (lapCount >= totalLapCount)
        {
            SceneManager.LoadScene(endScene);
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<Controller_Vehicle>().Reset();
                players[i].GetComponent<Controller_Vehicle>().lapCount = lapCount;
                players[i].GetComponent<Controller_Vehicle>().isGoingReverse = true; // Reset progress relative to the finish line.
            }

            GetComponent<Controller_MapBuild>().buildModeChangeTimer = Manager_UI.Get().Fade_Black();
        }
    }
}

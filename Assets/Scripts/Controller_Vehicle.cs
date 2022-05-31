using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Vehicle : MonoBehaviour
{
    [Header("Attributes")]
    public float speed_Base_kmh = 6;
    private float speed_Base_ms {get { return speed_Base_kmh / 60 / 60 * 1000; } }
    public float acceleration = 5;
    public float driftModifier = 0.5f;
    public float turnSpeed = 1;

    [Header("Temp Attributes")]
    public float friction_Modifier = 1;
    public float speed_Modifier = 1;


    [Header("Settings")]
    public float checkPointSaveFrequency = 0.5f;
    private float checkPointSaveTimer;
    public float checkPointRollback = 2f;
    private float deathTimer = -1;
    private float returnToTrackTimer = -1;

    [Header("Read-Only (That means don't touch)")]
    public Vector3 velocity;
    public string currentSpeed;
    public int lapCount = 0;
    public bool isGoingReverse = false;

    [Header("Particles")]
    public ParticleSystem[] DriftSmoke;

    Vector3 startPos;
    Vector3 startRot;
    float distanceToGround;
    Manager_Goals goalManager;
    
    [HideInInspector]
    public int playerIndex = 0;

    [System.Serializable]
    public class PlayerInput
    {
        public string playerName = "Player X";
        public int playerIndex = 0;
        public bool isController = false;

        public KeyCode forward = KeyCode.W;
        public KeyCode backWards = KeyCode.S;
        public KeyCode right = KeyCode.D;
        public KeyCode left = KeyCode.A;

        public KeyCode handBrake = KeyCode.Space;
    }

    public PlayerInput[] Inputs;

    [HideInInspector]
    public PlayerInput currentInput;

    public bool pauseCar = false;

    void Awake()
    {
        tag = "Player";
        currentInput = Inputs[0];
    }

    void Start()
    {
        Manager_UI.Get().AdjustScreen();

        goalManager = Manager_UI.Get().GetComponent<Manager_Goals>();

        startPos = transform.position;
        startRot = transform.eulerAngles;

        RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        distanceToGround = groundCheck.distance;


    }

    float goalCooldwon;

    // Update is called once per frame
    void Update()
    {



        if (pauseCar)
        {
            return;
        }

        float timeStep = Time.deltaTime;

        goalCooldwon -= Time.deltaTime;

        Particles();

        Move(timeStep);

        OnCollisin();

        CheckPointSaving();

        transform.position += velocity * timeStep;

        if (goalCooldwon < 0 && goalManager.isPassingGoal(transform, velocity, isGoingReverse, lapCount, out isGoingReverse, out lapCount))
            goalCooldwon = 0.5f;

        if (deathTimer != -1)
        {
            deathTimer -= Time.deltaTime;

            if (deathTimer < 0)
            {
                deathTimer = -1;
                OnRespawn();
            }
        }


        if (Input.GetKeyDown(KeyCode.X))
            OnDeath();

        if (transform.position.y < -10)
            OnDeath();


        if (Input.GetKeyDown(KeyCode.C))
        {
            returnToTrackTimer = Manager_UI.Get().Fade_Black(playerIndex);
        }

        if (returnToTrackTimer != -1)
        {
            returnToTrackTimer -= timeStep;

            if(returnToTrackTimer < 0)
												{
                returnToTrackTimer = -1;

                transform.position = startPos;
                transform.eulerAngles = startRot;
                velocity = Vector3.zero;

                lapCount = 0;
            }
        }
    }

				public void Reset()
				{
        returnToTrackTimer = Manager_UI.Get().Fade_Black(playerIndex);
        lapCount = 0;
    }

				void Move(float timeStep)
    {
        RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        bool isGrounded = groundCheck.transform != null && groundCheck.distance <= distanceToGround;

        if (isGrounded)
            velocity.y = 0;
        else
            velocity += Physics.gravity * timeStep;

        float forwardModifier = (Input.GetKey(currentInput.forward) ? 1 : 0) + (Input.GetKey(currentInput.backWards) ? -1 : 0);
        float sidewayModifier = (Input.GetKey(currentInput.right) ? 1 : 0) + (Input.GetKey(currentInput.left) ? -1 : 0);

        float frictionModifier = Input.GetKey(currentInput.handBrake) ? driftModifier : 1f;

        if (currentInput.isController) // controller override
        {
            int currentIndex = playerIndex + 1; // Joysticks don't actually use index, but player number.

            if (!Inputs[0].isController)
                currentIndex--;

            KeyCode accelerationKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + currentIndex + "Button0");

            KeyCode brakeKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + currentIndex + "Button1");
            KeyCode handBrakeKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + currentIndex + "Button5");
            KeyCode reverseKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + currentIndex + "Button2");

            forwardModifier = (Input.GetKey(accelerationKey) ? 1 : 0) + (Input.GetKey(reverseKey) ? -0.1f : 0);
            sidewayModifier = Input.GetAxis("Horizontal_C_" + currentIndex);

            frictionModifier = Input.GetKey(handBrakeKey) ? driftModifier : 1f;
        }

        if (frictionModifier != 1)
            forwardModifier = 0;

        transform.localEulerAngles += new Vector3(0, sidewayModifier * timeStep * turnSpeed, 0);

        sidewayModifier = 0; // Temp

        float speed_Base = speed_Base_kmh / 6 / 6 * 10;

        Vector3 newVelocity = (transform.forward * forwardModifier + transform.right * sidewayModifier).normalized * speed_Base * speed_Modifier;

        float frictionStep = acceleration * timeStep * frictionModifier * friction_Modifier;

        velocity -= velocity * frictionStep;
        velocity += newVelocity * frictionStep;

        #region current Speed string
        float speed = Mathf.Floor(velocity.magnitude * 100) / 100;
        float speed_kmh = Mathf.Round(speed * 60 * 60 / 1000);

        currentSpeed = "Current speed is: " + speed + " m/s. (" + speed_kmh + " km/h)";
        #endregion
    }

    void Particles()
				{
        bool isDrifting = Input.GetKey(currentInput.handBrake) && velocity.magnitude > (speed_Base_ms * 0.15f); // Am I drifting AND I am still above X % of my base speed? Good, then play the drift effects

        RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        bool isGrounded = groundCheck.transform != null && groundCheck.distance <= distanceToGround;

        for (int i = 0; i < DriftSmoke.Length; i++)
								{
            DriftSmoke[i].enableEmission = isDrifting && isGrounded;

								}

    }


    public List<CheckPoint> checkPoints;
    [System.Serializable]
    public struct CheckPoint
    {
        public Vector3 position;
        public float time;
    }

    void CheckPointSaving()
    {
        checkPointSaveTimer += Time.deltaTime;

        if (checkPointSaveTimer < checkPointSaveFrequency)
            return;

        bool isStable = velocity.y == 0;

        /// I want to save my last grounded location ever X of a second.
        /// In the future, I should also check if I am actually ahead of my previous checkpoint before saving it.
        if (isStable)
        {
            CheckPoint newCheckPoint = new CheckPoint();

            newCheckPoint.position = transform.position;
            newCheckPoint.time = Time.timeSinceLevelLoad;

            checkPoints.Add(newCheckPoint);
            checkPointSaveTimer = 0;
        }




    }


    public void OnDeath()
    {
        if (deathTimer != -1)
            return;

        deathTimer = Manager_UI.Get().Fade_Black(playerIndex);
    }

    void OnRespawn()
    {
        /// Then, I want to respawn Y seconds "behind" my last grounded position.

        #region Find Checkpoint

        CheckPoint lastCheckPoint = checkPoints[checkPoints.Count - 1];
        float baseTime = lastCheckPoint.time;

        Vector3 relativeForward = Vector3.zero;

        for (int i = 1; i < checkPoints.Count; i++)
        {
            int index = checkPoints.Count - 1 - i; // I really don't want to mess with loops right now.

            float relativeTime = baseTime - checkPoints[index].time;

            relativeForward = checkPoints[index + 1].position - checkPoints[index].position;

            if (relativeTime < checkPointRollback) // if this checkpoint has NOT surprassed the rollback time yet.
                lastCheckPoint = checkPoints[index];
            else
                break;
        }
        #endregion

        transform.position = lastCheckPoint.position;
        transform.forward = relativeForward.normalized;
        velocity = Vector3.zero;

        /// If I die within Z seconds of respawning, I respawn 5 seconds behind. Or begenning of last tile, when that is implemented.
    }





    void OnCollisin()
    {
        Collider[] overlaps = new Collider[4];
        LayerMask IgnoreLayer = ~new LayerMask();

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        //CapsuleCollider cylinderCollider = GetComponent<CapsuleCollider>();

        float colliderHeight = 2;
        float colliderRadius = 0.5f;

        Vector3 End = transform.position + transform.up * (colliderHeight / 2 - colliderRadius);

        int lenght = Physics.OverlapBoxNonAlloc(transform.position + boxCollider.center, boxCollider.size * 1.5f, overlaps, transform.rotation, IgnoreLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < lenght; i++)
        {
            Transform t = overlaps[i].transform;

            if (t == transform)
                continue;

            Vector3 dir;
            float distance;

            if (Physics.ComputePenetration(boxCollider, transform.position + boxCollider.center, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out distance))
            {
                dir = dir - Vector3.Project(dir, transform.up); // This is relative horizontal, not relative to gravity.

                transform.position = transform.position + dir * distance;

                velocity -= Vector3.Project(velocity, dir); // Removes the velocity once impacting with a wall.
            }
        }
    }

}

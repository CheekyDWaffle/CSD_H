using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Vehicle : MonoBehaviour
{
    [Header("Attributes")]
    public float speed_Base_kmh = 6;
    private float speed_Base_ms {get { return speed_Base_kmh / 60 / 60 * 1000; } }
    public float acceleration = 5;
    public float turnSpeed = 1;
    public float sidewayTractionMultiplier = 0.5f;

    [Header("Drift Attributes")]
    public float driftModifier = 0.5f;
    public float driftTurnMultiplier = 2;


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

    void Awake() // Awake is triggered before any Start(), so things that looks for stuff tagged as "player" as this object is spawned, will always find this.
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

        RaycastHit groundCheck; // I am checking the innitial distance to the ground.
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        distanceToGround = groundCheck.distance;
    }

    float goalCooldwon;

    // Update is called once per frame
    void Update()
    {
        if (pauseCar) // the car is being told to sit still and wait.
            return;

        float timeStep = Time.deltaTime; // Identical to Time.deltaTime. For thise project it serves no specific purpose.

        goalCooldwon -= timeStep;

        Particles(); // The particle system

        Move(timeStep); // This is actually the part that updates velocity, and not the part that moves the car.

        OnCollisin(); // Hit detection for the car

        transform.position += velocity * timeStep; // Actually moving the car.

        CheckPointSaving(); // Checkpoints

								#region Misc. Timers, like death and respawn

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

        if (transform.position.y < -10)
            OnDeath();




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

        #endregion

        if (Input.GetKeyDown(KeyCode.C)) // Hard Reset for debug purposes.
            returnToTrackTimer = Manager_UI.Get().Fade_Black(playerIndex);
        
    }

				void Move(float timeStep)
    {
								#region Ground check.
								RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        bool isGrounded = groundCheck.transform != null && groundCheck.distance <= distanceToGround;

        if (isGrounded)
            velocity.y = 0;
        else
            velocity += Physics.gravity * timeStep;

        #endregion

        #region Keyboard & Controller Input
        float forwardModifier = 0;
        float sidewayModifier = 0;
        float frictionModifier = 0;

        if (!currentInput.isController) // controller override
        {
            forwardModifier = (Input.GetKey(currentInput.forward) ? 1 : 0) + (Input.GetKey(currentInput.backWards) ? -1 : 0);
            sidewayModifier = (Input.GetKey(currentInput.right) ? 1 : 0) + (Input.GetKey(currentInput.left) ? -1 : 0);

            frictionModifier = Input.GetKey(currentInput.handBrake) ? driftModifier : 1f;
        }
        else if (currentInput.isController) // controller override
        {
            int currentIndex = playerIndex + 1; // Joysticks don't actually use index, but player number.

            if (!Inputs[0].isController)
                currentIndex--;


            string controllerName = "Joystick" + currentIndex + "Button";
            string leftStickName = "Horizontal_C_" + currentIndex;

            leftStickName = "Horizontal_C_Universal"; // This swapps the joystick to the universal one. If this doesn't work, then I can not understand.


            KeyCode accelerationKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), controllerName+0);

            KeyCode brakeKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), controllerName+1);
            KeyCode handBrakeKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), controllerName+5);
            KeyCode reverseKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), controllerName+2);

            forwardModifier = (Input.GetKey(accelerationKey) ? 1 : 0) + (Input.GetKey(reverseKey) ? -0.1f : 0);
            sidewayModifier = Input.GetAxis(leftStickName);

            frictionModifier = Input.GetKey(handBrakeKey) ? driftModifier : 1f;
        }

        bool isDrifting = frictionModifier != 1;

    #endregion

        #region Axel
        float minimumTurnModifier = Mathf.Clamp(velocity.magnitude / (speed_Base_ms), 0, 0.1f);

        Vector3 wheelForward = Quaternion.AngleAxis(sidewayModifier * (isDrifting ? driftTurnMultiplier * turnSpeed : turnSpeed) * minimumTurnModifier, transform.up) * transform.forward;

        
        transform.forward = wheelForward;

        #endregion

        #region Applying the actual velocity
        // The old turn code.
        //transform.localEulerAngles += new Vector3(0, sidewayModifier * timeStep * turnSpeed, 0);

        float speed_Base = speed_Base_kmh / 6 / 6 * 10;
        float frictionStep = acceleration * timeStep * frictionModifier * friction_Modifier;

        Vector3 newVelocity = transform.forward * (isDrifting ? 1 : forwardModifier) * speed_Base * speed_Modifier;

        if (isGrounded)
        {
            Vector3 forwardVelocity = Vector3.Project(velocity, transform.forward);
            Vector3 sidewayVelocity = velocity - forwardVelocity;

           // velocity -= velocity * frictionStep; // This simulates ever present friction from the ground.
           // velocity += newVelocity * frictionStep; // This is acceleration, but also friction. There is no specific code to cap the velocity, because it reaches "terminal velocity" instead. Same result.

    
            velocity -= forwardVelocity * frictionStep;
            velocity -= sidewayVelocity * frictionStep * sidewayTractionMultiplier;

            velocity += newVelocity * frictionStep; // This is acceleration, but also friction. There is no specific code to cap the velocity, because it reaches "terminal velocity" instead. Same result.
        }
								#endregion

								#region Literally just speed messurement. Doesn't affect anything.
								float speed = Mathf.Floor(velocity.magnitude * 100) / 100;
        float speed_kmh = Mathf.Round(speed * 60 * 60 / 1000);

        currentSpeed = "Current speed is: " + speed + " m/s. (" + speed_kmh + " km/h)";
        #endregion
    }

    void Particles()
				{
								#region Drifting based particles
								bool isDrifting = Input.GetKey(currentInput.handBrake) && velocity.magnitude > (speed_Base_ms * 0.15f); // Am I drifting AND I am still above X % of my base speed? Good, then play the drift effects

        RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        bool isGrounded = groundCheck.transform != null && groundCheck.distance <= distanceToGround;

        for (int i = 0; i < DriftSmoke.Length; i++)
								{
            DriftSmoke[i].enableEmission = isDrifting && isGrounded;

								}
								#endregion
				}

				#region The checkpoint system
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

				#endregion

				#region "Death" and respawn
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

    public void Reset()
    {
        returnToTrackTimer = Manager_UI.Get().Fade_Black(playerIndex);
        lapCount = 0;
    }


    #endregion


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

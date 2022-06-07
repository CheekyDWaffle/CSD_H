using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Controller_Vehicle : MonoBehaviour
{
    [Header("Attributes")]
    public float speed_Base_kmh = 6;
    public float reverseSpeedPercentage = 0.1f;
    private float speed_Base_ms { get { return speed_Base_kmh / 60 / 60 * 1000; } }
    public float brakeStrength = 5;
    public float handbrakeStrength = 5;
    public float handbrakeTurnMultiplier = 2;
    public float roadGrip = 5;
    public float turnSpeed = 1;
    public float roadGripSidewaysModifier = 0.5f;
    public float mimimumSpeedBeforeReversing_ms = 5;

    [Header("Temp Attributes (Hazards and such)")]
    public float grip_Multiplier = 1;
    public float speed_Multiplier = 1;
    public float turnSpeed_Modifier = 1;


    [Header("Settings")]
    public float checkPointSaveFrequency = 0.5f;
    private float checkPointSaveTimer;
    public float checkPointRollback = 2f;
    private float deathTimer = -1;
    private float returnToTrackTimer = -1;
    public float axelPosition = 1;

    [Header("Read-Only (That means don't touch)")]
    public Vector3 velocity;
    public string currentSpeed;
    public int lapCount = 0;
    public bool isGoingReverse = false;

    [Header("Particles")]
    public ParticleSystem[] DriftSmoke;

    [HideInInspector]
    public Vector3 startPos, startRot, startRight;
    float distanceToGround;
    Manager_Goals goalManager;

    [HideInInspector]
    public int playerIndex = 0;
    AudioSource source;

    public bool pauseCar = false;

    float soundTimer = 0;
    bool canPlaySound { get { return soundTimer < 0; } }

    void Awake() // Awake is triggered before any Start(), so things that looks for stuff tagged as "player" as this object is spawned, will always find this.
    {
        tag = "Player";

        startPos = transform.position;
        startRot = transform.eulerAngles;
        startRight = transform.right;

        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        Manager_UI.Get().AdjustScreen();

        goalManager = Manager_UI.Get().GetComponent<Manager_Goals>();




        RaycastHit groundCheck; // I am checking the innitial distance to the ground.
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        distanceToGround = groundCheck.distance;
    }

    float goalCooldwon;

    // Update is called once per frame
    void Update()
    {
        soundTimer -= Time.deltaTime;

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

            if (returnToTrackTimer < 0)
                ReturnToTrack();
        }

        #endregion

        //if (Input.GetKeyDown(KeyCode.C)) // Hard Reset for debug purposes.
        //    returnToTrackTimer = Manager_UI.Get().Fade_Black(playerIndex);

    }

    void Move(float timeStep)
    {
        #region Literally just speed messurement. Doesn't affect anything.
        float speed = Mathf.Floor(velocity.magnitude * 100) / 100 * Mathf.Sign(Vector3.Dot(velocity, transform.forward));
        float speed_kmh = Mathf.Round(speed * 60 * 60 / 1000);

        currentSpeed = "Current speed is: " + speed + " m/s. (" + speed_kmh + " km/h)";


  

        #endregion

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

        bool isReversing = !NewInput.isAccelerating && NewInput.onBrakeReverse && speed < mimimumSpeedBeforeReversing_ms;
        bool isBraking = NewInput.onBrakeReverse && !isReversing;

        float forwardModifier = NewInput.isAccelerating ? 1f : 0f + (isReversing ? -reverseSpeedPercentage : 0); // This is based off of my intention to reverse
        float sidewayModifier = NewInput.turning * (speed < 0 ? -1 : 1); // This is based off of literally & physically reversing.
        #endregion

        #region Turning

        Debug.DrawRay(transform.position + transform.forward * axelPosition, transform.up, Color.red);

        float minimumTurnModifier = Mathf.Clamp(velocity.magnitude / mimimumSpeedBeforeReversing_ms, 0, 1);
        float angle = sidewayModifier * (NewInput.isDrifting ? handbrakeTurnMultiplier : 1) * turnSpeed * turnSpeed_Modifier * minimumTurnModifier;

        transform.RotateAround(transform.position + transform.forward * axelPosition, transform.up, angle * timeStep);
        #endregion

        if (!NewInput.isDrifting && speed > 5 && canPlaySound)
        {
            source.PlayOneShot(Sounds[0].sound);
            soundTimer = Sounds[0].sound.length;
        }

        if (NewInput.isDrifting && velocity.magnitude > (speed_Base_ms * 0.15f) && canPlaySound)
        {
            source.PlayOneShot(Sounds[1].sound);
            soundTimer = Sounds[1].sound.length;
        }

        #region Applying the actual velocity
        float frictionStep = roadGrip * timeStep * grip_Multiplier;
        Vector3 newVelocity = transform.forward * forwardModifier * speed_Base_ms * speed_Multiplier;

        if (isGrounded)
        {
            Vector3 forwardVelocity = Vector3.Project(velocity, transform.forward);
            Vector3 sidewayVelocity = velocity - forwardVelocity;

            // velocity -= velocity * frictionStep; // This simulates ever present friction from the ground.
            // velocity += newVelocity * frictionStep; // This is acceleration, but also friction. There is no specific code to cap the velocity, because it reaches "terminal velocity" instead. Same result.


            velocity -= forwardVelocity * frictionStep * (isBraking ? brakeStrength : 1) * (NewInput.isDrifting ? handbrakeStrength : 1);
            velocity -= sidewayVelocity * frictionStep * roadGripSidewaysModifier;

            velocity += newVelocity * frictionStep; // This is acceleration, but also friction. There is no specific code to cap the velocity, because it reaches "terminal velocity" instead. Same result.
        }
        #endregion
    }

    void Particles()
    {
        #region Drifting based particles
        bool isDrifting = NewInput.isDrifting && velocity.magnitude > (speed_Base_ms * 0.15f); // Am I drifting AND I am still above X % of my base speed? Good, then play the drift effects

        RaycastHit groundCheck;
        Physics.Raycast(transform.position, Vector3.down, out groundCheck);
        bool isGrounded = groundCheck.transform != null && groundCheck.distance <= distanceToGround;

        for (int i = 0; i < DriftSmoke.Length; i++)
        {
            DriftSmoke[i].enableEmission = isDrifting && isGrounded;

        }
        #endregion
    }

    public void ReturnToTrack()
    {
        returnToTrackTimer = -1;

        transform.position = startPos;
        transform.eulerAngles = startRot;
        velocity = Vector3.zero;
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

    #region Collect Inputs

    [System.Serializable]
    public class NewInputClass
				{
        public bool isAccelerating;
        public float turning;
        public bool isDrifting;
        public bool onBrakeReverse;
				}

    [System.Serializable]
    public struct SoundClass
    {
        public string name;
        public AudioClip sound;
    }
    public SoundClass[] Sounds;


    public NewInputClass NewInput = new NewInputClass();

    bool Input_isAccelerating;
    void OnAccelerate(InputValue value)
    {
        NewInput.isAccelerating = value.Get<float>() != 0 ? true : false;
    }

    void OnTurning(InputValue value)
    {
        NewInput.turning = value.Get<float>();
    }

    void OnDrift(InputValue value)
    {
        NewInput.isDrifting = value.Get<float>() != 0 ? true : false;
    }

    void OnBrakeReverse(InputValue value)
    {
        NewInput.onBrakeReverse = value.Get<float>() != 0 ? true : false;
    }

    #endregion

}

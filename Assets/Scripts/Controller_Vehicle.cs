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

    [Header("Settings")]
    public float checkPointSaveFrequency = 0.5f;
    private float checkPointSaveTimer;
    public float checkPointRollback = 2f;
    private float deathTimer = -1;

    [Header("Read-Only")]
    public Vector3 velocity;
    public string currentSpeed;

    [System.Serializable]
    public class Characters
    {
        public Transform playerTransform;
        public Vector3 velocity;

        public KeyCode forward = KeyCode.W;
        public KeyCode backWards = KeyCode.S;
        public KeyCode right = KeyCode.D;
        public KeyCode left  = KeyCode.A;
    }

    public Characters[] players;
    public bool pauseCar = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {



        if (pauseCar)
        {
            return;
        }

        float timeStep = Time.deltaTime;


        Move(timeStep);

        OnCollisin();

        CheckPointSaving();

        transform.position += velocity * timeStep;

        if (deathTimer != -1)
        {
            deathTimer -= Time.deltaTime;

            if(deathTimer < 0)
												{
                deathTimer = -1;
                OnRespawn();
												}
        }


        if (Input.GetKeyDown(KeyCode.X))
            OnDeath();

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


    void OnDeath()
    {
        if (deathTimer != -1)
            return;

        deathTimer = Manager_UI.Get().Fade_Black();
    }

    void OnRespawn()
				{
        /// Then, I want to respawn Y seconds "behind" my last grounded position.

        #region Find Checkpoint

        CheckPoint lastCheckPoint = checkPoints[checkPoints.Count - 1];
        float baseTime = lastCheckPoint.time;

        for (int i = 0; i < checkPoints.Count; i++)
        {
            int index = checkPoints.Count - 1 - i; // I really don't want to mess with loops right now.

            float relativeTime = baseTime - checkPoints[index].time;

            if (relativeTime < checkPointRollback) // if this checkpoint has NOT surprassed the rollback time yet.
                lastCheckPoint = checkPoints[index];
            else
                break;
        }
        #endregion

        transform.position = lastCheckPoint.position;
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

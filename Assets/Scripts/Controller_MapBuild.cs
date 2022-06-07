using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Controller_MapBuild : MonoBehaviour
{
    [System.Serializable]
    public class Player
    {
        public string name;
        public RectTransform cursor;
        public Color color;
        public Vector2 worldPosition;
        public Vector2 gridPosition;
        public Vector2 cursorInput;
    }


    [Header("Assign")]
    public Player[] players;
    public Transform buildPhaseTransform;
    public Sprite occupied_I_Sprite;
    public Sprite occupied_L_Sprite;
    public GameObject prefab_track_I;
    public GameObject prefab_track_L;
    public Controller_Vehicle car1;
    public HazardManager managerHazard;
    private Builder_UI_Manager builderUI;

    [Header("Settings")]
    public float cursorSpeed = 2;
    public int gridSize = 100;
    public int gridLength = 10;
    public int trackSize = 20;

    [Header("Read Only")]
    public bool[,] gridCheck;

    bool isInBuildMode = false;
    float buildModeChangeTimer = -1;

    int currentRotation = 0;


    // Start is called before the first frame update
    void Start()
    {
        gridCheck = new bool[gridLength, gridLength];
        car1.pauseCar = isInBuildMode;
        buildPhaseTransform.gameObject.SetActive(isInBuildMode);
    }

    // Update is called once per frame
    void Update()
    {
        #region Buildmode Transition
        {
            if (buildModeChangeTimer != -1)
            {
                buildModeChangeTimer -= Time.deltaTime;

                if (buildModeChangeTimer < 0)
                {
                    isInBuildMode = !isInBuildMode;
                    car1.pauseCar = isInBuildMode;

                    buildModeChangeTimer = -1;
                }
            }


           // if (Input.GetKeyDown(KeyCode.LeftShift) && buildModeChangeTimer == -1)
           //     buildModeChangeTimer = Manager_UI.Get().Fade_Black();

            buildPhaseTransform.gameObject.SetActive(isInBuildMode);

            if (!isInBuildMode)
            {
                return;
            }
        }
        #endregion


        float timeStep = Time.deltaTime;

        Player localPlayer = players[0];


        int moveVertical = (Input.GetKeyDown(KeyCode.W) ? 1 : 0) + (Input.GetKeyDown(KeyCode.S) ? -1 : 0);
        int moveHorizontal = (Input.GetKeyDown(KeyCode.D) ? 1 : 0) + (Input.GetKeyDown(KeyCode.A) ? -1 : 0);

        localPlayer.cursorInput = new Vector2(moveHorizontal, moveVertical);


        for (int i = 0; i < 1; i++)
        {
            Player currentPlayer = players[i];

            currentPlayer.worldPosition += currentPlayer.cursorInput * trackSize / 2;// * cursorSpeed * timeStep;
            currentPlayer.gridPosition.x = Mathf.Round(currentPlayer.worldPosition.x / gridSize) * gridSize;
            currentPlayer.gridPosition.y = Mathf.Round(currentPlayer.worldPosition.y / gridSize) * gridSize;

            Vector2 gridUI = currentPlayer.gridPosition;
            gridUI.x += gridSize / 2;
            gridUI.y += gridSize / 2;

            currentPlayer.cursor.localPosition = gridUI;

            #region This whole region makes no sense, what am I doing??? - Talha
            Vector2 rawGrid = localPlayer.gridPosition / gridSize + new Vector2(gridLength, gridLength) / 2;

            Vector3 hazardVector = new Vector3(currentPlayer.gridPosition.x + gridLength / 2, 0.9f, currentPlayer.gridPosition.y + gridLength / 2);

            managerHazard.raycastOrigin = new Vector3(rawGrid.x * trackSize + trackSize / 2, 10f, rawGrid.y * trackSize + trackSize / 2);

            #endregion

            #region Fetch Available Hazards

            //builderUI.usableHazards = builderUI.allHazards;
            #endregion

            if (Input.GetKeyDown(KeyCode.Q))
            {
                //SpawnTrack(localPlayer, false);

                //Vector3 hazardVector = new Vector3(currentPlayer.worldPosition.x, 0, currentPlayer.worldPosition.y);

                //managerHazard.raycastOrigin = hazardVector;
                //managerHazard.randomSpawn(0);


                builderUI.DisplayChange(1);

            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                //SpawnTrack(localPlayer, true);
                builderUI.DisplayChange(-1);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                managerHazard.SpawnHazard();
            }



        }


    }


    void SpawnTrack(Player localPlayer, bool alt)
    {
        Vector2 rawGrid = localPlayer.gridPosition / gridSize + new Vector2(gridLength, gridLength) / 2;

        bool gridIsOccupied = gridCheck[(int)rawGrid.x, (int)rawGrid.y];

        if (!gridIsOccupied)
        {
            Transform newTrackPiece = transform;

            if (alt)
                newTrackPiece = Instantiate(prefab_track_L).transform;
            else
                newTrackPiece = Instantiate(prefab_track_I).transform;

            if (alt)
            {
                newTrackPiece.eulerAngles += new Vector3(0, 180);
            }

            for (int i = 0; i < currentRotation; i++)
            {
                newTrackPiece.eulerAngles += new Vector3(0, 90);
            }


            newTrackPiece.position = new Vector3(rawGrid.x * trackSize + trackSize / 2, 0, rawGrid.y * trackSize + trackSize / 2);


            GameObject occupiedMarker = Instantiate(localPlayer.cursor.gameObject, buildPhaseTransform);
            occupiedMarker.GetComponent<Image>().sprite = alt ? occupied_L_Sprite : occupied_I_Sprite;
            occupiedMarker.GetComponent<Image>().color = Color.white;

            for (int i = 0; i < currentRotation; i++)
            {
                occupiedMarker.transform.eulerAngles -= new Vector3(0, 0, 90);
            }

            if (alt)
            {

                currentRotation += 1;
                if (currentRotation == 4)
                    currentRotation = 0;
            }



            gridCheck[(int)rawGrid.x, (int)rawGrid.y] = true;
        }
    }
}

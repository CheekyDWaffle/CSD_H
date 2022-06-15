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
        public Vector2 lerpPosition;
        public Vector2 gridPosition;
        public bool hasPlacedHazard;
    }

    [Header("Assign")]
    public GameObject buildMarkerDefault;
    private GameObject[] buildMarkers;
    public Player[] players;
    public Transform buildPhaseTransform;
    public HazardManager managerHazard;
    public Builder_UI_Manager[] builderUIArray;

    Transform cameraTransform;

    [Header("Settings")]
    public float cursorSpeed = 2;
    public int gridSize = 100;
    public int trackSize = 20;

    [Header("Read Only")]
    public bool[,] gridCheck;

    bool isInBuildMode = false;
    [HideInInspector]
    public float buildModeChangeTimer = -1;

    // Start is called before the first frame update
    void Start()
    {
        gridCheck = new bool[10, 10];
        cameraTransform = GetComponentInChildren<Camera>(true).transform;

								#region Marker Setup

								int maxPlayerNumber = 4;

        buildMarkers = new GameObject[maxPlayerNumber];

        for (int i = 0; i < maxPlayerNumber; i++)
        {
            buildMarkers[i] = (i == 0 ? buildMarkerDefault : Instantiate(buildMarkerDefault));
            buildMarkers[i].transform.SetParent(buildMarkerDefault.transform.parent);

            buildMarkers[i].GetComponent<Image>().color = Manager_UI.Get().colors[i];

            buildMarkers[i].name = "Build Marker (Player " + (i + 1) + ")";
            buildMarkers[i].SetActive(i == 0);
        }
        #endregion


        for (int i = 0; i < builderUIArray.Length; i++)
            builderUIArray[i].gameObject.SetActive(false);
        
    }

				// Update is called once per frame
				void Update()
    {
        #region Debug

        if (Input.GetAxis("Horizontal_C_1") != 0 || Input.GetAxis("Horizontal_C_2") != 0)
        {
            Debug.LogError("If this shows up, Talha will get upsetti-spaghetti |" + Input.GetAxis("Horizontal_C_1") + "|" + Input.GetAxis("Horizontal_C_2"));
        }

								#endregion


								#region Buildmode Transition
								GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerObjects.Length; i++)
        {
            Controller_Vehicle currentCar = playerObjects[i].GetComponent<Controller_Vehicle>();

            currentCar.GetComponentInChildren<Camera>(true).gameObject.SetActive(!isInBuildMode);
        }

        if (buildModeChangeTimer != -1)
        {
            buildModeChangeTimer -= Time.deltaTime;

            if (buildModeChangeTimer < 0)
            {
                isInBuildMode = !isInBuildMode;

                #region bandaid fix: Controller Input doesn't properly reset the "Place Hazard" input
                for (int i = 0; i < playerObjects.Length; i++)
                {
                    Controller_Vehicle currentCar = playerObjects[i].GetComponent<Controller_Vehicle>();
                    currentCar.NewInput.placeHazard = false; 
                }
																#endregion

																buildModeChangeTimer = -1;
            }
        }


        if (Input.GetKeyDown(KeyCode.F) && buildModeChangeTimer == -1)
            buildModeChangeTimer = Manager_UI.Get().Fade_Black();

        buildPhaseTransform.gameObject.SetActive(isInBuildMode);
        cameraTransform.GetComponent<Camera>().enabled = true;
        cameraTransform.GetComponent<Camera>().orthographicSize = 5.4f * trackSize;
        #endregion

        for (int i = 0; i < playerObjects.Length; i++)
            BuildPhase(i, playerObjects[i].GetComponent<Controller_Vehicle>());


								#region Check if all players are done
								bool allPlayersHasPlacedHazard = true;

        for (int i = 0; i < playerObjects.Length; i++)
            if (!players[i].hasPlacedHazard)
            {
                allPlayersHasPlacedHazard = false;
                break;
            }

        if(allPlayersHasPlacedHazard)
        {
            for (int i = 0; i < playerObjects.Length; i++)
                players[i].hasPlacedHazard = false;

            buildModeChangeTimer = Manager_UI.Get().Fade_Black();
        }
								#endregion

				}

				void BuildPhase(int playerIndex, Controller_Vehicle currentVehicle)
				{
								Player currentMarker = players[playerIndex];
        buildMarkers[playerIndex].SetActive(!currentMarker.hasPlacedHazard);
        builderUIArray[playerIndex].gameObject.SetActive(!currentMarker.hasPlacedHazard);

        currentVehicle.pauseCar = isInBuildMode;

        if (!isInBuildMode || currentMarker.hasPlacedHazard)
            return;

        #region Buildphase inputs, markers, etc
        // Input, which I fetch from the car script.
        currentMarker.lerpPosition += currentVehicle.NewInput.navigateBuild * cursorSpeed * Time.deltaTime;
        currentMarker.gridPosition = new Vector2(Mathf.Round(currentMarker.lerpPosition.x), Mathf.Round(currentMarker.lerpPosition.y));

        // Update the Marker UI element
        buildMarkers[playerIndex].transform.localPosition = (currentMarker.gridPosition * gridSize) + (Vector2.one * gridSize / 2); // This centers the marker on the UI.
        #endregion

        #region This is the part that interacts with the Hazard Manager

        // The Vector the Hazardmanager ends up readding
        Vector2 hazardVector = currentMarker.gridPosition * trackSize + Vector2.one * trackSize / 2 + Vector2.one * 10 * gridSize; // Gridsize to real world cordinates; Add half a track size to get to the center; Decenter it by 10 grids.

        managerHazard.raycastOrigin = new Vector3(hazardVector.x, 10f, hazardVector.y);
        managerHazard.testCube.transform.position = managerHazard.raycastOrigin;
        managerHazard.builderUI = builderUIArray[playerIndex];
        managerHazard.Getlist();

        if(currentVehicle.NewInput.navigateHazards != 0)
								{
            builderUIArray[playerIndex].DisplayChange(Mathf.RoundToInt(currentVehicle.NewInput.navigateHazards));
            currentVehicle.NewInput.navigateHazards = 0;
        }

        if (currentVehicle.NewInput.placeHazard)
        {
            currentVehicle.NewInput.placeHazard = false; // If this goes first, then the button resets even if the hazardmanager bugs out.
            currentMarker.hasPlacedHazard = managerHazard.SpawnHazard(); // returns True if it worked.

        }


            #endregion
        }
}

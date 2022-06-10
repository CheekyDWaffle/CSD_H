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
    }

    [Header("Assign")]
    public GameObject buildMarkerDefault;
    private GameObject[] buildMarkers;
    public Player[] players;
    public Transform buildPhaseTransform;
    public HazardManager managerHazard;
    public Builder_UI_Manager[] builderUIs;

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

        builderUIs[0].DisplayChange(0);
        builderUIs[1].DisplayChange(0);
    }

				// Update is called once per frame
				void Update()
    {
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
    }

    void BuildPhase(int playerIndex, Controller_Vehicle currentVehicle)
				{
								Player currentMarker = players[playerIndex];
        buildMarkers[playerIndex].SetActive(true);

        currentVehicle.pauseCar = isInBuildMode;

        if (!isInBuildMode)
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

        if (Input.GetKeyDown(KeyCode.Q))
            builderUIs[playerIndex].DisplayChange(1);

        if (Input.GetKeyDown(KeyCode.E))
            builderUIs[playerIndex].DisplayChange(-1);

        print(playerIndex);

								#endregion
				}
}

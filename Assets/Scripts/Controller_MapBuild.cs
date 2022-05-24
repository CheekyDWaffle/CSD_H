using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public MonoBehaviour hazardManager;
    public Canvas hazard_UI;

    [Header("Settings")]
    public float cursorSpeed = 2;
    public int gridSize = 100;
    public int gridLength = 10;
    public int trackSize = 20;

    [Header("Read Only")]
    public bool[,] gridCheck;

    bool isInBuildMode = true;
    float buildModeChangeTimer = -1;

    int currentRotation = 0;


    // Start is called before the first frame update
    void Start()
    {
        gridCheck = new bool [gridLength, gridLength];
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


            if (Input.GetKeyDown(KeyCode.LeftShift) && buildModeChangeTimer == -1)
                buildModeChangeTimer = Manager_UI.Get().Fade_Black();

            buildPhaseTransform.gameObject.SetActive(isInBuildMode);

            if (!isInBuildMode)
            {
                return;
            }
        }
        #endregion


        float timeStep = Time.deltaTime;

        Player localPlayer = players[0];


        int moveVertical = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        int moveHorizontal = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);

        localPlayer.cursorInput = new Vector2(moveHorizontal, moveVertical);


        for (int i = 0; i < players.Length; i++)
        {
            Player currentPlayer = players[0];

            currentPlayer.worldPosition += currentPlayer.cursorInput * cursorSpeed * timeStep;
            currentPlayer.gridPosition.x = Mathf.Round(currentPlayer.worldPosition.x / gridSize) * gridSize;
            currentPlayer.gridPosition.y = Mathf.Round(currentPlayer.worldPosition.y / gridSize) * gridSize;

            Vector2 gridUI = currentPlayer.gridPosition;
            gridUI.x += gridSize / 2;
            gridUI.y += gridSize / 2;

            currentPlayer.cursor.localPosition = gridUI;

    




            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnTrack(localPlayer, false);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                SpawnTrack(localPlayer, true);
            }

            if (Input.GetKey(KeyCode.Return))
            {
                //spawnMod trenger vector 3 til selected track som sendes til hazard manager, og åpner TrackMod selection menue.
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


            newTrackPiece.position = new Vector3(rawGrid.x * trackSize, 0, rawGrid.y * trackSize);


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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public RndHazardLocation randLock;
    public List<GameObject> trackModPrefab;
    public int hazardSpawnAmunt;
    public Builder_UI_Manager builderUI;
    List<Vector3> randVector3;

    [HideInInspector]
    public Vector3 raycastOrigin;
    [HideInInspector]
    public List<int> availableHazards;
    [HideInInspector]
    public TrackMemory selectedTrack;

    public GameObject testCube;
    private Vector3 lastVector3;

    private void Update()
    {

        if (raycastOrigin != lastVector3)
        {
            testCube.transform.position = raycastOrigin;
            //RaycastHit hit;
            Getlist();
        }
        lastVector3 = raycastOrigin;
    }

    public void SpawnHazard()
    {
        if (selectedTrack.availableHazards.Count != 0)
        {
            selectedTrack.availableHazards.Remove(builderUI.currentEnum);
            RandomSpawn();
            Getlist();
        }
    }
    public void RandomSpawn()
    {
        randVector3 = randLock.HazardLocations(raycastOrigin, hazardSpawnAmunt);
        for (int i = 0; i < randVector3.Count; i++)
        {
            Instantiate(trackModPrefab[builderUI.currentEnum], randVector3[i], Quaternion.identity);
            print("spawning " + builderUI.currentEnum);
        }
    }

    public void Getlist()
    {
        Collider[] hitCollider = Physics.OverlapSphere(raycastOrigin, 50);
        if (hitCollider.Length != 0)
        {
            foreach (Collider collider in hitCollider)
            {
                if (collider.GetComponent<TrackMemory>())
                {
                    selectedTrack = collider.GetComponent<TrackMemory>();
                    builderUI.TileChange(new List<int>(collider.GetComponent<TrackMemory>().availableHazards));
                    break;
                }
                else
                {
                    builderUI.TileChange(new List<int>());
                }
            }
        }
        else builderUI.TileChange(new List<int>());
    }
    public enum hazardEnum { oilspill, boulders, lazer }
}

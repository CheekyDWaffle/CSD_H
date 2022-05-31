using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public RndHazardLocation randLock;
    public List<GameObject> trackModPrefab;
    public Vector3 raycastOrigin;
    List<Vector3> randVector3;
  
    public void randomSpawn(int trackMod)
    {
        randVector3 = randLock.HazardLocations(raycastOrigin);
        for (int i = 0; i < randVector3.Count; i++)
        {
            Instantiate(trackModPrefab[trackMod], randVector3[i],Quaternion.identity);
        }
    }

    public void hazardSone()
    {

    }
    public enum hazardEnum { oilspill, boulders, lazer}
}

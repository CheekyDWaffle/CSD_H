using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RndHazardLocation : MonoBehaviour
{
    public Vector3 rayOrigin;

    public float rayMaxDist, radius;
    public int rayShots;
    public List<Vector3> rayHits;


    public List<Vector3> HazardLocations(Vector3 rayOrigin)
    {
        rayHits = new List<Vector3>();
        while (rayHits.Count < rayShots)
        {
            RaycastHit hit;
            Vector3 ranLoc = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            ranLoc += rayOrigin;

            if (Physics.Raycast(ranLoc, Vector3.down, out hit, rayMaxDist))
            {
                //check for valid hit
                rayHits.Add(hit.point);
            }
        }
        return rayHits;

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 item in rayHits)
        {
            Gizmos.DrawCube(item, Vector3.one * 5);
        }
    }
}

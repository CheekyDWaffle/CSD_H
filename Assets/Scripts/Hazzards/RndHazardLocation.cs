using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RndHazardLocation : MonoBehaviour
{
    public Vector3 rayOrigin;

    public float radius;
    public List<Vector3> rayHits;

    int loopBreak = 0;

    public List<Vector3> HazardLocations(Vector3 rayOrigin, int rayShots)
    {

        loopBreak = 1000;
        rayHits = new List<Vector3>();
        while (rayHits.Count < rayShots)
        {
            loopBreak--;
            RaycastHit hit;
            Vector3 ranLoc = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            ranLoc += rayOrigin;

            if (Physics.Raycast(ranLoc, Vector3.down, out hit, 1000))
            {
                //check for valid hit
                rayHits.Add(hit.point);
            }
            if (loopBreak <=0)
            {
                print("failed loop");
                print(rayOrigin);
                break;
            }

        }
        return rayHits;

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 item in rayHits)
        {
            Gizmos.DrawCube(item, Vector3.one);
        }
    }
}

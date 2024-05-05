using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackCreator : MonoBehaviour
{
    [SerializeField]
    private TextAsset trackFile;

    [SerializeField]
    private GameObject waypointPrefab;

    [SerializeField]
    private GameObject drone;

    private List<Vector3> waypoints;

    void Start()
    {
        waypoints = ParseFile.Parse(trackFile);

        foreach (Vector3 w in waypoints)
            Instantiate(waypointPrefab, w, Quaternion.identity, transform);

        Vector3 dir = Vector3.Normalize(new Vector3(waypoints[1].x - waypoints[0].x, 0, waypoints[1].z - waypoints[0].z));
        drone.transform.LookAt(drone.transform.position + dir, Vector3.up);
        drone.transform.position = waypoints[0];
    }
}

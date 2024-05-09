using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TrackCreator : MonoBehaviour
{
    [SerializeField]
    private TextAsset trackFile;

    [SerializeField]
    private Waypoint waypointPrefab;

    [SerializeField]
    private GameObject drone;

    private List<Vector3> waypointPositions = new List<Vector3>();

    private List<Waypoint> waypoints = new List<Waypoint>();

    void Start()
    {
        waypointPositions = ParseFile.Parse(trackFile);

        for (int i = 0; i < waypointPositions.Count; i++)
        {
            Waypoint w = Instantiate(waypointPrefab, waypointPositions[i], Quaternion.identity, transform);
            w.gameObject.SetActive(false);
            waypoints.Add(w);

            Func<int, UnityAction> y = (int idx) =>
            {
                return () =>
                {
                    if (waypoints.Count > idx + 1)
                        waypoints[idx + 1].gameObject.SetActive(true);
                };
            };
            w.OnDestroyed.AddListener(y(i));
        }
        waypoints[0].gameObject.SetActive(true);

        Vector3 dir = Vector3.Normalize(new Vector3(waypointPositions[1].x - waypointPositions[0].x, 0, waypointPositions[1].z - waypointPositions[0].z));
        drone.transform.LookAt(drone.transform.position + dir, Vector3.up);
        drone.transform.position = waypointPositions[0];
    }
}

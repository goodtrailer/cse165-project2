using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WaypointManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset trackFile;

    [SerializeField]
    private Waypoint waypointPrefab;

    [SerializeField]
    private GameObject waypointArrow;

    [SerializeField]
    private LineRenderer waypointLine;

    [SerializeField]
    private GameObject drone;

    [SerializeField]
    private GestureRecognizer gestureRecognizer;

    [SerializeField]
    private Stopwatch lapStopwatch;

    [SerializeField]
    private TextMeshProUGUI lapFinishedText;

    private GameObject centerEyeAnchor;

    private List<Vector3> waypointPositions = new List<Vector3>();
    private List<Waypoint> waypoints = new List<Waypoint>();
    private int currentWaypoint = 0;

    void Start()
    {
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor");

        gestureRecognizer.GetRecognizedEvent("Peace").AddListener(p => waypointLine.enabled = currentWaypoint < waypoints.Count && p);

        waypointPositions = ParseFile.Parse(trackFile);
        for (int i = 0; i < waypointPositions.Count; i++)
        {
            Waypoint w = Instantiate(waypointPrefab, waypointPositions[i], Quaternion.identity, transform);
            w.gameObject.SetActive(false);
            waypoints.Add(w);

            w.OnDestroyed.AddListener(() =>
            {
                currentWaypoint++;

                if (waypoints.Count > currentWaypoint)
                    waypoints[currentWaypoint].gameObject.SetActive(true);
                else
                {
                    lapStopwatch.StopStopwatch();
                    lapStopwatch.enabled = false;
                    lapFinishedText.text = "FINISHED";
                    waypointArrow.SetActive(false);
                    waypointLine.enabled = false;
                }
            });
        }
        waypoints[0].gameObject.SetActive(true);

        Vector3 dir = Vector3.Normalize(new Vector3(waypointPositions[1].x - waypointPositions[0].x, 0, waypointPositions[1].z - waypointPositions[0].z));
        drone.transform.LookAt(drone.transform.position + dir, Vector3.up);
        drone.transform.position = waypointPositions[0];
    }

    void Update()
    {
        if (waypoints.Count <= currentWaypoint)
            return;

        waypointArrow.transform.LookAt(waypoints[currentWaypoint].transform);

        waypointLine.positionCount = waypoints.Count - currentWaypoint + 1;

        List<Vector3> positions = new List<Vector3>();
        positions.Add(centerEyeAnchor.transform.position + 4 * centerEyeAnchor.transform.forward - 0.5f * centerEyeAnchor.transform.up);
        positions.AddRange(waypointPositions.GetRange(currentWaypoint, waypoints.Count - currentWaypoint));
        waypointLine.SetPositions(positions.ToArray());
    }
}

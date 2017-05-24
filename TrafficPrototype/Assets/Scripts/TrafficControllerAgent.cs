using System.Collections.Generic;
using System.Linq;
using Search;
using UnityEngine;

public class TrafficControllerAgent : MonoBehaviour {
    public GameObject WaypointContainer;

    public Waypoint[] DummyPath;

    private Waypoint[] waypoints;

    // Use this for initialization
    void Start() {
        var waypointsList = new List<Waypoint>();
        foreach (Transform child in WaypointContainer.transform) {
            var waypoint = child.GetComponent<Waypoint>();
            waypointsList.Add(waypoint);
        }
        waypoints = waypointsList.ToArray();
    }

    // Update is called once per frame

    void Update() {
    }

    public Waypoint[] CalculatePath(Vector3 transformPosition, Vector3 position) {
//        var path = AStar.FindPath(transformPosition, position, NextLocations);
//        Debug.Log(path);
//        return path.Select(LocationToWaypoint).ToArray();
        return DummyPath;
    }

    Vector3[] NextLocations(Vector3 v) {
        var waypoint = LocationToWaypoint(v);
        return waypoint.Neighbours.Select(n => n.transform.position).ToArray();
    }

    Waypoint LocationToWaypoint(Vector3 loc) {
        return waypoints.First(w => w.transform.position == loc);
    }
}
using System.Linq;
using Search;
using UnityEngine;

public class TrafficControllerAgent : MonoBehaviour {
    public GameObject WaypointContainer;

    public Waypoint[] DummyPath;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame

    void Update() {
    }

    public Waypoint[] CalculatePath(Waypoint start, Waypoint end) {
        var path = AStar.FindPath(start, end, NextLocations);
        return path.ToArray();
    }

    Waypoint[] NextLocations(Waypoint w) {
        return w.Neighbours.ToArray();
    }
}
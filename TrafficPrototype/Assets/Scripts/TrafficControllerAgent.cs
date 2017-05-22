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

    public Waypoint[] CalculatePath(Vector3 transformPosition, Vector3 position) {
        return DummyPath;
    }
}
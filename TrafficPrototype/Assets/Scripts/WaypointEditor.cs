using UnityEngine;

[RequireComponent(typeof(TrafficControllerAgent))]
public class WaypointEditor : MonoBehaviour {
    public GameObject WaypointPrefab;
    public GameObject ConfigurationPrefab;
    public bool EditMode;

    // Use this for initialization
    void Start() {
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColabScenario : MonoBehaviour {

    public float TrainArrivesAfter;
    public float PodsLeaveAfter;
    public float PeopleLeftAfter;

    public GameObject VehiclePrefab;

    public Waypoint[] WaypointsToClose;
    public Waypoint TrainStation;

    public Text TrainArrivingText;

    private TrafficControllerAgent TCA;
    
    // Use this for initialization
    void Start() {
        TrainArrivingText.text = "";
        TCA = FindObjectOfType<TrafficControllerAgent>();
        StartCoroutine(RunScenario());
    }

    // Update is called once per frame
    void Update() {
    }

    IEnumerator RunScenario() {
        yield return new WaitForSeconds(TrainArrivesAfter);
        TrainArrivingText.text = "Train arriving!";
        CloseWaypoints();
        EnableTrainStation();
        yield return new WaitForSeconds(PodsLeaveAfter);
        TrainArrivingText.text = "People leaving!";
        ReenableWaypoints();
        EnableStationSpawning();
        yield return new WaitForSeconds(PeopleLeftAfter);
        DisableStationSpawning();
        TrainArrivingText.text = "People left.";
    }

    private void DisableStationSpawning() {
        var sp = TrainStation.GetComponent<SpawnPoint>();
        sp.EnableSpawning = false;
        sp.RespawnAfterReachDest = false;
    }

    private void EnableTrainStation() {
        TrainStation.RoadEnd = true;
        TCA.ForceUpdateEndWaypoint();
    }

    private void EnableStationSpawning() {
        var sp = TrainStation.gameObject.AddComponent<SpawnPoint>();
        sp.SpawnsPerMinute = 20;
        sp.EnableSpawning = true;
        sp.VehiclePrefab = VehiclePrefab;
    }

    private void ReenableWaypoints() {
        foreach (var wp in WaypointsToClose) {
            wp.RoadEnd = true;
        }
        TCA.ForceUpdateEndWaypoint();
    }

    private void CloseWaypoints() {
        foreach (var wp in WaypointsToClose) {
            wp.RoadEnd = false;
        }
        TCA.ForceUpdateEndWaypoint();
    }
}
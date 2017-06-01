using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Waypoint))]
public class SpawnPoint : MonoBehaviour {
    public int SpawnsPerMinute;
    public bool EnableSpawning;
    public int MaxVehiclesSpawned;

    public GameObject VehiclePrefab;

    private Waypoint wp;
    private Vector3 directionToNeighbour;
    private LaneConfiguration lc;
    private Vector3[] laneLocs;
    private Waypoint[] endWaypoints;
    private float spawnDelay;
    private int vehiclesSpawned;

    private TrafficControllerAgent TCA;

    // Use this for initialization
    void Start() {
        wp = GetComponent<Waypoint>();
        TCA = FindObjectOfType<TrafficControllerAgent>();
        spawnDelay = Mathf.Pow(SpawnsPerMinute / 60f, -1);

        directionToNeighbour = XVector3.Direction(wp.transform.position, wp.Neighbours[0].transform.position);
        lc = TCA.GetLaneConfiguration(wp, wp.Neighbours[0]);

        endWaypoints = TCA.GetEndWaypoints();

        InitLanes();

        StartCoroutine(WaitSpawn());
    }

    private void InitLanes() {
        var lanes = new List<Vector3>();
        for (int i = 0; i < lc.NumberOfLeftLanes(); i++) {
            if (lc.LeftLaneOpen(i)) {
                lanes.Add(CalculateLaneLocation(i));
            }
        }
        laneLocs = lanes.ToArray();
    }

    // Update is called once per frame
    void Update() {
    }

    IEnumerator WaitSpawn() {
        while (EnableSpawning) {
            yield return new WaitForSeconds(spawnDelay);
            SpawnVehicle();
            if (MaxVehiclesSpawned > 0) {
                EnableSpawning = vehiclesSpawned != MaxVehiclesSpawned;    
            }
        }
    }

    private void SpawnVehicle() {
        var traffic = GameObject.FindGameObjectWithTag("TrafficContainer");
        var vehicle = Instantiate(VehiclePrefab, laneLocs[0], Quaternion.identity, traffic.transform);
        var navigationAgent = vehicle.GetComponent<NavigationAgent>();
        navigationAgent.TCA = TCA;
        navigationAgent.Destination = endWaypoints[Random.Range(0, endWaypoints.Length - 1)];
        vehiclesSpawned++;
    }

    private Vector3 CalculateLaneLocation(int lane) {
        var offset = DrivingAgent.GetLaneOffset(lane + 1);
        var left = Vector3.Cross(Vector3.up, directionToNeighbour).normalized;
        return wp.transform.position - left * offset;
    }
}
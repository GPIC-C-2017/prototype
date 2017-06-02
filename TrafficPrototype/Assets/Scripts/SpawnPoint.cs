using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Waypoint))]
public class SpawnPoint : MonoBehaviour {
    public int SpawnsPerMinute;
    public bool EnableSpawning;
    public int MaxVehiclesSpawned;

    public GameObject VehiclePrefab;

    public static float GlobalRatio = 1f;

    private Waypoint wp;
    private Vector3 directionToNeighbour;
    private LaneConfiguration lc;
    private Vector3[] laneLocs;
    private Waypoint[] endWaypoints;
    private float spawnDelay;
    private int vehiclesSpawned;

    private TrafficControllerAgent TCA;
    private GameObject trafficContainer;
    private GameObject lastSpawnedVehicle;

    void Awake() {
        wp = GetComponent<Waypoint>();
        TCA = FindObjectOfType<TrafficControllerAgent>();
        trafficContainer = GameObject.FindGameObjectWithTag("TrafficContainer");
    }

    // Use this for initialization
    void Start() {
        spawnDelay = Mathf.Pow(SpawnsPerMinute * GlobalRatio / 60f, -1);

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
        // Recalculate spawn delay to allow for runtime changes to values
        spawnDelay = Mathf.Pow(SpawnsPerMinute * GlobalRatio / 60f, -1);

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

    public void SpawnVehicle() {
        StartCoroutine(SpawnWhenClear());
    }

    IEnumerator SpawnWhenClear() {
        while (SpawnPointIsNotClear()) {
            yield return new WaitForSeconds(0.5f);
        }
        ForceSpawnVehicle();
    }

    private bool SpawnPointIsNotClear() {
        if (lastSpawnedVehicle == null)
            return false;
        float distance =  Vector3.Distance(
                   gameObject.transform.position,
                   lastSpawnedVehicle.transform.position);
        bool isClear = distance > lastSpawnedVehicle.GetComponent<Collider>().bounds.size.z * 1.3f;
        if (!isClear) {
            Debug.Log(distance);
        }
        return !isClear;
    }

    private void ForceSpawnVehicle() {
        var vehicle = Instantiate(VehiclePrefab, laneLocs[0], Quaternion.identity, trafficContainer.transform);
        var navigationAgent = vehicle.GetComponent<NavigationAgent>();
        navigationAgent.TCA = TCA;
        navigationAgent.Destination = RandomWaypointBesideCurrent();
        navigationAgent.StartingPoint = wp;
        vehicle.transform.LookAt(wp.Neighbours[0].transform);
        vehiclesSpawned++;
        lastSpawnedVehicle = vehicle;
    }

    private Waypoint RandomWaypointBesideCurrent() {
        var random = endWaypoints[Random.Range(0, endWaypoints.Length - 1)];
        while (wp == random) {
            random = endWaypoints[Random.Range(0, endWaypoints.Length - 1)];
        }

        return random;
    }

    private Vector3 CalculateLaneLocation(int lane) {
        var offset = DrivingAgent.GetLaneOffset(lane + 1);
        var left = Vector3.Cross(Vector3.up, directionToNeighbour).normalized;
        return wp.transform.position - left * offset;
    }
}
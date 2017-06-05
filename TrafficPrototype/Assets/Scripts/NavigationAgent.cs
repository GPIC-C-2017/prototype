using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/**
 * This is the agent which represents the navigation abilities of the vehicle,
 * and is responsible for:
 * - Requesting a path to the TrafficControllerAgent, and storing it
 * - Continously feeding the next junction of the path to the DrivingAgent, so we can move towards it
 * - Respond appropriately to the DrivingAgent's reports of permanent obstructions, by requesting a
 * 	  new path from the TrafficControllerAgent
 */
[RequireComponent(typeof(DrivingAgent))]
public class NavigationAgent : MonoBehaviour {
    public Waypoint StartingPoint;
    public Waypoint Destination;

    public bool respawnAtRandomWaypoint = true;

    public TrafficControllerAgent TCA;
    private DrivingAgent DA;
    private PerformanceMeasurer PF;

    public Waypoint[] path;
    private int headingToIndex = -1;

    private LaneConfiguration currentConf;

    private DateTime created;

    /**
     * This method is called by the DrivingAgent when it spots a permanent obstruction
     * on its path, which in turn requires a new path to be devised. 
     */
    public void ObstructionDetected() {
        // TODO
        // It should report the obstruction appropriately to the city traffic controller.
        // After that, ask for a new path to the same destination, which should now avoid
        // the obstruction. Finally, update inner state and feed the Driving agent with the
        // first junction of the new path, which should be directly behind the vehicle 
        // (i.e. should require the vehicle to do a U-turn).
    }

    public void RequestPathTo(Waypoint start, GameObject destinationNode) {
        // Request path from the current position, to the destination node.
        path = TCA.CalculatePath(start, destinationNode.GetComponent<Waypoint>());
        // Update inner state, and feed the DrivingAgent with the first junction
        // of the path.
        UpdateTarget();
    }

    // Use this for initialization
    void Awake() {
        DA = GetComponent<DrivingAgent>();
        PF = FindObjectOfType<PerformanceMeasurer>();
    }

    void Start() {
        created = DateTime.Now;
        if (Destination != null) {
            if (StartingPoint == null) {
                StartingPoint = ClosestWaypoint();
            }
            RequestPathTo(StartingPoint, Destination.gameObject);
        }
        else {
            Destroy(this);
        }
        
    }

    public TimeSpan JourneySecondsElapsed() {
        return DateTime.Now - created;
    }

    // Update is called once per frame
    void Update() {
        if (DA.ReachedCurrentTarget()) {
            if (headingToIndex == path.Length - 1) {
                if (PF != null)
                    PF.ReachedTarget();
                DestroyAndRespawnAtRandomWaypoint();
                return;
            }
            UpdateTarget();
        }
    }

    private Waypoint GetRandomSpawnPoint() {
        int index = Random.Range(0, TCA.GetSpawnPoints().Length - 1);
        return TCA.GetSpawnPoints()[index];
    }

    private Waypoint GetClosestSpawnPoint() {
        var nearbyWaypoints = TCA.GetSpawnPoints();
        Waypoint closest = nearbyWaypoints[0];
        var distance = Mathf.Infinity;
        foreach (var waypoint in nearbyWaypoints) {
            var diff = transform.position - waypoint.transform.position;
            var curDistance = diff.sqrMagnitude;
            if (curDistance < distance) {
                closest = waypoint;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void RequestSpawnFromWaypoint(Waypoint w) {
        SpawnPoint s = w.GetComponent<SpawnPoint>();
        
        if (s.EnableSpawning && s.RespawnAfterReachDest)
            s.SpawnVehicle();
    }

    public void DestroyAndRespawnAtRandomWaypoint() {
        if (respawnAtRandomWaypoint)
            RequestSpawnFromWaypoint(GetRandomSpawnPoint());
        Destroy(gameObject);
    }

    public void DestroyAndRespawnAtClosestWaypoint() {
        RequestSpawnFromWaypoint(GetClosestSpawnPoint());
        Destroy(gameObject);
    }

    // Sets the next target and updates the rest of the path accordingly
    void UpdateTarget() {
        Waypoint fromWp;
        Vector3 fromLoc;

        var previousIndex = headingToIndex;
        headingToIndex++;

        // if there is no previous waypoint
        // i.e. approaching first target
        if (headingToIndex == 0) {
            fromWp = ClosestWaypoint();
            fromLoc = gameObject.transform.position;
        }
        else {
            fromWp = path[previousIndex];
            fromLoc = fromWp.transform.position;
        }

        var toWp = path[headingToIndex];
        var toLoc = toWp.transform.position;

        var heading = toLoc - fromLoc;
        var direction = heading / heading.magnitude;

        var lc = TCA.GetLaneConfiguration(fromWp, toWp);

        Vector3 turnDirection;
		var currentLane = DA.CurrentLane;

		int lane;

		lane = DetermineLane (lc, direction, out turnDirection);

		var nextLane = 0;
		if (turnDirection == Vector3.left) {
			nextLane = -1 * lane;
		} else if (turnDirection == Vector3.right) {
			nextLane = 1 * lane;
		}

        if (toWp.Neighbours.Length <= 3 && lc != null) {
            // There are no options ahead. Just a bend. 
            // See if I can keep the current lane.

            lane = lc.LeftLaneOpen(currentLane) ? currentLane : lc.RightMost();
            nextLane = 0;

        }

		DA.SetLane(lane);
        DA.SetNextTarget(toLoc, direction, nextLane);

    }

    public Waypoint ClosestWaypoint() {
        var nearbyWaypoints = TCA.GetWaypoints();

        Waypoint closest = nearbyWaypoints[0];
        var distance = Mathf.Infinity;
        foreach (var waypoint in nearbyWaypoints) {
            var diff = transform.position - waypoint.transform.position;
            var curDistance = diff.sqrMagnitude;
            if (curDistance < distance) {
                closest = waypoint;
                distance = curDistance;
            }
        }

        return closest;
    }

    // we determine the lane by considering the angle between the previous target
    // and the 2nd next waypoint:
    // (2) ---- (3)
    //  |
    //  |
    // (1)
    // when travelling (1) -> (2), we'll need to keep to the right-most lane as we'll be turning right.
    private int DetermineLane(LaneConfiguration lc, Vector3 direction, out Vector3 willTurn) {
        // there is no previous waypoint
        willTurn = Vector3.zero;
        if (headingToIndex == 0)
            return 1;

        // if there is no next
        if (headingToIndex + 1 == path.Length)
            return 1;

        var previous = path[headingToIndex - 1].transform.position;
        var next = path[headingToIndex + 1].transform.position;

        var heading = next - previous;
        var distance = heading.magnitude;
        var targetDir = heading / distance; // This is now the normalized direction.

        var relativeDir = XVector3.AngleDir(direction, targetDir, Vector3.up);

        willTurn = relativeDir;

        return relativeDir == Vector3.left ? lc.LeftMost() : lc.RightMost();
    }
}
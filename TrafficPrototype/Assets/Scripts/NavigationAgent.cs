using System.Collections;
using UnityEngine;

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
    public Waypoint Destination;

    public TrafficControllerAgent TCA;
    private DrivingAgent DA;

    private Waypoint[] path;
    private int currentWaypoint;

    private LaneConfiguration currentConf;

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

    public void RequestPathTo(GameObject destinationNode) {
        // TODO
        // Request path from the current position, to the destination node.
        // Update inner state, and feed the DrivingAgent with the first junction
        //  of the path.
        path = TCA.CalculatePath(ClosestWaypoint(), destinationNode.GetComponent<Waypoint>());
        NextTarget();
    }

    // Use this for initialization
    void Start() {
        DA = GetComponent<DrivingAgent>();
        StartCoroutine(WaitAndGetPath());
    }

    // Update is called once per frame
    void Update() {
        if (DA.ReachedCurrentTarget()) NextTarget();
    }

    // Sets the next target and updates the rest of the path accordingly
    void NextTarget() {
        Waypoint fromWp;
        Vector3 fromLoc;
        // if there is no previous waypoint
        if (currentWaypoint == 0) {
            fromWp = ClosestWaypoint();
            fromLoc = gameObject.transform.position;
        }
        else {
            fromWp = path[currentWaypoint - 1];
            fromLoc = fromWp.transform.position;
        }

        var toWp = path[currentWaypoint];
        var toLoc = toWp.transform.position;

        var heading = fromLoc - toLoc;
        var direction = heading / heading.magnitude;

        var lc = TCA.GetLaneConfiguration(fromWp, toWp);
        
        Vector3 turnDirection;
        DA.SetLane(DetermineLane(lc, direction, out turnDirection));

        var nextLane = 0;
        if (turnDirection == Vector3.left) {
            nextLane = -1;
        } else if (turnDirection == Vector3.right) {
            nextLane = 1;
        }
        DA.SetNextTarget(toLoc, direction, nextLane);

        currentWaypoint++;
    }

    // wait a small delay before getting the path to the target
    // this ensures that the TCA has enough time to initiliase
    IEnumerator WaitAndGetPath() {
        yield return new WaitForSeconds(0.1f);
        if (Destination != null) RequestPathTo(Destination.gameObject);
    }

    Waypoint ClosestWaypoint() {
        var nearbyWaypoints = FindObjectsOfType<Waypoint>();

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
    int DetermineLane(LaneConfiguration lc, Vector3 direction, out Vector3 willTurn) {
        // there is no previous waypoint
        willTurn = Vector3.zero;
        if (currentWaypoint == 0)
            return 1;

        // if there is no next
        if (currentWaypoint + 1 == path.Length)
            return 1;

        if (lc.NumberOfLeftLanes() < 2)
            return 1;

        var previous = path[currentWaypoint - 1].transform.position;
        var current = path[currentWaypoint].transform.position;
        var next = path[currentWaypoint + 1].transform.position;
        
        var heading = next - previous;
        var distance = heading.magnitude;
        var targetDir = heading / distance; // This is now the normalized direction.

        var relativeDir = XVector3.AngleDir(direction, targetDir, Vector3.up);

        willTurn = relativeDir;
        return relativeDir == Vector3.left ? lc.LeftMost() : lc.RightMost();
    }


}
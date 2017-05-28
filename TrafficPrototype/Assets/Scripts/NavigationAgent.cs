using System.Collections;
using System.Linq;
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

    private Waypoint[] path;
    private DrivingAgent DA;

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
        
        path = TCA.CalculatePath(closest, destinationNode.GetComponent<Waypoint>());
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
        DA.SetNextTarget(path[0].transform);
        path = path.Skip(1).ToArray();
    }

    IEnumerator WaitAndGetPath() {
        yield return new WaitForSeconds(0.1f);
        if (Destination != null) RequestPathTo(Destination.gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This is the agent which represents the navigation abilities of the vehicle,
 * and is responsible for:
 * - Requesting a path to the TrafficControllerAgent, and storing it
 * - Continously feeding the next junction of the path to the DrivingAgent, so we can move towards it
 * - Respond appropriately to the DrivingAgent's reports of permanent obstructions, by requesting a
 * 	  new path from the TrafficControllerAgent
 */
[RequireComponent (typeof (DrivingAgent))]
public class NavigationAgent : MonoBehaviour {

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
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

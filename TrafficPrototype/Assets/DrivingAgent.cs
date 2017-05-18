using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This is the agent which represents the Autonomous Driving capabilities of a vehicle.
 * It is built on top of a 'VehicleAgent' and is able to:
 * - Navigate towards a set target, i.e. the next junction
 * - Use the pedals (VehicleAgent's Accelerate and Brake methods) to keep a safe, consistent speed
 * - Use the steering wheel (VehicleAgent's SteerLeft, and SteerRight methods) to follow the road
 * - Keeping the appropriate safety distance with the vehicle in front
 * - Brake suddenly to avoid close, fixed obstacles
 * - Change lanes and behave appropriately with regards to nearby pods
 * - Report permanent obstructions to the NavigationAgent (i.e. to request for a new target)
 **/
[RequireComponent (typeof (VehicleAgent))]
[RequireComponent (typeof (NavigationAgent))]
public class DrivingAgent : MonoBehaviour {

	public void SetNextTarget(Vector3 target) {
		// TODO
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


}

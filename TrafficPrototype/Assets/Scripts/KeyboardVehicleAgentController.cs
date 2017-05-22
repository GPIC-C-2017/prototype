using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * A simple script to control a VehicleAgent using the WASD keys.
 */
[RequireComponent (typeof (VehicleAgent))]
public class KeyboardVehicleAgentController : MonoBehaviour {

	public KeyCode forwardKey = KeyCode.W;
	public KeyCode backwardKey = KeyCode.S;
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;

	// Update is called once per frame
	void Update () {

		VehicleAgent vehicle = gameObject.GetComponent<VehicleAgent> ();

		if (Input.GetKey (forwardKey))
			vehicle.Accelerate ();

		if (Input.GetKey (backwardKey))
			vehicle.Brake ();

		if (Input.GetKey (leftKey))
			vehicle.SteerLeft ();

		if (Input.GetKey (rightKey))
			vehicle.SteerRight ();
		
	}

}

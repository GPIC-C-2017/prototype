using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (DrivingAgent))]
public class ChangeLayerAtJunctions : MonoBehaviour {

	public const bool avoidCollisionsAtJunctions = true;

	public float junctionsRadius = 10.0f;

	public const string vehiclesOnRoadsLayer = "VehiclesOnRoads";
	public const string vehiclesAtJunctionsLayer = "VehiclesAtJunctions";

	private int vehiclesOnRoadsLayerMask;
	private int vehiclesAtJunctionsLayerMask;
	
	private DrivingAgent vehicle;
	
	// Use this for initialization
	void Start () {
		vehicle = gameObject.GetComponent<DrivingAgent>();
		
		vehiclesAtJunctionsLayerMask = LayerMask.NameToLayer(vehiclesAtJunctionsLayer);
		vehiclesOnRoadsLayerMask = LayerMask.NameToLayer(vehiclesOnRoadsLayer);
		
		if (avoidCollisionsAtJunctions)
			Physics.IgnoreLayerCollision(
				vehiclesAtJunctionsLayerMask, 
				vehiclesAtJunctionsLayerMask
			);
		
	}
	
	// Update is called once per frame
	void Update () {

		if (VehicleAtJunction()) {
			gameObject.layer = vehiclesAtJunctionsLayerMask;

		} else {
			gameObject.layer = vehiclesOnRoadsLayerMask;

		}

	}

	private bool VehicleAtJunction() {
		return vehicle.IsAtJunction(junctionsRadius);
	}
	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (DrivingAgent))]
public class ChangeLayerAtJunctions : MonoBehaviour {

	public const bool AvoidCollisionsAtJunctions = true;

	public float JunctionsRadius = 10.0f;

	public const string VehiclesOnRoadsLayer = "VehiclesOnRoads";
	public const string VehiclesAtJunctionsLayer = "VehiclesAtJunctions";

	private int vehiclesOnRoadsLayerMask;
	private int vehiclesAtJunctionsLayerMask;
	
	private DrivingAgent vehicle;
	
	// Use this for initialization
	void Start () {
		vehicle = gameObject.GetComponent<DrivingAgent>();
		
		vehiclesAtJunctionsLayerMask = LayerMask.NameToLayer(VehiclesAtJunctionsLayer);
		vehiclesOnRoadsLayerMask = LayerMask.NameToLayer(VehiclesOnRoadsLayer);
		
		if (AvoidCollisionsAtJunctions)
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
		return vehicle.IsAtJunction(JunctionsRadius);
	}
	
}

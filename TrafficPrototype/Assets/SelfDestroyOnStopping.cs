using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

[RequireComponent( typeof(VehicleAgent))]
[RequireComponent( typeof(NavigationAgent))]
public class SelfDestroyOnStopping : MonoBehaviour {

	public float MinSpeedThreshold = 1f;
	public float DestroyTimer = 3f;

	private VehicleAgent vehicle;
	
	void Start() {
		vehicle = gameObject.GetComponent<VehicleAgent>();
	}
		
	// Update is called once per frame
	void Update () {
		if (IsStopped()) {
			StartCoroutine(WaitAndCheckAgain());
		}
	}
	
	IEnumerator WaitAndCheckAgain() {
		yield return new WaitForSeconds(DestroyTimer);
		if (IsStopped()) {
			gameObject.GetComponent<NavigationAgent>().DestroyAndRespawnAtRandomWaypoint();
		}
	}

	bool IsStopped() {
		return vehicle.GetCurrentSpeed() < MinSpeedThreshold;
	}
}

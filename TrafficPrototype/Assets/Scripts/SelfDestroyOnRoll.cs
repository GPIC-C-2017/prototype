using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (NavigationAgent))]
public class SelfDestroyOnRoll : MonoBehaviour {

	public float RotationThreshold = 2f;
	
	// Update is called once per frame
	void Update () {
		if (IsOverThreshold(gameObject.transform.rotation.eulerAngles.x)
		    || IsOverThreshold(gameObject.transform.rotation.eulerAngles.z)) {
			gameObject.GetComponent<NavigationAgent>().DestroyAndRespawnAtRandomWaypoint();
		}
	}

	private bool IsOverThreshold(float eulerAngle) {
		return !(eulerAngle > 360 - RotationThreshold ||
				 eulerAngle < RotationThreshold) ;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * A stupid Navigation agent, which sets a single objective
 * when starting the environment 
 */
[RequireComponent (typeof (DrivingAgent))]
public class DummyNavigationAgent : MonoBehaviour {

	public GameObject navigationTarget;

	// Use this for initialization
	void Update () {

		gameObject.GetComponent<DrivingAgent> ().SetNextTarget (navigationTarget.transform);
		
	}

}

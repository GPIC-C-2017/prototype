using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Keep the same relative position to another object at all times.
 */

public class FollowObject : MonoBehaviour {

	public GameObject objectToFollow;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
		if (objectToFollow != null) {
			// Calculate and store the relative position offset between the two objects.
			offset = gameObject.transform.position - objectToFollow.transform.position;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (objectToFollow != null) {
			// Reposition the game object so it has the same offset as it originally did.
			gameObject.transform.position = objectToFollow.transform.position + offset;
		}
	}
}

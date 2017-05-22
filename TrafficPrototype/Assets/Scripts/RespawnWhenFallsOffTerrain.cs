using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Respawn the object when it falls from the terrain (i.e. the Y position is lower than
 * a given value) at its original position.
 */

[RequireComponent (typeof (Rigidbody))]
public class RespawnWhenFallsOffTerrain : MonoBehaviour {

	[Range (-1000.0f, 0f)]
	public float respawnWhenFallingLowerThanY = -4f;

	public bool cancelAllForcesWhenRespawing = true;

	private Vector3 initialPosition;
	private Quaternion initialRotation;

	void Start () {
		// Store the initial coordinates of the object
		initialPosition = gameObject.transform.position;
		initialRotation = gameObject.transform.rotation;
	}
	
	void Update () {
		if (NeedsRespawning ())
			Respawn ();
	}
		
	private bool NeedsRespawning() {
		return gameObject.transform.position.y < respawnWhenFallingLowerThanY;
	}

	private void Respawn() {
		gameObject.transform.position = initialPosition;
		gameObject.transform.rotation = initialRotation;

		if (cancelAllForcesWhenRespawing) {
			Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

}

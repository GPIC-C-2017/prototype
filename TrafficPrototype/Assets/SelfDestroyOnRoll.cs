using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroyOnRoll : MonoBehaviour {

	public float rotationThreshold = 2f;
	
	// Update is called once per frame
	void Update () {
		if (IsOverThreshold(gameObject.transform.rotation.eulerAngles.x)
		    || IsOverThreshold(gameObject.transform.rotation.eulerAngles.z)) {
			Debug.Log("-");
			Debug.Log(gameObject.transform.rotation.eulerAngles.x);
			Debug.Log(gameObject.transform.rotation.eulerAngles.z);
			Destroy(gameObject);
		}
	}

	private bool IsOverThreshold(float eulerAngle) {
		return !(eulerAngle > 360 - rotationThreshold ||
				 eulerAngle < rotationThreshold) ;
	}
}

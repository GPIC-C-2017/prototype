using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LanesChoice {
	Internal,
	External
};

[RequireComponent (typeof(LaneConfiguration))]
public class CloseAndReopenLanes : MonoBehaviour {

	public int lanesToClose;
	public float seconds;
	public LanesChoice startFrom;
	

	public GameObject obstacle;

	private LaneConfiguration LC;

	// Use this for initialization
	void Start () {
		LC = gameObject.GetComponent<LaneConfiguration> ();
		StartCoroutine(WaitAndClose());
	}

	IEnumerator WaitAndClose() {
		yield return new WaitForSeconds (seconds);
		StartCoroutine(WaitAndActivateObstacle());
		CloseLanes ();
		StartCoroutine (WaitAndOpen());
	}

	IEnumerator WaitAndActivateObstacle() {
		yield return new WaitForSeconds(seconds / 3);
		obstacle.SetActive(true);

	}

	IEnumerator WaitAndDeactivateObstacle() {
		
		yield return new WaitForSeconds(0.00001f);
		//yield return new WaitForSeconds(seconds / 2);
		obstacle.SetActive(false);

	}

	private void CloseLanes() {
		SetLanes ('0');
		Debug.Log("Lanes closed.");
	}
	private void OpenLanes() {
		SetLanes ('1');
		Debug.Log("Lanes opened.");
	}

	private void SetLanes(char newStatus) {

		if (startFrom == LanesChoice.External) {

			char[] LeftLanes = LC.LeftLanes.ToCharArray();
			for (int i = LeftLanes.Length - 1; i >= LeftLanes.Length - lanesToClose - 1; i--) {
				LeftLanes[i] = newStatus;
			}
			LC.LeftLanes = new string(LeftLanes);

			char[] RightLanes = LC.RightLanes.ToCharArray();
			for (int i = RightLanes.Length - 1; i >= RightLanes.Length - lanesToClose - 1; i--) {
				RightLanes[i] = newStatus;
			}
			LC.RightLanes = new string(RightLanes);

		}
		else {
			
			char[] LeftLanes = LC.LeftLanes.ToCharArray();
			for (int i = 0; i < LeftLanes.Length; i++) {
				LeftLanes [i] = newStatus;
			}
			LC.LeftLanes = new string(LeftLanes);
		
			char[] RightLanes = LC.RightLanes.ToCharArray();
			for (int i = 0; i < RightLanes.Length; i++) {
				RightLanes [i] = newStatus;
			}
			LC.RightLanes = new string(RightLanes);

		}

	}

	IEnumerator WaitAndOpen() {
		yield return new WaitForSeconds (seconds);
		StartCoroutine(WaitAndDeactivateObstacle());
		OpenLanes ();
		StartCoroutine (WaitAndClose());
	}

}

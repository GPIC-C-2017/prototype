using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadBalancing : MonoBehaviour {

	public Waypoint WaypointToClose;
	public float CloseAfterSeconds;
	public float ReopenAfterSeconds;

    public bool CloseLanes;

	public GameObject Obstacle;

	private Waypoint[] previousNeighbours;

	// Use this for initialization
	void Start () {
		previousNeighbours = WaypointToClose.Neighbours;
		StartCoroutine(CloseAfterTime());
		Obstacle.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator CloseAfterTime() {
		yield return new WaitForSeconds(CloseAfterSeconds);
        if (CloseLanes)
    		CloseWaypoint();
		Obstacle.SetActive(true);
		StartCoroutine(ReopenAfterTime());
	}

	IEnumerator ReopenAfterTime() {
		yield return new WaitForSeconds(ReopenAfterSeconds);
        if (CloseLanes)
            OpenWaypoint();
		Obstacle.SetActive(false);
		StartCoroutine(CloseAfterTime());
	}

	private void OpenWaypoint() {
		WaypointToClose.Neighbours = previousNeighbours;
	}

	private void CloseWaypoint() {
		WaypointToClose.Neighbours = new Waypoint[] { };
	}
}

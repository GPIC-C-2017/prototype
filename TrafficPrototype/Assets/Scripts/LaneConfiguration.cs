using UnityEngine;

public class LaneConfiguration : MonoBehaviour {
	public Waypoint From;
	public Waypoint To;

	public string LeftLanes;
	public string RightLanes;

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));

		if (From != null) {
			Gizmos.DrawLine(transform.position, From.transform.position);
		}

		if (To != null) {
			Gizmos.DrawLine(transform.position, To.transform.position);
		}
	}
}
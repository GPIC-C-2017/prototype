using System;
using System.Collections.Specialized;
using UnityEngine;

public class LaneConfiguration : MonoBehaviour {
	public Waypoint From;
	public Waypoint To;

	public string LeftLanes = "1";
	public string RightLanes = "1";

	public const char LaneEnabled = '1';
	public const char LaneDisabled = '0';

	void Start() {
	}

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

	public bool LeftLaneOpen(int index) {
		return LeftLanes[index] == LaneEnabled;
	}
	
	public bool RightLaneOpen(int index) {
		return RightLanes[index] == LaneEnabled;
	}

	public int NumberOfLeftLanes() {
		return LeftLanes.Length;
	}

	public int NumberOfRightLanes() {
		return RightLanes.Length;
	}

	public int LeftMost() {
		for (int i = 0; i < LeftLanes.Length; i++) {
			if (LeftLanes[i] == LaneEnabled) {
				return i + 1;
			}
		}

		throw new ArgumentOutOfRangeException();
	}
	
	public int RightMost() {
		for (int i = LeftLanes.Length - 1; i >= 0; i--) {
			if (LeftLanes[i] == LaneEnabled) {
				return i + 1;
			}
		}

		throw new ArgumentOutOfRangeException();
	}
}
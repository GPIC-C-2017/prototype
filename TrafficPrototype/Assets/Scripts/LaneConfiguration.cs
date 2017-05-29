using System;
using System.Collections.Specialized;
using UnityEngine;

public class LaneConfiguration : MonoBehaviour {
	public Waypoint From;
	public Waypoint To;

	public string LeftLanes;
	public string RightLanes;

	private BitVector32 leftConf;
	private BitVector32 rightConf;

	void Start() {
		leftConf = CreateBitVector(LeftLanes);
		rightConf = CreateBitVector(RightLanes);
	}

	BitVector32 CreateBitVector(string conf) {
		var vector = new BitVector32();
		for (var i = 0; i < conf.Length; i++) {
			var i1 = int.Parse(conf[i].ToString());
			vector[i] = i1 == 1;
		}
		return vector;
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

	public bool LeftLaneStatus(int index) {
		return leftConf[index];
	}
	
	public bool RightLaneStatus(int index) {
		return rightConf[index];
	}

	public int NumberOfLeftLanes() {
		return LeftLanes.Length;
	}

	public int NumberOfRightLanes() {
		return RightLanes.Length;
	}

	public int RightMost() {
		for (int i = 0; i < LeftLanes.Length; i++) {
			if (leftConf[i]) {
				return i + 1;
			}
		}

		return -1;
	}
	
	public int LeftMost() {
		for (int i = LeftLanes.Length - 1; i != 0; i--) {
			if (leftConf[i]) {
				return i + 1;
			}
		}

		return -1;
	}
}
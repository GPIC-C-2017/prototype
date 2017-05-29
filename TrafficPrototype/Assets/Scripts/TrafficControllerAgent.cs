using System.Collections.Generic;
using System.Linq;
using Search;
using UnityEngine;

public class TrafficControllerAgent : MonoBehaviour {
    public GameObject WaypointContainer;

    private LaneConfiguration[] laneConfigurations;

    // Use this for initialization
    void Start() {
        var lanes = new List<LaneConfiguration>();
        foreach (Transform child in WaypointContainer.transform) {
            var lc = child.GetComponent<LaneConfiguration>();
            if (lc != null) {
                lanes.Add(lc);
            }
        }
        laneConfigurations = lanes.ToArray();
    }

    // Update is called once per frame

    void Update() {
    }

    public Waypoint[] CalculatePath(Waypoint start, Waypoint end) {
        var path = AStar.FindPath(start, end, NextLocations);
        return path.ToArray();
    }

    public LaneConfiguration GetLaneConfiguration(Waypoint from, Waypoint to) {
        LaneConfiguration lc;
        return FindLaneConf(from, to, out lc) ? CorrectOrder(from, to, lc) : null;
    }

    private bool FindLaneConf(Waypoint from, Waypoint to, out LaneConfiguration lc) {
        foreach (var laneConf in laneConfigurations) {
            if (laneConf.From == from && laneConf.To == to) {
                lc = laneConf;
                return true;
            }

            if (laneConf.From == to && laneConf.To == from) {
                lc = laneConf;
                return true;
            } 
        }

        lc = default(LaneConfiguration);
        return false;
    }

    private LaneConfiguration CorrectOrder(Waypoint from, Waypoint to, LaneConfiguration lc) {
        if (lc.From == from && lc.To == to) {
            return lc;
        }

        var nlc = WaypointContainer.AddComponent<LaneConfiguration>();
        nlc.From = to;
        nlc.To = from;
        nlc.LeftLanes = lc.RightLanes;
        nlc.RightLanes = lc.LeftLanes;
        return nlc;
    }

    private Waypoint[] NextLocations(Waypoint w) {
        return w.Neighbours.ToArray();
    }
}
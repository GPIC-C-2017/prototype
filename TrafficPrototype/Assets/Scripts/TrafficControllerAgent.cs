using System.Collections.Generic;
using Search;
using UnityEngine;

public class TrafficControllerAgent : MonoBehaviour {
    public GameObject WaypointContainer;
    public GameObject LaneConfigurationPrefab;

    private List<LaneConfiguration> laneConfigurations;
    private Waypoint[] endWaypoints;

    void Awake() {
        var lanes = new List<LaneConfiguration>();
        var wps = new List<Waypoint>();
        
        foreach (Transform child in WaypointContainer.transform) {
            var lc = child.GetComponent<LaneConfiguration>();
            if (lc != null) {
                lanes.Add(lc);
            }

            var wp = child.GetComponent<Waypoint>();
            if (wp != null && wp.RoadEnd) {
                wps.Add(wp);
            }
        }
       
        foreach (var lc in lanes.ToArray()) {
            lanes.Add(CreateReverse(lc));
        }
        
        laneConfigurations = lanes;
        
        endWaypoints = wps.ToArray();
    }

    // Update is called once per frame
    void Update() {
    }

    public Waypoint[] GetEndWaypoints() {
        return endWaypoints;
    }

    public Waypoint[] CalculatePath(Waypoint start, Waypoint end) {
        var path = AStar.FindPath(start, end, w => w.Neighbours);
        return path.ToArray();
    }

    public LaneConfiguration GetLaneConfiguration(Waypoint from, Waypoint to) {
        LaneConfiguration lc;
        return FindLaneConf(from, to, out lc) ? lc : null;
    }

    private bool FindLaneConf(Waypoint from, Waypoint to, out LaneConfiguration lc) {
        foreach (var laneConf in laneConfigurations) {
            if (laneConf.From == from && laneConf.To == to) {
                lc = laneConf;
                return true;
            }
        }
        lc = default(LaneConfiguration);
        return false;
    }

    private LaneConfiguration CreateReverse(LaneConfiguration lc) {
        var midPoint = (lc.To.transform.position - lc.From.transform.position) * 0.5f;
        midPoint += lc.From.transform.position;
        
        var nlcGameObject = Instantiate(LaneConfigurationPrefab, midPoint, Quaternion.identity, WaypointContainer.transform);
        nlcGameObject.name = "LaneConfiguration-Reverse";
        
        var nlc = nlcGameObject.GetComponent<LaneConfiguration>();
        nlc.From = lc.To;
        nlc.To = lc.From;
        nlc.LeftLanes = lc.RightLanes;
        nlc.RightLanes = lc.LeftLanes;
        
        return nlc;
    }
}
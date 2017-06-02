using System.Collections.Generic;
using Search;
using UnityEngine;

public class TrafficControllerAgent : MonoBehaviour {
    public GameObject WaypointContainer;
    public GameObject LaneConfigurationPrefab;

    private List<LaneConfiguration> laneConfigurations;
    private Waypoint[] endWaypoints;
    private Waypoint[] waypointsCache;
    private Waypoint[] spawnPointsCache;

    void Awake() {
        var lanes = new List<LaneConfiguration>();
        var _endWaypoints = new List<Waypoint>();
        var waypoints = new List<Waypoint>();
        var spawnpoints = new List<Waypoint>();
        
        foreach (Transform child in WaypointContainer.transform) {
            var lc = child.GetComponent<LaneConfiguration>();
            if (lc != null) {
                lanes.Add(lc);
            }

            var wp = child.GetComponent<Waypoint>();
            if (wp != null) {
                if (wp.RoadEnd) {
                    _endWaypoints.Add(wp);
                }
                else {
                    waypoints.Add(wp);
                }
                if (wp.GetComponent<SpawnPoint>() != null) {
                    spawnpoints.Add(wp);
                }
                
            }
        }
       
        foreach (var lc in lanes.ToArray()) {
            lanes.Add(CreateReverse(lc));
        }
        
        laneConfigurations = lanes;

        endWaypoints = _endWaypoints.ToArray();
        waypointsCache = waypoints.ToArray();
        spawnPointsCache = spawnpoints.ToArray();
    }

    public Waypoint[] GetEndWaypoints() {
        return endWaypoints;
    }

    public Waypoint[] GetWaypoints() {
        return waypointsCache;
    }

    public Waypoint[] GetSpawnPoints() {
        return spawnPointsCache;
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
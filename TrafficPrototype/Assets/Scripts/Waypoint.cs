using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Waypoint : MonoBehaviour {
    public Waypoint[] Neighbours;

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        foreach (var neighbour in Neighbours) {
            if (neighbour != null) {
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }

    void Start() {
    }

    void Update() {
    }

    public void AddNeighbour(Waypoint waypoint) {
        if (waypoint == this || Neighbours.Contains(waypoint)) return;

        var nonNull = Neighbours.Where(n => n != null);
        var neighbours = new List<Waypoint>(nonNull) {waypoint};
        Neighbours = neighbours.ToArray();
        EditorUtility.SetDirty(this);
    }

    public void ClearNeighbours() {
        Neighbours = new Waypoint[] { };
    }

    public void RemoveNeighbour(Waypoint waypoint) {
        if (waypoint == this || !Neighbours.Contains(waypoint)) return;

        Neighbours = Neighbours.Where(n => n != waypoint).ToArray();
    }
}

[CustomEditor(typeof(Waypoint))]
[CanEditMultipleObjects]
public class WaypointExt : Editor {
    private WaypointEditor wpEditor;

    void OnEnable() {
        wpEditor = FindObjectOfType<WaypointEditor>();
    }

    void OnSceneGUI() {
        var e = Event.current;
        var selected = SelectedWaypoints();

        if (e.type == EventType.KeyDown && selected.Count > 1) {
            if (e.keyCode == KeyCode.Slash) {
                ConnectWaypoints(selected);
            }

            if (e.keyCode == KeyCode.Backslash) {
                DisconnectWaypoints(selected);
            }

            if (selected.Count == 2 && e.keyCode == KeyCode.Quote) {
                ConfigureLanes(selected);
            }
        }
    }

    void ConfigureLanes(List<Waypoint> selected) {
        var from = selected[0];
        var to = selected[1];
        
        var configs = FindObjectsOfType<LaneConfiguration>();
        foreach (var config in configs) {
            // if this configuration already exists, don't create a new one
            if (config.From == from && config.To == to) return;
            
            // if the reverse configuration already exists, don't create a new one
            if (config.From == to && config.To == from) return;
        }
        
        var midPoint = (to.transform.position - from.transform.position) * 0.5f;
        midPoint += from.transform.position;
        
        var obj = Instantiate(wpEditor.ConfigurationPrefab, midPoint, Quaternion.identity, wpEditor.transform);
        obj.name = "LaneConfiguration";
        
        var conf = obj.GetComponent<LaneConfiguration>();
        conf.From = from;
        conf.To = to;
    }

    void DisconnectWaypoints(List<Waypoint> selected) {
        foreach (var waypoint in selected) {
            foreach (var wayp in selected) {
                waypoint.RemoveNeighbour(wayp);
            }
        }
    }

    void ConnectWaypoints(List<Waypoint> selected) {
        foreach (var waypoint in selected) {
            foreach (var wayp in selected) {
                waypoint.AddNeighbour(wayp);
            }
        }
    }

    List<Waypoint> SelectedWaypoints() {
        return Selection.gameObjects
            .Where(go => go.GetComponent<Waypoint>())
            .Select(go => go.GetComponent<Waypoint>())
            .ToList();
    }
}
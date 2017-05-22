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
    void OnSceneGUI() {
        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            if (Selection.gameObjects.Length > 0 && e.keyCode == KeyCode.Slash) {
                ConnectWaypoints();
            }

            if (Selection.gameObjects.Length > 0 && e.keyCode == KeyCode.Backslash) {
                DisconnectWaypoints();
            }
        }
    }

    void DisconnectWaypoints() {
        var selected = SelectedWaypoints();
        foreach (var waypoint in selected) {
            foreach (var wayp in selected) {
                waypoint.RemoveNeighbour(wayp);
            }
        }
    }

    void ConnectWaypoints() {
        var selected = SelectedWaypoints();

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
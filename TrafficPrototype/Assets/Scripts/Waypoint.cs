using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Waypoint : MonoBehaviour {
    public Waypoint[] Neighbours;

    void OnDrawGizmosSelected() {
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
}
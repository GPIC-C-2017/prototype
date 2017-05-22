using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class PathEditor : EditorWindow {
    private Waypoint[] waypoints;
    private IList<Waypoint> selected = new List<Waypoint>();

    private Vector2 scrollPosition;

    [MenuItem("Paths/Path Editor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(PathEditor));
    }

    void OnSelectionChange() {
        selected = Selection.gameObjects
            .Where(g => g.GetComponent<Waypoint>() != null)
            .Select(g => g.GetComponent<Waypoint>())
            .ToList();

        Repaint();
    }

    void OnGUI() {
        if (selected.Any())
            WithSelected();
    }

    void WithSelected() {
        EditorGUILayout.LabelField("Selected waypoints");
        EditorGUILayout.Separator();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            foreach (var waypoint in selected) {
                EditorGUILayout.LabelField(waypoint.name);
            }
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Clear Neighbours")) {
            foreach (var waypoint in selected) {
                waypoint.ClearNeighbours();
            }
        }

        if (GUILayout.Button("Connect waypoints")) {
            foreach (var waypoint in selected) {
                foreach (var wayp in selected) {
                    waypoint.AddNeighbour(wayp);
                }
            }
        }
    }

//    private void WaypointCreator()
//    {
//        EditorGUILayout.BeginHorizontal();
//        {
//            EditorGUILayout.LabelField("Create waypoint");
//            if (GUILayout.Button("Create Waypoint"))
//            {
//            }
//        }
//        EditorGUILayout.EndHorizontal();
//        var pos = Vector3.zero;
//        pos = EditorGUILayout.Vector3Field("Position", pos);
//    }
}
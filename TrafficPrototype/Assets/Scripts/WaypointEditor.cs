using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TrafficControllerAgent))]
public class WaypointEditor : MonoBehaviour {
    public GameObject WaypointPrefab;
    public bool EditMode;

    // Use this for initialization
    void Start() {
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;

        // go through children
        foreach (Transform child in transform) {
            Gizmos.DrawCube(child.position, new Vector3(0.5f, 0.5f, 0.5f));
            var waypointObj = child.GetComponent<Waypoint>();
            foreach (var neighbour in waypointObj.Neighbours) {
                Gizmos.DrawLine(child.position, neighbour ? neighbour.transform.position : Vector3.zero);
            }
        }
    }
}

[CustomEditor(typeof(WaypointEditor))]
public class WaypointEditorExt : Editor {
    GameObject waypointPrefab;
    WaypointEditor editor;

    void OnEnable() {
        editor = target as WaypointEditor;
        waypointPrefab = editor.WaypointPrefab;
    }

    void OnSceneGUI() {
        if (!editor.EditMode) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.K) {
            RaycastHit hit;
            if (MouseToScene(e, out hit)) {
                CreateWaypoint(hit.point);
            }
        }
    }

    bool MouseToScene(Event e, out RaycastHit result) {
        // voodoo magic from Simply A* free asset
        Vector3 mousePos = new Vector3(e.mousePosition.x,
            -e.mousePosition.y + SceneView.lastActiveSceneView.camera.pixelHeight);
        Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out result, Mathf.Infinity);
    }

    void CreateWaypoint(Vector3 pos) {
        var obj = Instantiate(waypointPrefab, pos, Quaternion.identity, editor.transform);
        obj.name = "Waypoint";
    }

//	public override void OnInspectorGUI()
//	{
//		EditorGUILayout.BeginHorizontal();
//		{
//			EditorGUILayout.LabelField("Status: " + (EditEnabled ? "Active" : "Disabled"));
//			if (GUILayout.Button("Active"))
//				EditEnabled = !EditEnabled;
//		}
//		EditorGUILayout.EndHorizontal();
//
//	}
}
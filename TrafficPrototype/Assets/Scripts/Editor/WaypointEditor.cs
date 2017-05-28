using UnityEditor;
using UnityEngine;

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
        if (e.type == EventType.KeyDown) {
            if (e.keyCode == KeyCode.K) {
                RaycastHit hit;
                if (MouseToScene(e, out hit)) {
                    CreateWaypoint(hit.point);
                }
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
}
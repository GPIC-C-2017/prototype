using UnityEngine;

public static class XVector3 {
    public static Vector3 AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        var dir = Vector3.Dot(perp, up);

        if (dir > 0.0f) {
            return Vector3.left;
        }
        else if (dir < 0.0f) {
            return Vector3.right;
        }
        else {
            return Vector3.forward;
        }
    }        
}
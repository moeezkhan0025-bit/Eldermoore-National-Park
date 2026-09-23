#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Custom editor: draws DRAGGABLE handles for each PatrolPath waypoint in the Scene
// view, plus buttons to add/remove points. Drag the handles to shape the patrol
// route visually. IMPORTANT: this file must live in a folder named "Editor".
[CustomEditor(typeof(PatrolPath))]
public class PatrolPathEditor : Editor
{
    void OnSceneGUI()
    {
        var path = (PatrolPath)target;
        if (path.waypoints == null) return;

        Handles.color = Color.cyan;
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(path.waypoints[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Waypoint");
                path.waypoints[i] = newPos;
            }
            Handles.Label(path.waypoints[i] + Vector2.up * 0.3f, $"P{i}");
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var path = (PatrolPath)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Waypoint (at last + offset)"))
        {
            Undo.RecordObject(path, "Add Waypoint");
            Vector2 last = path.waypoints.Count > 0 ? path.waypoints[path.waypoints.Count - 1] : (Vector2)path.transform.position;
            path.waypoints.Add(last + Vector2.right * 2f);
        }
        if (GUILayout.Button("Remove Last Waypoint") && path.waypoints.Count > 0)
        {
            Undo.RecordObject(path, "Remove Waypoint");
            path.waypoints.RemoveAt(path.waypoints.Count - 1);
        }
    }
}
#endif
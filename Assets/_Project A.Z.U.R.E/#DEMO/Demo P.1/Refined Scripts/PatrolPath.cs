using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public List<Vector2> waypoints = new List<Vector2>() { Vector2.zero, Vector2.right * 3f };
    public bool loop = false;

    public int Count => waypoints.Count;
    public Vector2 Get(int i) => waypoints[Mathf.Clamp(i, 0, waypoints.Count - 1)];

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.DrawWireSphere(waypoints[i], 0.2f);
            if (i > 0) Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
        }
        if (loop && waypoints.Count > 1)
            Gizmos.DrawLine(waypoints[waypoints.Count - 1], waypoints[0]);
    }
}
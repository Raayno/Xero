using UnityEditor;
using UnityEngine;

public class EnemyAIWaypointSequence : MonoBehaviour
{
    public EnemyAIWaypoint[] Waypoints;

    private void OnDrawGizmos()
    {
        if (Waypoints == null || Waypoints.Length == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < Waypoints.Length; i++)
        {
            if (Waypoints[i] != null)
            {
                Gizmos.DrawLine(Waypoints[i].transform.position, Waypoints[(i + 1) % Waypoints.Length].transform.position);
                Handles.Label(Waypoints[i].transform.position + 0.2f * i * Vector3.up, $"{i + 1}");
            }
        }
    }
}

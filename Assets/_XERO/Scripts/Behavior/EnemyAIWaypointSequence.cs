using UnityEditor;
using UnityEngine;

public class EnemyAIWaypointSequence : MonoBehaviour
{
    public EnemyAIWaypoint[] Waypoints;

    private void OnDrawGizmosSelected()
    {
        if (Waypoints == null || Waypoints.Length == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < Waypoints.Length; i++)
        {
            EnemyAIWaypoint enemyAIWaypoint = Waypoints[i];
            if (enemyAIWaypoint != null)
            {
                enemyAIWaypoint.DrawGizmos();
                Gizmos.DrawLine(enemyAIWaypoint.transform.position, Waypoints[(i + 1) % Waypoints.Length].transform.position);
                Handles.Label(enemyAIWaypoint.transform.position + 0.2f * i * Vector3.up, $"Waypoint {i + 1} in sequence", EditorStyles.boldLabel);
            }
        }
    }
}

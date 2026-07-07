using UnityEngine;
using Unity.Behavior;

public class DebugEnemyBehaviorAgent : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private Color spottingFieldOfViewColor = Color.red;
    [SerializeField] private Color chasingFieldOfViewColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (!isActiveAndEnabled) return;

        agent.GetVariable<Transform>("SelfEyes", out var agentEyes);
        if (agentEyes == null || agentEyes.Value == null) return;

        agent.GetVariable<Vector2>("SpottingRadiusAndAngle", out var spottingRadiusAndAngle);
        if (spottingRadiusAndAngle != null && spottingRadiusAndAngle.Value != null)
        {
            DrawFieldOfView(spottingRadiusAndAngle.Value, spottingFieldOfViewColor);
        }
        
        agent.GetVariable<Vector2>("ChasingRadiusAndAngle", out var chasingRadiusAndAngle);
        if (chasingRadiusAndAngle != null && chasingRadiusAndAngle.Value != null)
        {
            DrawFieldOfView(chasingRadiusAndAngle.Value, chasingFieldOfViewColor);
        }

        void DrawFieldOfView(Vector2 radiusAndAngle, Color color)
        {
            float radius = radiusAndAngle.x;
            float angle = radiusAndAngle.y;

            Gizmos.color = color;

            Vector3 forward = agentEyes.Value.forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -angle/2, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, angle/2, 0) * forward;

            Gizmos.DrawLine(agentEyes.Value.position, agentEyes.Value.position + leftBoundary * radius);
            Gizmos.DrawLine(agentEyes.Value.position, agentEyes.Value.position + rightBoundary * radius);
            Gizmos.DrawLine(agentEyes.Value.position + leftBoundary * radius, agentEyes.Value.position + rightBoundary * radius);
        }
    }
}

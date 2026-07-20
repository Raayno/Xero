#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Behavior;

[RequireComponent(typeof(BehaviorGraphAgent))]
public class DebugEnemyBehaviorAgent : MonoBehaviour
{
    [Header("Blackboard Variables")]
    [SerializeField] private Transform agentEyes;
    [SerializeField] private Vector2 spottingRadiusAndAngle;
    [SerializeField] private Vector2 chasingRadiusAndAngle;
    [SerializeField] private float lookAroundAngle;
    [SerializeField] private Vector2 dashRadiusAndAngle;

    [Header("Settings")]
    [Range(1, 13)][SerializeField] private int fieldOfViewApproximationDensity = 6;
    [SerializeField] private Color spottingFieldOfViewColor = Color.red;
    [SerializeField] private Color chasingFieldOfViewColor = Color.yellow;
    [SerializeField] private Color lookAroundGizmoColor = Color.blue;
    [SerializeField] private float lookAroundGizmoLength = 1f;
    [SerializeField] private Color dashGizmoColor = Color.purple;
    [SerializeField] private bool enablePlayModeGizmos = false;

    void OnEnable()
    {
        agent = GetComponent<BehaviorGraphAgent>();
        enablePlayModeGizmos = true;
    }

    void OnDisable()
    {
        enablePlayModeGizmos = false;
    }

    private BehaviorGraphAgent agent;
    [Tooltip("Play Mode Only")]
    [Alchemy.Inspector.Button] private void SynchronizeWithAgent()
    {
        agent = GetComponent<BehaviorGraphAgent>();
        if (agent == null)
        {
            Debug.LogWarning("No BehaviorGraphAgent found on this GameObject.");
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("SynchronizeWithAgent should be called during play mode.");
            return;
        }

        agent.GetVariable<Transform>("TargetEyes", out var t1);
        agentEyes = t1?.Value;

        agent.GetVariable<Vector2>("SpottingRadiusAndAngle", out var t2);
        spottingRadiusAndAngle = t2?.Value ?? Vector2.zero;

        agent.GetVariable<Vector2>("ChasingRadiusAndAngle", out var t3);
        chasingRadiusAndAngle = t3?.Value ?? Vector2.zero;

        agent.GetVariable<Vector2>("DashRadiusAndAngle", out var t4);
        dashRadiusAndAngle = t4?.Value ?? Vector2.zero;
    }
    

    private void OnDrawGizmosSelected()
    {
        if (!isActiveAndEnabled) return;

        if (enablePlayModeGizmos) OnDrawGizmosPlayMode();

        if (agentEyes == null) return;

        DrawFieldOfView(spottingRadiusAndAngle, spottingFieldOfViewColor, fieldOfViewApproximationDensity);
        DrawFieldOfView(chasingRadiusAndAngle, chasingFieldOfViewColor, fieldOfViewApproximationDensity);
        DrawFieldOfView(dashRadiusAndAngle, dashGizmoColor, 1);

        DrawFieldOfView(new Vector2(lookAroundGizmoLength, lookAroundAngle), lookAroundGizmoColor, 0);

        void DrawFieldOfView(Vector2 radiusAndAngle, Color color, int approximationDensity)
        {
            float radius = radiusAndAngle.x;
            float angle = radiusAndAngle.y;

            Gizmos.color = color;

            Vector3 forward = agentEyes.forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -angle/2, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, angle/2, 0) * forward;

            Gizmos.DrawLine(agentEyes.position, agentEyes.position + leftBoundary * radius);
            Gizmos.DrawLine(agentEyes.position, agentEyes.position + rightBoundary * radius);

            Vector3 previousPoint = agentEyes.position + leftBoundary * radius;
            for (int i = 0; i < approximationDensity; i++)
            {
                float t = (float)(i + 1) / approximationDensity;
                float currentAngle = -angle / 2 + t * angle;
                Vector3 currentDirection = Quaternion.Euler(0, currentAngle, 0) * forward;
                Vector3 currentPoint = agentEyes.position + currentDirection * radius;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
    
    private Vector3 locationCashed;
    private Vector3 partialLocationCashed; 
    private void OnDrawGizmosPlayMode()
    {
        if (agent == null) return;

        agent.GetVariable<Transform>("TargetEyes", out var targetEyesVar);
        Transform targetEyes = targetEyesVar?.Value;
        agent.GetVariable<EnemyRoamingStates>("State", out var stateVar);

        if (stateVar != null && stateVar.Value == EnemyRoamingStates.Chasing)
        {
            if (targetEyes != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(agentEyes.position, targetEyes.position);
            }
            
            agent.GetVariable<Vector3>("LastKnownTargetPosition", out var lastKnownPosVar);
            Vector3 lastKnownPos = lastKnownPosVar?.Value ?? Vector3.zero;

            if (lastKnownPos != Vector3.zero)
            {
                // Draw the last known position of the target
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(lastKnownPos, 0.2f);
                Handles.Label(lastKnownPos + Vector3.up * 0.5f, "Last Known Position", EditorStyles.boldLabel);


                if (locationCashed != lastKnownPos)
                {
                    locationCashed = lastKnownPos;
                    // Set only on change
                    partialLocationCashed = agentEyes.position + (lastKnownPos - agentEyes.position) * FixedNavigateToLocationAction.partialLocationMultiplier;
                }
                // Draw the partial location based on the last known position if location (lastKnownPos) has changed
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(partialLocationCashed, 0.2f);
            }
        }
    }
}
#endif

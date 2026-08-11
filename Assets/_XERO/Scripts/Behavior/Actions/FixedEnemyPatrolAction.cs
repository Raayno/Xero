using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FixedEnemyPatrol", story: "[Agent] patrols along [Waypoints] at [Speed] stopping at each one for time ranging between [WaypointWaitTime] using [Animator], [NavMeshAgent]", category: "Action", id: "e1f3c5b2a4d34e7b9c8f6a5d7e2b4c1d")]
public partial class FixedEnemyPatrolAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<EnemyAIWaypointSequence> Waypoints;
    [SerializeReference] public BlackboardVariable<Vector2> WaypointWaitTime;
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMeshAgent;
    [SerializeReference] public BlackboardVariable<float> Speed = new(3.5f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);
    [Tooltip("Should patrol restart from the latest point?")]
    [SerializeReference] public BlackboardVariable<bool> PreserveLatestPatrolPoint = new(false);

    [CreateProperty] private Vector3 m_CurrentTarget;
    [CreateProperty] private float m_OriginalStoppingDistance = -1f;
    [CreateProperty] private float m_OriginalSpeed = -1f;
    [CreateProperty] private float m_WaypointWait;
    [CreateProperty] private float m_WaypointWaitTimer;
    private float m_CurrentSpeed;
    [CreateProperty] private int m_CurrentPatrolPoint = 0;
    [CreateProperty] private bool m_Waiting;
    [CreateProperty] private readonly string[] m_AnimatorParameters = new[] { "SpeedMagnitude" };
    private enum AnimatorParameter { SpeedMagnitude }

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        if (Waypoints.Value == null || Waypoints.Value.Waypoints.Length == 0)
        {
            LogFailure("No waypoints to patrol assigned.");
            return Status.Failure;
        }

        if (NavMeshAgent.Value == null || NavMeshAgent.Value.enabled == false)
        {
            LogFailure("No NavMeshAgent assigned or it is disabled.");
            return Status.Failure;
        }

        Initialize();

        SetWaiting(false);
        m_WaypointWaitTimer = 0.0f;

        SetNextWaypointDestination();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Waypoints.Value == null)
        {
            return Status.Failure;
        }

        if (m_Waiting)
        {
            if (m_WaypointWaitTimer > 0.0f)
            {
                m_WaypointWaitTimer -= Time.deltaTime;
            }
            else
            {
                m_WaypointWaitTimer = 0f;
                SetWaiting(false);
                SetNextWaypointDestination();
            }
        }
        else
        {

            float distance = GetDistanceToWaypoint();
            bool destinationReached = distance <= DistanceThreshold;

            // Check if we've reached the waypoint (ensuring NavMeshAgent.Value has completed path calculation if available)
            if (destinationReached && (NavMeshAgent.Value == null || !NavMeshAgent.Value.pathPending))
            {
                m_WaypointWait = UnityEngine.Random.Range(WaypointWaitTime.Value.x, WaypointWaitTime.Value.y);
                m_WaypointWaitTimer = m_WaypointWait;
                SetWaiting(true);
                m_CurrentSpeed = 0;

                return Status.Running;
            }
            else 
            {
                if (NavMeshAgent.Value == null) // transform-based movement
                {
                    Debug.LogError("Transform-based movement is not implemented in this version of EnemyPatrolAction. Please ensure a NavMeshAgent.Value is attached to the Agent.");
                    return Status.Failure;
                }
                else m_CurrentSpeed = NavMeshAgent.Value.velocity.magnitude;
            }
        }

        UpdateAnimatorSpeed();

        return Status.Running;
    }

    protected void SetWaiting(bool waiting)
    {
        m_Waiting = waiting;
    }

    protected override void OnEnd()
    {
        UpdateAnimatorSpeed(0f);

        if (NavMeshAgent.Value != null)
        {
            if (NavMeshAgent.Value.isOnNavMesh)
            {
                NavMeshAgent.Value.ResetPath();
            }
            NavMeshAgent.Value.speed = m_OriginalSpeed;
            NavMeshAgent.Value.stoppingDistance = m_OriginalStoppingDistance;
        }
    }

    protected override void OnDeserialize()
    {
        // If using a navigation mesh, we need to reset default value before Initialize.
        NavMeshAgent.Value = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (NavMeshAgent.Value != null)
        {
            if (m_OriginalSpeed >= 0f)
                NavMeshAgent.Value.speed = m_OriginalSpeed;
            if (m_OriginalStoppingDistance >= 0f)
                NavMeshAgent.Value.stoppingDistance = m_OriginalStoppingDistance;

            NavMeshAgent.Value.Warp(Agent.Value.transform.position);
        }

        int patrolPoint = m_CurrentPatrolPoint - 1;
        Initialize();
        // During deserialization, bypass PreserveLatestPatrolPoint.
        m_CurrentPatrolPoint = patrolPoint;
    }

    private void Initialize()
    {
        if (NavMeshAgent.Value != null)
        {
            if (NavMeshAgent.Value.isOnNavMesh)
            {
                NavMeshAgent.Value.ResetPath();
            }

            m_OriginalSpeed = NavMeshAgent.Value.speed;
            NavMeshAgent.Value.speed = Speed.Value;
            m_OriginalStoppingDistance = NavMeshAgent.Value.stoppingDistance;
            NavMeshAgent.Value.stoppingDistance = DistanceThreshold;
        }

        m_CurrentPatrolPoint = PreserveLatestPatrolPoint.Value ? m_CurrentPatrolPoint - 1 : -1;

        UpdateAnimatorSpeed(0f);
    }

    private float GetDistanceToWaypoint()
    {
        if (NavMeshAgent.Value != null)
        {
            return NavMeshAgent.Value.remainingDistance;
        }

        Vector3 targetPosition = m_CurrentTarget;
        Vector3 agentPosition = Agent.Value.transform.position;
        agentPosition.y = targetPosition.y; // Ignore y for distance check.
        return Vector3.Distance(agentPosition, targetPosition);
    }

    private void SetNextWaypointDestination()
    {
        m_CurrentPatrolPoint = (m_CurrentPatrolPoint + 1) % Waypoints.Value.Waypoints.Length;

        m_CurrentTarget = GetRandomPointInWaypointRadius(Waypoints.Value.Waypoints[m_CurrentPatrolPoint]);
        static Vector3 GetRandomPointInWaypointRadius(EnemyAIWaypoint waypoint)
        {
            if (waypoint.Radius <= 0f)
            {
                return waypoint.transform.position; // If radius is zero or negative, return the center of the waypoint.
            }

            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * waypoint.Radius;
            randomDirection += waypoint.transform.position;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, waypoint.Radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                return waypoint.transform.position; // Fallback to the center of the waypoint if no valid point is found.
            }
        }

        if (NavMeshAgent.Value != null)
        {
            NavMeshAgent.Value.SetDestination(m_CurrentTarget);
        }
    }

    private void UpdateAnimatorSpeed(float explicitSpeed = -1f)
    {
        if (Animator.Value == null) return;

        float speedToSet = explicitSpeed >= 0f ? explicitSpeed : m_CurrentSpeed;
        Animator.Value.SetFloat(m_AnimatorParameters[(int)AnimatorParameter.SpeedMagnitude], speedToSet);
    }
}

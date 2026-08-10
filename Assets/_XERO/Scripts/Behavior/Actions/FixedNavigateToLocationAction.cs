using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "FixedNavigateToLocation",
    story: "[Agent] partially navigates at [Speed] to [Location] using [Animator] , [NavMeshAgent]",
    category: "Action",
    id: "c67c5c55de9fe94897cf41976250cc83")]
public partial class FixedNavigateToLocationAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMeshAgent;
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new("SpeedMagnitude");

    // This will only be used in movement without a navigation agent.
    [SerializeReference] public BlackboardVariable<float> SlowDownDistance = new(1.0f);
    [Tooltip("(NavMeshAgent only) If true, the node returns Failure when the agent cannot reach the destination (e.g. unreachable position or outside navmesh bounds). If false, returns Success.")]
    [SerializeReference] public BlackboardVariable<bool> FailIfUnreachable = new(true);

    private Vector3 m_LastLocationPosition;
    [CreateProperty] private float m_OriginalStoppingDistance = -1f;
    [CreateProperty] private float m_OriginalSpeed = -1f;
    [CreateProperty] private readonly string[] m_AnimatorParameters = new[] { "IsWalk", "SpeedMagnitude" };
    private enum AnimatorParameter { IsWalk, SpeedMagnitude }
    private readonly float m_CurrentSpeed;
    private float m_StallTimer = 0f;
    [Tooltip("The multiplier for the distance to the target location. A value of 1 means the agent will navigate to the exact location, while a value of 0.5 means the agent will navigate to a point halfway between its current position and the target location.")]
    public static readonly float partialLocationMultiplier = 0.5f;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Location.Value == null)
        {
            LogFailure("Agent or Location is not assigned.");
            return ReturnFailIfUnreachable();
        }

        if (NavMeshAgent.Value == null || NavMeshAgent.Value.enabled == false)
        {
            LogFailure("No NavMeshAgent assigned or it is disabled.");
            return Status.Failure;
        }


        return Initialize();
    }

    protected override Status OnUpdate()
    {
        if (Agent == null || Agent.Value == null || Location == null || NavMeshAgent == null || NavMeshAgent.Value == null)
        {
            Debug.LogError("Agent, Location, or NavMeshAgent is not assigned.");
            return ReturnFailIfUnreachable();
        }

        float distance = GetDistanceToLocation(out Vector3 locationPosition);

        // Check if the location has changed.
        bool locationChanged = m_LastLocationPosition != locationPosition;

        if (locationChanged)
        {
            m_LastLocationPosition = locationPosition;
            NavMeshAgent.Value.SetDestination(locationPosition);
            m_StallTimer = 0f;
        }

        float threshold = DistanceThreshold != null ? DistanceThreshold.Value : 0.2f;
        bool destinationReached = distance <= threshold;

        if (destinationReached && !NavMeshAgent.Value.pathPending)
        {
            return Status.Success;
        }
        
        if (!NavMeshAgent.Value.pathPending)
        {
            if (NavMeshAgent.Value.pathStatus == NavMeshPathStatus.PathPartial || NavMeshAgent.Value.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                return ReturnFailIfUnreachable();
            }

            // Fix Stall check using a Time buffer so it never fails on single-frame recalculations
            if (!NavMeshAgent.Value.hasPath || NavMeshAgent.Value.velocity.sqrMagnitude < 0.01f)
            {
                if (NavMeshAgent.Value.remainingDistance <= NavMeshAgent.Value.stoppingDistance + 0.1f)
                {
                    return Status.Success; 
                }

                m_StallTimer += Time.deltaTime;
                if (m_StallTimer > 0.5f) // Must be completely stuck for half a second before failing
                {
                    return ReturnFailIfUnreachable();
                }
            }
            else
            {
                m_StallTimer = 0f; // Reset if moving safely
            }
        }

        UpdateAnimatorSpeed();

        return Status.Running;
    }

    private Status ReturnFailIfUnreachable()
    {
        Debug.LogWarning("Agent cannot reach the destination.");
        return Status.Failure;
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

        cashedAgentPosition = null;
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

        Initialize();
    }

    private Status Initialize()
    {
        float distance = GetDistanceToLocation(out Vector3 locationPosition);
        m_LastLocationPosition = locationPosition;

        if (distance <= DistanceThreshold)
        {
            return Status.Success;
        }

        // If using a navigation mesh, set target position for navigation mesh agent.
        if (NavMeshAgent.Value != null)
        {
            if (NavMeshAgent.Value.isOnNavMesh)
            {
                NavMeshAgent.Value.ResetPath();
            }

            m_OriginalSpeed = NavMeshAgent.Value.speed;
            NavMeshAgent.Value.speed = Speed;
            m_OriginalStoppingDistance = NavMeshAgent.Value.stoppingDistance;
            NavMeshAgent.Value.stoppingDistance = DistanceThreshold;
            NavMeshAgent.Value.SetDestination(locationPosition);
        }

        UpdateAnimatorSpeed(0f);

        return Status.Running;
    }

    private Vector3? cashedAgentPosition = null;
    private float GetDistanceToLocation(out Vector3 locationPosition)
    {
        cashedAgentPosition ??= Agent.Value.transform.position;
        Vector3 directionToLocation = Location.Value - cashedAgentPosition.Value;
        locationPosition = cashedAgentPosition.Value + directionToLocation * partialLocationMultiplier;
        return Vector3.Distance(new Vector3(cashedAgentPosition.Value.x, locationPosition.y, cashedAgentPosition.Value.z), locationPosition);
    }

    private void UpdateAnimatorSpeed(float explicitSpeed = -1f)
    {
        if (Animator.Value == null) return;

        float speedToSet = explicitSpeed >= 0f ? explicitSpeed : m_CurrentSpeed;
        Animator.Value.SetFloat(m_AnimatorParameters[(int)AnimatorParameter.SpeedMagnitude], speedToSet);
    }
}

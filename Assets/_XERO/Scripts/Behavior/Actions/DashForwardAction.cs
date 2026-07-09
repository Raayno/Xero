using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Unity.Properties;
using DG.Tweening;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DashForward", story: "[Agent] moves as far as possible forward on [NavMeshAgent] , not further than [Distance] , in the short time of [Duration] using [DashEaseCurveWrapper]", category: "Action", id: "318b390040091603d342d8e63ef829c7")]
public partial class DashForwardAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMeshAgent;
    [Tooltip("The distance to dash forward, in world units. If 0, the dash is skipped and the action returns success immediately.")]
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<float> Duration;
    [SerializeReference] public BlackboardVariable<AnimationCurveWrapper> DashEaseCurveWrapper;

    private Transform transform;
    private Vector3 targetPosition;
    private bool isDashing;
    private Tween dashTween;

    protected override Status OnStart()
    {
        if (Distance.Value == 0f) return Status.Success; // Means dash is meant to be skipped, so we can return success immediately
        if (Agent == null || NavMeshAgent == null || Distance == null || Duration == null) return Status.Failure;
        if (Distance.Value < 0f || Duration.Value <= 0f) return Status.Failure;

        transform = Agent.Value.transform;

        Vector3 dashDirection = transform.forward;
        Vector3 intendedDestination = transform.position + (dashDirection * Distance.Value);

        // Check for obstacles
        if (NavMesh.Raycast(transform.position, intendedDestination, out NavMeshHit hit, NavMesh.AllAreas))
        {
            // Truncate destination and pull back slightly by agent radius to avoid clipping
            targetPosition = hit.position - (dashDirection * NavMeshAgent.Value.radius);
        }
        else
        {
            targetPosition = intendedDestination;
        }

        // Check for holes at the target position and adjust to nearest valid position on the NavMesh
        int samples = 5; // current--5--4--3--2--1(targetPosition)
        Vector3 backwardsStep = -dashDirection * Distance.Value / samples;
        while (samples > 0 && !NavMesh.SamplePosition(targetPosition, out _, NavMeshAgent.Value.radius, NavMesh.AllAreas))
        {
            targetPosition += backwardsStep;
            samples--;
        }

        if (NavMeshAgent.Value != null)
        {
            // Disable the NavMeshAgent during the dash to prevent it from interfering with the movement
            NavMeshAgent.Value.isStopped = true;
            NavMeshAgent.Value.updatePosition = false;
            NavMeshAgent.Value.updateRotation = false;
        }

        isDashing = true;

        AnimationCurve easeCurve = (DashEaseCurveWrapper != null && DashEaseCurveWrapper.Value != null) 
            ? DashEaseCurveWrapper.Value.Curve 
            : AnimationCurve.Linear(0f, 0f, 1f, 1f);

        dashTween = transform.DOMove(targetPosition, Duration.Value)
            .SetEase(easeCurve)
            .OnComplete(() => isDashing = false);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!isDashing) return Status.Success;
        return Status.Running;
    }

    protected override void OnEnd()
    {
        isDashing = false;

        if (dashTween != null && dashTween.IsActive())
        {
            dashTween.Kill();
        }

        if (NavMeshAgent != null && NavMeshAgent.Value != null)
        {
            NavMeshAgent.Value.Warp(transform.position);
            
            NavMeshAgent.Value.isStopped = false;
            NavMeshAgent.Value.updatePosition = true;
            NavMeshAgent.Value.updateRotation = true;
        }
    }
}


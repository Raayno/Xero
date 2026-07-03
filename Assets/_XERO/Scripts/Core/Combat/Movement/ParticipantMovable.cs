using UnityEngine;
using DG.Tweening;
using UnityEngine.Timeline;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;


public class ParticipantMovable : MonoBehaviour
{
    private static readonly Quaternion positiveInfinityQuaternion = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    [Header("Movement animation")]
    [SerializeField] private TimelineAsset moveToTargetTimelineAsset;
    [SerializeField] private AnimationCurve  moveToTargetTraversalSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private TimelineAsset returnTimelineAsset;
    [SerializeField] private AnimationCurve  returnTraversalSpeedCurve;
    
    [Header("References")]
    [SerializeField] private Participant participant;
    [SerializeField] private HitTransform hitTransform;
    [SerializeField] private AttackTransform attackTransform;
    private Vector3 originalPosition = Vector3.positiveInfinity;
    private Quaternion originalRotation = positiveInfinityQuaternion;

    public bool HasOriginalPosition => originalPosition != Vector3.positiveInfinity && originalRotation != positiveInfinityQuaternion;

    public async UniTask MoveToTargetAsync(List<Participant> targets, CancellationToken cancellationToken = default)
    {
        if (moveToTargetTimelineAsset == null)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> MoveToTargetTimelineAsset is not assigned.");
            return;
        }
        if (targets == null || targets.Count == 0 || targets.Count > 1 || targets[0] == null)
        {
            return;
        }

        // For simplicity, we will just use the first target's HitTransform for movement
        HitTransform targetHitTransform = targets[0].GetComponentInChildren<HitTransform>();
        if (targetHitTransform == null)
        {
            Debug.LogError($"<color=purple>[ParticipantMovable]</color> Target '{targets[0].name}' does not have a HitTransform.");
            return;
        }

        await AttackMovementAsync(targetHitTransform, cancellationToken);
    }

    public async UniTask ReturnToOriginalPositionAsync(CancellationToken cancellationToken = default)
    {
        if (moveToTargetTimelineAsset == null)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> MoveToTargetTimelineAsset is not assigned.");
            return;
        }
        if (originalPosition == Vector3.positiveInfinity)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> Original position is not set. Cannot return to original position.");
            return;
        }
        if (originalRotation == positiveInfinityQuaternion)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> Original rotation is not set. Cannot return to original rotation.");
            return;
        }
        await ReturnMovementAsync(cancellationToken);
    }

    private async UniTask AttackMovementAsync(HitTransform targetHitTransform, CancellationToken cancellationToken)
    {
        // Calculate the route vector and target position based on the attack and hit transforms
        Vector3 routeVector = targetHitTransform.Position - attackTransform.Position;
        Vector3 targetPosition = transform.position + routeVector;

        // Store the original position and rotation of the participant
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Calculate the rotation needed to face the target position
        Quaternion routeRotation = Quaternion.LookRotation(routeVector, Vector3.up);
        await Rotate(transform, routeRotation);

        await Move(targetPosition, moveToTargetTimelineAsset, moveToTargetTraversalSpeedCurve, cancellationToken);

        await Rotate(transform, originalRotation);
    }

    private async UniTask ReturnMovementAsync(CancellationToken cancellationToken)
    {
        if (returnTimelineAsset == null) returnTimelineAsset = moveToTargetTimelineAsset;
        if (returnTraversalSpeedCurve.keys.Length < 2) returnTraversalSpeedCurve = moveToTargetTraversalSpeedCurve;

        Quaternion routeRotation = Quaternion.LookRotation(originalPosition - transform.position, Vector3.up);
        await Rotate(transform, routeRotation);
        
        await Move(originalPosition, returnTimelineAsset, returnTraversalSpeedCurve, cancellationToken);

        await Rotate(transform, originalRotation);
    }

    private async UniTask Move(Vector3 targetPosition, TimelineAsset timelineAsset, AnimationCurve traversalSpeedCurve, CancellationToken cancellationToken)
    {
        TimelineManager.PlayTimeline(timelineAsset, participant.Animator);
        Tween tween = transform.DOMove(targetPosition, (float)timelineAsset.duration).SetEase(traversalSpeedCurve);
        using var cancellationRegistration = cancellationToken.Register(() => tween.Kill());
        await UniTask.WaitUntil(() => !tween.active || tween.IsComplete(), cancellationToken: cancellationToken);
    }

    private UniTask Rotate(Transform targetTransform, Quaternion rotation)
    {
        targetTransform.rotation = rotation;
        return UniTask.CompletedTask;
    }

    private void Reset()
    {
        if (participant == null)
        {
            participant = transform.parent.GetComponent<Participant>();
        }
        if (hitTransform == null)
        {
            hitTransform = participant.GetComponentInChildren<HitTransform>();
        }
        if (attackTransform == null)
        {
            attackTransform = GetComponentInChildren<AttackTransform>();
        }
    }
}
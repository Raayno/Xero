using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Timeline;


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

    public IEnumerator MoveToTargetAsync(System.Collections.Generic.List<Participant> targets)
    {
        if (moveToTargetTimelineAsset == null)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> MoveToTargetTimelineAsset is not assigned.");
            yield break;
        }
        if (targets == null || targets.Count == 0 || targets.Count > 1 || targets[0] == null)
        {
            yield break;
        }

        // For simplicity, we will just use the first target's HitTransform for movement
        HitTransform targetHitTransform = targets[0].GetComponentInChildren<HitTransform>();
        if (targetHitTransform == null)
        {
            Debug.LogError($"<color=purple>[ParticipantMovable]</color> Target '{targets[0].name}' does not have a HitTransform.");
            yield break;
        }

        yield return AttackMovementAsync(targetHitTransform);
    }

    public IEnumerator ReturnToOriginalPositionAsync()
    {
        if (moveToTargetTimelineAsset == null)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> MoveToTargetTimelineAsset is not assigned.");
            yield break;
        }
        if (originalPosition == null || originalPosition == Vector3.positiveInfinity)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> Original position is not set. Cannot return to original position.");
            yield break;
        }
        if (originalRotation == null || originalRotation == positiveInfinityQuaternion)
        {
            Debug.LogError("<color=purple>[ParticipantMovable]</color> Original rotation is not set. Cannot return to original rotation.");
            yield break;
        }
        yield return ReturnMovementAsync();
    }

    private IEnumerator AttackMovementAsync(HitTransform targetHitTransform)
    {
        // Calculate the route vector and target position based on the attack and hit transforms
        Vector3 routeVector = targetHitTransform.Position - attackTransform.Position;
        Vector3 targetPosition = transform.position + routeVector;

        // Store the original position and rotation of the participant
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Calculate the rotation needed to face the target position
        Quaternion routeRotation = Quaternion.LookRotation(routeVector, Vector3.up);
        yield return Rotate(transform, routeRotation);

        yield return Move(targetPosition, moveToTargetTimelineAsset, moveToTargetTraversalSpeedCurve);

        yield return Rotate(transform, originalRotation);
    }

    private IEnumerator ReturnMovementAsync()
    {
        if (returnTimelineAsset == null) returnTimelineAsset = moveToTargetTimelineAsset;
        if (returnTraversalSpeedCurve.keys.Length < 2) returnTraversalSpeedCurve = moveToTargetTraversalSpeedCurve;

        Quaternion routeRotation = Quaternion.LookRotation(originalPosition - transform.position, Vector3.up);
        yield return Rotate(transform, routeRotation);
        
        yield return Move(originalPosition, returnTimelineAsset, returnTraversalSpeedCurve);

        yield return Rotate(transform, originalRotation);
    }

    private IEnumerator Move(Vector3 targetPosition, TimelineAsset timelineAsset, AnimationCurve traversalSpeedCurve)
    {
        participant.playableDirector.playableAsset = timelineAsset;
        participant.playableDirector.Play();
        yield return transform.DOMove(targetPosition, (float)timelineAsset.duration).SetEase(traversalSpeedCurve).WaitForCompletion();
        participant.playableDirector.Stop();
    }

    private IEnumerator Rotate(Transform targetTransform, Quaternion rotation)
    {
        targetTransform.rotation = rotation;
        yield break;
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
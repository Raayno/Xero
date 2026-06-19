using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class CombatTargetMoveBehaviour : PlayableBehaviour
{
    [Header("Testing / Timeline Preview")]
    [SerializeField] private bool useTestTransformTarget;

    [AllowNesting]
    [ShowIf(nameof(useTestTransformTarget))]
    [SerializeField] private ExposedReference<Transform> testTransformTarget;

    [Header("Destination")]
    [AllowNesting]
    [HideIf(nameof(useTestTransformTarget))]
    [SerializeField]
    private CombatTargetMoveDestinationSource destinationSource =
        CombatTargetMoveDestinationSource.CurrentActionFirstReceiver;

    [AllowNesting]
    [ShowIf(nameof(UsesCombatTargetDestination))]
    [SerializeField] private CombatMovePointType destinationMovePoint = CombatMovePointType.Root;

    [AllowNesting]
    [ShowIf(nameof(UsesExposedCombatTarget))]
    [SerializeField] private ExposedReference<CombatTarget> exposedCombatTarget;

    [AllowNesting]
    [ShowIf(nameof(UsesWorldPosition))]
    [SerializeField] private Vector3 worldPosition;

    [AllowNesting]
    [ShowIf(nameof(UsesActorLocalOffset))]
    [SerializeField] private Vector3 actorLocalOffset;

    [Header("Stopping / Offset")]
    [Tooltip("Stops this many units before the final destination, based on the direction from start position to destination.")]
    [AllowNesting]
    [ShowIf(nameof(UsesArrivalDistance))]
    [SerializeField] private float arrivalDistance;

    [AllowNesting]
    [ShowIf(nameof(UsesOffset))]
    [SerializeField]
    private CombatTargetMoveOffsetSpace offsetSpace =
        CombatTargetMoveOffsetSpace.DestinationLocal;

    [AllowNesting]
    [ShowIf(nameof(UsesOffset))]
    [SerializeField] private Vector3 offset;

    [Header("Movement Curve")]
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Rotation")]
    [SerializeField] private bool faceMoveDirection = true;

    [AllowNesting]
    [ShowIf(nameof(IsReturningToActionStart))]
    [SerializeField] private bool restoreActionStartRotation = true;

    [SerializeField] private bool keepOriginalY = true;

    private bool hasCapturedStart;
    private bool warnedMissingDestination;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public void Apply(
        CombatTarget actor,
        IExposedPropertyTable resolver,
        float normalizedTime)
    {
        if (actor == null)
        {
            return;
        }

        if (!hasCapturedStart)
        {
            CaptureStart(actor);
        }

        bool hasDestination = TryGetDestination(
            actor,
            resolver,
            out Vector3 destination);

        if (!hasDestination)
        {
            return;
        }

        if (keepOriginalY)
        {
            destination.y = startPosition.y;
        }

        destination = ApplyArrivalDistance(destination);

        float curveTime = EvaluateCurve(Mathf.Clamp01(normalizedTime));

        actor.transform.position = Vector3.LerpUnclamped(
            startPosition,
            destination,
            curveTime);

        ApplyRotation(actor, destination, curveTime);
    }

    public void ResetRuntimeState()
    {
        hasCapturedStart = false;
        warnedMissingDestination = false;
        startPosition = Vector3.zero;
        startRotation = Quaternion.identity;
    }

    private void CaptureStart(CombatTarget actor)
    {
        hasCapturedStart = true;
        warnedMissingDestination = false;
        startPosition = actor.transform.position;
        startRotation = actor.transform.rotation;
    }

    private bool TryGetDestination(
        CombatTarget actor,
        IExposedPropertyTable resolver,
        out Vector3 destination)
    {
        if (useTestTransformTarget)
        {
            return TryGetTestTransformDestination(
                actor,
                resolver,
                out destination);
        }

        return TryGetRuntimeDestination(
            actor,
            resolver,
            out destination);
    }

    private bool TryGetTestTransformDestination(
        CombatTarget actor,
        IExposedPropertyTable resolver,
        out Vector3 destination)
    {
        destination = startPosition;

        Transform target = testTransformTarget.Resolve(resolver);

        if (target == null)
        {
            LogMissingDestinationOnce(
                "[CombatTargetMoveBehaviour] Test mode is enabled, but Test Transform Target is not assigned. Movement skipped.");

            return false;
        }

        destination = target.position;
        destination += GetOffset(actor, null, target);

        return true;
    }

    private bool TryGetRuntimeDestination(
        CombatTarget actor,
        IExposedPropertyTable resolver,
        out Vector3 destination)
    {
        destination = startPosition;

        switch (destinationSource)
        {
            case CombatTargetMoveDestinationSource.CurrentActionFirstReceiver:
                {
                    CombatTarget receiver = GetFirstReceiver(actor);

                    if (receiver == null)
                    {
                        LogMissingDestinationOnce(
                            "[CombatTargetMoveBehaviour] Current action receiver was not found. Movement skipped.");

                        return false;
                    }

                    destination = GetCombatTargetDestination(actor, receiver);
                    return true;
                }

            case CombatTargetMoveDestinationSource.CurrentActionAttacker:
                {
                    CombatTarget attacker = GetAttacker(actor);

                    if (attacker == null)
                    {
                        LogMissingDestinationOnce(
                            "[CombatTargetMoveBehaviour] Current action attacker was not found. Movement skipped.");

                        return false;
                    }

                    destination = GetCombatTargetDestination(actor, attacker);
                    return true;
                }

            case CombatTargetMoveDestinationSource.CurrentActionStartPosition:
                {
                    if (!actor.HasActionStartTransform)
                    {
                        LogMissingDestinationOnce(
                            "[CombatTargetMoveBehaviour] Action start position was not captured. Movement skipped.");

                        return false;
                    }

                    destination = actor.ActionStartPosition;
                    return true;
                }

            case CombatTargetMoveDestinationSource.ExposedCombatTarget:
                {
                    CombatTarget exposedTarget = exposedCombatTarget.Resolve(resolver);

                    if (exposedTarget == null)
                    {
                        LogMissingDestinationOnce(
                            "[CombatTargetMoveBehaviour] Exposed CombatTarget is not assigned. Movement skipped.");

                        return false;
                    }

                    destination = GetCombatTargetDestination(actor, exposedTarget);
                    return true;
                }

            case CombatTargetMoveDestinationSource.WorldPosition:
                {
                    destination = worldPosition;
                    destination += GetOffset(actor, null, null);
                    return true;
                }

            case CombatTargetMoveDestinationSource.ActorLocalOffset:
                {
                    destination = actor.transform.TransformPoint(actorLocalOffset);
                    return true;
                }

            default:
                {
                    LogMissingDestinationOnce(
                        $"[CombatTargetMoveBehaviour] Unsupported destination source: {destinationSource}. Movement skipped.");

                    return false;
                }
        }
    }

    private Vector3 GetCombatTargetDestination(
        CombatTarget actor,
        CombatTarget destinationTarget)
    {
        Transform destinationTransform =
            destinationTarget.GetMovePointTransform(destinationMovePoint);

        Vector3 destination = destinationTransform != null
            ? destinationTransform.position
            : destinationTarget.transform.position;

        destination += GetOffset(actor, destinationTarget, destinationTransform);

        return destination;
    }

    private Vector3 ApplyArrivalDistance(Vector3 destination)
    {
        if (!UsesArrivalDistance())
        {
            return destination;
        }

        if (arrivalDistance <= 0f)
        {
            return destination;
        }

        Vector3 startToDestination = destination - startPosition;

        if (keepOriginalY)
        {
            startToDestination.y = 0f;
        }

        float distance = startToDestination.magnitude;

        if (distance <= 0.0001f)
        {
            return destination;
        }

        float safeArrivalDistance = Mathf.Min(arrivalDistance, distance);

        Vector3 direction = startToDestination.normalized;

        return destination - direction * safeArrivalDistance;
    }

    private Vector3 GetOffset(
        CombatTarget actor,
        CombatTarget destinationTarget,
        Transform destinationTransform)
    {
        switch (offsetSpace)
        {
            case CombatTargetMoveOffsetSpace.World:
                return offset;

            case CombatTargetMoveOffsetSpace.DestinationLocal:
                {
                    if (destinationTransform == null)
                    {
                        if (destinationTarget != null)
                        {
                            destinationTransform = destinationTarget.transform;
                        }
                    }

                    if (destinationTransform == null)
                    {
                        return offset;
                    }

                    return destinationTransform.TransformDirection(offset);
                }

            case CombatTargetMoveOffsetSpace.ActorLocal:
                {
                    if (actor == null)
                    {
                        return offset;
                    }

                    return actor.transform.TransformDirection(offset);
                }

            default:
                return offset;
        }
    }

    private CombatTarget GetFirstReceiver(CombatTarget actor)
    {
        if (actor == null)
        {
            return null;
        }

        CombatActionContext context = actor.CurrentActionContext;

        if (context == null)
        {
            return null;
        }

        return context.GetFirstReceiver();
    }

    private CombatTarget GetAttacker(CombatTarget actor)
    {
        if (actor == null)
        {
            return null;
        }

        CombatActionContext context = actor.CurrentActionContext;

        if (context == null)
        {
            return null;
        }

        return context.Attacker;
    }

    private void ApplyRotation(
        CombatTarget actor,
        Vector3 destination,
        float curveTime)
    {
        if (IsReturningToActionStart() && restoreActionStartRotation)
        {
            actor.transform.rotation = Quaternion.Slerp(
                startRotation,
                actor.ActionStartRotation,
                curveTime);

            return;
        }

        if (!faceMoveDirection)
        {
            return;
        }

        Vector3 direction = destination - startPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        actor.transform.rotation = Quaternion.Slerp(
            startRotation,
            targetRotation,
            curveTime);
    }

    private float EvaluateCurve(float normalizedTime)
    {
        if (movementCurve == null || movementCurve.length == 0)
        {
            return normalizedTime;
        }

        return movementCurve.Evaluate(normalizedTime);
    }

    private void LogMissingDestinationOnce(string message)
    {
        if (warnedMissingDestination)
        {
            return;
        }

        warnedMissingDestination = true;

        Debug.LogWarning(message);
    }

    private bool UsesCombatTargetDestination()
    {
        if (useTestTransformTarget)
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.CurrentActionFirstReceiver ||
               destinationSource == CombatTargetMoveDestinationSource.CurrentActionAttacker ||
               destinationSource == CombatTargetMoveDestinationSource.ExposedCombatTarget;
    }

    private bool UsesExposedCombatTarget()
    {
        if (useTestTransformTarget)
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.ExposedCombatTarget;
    }

    private bool UsesWorldPosition()
    {
        if (useTestTransformTarget)
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.WorldPosition;
    }

    private bool UsesActorLocalOffset()
    {
        if (useTestTransformTarget)
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.ActorLocalOffset;
    }

    private bool UsesOffset()
    {
        if (IsReturningToActionStart())
        {
            return false;
        }

        if (destinationSource == CombatTargetMoveDestinationSource.ActorLocalOffset && !useTestTransformTarget)
        {
            return false;
        }

        return true;
    }

    private bool UsesArrivalDistance()
    {
        if (IsReturningToActionStart())
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.CurrentActionFirstReceiver ||
               destinationSource == CombatTargetMoveDestinationSource.CurrentActionAttacker ||
               destinationSource == CombatTargetMoveDestinationSource.ExposedCombatTarget ||
               useTestTransformTarget;
    }

    private bool IsReturningToActionStart()
    {
        if (useTestTransformTarget)
        {
            return false;
        }

        return destinationSource == CombatTargetMoveDestinationSource.CurrentActionStartPosition;
    }
}
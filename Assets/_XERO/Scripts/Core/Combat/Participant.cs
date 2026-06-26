using System;
using UnityEngine;
using UnityEngine.Playables;

public abstract class Participant : MonoBehaviour
{
    [Header("Combat Participant")]
    [SerializeField] private string combatantName;
    public bool IsPlayerTeam = false;

    [Header("Attack Sequence")]
    [SerializeField] private PlayableDirector attackSequenceDirector;

    [Header("Timeline Move Points")]
    [SerializeField] private Transform timelineMovePoint;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private Transform customPointA;
    [SerializeField] private Transform customPointB;

    private Vector3 actionStartPosition;
    private Quaternion actionStartRotation;
    private bool hasActionStartTransform;

    public string CombatantName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(combatantName))
            {
                return combatantName;
            }

            return gameObject.name;
        }
    }

    public bool IsDefeated { get; private set; }

    public PlayableDirector AttackSequenceDirector => attackSequenceDirector;

    public CombatActionContext CurrentActionContext { get; private set; }

    public Vector3 ActionStartPosition => actionStartPosition;
    public Quaternion ActionStartRotation => actionStartRotation;
    public bool HasActionStartTransform => hasActionStartTransform;

    public event Action<Participant> Defeated;
    public event Action<Participant> AttackSequenceFinished;

    public virtual void SetCurrentActionContext(CombatActionContext actionContext)
    {
        CurrentActionContext = actionContext;
        CaptureActionStartTransform();
    }

    public virtual void ClearCurrentActionContext()
    {
        CurrentActionContext = null;
        hasActionStartTransform = false;
    }

    public virtual void CaptureActionStartTransform()
    {
        actionStartPosition = transform.position;
        actionStartRotation = transform.rotation;
        hasActionStartTransform = true;
    }

    public virtual Transform GetMovePointTransform(CombatMovePointType movePointType)
    {
        switch (movePointType)
        {
            case CombatMovePointType.Root:
                return transform;

            case CombatMovePointType.TimelineMovePoint:
                return timelineMovePoint != null ? timelineMovePoint : transform;

            case CombatMovePointType.AttackPoint:
                return attackPoint != null ? attackPoint : transform;

            case CombatMovePointType.HitPoint:
                return hitPoint != null ? hitPoint : transform;

            case CombatMovePointType.CustomPointA:
                return customPointA != null ? customPointA : transform;

            case CombatMovePointType.CustomPointB:
                return customPointB != null ? customPointB : transform;

            default:
                return transform;
        }
    }

    public virtual Vector3 GetMovePointPosition(CombatMovePointType movePointType)
    {
        Transform movePoint = GetMovePointTransform(movePointType);

        if (movePoint == null)
        {
            return transform.position;
        }

        return movePoint.position;
    }

    public virtual void PlayAttackSequence(AttackDataSO attackDataSO)
    {
        if (IsDefeated)
        {
            Debug.LogWarning(
                $"[CombatTarget] {CombatantName} cannot attack because it is defeated.");

            return;
        }

        if (attackDataSO == null)
        {
            Debug.LogError(
                $"[CombatTarget] {CombatantName} received null attack data.");

            return;
        }

        if (attackDataSO.TimelineAsset == null)
        {
            Debug.LogError(
                $"[CombatTarget] Attack '{attackDataSO.name}' has no timeline assigned.");

            return;
        }

        if (attackSequenceDirector == null)
        {
            Debug.LogError(
                $"[CombatTarget] No attack PlayableDirector is assigned to {CombatantName}.");

            return;
        }

        attackSequenceDirector.playableAsset = attackDataSO.TimelineAsset;
        attackSequenceDirector.Stop();
        attackSequenceDirector.time = 0d;
        attackSequenceDirector.Evaluate();
        attackSequenceDirector.Play();

        Debug.Log(
            $"<color=#55AAFF>[Combat]</color> " +
            $"{CombatantName} started its attack sequence.");
    }

    public virtual void AttackSequenceEnd()
    {
        Debug.Log(
            $"<color=#55FF88>[Combat]</color> " +
            $"{CombatantName} finished its attack sequence.");

        AttackSequenceFinished?.Invoke(this);
    }

    public virtual void MarkAsDefeated()
    {
        if (IsDefeated)
        {
            return;
        }

        IsDefeated = true;

        Defeated?.Invoke(this);
    }

    public virtual void ResetCombatTarget()
    {
        IsDefeated = false;
    }

    public virtual void StopAttackSequence()
    {
        if (attackSequenceDirector == null)
        {
            return;
        }

        attackSequenceDirector.Stop();
        attackSequenceDirector.time = 0d;
        attackSequenceDirector.Evaluate();
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(combatantName))
        {
            combatantName = gameObject.name;
        }
    }
#endif
}
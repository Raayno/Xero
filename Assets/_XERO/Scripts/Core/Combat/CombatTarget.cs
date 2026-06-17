using System;
using UnityEngine;
using UnityEngine.Playables;

public abstract class CombatTarget : MonoBehaviour
{
    [Header("Combat Target")]
    [SerializeField] private string combatantName;

    [Header("Attack Sequence")]
    [SerializeField] private PlayableDirector attackSequenceDirector;

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

    public event Action<CombatTarget> Defeated;
    public event Action<CombatTarget> AttackSequenceFinished;

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

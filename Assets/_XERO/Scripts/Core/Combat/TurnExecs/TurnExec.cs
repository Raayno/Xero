using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class TurnExec: MonoBehaviour
{
    [SerializeField] private AttackSelector attackSelector;

    public IEnumerator ExecuteTurn(Participant executor)
    {
        if (attackSelector == null)
        {
            Debug.LogError("[TurnExec] AttackSelector is not assigned.");
            yield break;
        }

        var attack = attackSelector.SelectAttack();
        if (attack == null)
        {
            Debug.LogError("[TurnExec] AttackSelector returned a null attack.");
            yield break;
        }

        if (attack.TargetSelector == null)
        {
            Debug.LogError($"[TurnExec] Attack '{attack.name}' has no target selector assigned.");
            yield break;
        }

        List<Participant> targets = null;
        yield return attack.TargetSelector.SelectTargetsAsync(executor, selectedTargets => targets = selectedTargets);

        if (targets == null || targets.Count == 0)
        {
            yield break;
        }

        yield return ExecuteTurn(executor, attack, targets);
    }

    protected abstract IEnumerator ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets);
}
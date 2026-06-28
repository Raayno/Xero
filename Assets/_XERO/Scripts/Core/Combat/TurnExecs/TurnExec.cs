using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class TurnExec: MonoBehaviour
{
    [SerializeField] private AttackSelector attackSelector;
    [SerializeField] private List<AttackDataSO> availableAttacks;

    public IEnumerator ExecuteTurn(Participant executor)
    {
        if (attackSelector == null)
        {
            Debug.LogError("[TurnExec] AttackSelector is not assigned.");
            yield break;
        }

        // Select an attack
        AttackDataSO attack = null;
        yield return attackSelector.SelectAttackAsync(availableAttacks, selectedAttack => attack = selectedAttack);

        if (attack == null)
        {
            Debug.LogError("[TurnExec] AttackSelector returned a null attack.");
            yield break;
        }
        Debug.Log($"[TurnExec] {executor.name} selected attack: {attack.name}");

        if (attack.TargetSelector == null)
        {
            Debug.LogError($"[TurnExec] Attack '{attack.name}' has no target selector assigned.");
            yield break;
        }

        // Select targets
        List<Participant> targets = null;
        yield return attack.TargetSelector.SelectTargetsAsync(executor, selectedTargets => targets = selectedTargets);

        if (targets == null || targets.Count == 0)
        {
            yield break;
        }

        Debug.Log($"[TurnExec] {executor.name} selected {targets.Count} target(s) for attack: {attack.name}. That is: {string.Join(", ", targets.ConvertAll(t => t.name))}");

        yield return ExecuteTurn(executor, attack, targets);
    }

    protected abstract IEnumerator ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets);
}
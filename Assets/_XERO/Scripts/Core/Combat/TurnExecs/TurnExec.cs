using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class TurnExec: MonoBehaviour
{
    [SerializeField] private GameObject targetSelectors;
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

        if (attack.TargetSelectorType == null)
        {
            Debug.LogError($"[TurnExec] Attack '{attack.name}' has no target selector assigned.");
            yield break;
        }

        var targetSelector = (TargetSelector)targetSelectors.GetComponent(attack.TargetSelectorType.Type);

        if (targetSelector == null)
        {
            Debug.LogError($"[TurnExec] TargetSelector of type '{attack.TargetSelectorType}' not found on {targetSelectors.name} GameObject.");
            yield break;
        }

        // Select targets
        List<Participant> targets = null;
        yield return targetSelector.SelectTargetsAsync(executor, selectedTargets => targets = selectedTargets);

        if (targets == null || targets.Count == 0)
        {
            yield break;
        }

        yield return ExecuteTurn(executor, attack, targets);
    }

    protected virtual void Reset()
    {
        if (targetSelectors == null)
        {
            var t = FindFirstObjectByType<TargetSelector>();
            if (t != null)
            {
                targetSelectors = t.gameObject;
            }
        }
    }

    protected abstract IEnumerator ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets);
}
using UnityEngine;
using System.Collections.Generic;

public abstract class TurnExec: MonoBehaviour
{
    [SerializeField] private AttackSelector attackSelector;

    public void ExecuteTurn(Participant executor)
    {
        var attack = attackSelector.SelectAttack();
        var targets = attack.TargetSelector.SelectTargets(executor);
        ExecuteTurn(executor, attack, targets);
    }

    protected abstract void ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets);
}
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnExec : TurnExec
{
    protected override void ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        Debug.Log($"Executing enemy turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");
        return;
    }
}
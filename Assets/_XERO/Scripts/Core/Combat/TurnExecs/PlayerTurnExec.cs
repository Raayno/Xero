using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnExec : TurnExec
{
    protected override IEnumerator ExecuteTurn(Participant executor, AttackDataSO attack, List<Participant> targets)
    {
        Debug.Log($"Executing player turn for {executor.name} with attack {attack.name} on targets: {string.Join(", ", targets.ConvertAll(t => t.name))}");

        
        yield break;
    }
}
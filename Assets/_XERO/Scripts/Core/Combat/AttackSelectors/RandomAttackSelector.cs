using System.Collections.Generic;
using UnityEngine;

public class RandomAttackSelector : AttackSelector
{
    protected override AttackDataSO SelectAttack(List<AttackDataSO> attacks)
    {
        return attacks[Random.Range(0, attacks.Count)];
    }
}
using System;
using UnityEngine;

public class EnemyParticipant : Participant
{
    [SerializeField] private EnemyCombatTargetData enemyCombatTargetData;

    // Add enemy-specific combat logic here later.
    public EnemyCombatTargetData GetData()
    {
        return enemyCombatTargetData;
    }
}

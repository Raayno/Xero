using System;
using UnityEngine;

public class EnemyCombatTarget : CombatTarget
{
    [SerializeField] private EnemyCombatTargetData enemyCombatTargetData;

    // Add enemy-specific combat logic here later.
    public EnemyCombatTargetData GetData()
    {
        return enemyCombatTargetData;
    }
}

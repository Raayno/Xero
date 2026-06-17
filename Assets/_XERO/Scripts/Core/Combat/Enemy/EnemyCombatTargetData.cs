using System;
using System.Collections.Generic;

[Serializable]
public class EnemyCombatTargetData : CombatTargetData
{
    public List<EnemyAttackDataSO> attacks;
}

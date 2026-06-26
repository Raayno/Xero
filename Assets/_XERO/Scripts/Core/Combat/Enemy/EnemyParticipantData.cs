using System;
using System.Collections.Generic;

[Serializable]
public class EnemyParticipantData : CombatTargetData
{
    public List<EnemyAttackDataSO> attacks;
}

using System;
using System.Collections.Generic;

[Serializable]
public class PlayerCombatTargetData : CombatTargetData
{
    public List<PlayerAttackDataSO> attacks;
}

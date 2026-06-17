using UnityEngine;

public class PlayerCombatTarget : CombatTarget
{

    [SerializeField] private PlayerCombatTargetData combatTargetData;

    public PlayerCombatTargetData GetData() => combatTargetData;
    // Add player-specific combat logic here later.
}
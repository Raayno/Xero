using UnityEngine;

public static class SpecialCombatDataCarrier
{
    private static BattleEntryType _battleEntryType = BattleEntryType.EnemyAttack;

    /// <summary>
    /// Represents the way in which the battle is currently being entered.
    /// </summary>
    public static BattleEntryType BattleEntryType
    {
        get => _battleEntryType;
        set
        {
            if (!VariablesLockedForTransition)
            {
                if (enableDebug) Debug.Log($"BattleEntryType changed from {_battleEntryType} to {value}");
                _battleEntryType = value;
            }
            else
            {
                if (enableDebug) Debug.LogWarning("Attempted to set BattleEntryType during transition. Change ignored.");
            }
        }
    }

    /// <summary>
    /// Set to true when the transition to combat starts, use to prevent overriding values from OnDisable or OnDestroy events of other scripts during the transition
    /// </summary>
    public static bool VariablesLockedForTransition { get; set; }= false;

    private static readonly bool enableDebug = true;
}

public enum BattleEntryType
{
    EnemyAttack,
    PlayerParry,
    PlayerAttack
}

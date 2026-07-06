using UnityEngine;

public static class CombatEnemyDataCarrier
{
    private static readonly bool enableDebug = true;
    private static CombatEnemiesData combatParticipantsData;

    public static CombatEnemiesData CombatEnemiesData
    {
        get
        {
            if (combatParticipantsData == null)
            {
                Debug.LogError("[CombatDataCarrier] CombatInitializationData is null. Returning a new instance.");
                return null;
            }
            return combatParticipantsData;
        }
        set
        {
            if (enableDebug) Debug.Log($"[CombatDataCarrier] CombatInitializationData set from {value.GetType()}.");
            combatParticipantsData = value;
        }
    }
    public static CombatResolutionData CombatResolutionData
    {
        get
        {
            if (CombatResolutionData == null)
            {
                Debug.LogError("[CombatDataCarrier] CombatResolutionData is null. Returning a new instance.");
                return new CombatResolutionData();
            }
            return CombatResolutionData;
        }
        set
        {
            if (enableDebug) Debug.Log($"[CombatDataCarrier] CombatResolutionData set from {value.GetType()}.");
            CombatResolutionData = value;
        }
    }
}

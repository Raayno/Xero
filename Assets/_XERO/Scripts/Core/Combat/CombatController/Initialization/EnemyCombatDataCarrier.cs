using UnityEngine;

[System.Serializable]
public class EnemiesCombatData
{
    [SerializeField] private EnemyParticipant[] enemyParticipantPrefabs;
    [SerializeField, Gaskellgames.ReadOnly] private string freeRoamEnemyPersistenceKey;

    public EnemyParticipant[] EnemyParticipants => enemyParticipantPrefabs;
    public string FreeRoamEnemyPersistenceKey => freeRoamEnemyPersistenceKey;

    public EnemiesCombatData(EnemiesCombatData defaultEnemiesData = null, EnemyParticipant[] enemyParticipantPrefabs = null, string freeRoamEnemyPersistenceKey = null)
    {
        this.enemyParticipantPrefabs = enemyParticipantPrefabs ?? defaultEnemiesData.enemyParticipantPrefabs ?? new EnemyParticipant[0];
        this.freeRoamEnemyPersistenceKey = freeRoamEnemyPersistenceKey ?? defaultEnemiesData.freeRoamEnemyPersistenceKey ?? string.Empty;
    }
}

public static class EnemyCombatDataCarrier
{
    private static readonly bool enableDebug = true;
    private static EnemiesCombatData combatParticipantsData;

    public static EnemiesCombatData EnemiesCombatData
    {
        get
        {
            if (combatParticipantsData == null || combatParticipantsData.EnemyParticipants.Length == 0)
            {
                Debug.LogError("[EnemyCombatDataCarrier] CombatEnemiesData is null. Returning a new instance.");
                return null;
            }
            return combatParticipantsData;
        }
        set
        {
            if (value == null || value.EnemyParticipants.Length == 0)
            {
                Debug.LogError("[EnemyCombatDataCarrier] Attempted to set CombatEnemiesData to null or empty. Ignoring.");
                return;
            }
            if (enableDebug) Debug.Log($"[EnemyCombatDataCarrier] CombatEnemiesData set to {value}.");
            combatParticipantsData = value;
        }
    }
}

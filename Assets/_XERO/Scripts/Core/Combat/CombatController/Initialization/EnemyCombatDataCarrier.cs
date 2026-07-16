using UnityEngine;

[System.Serializable]
public class EnemiesCombatData
{
    [Header("Enemy Participant Prefabs")]
    [SerializeField] private EnemyParticipant[] enemyParticipantPrefabs;

    public EnemyParticipant[] EnemyParticipants => enemyParticipantPrefabs;

    public EnemiesCombatData(EnemyParticipant[] enemyParticipantPrefabs = null)
    {
        this.enemyParticipantPrefabs = enemyParticipantPrefabs ?? (new EnemyParticipant[0]);
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

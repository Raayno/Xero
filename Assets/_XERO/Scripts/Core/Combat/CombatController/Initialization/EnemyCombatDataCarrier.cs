using UnityEngine;

[System.Serializable]
public class EnemiesCombatData
{
    [SerializeField] private EnemyParticipant[] enemyParticipantPrefabs;
    [SerializeField] private bool canReappearInFreeRoam;
    [SerializeField, Gaskellgames.ReadOnly] private string freeRoamEnemyPersistenceKey;
    [SerializeField] private string sceneToLoadAfterCombat;

    public EnemyParticipant[] EnemyParticipants => enemyParticipantPrefabs;
    public string FreeRoamEnemyPersistenceKey => freeRoamEnemyPersistenceKey;
    public bool CanReappearInFreeRoam => canReappearInFreeRoam;
    public string SceneToLoadAfterCombatAddressibleKey => sceneToLoadAfterCombat;
    
    public EnemiesCombatData(EnemiesCombatData defaultEnemiesData = null, EnemyParticipant[] enemyParticipantPrefabs = null, string freeRoamEnemyPersistenceKey = null, bool? canReappearInFreeRoam = null, string sceneToLoadAfterCombat = null)
    {
        this.enemyParticipantPrefabs = enemyParticipantPrefabs ?? (defaultEnemiesData != null ? defaultEnemiesData.enemyParticipantPrefabs : new EnemyParticipant[0]);
        this.freeRoamEnemyPersistenceKey = freeRoamEnemyPersistenceKey ?? (defaultEnemiesData != null ? defaultEnemiesData.freeRoamEnemyPersistenceKey : string.Empty);
        this.canReappearInFreeRoam = canReappearInFreeRoam ?? (defaultEnemiesData != null ? defaultEnemiesData.canReappearInFreeRoam : true);
        this.sceneToLoadAfterCombat = sceneToLoadAfterCombat ?? (defaultEnemiesData != null ? defaultEnemiesData.sceneToLoadAfterCombat : string.Empty);
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

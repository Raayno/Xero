using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class CombatInitialization
{
    [Header("Fallback if DataCarrier was not set")]
    [SerializeField] private PlayerParticipant[] defaultPlayerParticipants;
    [SerializeField] private CombatEnemiesData enemiesData = new();
    private CombatEnemiesData EnemiesData
    {
        get
        {
            if (CombatEnemyDataCarrier.CombatEnemiesData != null)
            {
                return CombatEnemyDataCarrier.CombatEnemiesData;
            }
            else
            {
                Debug.LogWarning("[CombatInitialization] CombatParticipantsDataCarrier data is null. Returning default arena preset.");
                return enemiesData;
            }
        }
    }
    [SerializeField] private CombatPositioning combatPositioning = new();
    public CombatPositioning CombatPositioning => combatPositioning;
    public Transform PlayersTransform { get; set; }
    public Transform EnemiesTransform { get; set; }
    
    public void InitializeCombat(List<EnemyParticipant> enemyParticipants, List<PlayerParticipant> playerParticipants)
    {
        // This is a fallback in case Players were not assigned in the FreeRoam
        if (PlayerCombatDataCarrier.PlayerParticipants == null || PlayerCombatDataCarrier.PlayerParticipants.Length == 0)
        {
            if (defaultPlayerParticipants == null || defaultPlayerParticipants.Length == 0)
            {
                Debug.LogError("[CombatInitialization] DefaultPlayerParticipants is null or empty. Cannot initialize combat.");
                return;
            }
            Debug.LogError("[CombatInitialization] PlayerParticipants is null or empty. Using default player prefab.");
            PlayerCombatDataCarrier.PlayerParticipants = defaultPlayerParticipants;
        }

        SpecialCombatDataCarrier.VariablesLockedForTransition = false; // Unlock variables after transition is complete

        InstantiateParticipants(PlayerCombatDataCarrier.PlayerParticipants, true);
        InstantiateParticipants(EnemiesData.EnemyParticipants, false);

        void InstantiateParticipants(Participant[] prefabs, bool isPlayer)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                Pose poseData = combatPositioning.GetPose(isPlayer, i, PlayerCombatDataCarrier.PlayerParticipants.Length, EnemiesData.EnemyParticipants.Length);

                var instance = UnityEngine.Object.Instantiate(prefabs[i], poseData.position, poseData.rotation, isPlayer ? PlayersTransform : EnemiesTransform);

                if (isPlayer) playerParticipants.Add((PlayerParticipant)instance);
                else enemyParticipants.Add((EnemyParticipant)instance);
            }
        }
    }
}

[Serializable]
public class CombatEnemiesData
{
    [Header("Enemy Participant Prefabs")]
    [SerializeField] private EnemyParticipant[] enemyParticipantPrefabs;

    public EnemyParticipant[] EnemyParticipants => enemyParticipantPrefabs;
}

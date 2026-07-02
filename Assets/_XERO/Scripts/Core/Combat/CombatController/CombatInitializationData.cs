using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class CombatInitializationData
{
    [SerializeField] private bool allowOverwrite = false;

    [Header("Participant Prefabs")]
    [Tooltip("Also use these for the initial positioning of participants in the scene with [Add Current Positioning of Participants]. Be sure to change back to prefabs, otherwise nothing will load.")]
    [SerializeField] private PlayerParticipant[] playerParticipantPrefabs;
    
    [Tooltip("Also use these for the initial positioning of participants in the scene with [Add Current Positioning of Participants]. Be sure to change back to prefabs, otherwise nothing will load.")]
    [SerializeField] private EnemyParticipant[] enemyParticipantPrefabs;
    
    [Header("Positioning")]
    [Tooltip("Kinda a dictionary, number of players and enemies as key, positions and rotations as value. This is used to set the initial positions and rotations of participants in the scene.")]
    [SerializeField] private List<CombatPositionsAndRotations> combatPositionsAndRotations = new();
    public Pose GetPose(bool isPlayer, int index, int playerCount, int enemyCount)
    {
        var c = combatPositionsAndRotations.Find(cpr => cpr.Key == (playerCount, enemyCount));
        if (c == null)
        {
            Debug.LogWarning($"[CombatInitializationData] No CombatPositionsAndRotations found for {playerCount} players and {enemyCount} enemies.");
            return default;
        }
        return c.GetPose(isPlayer, index);
    }

    public PlayerParticipant[] PlayerParticipants => playerParticipantPrefabs;
    public EnemyParticipant[] EnemyParticipants => enemyParticipantPrefabs;
    
    public void AddCurrentPositioningOfParticipants(List<Participant> players, List<Participant> enemies)
    {
        if (players.Count == 0 || enemies.Count == 0)
        {
            Debug.LogWarning("[CombatInitializationData] Cannot add current positioning of participants because either playerParticipants or enemyParticipants is empty.");
            return;
        }

        var c = combatPositionsAndRotations.Find(cpr => cpr.Key == (players.Count, enemies.Count));
        if (c != null)
        {
            if (!allowOverwrite)
            {
                Debug.LogWarning($"[CombatInitializationData] CombatPositionsAndRotations for {players.Count} players and {enemies.Count} enemies already exists. Set 'allowOverwrite' to true to overwrite it and try again.");
                return;
            }
            else
            {
                combatPositionsAndRotations.Remove(c);
                Debug.Log($"[CombatInitializationData] Overwriting CombatPositionsAndRotations for {players.Count} players and {enemies.Count} enemies.");
            }
        }
        else
        {
            Debug.Log($"[CombatInitializationData] Adding CombatPositionsAndRotations for {players.Count} players and {enemies.Count} enemies.");
        }

        c = new CombatPositionsAndRotations();

        c.Set(
            players.Select(p => new Pose(p.transform.position, p.transform.rotation)).ToArray(),
            enemies.Select(e => new Pose(e.transform.position, e.transform.rotation)).ToArray()
        );

        combatPositionsAndRotations.Add(c);
    }
}

[Serializable]
public class CombatPositionsAndRotations
{
    [SerializeField] private Pose[] playerPositionsAndRotations;
    [SerializeField] private Pose[] enemyPositionsAndRotations;

    public (int, int) Key => (playerPositionsAndRotations.Length, enemyPositionsAndRotations.Length);
    public Pose GetPose(bool isPlayer, int index) => isPlayer ? playerPositionsAndRotations[index] : enemyPositionsAndRotations[index];
    public void Set(Pose[] playerPoses, Pose[] enemyPoses)
    {
        Debug.Log($"[CombatPositionsAndRotations] Setting positions and rotations for {playerPoses.Length} players and {enemyPoses.Length} enemies.");
        playerPositionsAndRotations = playerPoses;
        enemyPositionsAndRotations = enemyPoses;
    }
}
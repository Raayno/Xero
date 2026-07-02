using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CombatInitializationData : ScriptableObject
{
    [Header("Combat Initialization Data")]
    [SerializeField] private List<PlayerParticipant> playerParticipantSOs = new();
    [SerializeField] private List<EnemyParticipant> enemyParticipantsSOs = new();

    public List<PlayerParticipant> PlayerParticipants => playerParticipantSOs;
    public List<EnemyParticipant> EnemyParticipants => enemyParticipantsSOs;
}
using UnityEngine;

[System.Serializable]
public class PlayersCombatData
{
    [Header("Player Participant Prefabs")]
    [SerializeField] private PlayerParticipant[] playerParticipantPrefabs;

    public PlayerParticipant[] PlayerParticipants => playerParticipantPrefabs;

    public PlayersCombatData(PlayerParticipant[] playerParticipantPrefabs = null)
    {
        this.playerParticipantPrefabs = playerParticipantPrefabs ?? (new PlayerParticipant[0]);
    }
}

public static class PlayerCombatDataCarrier
{
    private static readonly bool enableDebug = true;
    private static PlayersCombatData playersCombatData;

    public static PlayersCombatData PlayersCombatData
    {
        get
        {
            if (playersCombatData == null || playersCombatData.PlayerParticipants.Length == 0)
            {
                //Debug.LogError("[PlayerCombatDataCarrier] PlayersCombatData is null or doesn't contain any players. Returning null.");
                return null;
            }
            return playersCombatData;
        }
        set
        {
            if (value == null || value.PlayerParticipants.Length == 0)
            {
                Debug.LogError("[PlayerCombatDataCarrier] Attempted to set PlayersCombatData to null or empty. Ignoring.");
                return;
            }
            if (enableDebug) Debug.Log($"[PlayerCombatDataCarrier] PlayerParticipants set to {value}.");
            playersCombatData = value;
        }
    }
}

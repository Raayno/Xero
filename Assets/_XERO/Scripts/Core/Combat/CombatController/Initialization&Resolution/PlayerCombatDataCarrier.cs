using UnityEngine;

public static class PlayerCombatDataCarrier
{
    private static PlayerParticipant[] playerParticipants;

    public static PlayerParticipant[] PlayerParticipants
    {
        get
        {
            if (playerParticipants == null || playerParticipants.Length == 0)
            {
                Debug.LogError("[PlayerCombatDataCarrier] PlayerParticipants is null or empty. Returning a new instance.");
                return new PlayerParticipant[0];
            }
            return playerParticipants;
        }
        set
        {
            if (value == null || value.Length == 0)
            {
                Debug.LogError("[PlayerCombatDataCarrier] Attempted to set PlayerParticipants to null or empty. Ignoring.");
                return;
            }
            Debug.Log($"[PlayerCombatDataCarrier] PlayerParticipants set from {value.GetType()}.");
            playerParticipants = value;
        }
    }
}

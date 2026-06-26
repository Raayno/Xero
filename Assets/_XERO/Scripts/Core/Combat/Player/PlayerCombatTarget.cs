using UnityEngine;

public class PlayerParticipant : Participant
{

    [SerializeField] private PlayerCombatTargetData combatTargetData;

    public PlayerCombatTargetData GetData() => combatTargetData;
    // Add player-specific combat logic here later.
}
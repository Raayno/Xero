using UnityEngine;

public class EnemyParticipant : Participant
{
    [SerializeField] private EnemyParticipantData enemyCombatTargetData;

    // Add enemy-specific combat logic here later.
    public EnemyParticipantData GetData()
    {
        return enemyCombatTargetData;
    }
}

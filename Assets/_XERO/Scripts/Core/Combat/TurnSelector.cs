using System.Collections.Generic;
using UnityEngine;

public abstract class TurnSelector : MonoBehaviour
{
    public List<Participant> TurnTimeline { get; protected set; } = new();

    [Header("Turn Timeline Parameters")]
    [Tooltip("The amount of turns to plan ahead in the timeline")]
    [SerializeField] protected int foresightLength = 10;

    /// <summary>
    /// Number of turns that have been completed.
    /// </summary>
    protected int turnCount = -1;

    /// <summary>
    /// Plans the turn timeline based on the provided player and enemy participants. This method should be called when a new turn begins, and it updates the timeline to reflect the current state of the combatants.
    /// </summary>
    public void NextTurn(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants)
    {
        if (playerParticipants == null || enemyParticipants == null)
        {
            Debug.LogWarning("[CombatTimelineController] Cannot update timeline because one or both participant lists are null.");
            return;
        }
        
        PlanTurnTimeline(playerParticipants, enemyParticipants);
        ++turnCount;
    }

    protected abstract void PlanTurnTimeline(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants);

    public Participant GetCurrentParticipant()
    {
        if (TurnTimeline == null || TurnTimeline.Count == 0)
        {
            Debug.LogWarning("[CombatTimelineController] Timeline is empty. No current target available.");
            return null;
        }

        return TurnTimeline[turnCount];
    }
}
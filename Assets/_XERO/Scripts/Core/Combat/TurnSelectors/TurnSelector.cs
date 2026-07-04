using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class TurnSelector : ScriptableObject
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
    /// Plans the turn timeline based on the provided player and enemy participants. This method should be called when a new turn begins, and it updates the timeline to reflect the current state of the participants.
    /// </summary>
    public void NextTurn(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants)
    {
        if (playerParticipants == null || enemyParticipants == null)
        {
            Debug.LogWarning("[CombatTimelineController] Cannot update timeline because one or both participant lists are null.");
            return;
        }
        
        PlanTurnTimeline(ValidatedParticipantList(playerParticipants), ValidatedParticipantList(enemyParticipants));
        ++turnCount;
    }

    private List<PlayerParticipant> ValidatedParticipantList(List<PlayerParticipant> participants) => participants.Where(p => p != null && !p.damageable.IsDefeated).ToList();
    private List<EnemyParticipant> ValidatedParticipantList(List<EnemyParticipant> participants) => participants.Where(p => p != null && !p.damageable.IsDefeated).ToList();

    protected virtual void PlanTurnTimeline(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants)
    {
        if (TurnTimeline.Count == 0 || TurnTimeline == null || TurnTimeline.All(p => p == null))
            InitializeTimeline(playerParticipants, enemyParticipants);
        else
            UpdateTimeline(playerParticipants, enemyParticipants);
    }

    protected abstract void InitializeTimeline(List<PlayerParticipant> players, List<EnemyParticipant> enemies);
    protected abstract void UpdateTimeline(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants);

    public Participant GetCurrentParticipant()
    {
        if (TurnTimeline == null || TurnTimeline.Count == 0)
        {
            Debug.LogWarning("[CombatTimelineController] Timeline is empty. No current target available.");
            return null;
        }

        if (TurnTimeline[0] == null)
        {
            Debug.LogWarning("[CombatTimelineController] Current participant is null. This may indicate an issue with the timeline or a missed participant destruction.");
            return null;
        }

        return TurnTimeline[0];
    }

    public void ResetTimeline()
    {
        TurnTimeline.Clear();
        turnCount = -1;
    }
}
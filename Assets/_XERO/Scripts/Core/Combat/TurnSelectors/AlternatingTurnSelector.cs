using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Alternate between player and enemy participants in the turn timeline.
/// The first participant in the timeline will always be a player participant, followed by an enemy participant, and so on.
/// If some are removed, their turn is simply skipped, and the next participant in line will take their turn.
/// If a player or enemy participant is removed, the timeline will continue to alternate between the remaining participants.
/// </summary>
[EnsureAssetInstance]
public class AlternatingTurnSelector : TurnSelector
{
    protected override void InitializeTimeline(List<PlayerParticipant> players, List<EnemyParticipant> enemies)
    {
        TurnTimeline.Clear();
        int nextPlayerIndex = 0;
        int nextEnemyIndex = 0;

        for (int i = 0; i < foresightLength; i++)
        {
            if (i % 2 == 0)
            {
                TurnTimeline.Add(players[nextPlayerIndex % players.Count]);
                ++nextPlayerIndex;
            }
            else
            {
                TurnTimeline.Add(enemies[nextEnemyIndex % enemies.Count]);
                ++nextEnemyIndex;
            }
        }
    }

    protected override void UpdateTimeline(List<PlayerParticipant> playerParticipants, List<EnemyParticipant> enemyParticipants)
    {
        // I've got no idea if this is faster than jus reinitializing the timeline, but it should be more efficient in terms of memory allocation since we are reusing the existing list instead of creating a new one.
        TurnTimeline.RemoveAt(0); // Remove the participant who just completed their turn
        TurnTimeline.RemoveAll(p => p == null || (!playerParticipants.Contains(p) && !enemyParticipants.Contains(p)));

        bool lastEntryIsPlayer = TurnTimeline.LastOrDefault() is PlayerParticipant;
        int nextPlayerIndex = playerParticipants.FindIndex(p => p.GetType() == TurnTimeline.FindLast(t => t is PlayerParticipant).GetType()) + 1; // if not found, will return -1, which is fine because we will just start from the beginning of the list (-1 + 1 = 0)
        int nextEnemyIndex = enemyParticipants.FindIndex(e => e.GetType() == TurnTimeline.FindLast(t => t is EnemyParticipant).GetType()) + 1;
        for (int i = TurnTimeline.Count; i < foresightLength; i++)
        {
            if (lastEntryIsPlayer)
            {
                if (enemyParticipants.Count == 0) break;
                // add enemy participant next
                TurnTimeline.Add(enemyParticipants[nextEnemyIndex % enemyParticipants.Count]);
                ++nextEnemyIndex;
            }
            else
            {
                if (playerParticipants.Count == 0) break;
                // add player participant next
                TurnTimeline.Add(playerParticipants[nextPlayerIndex % playerParticipants.Count]);
                ++nextPlayerIndex;
            }

            lastEntryIsPlayer = !lastEntryIsPlayer;
        }
    }
}
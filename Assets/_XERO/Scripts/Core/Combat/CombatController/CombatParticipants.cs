using UnityEngine;
using System.Collections.Generic;

public partial class CombatController : MoreMountains.Tools.MMSingleton<CombatController>
{
    [Header("Participants")]
    private readonly List<EnemyParticipant> aliveEnemyParticipants = new();
    private readonly List<PlayerParticipant> alivePlayerParticipants = new();
    
    public List<Participant> GetEnemiesAsParticipant() => new(aliveEnemyParticipants);
    public List<Participant> GetPlayersAsParticipant() => new(alivePlayerParticipants);

    private readonly List<EnemyParticipant> defeatedEnemyParticipants = new();
    private readonly List<PlayerParticipant> defeatedPlayerParticipants = new();

    public List<Participant> GetDefeatedEnemiesAsParticipant() => new(defeatedEnemyParticipants);
    public List<Participant> GetDefeatedPlayersAsParticipant() => new(defeatedPlayerParticipants);

    private void MoveParticipantToAlive(Participant participant)
    {
        if (participant is PlayerParticipant player)
        {
            alivePlayerParticipants.Add(player);
            defeatedPlayerParticipants.Remove(player);
        }
        else if (participant is EnemyParticipant enemy)
        {
            aliveEnemyParticipants.Add(enemy);
            defeatedEnemyParticipants.Remove(enemy);
        }
    }

    private void MoveParticipantToDefeated(Participant participant)
    {
        if (participant is PlayerParticipant player)
        {
            alivePlayerParticipants.Remove(player);
            defeatedPlayerParticipants.Add(player);
        }
        else if (participant is EnemyParticipant enemy)
        {
            aliveEnemyParticipants.Remove(enemy);
            defeatedEnemyParticipants.Add(enemy);
        }

        CheckForCombatEnd();
    }

    private void RemoveAndDestroyAllParticipants()
    {
        foreach (var player in alivePlayerParticipants)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        alivePlayerParticipants.Clear();

        foreach (var enemy in aliveEnemyParticipants)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        aliveEnemyParticipants.Clear();

        foreach (var player in defeatedPlayerParticipants)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        defeatedPlayerParticipants.Clear();

        foreach (var enemy in defeatedEnemyParticipants)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        defeatedEnemyParticipants.Clear();
    }
}

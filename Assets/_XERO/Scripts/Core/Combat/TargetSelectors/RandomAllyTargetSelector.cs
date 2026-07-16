using System.Collections.Generic;
using UnityEngine;

public class RandomAllyTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets(Participant self)
    {
        var players = combatController.GetPlayersAsParticipant();
        players.Remove(self);
        return new() {players[Random.Range(0, players.Count)]};        
    }
}
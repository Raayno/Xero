using System.Collections.Generic;
using UnityEngine;

public class RandomPlayerTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        var players = combatController.GetPlayersAsParticipant();
        return new() {players[Random.Range(0, players.Count)]};        
    }
}
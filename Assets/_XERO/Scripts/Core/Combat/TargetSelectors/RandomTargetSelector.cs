using System.Collections.Generic;
using UnityEngine;

public class RandomTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        var players = combatController.GetPlayers();
        var enys = combatController.GetEnemies();
        int randomIndex = Random.Range(0, players.Count + enys.Count);
        if (randomIndex < players.Count)
        {
            return new() { players[randomIndex] };
        }
        else
        {
            return new() { enys[randomIndex - players.Count] };
        }      
    }
}
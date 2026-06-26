using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        var enys = combatController.GetEnemies();
        return new() {enys[Random.Range(0, enys.Count)]};        
    }
}
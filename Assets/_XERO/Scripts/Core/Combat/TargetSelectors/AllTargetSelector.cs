using System.Collections.Generic;

public class AllTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        var a = combatController.GetPlayers();
        a.AddRange(combatController.GetEnemies());
        return a;
    }
}
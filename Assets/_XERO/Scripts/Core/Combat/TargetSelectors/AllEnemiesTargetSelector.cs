using System.Collections.Generic;

public class AllEnemiesTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        return combatController.GetEnemies();
    }
}
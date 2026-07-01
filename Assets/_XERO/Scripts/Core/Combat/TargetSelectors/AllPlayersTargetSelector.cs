using System.Collections.Generic;

public class AllPlayersTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        return combatController.GetPlayers();
    }
}
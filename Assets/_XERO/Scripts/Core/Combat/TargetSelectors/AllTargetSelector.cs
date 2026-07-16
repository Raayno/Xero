using System.Collections.Generic;

public class AllTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets()
    {
        var a = combatController.GetPlayersAsParticipant();
        a.AddRange(combatController.GetEnemiesAsParticipant());
        return a;
    }
}
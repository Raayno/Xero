using System.Collections.Generic;

public class AlliesTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets(Participant self) => new() {self};
}
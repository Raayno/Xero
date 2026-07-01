using System.Collections.Generic;

public class SelfTargetSelector : TargetSelector
{
    protected override List<Participant> SelectTargets(Participant self) => new() {self};
}
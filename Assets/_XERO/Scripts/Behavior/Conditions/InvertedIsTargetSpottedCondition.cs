using System;
using Unity.Behavior;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "NotIsTargetSpotted", story: "[TargetEyes] on [TargetTag] is not within [RadiusAndAngle] or is hidden behind something (except [ExcludedLayers] ) from [AgentEyes]", category: "Conditions", id: "752341b170f9884cdf22000d288b5a5a")]
public class InvertedIsTargetSpottedCondition : IsTargetSpottedCondition
{
    public override bool IsTrue()
    {
        return !base.IsTrue();
    }
}

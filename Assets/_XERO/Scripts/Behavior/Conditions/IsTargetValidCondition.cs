using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsTargetValid", story: "[TargetEyes] is assigned. Invert? [IsInverted]", category: "Conditions", id: "a240ef3b243349bd9ff8b65c7b3f4283")]
public partial class IsTargetValidCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> TargetEyes;
    [SerializeReference] public BlackboardVariable<bool> IsInverted;

    public override bool IsTrue() => IsInverted.Value ? !CheckIfTargetIsValid() : CheckIfTargetIsValid();

    private bool CheckIfTargetIsValid()
    {
        if (TargetEyes == null || TargetEyes.Value == null)
        {
            return false;
        }
        return true;
    }
}

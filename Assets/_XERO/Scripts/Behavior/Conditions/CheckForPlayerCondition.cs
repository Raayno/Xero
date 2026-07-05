using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckForPlayer", story: "If [PlayerTarget] is not null", category: "Conditions", id: "f441d852b0d47f38972dee70e3550ab5")]
public partial class CheckForPlayerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> PlayerTarget;

    public override bool IsTrue()
    {
        if(PlayerTarget != null)
        {
            return true;
        }
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}

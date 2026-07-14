using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Set Variable To Null",
    description: "Tries to set the value of a given variable to null if it supports reference types.",
    story: "Set [Variable] to null",
    category: "Action/Blackboard",
    id: "bc3e2ba20ca28228ab4e171e64cc7f3c")] // Wygenerowano nowe, unikalne ID
internal partial class SetVariableValueToNullAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable Variable;

    protected override Status OnStart()
    {
        if (Variable == null)
        {
            return Status.Failure;
        }

        Type variableType = Variable.Type;

        if (variableType == null)
        {
            return Status.Failure;
        }

        if (!variableType.IsValueType || Nullable.GetUnderlyingType(variableType) != null)
        {
            Variable.ObjectValue = null;
            return Status.Success;
        }

        Debug.LogWarning($"Cannot set '{Variable.Name}' to null because it is a value type ({variableType.Name}).");
        return Status.Failure;
    }
}

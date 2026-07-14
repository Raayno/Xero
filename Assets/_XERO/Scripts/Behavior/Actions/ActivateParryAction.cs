using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ActivateParry", story: "[Target] unlocks the parry system Invert [IsInvert]", category: "Action", id: "81933de0bd9fcde772e883633b346735")]
public partial class ActivateParryAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<bool> IsInvert;

    protected override Status OnStart()
    {
        if (Target.Value == null)
        {
            return Status.Failure;
        }

        var eyesTag = Target.Value.GetComponent<EyesTag>();

        if (eyesTag.NumbersOfEnemiesChasingThisPlayer <= 0 && !IsInvert.Value)
        {
            eyesTag.NumbersOfEnemiesChasingThisPlayer = 1;
            (bool flowControl, Status value) = HandleActivation(eyesTag);
            if (!flowControl)
            {
                return value;
            }
        }
        else if (eyesTag.NumbersOfEnemiesChasingThisPlayer > 0 && IsInvert.Value)
        {
            eyesTag.NumbersOfEnemiesChasingThisPlayer--;
            (bool flowControl, Status value) = HandleActivation(eyesTag);
            if (!flowControl)
            {
                return value;
            }
        }
        // else either the parry system is already active or the player is not being chased, so we don't need to do anything.

        return Status.Success;

        (bool flowControl, Status value) HandleActivation(EyesTag eyesTag)
        {
            ParryThirdPersonControllerExtension parryController;

            if (eyesTag != null)
            {
                parryController = eyesTag.ParryExtension;
            }
            else
            {
                parryController = Target.Value.GetComponentInChildren<ParryThirdPersonControllerExtension>();
                if (parryController == null)
                {
                    Debug.LogError("[ActivateParryAction] ParryThirdPersonControllerExtension not found on target or the children of its parent.");
                    return (flowControl: false, value: Status.Failure);
                }
            }

            if (parryController == null)
            {
                Debug.LogError("[ActivateParryAction] ParryThirdPersonControllerExtension not found on target or the children of its parent.");
                return (flowControl: true, value: default);
            }
            parryController.enabled = !IsInvert.Value;

            // Enable or disable the parry input system based on the IsInvert value
            ParryInput.Instance.IsEnabled = !IsInvert.Value;
            return (flowControl: true, value: default);
        }
    }
}


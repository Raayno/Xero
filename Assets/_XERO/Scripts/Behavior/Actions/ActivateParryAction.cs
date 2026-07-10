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
            Debug.LogError("[ActivateParryAction] Target is null. Cannot activate parry.");
            return Status.Failure;
        }

        var eyesTag = Target.Value.GetComponent<EyesTag>();

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
                return Status.Failure;
            }
        }

        parryController.enabled = !IsInvert.Value;

        // Enable or disable the parry input system based on the IsInvert value
        ParryInput.Instance.IsEnabled = !IsInvert.Value;

        return Status.Success;
    }
}


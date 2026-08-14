using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ActivateParry", story: "[Target] unlocks the [ParryAvailableModule] Parry bool [ParryIsEnabled] Invert [IsInvert]", category: "Action", id: "81933de0bd9fcde772e883633b346735")]
public partial class ActivateParryAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<PlayerBehavior_ParryModule> ParryAvailableModule;
    [SerializeReference] public BlackboardVariable<bool> ParryIsEnabled;
    [SerializeReference] public BlackboardVariable<bool> IsInvert;

    protected override Status OnStart()
    {
        if (Target.Value == null)
        {
            Debug.LogError("[ActivateParryAction] Target is not assigned.");
            return Status.Success; // Return success to avoid failing the behavior tree due to a missing target.
        }

        var eyesTag = Target.Value.GetComponent<EyesTagPlayer>();

        if (eyesTag.NumbersOfEnemiesChasingThisPlayer <= 0 && !IsInvert.Value)
        {
            eyesTag.NumbersOfEnemiesChasingThisPlayer = 1;
            HandleActivation(eyesTag);
        }
        else if (eyesTag.NumbersOfEnemiesChasingThisPlayer > 0 && IsInvert.Value)
        {
            eyesTag.NumbersOfEnemiesChasingThisPlayer--;
            HandleActivation(eyesTag, false);
        }
        // else either the parry system is already active or the player is not being chased, so we don't need to do anything.

        return Status.Success;

        void HandleActivation(EyesTagPlayer eyesTag, bool activate = true)
        {
            var playerBehavior = eyesTag.PlayerBehavior;
            if (playerBehavior == null)
            {
                Debug.LogError("[ActivateParryAction] PlayerBehavior component not found on the target.");
                return;
            }

            if (ParryAvailableModule.Value == null)
            {
                Debug.LogError("[ActivateParryAction] ParryModule reference is not set.");
                return;
            }

            if (activate)
            {
                playerBehavior.TryTransition(null, ParryAvailableModule.Value);
                ParryIsEnabled.Value = true;
            }
            else
            {
                playerBehavior.TryTransition(ParryAvailableModule.Value, null);
                ParryIsEnabled.Value = false;
            }
        }
    }
}

using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ResetEnemyAnimator", story: "Reset [Animator]", category: "Action", id: "d6038bab9465d3ac5c0b4d1fbef18438")]
public partial class ResetEnemyAnimatorAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [CreateProperty] private readonly string[] m_AnimatorParameters = new[] { "SpeedMagnitude", "IsStun", "IsDash" };
    private enum AnimatorParameter { SpeedMagnitude, IsStun, IsDash }

    protected override Status OnStart()
    {
        if (Animator == null || Animator.Value == null)
        {
            Debug.LogError("[ResetEnemyAnimatorAction] Animator is not assigned.");
            return Status.Failure;
        }

        Animator.Value.SetFloat(m_AnimatorParameters[(int)AnimatorParameter.SpeedMagnitude], 0f);
        Animator.Value.SetBool(m_AnimatorParameters[(int)AnimatorParameter.IsStun], false);
        Animator.Value.SetBool(m_AnimatorParameters[(int)AnimatorParameter.IsDash], false);

        return Status.Success;
    }
}


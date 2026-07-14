using MoreMountains.Feedbacks;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayFeedback", story: "Plays [FeedbackType] with [Intensity] through [Feedbacks] on [Agent]", category: "Action", id: "aebca5af56334fe89ef87e9e302a3bf6")]
public partial class PlayFeedbackAction : Action
{
    [SerializeReference] public BlackboardVariable<FeedbackType> FeedbackType;
    [SerializeReference] public BlackboardVariable<float> Intensity;
    [SerializeReference] public BlackboardVariable<Feedbacks> Feedbacks;
    [SerializeReference] public BlackboardVariable<Transform> Agent;
    protected override Status OnStart()
    {
        if (Feedbacks == null || FeedbackType == null)
        {
            Debug.LogWarning($"<color=orange>[PlayFeedbackAction]</color> Feedbacks or FeedbackType is null.");
            return Status.Failure;
        }
        Feedbacks.Value.PlayFeedback(FeedbackType.Value, Agent.Value.position, Intensity != null ? Intensity.Value : 1f);
        return Status.Success;
    }
}


using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAround", story: "[Agent] rotates by [Angle] to the left, and to the right for [LookCycleDuration] seconds", category: "Action", id: "a5089a6880776f369b2c9e516df0f5c2")]
public partial class LookAroundAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Angle;
    [SerializeReference] public BlackboardVariable<float> LookCycleDuration;

    [CreateProperty] private Transform m_AgentTransform;
    [CreateProperty] private Quaternion m_InitialRotation;
    [CreateProperty] private float m_ElapsedTime;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null || Angle == null || LookCycleDuration == null)
        {
            return Status.Failure;
        }

        m_AgentTransform = Agent.Value.transform;
        m_InitialRotation = m_AgentTransform.rotation;
        m_ElapsedTime = 0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_AgentTransform == null || Angle == null)
        {
            return Status.Failure;
        }

        m_ElapsedTime += Time.deltaTime;

        if (LookCycleDuration.Value <= 0f)
        {
            return Status.Failure;
        }
        float normalizedTime = Mathf.Clamp01(m_ElapsedTime / LookCycleDuration);
        float angleOffset = -Mathf.Sin(normalizedTime * Mathf.PI * 2f) * Mathf.Abs(Angle.Value);
        m_AgentTransform.rotation = m_InitialRotation * Quaternion.Euler(0f, angleOffset, 0f);

        if (m_ElapsedTime >= LookCycleDuration.Value)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}


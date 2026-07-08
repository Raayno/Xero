using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsTargetSpotted", story: "[TargetEyes] with [TargetTag] is NOT null, is within [RadiusAndAngle] and is NOT hidden behind anything except [ExcludedLayers] looking from [AgentEyes] Invert [IsInverted]", category: "Conditions", id: "752341b170f9884cdf22000d288b5a5f")]
public partial class IsTargetSpottedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> TargetEyes;
    [SerializeReference] public BlackboardVariable<string> TargetTag;
    [SerializeReference] public BlackboardVariable<Vector2> RadiusAndAngle;
    [SerializeReference] public BlackboardVariable<List<string>> ExcludedLayers;
    [SerializeReference] public BlackboardVariable<Transform> AgentEyes;
    [SerializeReference] public BlackboardVariable<bool> IsInverted; 
    private readonly bool enableDebug = false;

    public override bool IsTrue()
    {
        bool isInverted = IsInverted != null && IsInverted.Value;
        if (enableDebug) Debug.LogWarning("IsInverted: " + isInverted);
        return isInverted ? !CheckIfTargetIsSpotted() : CheckIfTargetIsSpotted();
    }

    private bool CheckIfTargetIsSpotted()
    {
        if (AgentEyes.Value == null)
        {
            Debug.LogError("AgentEyes is not assigned.");
            return false;
        }

        if (TargetEyes.Value == null)
        {
            if (enableDebug)
            {
                Debug.Log($"<color=red>TargetEyes is null.</color>");
            }
            return false;
        }

        Vector3 directionToTarget = TargetEyes.Value.position - AgentEyes.Value.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > RadiusAndAngle.Value.x)
        {
            if (enableDebug)
            {
                Debug.Log($"<color=red>Target is too far away.</color>");
            }
            return false;
        }

        float angleToTarget = Vector3.Angle(AgentEyes.Value.forward, IgnoreYComponent(directionToTarget));
        if (angleToTarget > RadiusAndAngle.Value.y/2)
        {
            if (enableDebug)
            {
                Debug.Log($"<color=red>Target is outside of the field of view.</color>");
            }
            return false;
        }

        LayerMask layerMask = ExcludedLayers != null && ExcludedLayers.Value != null
            ? ~LayerMask.GetMask(ExcludedLayers.Value.ToArray())
            : ~0;

        Vector3 rayDirection = directionToTarget.normalized;
        if (Physics.Raycast(AgentEyes.Value.position, rayDirection, out RaycastHit hit, distanceToTarget, layerMask, QueryTriggerInteraction.Ignore))
        {
            Transform targetRoot = TargetEyes.Value.root;
            bool hitIsTarget = hit.transform == TargetEyes.Value || hit.transform.IsChildOf(targetRoot);

            if (!hitIsTarget && !string.IsNullOrEmpty(TargetTag?.Value) && hit.transform.CompareTag(TargetTag.Value))
            {
                hitIsTarget = true;
            }

            if (!hitIsTarget)
            {
                if (enableDebug)
                {
                    Debug.Log($"<color=red>Target is hidden behind something else. Hit: {hit.transform.name}, Target Tag: {TargetTag.Value}</color>");
                }
                return false; // Target is hidden behind something else
            }
        }

        if (enableDebug)
        {
            Debug.Log($"<color=green>Target is spotted!</color>");
        }
        return true;
    }

    private Vector3 IgnoreYComponent(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }
}

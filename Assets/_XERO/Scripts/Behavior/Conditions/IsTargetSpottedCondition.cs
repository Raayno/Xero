using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsTargetSpotted", story: "[TargetEyes] on [TargetTag] is within [RadiusAndAngle] and is NOT hidden behind anything (except [ExcludedLayers] ) from [AgentEyes]", category: "Conditions", id: "752341b170f9884cdf22000d288b5a5f")]
public partial class IsTargetSpottedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> TargetEyes;
    [SerializeReference] public BlackboardVariable<string> TargetTag;
    [SerializeReference] public BlackboardVariable<Vector2> RadiusAndAngle;
    [SerializeReference] public BlackboardVariable<List<string>> ExcludedLayers;
    [SerializeReference] public BlackboardVariable<Transform> AgentEyes;
    private readonly bool enableDebug = true;

    public override bool IsTrue()
    {
        if (TargetEyes.Value == null || AgentEyes.Value == null)
        {
            return false;
        }

        Vector3 directionToTarget = TargetEyes.Value.position - AgentEyes.Value.transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > RadiusAndAngle.Value.x)
        {
            if (enableDebug)
            {
                Debug.Log($"Target is too far away. Distance: {distanceToTarget}, Radius: {RadiusAndAngle.Value.x}");
            }
            return false;
        }

        float angleToTarget = Vector3.Angle(AgentEyes.Value.forward, IgnoreYComponent(directionToTarget));
        if (angleToTarget > RadiusAndAngle.Value.y/2)
        {
            if (enableDebug)
            {
                Debug.Log($"Target is outside of the field of view. Angle: {angleToTarget}, Field of View: {RadiusAndAngle.Value.y}");
            }
            return false;
        }

        LayerMask layerMask = ~LayerMask.GetMask(ExcludedLayers.Value.ToArray());

        if (Physics.Raycast(AgentEyes.Value.transform.position, directionToTarget, out RaycastHit hit, distanceToTarget, layerMask))
        {
            if (!hit.transform.CompareTag(TargetTag.Value))
            {
                if (enableDebug)
                {
                    Debug.Log($"Target is hidden behind something else. Hit: {hit.transform.name}, Target Tag: {TargetTag.Value}");
                }
                return false; // Target is hidden behind something else
            }
        }

        return true;
    }

    private Vector3 IgnoreYComponent(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}

using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckEnemyZoneEscape", story: "Check [self] is outside of [RoamRadius] using [SpawnPosition] and set [State]", category: "Action", id: "89e9dcbd5eccf41545e4218948ce71db")]
public partial class CheckEnemyZoneEscapeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> RoamRadius;
    [SerializeReference] public BlackboardVariable<Vector3> SpawnPosition;
    [SerializeReference] public BlackboardVariable<EnemyRoamingStates> State;
    protected override Status OnUpdate()
    {
        float distanceToCheck = Vector3.Distance(Self.Value.transform.position, SpawnPosition.Value);
        float roamRadius = RoamRadius.Value;

        Debug.Log(distanceToCheck + ", " + roamRadius);
        if (distanceToCheck > roamRadius)
        {
            State.Value = EnemyRoamingStates.Patroling;
            return Status.Failure;
        }
        else
        {
            return Status.Running;
        }
    }
}


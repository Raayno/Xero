using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomPatrolPoint", story: "Using [SpawnPosition] and [RaomRadius] creates random [PatrolPoint] and Creates a Random [WaitDuration] using [Min] [Max] values", category: "Action", id: "80d5e86cb01275c73a52c0f531277316")]
public partial class GetRandomPatrolPointAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> SpawnPosition;
    [SerializeReference] public BlackboardVariable<float> RaomRadius;
    [SerializeReference] public BlackboardVariable<Vector3> PatrolPoint;
    [SerializeReference] public BlackboardVariable<float> WaitDuration;
    [SerializeReference] public BlackboardVariable<float> Min;
    [SerializeReference] public BlackboardVariable<float> Max;
    [SerializeReference] public BlackboardVariable<float> RoamRadius;

   protected override Status OnStart()
    {
        WaitDuration.Value = Random.Range(Min.Value, Max.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * RoamRadius.Value;
            Vector3 candidatePosition = SpawnPosition.Value + randomDirection;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, RoamRadius.Value, NavMesh.AllAreas))
            {
                PatrolPoint.Value = hit.position;
                return Status.Success; // Done, exit the node.
            }
        }

        PatrolPoint.Value = SpawnPosition.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

}


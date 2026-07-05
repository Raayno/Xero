using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetStartPosition", story: "Setting [SpawnPosition] as [enemy] startposition", category: "Action", id: "9e222854f3018ea6a42cc9df4c281ea0")]
public partial class SetStartPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> SpawnPosition;
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;
    protected override Status OnStart()
    {
        SpawnPosition.Value = Enemy.Value.transform.position;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TransformToVector3Action", story: "Set [Vector3Variable] to [Transform] position", category: "Action", id: "0c1fd72d5a14d04d7c5c876ca545f8c5")]
public partial class TransformToVector3Action : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Vector3Variable;
    [SerializeReference] public BlackboardVariable<Transform> Transform;

    protected override Status OnStart()
    {
        if (Vector3Variable == null || Transform == null)
        {
            return Status.Failure;
        }
        Vector3Variable.Value = Transform.Value.position;
        return Status.Success;
    }
}


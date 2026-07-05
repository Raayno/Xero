using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TestSequence", story: "This is test sequence", category: "Flow", id: "ce738757d44f41cfd1bb66ba0ba3ed08")]
public partial class TestSequence : Composite
{
    [SerializeReference] public Node Thisistestport1;
    [SerializeReference] public Node Thisistestport2;

    protected override Status OnStart()
    {
        
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


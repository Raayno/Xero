using UnityEngine;
using Unity.Behavior;
using System;

public class EnemyRoamZone : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent[] behaviorAgent;
    [SerializeField] private bool enableDebug;
    private bool isPlayerAssigned = false;

    private void OnTriggerEnter(Collider other)
    {
        AssignPlayerToBehaviorAgent(other);
    }

    private void OnTriggerStay(Collider other)
    {
        AssignPlayerToBehaviorAgent(other);
    }

    private void OnTriggerExit(Collider other)
    {
        //if (enableDebug) Debug.Log($"OnTriggerExit called with {other.name}");
        if (other.CompareTag("Player") && other.TryGetComponent(out EyesTag _))
        {
            UnassignPlayerFromBehaviorAgent();
        }
    }

    private void AssignPlayerToBehaviorAgent(Collider other)
    {
        if (isPlayerAssigned) return;

        if (!other.CompareTag("Player") || !other.TryGetComponent(out EyesTag _)) return;
        
        if (enableDebug) Debug.Log($"AssignPlayerToBehaviorAgent called with {other.name}");

        foreach (var agent in behaviorAgent)
        {
            if (enableDebug) Debug.Log($"Assigning player {other.name} to agent {agent.name}");
            agent.SetVariableValue("TargetEyes", other.transform);
        }
        isPlayerAssigned = true;
    }

    private void UnassignPlayerFromBehaviorAgent()
    {
        if (!isPlayerAssigned) return;

        if (enableDebug) Debug.Log($"UnassignPlayerFromBehaviorAgent called");
        foreach (var agent in behaviorAgent)
        {
            if (enableDebug) Debug.Log($"Unassigning player from agent {agent.name}");
            agent.SetVariableValue("UnassignTargetEyes", true);
        }
        isPlayerAssigned = false;
    }
}

using UnityEngine;
using Unity.Behavior;

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
        if (enableDebug) Debug.Log($"OnTriggerExit called with {other.name}");
        if (other.CompareTag("Player"))
        {
            UnassignPlayerFromBehaviorAgent();
        }
    }

    private void AssignPlayerToBehaviorAgent(Collider other)
    {
        if (enableDebug) Debug.Log($"AssignPlayerToBehaviorAgent called with {other.name}");
        if (isPlayerAssigned) return;

        if (!other.CompareTag("Player")) return;

        foreach (var agent in behaviorAgent)
        {
            agent.SetVariableValue("TargetEyes", other.transform);
        }
        isPlayerAssigned = true;
    }

    private void UnassignPlayerFromBehaviorAgent()
    {
        if (enableDebug) Debug.Log($"UnassignPlayerFromBehaviorAgent called");
        if (!isPlayerAssigned) return;

        foreach (var agent in behaviorAgent)
        {
            agent.SetVariableValue<Transform>("TargetEyes", null);
        }
        isPlayerAssigned = false;
    }
}

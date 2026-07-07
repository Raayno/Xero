using UnityEngine;
using Unity.Behavior;

public class EnemyRoamZone : MonoBehaviour
{
    private BehaviorAgent[] behaviorAgent;
    private bool isPlayerAssignedToBehaviorAgent = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerAssignedToBehaviorAgent = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerAssignedToBehaviorAgent = false;
        }
    }

    private void AssignPlayerToBehaviorAgent(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPlayerAssignedToBehaviorAgent) return;

        foreach (var agent in behaviorAgent)
        {
            agent?.SetVariable("TargetEyes", other.GetComponentInChildren<EyesTag>().transform);
        }
    }
}

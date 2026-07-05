using StarterAssets;
using System;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyFrontTriggerDetector : MonoBehaviour
{
    private const string CanSeePlayer = "CanSeePlayer";
    private const string PLAYER_TARGET = "PlayerTarget";
    [Tooltip("Angle in degrees considered 'in front'.")]
    [Range(0f, 180f)]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private BlackboardReference blackboard;

    [Header("Debug")]
    [SerializeField] private GameObject detectedPlayer;

    private GameObject targetPlayer;
    private bool isPlayerTriggered = false;

    private void Awake()
    {
        blackboard = GetComponent<BehaviorGraphAgent>().BlackboardReference;
        blackboard.SetVariableValue(CanSeePlayer, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController component))
        {
            isPlayerTriggered = true;
            if (!targetPlayer)
                targetPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController component))
        {
            isPlayerTriggered = false;
        }
    }

    private void Update()
    {
        if (isPlayerTriggered)
        {
            Vector3 directionToPlayer =
                (targetPlayer.transform.position - transform.position).normalized;

            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle <= viewAngle * 0.5f)
            {
                blackboard.SetVariableValue(CanSeePlayer, true);
                detectedPlayer = targetPlayer;
            }
            else
            {
                detectedPlayer = null;
            }

            if (detectedPlayer)
                blackboard.SetVariableValue(PLAYER_TARGET, detectedPlayer.transform);
        }
    }
}
using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    [Header("Activation")]
    public Transform player;
    public float activationRange = 3f;

    EnemyParryController parryController;
    bool combatStarted;

    void Awake()
    {
        parryController = GetComponent<EnemyParryController>();
    }

    void Update()
    {
        if (player == null || parryController == null || combatStarted)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activationRange)
        {
            combatStarted = true;

            Debug.Log("[COMBAT] EnemyActivation → Player entered combat range");

            CombatManager.Instance.StartCombat(parryController);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}

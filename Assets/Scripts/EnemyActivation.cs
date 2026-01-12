using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    [Header("Activation")]
    public Transform player;
    public float activationRange = 3f;

    EnemyParryController parryController;
    bool isActive;

    void Awake()
    {
        parryController = GetComponent<EnemyParryController>();
    }

    void Update()
    {
        if (player == null || parryController == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activationRange && !isActive)
        {
            isActive = true;
            parryController.Activate(player); // ✅ PASS PLAYER
        }
        else if (distance > activationRange && isActive)
        {
            isActive = false;
            parryController.Deactivate();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]

    EnemyHealthBarUI healthBarUI;


    public int maxHealth = 100;
    public int currentHealth;

    void Awake()
    {
        healthBarUI = FindAnyObjectByType<EnemyHealthBarUI>();

        currentHealth = maxHealth;
    }

   public void TakeDamage(int damage)
{
    currentHealth -= damage;
    currentHealth = Mathf.Max(0, currentHealth);

    Debug.Log($"[ENEMY] Took {damage} damage → HP {currentHealth}/{maxHealth}");

    healthBarUI?.ShowDamage(damage);

    if (currentHealth <= 0)
    {
        Die();
    }
}


    void Die()
    {
        Debug.Log("Enemy Died");

        // End combat cleanly
        CombatManager.Instance?.EndCombat(playerDied: false);

        Destroy(gameObject);
    }
}

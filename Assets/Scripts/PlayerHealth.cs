using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    // UI
    [Header("UI")]
    public PlayerHealthUI healthUI;

    // Damage Text
    [Header("Damage Text")]
    public PlayerDamageText damageText;

    [Header("Death Teleport")]
    public Transform resetPoint;

    [Header("Hit Visual")]
    public Renderer playerRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    Color originalColor;

    // Read-only access
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;

        healthUI?.UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        bool isDeathHit = currentHealth <= 0;

        // 🔥 SHOW DAMAGE NUMBER (different color if death hit)
        damageText?.ShowDamage(damage, isDeathHit);

        StartCoroutine(HitFlash());

        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");

        healthUI?.UpdateHealthUI();

        if (isDeathHit)
        {
            HandleDeath();
        }
    }

    IEnumerator HitFlash()
    {
        if (playerRenderer == null)
            yield break;

        playerRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        playerRenderer.material.color = originalColor;
    }

    void HandleDeath()
    {
        Debug.Log("Player died → teleport");

        if (resetPoint != null)
            transform.position = resetPoint.position;

        currentHealth = maxHealth;

        healthUI?.UpdateHealthUI();
    }
}

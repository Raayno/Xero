using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    // ✅ UI reference
    [Header("UI")]
    public PlayerHealthUI healthUI;

    [Header("Death Teleport")]
    public Transform resetPoint;

    [Header("Hit Visual")]
    public Renderer playerRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    Color originalColor;

    // ✅ READ-ONLY access for UI
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;

        // update UI on start
        healthUI?.UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        StartCoroutine(HitFlash());

        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");

        // update UI on damage
        healthUI?.UpdateHealthUI();

        if (currentHealth <= 0)
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

        // update UI after respawn
        healthUI?.UpdateHealthUI();
    }
}

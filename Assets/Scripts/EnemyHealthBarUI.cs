using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;
    public TextMeshProUGUI damageText;

    [Header("Damage Text")]
    public float damageTextDuration = 0.8f;
    public Vector2 damageTextOffset = new Vector2(0f, 40f);

    EnemyHealth currentEnemyHealth;
    Coroutine damageRoutine;

    void Awake()
    {
        // Start hidden (combat-only UI)
        gameObject.SetActive(false);

        if (damageText != null)
            damageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentEnemyHealth == null)
            return;

        float percent =
            (float)currentEnemyHealth.currentHealth /
            currentEnemyHealth.maxHealth;

        fillImage.fillAmount = percent;
    }

    // ================= COMBAT MANAGER CALLS =================

    public void Show(EnemyHealth enemyHealth)
    {
        currentEnemyHealth = enemyHealth;
        gameObject.SetActive(true);
        Update();
    }

    public void Hide()
    {
        currentEnemyHealth = null;

        // Stop any running damage animation
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        if (damageText != null)
            damageText.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    // ================= DAMAGE NUMBER =================

    public void ShowDamage(int damage)
    {
        if (damageText == null)
            return;

        // 🔑 CRITICAL FIX:
        // Ensure parent UI is active so text can render
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageRoutine(damage));
    }

    IEnumerator DamageRoutine(int damage)
    {
        damageText.gameObject.SetActive(true);
        damageText.text = "-" + damage.ToString();

        RectTransform rect = damageText.rectTransform;
        rect.anchoredPosition = damageTextOffset;

        yield return new WaitForSeconds(damageTextDuration);

        damageText.gameObject.SetActive(false);
        damageRoutine = null;
    }
}

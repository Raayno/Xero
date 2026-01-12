using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerDamageText : MonoBehaviour
{
    [Header("Text Reference")]
    public TextMeshProUGUI damageText;

    [Header("Display Mode")]
    [Tooltip("If ON, number will be auto-generated from damage value")]
    public bool autoNumber = true;

    [Tooltip("Used only when Auto Number is OFF")]
    public string customText = "HIT";

    [Header("Timing")]
    public float showDuration = 0.8f;
    public float floatSpeed = 0.6f;

    [Header("Colors")]
    public Color normalDamageColor = Color.red;
    public Color deathDamageColor = Color.yellow;

    Vector3 startLocalPos;

    void Awake()
    {
        startLocalPos = damageText.rectTransform.localPosition;
        damageText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call from PlayerHealth
    /// </summary>
    public void ShowDamage(int damage, bool isDeathHit)
    {
        StopAllCoroutines();

        damageText.gameObject.SetActive(true);

        // TEXT MODE
        if (autoNumber)
            damageText.text = "-" + damage.ToString();
        else
            damageText.text = customText;

        // COLOR MODE
        damageText.color = isDeathHit ? deathDamageColor : normalDamageColor;

        // Reset transform
        damageText.rectTransform.localPosition = startLocalPos;
        damageText.rectTransform.localScale = Vector3.one * 1.2f;

        StartCoroutine(DamageRoutine());
    }

    IEnumerator DamageRoutine()
    {
        float t = 0f;

        while (t < showDuration)
        {
            t += Time.deltaTime;

            // Float upward
            damageText.rectTransform.localPosition +=
                Vector3.up * floatSpeed * Time.deltaTime;

            // Smooth scale back
            damageText.rectTransform.localScale =
                Vector3.Lerp(
                    damageText.rectTransform.localScale,
                    Vector3.one,
                    Time.deltaTime * 10f
                );

            yield return null;
        }

        damageText.gameObject.SetActive(false);
    }
}

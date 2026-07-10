using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class IconCooldownController : MonoBehaviour
{
    [SerializeField] private Image filledCooldownOverlay;
    [SerializeField] private AnimationCurve cooldownCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private MMF_Player cooldownFeedback;
    [NaughtyAttributes.Button("Test Cooldown")] private void TestCooldownButton() => StartCooldown(3f); // Test with a 3-second cooldown
    private float cashedDuration = 0f;
    private float elapsedTime = 0f;

    public void StartCooldown(float duration, bool isOverride = false)
    {
        if (filledCooldownOverlay == null)
        {
            Debug.LogWarning("Filled cooldown overlay is not assigned.");
            return;
        }

        if (cooldownCoroutine != null)
        {
            if (!isOverride)
            {
                Debug.LogWarning("Cooldown is already running. Use isOverride = true to restart.");
                return;
            }
            StopCoroutine(cooldownCoroutine);
        }
        cashedDuration = duration;
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(duration));
    }

    public void SkipCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
        filledCooldownOverlay.fillAmount = 0f; // Reset the overlay to empty
        CooldownComplete(); // Call the cooldown complete method
    }

    public void PauseCooldown(bool isUnfreeze = false)
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
    }

    public void ResumeCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(cashedDuration, true));
    }

    private Coroutine cooldownCoroutine;
    private System.Collections.IEnumerator CooldownCoroutine(float duration, bool isResume = false)
    {
        if (!isResume) elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float fillAmount = cooldownCurve.Evaluate(normalizedTime);
            filledCooldownOverlay.fillAmount = fillAmount;
            yield return null;
        }

        filledCooldownOverlay.fillAmount = 0f; // Ensure the overlay is fully empty at the end
        CooldownComplete();
        cooldownCoroutine = null; // Reset the coroutine reference
    }

    protected virtual void CooldownComplete()
    {
        if (cooldownFeedback != null)
        {
            cooldownFeedback.PlayFeedbacks();
        }
    }

    private void OnValidate()
    {
        if (filledCooldownOverlay == null)
        {
            Image[] images = GetComponentsInChildren<Image>();
            var thisImage = GetComponent<Image>();
            if (images.Length > 0)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != thisImage && images[i].type == Image.Type.Filled)
                    {
                        filledCooldownOverlay = images[i];
                        break;
                    }
                }

                if (filledCooldownOverlay == null)
                {
                    Debug.LogWarning("No filled Image component found in children to assign as filledCooldownOverlay.");
                }
            }
            else
            {
                Debug.LogWarning("No Image components found in children to assign as filledCooldownOverlay.");
            }
        }
    }
}

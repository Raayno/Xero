using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region DATA
[System.Serializable]
public class ParryStep
{
    public float windupTime = 0.5f;
    public float parryDuration = 0.25f;
    public float afterDelay = 0.15f;
}
#endregion

public class EnemyParryController : MonoBehaviour
{
    // 🔔 TURN EVENTS
    public System.Action OnEnemyTurnStart;
    public System.Action OnEnemyTurnEnd;

    [Header("Parry Pattern (Step Based)")]
    public List<ParryStep> steps = new List<ParryStep>();

    [Header("Damage")]
    public int enemyDamage = 20;

    // ================= STATE =================
    public bool attackActive { get; private set; }

    int currentStepIndex;
    bool parrySolvedThisStep;
    bool isActive;

    // ================= REFERENCES =================
    PlayerHealth playerHealth;
    PlayerController playerController;
    EnemyVisuals visuals;
    CamraFollow cameraFollow;

    Coroutine parryRoutine;

    void Awake()
    {
        visuals = GetComponent<EnemyVisuals>();
        cameraFollow = FindObjectOfType<CamraFollow>();
    }

    // ================= ACTIVATION =================
    public void Activate(Transform player)
    {
        if (isActive) return;
        isActive = true;

        OnEnemyTurnStart?.Invoke(); // 🔔 ENEMY TURN START

        playerHealth = player.GetComponent<PlayerHealth>();
        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
            playerController.currentEnemy = this;

        StopAllCoroutines();
        parryRoutine = StartCoroutine(ParrySequence());
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        StopAllCoroutines();
        attackActive = false;

        visuals?.SetIdle();
        cameraFollow?.ZoomOut();

        if (playerController != null && playerController.currentEnemy == this)
            playerController.currentEnemy = null;
    }

    // ================= MAIN LOGIC =================
    IEnumerator ParrySequence()
    {
        currentStepIndex = 0;

        while (currentStepIndex < steps.Count)
        {
            ParryStep step = steps[currentStepIndex];
            parrySolvedThisStep = false;

            // 🟡 WIND-UP
            attackActive = false;
            visuals?.SetWindup();
            cameraFollow?.ZoomIn();
            yield return new WaitForSeconds(step.windupTime);

            // 🟣 PARRY WINDOW
            attackActive = true;
            visuals?.SetParryWindow();

            float timer = 0f;
            while (timer < step.parryDuration)
            {
                if (parrySolvedThisStep)
                    break;

                timer += Time.deltaTime;
                yield return null;
            }

            attackActive = false;
            cameraFollow?.ZoomOut();

            // ================= RESOLVE =================
            if (!parrySolvedThisStep)
            {
                playerHealth?.TakeDamage(enemyDamage);
                visuals?.SetIdle();
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                visuals?.SetParrySuccess();
                yield return new WaitForSeconds(step.afterDelay);
            }

            currentStepIndex++;
        }

        // ================= TURN END =================
        visuals?.SetIdle();

        OnEnemyTurnEnd?.Invoke(); // 🔔 ENEMY TURN END
        Deactivate();
    }

    // ================= PLAYER CALLBACK =================
    public void OnParried()
    {
        if (!attackActive)
            return;

        parrySolvedThisStep = true;
    }
}

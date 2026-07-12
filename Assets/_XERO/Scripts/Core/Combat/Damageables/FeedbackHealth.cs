using MoreMountains.Feedbacks;
using UnityEngine;

public class FeedbackHealth : Health
{
    [SerializeField] protected MMF_Player damageFeedback;
    [SerializeField] protected MMF_Player healFeedback;
    [SerializeField] protected MMF_Player deathFeedback;
    [SerializeField] protected bool isDeathFeedbackStopDamageFeedback = true;
    [SerializeField] protected float damageCooldown = 0.25f;

    protected float lastDamageTakenAt = int.MinValue;

    public override void TakeDamage(DamageDataSO damageDataSO)
    {
        if (damageDataSO.DamageAmount == 0)
        {
            Debug.LogWarning("<color=red>[FeedbackHealth]</color> Damage amount is 0, no feedback will be played.");
            base.TakeDamage(damageDataSO);
            return;
        }
        
        try
        {
            if (damageFeedback != null)
            {
                if (Time.time - lastDamageTakenAt >= damageCooldown)
                {
                    lastDamageTakenAt = Time.time;
                    damageFeedback.PlayFeedbacks(transform.position, damageDataSO.DamageAmount);
                }
            }
            else
            {
                Debug.LogError("<color=red>[FeedbackHealth]</color> BarbarianEnemy reference is not assigned.");
            }

            if (healFeedback != null)
            {
                if (damageDataSO.DamageAmount < 0)
                {
                    healFeedback.PlayFeedbacks(transform.position, damageDataSO.DamageAmount);
                }
            }
            else
            {
                Debug.LogWarning("<color=red>[FeedbackHealth]</color> BarbarianEnemy reference is not assigned.");
            }
        }
        finally
        {
            base.TakeDamage(damageDataSO);
        }
    }

    public override void Kill()
    {
        try
        {
            if (deathFeedback != null)
            {
                if (isDeathFeedbackStopDamageFeedback && damageFeedback != null)
                {
                    damageFeedback.StopFeedbacks();
                }
                deathFeedback.PlayFeedbacks(transform.position);
            }
            else
            {
                Debug.LogWarning("<color=red>[FeedbackHealth]</color> BarbarianEnemy reference is not assigned.");
            }
        }
        finally
        {
            base.Kill();
        }
    }
}
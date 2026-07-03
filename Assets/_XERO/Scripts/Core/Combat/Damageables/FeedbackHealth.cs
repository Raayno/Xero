using MoreMountains.Feedbacks;
using UnityEngine;

public class FeedbackHealth : Health
{
    [SerializeField] protected MMFeedbacks damageFeedback;
    [SerializeField] protected float damageCooldown = 0.25f;

    protected float lastDamageTakenAt = int.MinValue;

    public override void TakeDamage(DamageDataSO damageDataSO)
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

        base.TakeDamage(damageDataSO);
    }
}
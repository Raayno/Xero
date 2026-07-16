using System;
using UnityEngine;

public abstract class CombatDamageable : MonoBehaviour
{
    [SerializeField] private Participant participant;
    /// <summary>
    /// Negative dmg should heal the Damageable
    /// </summary>
    public virtual void TakeDamage(DamageDataSO dmg)
    {
        PlayDamageFeedback(dmg.DamageAmount);
        Debug.LogWarning($"<color=orange>[CombatDamageable]</color> TakeDamage method not implemented in {GetType().Name}. Please override this method in a derived class.");
    }
    protected void PlayDamageFeedback(float damageAmount)
    {
        if (damageAmount > 0)
            participant.Feedbacks.PlayFeedback(participant is PlayerParticipant ? FeedbackType.PlayerOnDamage : FeedbackType.EnemyOnDamage, transform.position, damageAmount);
        else if (damageAmount < 0)
            participant.Feedbacks.PlayFeedback(participant is PlayerParticipant ? FeedbackType.PlayerOnHeal : FeedbackType.EnemyOnHeal, transform.position, damageAmount);
    }
    public event Action<Participant> OnDefeated;
    public event Action<Participant> OnResurrected;
    public bool IsDefeated { get; protected set; }

    public virtual void Resurrect(int healthAmount = 1)
    {
        IsDefeated = false;
        OnResurrected?.Invoke(participant);
        TakeDamage(new(damageAmount: -healthAmount));
    }

    public virtual void Kill()
    {
        participant.Feedbacks.PlayFeedback(participant is PlayerParticipant ? FeedbackType.PlayerOnDeath : FeedbackType.EnemyOnDeath, transform.position);
        if (!IsDefeated) 
        {
            SetDamageableStatsToDead();
            OnDefeated?.Invoke(participant);
        }
        IsDefeated = true;
    }

    protected abstract void SetDamageableStatsToDead();

    protected virtual void Reset()
    {
#pragma warning disable UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
        participant ??= GetComponent<Participant>();
#pragma warning restore UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
    }
}
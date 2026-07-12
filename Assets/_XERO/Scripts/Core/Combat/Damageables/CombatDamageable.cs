using System;
using UnityEngine;

public abstract class CombatDamageable : MonoBehaviour
{
    [SerializeField] private Participant participant;
    /// <summary>
    /// Negative dmg should heal the Damageable
    /// </summary>
    public abstract void TakeDamage(DamageDataSO dmg);
    public event Action<Participant> OnDefeated;
    public bool IsDefeated { get; protected set; }

    public virtual void Resurrect(int healthAmount = 1)
    {
        IsDefeated = false;
        TakeDamage(new(damageAmount: -healthAmount));
    }

    public virtual void Kill()
    {
        if (!IsDefeated) 
        {
            TakeDeathDamage();
            OnDefeated?.Invoke(participant);
        }
        IsDefeated = true;
    }

    protected abstract void TakeDeathDamage();

    protected virtual void Reset()
    {
#pragma warning disable UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
        participant ??= GetComponent<Participant>();
#pragma warning restore UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
    }
}
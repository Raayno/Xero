using NaughtyAttributes;
using System;
using UnityEngine;

public abstract class Damageable
{
    public event Action<Participant> Defeated;
    [Required][SerializeField] private Participant participant;
    public abstract void TakeDamage(int damageAmount);
    public abstract void Heal(int healAmount);
    public bool IsDefeated { get; protected set; }

    public virtual void MarkAsDefeated()
    {
        if (IsDefeated)
        {
            return;
        }

        IsDefeated = true;

        Defeated?.Invoke(participant);
    }

    public virtual void Reset()
    {
        IsDefeated = false;
    }
}
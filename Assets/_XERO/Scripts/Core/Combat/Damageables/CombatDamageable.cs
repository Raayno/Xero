using System;
using UnityEngine;

public abstract class CombatDamageable : Damageable
{
    public event Action<Participant> OnDefeated;
    [SerializeField] private Participant participant;

    public override void Kill()
    {
        if (!IsDefeated) 
        {
            TakeDeathDamage();
            OnDefeated?.Invoke(participant);
        }
        base.Kill();
    }

    protected abstract void TakeDeathDamage();

    protected virtual void Reset()
    {
        if (participant == null)
        {
#pragma warning disable UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
            participant = GetComponent<Participant>();
#pragma warning restore UNT0039 // Use RequireComponent attribute when self-invoking GetComponent
        }
    }
}
using System;
using UnityEngine;

[RequireComponent(typeof(Participant))]
public abstract class CombatDamageable : Damageable
{
    public event Action<Participant> OnDefeated;
    [SerializeField] private Participant participant;

    protected virtual void Reset()
    {
        if (participant == null)
        {
            participant = GetComponent<Participant>();
        }
    }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
    public virtual void Kill()
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
    {
        if (!IsDefeated) 
        {
            TakeDeathDamage();
            OnDefeated?.Invoke(participant);
        }
        base.Kill();
    }

    protected abstract void TakeDeathDamage();
}
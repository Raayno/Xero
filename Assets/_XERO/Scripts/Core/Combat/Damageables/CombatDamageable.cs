using System;
using UnityEngine;

[RequireComponent(typeof(Participant))]
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
            participant = GetComponent<Participant>();
        }
    }
}
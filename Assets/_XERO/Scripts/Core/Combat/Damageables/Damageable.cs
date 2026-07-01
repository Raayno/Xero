using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    /// <summary>
    /// Negative dmg should heal the Damageable
    /// </summary>
    public abstract void TakeDamage(DamageDataSO dmg);
    public bool IsDefeated { get; protected set; }

    public virtual void Resurrect(int healthAmount = 1)
    {
        IsDefeated = false;
        TakeDamage(new(damageAmount: -healthAmount));
    }

    public virtual void Kill()
    {
        IsDefeated = true;
    }
}
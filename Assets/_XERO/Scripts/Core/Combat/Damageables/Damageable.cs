using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    public abstract void TakeDamage(int damageAmount);
    public abstract void Heal(int healAmount);
    public bool IsDefeated { get; protected set; }

    public virtual void Resurrect(int healthAmount = 1)
    {
        IsDefeated = false;
        Heal(healthAmount);
    }

    public virtual void Kill()
    {
        IsDefeated = true;
    }
}
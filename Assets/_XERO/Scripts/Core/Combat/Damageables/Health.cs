using System;
using UnityEngine;

public class Health : CombatDamageable
{

    [SerializeField] [Range(0,999999999)] private int maxHP = 1000;
    [SerializeField] [Range(0,999999999)] private int currentHP;

    public int MaxHealth => maxHP;
    public int CurrentHealth => currentHP;

    public override void TakeDamage(DamageDataSO dmg)
    {
        Debug.Log($"<color=red>[Health]</color> {gameObject.name} took {dmg.DamageAmount} damage.");
        currentHP -= Mathf.Clamp(Mathf.RoundToInt(dmg.DamageAmount), 0, maxHP);
        if (currentHP <= 0)
        {
            currentHP = 0;
            Kill();
        }
        else
        {
            PlayDamageFeedback(dmg.DamageAmount);
        }
    }

    protected override void SetDamageableStatsToDead() => currentHP = 0;

    protected override void Reset()
    {
        base.Reset();
        currentHP = maxHP;
    }
}
using System;
using UnityEngine;

public class Health : CombatDamageable
{

    [SerializeField] [Range(0,999999999)] private int maxHP = 1000;
    [SerializeField] [Range(0,999999999)] private int currentHP;

    public int MaxHealth => maxHP;
    public int CurrentHealth => currentHP;

    protected override void Reset()
    {
        base.Reset();
        currentHP = maxHP;
    }

    public override void TakeDamage(int damageAmount)
    {
        currentHP -= damageAmount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Kill();
        }
    }

    public override void Heal(int healAmount)
    {
        currentHP = Mathf.Clamp(healAmount, 0, maxHP);
    }

    protected override void TakeDeathDamage()
    {
        currentHP = 0;
    }
}
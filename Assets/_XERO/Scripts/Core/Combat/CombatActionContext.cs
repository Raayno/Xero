using System.Collections.Generic;

public class CombatActionContext
{
    public CombatTarget Attacker { get; }
    public AttackDataSO AttackData { get; }
    public IReadOnlyList<CombatTarget> Receivers => receivers;

    private readonly List<CombatTarget> receivers;

    public CombatActionContext(
        CombatTarget attacker,
        AttackDataSO attackData,
        List<CombatTarget> receivers)
    {
        Attacker = attacker;
        AttackData = attackData;
        this.receivers = receivers != null
            ? new List<CombatTarget>(receivers)
            : new List<CombatTarget>();
    }

    public bool IsValid()
    {
        if (Attacker == null)
        {
            return false;
        }

        if (AttackData == null)
        {
            return false;
        }

        if (Attacker.IsDefeated)
        {
            return false;
        }

        if (receivers == null || receivers.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < receivers.Count; i++)
        {
            CombatTarget receiver = receivers[i];

            if (receiver == null)
            {
                return false;
            }

            if (receiver.IsDefeated)
            {
                return false;
            }
        }

        return true;
    }

    public CombatTarget GetFirstReceiver()
    {
        if (receivers == null || receivers.Count == 0)
        {
            return null;
        }

        return receivers[0];
    }
}
using System.Collections.Generic;

public class CombatActionContext
{
    public Participant Attacker { get; }
    public AttackDataSO AttackData { get; }
    public IReadOnlyList<Participant> Receivers => receivers;

    private readonly List<Participant> receivers;

    public CombatActionContext(
        Participant attacker,
        AttackDataSO attackData,
        List<Participant> receivers)
    {
        Attacker = attacker;
        AttackData = attackData;
        this.receivers = receivers != null
            ? new List<Participant>(receivers)
            : new List<Participant>();
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
            Participant receiver = receivers[i];

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

    public Participant GetFirstReceiver()
    {
        if (receivers == null || receivers.Count == 0)
        {
            return null;
        }

        return receivers[0];
    }
}
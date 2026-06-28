using System.Collections.Generic;

public class CombatActionContext
{
    public Participant Attacker { get; }
    public AttackDataSO AttackData { get; }
    public IReadOnlyList<Participant> Targets => targets;

    private readonly List<Participant> targets;

    public CombatActionContext(
        Participant attacker,
        AttackDataSO attackData,
        List<Participant> receivers)
    {
        Attacker = attacker;
        AttackData = attackData;
        targets = receivers != null
            ? new List<Participant>(receivers)
            : new List<Participant>();
    }

    public Participant GetFirstTarget()
    {
        if (targets == null || targets.Count == 0)
        {
            return null;
        }

        return targets[0];
    }
}
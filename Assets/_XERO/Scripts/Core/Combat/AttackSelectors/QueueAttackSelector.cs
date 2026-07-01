using System.Collections.Generic;

public class QueueAttackSelector : AttackSelector
{
    protected override AttackDataSO SelectAttack(List<AttackDataSO> attacks)
    {
        var a = attacks[0];
        attacks.RemoveAt(0); // Remove the selected attack from the list to simulate a queue
        attacks.Add(a); // Add the selected attack back to the end of the list to maintain the queue order
        return a;
    }
}
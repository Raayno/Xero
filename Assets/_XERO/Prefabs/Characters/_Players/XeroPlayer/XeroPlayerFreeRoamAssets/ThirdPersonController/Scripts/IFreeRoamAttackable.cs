public interface IFreeRoamAttackable
{
    /// <returns>False if should NOT block other things in range from being attacked by this attack</returns>
    bool OnAttack();
}
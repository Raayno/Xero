using Unity.Behavior;

[BlackboardEnum]
//  ------------IMPORTANT NOTE------------
//  ADD NEW VALUES TO END OF ENUM to avoid breaking existing serialized data. DO NOT CHANGE ORDER of existing values.
public enum FeedbackType
{
    None,
    EnemyOnAttack,
    EnemyOnDamage,
    EnemyOnHeal,
    EnemyOnDeath,
    PlayerOnAttack,
    PlayerOnParry,
    PlayerOnDamage,
    PlayerOnHeal,
    PlayerOnDeath,
    FreeRoamEnemyOnDashAnticipation,
}
//  ------------IMPORTANT NOTE------------
//  ADD NEW VALUES TO END OF ENUM to avoid breaking existing serialized data. DO NOT CHANGE ORDER of existing values.

using UnityEngine;

public class EnemyActionSelector : MonoBehaviour
{
    public AttackDataSO SelectAttack(EnemyCombatTarget enemyCombatTarget)
    {
        if (enemyCombatTarget == null)
        {
            Debug.LogError("[EnemyActionSelector] Enemy combat target is null.");
            return null;
        }

        EnemyCombatTargetData enemyData = enemyCombatTarget.GetData();

        if (enemyData == null)
        {
            Debug.LogError(
                $"[EnemyActionSelector] Enemy {enemyCombatTarget.CombatantName} has no combat target data assigned.");

            return null;
        }

        if (enemyData.attacks == null || enemyData.attacks.Count == 0)
        {
            Debug.LogError(
                $"[EnemyActionSelector] Enemy {enemyCombatTarget.CombatantName} has no attacks assigned.");

            return null;
        }

        AttackDataSO selectedAttack = enemyData.attacks[Random.Range(0, enemyData.attacks.Count)];

        if (selectedAttack == null)
        {
            Debug.LogError(
                $"[EnemyActionSelector] Enemy {enemyCombatTarget.CombatantName} selected a null attack.");

            return null;
        }

        return selectedAttack;
    }
}

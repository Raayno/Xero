using UnityEngine;

public enum PlayerAttackType
{
    Light,
    Heavy,
    Quick,
    Break,
    Special
}

public class PlayerCombatActions : MonoBehaviour
{
    [Header("Damage Values")]
    public int lightDamage = 10;
    public int heavyDamage = 20;
    public int quickDamage = 8;
    public int breakDamage = 15;
    public int specialDamage = 25;

    void Update()
    {
        if (!CombatManager.Instance.IsPlayerTurn())
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            PerformAttack(PlayerAttackType.Light);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            PerformAttack(PlayerAttackType.Heavy);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            PerformAttack(PlayerAttackType.Quick);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            PerformAttack(PlayerAttackType.Break);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            PerformAttack(PlayerAttackType.Special);
    }

    public void PerformAttack(PlayerAttackType type)
    {
        if (!CombatManager.Instance.IsPlayerTurn())
        {
            Debug.LogWarning("[PLAYER] Attack attempted outside player turn");
            return;
        }

        EnemyParryController enemy = CombatManager.Instance.CurrentEnemy;

        if (enemy == null)
        {
            Debug.LogWarning("[PLAYER] No active enemy in CombatManager");
            return;
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogWarning("[PLAYER] Active enemy has no EnemyHealth component");
            return;
        }

        int damage = GetDamage(type);
        enemyHealth.TakeDamage(damage);

        Debug.Log($"[PLAYER] Attack {type} → {damage} damage");

        CombatManager.Instance.EndPlayerTurn();
    }

    int GetDamage(PlayerAttackType type)
    {
        switch (type)
        {
            case PlayerAttackType.Light: return lightDamage;
            case PlayerAttackType.Heavy: return heavyDamage;
            case PlayerAttackType.Quick: return quickDamage;
            case PlayerAttackType.Break: return breakDamage;
            case PlayerAttackType.Special: return specialDamage;
        }
        return 0;
    }
}

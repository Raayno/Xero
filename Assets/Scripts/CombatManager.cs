using UnityEngine;

public enum CombatTurn
{
    None,
    Player,
    Enemy
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;



    [Header("References")]
    public PlayerController playerController;
    public PlayerHealth playerHealth;

[Header("UI")]
public EnemyHealthBarUI enemyHealthBarUI;


    CombatTurn currentTurn;
    EnemyParryController currentEnemy;

    // 🔑 Read-only access for player attacks
    public EnemyParryController CurrentEnemy => currentEnemy;

    Vector3 playerReturnPoint;

    void Awake()
    {
        Instance = this;
        Debug.Log("[COMBAT] CombatManager Initialized");
    }

    // ================= COMBAT ENTRY =================
    public void StartCombat(EnemyParryController enemy)
    {
        if (currentTurn != CombatTurn.None)
        {
            Debug.LogWarning("[COMBAT] Attempted to start combat while already in combat");
            return;
        }

        currentEnemy = enemy;

        EnemyHealth enemyHealth =
    currentEnemy.GetComponent<EnemyHealth>();

enemyHealthBarUI?.Show(enemyHealth);

        currentTurn = CombatTurn.Player;

        playerReturnPoint = playerController.transform.position;
        playerController.allowMovementInCombat = false;

        Debug.Log("[COMBAT] Combat Started");
        Debug.Log("[TURN] Player Turn Started");
    }

    // ================= PLAYER TURN =================
    public bool IsPlayerTurn()
    {
        return currentTurn == CombatTurn.Player;
    }

    public void EndPlayerTurn()
    {
        if (currentEnemy == null)
        {
            Debug.LogWarning("[TURN] EndPlayerTurn called but no enemy exists");
            return;
        }

        Debug.Log("[TURN] Player Turn Ended");

        currentTurn = CombatTurn.Enemy;

        currentEnemy.OnEnemyTurnEnd += OnEnemyTurnFinished;

        Debug.Log("[TURN] Enemy Turn Started");

        currentEnemy.Activate(playerController.transform);
    }

    // ================= ENEMY TURN =================
   void OnEnemyTurnFinished()
{
    // Combat may have already ended
    if (currentEnemy == null)
    {
        Debug.Log("[TURN] Enemy turn finished AFTER combat ended (ignored)");
        return;
    }

    currentEnemy.OnEnemyTurnEnd -= OnEnemyTurnFinished;

    Debug.Log("[TURN] Enemy Turn Ended");

    if (playerHealth.CurrentHealth <= 0)
    {
        Debug.Log("[COMBAT] Player died during enemy turn");
        EndCombat(true);
        return;
    }

    currentTurn = CombatTurn.Player;
    Debug.Log("[TURN] Player Turn Started");
}


    // ================= COMBAT EXIT =================
   public void EndCombat(bool playerDied)
{
    if (currentTurn == CombatTurn.None)
        return;

    Debug.Log("[COMBAT] Combat Ended");

    // Unsubscribe safely
    
    if (currentEnemy != null)
        currentEnemy.OnEnemyTurnEnd -= OnEnemyTurnFinished;

    playerController.allowMovementInCombat = true;

    if (playerDied)
    {
        Debug.Log("[COMBAT] Player reset to start point");
        playerController.transform.position = playerReturnPoint;
    }
enemyHealthBarUI?.Hide();

    currentEnemy = null;
    currentTurn = CombatTurn.None;
}

}

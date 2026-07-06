using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using MoreMountains.Tools;

public class CombatController : MMSingleton<CombatController>
{

    #region Participants
    [Header("Participants")]
    private readonly List<EnemyParticipant> enemyParticipants = new();
    private readonly List<PlayerParticipant> playerParticipants = new();
    
    public List<Participant> GetEnemies() => new(enemyParticipants);
    public List<Participant> GetPlayers() => new(playerParticipants);
    #endregion

    [Header("Turn Management")]
    [SerializeField] private TurnSelector turnSelector;

    [Header("Input Management")]

    [Header("Combat Initialization")]
    [SerializeField] private CombatInitialization combatInitialization;
    [Button("Add Current Positioning of Participants")] private void AddCurrentPositioningOfParticipants() => combatInitialization?.CombatPositioning.AddCurrentPositioningOfParticipants(GetPlayers(), GetEnemies());
    
    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    private CancellationTokenSource cancellationTokenSource;

    protected override void Awake()
    {
        Reset();
    }

    private void Start()
    {
        InitializeCombat();
    }

    #region Initialization
    [Button("Reset Combat")]
    private void InitializeCombat()
    {
        CleanseCombat();

        combatInitialization.InitializeCombat(enemyParticipants, playerParticipants);

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        RunCombatLoopAsync(cancellationTokenSource.Token).Forget();
    }


    [Button("Cleanse Combat")]
    private void CleanseCombat()
    {
        // Cancel any ongoing combat loop
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        // Reset the turn selector
        if (turnSelector != null) turnSelector.ResetTimeline();

        // Clear all subscriptions to timeline signals (game-wide)
        TimelineSignalBridge.UnsubscribeAll();

        // Destroy all existing participants
        foreach (var player in playerParticipants)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        playerParticipants.Clear();
        foreach (var enemy in enemyParticipants)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        enemyParticipants.Clear();
    }
    #endregion

    #region Combat Loop
    private async UniTask RunCombatLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!VerifyParticipants())
            {
                break;
            }

            turnSelector.NextTurn(playerParticipants, enemyParticipants);
            var currentParticipant = turnSelector.GetCurrentParticipant();
            if (currentParticipant == null)
            {
                Debug.LogError("[CombatController] Current participant is null.");
                break;
            }

            if (enableDebug)
            {
                Debug.Log($"<color=#55AAFF>[Combat]</color> Current turn: {turnSelector.GetCurrentParticipant().CombatantName}");
                string timeline = "Timeline: ";
                foreach (var participant in turnSelector.TurnTimeline)
                {
                    timeline += participant.CombatantName + " -> ";
                }
                Debug.Log($"<color=#55AAFF>[Combat]</color> {timeline}");
            }

            if (currentParticipant.turnExec == null)
            {
                Debug.LogError($"[CombatController] {currentParticipant.CombatantName} has no turn participant assigned.");
                break;
            }
            
            // Create a linked CancellationTokenSource for this turn so the participant's
            // destroy token and the global combat token are both observed.
            // *without overwriting the global token, so that partcipant's destroy token is observed during his turn execution.
            using (var turnCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, currentParticipant.GetCancellationTokenOnDestroy()))
            {
                await currentParticipant.turnExec.ExecuteTurn(currentParticipant, turnCancellationTokenSource.Token);
            }
            Debug.Log($"<color=#55AAFF>[Combat]</color> {currentParticipant.CombatantName} completed their turn.");
        }
    }

    bool VerifyParticipants()
    {
        if (playerParticipants == null || playerParticipants.Count == 0)
        {
            Debug.LogError("[CombatController] Player participants are not assigned or empty.");
            return false;
        }

        if (enemyParticipants == null || enemyParticipants.Count == 0)
        {
            Debug.LogError("[CombatController] Enemy participants are not assigned or empty.");
            return false;
        }

        return true;
    }
    #endregion

    void OnDestroy()
    {
        CleanseCombat();
    }
    
    void Reset()
    {
        combatInitialization.PlayersTransform = GetChild(transform, "Players", combatInitialization.PlayersTransform);
        combatInitialization.EnemiesTransform = GetChild(transform, "Enemies", combatInitialization.EnemiesTransform);

        static Transform GetChild(Transform parent, string name, Transform child)
        {
            if (child != null) return child;

            child = parent.Find(name);

            if (child == null)
            {
                Debug.LogWarning($"[CombatController] Child '{name}' not found under '{parent.name}'. Creating a new GameObject.");
                // faster and cleaner than using GameObject.Instantiate with a prefab, since we just need an empty GameObject to hold the participants
                child = new GameObject(name).transform;
                child.SetParent(parent);
            }
            return child;
        }
    }
}

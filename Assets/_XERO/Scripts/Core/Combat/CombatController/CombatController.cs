using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CombatController : MonoBehaviour
{

    #region Participants
    [Header("Participants")]
    [SerializeField] private List<EnemyParticipant> enemyParticipants = new();
    [SerializeField] private List<PlayerParticipant> playerParticipants = new();

    
    public List<Participant> GetEnemies() => new(enemyParticipants);
    public List<Participant> GetPlayers() => new(playerParticipants);
    #endregion

    [Header("Turn Management")]
    [SerializeField] private TurnSelector turnSelector;

    [Header("UI Management")]
    public ManualAttackSelectorUI CombatOptionsUIManager;

    [Header("Attack Management")]
    public TimelineSignalBridge timelineSignalBridge;

    [Header("Input Management")]
    [SerializeField] private ParticipantPointInput participantPointInput;
    public ParticipantPointInput ParticipantPointInput => participantPointInput;
    [SerializeField] private ParryInput parryInput;
    public ParryInput ParryInput => parryInput;

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;
    [SerializeField] private CombatInitializationData combatInitializationData;
    [Button("Reset Combat")] private void ResetCombat()
    {
        CleanseCombat();
        InitializeCombat(playerParticipants, enemyParticipants);
    }
    
    private void Start()
    {
        RunCombatLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    #region Initialization
    public void InitializeCombat(List<PlayerParticipant> players, List<EnemyParticipant> enemies)
    {
        
    }

    private void CleanseCombat()
    {
        
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
                Debug.LogError($"[CombatController] {currentParticipant.CombatantName} has no turn executor assigned.");
                break;
            }

            await currentParticipant.turnExec.ExecuteTurn(currentParticipant, cancellationToken);
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
    
    #region Singleton
    private static CombatController _instance;
    public static CombatController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CombatController>();
                if (_instance == null)
                {
                    Debug.LogWarning("[CombatController] No instance found in the scene, but it was requested. Creating a new instance.");
                    GameObject go = new("CombatController");
                    _instance = go.AddComponent<CombatController>();
                }
            }
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    void Reset()
    {
        timelineSignalBridge = GetComponentInChildren<TimelineSignalBridge>();
        participantPointInput = GetComponentInChildren<ParticipantPointInput>();
        parryInput = GetComponentInChildren<ParryInput>();
    }
}
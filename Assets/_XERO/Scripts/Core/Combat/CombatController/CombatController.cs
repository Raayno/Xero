using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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
    
    private void Start()
    {
        StartCoroutine(Combat());
    }

    private IEnumerator Combat()
    {
        if (!VerifyParticipants())
        {
            yield break;
        }

        turnSelector.NextTurn(playerParticipants, enemyParticipants);
        var currentParticipant = turnSelector.GetCurrentParticipant();
        if (currentParticipant == null)
        {
            Debug.LogError("[CombatController] Current participant is null.");
            yield break;
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
            yield break;
        }

        yield return currentParticipant.turnExec.ExecuteTurn(currentParticipant);
        Debug.Log($"<color=#55AAFF>[Combat]</color> {currentParticipant.CombatantName} completed their turn.");
    
        yield return Combat();
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
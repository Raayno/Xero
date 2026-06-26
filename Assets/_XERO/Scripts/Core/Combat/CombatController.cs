using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    #region Participants
    [Header("Participants")]
    [SerializeField] private List<EnemyParticipant> enemyParticipants = new();
    [SerializeField] private List<PlayerParticipant> playerParticipants = new();

    
    public List<EnemyParticipant> GetEnemies() => new(enemyParticipants);
    public List<PlayerParticipant> GetPlayers() => new(playerParticipants);
    #endregion

    [Header("Turn Management")]
    [SerializeField] private TurnSelector turnSelector;

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;
    
    private void Start()
    {
        StartCoroutine(Combat());
    }

    private IEnumerator Combat()
    {
        VerifyParticipants();
        turnSelector.NextTurn(playerParticipants, enemyParticipants);
        turnSelector.GetCurrentParticipant();
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
        
        yield return null;
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

    /*#region Unverified
    [SerializeField] private CombatActionResolver combatActionResolver;

    [Header("Player Turn")]
    [SerializeField] private CombatOptionsUIManager combatOptionsUIManager;
    [SerializeField] private CombatTargetSelectionManager combatTargetSelectionManager;
    [SerializeField] private CombatTargetProvider combatTargetProvider;

    [Header("Enemy Turn")]
    [SerializeField] private EnemyActionSelector enemyActionSelector;
    [SerializeField] private EnemyTargetSelector enemyTargetSelector;

    private Participant currentCombatTarget;
    private AttackDataSO pendingPlayerAttack;

    private void OnEnable()
    {
        CombatOptionsUIButton.OnOptionSelected += CombatOptionsUIButton_OnOptionSelected;

        if (combatTargetSelectionManager)
        {
            combatTargetSelectionManager.TargetSelected += CombatTargetSelectionManager_TargetSelected;
            combatTargetSelectionManager.TargetSelectionCancelled += CombatTargetSelectionManager_TargetSelectionCancelled;
        }

        if (combatActionResolver)
            combatActionResolver.ActionFinished += CombatActionResolver_ActionFinished;
    }

    private void OnDisable()
    {
        CombatOptionsUIButton.OnOptionSelected -= CombatOptionsUIButton_OnOptionSelected;

        if (combatTargetSelectionManager)
        {
            combatTargetSelectionManager.TargetSelected -= CombatTargetSelectionManager_TargetSelected;
            combatTargetSelectionManager.TargetSelectionCancelled -= CombatTargetSelectionManager_TargetSelectionCancelled;
        }

        if (combatActionResolver)
            combatActionResolver.ActionFinished -= CombatActionResolver_ActionFinished;
    }


    private void StartCombat()
    {
        ClearPendingPlayerAttack();

        currentCombatTarget = combatTimelineController.GetCurrentParticipant();

        if (currentCombatTarget == null)
        {
            Debug.LogWarning("[CombatManager] No current combat target found. Combat cannot continue.");
            return;
        }

        Debug.Log($"<color=#55AAFF>[Combat]</color> Current turn: {currentCombatTarget.CombatantName}");

        if (currentCombatTarget is PlayerParticipant playerCombatTarget)
        {
            StartPlayerTurn(playerCombatTarget);
            return;
        }

        if (currentCombatTarget is EnemyParticipant enemyCombatTarget)
        {
            StartEnemyTurn(enemyCombatTarget);
            return;
        }

        Debug.LogError(
            $"[CombatManager] Unsupported combat target type: {currentCombatTarget.GetType().Name}");
    }

    private void StartPlayerTurn(PlayerParticipant playerCombatTarget)
    {
        combatOptionsUIManager.ShowUI(playerCombatTarget);
    }

    private void StartEnemyTurn(EnemyParticipant enemyCombatTarget)
    {
        AttackDataSO selectedAttack = enemyActionSelector.SelectAttack(enemyCombatTarget);

        if (selectedAttack == null)
        {
            EndCurrentTurnSafely();
            return;
        }

        List<Participant> selectedTargets =
            enemyTargetSelector.SelectTargets(enemyCombatTarget, selectedAttack);

        CombatActionContext actionContext = new CombatActionContext(
            enemyCombatTarget,
            selectedAttack,
            selectedTargets);

        PlayActionContext(actionContext);
    }

    private void CombatOptionsUIButton_OnOptionSelected(AttackDataSO selectedAttack)
    {
        if (selectedAttack == null)
        {
            Debug.LogError("[CombatManager] Selected attack is null.");
            return;
        }

        if (currentCombatTarget == null)
        {
            Debug.LogError("[CombatManager] Current combat target is null.");
            return;
        }

        if (currentCombatTarget is not PlayerParticipant)
        {
            Debug.LogWarning("[CombatManager] Ignoring player option because current target is not a player.");
            return;
        }

        pendingPlayerAttack = selectedAttack;

        combatOptionsUIManager.HideUI();

        if (RequiresManualSelection(selectedAttack.TargetType))
        {
            combatTargetSelectionManager.BeginSelection(currentCombatTarget, selectedAttack);
            return;
        }

        List<Participant> selectedTargets =
            combatTargetProvider.GetAutoTargets(currentCombatTarget, selectedAttack);

        CombatActionContext actionContext = new CombatActionContext(
            currentCombatTarget,
            selectedAttack,
            selectedTargets);

        PlayActionContext(actionContext);
    }

    private void CombatTargetSelectionManager_TargetSelected(Participant selectedTarget)
    {
        if (pendingPlayerAttack == null)
        {
            Debug.LogError("[CombatManager] Target selected, but pending player attack is null.");
            return;
        }

        if (currentCombatTarget == null)
        {
            Debug.LogError("[CombatManager] Target selected, but current combat target is null.");
            ClearPendingPlayerAttack();
            return;
        }

        List<Participant> selectedTargets = new List<Participant>
        {
            selectedTarget
        };

        CombatActionContext actionContext = new CombatActionContext(
            currentCombatTarget,
            pendingPlayerAttack,
            selectedTargets);

        ClearPendingPlayerAttack();

        PlayActionContext(actionContext);
    }

    private void CombatTargetSelectionManager_TargetSelectionCancelled()
    {
        if (currentCombatTarget is PlayerParticipant playerCombatTarget)
        {
            combatOptionsUIManager.ShowUI(playerCombatTarget);
        }

        ClearPendingPlayerAttack();
    }

    private void PlayActionContext(CombatActionContext actionContext)
    {
        if (actionContext == null)
        {
            Debug.LogError("[CombatManager] Cannot play action because context is null.");
            EndCurrentTurnSafely();
            return;
        }

        if (!actionContext.IsValid())
        {
            Debug.LogError("[CombatManager] Cannot play action because context is invalid.");
            EndCurrentTurnSafely();
            return;
        }

        combatOptionsUIManager.HideUI();

        combatActionResolver.PlayAction(actionContext);
    }

    private void CombatActionResolver_ActionFinished(CombatActionContext actionContext)
    {
        ClearPendingPlayerAttack();

        combatTimelineController.OnTurnComplete();

        StartCombat();
    }

    private void EndCurrentTurnSafely()
    {
        ClearPendingPlayerAttack();

        if (combatTimelineController == null)
        {
            return;
        }

        combatTimelineController.OnTurnComplete();

        StartCombat();
    }

    private bool RequiresManualSelection(CombatActionTargetType targetType)
    {
        return targetType == CombatActionTargetType.SingleEnemy ||
               targetType == CombatActionTargetType.SingleAlly;
    }

    private void ClearPendingPlayerAttack()
    {
        pendingPlayerAttack = null;
    }
    #endregion
    */
}
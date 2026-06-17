using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Core Combat References")]
    [SerializeField] private CombatTimelineController combatTimelineController;
    [SerializeField] private CombatActionResolver combatActionResolver;

    [Header("Player Turn")]
    [SerializeField] private CombatOptionsUIManager combatOptionsUIManager;
    [SerializeField] private CombatTargetSelectionManager combatTargetSelectionManager;
    [SerializeField] private CombatTargetProvider combatTargetProvider;

    [Header("Enemy Turn")]
    [SerializeField] private EnemyActionSelector enemyActionSelector;
    [SerializeField] private EnemyTargetSelector enemyTargetSelector;

    private CombatTarget currentCombatTarget;
    private AttackDataSO pendingPlayerAttack;

    private void OnEnable()
    {
        CombatOptionsUIButton.OnOptionSelected += CombatOptionsUIButton_OnOptionSelected;

        if (combatTargetSelectionManager != null)
        {
            combatTargetSelectionManager.TargetSelected += CombatTargetSelectionManager_TargetSelected;
            combatTargetSelectionManager.TargetSelectionCancelled += CombatTargetSelectionManager_TargetSelectionCancelled;
        }

        if (combatActionResolver != null)
        {
            combatActionResolver.ActionFinished += CombatActionResolver_ActionFinished;
        }
    }

    private void OnDisable()
    {
        CombatOptionsUIButton.OnOptionSelected -= CombatOptionsUIButton_OnOptionSelected;

        if (combatTargetSelectionManager != null)
        {
            combatTargetSelectionManager.TargetSelected -= CombatTargetSelectionManager_TargetSelected;
            combatTargetSelectionManager.TargetSelectionCancelled -= CombatTargetSelectionManager_TargetSelectionCancelled;
        }

        if (combatActionResolver != null)
        {
            combatActionResolver.ActionFinished -= CombatActionResolver_ActionFinished;
        }
    }

    private void Start()
    {
        StartCombat();
    }

    private void StartCombat()
    {
        ClearPendingPlayerAttack();

        if (!CanStartCombat())
        {
            return;
        }

        currentCombatTarget = combatTimelineController.GetCurrentTarget();

        if (currentCombatTarget == null)
        {
            Debug.LogWarning("[CombatManager] No current combat target found. Combat cannot continue.");
            return;
        }

        Debug.Log($"<color=#55AAFF>[Combat]</color> Current turn: {currentCombatTarget.CombatantName}");

        if (currentCombatTarget is PlayerCombatTarget playerCombatTarget)
        {
            StartPlayerTurn(playerCombatTarget);
            return;
        }

        if (currentCombatTarget is EnemyCombatTarget enemyCombatTarget)
        {
            StartEnemyTurn(enemyCombatTarget);
            return;
        }

        Debug.LogError(
            $"[CombatManager] Unsupported combat target type: {currentCombatTarget.GetType().Name}");
    }

    private bool CanStartCombat()
    {
        if (combatTimelineController == null)
        {
            Debug.LogError("[CombatManager] CombatTimelineController is not assigned.");
            return false;
        }

        if (combatActionResolver == null)
        {
            Debug.LogError("[CombatManager] CombatActionResolver is not assigned.");
            return false;
        }

        if (combatOptionsUIManager == null)
        {
            Debug.LogError("[CombatManager] CombatOptionsUIManager is not assigned.");
            return false;
        }

        if (combatTargetSelectionManager == null)
        {
            Debug.LogError("[CombatManager] CombatTargetSelectionManager is not assigned.");
            return false;
        }

        if (combatTargetProvider == null)
        {
            Debug.LogError("[CombatManager] CombatTargetProvider is not assigned.");
            return false;
        }

        if (enemyActionSelector == null)
        {
            Debug.LogError("[CombatManager] EnemyActionSelector is not assigned.");
            return false;
        }

        if (enemyTargetSelector == null)
        {
            Debug.LogError("[CombatManager] EnemyTargetSelector is not assigned.");
            return false;
        }

        return true;
    }

    private void StartPlayerTurn(PlayerCombatTarget playerCombatTarget)
    {
        combatOptionsUIManager.ShowUI(playerCombatTarget);
    }

    private void StartEnemyTurn(EnemyCombatTarget enemyCombatTarget)
    {
        AttackDataSO selectedAttack = enemyActionSelector.SelectAttack(enemyCombatTarget);

        if (selectedAttack == null)
        {
            EndCurrentTurnSafely();
            return;
        }

        List<CombatTarget> selectedTargets =
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

        if (currentCombatTarget is not PlayerCombatTarget)
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

        List<CombatTarget> selectedTargets =
            combatTargetProvider.GetAutoTargets(currentCombatTarget, selectedAttack);

        CombatActionContext actionContext = new CombatActionContext(
            currentCombatTarget,
            selectedAttack,
            selectedTargets);

        PlayActionContext(actionContext);
    }

    private void CombatTargetSelectionManager_TargetSelected(CombatTarget selectedTarget)
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

        List<CombatTarget> selectedTargets = new List<CombatTarget>
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
        if (currentCombatTarget is PlayerCombatTarget playerCombatTarget)
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
}
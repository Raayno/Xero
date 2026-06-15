using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatTimelineController combatTimelineController;
    [SerializeField] private CombatOptionsUIManager combatOptionsUIManager;

    private CombatTarget combatTarget;

    private void OnEnable()
    {
        CombatOptionsUIButton.OnOptionSelected += CombatOptionsUIButton_OnOptionSelected;
    }

    private void CombatOptionsUIButton_OnOptionSelected(AttackDataSO obj)
    {
        PlayAttack(obj);
    }

    private void PlayAttack(AttackDataSO obj)
    {
        combatOptionsUIManager.HideUI();
        combatTarget = combatTimelineController.GetCurrentTarget();
        combatTarget.PlayAttackSequence(obj);
        combatTarget.AttackSequenceFinished += CombatTarget_AttackSequenceFinished;
    }

    private void CombatTarget_AttackSequenceFinished(CombatTarget obj)
    {
        combatTimelineController.OnTurnComplete();
        StartCombat();
        obj.AttackSequenceFinished -= CombatTarget_AttackSequenceFinished;
    }

    private void OnDisable()
    {
        CombatOptionsUIButton.OnOptionSelected += CombatOptionsUIButton_OnOptionSelected;
    }

    private void Start()
    {
        StartCombat();
    }

    private void StartCombat()
    {
        Debug.Log("Combat Started!");
        CombatTarget combatTarget = combatTimelineController.GetCurrentTarget();
        PlayerCombatTargetData combatTargetData = combatTarget.GetData();

        if (combatTarget is PlayerCombatTarget)
        {
            combatOptionsUIManager.ShowUI(combatTarget);
        }
        else
        {
            AttackDataSO attackDataSO = combatTargetData.attacks[Random.Range(0, combatTargetData.attacks.Count)];
            PlayAttack(attackDataSO);
        }
    }
}
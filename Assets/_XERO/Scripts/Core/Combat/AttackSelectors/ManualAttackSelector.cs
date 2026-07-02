using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ManualAttackSelector : AttackSelector
{
    protected static ManualAttackSelectorUI UI;
    protected AttackDataSO selectedAttack;

    public override async UniTask<AttackDataSO> SelectAttackAsync(List<AttackDataSO> attacks, CancellationToken cancellationToken = default)
    {
        selectedAttack = null;
        UI = UI != null ? UI : CombatController.Instance.CombatOptionsUIManager;

        var playerAttacks = attacks.OfType<PlayerAttackDataSO>().ToList();

        if(playerAttacks.Count < attacks.Count)
        {
            Debug.LogWarning("No player attacks available to show in the UI.");
        }

        if (playerAttacks.Count > 0)
        {
            SubscribeToUIEvents(true);
            UI.ShowUI(playerAttacks);
        }
        else
        {
            Debug.LogError("No player attacks available to show in the UI.");
            return null;
        }

        try
        {
            await UniTask.WaitUntil(() => selectedAttack != null, cancellationToken: cancellationToken);
            return selectedAttack;
        }
        finally
        {
            SubscribeToUIEvents(false);
            UI?.HideUI();
        }
    }

    protected void SubscribeToUIEvents(bool subscribe)
    {
        if (subscribe)
        {
            ManualAttackSelectorUIButton.OnOptionSelected += OnOptionSelected;
        }
        else
        {
            ManualAttackSelectorUIButton.OnOptionSelected -= OnOptionSelected;
        }
    }

    protected void OnOptionSelected(AttackDataSO selected)
    {
        if (selected == null)
        {
            Debug.LogError("Selected attack is null.");
            return;
        }
        UI.HideUI();
        selectedAttack = selected;
    }
}
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ManualAttackSelector : AttackSelector
{
    protected static ManualAttackSelectorUI UI;
    protected AttackDataSO selectedAttack;

    public override IEnumerator SelectAttackAsync(List<AttackDataSO> attacks, Action<AttackDataSO> onCompleted)
    {
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
            onCompleted(null);
            yield break;
        }

        yield return new WaitUntil(() => selectedAttack != null);

        onCompleted?.Invoke(selectedAttack);
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
        SubscribeToUIEvents(false);
        selectedAttack = selected;
    }
}
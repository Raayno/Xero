using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

public class CombatOptionsUIManager : MonoBehaviour
{
    [SerializeField] private Transform combatOptionsContainer;
    [SerializeField] private List<CombatOptionsUIButton> combatOptionsUIList = new List<CombatOptionsUIButton>();

    private void Awake()
    {
        HideUI();
    }

    public void ShowUI(PlayerParticipant combatTarget)
    {
        combatOptionsContainer.gameObject.SetActive(true);
        ShowOptionsUI(combatTarget.GetData());
    }

    public void HideUI()
    {
        combatOptionsContainer.gameObject.SetActive(false);
        HideOptionsUI();
    }

    private void ShowOptionsUI(PlayerCombatTargetData combatTargetData)
    {
        HideOptionsUI();
        for (int i = 0; i < combatTargetData.attacks.Count; i++)
        {
            CombatOptionsUIButton combatOptionsUIButton = combatOptionsUIList[i];

            combatOptionsUIButton.gameObject.SetActive(true);

            PlayerAttackDataSO attackDataSO = combatTargetData.attacks[i];
            combatOptionsUIButton.ShowUI(attackDataSO);
        }
    }

    private void HideOptionsUI()
    {
        foreach (var item in combatOptionsUIList)
        {
            item.gameObject.SetActive(false);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ManualAttackSelectorUI : MonoBehaviour
{
    [SerializeField] private Transform combatOptionsContainer;
    [SerializeField] private List<ManualAttackSelectorUIButton> combatOptionsUIList = new();

    private void Awake()
    {
        HideUI();
    }

    public void ShowUI(List<PlayerAttackDataSO> attacks)
    {
        combatOptionsContainer.gameObject.SetActive(true);
        ShowOptionsUI(attacks);
    }

    public void HideUI()
    {
        combatOptionsContainer.gameObject.SetActive(false);
        HideOptionsUI();
    }

    private void ShowOptionsUI(List<PlayerAttackDataSO> attacks)
    {
        HideOptionsUI();
        for (int i = 0; i < attacks.Count; i++)
        {
            ManualAttackSelectorUIButton combatOptionsUIButton = combatOptionsUIList[i];

            combatOptionsUIButton.gameObject.SetActive(true);

            PlayerAttackDataSO attackDataSO = attacks[i];
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

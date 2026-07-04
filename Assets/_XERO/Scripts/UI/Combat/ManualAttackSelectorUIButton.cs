using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ManualAttackSelectorUIButton : MonoBehaviour
{
    public static event Action<AttackDataSO> OnOptionSelected;

    [SerializeField] private TMP_Text nameTXT;
    [SerializeField] private TMP_Text descriptionTXT;
    [SerializeField] private Image inputIMG;

    [SerializeField] private Button button;

    private AttackDataSO attackDataSO;

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        OnOptionSelected?.Invoke(attackDataSO);
    }

    public void ShowUI(PlayerAttackDataSO attackDataSO)
    {
        this.attackDataSO = attackDataSO;
        nameTXT.text = attackDataSO.AttackName;
        descriptionTXT.text = attackDataSO.AttackDescription;
        inputIMG.gameObject.SetActive(false);
    }

    private void Reset()
    {
        if (button == null) button = GetComponent<Button>();
    }
}

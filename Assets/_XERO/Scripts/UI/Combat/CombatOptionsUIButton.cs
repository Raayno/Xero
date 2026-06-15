using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CombatOptionsUIButton : MonoBehaviour
{
    public static event Action<AttackDataSO> OnOptionSelected;

    [SerializeField] private TMP_Text nameTXT;
    [SerializeField] private TMP_Text descriptionTXT;
    [SerializeField] private Image inputIMG;

    [SerializeField] private Button button;

    private AttackDataSO attackDataSO;
    private void OnValidate()
    {
        if(!button)
            button = GetComponent<Button>();
    }

    private void Awake()
    {
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonCliek);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonCliek);
    }

    private void OnButtonCliek()
    {
        OnOptionSelected?.Invoke(attackDataSO);
    }

    public void ShowUI(PlayerAttackDataSO attackDataSO)
    {
        this.attackDataSO = attackDataSO;
        nameTXT.text = attackDataSO.attackName;
        descriptionTXT.text = attackDataSO.attackDescription;
        inputIMG.gameObject.SetActive(false);
    }
}

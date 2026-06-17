using UnityEngine;

[CreateAssetMenu(fileName = "DamageDataSO", menuName = "Combat/DamageDataSO")]
public class DamageDataSO : ScriptableObject
{
    [SerializeField] private float damageAmount;
    [SerializeField] private float stunAmount;

    public float DamageAmount => damageAmount;
    public float StunAmount => stunAmount;
}
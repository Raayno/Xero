using UnityEngine;

[CreateAssetMenu(fileName = "DamageDataSO", menuName = "Combat/DamageDataSO")]

public class DamageDataSO : ScriptableObject
{
    public float damageAmount;
    public float stunnAmount;
}

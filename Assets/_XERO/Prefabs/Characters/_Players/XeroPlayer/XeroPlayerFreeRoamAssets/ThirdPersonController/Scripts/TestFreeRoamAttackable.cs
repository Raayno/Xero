using UnityEngine;
using MoreMountains.Feedbacks;

public class TestFreeRoamAttackable : MonoBehaviour, IFreeRoamAttackable
{
    [SerializeField] private MMF_Player feedbacks;

    public bool OnAttack()
    {
        Debug.Log($"{gameObject.name} was attacked!");
        if (feedbacks != null)
        {
            feedbacks.PlayFeedbacks();
        }
        return true; // Return true to indicate that this object should block other things in range from being attacked by this attack
    }
}
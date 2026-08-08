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
        return false; // do not block other attackables from being attacked
    }
}
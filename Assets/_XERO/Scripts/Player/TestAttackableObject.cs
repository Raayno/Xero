using MoreMountains.Feedbacks;
using UnityEngine;

public class TestAttackableObject : MonoBehaviour, IAttackable
{
    [SerializeField] private MMF_Player testFeedback;

    public void OnAttack()
    {
        Debug.Log($"<color=red>[IAttackable]</color> {name} received attack.");
        testFeedback?.PlayFeedbacks();
    }
}

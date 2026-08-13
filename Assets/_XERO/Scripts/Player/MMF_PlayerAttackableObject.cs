using MoreMountains.Feedbacks;
using UnityEngine;

public class MMF_PlayerAttackableObject : MonoBehaviour, IAttackable
{
    [SerializeField] private MMF_Player feedback;

    public void OnAttack()
    {
        Debug.Log($"<color=red>[IAttackable]</color> {name} received attack.");
        feedback?.PlayFeedbacks();
    }
}

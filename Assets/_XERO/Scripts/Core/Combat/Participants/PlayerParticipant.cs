using UnityEngine;
using UnityEngine.Timeline;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerParticipant : Participant
{
    [Header("Player-specific")]
    [SerializeField] private TimelineAsset parryTimelineAsset;
    [SerializeField] private SignalAsset parrySignalAsset;
    [SerializeField] private SignalAsset counterattackSignalAsset;
    [SerializeField] private TimelineAsset parryCounterattackTimelineAsset;
    [SerializeField] private DamageDataSO counterattackDamageData;
    private bool canParry = true;
    private bool isTrueParry = false;
    public bool IsTrueParry => isTrueParry;

#region Parry
    public void OnParry()
    {
        if (canParry) ExecuteParrySequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask ExecuteParrySequenceAsync(CancellationToken cancellationToken)
    {
        isTrueParry = false;
        canParry = false;

        try
        {
            TimelineSignalBridge.SubscribeToSignal(true, parrySignalAsset, OnParrySignal);

            TimelineManager.PlayTimeline(parryTimelineAsset, Animator);

            await UniTask.Delay(System.TimeSpan.FromSeconds(parryTimelineAsset.duration), cancellationToken: cancellationToken);
        }
        finally
        {
            canParry = true;
            TimelineSignalBridge.SubscribeToSignal(false, parrySignalAsset, OnParrySignal);
            isTrueParry = false;
        }
    }

    private void OnParrySignal()
    {
        isTrueParry = !isTrueParry;
        if (enableDebug) Debug.Log($"<color=green>[PlayerParticipant]</color> True Parry window of {participantName} is now {(isTrueParry ? "open" : "closed")}.");
    }
#endregion

#region Counterattack (all attacks parried)
    public async UniTask OnPerformCounterattack(CombatDamageable target, CancellationToken cancellationToken)
    {
        if (enableDebug) Debug.Log($"<color=green>[PlayerParticipant]</color> {participantName} is performing a counterattack on {target.name}.");
        if (parryCounterattackTimelineAsset == null || counterattackDamageData == null)
        {
            Debug.LogWarning($"<color=green>[PlayerParticipant]</color> Perfect parry timeline or counterattack damage data is not set for {participantName}. Cannot perform counterattack.");
            return;
        }

        try
        {
            Feedbacks.PlayFeedback(FeedbackType.PlayerOnCounterattack, target.transform.position);
            
            TimelineSignalBridge.SubscribeToSignal(true, counterattackSignalAsset, () => OnCounterattackSignal(target));

            TimelineManager.PlayTimeline(parryCounterattackTimelineAsset, Animator);

            await UniTask.Delay(System.TimeSpan.FromSeconds(parryCounterattackTimelineAsset.duration), cancellationToken: cancellationToken);
        }
        finally
        {
            TimelineSignalBridge.SubscribeToSignal(false, counterattackSignalAsset, () => OnCounterattackSignal(target));
        }
    }

    private void OnCounterattackSignal(CombatDamageable target)
    {
        if (enableDebug) Debug.Log($"<color=green>[PlayerParticipant]</color> Counterattack signal received for {participantName} targeting {target.name}.");
        target.TakeDamage(counterattackDamageData);
    }
#endregion
}
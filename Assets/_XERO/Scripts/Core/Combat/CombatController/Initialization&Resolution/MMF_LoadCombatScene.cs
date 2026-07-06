using MoreMountains.Feedbacks;
using UnityEngine;

[AddComponentMenu("")]
[FeedbackHelp("This feedback will request the load of a combat arena scene, using the method of your choice")]
[System.Serializable]
[FeedbackPath("Scene/Load Combat Scene")]
public class MMF_LoadCombatScene : MMF_LoadScene
{
    public MMF_LoadCombatScene()
    {
        DestinationSceneAddressibleKey = "Zone_ID/Arenas/Combat_ID";
    }

    [MMFInspectorGroup("Combat Data", true)] 
    public CombatEnemiesData participantsData;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (!Active || !FeedbackTypeAuthorized) return;

        if (participantsData != null)
        {
            CombatEnemyDataCarrier.CombatEnemiesData = participantsData;
        }

        base.CustomPlayFeedback(position, feedbacksIntensity);
    }
}

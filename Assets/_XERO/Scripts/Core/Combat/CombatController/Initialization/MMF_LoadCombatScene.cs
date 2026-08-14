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
    public EnemiesCombatData participantsData;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (!Active || !FeedbackTypeAuthorized) return;

        if (DestinationSceneAddressibleKey == "Zone_ID/Arenas/Combat_ID")
        {
            Debug.LogWarning($"<color=orange>[MMF_LoadCombatScene]</color> The DestinationSceneAddressibleKey is set to the default value 'Zone_ID/Arenas/Combat_ID'. Please set it to the correct addressable key for the combat arena scene you want to load.");
        }

        if (participantsData != null)
        {
            EnemyCombatDataCarrier.EnemiesCombatData = participantsData;
        }
        SpecialCombatDataCarrier.VariablesLockedForTransition = true; // Lock variables to prevent overriding during transition
        // MUST BE UNLOCKED in the Combat Initialization script after the transition is complete!!!

        base.CustomPlayFeedback(position, feedbacksIntensity);
    }
}

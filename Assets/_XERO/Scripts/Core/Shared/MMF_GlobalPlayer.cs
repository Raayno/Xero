using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Gaskellgames;
using System.Linq;

public class MMF_GlobalPlayer : MMSingleton<MMF_GlobalPlayer>
{
    [SerializeField] private SerializedDictionary<FeedbackType, MMF_Player> globalFeedbacks = new();

    public void PlayGlobalFeedback(FeedbackType feedbackType, Vector3 position, float intensity = 1f)
    {
        if (globalFeedbacks.TryGetValue(feedbackType, out MMF_Player feedback))
        {
            feedback.PlayFeedbacks(position, intensity);
        }
        else
        {
            Debug.LogWarning($"<color=orange>[MMF_GlobalPlayer]</color> No feedback found for GlobalFeedbackType: {feedbackType}");
        }
    }

    [NaughtyAttributes.Button("Sort Dictionary Alphabetically by FeedbackType")]
    private void SortDictionaryByFeedbackType()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("<color=orange>[Feedbacks]</color> Dictionary sorted alphabetically by FeedbackType. Missing entries:");
        
        var sorted = new SerializedDictionary<FeedbackType, MMF_Player>();
        
        // Sort the feedbacks dictionary by the string representation of the FeedbackType enum values
        var alphabeticalFeedbackTypes = System.Enum.GetValues(typeof(FeedbackType))
            .Cast<FeedbackType>()
            .OrderBy(e => e.ToString());

        foreach (FeedbackType feedbackType in alphabeticalFeedbackTypes)
        {
            if (feedbackType == FeedbackType.None) continue; // Skip the None type
            
            if (globalFeedbacks.TryGetValue(feedbackType, out MMF_Player feedback))
            {
                sorted.Add(feedbackType, feedback);
            }
            else
            {
                sb.AppendLine($"{feedbackType}");
            }
        }
        
        globalFeedbacks = sorted;
        Debug.Log(sb.ToString());
    }
}

using UnityEngine;
using MoreMountains.Feedbacks;
using Gaskellgames;
using System.Linq;

public class Feedbacks : MonoBehaviour
{
    [Header("Local Feedbacks")]
    [SerializeField] private SerializedDictionary<FeedbackType, MMF_Player> feedbacks = new();
    public void PlayFeedback(FeedbackType feedbackType, Vector3 position, float intensity = 1f, bool playGlobal = true)
    {
        if (feedbacks.TryGetValue(feedbackType, out MMF_Player feedback))
        {
            feedback.PlayFeedbacks(position, intensity);
        }
        else
        {
            Debug.LogWarning($"<color=orange>[Feedbacks]</color> No feedback found for FeedbackType: {feedbackType}");
        }
        if (playGlobal) MMF_GlobalPlayer.Instance.PlayGlobalFeedback(feedbackType, position, intensity);
    }

    [Button("Sort Dictionary Alphabetically by FeedbackType")]
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
            
            if (feedbacks.TryGetValue(feedbackType, out MMF_Player feedback))
            {
                sorted.Add(feedbackType, feedback);
            }
            else
            {
                sb.AppendLine($"{feedbackType}");
            }
        }
        
        feedbacks = sorted;
        Debug.Log(sb.ToString());
    }

    [SerializeField] private string searchPrefix;
    [Button("Search for missing with SearchPrefix")]
    private void SearchForMissingWithPrefix()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine($"<color=orange>[Feedbacks]</color> Missing FeedbackTypes with prefix '{searchPrefix}':");
        
        var allFeedbackTypes = System.Enum.GetValues(typeof(FeedbackType))
            .Cast<FeedbackType>();

        bool foundAnyMissing = false;
        foreach (FeedbackType feedbackType in allFeedbackTypes)
        {
            if (feedbackType == FeedbackType.None) continue; // Skip the None type
            
            if (feedbackType.ToString().StartsWith(searchPrefix) && !feedbacks.ContainsKey(feedbackType))
            {
                foundAnyMissing = true;
                sb.AppendLine($"{feedbackType}");
            }
        }
        
        if (!foundAnyMissing)
        {
            Debug.Log("<color=orange>[Feedbacks]</color> No missing FeedbackTypes found with the specified prefix.");
        }
        else
        {
            Debug.Log(sb.ToString());
        }
    }
}

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
        if (playGlobal) MMGlobalPlayer.Instance.PlayGlobalFeedback(feedbackType, position, intensity);
    }

    [Button("Sort Alphabetically by FeedbackType")]
    private void SortDictionaryAlphabeticallyByFeedbackType()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("<color=orange>[Feedbacks]</color> Dictionary sorted alphabetically by FeedbackType. Missing entries:");
        
        var sorted = new SerializedDictionary<FeedbackType, MMF_Player>();
        
        // Sort the feedbacks dictionary by the string representation of the FeedbackType enum values
        var alphabeticalFeedbackTypes = System.Enum.GetValues(typeof(FeedbackType))
            .Cast<FeedbackType>()
            .OrderBy(e => e.ToString());

        int childIndex = 0;
        foreach (FeedbackType feedbackType in alphabeticalFeedbackTypes)
        {
            if (feedbackType == FeedbackType.None) continue; // Skip the None type
            
            if (feedbacks.TryGetValue(feedbackType, out MMF_Player feedback))
            {
                sorted.Add(feedbackType, feedback);
                // Move the corresponding child in the hierarchy to match the sorted order
                if (feedback != null && feedback.transform.parent == transform && childIndex < transform.childCount)
                    feedback.transform.SetSiblingIndex(childIndex);
                childIndex++;
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
    private FeedbackType[] GetMissingFeedbackTypesWithPrefix()
    {
        return System.Enum.GetValues(typeof(FeedbackType))
            .Cast<FeedbackType>()
            .Where(type => type != FeedbackType.None 
                        && type.ToString().StartsWith(searchPrefix) 
                        && !feedbacks.ContainsKey(type))
                        .ToArray();
    }

    [Button("Search for missing with SearchPrefix")]
    private void SearchForMissingWithPrefix()
    {
        var missingTypes = GetMissingFeedbackTypesWithPrefix();

        if (missingTypes.Length == 0)
        {
            Debug.Log("<color=orange>[Feedbacks]</color> No missing FeedbackTypes found with the specified prefix.");
            return;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine($"<color=orange>[Feedbacks]</color> Missing FeedbackTypes with prefix '{searchPrefix}':");
        
        foreach (var feedbackType in missingTypes)
        {
            sb.AppendLine($"{feedbackType}");
        }
        
        Debug.Log(sb.ToString());
    }

    [Button("Add missing with SearchPrefix")]
    private void AddMissingWithPrefix()
    {
        var missingTypes = GetMissingFeedbackTypesWithPrefix();

        if (missingTypes.Length == 0)
        {
            Debug.Log("<color=orange>[Feedbacks]</color> No missing FeedbackTypes found with the specified prefix.");
            return;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine($"<color=orange>[Feedbacks]</color> Adding missing FeedbackTypes with prefix '{searchPrefix}':");
        
        foreach (var feedbackType in missingTypes)
        {
            feedbacks.Add(feedbackType, null);
            sb.AppendLine($"{feedbackType}");
        }
        
        Debug.Log(sb.ToString());
    }
}

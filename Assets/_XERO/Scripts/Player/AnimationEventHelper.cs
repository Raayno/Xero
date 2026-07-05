using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelper : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<string, UnityEvent> keyValuePairs;
    public void OnEventTrigger(string eventName)
    {
        eventName.Replace(" ","");
        if (keyValuePairs.ContainsKey(eventName))
        {
            keyValuePairs[eventName]?.Invoke();
        }
    }

    public void OnFootstep()
    {

    }
}

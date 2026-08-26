using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

[RequireComponent(typeof(Collider))]
public class MMColliderActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider coll;
    [SerializeField] private int intensity = 1;
    [SerializeField] private MMF_Player[] feedbacks = new MMF_Player[0];
    public MMF_Player[] Feedbacks
    {
        get => feedbacks;
        set => feedbacks = value;
    }
    [Header("Play feedbacks on")]
    [SerializeField] private bool onTriggerEnter = true;
    [SerializeField] private bool onTriggerExit = false;
    [Header("with layers")]
    [SerializeField] private LayerMask layerMask = -1;

    public int LayerOfOtherObject { get; private set; } = -1;

    public void OnTriggerEnter(Collider other)
    {
        if (!onTriggerEnter) return;
        if (!layerMask.MMContains(other.gameObject.layer)) return;

        foreach (var feedback in feedbacks)
        {
            feedback.PlayFeedbacks(transform.position, intensity);
        }

        LayerOfOtherObject = other.gameObject.layer;
    }

    public void OnTriggerExit(Collider other)
    {
        if (!onTriggerExit) return;
        if (!layerMask.MMContains(other.gameObject.layer)) return;

        foreach (var feedback in feedbacks)
        {
            feedback.PlayFeedbacks(transform.position, intensity);
        }

        LayerOfOtherObject = other.gameObject.layer;
    }

    private void OnValidate()
    {
        if (coll == null)
        {
            coll = GetComponent<Collider>();
        }
        if (feedbacks == null || feedbacks.Length == 0)
        {
            feedbacks = GetComponents<MMF_Player>();
        }
        if (layerMask == -1)
        {
            layerMask = coll.includeLayers;
        }
    }
}

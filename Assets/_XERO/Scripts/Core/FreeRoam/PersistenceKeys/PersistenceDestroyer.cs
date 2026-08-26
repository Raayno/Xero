using UnityEngine;
using MoreMountains.Feedbacks;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(PersistenceKey))]
public class PersistenceDestroyer : MonoBehaviour
{
    [SerializeField] private MMF_Player justKilledFeedback;
    [SerializeField] private PersistenceKey persistenceKey;
    [SerializeField, HideInInspector] private PersistenceRegistry registry;

    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        object value = registry.GetValue(persistenceKey.Key);

        if (value == null) return;
        
        if (value is bool boolValue)
        {
            if (boolValue)
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                DestroyAfterFeedback(cancellationTokenSource.Token).Forget();
            }
            else
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }

    private async UniTask DestroyAfterFeedback(CancellationToken cancellationToken)
    {
        if (justKilledFeedback != null)
        {
            justKilledFeedback.PlayFeedbacks();
            await UniTask.WaitUntil(() => !justKilledFeedback.IsPlaying, cancellationToken: cancellationToken);
        }

        gameObject.SetActive(false);

        registry.SetValue(persistenceKey.Key, value: false, isClearable: true);

        Destroy(gameObject);
    }

    void OnValidate()
    {
        if (persistenceKey == null)
        {
            persistenceKey = GetComponent<PersistenceKey>();
        }
    }
}

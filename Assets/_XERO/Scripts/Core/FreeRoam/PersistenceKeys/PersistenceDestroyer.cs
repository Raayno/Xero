using UnityEngine;

[RequireComponent(typeof(PersistenceKey))]
public class PersistenceDestroyer : MonoBehaviour
{
    [SerializeField] private PersistenceKey persistenceKey;
    [SerializeField, HideInInspector] private PersistenceRegistry registry;

    private void Awake()
    {
        if (registry.ContainsKey(persistenceKey.Key))
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
            return; 
        }
    }

    void OnValidate()
    {
        if (persistenceKey == null)
        {
            persistenceKey = GetComponent<PersistenceKey>();
        }
    }
}

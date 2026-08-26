using UnityEngine;

[RequireComponent(typeof(PersistenceKey))]
public class DisableEnemyGroupFunctionalityIfPersistence : MonoBehaviour
{
    [SerializeField] private Transform enemies;
    [SerializeField] private PersistenceKey persistenceKey;
    [SerializeField, HideInInspector] private PersistenceRegistry registry;
    private void Awake()
    {
        object value = registry.GetValue(persistenceKey.Key);

        if (value == null) return;

        if (value is bool wasJustKilled && !wasJustKilled) return;
        
        if (enemies != null)
        {
            for (int i = 0; i < enemies.childCount; i++)
            {
                var enemy = enemies.GetChild(i).gameObject;
                if (enemy == null) continue;
                DestroyNonRendererAndNonTransformComponents();

                void DestroyNonRendererAndNonTransformComponents()
                {
                    foreach (var behaviour in enemy.GetComponentsInChildren<Behaviour>())
                    {
                        behaviour.enabled = false;
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        if (enemies == null)
        {
            enemies = transform.Find("Enemies");
        }
        
        if (persistenceKey == null)
        {
            persistenceKey = GetComponent<PersistenceKey>();
        }
    }
}

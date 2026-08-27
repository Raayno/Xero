using UnityEngine;

[RequireComponent(typeof(PersistenceKey))]
public class DisableEnemyGroupFunctionalityIfPersistence : MonoBehaviour
{
    [SerializeField] private Transform enemies;
    [SerializeField] private Behaviour[] behavioursToKeepEnabled;
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
                DisableNonRendererAndNonTransformComponents();

                void DisableNonRendererAndNonTransformComponents()
                {
                    foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                    bool thereAreBehavioursToKeepEnabled = behavioursToKeepEnabled != null && behavioursToKeepEnabled.Length > 0;
                    foreach (var behaviour in enemy.GetComponentsInChildren<Behaviour>())
                    {
                        if (behaviour == this
                            || (thereAreBehavioursToKeepEnabled && System.Array.Exists(behavioursToKeepEnabled, b => b == behaviour)))
                        {
                            continue;
                        }

                        behaviour.enabled = false;
                    }
                    Debug.Log($"[DisableEnemyGroupFunctionalityIfPersistence] Disabled all components on {enemy.name} except for Transform, Renderer, and specified behaviours.");
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

using UnityEngine;

[ExecuteAlways]
public class PersistenceKey : MonoBehaviour
{
    [SerializeField, Gaskellgames.ReadOnly] private string key;
    public string PersistentKey => key;

#if UNITY_EDITOR
    [SerializeField, HideInInspector] private PersistenceRegistry registry;

    [ContextMenu("Activate Persistence Key")]
    private void ActivatePersistanceKey() => registry.ActivatePersistenceKey(key, true);

    [ContextMenu("Deactivate Persistence Key")]
    private void DeactivatePersistenceKey() => registry.RemovePersistenceKey(key);

    private void Reset()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) 
        {
            key = string.Empty; // Reset key for prefab assets to avoid duplicates
            UnityEditor.EditorUtility.SetDirty(this);
            return;
        }

        GenerateKey();
    }
    
    private void OnValidate()
    {
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) 
        {
            key = string.Empty; // Reset key for prefab assets to avoid duplicates
            UnityEditor.EditorUtility.SetDirty(this);
            return;
        }

        if (registry == null) return;

        Event currentEvent = Event.current;
        bool isUserDuplicating = currentEvent != null && 
                                 currentEvent.type == EventType.ExecuteCommand && 
                                 currentEvent.commandName == "Duplicate";
        if (isUserDuplicating)
        {
            GenerateKey();
            return;
        } 

        if (string.IsNullOrEmpty(key))
        {
            GenerateKey();
        }
    }

    private void GenerateKey()
    {
        key = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif
}

using UnityEngine;
using Gaskellgames;

public partial class PersistenceRegistry : ScriptableObject
{
    [SerializeField] private string utilityField = string.Empty;
     
#if UNITY_EDITOR
    [Button] private void GetValueForUtilityField()
    {
        object value = GetValue(utilityField);
        if (value != null)
        {
            Debug.Log($"PersistenceRegistry: Value for key '{utilityField}': {value}");
        }
        else
        {
            Debug.LogWarning($"PersistenceRegistry: No value found for key '{utilityField}'.");
        }
    }

    [Button, Tooltip("Check active persistances for the key entered in Utility Field without removing them")] 
    private void CheckActivePersistancesForUtilityFieldKey()
    {
        bool foundAny = false;
        
        if (activePersistancesClearable.ContainsKey(utilityField))
        {
            Debug.Log($"PersistenceRegistry: [FOUND] Clearable object persistance exists for key '{utilityField}'.");
            foundAny = true;
        }
        
        if (activeObjectPersistancesNonclearable.ContainsKey(utilityField))
        {
            Debug.Log($"PersistenceRegistry: [FOUND] Non-clearable object persistance exists for key '{utilityField}'.");
            foundAny = true;
        }

        if (!foundAny)
        {
            Debug.LogWarning($"PersistenceRegistry: No active persistance found for key '{utilityField}'.");
        }
    }

    [Button, Tooltip("Deactivate active persistances for the key entered in Utility Field (the only way to deactivate non-clearable persistances)")] 
    private void RemoveActivePersistancesForUtilityFieldKey()
    {
        bool removedAny = false;

        if (activePersistancesClearable.ContainsKey(utilityField))
        {
            RemovePersistenceKey(utilityField);
            Debug.Log($"PersistenceRegistry: Clearable object persistance for key '{utilityField}' removed.");
            removedAny = true;
        }

        if (activeObjectPersistancesNonclearable.ContainsKey(utilityField))
        {
            activeObjectPersistancesNonclearable.Remove(utilityField);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            Debug.Log($"PersistenceRegistry: Non-clearable object persistance for key '{utilityField}' removed.");
            removedAny = true;
        }

        if (!removedAny)
        {
            Debug.LogWarning($"PersistenceRegistry: No active persistance found for key '{utilityField}' to remove.");
        }
        else
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    [Button] private void RemoveAllActivePersistances()
    {
        ClearAllPersistences();
        Debug.Log($"PersistenceRegistry: All active persistances removed.");
    }
#endif
}
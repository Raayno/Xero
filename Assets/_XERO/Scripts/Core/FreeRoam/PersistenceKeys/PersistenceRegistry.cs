using System.Collections.Generic;
using UnityEngine;
using Gaskellgames;


[System.Serializable]
public struct PersistenceItem
{
    public string key;
    public string typeName;
    public string rawData;

    public PersistenceItem(string key, object value)
    {
        this.key = key;
        if (value == null)
        {
            typeName = string.Empty;
            rawData = string.Empty;
            return;
        }

        System.Type type = value.GetType();
        typeName = type.AssemblyQualifiedName;

        // Jawne kodowanie typów podstawowych (JsonUtility nie lubi surowych prymitywów)
        if (value is bool b) rawData = b.ToString();
        else if (value is int i) rawData = i.ToString();
        else if (value is float f) rawData = f.ToString(System.Globalization.CultureInfo.InvariantCulture);
        else if (value is string s) rawData = s;
        else rawData = JsonUtility.ToJson(value); // Dla niestandardowych klas [System.Serializable]
    }

    public readonly object Deserialize()
    {
        if (string.IsNullOrEmpty(typeName)) return null;
        System.Type type = System.Type.GetType(typeName);
        if (type == null) return null;

        // Jawne dekodowanie typów podstawowych
        if (type == typeof(bool)) return bool.Parse(rawData);
        if (type == typeof(int)) return int.Parse(rawData);
        if (type == typeof(float)) return float.Parse(rawData, System.Globalization.CultureInfo.InvariantCulture);
        if (type == typeof(string)) return rawData;

        return JsonUtility.FromJson(rawData, type);
    }
}

[EnsureAssetInstance]
public class PersistenceRegistry : ScriptableObject, ISerializationCallbackReceiver
{
    // Używamy zwykłych list struktur, które Unity serializuje bezbłędnie i natychmiast wyświetla w inspektorze
    [ReadOnly, SerializeField] private List<PersistenceItem> serializedClearable = new();
    [ReadOnly, SerializeField] private List<PersistenceItem> serializedNonclearable = new();

    private readonly Dictionary<string, object> activePersistancesClearable = new();
    private readonly Dictionary<string, object> activeObjectPersistancesNonclearable = new();

    // Wywoływane przez Unity podczas zapisu projektu/wejścia w Playmode - zabezpieczenie synchronizacji
    public void OnBeforeSerialize()
    {
        SyncDictionariesToLists();
    }

    // Wywoływane przez Unity zaraz po uruchomieniu Playmode - przywraca dane do pamięci RAM
    public void OnAfterDeserialize()
    {
        activePersistancesClearable.Clear();
        foreach (var item in serializedClearable)
        {
            activePersistancesClearable[item.key] = item.Deserialize();
        }

        activeObjectPersistancesNonclearable.Clear();
        foreach (var item in serializedNonclearable)
        {
            activeObjectPersistancesNonclearable[item.key] = item.Deserialize();
        }
    }

    private void SyncDictionariesToLists()
    {
        serializedClearable.Clear();
        foreach (var kvp in activePersistancesClearable)
        {
            serializedClearable.Add(new PersistenceItem(kvp.Key, kvp.Value));
        }

        serializedNonclearable.Clear();
        foreach (var kvp in activeObjectPersistancesNonclearable)
        {
            serializedNonclearable.Add(new PersistenceItem(kvp.Key, kvp.Value));
        }
    }

    public void ActivatePersistenceKey(string key, bool isClearable = true, object value = null)
    {
        value ??= true; 

        if (isClearable)
        {
            activePersistancesClearable[key] = value;
        }
        else
        {
            activeObjectPersistancesNonclearable[key] = value;
        }

        // Natychmiastowa synchronizacja z listami widocznymi w inspektorze
        SyncDictionariesToLists();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public bool ContainsKey(string key)
    {
        return activePersistancesClearable.ContainsKey(key) 
            || activeObjectPersistancesNonclearable.ContainsKey(key);
    }

    public object GetValue(string key)
    {
        if (activePersistancesClearable.TryGetValue(key, out var value))
        {
            return value;
        }
        if (activeObjectPersistancesNonclearable.TryGetValue(key, out value))
        {
            return value;
        }
        return null;
    }

    public void RemovePersistenceKey(string key)
    {
        if (activePersistancesClearable.ContainsKey(key))
            activePersistancesClearable.Remove(key);
            
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void ClearAllPersistences()
    {
        activePersistancesClearable.Clear();
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ReadOnly, SerializeField, OnValueChanged(nameof(ValidatePersistences))] private List<string> allKeys = new();
    public List<string> AllKeys
    {
        get => allKeys;
        set => allKeys = value;
    }

    [SerializeField] private string utilityField = string.Empty;
    
    [SerializeField] private bool enableDebug = true;

#if UNITY_EDITOR
    private void ValidatePersistences()
    {
        foreach (var key in activePersistancesClearable.Keys)
        {
            if (!allKeys.Contains(key))
            {
                activePersistancesClearable.Remove(key);
            }
        }
        foreach (var key in activeObjectPersistancesNonclearable.Keys)
        {
            if (!allKeys.Contains(key))
            {
                activeObjectPersistancesNonclearable.Remove(key);
            }
        }
    }

    [Button] private void SearchForUtilityField()
    {
        System.Text.StringBuilder sb = new($"PersistenceRegistry: Search results for '{utilityField}':\n");
        bool found = false;
        foreach (var key in allKeys)
        {
            if (key.Contains(utilityField))
            {
                found = true;
                sb.AppendLine(key);
            }
        }
        if (!found)
        {
            sb.AppendLine("Nothing found.");
        }
        Debug.Log(sb.ToString());
    }

    [Button] private void GetValueForUtilityField()
    {
        if (!allKeys.Contains(utilityField))
        {
            Debug.LogWarning($"PersistenceRegistry: Key '{utilityField}' not found in allKeys.");
        }

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
        if (!allKeys.Contains(utilityField))
        {
            Debug.LogWarning($"PersistenceRegistry: Key '{utilityField}' not found in allKeys, there shouldn't be any active persistances.");
        }

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
        if (!allKeys.Contains(utilityField))
        {
            Debug.LogWarning($"PersistenceRegistry: Key '{utilityField}' not found in allKeys. No persistances were removed.");
            return;
        }

        bool removedAny = false;

        if (activePersistancesClearable.ContainsKey(utilityField))
        {
            activePersistancesClearable.Remove(utilityField);
            Debug.Log($"PersistenceRegistry: Clearable object persistance for key '{utilityField}' removed.");
            removedAny = true;
        }

        if (activeObjectPersistancesNonclearable.ContainsKey(utilityField))
        {
            activeObjectPersistancesNonclearable.Remove(utilityField);
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

    [Button] private void CheckAllActivePersistances()
    {
        if (allKeys.Count == 0)
        {
            Debug.LogWarning($"PersistenceRegistry: No keys found in allKeys. There are {activePersistancesClearable.Count + activeObjectPersistancesNonclearable.Count} active persistances, but no keys to check against.");
            return;
        }

        Debug.Log($"PersistenceRegistry: Checking all active persistances...");
        foreach (var key in allKeys)
        {
            utilityField = key;
            CheckActivePersistancesForUtilityFieldKey();
        }
    }

    [Button] private void RemoveAllActivePersistances()
    {
        ClearAllPersistences();
        Debug.Log($"PersistenceRegistry: All active persistances removed.");
    }
#endif
}
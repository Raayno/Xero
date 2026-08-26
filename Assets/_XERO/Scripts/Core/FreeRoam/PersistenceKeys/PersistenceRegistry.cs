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
public partial class PersistenceRegistry : ScriptableObject, ISerializationCallbackReceiver
{
    [ReadOnly, SerializeField] private List<PersistenceItem> serializedClearable = new();
    [ReadOnly, SerializeField] private List<PersistenceItem> serializedNonclearable = new();

    private readonly Dictionary<string, object> activePersistancesClearable = new();
    private readonly Dictionary<string, object> activeObjectPersistancesNonclearable = new();
    public void OnBeforeSerialize()
    {
        SyncDictionariesToLists();
    }
    public void OnAfterDeserialize()
    {
        DeserializeListsBackToDictionaries();
    }

    private void DeserializeListsBackToDictionaries()
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

    public void SetValue(string key, object value = null, bool isClearable = true)
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

    /// <returns>null if not found</returns>
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

#if UNITY_EDITOR
    [SerializeField] private bool preservePersistencesOnPlayModeExit = true;
    private void OnEnable()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.EnteredEditMode &&
            !preservePersistencesOnPlayModeExit)
        {
            Debug.Log("PersistenceRegistry: Clearing all persistences after play mode exit.");
            ClearAllPersistences();
        }
    }
#endif
}
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Only in Assembly-CSharp
/// </summary>
[InitializeOnLoad]
public static class SOAssetGenerationProcessor
{
    private static readonly bool enableDebug = false;
    static SOAssetGenerationProcessor()
    {
        if (enableDebug) Debug.Log("[AssetGenerator] Scanning for missing ScriptableObject instances...");
        GenerateMissingAssets(typeof(EnsureAssetInstanceAttribute).Assembly);
        // GenerateMissingAssets(typeof(SOAssetGenerationProcessor).Assembly);
    }

    private static void GenerateMissingAssets(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {

            if (type.IsAbstract || type.IsInterface)
                continue;

            if (enableDebug) if(type.Name == nameof(SelfTargetSelector)) Debug.Log($"[AssetGenerator] Found TargetSelector type: {type.FullName} after filtering abstract and interface types.");
            
            // Check if the class inherits from ScriptableObject and has the attribute
            if (typeof(ScriptableObject).IsAssignableFrom(type) && 
                type.GetCustomAttribute<EnsureAssetInstanceAttribute>() != null &&
                type.GetCustomAttribute<IgnoreAssetInstanceEnsurement>() == null)
            {
                if (enableDebug) if(type.Name == nameof(SelfTargetSelector)) Debug.Log($"[AssetGenerator] Found ScriptableObject type with EnsureAssetInstanceAttribute: {type.FullName}. Processing...");
                ProcessSOType(type);
            }
        }
    }

    private static void ProcessSOType(Type soType)
    {
        EnsureAssetInstanceAttribute attribute = soType.GetCustomAttribute<EnsureAssetInstanceAttribute>();

        // Find the script file location using its class name
        string[] guids = AssetDatabase.FindAssets($"t:MonoScript {soType.Name}");
        if (guids.Length == 0)
        {
            Debug.LogError($"[AssetGenerator] Could not find script for {soType.Name}. Ensure the script is in the Assets folder.");
            return;
        }

        string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        string folderPath = Path.GetDirectoryName(scriptPath);

        // Define the desired asset path (custom/named exactly after the class)
        string nameToUse = (attribute.Name ?? soType.Name) + ".asset";
        string assetPath = Path.Combine(folderPath, $"{nameToUse}").Replace("\\", "/");

        // Create it only if it doesn't exist
        if (File.Exists(Path.GetFullPath(assetPath))) return;

        // Warn if an asset with the same name already exists somewhere else in the project
        SearchForExistingAsset(soType, nameToUse);

        ScriptableObject newAsset = ScriptableObject.CreateInstance(soType);

        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AssetGenerator] Generated missing instance for <b>{soType.Name}</b> at: {assetPath}");
    }

    private static void SearchForExistingAsset(Type soType, string assetName)
    {
        string[] existingAssets = AssetDatabase.FindAssets($"t:{soType.Name} {assetName}");

        if (existingAssets.Length == 0) return;
        foreach (string guid in existingAssets)
        {
            string existingAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(existingAssetPath) == assetName)
            {
                Debug.LogWarning($"[AssetGenerator] An instance of <b>{soType.Name}</b> already exists at: {existingAssetPath}");
            }
        }
    }
}
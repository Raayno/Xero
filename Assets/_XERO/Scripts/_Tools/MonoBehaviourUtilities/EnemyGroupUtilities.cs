#if UNITY_EDITOR
using UnityEngine;
using Gaskellgames;
using Unity.Behavior;
using MoreMountains.Feedbacks;
using UnityEditor;
using System.Linq;

[StripOnBuild]
public class EnemyGroupUtilities : MonoBehaviour
{
    [SerializeField, OnValueChanged(nameof(UpdateLoadCombatSceneFeedback))] private EnemiesCombatData participantsData;
    [SerializeField, OnValueChanged(nameof(UpdateLoadCombatSceneFeedback))] private Vector2Int ZoneAndArenaID = new(0, 0);
    [SerializeField, OnValueChanged(nameof(UpdateLoadCombatSceneFeedback))] private MMF_Player LoadCombatSceneFeedback;
    [SerializeField] private Transform enemiesParent;
    [SerializeField, OnValueChanged(nameof(UpdateRoamZone))] private EnemyRoamZone enemyRoamZone;

    [Button] private void UpdateRoamZone()
    {
        if (enemyRoamZone == null)
        {
            Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No EnemyRoamZone assigned in {gameObject.name}. Please assign an EnemyRoamZone component.");
            return;
        }

        BehaviorGraphAgent[] agents = enemiesParent.GetComponentsInChildren<BehaviorGraphAgent>();

        if (agents.Length == 0)
        {
            Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No BehaviorGraphAgent components found in children of {enemiesParent.name}. Please ensure that the enemy GameObjects have the BehaviorGraphAgent component.");
            return;
        }

        Undo.RecordObject(enemyRoamZone, "Update EnemyRoamZone BehaviorAgents");
        enemyRoamZone.BehaviorAgents = agents;
        EditorUtility.SetDirty(enemyRoamZone);

        Debug.Log($"<color=orange>[EnemyGroupUtilities]</color> Updated EnemyRoamZone with {agents.Length} BehaviorGraphAgents and set LoadCombatScene feedback to destination scene key: {LoadCombatSceneFeedback.GetFeedbackOfType<MMF_LoadCombatScene>().DestinationSceneAddressibleKey}");
    }

    [Button] private void UpdateLoadCombatSceneFeedback()
    {
        OnValidate();

        if (enemiesParent == null)
        {
            Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No EnemiesParent assigned in {gameObject.name}. Please assign a parent GameObject that contains all the enemy GameObjects.");
            return;
        }
        
        if (ZoneAndArenaID.x >= 0 && ZoneAndArenaID.y >= 0)
        {
            string destinationSceneKey = $"Zone{IntToString(ZoneAndArenaID.x)}/Arenas/Combat{IntToString(ZoneAndArenaID.y)}";

            Undo.RecordObject(LoadCombatSceneFeedback, "Update LoadCombatScene Feedback DestinationSceneAddressibleKey");
            LoadCombatSceneFeedback.GetFeedbackOfType<MMF_LoadCombatScene>().DestinationSceneAddressibleKey = destinationSceneKey;
            EditorUtility.SetDirty(LoadCombatSceneFeedback);

            Debug.Log($"<color=orange>[EnemyGroupUtilities]</color> Updated LoadCombatScene feedback to destination scene key: {destinationSceneKey}");
        }
        else
        {
            Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> ZoneAndArenaID is not set correctly in {gameObject.name}. Please set both Zone and Arena IDs to values greater than 0.");
        }
        
        Undo.RecordObject(LoadCombatSceneFeedback, "Update LoadCombatScene Feedback ParticipantsData");
        LoadCombatSceneFeedback.GetFeedbackOfType<MMF_LoadCombatScene>().participantsData = participantsData;
        EditorUtility.SetDirty(LoadCombatSceneFeedback);

        var eyesTags = enemiesParent.GetComponentsInChildren<EyesTag>();

        if (eyesTags.Length == 0)
        {
            Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No EyesTag components found in children of {enemiesParent.name}. Please ensure that the enemy GameObjects have the EyesTag component.");
            return;
        }

        foreach (var eyesTag in eyesTags)
        {
            if (eyesTag.TryGetComponent(out MMF_ColliderActions colliderActions))
            {
                Undo.RecordObject(colliderActions, "Update MMF_ColliderActions Feedbacks");
                colliderActions.Feedbacks = new MMF_Player[] { LoadCombatSceneFeedback };
                EditorUtility.SetDirty(colliderActions);
            }
            else
            {
                Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No MMF_ColliderActions found on {eyesTag.name}. Please ensure that the enemy GameObject has the MMF_ColliderActions component for loading the combat scene.");
            }

            if (eyesTag.TryGetComponent(out EnemyFreeRoamAttackable enemyFreeRoamAttackable))
            {
                Undo.RecordObject(enemyFreeRoamAttackable, "Update EnemyFreeRoamAttackable LoadCombatSceneFeedback");
                enemyFreeRoamAttackable.LoadCombatSceneFeedback = LoadCombatSceneFeedback;
                EditorUtility.SetDirty(enemyFreeRoamAttackable);
            }
            else
            {
                Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No EnemyFreeRoamAttackable found on {eyesTag.name}. Please ensure that the enemy GameObject has the EnemyFreeRoamAttackable component for loading the combat scene.");
            }

            if (eyesTag.TryGetComponent(out MMF_Player oldPerEnemyLoadCombatSceneFeedback))
            {
                Undo.DestroyObjectImmediate(oldPerEnemyLoadCombatSceneFeedback);
            }
        }

        Debug.Log($"<color=orange>[EnemyGroupUtilities]</color> Updated {eyesTags.Length} enemies to use the group LoadCombatScene feedback instead of individual ones.");
    }

    void OnValidate()
    {
        if (participantsData == null || participantsData.EnemyParticipants == null || participantsData.EnemyParticipants.Length == 0)
        {
            // foreach enemy child of enemiesParent, get the prefab path and load the Combat asset, then create a new EnemiesCombatData with those assets
            participantsData = new(participantsData, enemiesParent.Cast<Transform>()
                .Select(child =>
                {
                    var freeRoamPrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (freeRoamPrefab == null)
                    {
                        Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> No prefab found for {child.name}. Please ensure that the enemy GameObject is a prefab instance.");
                        return null;
                    }

                    string freeRoamPath = AssetDatabase.GetAssetPath(freeRoamPrefab); // Assets/Enemies/Goblin/FreeRoamGoblin.prefab
                    string directory = System.IO.Path.GetDirectoryName(freeRoamPath); // Assets/Enemies/Goblin
                    
                    string combatFileName = System.IO.Path.GetFileName(freeRoamPath).Replace("FreeRoam", "Combat"); 
                    
                    string combatPath = System.IO.Path.Combine(directory, combatFileName).Replace("\\", "/"); // Assets/Enemies/Goblin/CombatGoblin.prefab

                    var combatPrefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(combatPath);
                    if (combatPrefabGO == null)
                    {
                        Debug.LogError($"<color=red>[EnemyGroupUtilities]</color> Could not find Combat prefab at path: {combatPath}");
                        return null;
                    }

                    if (!combatPrefabGO.TryGetComponent<EnemyParticipant>(out var combatAsset))
                    {
                        Debug.LogWarning($"<color=orange>[EnemyGroupUtilities]</color> {combatPrefabGO.name} doesn't have EnemyParticipant component.");
                        return null;
                    }

                    return combatAsset;
                })
                .Where(asset => asset != null)
                .ToArray());
            EditorUtility.SetDirty(this);
        }

        if (TryGetComponent<PersistenceKey>(out var persistenceKey))
        {
            if (participantsData == null || participantsData.FreeRoamEnemyPersistenceKey == null || participantsData.FreeRoamEnemyPersistenceKey == string.Empty
                || participantsData.FreeRoamEnemyPersistenceKey != persistenceKey.Key)
            {
                participantsData = new (participantsData, freeRoamEnemyPersistenceKey: persistenceKey.Key);
            }
        }

        if (participantsData == null || participantsData.SceneToLoadAfterCombatAddressibleKey == null || participantsData.SceneToLoadAfterCombatAddressibleKey == string.Empty)
        {
            participantsData = new (participantsData, sceneToLoadAfterCombat: $"Zone{IntToString(ZoneAndArenaID.x)}/FreeRoam");
        }
    }

    static string IntToString(int num) => num < 10 ? $"0{num}" : num.ToString();
}
#endif

// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AxonGenesis
{
    public static class PrefabUtil
    {
        public static bool IsEditingPrefab => PrefabStageUtility.GetCurrentPrefabStage() != null;

        public static bool IsPrefabAsset(GameObject gameObject)
        {
            return PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab;
        }

        public static bool IsPrefabInstance(GameObject gameObject)
        {
            return PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.Connected;
        }

        public static bool IsPrefabRootInstance(GameObject gameObject)
        {
            if(gameObject == null) {
                return false;
            }   
            return PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
        }

        public static void OpenPrefab(GameObject prefabInstance)
        {
            // Check if the GameObject is a prefab instance
            //Debug.Log($"OpenPrefab:{prefabInstance.name}", prefabInstance);
            if (!IsPrefabRootInstance(prefabInstance)) {
                Debug.LogWarning("The GameObject provided is not a prefab root object.");
            }
            else {
                // Get the path of the original prefab asset
                GameObject prefabAsset = (GameObject)PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefabInstance);
                string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);

                if (!string.IsNullOrEmpty(prefabPath)) {
                    // Open the prefab in the editor
                    PrefabStage stage = PrefabStageUtility.OpenPrefab(prefabPath, prefabInstance);
                }
                else {
                    Debug.LogWarning("Unable to find the prefab path.");
                }
            }
        }

        public static void ExitPrefab()
        {
            StageUtility.GoBackToPreviousStage();
        }

        public static void UnpackPrefab(GameObject prefabInstance)
        {
            if (prefabInstance == null) {
                Debug.LogError("The provided GameObject is null.");
                return;
            }

            // Check if the GameObject is part of a prefab instance
            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(prefabInstance);
            if (prefabStatus == PrefabInstanceStatus.NotAPrefab) {
                Debug.LogWarning("The provided GameObject is not part of a prefab instance.");
                return;
            }

            // Unpack the prefab instance
            PrefabUtility.UnpackPrefabInstance(prefabInstance, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            Debug.Log($"Prefab instance '{prefabInstance.name}' has been unpacked.");//--KEEP
        }
    }

}//AxonGenesis

#endif

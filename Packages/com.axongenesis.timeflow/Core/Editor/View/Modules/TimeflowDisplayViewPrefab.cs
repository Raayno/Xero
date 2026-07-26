// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace AxonGenesis
{
    public class TimeflowDisplayViewPrefab
    {
        private readonly TimeflowViewDisplay Display;
        private readonly PrefabStage PrefabStage;

        private TimeflowViewDisplay.ObjectModes objectMode = TimeflowViewDisplay.ObjectModes.SelectedGroup;
        private List<GameObject> _storedDisplayObjects;
        private int index;
        private bool locked;
        private bool lockedOnly;
        private bool unlockedOnly;
        private bool visibleOnly;
        private bool enabledOnly;
        private bool wasTimeScopeEnabled = false;
        private bool wasTimeScopeLocalized = false;

        public bool IsShowing { get; private set; } = false;

        public TimeflowDisplayViewPrefab(TimeflowViewDisplay display, PrefabStage prefabStage)
        {
            Display = display;
            PrefabStage = prefabStage;
            IsShowing = false;
        }

        public void Show()
        {
            IsShowing = true;
            if (Display.IsPrefabMode) {
                StoreDisplayState();
            }
            GameObject prefabRoot = PrefabStage.prefabContentsRoot;
            if(prefabRoot == null) {
                Debug.LogWarning($"No prefab root found in prefab:{PrefabStage.assetPath}");
                return;
            }

            SelectionUtil.Select(prefabRoot);

            // Get the TimeflowObject from the prefab root
            TimeflowObject mainObj = null;
            if (prefabRoot != null) {
                prefabRoot.TryGetComponent<TimeflowObject>(out mainObj);
            }

            // Check for a Timeflow instance and make it active
            Timeflow tf = ObjectUtil.GetComponentInSelfOrChildren<Timeflow>(Selection.activeGameObject);
            if (tf != null && tf != Timeflow.Active) {
                tf.IsActive = true;
            }
            else tf = Timeflow.Active;

            // Automatically enter time scope moded for prefab
            tf.Display.Timeflow.IsTimeScopeLocalized = true; // force local time for prefabs
            if (mainObj != null) tf.Display.Timeflow.SetTimeScope(mainObj.Track.Keys[0]);

            // Load the selected game objects into view
            if (Selection.activeGameObject != null) {
                tf.Display.DisplaySelectedHierarchy();
            }

            tf.Display.View.FitTime(false, true);
        }

        public void Hide()
        {
            IsShowing = false;
            RestoreDisplayState();
        }

        private void StoreDisplayState()
        {
            // Cache the displayed objects to restore after exiting prefab mode
            if (Display.RootObjects != null && Display.RootObjects.Count > 0) {
                objectMode = Display.ObjectMode;
                _storedDisplayObjects = new List<GameObject>();

                foreach (TimeflowObject obj in Display.RootObjects) {
                    if (obj == null) continue;
                    _storedDisplayObjects.Add(obj.gameObject);
                    if (string.IsNullOrEmpty(Display.Timeflow.name)) Display.Timeflow.name = obj.name;
                }
            }

            index = Display.Index;
            locked = Display.IsLocked;

            lockedOnly = Display.LockedOnly;
            unlockedOnly = Display.UnlockedOnly;
            visibleOnly = Display.VisibleOnly;
            enabledOnly = Display.EnabledOnly;

            Display.IsPrefabMode = true;
            Display.ChannelMode = TimeflowViewDisplay.ChannelModes.None;
            Display.IsLocked = false; // unlock to allow changes

            wasTimeScopeEnabled = Display.Timeflow.IsTimeScopeEnabled;
            wasTimeScopeLocalized = Display.Timeflow.IsTimeScopeLocalized;
        }

        private void RestoreDisplayState()
        {
            // Revert to previous time scope state
            Display.Timeflow.IsTimeScopeEnabled = wasTimeScopeEnabled;
            Display.Timeflow.IsTimeScopeLocalized = wasTimeScopeLocalized;

            Display.IsLocked = false; // unlock to allow changes
            Display.Index = index;

            Display.LockedOnly = lockedOnly;
            Display.UnlockedOnly = unlockedOnly;
            Display.VisibleOnly = visibleOnly;
            Display.EnabledOnly = enabledOnly;

            Display.ObjectMode = objectMode;
            if (objectMode == TimeflowViewDisplay.ObjectModes.UserControlled) {
                Display.DisplayHierarchies(_storedDisplayObjects);
            }

            Display.IsLocked = locked; // restore original lock
        }
    }

}//AxonGenesis
#endif

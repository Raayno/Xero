// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Manages the main menu options displayed under Tool/AxonGenesis for Timeflow based operations. The
    /// Tools menu is mandatory as per the asset store submission guidelines
    /// </summary>
    public static partial class TimeflowMenu
    {

        public static List<string> exportFiles;

        private static int gameObjectCount;

        private static float lastInvokeTime = 0f;

        public static bool IsDuplicateInvoke()
        {
            if (lastInvokeTime == 0 || Time.realtimeSinceStartup > lastInvokeTime) {
                lastInvokeTime = Time.realtimeSinceStartup;
                return false;
            }
            lastInvokeTime = Time.realtimeSinceStartup;
            return true;
        }

        public static GameObject GetSelectedOrNewGameObject(string newName)
        {
            GameObject target = Selection.activeGameObject;

            if (target == null) {
                // Instantiate a new game object
                target = new GameObject(newName);
                UndoUtil.UndoCreate(target, $"Add {newName}");
                if (Timeflow.Active != null) {
                    target.transform.parent = Timeflow.Active.transform;
                    Timeflow.Active.View.Display.AddObjectToDisplay(target);
                }
                ObjectUtil.ResetTransform(target);
                SelectionUtil.Select(target);
            }
            return target;
        }

        public static void CollectDirectoryFiles(string dir, string[] exclude)
        {
            if (Directory.Exists(dir)) {
                int i;
                bool include;
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files) {
                    include = true;
                    if (exclude != null && exclude.Length > 0) {
                        for (i = 0; i < exclude.Length; i++) {
                            if (file.IndexOf(exclude[i]) > -1) {
                                include = false;
                                break;
                            }
                        }
                    }
                    if (include) {
                        exportFiles.Add(file);
                    }
                }
                string[] dirs = Directory.GetDirectories(dir);
                foreach (string d in dirs) {
                    include = true;
                    if (exclude != null && exclude.Length > 0) {
                        for (i = 0; i < exclude.Length; i++) {
                            if (d.IndexOf(exclude[i]) > -1) {
                                include = false;
                                break;
                            }
                        }
                    }
                    if (include) {
                        CollectDirectoryFiles(d, exclude);
                    }
                }
            }
        }

        #region DELEGATES

        public delegate void AddObjectDelegate(GameObject sibling);
        public static AddObjectDelegate OnAddNull = null;
        public static AddObjectDelegate OnAddChild = null;

        public delegate void GroupObjectsDelegate(GameObject obj, GameObject newGroup);
        public static GroupObjectsDelegate OnGroupObjects = null;

        #endregion


        #region ADDBEHAVIOR
        // These menu items are registered by the editors that implement each behavior
        #endregion

        #region ANIMATION-100

        [Shortcut(TimeflowShortcutInfo.Path_AutoKeyframing, KeyCode.K, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kAutoKeyframing + TimeflowMenu.Tab + TimeflowShortcutBindings.AutoKeyframing, false, 300)]
        [UnityEditor.MenuItem(MenuPath2 + kAutoKeyframing + TimeflowMenu.Tab + TimeflowShortcutBindings.AutoKeyframing, false, 300)]
        public static void ToggleAutoKeyframing()
        {
            if (Timeflow.Active == null) return;

            Timeflow.IsAutoKeyframingEnabled = !Timeflow.IsAutoKeyframingEnabled;

            if (Timeflow.Instances != null) {
                foreach (Timeflow t in Timeflow.Instances) {
                    if (t == null) continue;
                    t.AutoKeyframingEnabled = Timeflow.IsAutoKeyframingEnabled;
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kAutoKeyframing + TimeflowMenu.Tab + TimeflowShortcutBindings.AutoKeyframing, true)]
        [UnityEditor.MenuItem(MenuPath2 + kAutoKeyframing + TimeflowMenu.Tab + TimeflowShortcutBindings.AutoKeyframing, true)]
        public static bool ValidateToggleAutoKeyframing()
        {
            return Timeflow.Active != null;
        }

        [UnityEditor.MenuItem(MenuPath + kAddKeyframe + TimeflowMenu.Tab + TimeflowShortcutBindings.AddKeyframe, false, 301)]
        [UnityEditor.MenuItem(MenuPath2 + kAddKeyframe + TimeflowMenu.Tab + TimeflowShortcutBindings.AddKeyframe, false, 301)]
        public static void AddKeyframe()
        {
            if (IsDuplicateInvoke()) return;
            TimeflowCommands.AddKeyframe();
        }

        [UnityEditor.MenuItem(MenuPath + kAddKeyframe + TimeflowMenu.Tab + TimeflowShortcutBindings.AddKeyframe, true)]
        [UnityEditor.MenuItem(MenuPath2 + kAddKeyframe + TimeflowMenu.Tab + TimeflowShortcutBindings.AddKeyframe, true)]
        public static bool ValidateAddKeyframe()
        {
            if (Timeflow.Active == null) return false;
            if (Timeflow.Active.View == null) return false;
            if (Timeflow.Active.View.SelectedChannels == null) return false;

            return true;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddNewTimeflow, KeyCode.N, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kAddNewTimeflow + TimeflowMenu.Tab + TimeflowShortcutBindings.AddNewTimeflow, false, 350)]
        [UnityEditor.MenuItem(MenuPath2 + kAddNewTimeflow + TimeflowMenu.Tab + TimeflowShortcutBindings.AddNewTimeflow, false, 350)]
        public static void AddNewTimeflow()
        {
            if (IsDuplicateInvoke()) return;
            TimeflowEdit.AddTimeflow();
        }

        [Shortcut(TimeflowShortcutInfo.Path_Precompose, KeyCode.P, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kPrecompose + TimeflowMenu.Tab + TimeflowShortcutBindings.Precompose, false, 351)]
        [UnityEditor.MenuItem(MenuPath2 + kPrecompose + TimeflowMenu.Tab + TimeflowShortcutBindings.Precompose, false, 351)]
        public static void Precompose()
        {
            if (IsDuplicateInvoke()) return;
            Timeflow tf = Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<Timeflow>();
            if (tf == null && Selection.activeGameObject == null) tf = Timeflow.Active;
            if (tf == null && Selection.activeGameObject == null) {
                Debug.LogWarning("Please select a GameObject to precompose or a Timeflow instance to add a precomp to.");
                return;
            }
            if (tf != null) {
                TimeflowEdit.AddPrecomp();
            }
            else {
                TimeflowEdit.Precompose();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kPrecompose + TimeflowMenu.Tab + TimeflowShortcutBindings.Precompose, true)]
        [UnityEditor.MenuItem(MenuPath2 + kPrecompose + TimeflowMenu.Tab + TimeflowShortcutBindings.Precompose, true)]
        public static bool ValidatePrecompose()
        {
            if (Selection.activeGameObject == null) return false;
            Timeflow parent = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(Selection.activeGameObject);
            return parent != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_Decompose)]
        [UnityEditor.MenuItem(MenuPath + kDecompose, false, 352)]
        [UnityEditor.MenuItem(MenuPath2 + kDecompose, false, 352)]
        public static void Decompose()
        {
            if (IsDuplicateInvoke()) return;
            TimeflowEdit.Decompose();
        }

        [UnityEditor.MenuItem(MenuPath + kDecompose, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDecompose, true)]
        public static bool ValidateDecompose()
        {
            if (Selection.activeGameObject == null) return false;
            Timeflow parent = Selection.activeGameObject.GetComponent<Timeflow>();
            return parent != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_EnterPrefabPrecompEditMode, KeyCode.KeypadEnter, ShortcutModifiers.Action)]
        [UnityEditor.MenuItem(MenuPath + kEnterEditMode + TimeflowMenu.Tab + TimeflowShortcutBindings.EnterPrefabPrecompEditMode, false, 370)]
        [UnityEditor.MenuItem(MenuPath2 + kEnterEditMode + TimeflowMenu.Tab2 + TimeflowShortcutBindings.EnterPrefabPrecompEditMode, false, 370)]
        public static void EnterPrefabEditMode()
        {
            if (IsDuplicateInvoke()) return;
            GameObject gameObject = Selection.activeGameObject;
            //Debug.Log($"EnterPrefabEditMode:{gameObject.name}", gameObject);  
            bool isPrefab = PrefabUtil.IsPrefabRootInstance(gameObject);

            if (isPrefab) {
                PrefabUtil.OpenPrefab(gameObject);
            }
            else {
                if (gameObject.TryGetComponent<Timeflow>(out var t)) {
                    t.IsActive = true;
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kEnterEditMode + TimeflowMenu.Tab + TimeflowShortcutBindings.EnterPrefabPrecompEditMode, true)]
        [UnityEditor.MenuItem(MenuPath2 + kEnterEditMode + TimeflowMenu.Tab2 + TimeflowShortcutBindings.EnterPrefabPrecompEditMode, true)]
        public static bool ValidateEnterPrefabEditMode()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_ExitPrefabPrecompEditMode, KeyCode.KeypadMinus, ShortcutModifiers.Action)]
        [UnityEditor.MenuItem(MenuPath + kExitEditMode + TimeflowMenu.Tab + TimeflowShortcutBindings.ExitPrefabPrecompEditMode, false, 371)]
        [UnityEditor.MenuItem(MenuPath2 + kExitEditMode, false, 371)]
        public static void ExitPrefabEditMode()
        {
            if (IsDuplicateInvoke()) return;
            if (PrefabUtil.IsEditingPrefab) {
                PrefabUtil.ExitPrefab();
            }
            else
            if (Timeflow.Active != null && Timeflow.Active.TimeflowParent != null) {
                Timeflow.Active.TimeflowParent.IsActive = true;
            }
        }

        [UnityEditor.MenuItem(MenuPath + kExitEditMode + TimeflowMenu.Tab + TimeflowShortcutBindings.ExitPrefabPrecompEditMode, true)]
        [UnityEditor.MenuItem(MenuPath2 + kExitEditMode, true)]
        public static bool ValidateExitPrefabEditMode()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SaveSelectedPrefabs, KeyCode.S, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kSaveSelectedPrefabs + TimeflowMenu.Tab + TimeflowShortcutBindings.SaveSelectedPrefabs, false, 372)]
        [UnityEditor.MenuItem(MenuPath2 + kSaveSelectedPrefabs, false, 372)]
        public static void SavePrefabs()
        {
            if (IsDuplicateInvoke()) return;
            foreach (GameObject obj in Selection.gameObjects) {
                IBehaviorEditor[] behaviors = obj.GetComponents<IBehaviorEditor>();
                if (behaviors != null && behaviors.Length > 0) {
                    for (int i = 0; i < behaviors.Length; i++) {
                        behaviors[i].OnSavePrefab();
                    }
                }

                // Create the directory if it doesn't exist
                string dir = Application.dataPath + TimeflowPreferences.Current.PrefabSavePath;
                dir = dir.Replace("AssetsAssets", "Assets");
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                // Check if the prefab already exists or is new
                UnityEngine.Object original = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (original != null) {
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(original);
                    Debug.Log("Save Existing Prefab: " + obj.name + " : " + path);//--KEEP
                    PrefabUtility.SaveAsPrefabAsset(obj, path);
                }
                else {
                    string path = Path.Combine(TimeflowPreferences.Current.PrefabSavePath, obj.name + ".prefab");
                    Debug.Log("Saved New Prefab: " + obj.name + " : " + path);//--KEEP
                    PrefabUtility.SaveAsPrefabAssetAndConnect(obj, path, InteractionMode.AutomatedAction);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSaveSelectedPrefabs + TimeflowMenu.Tab + TimeflowShortcutBindings.SaveSelectedPrefabs, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSaveSelectedPrefabs, true)]
        public static bool ValidateSavePrefabs()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        #endregion

        #region DISPLAY-400

        public static void ActivateTimeflowForObject(GameObject go)
        {
            if (go == null) return;
            Timeflow timeflow = go.GetComponent<Timeflow>();
            if (timeflow != null) {
                if (timeflow.TimeflowParent != null) {
                    while (timeflow.TimeflowParent != null) {
                        timeflow = timeflow.TimeflowParent;
                    }
                }
                Timeflow.Active = timeflow;
                return;
            }
            timeflow = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(Selection.gameObjects[0]);
            if (timeflow != null) {
                Timeflow.Active = timeflow;
            }
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplayNothing, KeyCode.BackQuote, ShortcutModifiers.Action)]
        [UnityEditor.MenuItem(MenuPath + kDisplayNothing + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayNothing, false, 400)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayNothing, false, 400)]
        public static void DisplayNothing()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.DisplayNothing();
            }
            SelectionUtil.Clear();
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayNothing + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayNothing, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayNothing, true)]
        public static bool ValidateDisplayNothing()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplayEverything, KeyCode.BackQuote, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kDisplayEverything + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayEverything, false, 401)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayEverything, false, 401)]
        public static void DisplayEverything()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.DisplayEverything();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayEverything + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayEverything, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayEverything, true)]
        public static bool ValidateDisplayEverything()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplaySelectedOnly, KeyCode.BackQuote, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kDisplaySelectedOnly + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySelectedOnly, false, 402)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySelectedOnly, false, 402)]
        public static void DisplaySelectedOnly()
        {
            if (IsDuplicateInvoke()) return;
            if (Selection.activeGameObject != null) {
                Timeflow tf = Timeflow.Active;
                if (tf == null) {
                    tf = TimeflowEdit.AddTimeflow(false, TimeflowViewDisplay.ObjectModes.UserControlled);
                }
                if (tf != null) {
                    tf.View.Display.Clear();
                    tf.View.Display.AddSelectedObjectsToDisplay();
                    tf.View.Display.DisplaySelectedHierarchy();
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplaySelectedOnly + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySelectedOnly, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySelectedOnly, true)]
        public static bool ValidateDisplaySelectedOnly()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddSelectedToView, KeyCode.BackQuote, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kDisplayAddSelectedToView + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayAddSelectedToView, false, 403)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayAddSelectedToView, false, 403)]
        public static void DisplaySelectedAdd()
        {
            if (IsDuplicateInvoke()) return;
            if (Selection.activeGameObject != null) {
                ActivateTimeflowForObject(Selection.activeGameObject);
                Timeflow tf = Timeflow.Active;
                if (tf == null) {
                    tf = TimeflowEdit.AddTimeflow(false, TimeflowViewDisplay.ObjectModes.UserControlled);
                }
                if (tf != null) {
                    tf.View.Display.AddSelectedObjectsToDisplay();
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayAddSelectedToView + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayAddSelectedToView, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayAddSelectedToView, true)]
        public static bool ValidateDisplaySelectedAdd()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_ActiveSelectionGrouped, KeyCode.BackQuote, ShortcutModifiers.Alt | ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        [UnityEditor.MenuItem(MenuPath + kDisplayActiveSelectionGrouped + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayActiveSelectionGrouped, false, 420)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayActiveSelectionGrouped, false, 420)]
        public static void DisplayActiveSelection()
        {
            if (IsDuplicateInvoke()) return;
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                Timeflow timeflow = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(Selection.gameObjects[0]);
                if (timeflow != null) {
                    Timeflow.Active = timeflow;
                }
                if (Timeflow.Active != null) {
                    Timeflow.Active.View.Display.DisplaySelectedGroup();
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayActiveSelectionGrouped + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayActiveSelectionGrouped, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayActiveSelectionGrouped, true)]
        public static bool ValidateDisplayActiveSelection()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplaySelectedObject)]
        [UnityEditor.MenuItem(MenuPath + kDisplaySelectedObject, false, 421)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySelectedObject, false, 421)]
        public static void DisplayActiveSelectionObject()
        {
            if (IsDuplicateInvoke()) return;
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                Timeflow timeflow = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(Selection.gameObjects[0]);
                if (timeflow != null) {
                    Timeflow.Active = timeflow;
                }
                if (Timeflow.Active != null) {
                    Timeflow.Active.View.Display.DisplaySelectedObject();
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplaySelectedObject, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySelectedObject, true)]
        public static bool ValidateDisplayActiveSelectionObject()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SoloSelected, KeyCode.S, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kDisplaySoloSelected + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySoloSelected, false, 440)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySoloSelected, false, 440)]
        public static void DisplaySoloSelected()
        {
            DisplaySolo(false);
        }

        [UnityEditor.MenuItem(MenuPath + kDisplaySoloSelected + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySoloSelected, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySoloSelected, true)]
        public static bool ValidateDisplaySoloSelected()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SoloSelectedAppend, KeyCode.S, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kDisplaySoloSelectedAppend + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySoloSelectedAppend, false, 441)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySoloSelectedAppend, false, 441)]
        public static void DisplaySoloSelectedAdd()
        {
            DisplaySolo(true);
        }

        [UnityEditor.MenuItem(MenuPath + kDisplaySoloSelectedAppend + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplaySoloSelectedAppend, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplaySoloSelectedAppend, true)]
        public static bool ValidateDisplaySoloSelectedAdd()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        public static void DisplaySolo(bool additive)
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;

            if (Selection.activeGameObject == null) {
                // When nothing is selected, simply toggle solo mode
                Timeflow.IsSoloMode = !Timeflow.IsSoloMode;
                Timeflow.Active.Refresh();
            }
            else {
                TimeflowObject tobj = null;
                if (Selection.activeGameObject != null) Selection.activeGameObject.TryGetComponent<TimeflowObject>(out tobj);
                bool show = true;

                if (!additive) {
                    // Remove all soloed objects from the display
                    if (Timeflow.Active.Display.Objects != null) {
                        foreach (TimeflowObject t in Timeflow.Active.Display.Objects) {
                            t.DisplaySoloWithChannels(false);
                        }
                    }
                }

                //Debug.Log($"show:{show} Selection:{(Selection.gameObjects == null ? "NULL" : Selection.gameObjects.Length)}");
                if (show) Timeflow.Active.Display.AddSelectedObjectsToDisplay();

                if (Selection.gameObjects != null) {
                    foreach (GameObject go in Selection.gameObjects) {
                        TimeflowObject t;
                        if (go.TryGetComponent<TimeflowObject>(out t)) {
                            t.DisplaySoloWithChannels(show);
                        }
                    }
                }
                if (tobj == null) {
                    tobj = Timeflow.Active.Display.AddObjectToDisplay(Selection.activeGameObject);
                }
                tobj.DisplaySoloWithChannels(show);

                if (show) {
                    // Scroll to top of view
                    Timeflow.Active.View.ScrollOffset = new Vector2(Timeflow.Active.View.ScrollOffset.x, 0);
                }
                Timeflow.IsSoloMode = show;
                Timeflow.Active.Refresh();
            }
        }

        public static bool ValidateDisplaySolo()
        {
            return Timeflow.Active != null;
        }


        [Shortcut(TimeflowShortcutInfo.Path_ToggleHidden, KeyCode.BackQuote, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kDisplayToggleHidden + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayToggleHidden, false, 460)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayToggleHidden + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayToggleHidden, false, 460)]
        public static void ToggleHidden()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active != null) {
                UndoUtil.Undo(Timeflow.Active, "Hide Objects In Timeflow View", true);
                Timeflow.Active.View.Display.ToggleObjectsHiddenInDisplay(Selection.gameObjects);
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayToggleHidden + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayToggleHidden, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayToggleHidden + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayToggleHidden, true)]
        public static bool ValidateToggleHidden()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplayPrevious, KeyCode.LeftBracket, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kDisplayPrevious + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayPrevious, false, 480)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayPrevious + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayPrevious, false, 480)]
        public static void ShowRecentPrev()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.Previous();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayPrevious + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayPrevious, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayPrevious + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayPrevious, true)]
        public static bool ValidateShowRecentPrev()
        {
            return Timeflow.Active != null && Timeflow.Active.Displays != null && Timeflow.Active.Displays.Count > 1;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisplayNext, KeyCode.RightBracket, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kDisplayNext + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayNext, false, 481)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayNext + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayNext, false, 481)]
        public static void ShowRecentNext()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.Next();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDisplayNext + TimeflowMenu.Tab + TimeflowShortcutBindings.DisplayNext, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDisplayNext + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisplayNext, true)]
        public static bool ValidateShowRecentNext()
        {
            return Timeflow.Active != null && Timeflow.Active.Displays != null && Timeflow.Active.Displays.Count > 1;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToFullDuration, KeyCode.Alpha0, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToFullDuration + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToFullDuration, false, 500)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToFullDuration + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToFullDuration, false, 500)]
        public static void JumpToFullDuration()
        {
            if (IsDuplicateInvoke()) return;
            Timeflow.Active.Markers.GotoMarker(-1);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToFullDuration + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToFullDuration, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToFullDuration + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToFullDuration, true)]
        public static bool ValidateJumpToFullDuration()
        {
            return Timeflow.Active != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker1, KeyCode.Alpha1, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker1 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker1, false, 501)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker1 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker1, false, 501)]
        public static void JumpToMarker1()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(0);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker1 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker1, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker1 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker1, true)]
        public static bool ValidateJumpToMarker1()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 1;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker2, KeyCode.Alpha2, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker2 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker2, false, 502)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker2 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker2, false, 502)]
        public static void JumpToMarker2()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(1);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker2 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker2, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker2 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker2, true)]
        public static bool ValidateJumpToMarker2()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 2;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker3, KeyCode.Alpha3, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker3 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker3, false, 503)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker3 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker3, false, 503)]
        public static void JumpToMarker3()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(2);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker3 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker3, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker3 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker3, true)]
        public static bool ValidateJumpToMarker3()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 3;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker4, KeyCode.Alpha4, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker4 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker4, false, 504)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker4 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker4, false, 504)]
        public static void JumpToMarker4()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(3);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker4 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker4, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker4 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker4, true)]
        public static bool ValidateJumpToMarker4()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 4;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker5, KeyCode.Alpha5, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker5 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker5, false, 505)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker5 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker5, false, 505)]
        public static void JumpToMarker5()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(4);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker5 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker5, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker5 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker5, true)]
        public static bool ValidateJumpToMarker5()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 5;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker6, KeyCode.Alpha6, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker6 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker6, false, 506)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker6 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker6, false, 506)]
        public static void JumpToMarker6()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(5);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker6 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker6, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker6 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker6, true)]
        public static bool ValidateJumpToMarker6()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 6;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker7, KeyCode.Alpha7, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker7 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker7, false, 507)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker7 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker7, false, 507)]
        public static void JumpToMarker7()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(6);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker7 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker7, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker7 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker7, true)]
        public static bool ValidateJumpToMarker7()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 7;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker8, KeyCode.Alpha8, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker8 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker8, false, 508)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker8 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker8, false, 508)]
        public static void JumpToMarker8()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(7);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker8 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker8, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker8 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker8, true)]
        public static bool ValidateJumpToMarker8()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 8;
        }

        [Shortcut(TimeflowShortcutInfo.Path_JumpToMarker9, KeyCode.Alpha9, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kJumpToMarker9 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker9, false, 509)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker9 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker9, false, 509)]
        public static void JumpToMarker9()
        {
            if (IsDuplicateInvoke()) return;
            if (Timeflow.Active == null) return;
            Timeflow.Active.Markers.GotoMarker(8);
        }

        [UnityEditor.MenuItem(MenuPath + kJumpToMarker9 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker9, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJumpToMarker9 + TimeflowMenu.Tab + TimeflowShortcutBindings.JumpToMarker9, true)]
        public static bool ValidateJumpToMarker9()
        {
            return Timeflow.Active != null && Timeflow.Active.MarkerList != null && Timeflow.Active.MarkerList.Count >= 9;
        }


        [Shortcut(TimeflowShortcutInfo.Path_DestroyAllTimeflowBehaviors)]
        [UnityEditor.MenuItem(MenuPath + kDestroyAllTimeflowBehaviors, false, 580)]
        [UnityEditor.MenuItem(MenuPath2 + kDestroyAllTimeflowBehaviors, false, 580)]
        public static void RemoveTimeflowObjects() => TimeflowObject.RemoveTimeflowObjects();

        [UnityEditor.MenuItem(MenuPath + kDestroyAllTimeflowBehaviors, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDestroyAllTimeflowBehaviors, true)]
        public static bool ValidateRemoveTimeflowObjects() => TimeflowObject.ValidateRemoveTimeflowObjects();

        [Shortcut(TimeflowShortcutInfo.Path_DeleteChildren)]
        [UnityEditor.MenuItem(MenuPath + kDeleteChildren, false, 600)]
        [UnityEditor.MenuItem(MenuPath2 + kDeleteChildren, false, 600)]
        public static void DeleteChildren()
        {
            List<GameObject> toDelete = new List<GameObject>();
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            foreach (Transform obj in transforms) {
                if (obj.childCount > 0) {
                    foreach (Transform child in obj) {
                        toDelete.Add(child.gameObject);
                    }
                }
            }
            if (toDelete.Count > 0) {
                foreach (GameObject obj in toDelete) {
                    UndoUtil.UndoDestroy(obj);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDeleteChildren, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDeleteChildren, true)]
        public static bool ValidateDeleteChildren()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SortChildren)]
        [UnityEditor.MenuItem(MenuPath + kSortChildren, false, 601)]
        [UnityEditor.MenuItem(MenuPath2 + kSortChildren, false, 601)]
        public static void SortChildren()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.SortChildrenByName(obj);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSortChildren, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSortChildren, true)]
        public static bool ValidateSortChildren()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SortChildrenReverse)]
        [UnityEditor.MenuItem(MenuPath + kSortChildrenReverse, false, 602)]
        [UnityEditor.MenuItem(MenuPath2 + kSortChildrenReverse, false, 602)]
        public static void SortChildrenReverse()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.SortChildrenByNameReverse(obj);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSortChildrenReverse, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSortChildrenReverse, true)]
        public static bool ValidateSortChildrenReverse()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_HideChildrenInHierarchy)]
        [UnityEditor.MenuItem(MenuPath + kHideChildrenInHierarchy, false, 603)]
        [UnityEditor.MenuItem(MenuPath2 + kHideChildrenInHierarchy, false, 603)]
        public static void HideChildrenInHierarchy()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.ShowChildrenInHierarchy(obj, false);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kHideChildrenInHierarchy, true)]
        [UnityEditor.MenuItem(MenuPath2 + kHideChildrenInHierarchy, true)]
        public static bool ValidateHideChildrenInHierarchy()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_ShowChildrenInHierarchy)]
        [UnityEditor.MenuItem(MenuPath + kShowChildrenInHierarchy, false, 604)]
        [UnityEditor.MenuItem(MenuPath2 + kShowChildrenInHierarchy, false, 604)]
        public static void ShowChildrenInHierarchy()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.ShowChildrenInHierarchy(obj, true);
                }
            }
        }

        public static void SetAsFirstSibling()
        {
            if (Selection.activeGameObject == null) return;
            Selection.activeGameObject.transform.SetAsFirstSibling();
            if (Selection.activeGameObject.TryGetComponent<TimeflowObject>(out var obj)) {
                obj.SortOrder = 0;
            }
            if (Timeflow.Active != null) Timeflow.Active.Refresh(true);
        }

        public static void SetAsLastSibling()
        {
            if (Selection.activeGameObject == null) return;
            Selection.activeGameObject.transform.SetAsLastSibling();
            if (Selection.activeGameObject.TryGetComponent<TimeflowObject>(out var obj)) {
                obj.SortOrder = 9999999;
            }
            if (Timeflow.Active != null) Timeflow.Active.Refresh(true);
        }

        [UnityEditor.MenuItem(MenuPath + kShowChildrenInHierarchy, true)]
        [UnityEditor.MenuItem(MenuPath2 + kShowChildrenInHierarchy, true)]
        public static bool ValidateShowChildrenInHierarchy()
        {
            return Selection.activeGameObject != null;
        }

        public static void GroupObjects(string appendName, bool matchTransform)
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            if (transforms == null || transforms.Length == 0 || transforms[0] == null) return;
            UndoUtil.Undo(Timeflow.Active, "Group Objects", true);

            //Debug.Log($"Grouping {transforms.Length} objects with name '{appendName}' and matchTransform={matchTransform}");
            /// Make a copy of the list and sort it because the selection array isn't sorted correctly
            List<Transform> selected = new List<Transform>();
            foreach (Transform t in transforms) {
                UndoUtil.Undo(t.gameObject, "Group Objects");
                selected.Add(t);
            }
            selected.Sort((Transform t1, Transform t2) => { return t1.GetSiblingIndex().CompareTo(t2.GetSiblingIndex()); });

            GameObject group = GroupObjects(selected, appendName, matchTransform);
            SelectionUtil.Select(group);
        }

        public static GameObject GroupObjects(List<Transform> transforms, string appendName, bool matchTransform)
        {
            int siblingIndex = transforms[0].GetSiblingIndex();
            GameObject group = new GameObject(transforms[0].name);

            if (!string.IsNullOrEmpty(appendName) && !group.name.Contains(appendName))
                group.name = group.name + appendName;

            UndoUtil.UndoCreate(group, "Group Objects");
            Transform grp = group.transform;

            TimeflowObject tobj = null;
            if (transforms[0].TryGetComponent<TimeflowObject>(out tobj)) {
                TimeflowObject tgrp = group.GetComponent<TimeflowObject>();
                if (tgrp == null) tgrp = group.AddComponent<TimeflowObject>();
                tgrp.Track.GUIColor = tobj.Track.GUIColor;
            }

            Transform originalParent = transforms[0].parent;
            if (originalParent) {
                Undo.SetTransformParent(grp, originalParent, "Group Objects");
            }
            if (matchTransform) {
                grp.localPosition = transforms[0].localPosition;
                grp.localRotation = transforms[0].localRotation;
                grp.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else {
                ObjectUtil.ResetTransform(grp);
            }
            grp.gameObject.layer = transforms[0].gameObject.layer;
            grp.SetSiblingIndex(siblingIndex);

            bool isInView = false;
            if (Timeflow.Active != null && Timeflow.Active.View.Display.IsObjectDisplayed(transforms[0].gameObject)) {
                isInView = true;
            }
            if (transforms.Count == 1) {
                Undo.SetTransformParent(transforms[0], grp, "Group Objects");
            }
            else {
                foreach (Transform transform in transforms) {
                    grp.gameObject.layer = transform.gameObject.layer;
                    Undo.SetTransformParent(transform, grp, "Group Objects");
                }
            }
            if (isInView) {
                Timeflow.Active.View.Display.AddObjectToDisplay(group);
            }
            if (OnGroupObjects != null) OnGroupObjects(transforms[0].gameObject, group);
            return group;
        }

        [Shortcut(TimeflowShortcutInfo.Path_Group, KeyCode.G, ShortcutModifiers.Action)]
        [UnityEditor.MenuItem(MenuPath + kGroup + TimeflowMenu.Tab + TimeflowShortcutBindings.Group, false, 620)]
        [UnityEditor.MenuItem(MenuPath2 + kGroup + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Group, false, 620)]
        public static void GroupObjects() => GroupObjects(" Group", true);

        [UnityEditor.MenuItem(MenuPath + kGroup + TimeflowMenu.Tab + TimeflowShortcutBindings.Group, true)]
        [UnityEditor.MenuItem(MenuPath2 + kGroup + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Group, true)]
        public static bool ValidateGroupObjects()
        {
            return Selection.activeTransform;
        }

        // Already defined
        [Shortcut(TimeflowShortcutInfo.Path_Ungroup, KeyCode.U, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kUngroup + TimeflowMenu.Tab + TimeflowShortcutBindings.Ungroup, false, 621)]
        [UnityEditor.MenuItem(MenuPath2 + kUngroup + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Ungroup, false, 621)]
        public static void UngroupObjects()
        {
            if (Selection.transforms == null) return;
            List<GameObject> children = new List<GameObject>();

            foreach (Transform t in Selection.transforms) {
                if (t.childCount > 0) {
                    foreach (Transform child in t) {
                        children.Add(child.gameObject);
                    }
                }
            }

            if (children.Count > 0) {
                List<GameObject> displayed = new List<GameObject>();
                foreach (GameObject child in children) {
                    UndoUtil.Undo(child, "Ungroup Objects", true);
                    if (Timeflow.Active != null && Timeflow.Active.View.Display.IsObjectDisplayed(child)) {
                        displayed.Add(child);
                    }
                    Transform parent = child.transform.parent == null ? null : child.transform.parent.parent;
                    Undo.SetTransformParent(child.transform, parent, "Ungroup Objects");
                }

                if (Timeflow.Active != null && displayed.Count > 0) {
                    Timeflow.Active.View.Display.RemoveObjectsFromDisplay(displayed.ToArray());

                    foreach (GameObject obj in displayed) {
                        Timeflow.Active.View.Display.AddObjectToDisplay(obj);
                    }
                }
            }
            else {
                Debug.LogWarning("To ungroup objects, select the parent object of the group.");
            }
        }

        [UnityEditor.MenuItem(MenuPath + kUngroup + TimeflowMenu.Tab + TimeflowShortcutBindings.Ungroup, true)]
        [UnityEditor.MenuItem(MenuPath2 + kUngroup + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Ungroup, true)]
        public static bool ValidateUngroupObjects()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_Unparent)]
        [UnityEditor.MenuItem(MenuPath + kUnparent, false, 622)]
        [UnityEditor.MenuItem(MenuPath2 + kUnparent, false, 622)]
        public static void UnparentObjects()
        {
            if (Selection.gameObjects == null) return;
            foreach (GameObject obj in Selection.gameObjects) {
                UndoUtil.Undo(obj, "Unparent Objects", true);
                obj.transform.parent = obj.transform.parent == null ? null : obj.transform.parent.parent;
            }
        }

        [UnityEditor.MenuItem(MenuPath + kUnparent, true)]
        [UnityEditor.MenuItem(MenuPath2 + kUnparent, true)]
        public static bool ValidateUnparentObjects()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_Flatten)]
        [UnityEditor.MenuItem(MenuPath + kFlatten, false, 623)]
        [UnityEditor.MenuItem(MenuPath2 + kFlatten, false, 623)]
        public static void FlattenObjectHierarchy()
        {
            if (Selection.gameObjects == null) return;
            foreach (GameObject obj in Selection.gameObjects) {
                UndoUtil.Undo(obj, "Flatten Object Hierarchy", true);
                obj.transform.parent = null;
            }
        }

        [UnityEditor.MenuItem(MenuPath + kFlatten, true)]
        [UnityEditor.MenuItem(MenuPath2 + kFlatten, true)]

        public static bool ValidateFlattenObjectHierarchy()
        {
            return Selection.activeTransform;
        }


        [Shortcut(TimeflowShortcutInfo.Path_RemoveNumbering)]
        [UnityEditor.MenuItem(MenuPath + kRemoveNumbering, false, 624)]
        [UnityEditor.MenuItem(MenuPath2 + kRemoveNumbering, false, 624)]
        public static void RemoveNumbering()
        {
            if (Selection.gameObjects == null) return;
            StringUtil.RemoveNumbersFromNames(Selection.gameObjects);
        }

        [UnityEditor.MenuItem(MenuPath + kRemoveNumbering, true)]
        [UnityEditor.MenuItem(MenuPath2 + kRemoveNumbering, true)]

        public static bool ValidateRemoveNumbering()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_GetRendererSize)]
        [UnityEditor.MenuItem(MenuPath + kGetRendererSize, false, 650)]
        [UnityEditor.MenuItem(MenuPath2 + kGetRendererSize, false, 650)]
        public static void GetRendererSize()
        {
            AxonTools.GetRendererSize();
        }

        [UnityEditor.MenuItem(MenuPath + kGetRendererSize, true)]
        [UnityEditor.MenuItem(MenuPath2 + kGetRendererSize, true)]
        public static bool ValidateGetRendererSize()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_GetBoundingBox)]
        [UnityEditor.MenuItem(MenuPath + kGetBoundingBox, false, 651)]
        [UnityEditor.MenuItem(MenuPath2 + kGetBoundingBox, false, 651)]
        public static void GetBoundingBox()
        {
            AxonTools.GetBoundingBox();
        }

        [UnityEditor.MenuItem(MenuPath + kGetBoundingBox, true)]
        [UnityEditor.MenuItem(MenuPath2 + kGetBoundingBox, true)]
        public static bool ValidateGetBoundingBox()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_GetPolycount)]
        [UnityEditor.MenuItem(MenuPath + kGetPolycount, false, 652)]
        [UnityEditor.MenuItem(MenuPath2 + kGetPolycount, false, 652)]
        public static void GetMeshPolycount()
        {
            MeshUtil.GetMeshPolycount(Selection.activeGameObject);
        }

        [UnityEditor.MenuItem(MenuPath + kGetPolycount, true)]
        [UnityEditor.MenuItem(MenuPath2 + kGetPolycount, true)]
        public static bool ValidateGetMeshPolycount()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_FreezeMesh)]
        [UnityEditor.MenuItem(MenuPath + kFreezeMesh, false, 653)]
        [UnityEditor.MenuItem(MenuPath2 + kFreezeMesh, false, 653)]
        public static void FreezeMesh()
        {
            MeshUtil.FreezeMesh(Selection.activeGameObject);
        }

        [UnityEditor.MenuItem(MenuPath + kFreezeMesh, true)]
        [UnityEditor.MenuItem(MenuPath2 + kFreezeMesh, true)]
        public static bool ValidateFreezeMesh()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_CombineMeshes)]
        [UnityEditor.MenuItem(MenuPath + kCombineMeshes, false, 654)]
        [UnityEditor.MenuItem(MenuPath2 + kCombineMeshes, false, 654)]
        public static void CombineMeshs()
        {
            MeshUtil.CombineMeshes(Selection.gameObjects);
        }

        [UnityEditor.MenuItem(MenuPath + kCombineMeshes, true)]
        [UnityEditor.MenuItem(MenuPath2 + kCombineMeshes, true)]
        public static bool ValidateCombineMeshs()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }
        
        [UnityEditor.MenuItem(MenuPath + kDeselectAll + TimeflowMenu.Tab + TimeflowShortcutBindings.DeselectAll, false, 700)]
        [UnityEditor.MenuItem(MenuPath2 + kDeselectAll + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DeselectAll, false, 700)]
        public static void Deselect()
        {
            SelectionUtil.Clear();
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kDeselectAll + TimeflowMenu.Tab + TimeflowShortcutBindings.DeselectAll, true)]
        [UnityEditor.MenuItem(MenuPath2 + kDeselectAll + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DeselectAll, true)]
        public static bool ValidateDeselect()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectChildren, KeyCode.Minus, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kSelectChildren + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectChildren, false, 701)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectChildren + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectChildren, false, 701)]
        public static void SelectChildren()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            List<UnityEngine.Object> newSelection = new List<UnityEngine.Object>();
            foreach (Transform obj in transforms) {
                if (EditorInput.IsShift) {
                    newSelection.Add(obj.gameObject);
                }
                if (obj.childCount > 0) {
                    foreach (Transform child in obj) {
                        newSelection.Add(child.gameObject);
                    }
                }
            }
            SelectionUtil.Select(newSelection.ToArray() as UnityEngine.Object[]);
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSelectChildren + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectChildren, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectChildren + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectChildren, true)]
        public static bool ValidateSelectChildren()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectDescendants, KeyCode.Minus, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kSelectDescendants + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectDescendants, false, 702)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectDescendants + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectDescendants, false, 702)]
        public static void SelectDescendants()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            List<UnityEngine.Object> newSelection = new List<UnityEngine.Object>();
            foreach (Transform obj in transforms) {
                SelectDescendantsRecursive(ref newSelection, obj);
            }
            SelectionUtil.Select(newSelection.ToArray() as UnityEngine.Object[]);
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        public static void SelectDescendantsRecursive(ref List<UnityEngine.Object> newSelection, Transform parent)
        {
            newSelection.Add(parent.gameObject);

            if (parent.transform.childCount > 0) {
                foreach (Transform child in parent.transform) {
                    SelectDescendantsRecursive(ref newSelection, child);
                }
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSelectDescendants + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectDescendants, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectDescendants + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectDescendants, true)]
        public static bool ValidateSelectDescendants()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectParents, KeyCode.Plus, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kSelectParents + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectParents, false, 703)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectParents + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectParents, false, 703)]
        public static void SelectParents()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            List<UnityEngine.Object> newSelection = new List<UnityEngine.Object>();
            foreach (Transform obj in transforms) {
                if (obj.parent != null) {
                    newSelection.Add(obj.parent.gameObject);
                }
            }
            SelectionUtil.Select(newSelection.ToArray() as UnityEngine.Object[]);
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSelectParents + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectParents, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectParents + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectParents, true)]
        public static bool ValidateSelectParents()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectAncestors, KeyCode.Plus, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kSelectAncestors + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectAncestors, false, 704)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectAncestors + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectAncestors, false, 704)]
        public static void SelectAncestors()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            List<UnityEngine.Object> newSelection = new List<UnityEngine.Object>();
            foreach (Transform obj in transforms) {
                Transform parent = obj.parent;
                while (parent != null) {
                    newSelection.Add(parent.gameObject);
                    parent = parent.parent;
                }
            }
            SelectionUtil.Select(newSelection.ToArray() as UnityEngine.Object[]);
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                Timeflow.Active.View.OnSelectionChange();
            }
        }

        [UnityEditor.MenuItem(MenuPath + kSelectAncestors + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectAncestors, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectAncestors + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectAncestors, true)]
        public static bool ValidateSelectAncestors()
        {
            return Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectRenderersRecursive)]
        [UnityEditor.MenuItem(MenuPath + kSelectRenderersRecursive, false, 705)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectRenderersRecursive, false, 705)]
        public static void SelectRenderersRecursive()
        {
            AxonTools.SelectRenderersRecursive();
        }

        [UnityEditor.MenuItem(MenuPath + kSelectRenderersRecursive, true)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectRenderersRecursive, true)]
        public static bool ValidateSelectRenderersRecursive()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectMainCamera, KeyCode.M, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kSelectMainCamera + TimeflowMenu.Tab + TimeflowShortcutBindings.SelectMainCamera, false, 720)]
        [UnityEditor.MenuItem(MenuPath2 + kSelectMainCamera + TimeflowMenu.Tab2 + TimeflowShortcutBindings.SelectMainCamera, false, 720)]
        public static void SelectMainCamera()
        {
            if (Camera.main != null) {
                SelectionUtil.Select(Camera.main.gameObject);
            }
            else {
                Debug.LogWarning("There is no main camera instance in the scene. Please make sure a camera exists and is tagged as MainCamera.");
            }
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject1, KeyCode.F1, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject1 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject1, false, 740)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject1 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject1, false, 740)]
        public static void QuickSelect1()
        {
            Timeflow.QuickSelect(0);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject1 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject1, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject1 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject1, true)]
        public static bool ValidateQuickSelect1()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[0] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject2, KeyCode.F2, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject2 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject2, false, 741)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject2 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject2, false, 741)]
        public static void QuickSelect2()
        {
            Timeflow.QuickSelect(1);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject2 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject2, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject2 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject2, true)]
        public static bool ValidateQuickSelect2()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[1] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject3, KeyCode.F3, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject3 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject3, false, 742)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject3 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject3, false, 742)]
        public static void QuickSelect3()
        {
            Timeflow.QuickSelect(2);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject3 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject3, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject3 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject3, true)]
        public static bool ValidateQuickSelect3()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[2] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject4, KeyCode.F4, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject4 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject4, false, 743)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject4 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject4, false, 743)]
        public static void QuickSelect4()
        {
            Timeflow.QuickSelect(3);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject4 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject4, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject4 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject4, true)]
        public static bool ValidateQuickSelect4()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[3] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject5, KeyCode.F5, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject5 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject5, false, 744)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject5 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject5, false, 744)]
        public static void QuickSelect5()
        {
            Timeflow.QuickSelect(4);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject5 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject5, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject5 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject5, true)]
        public static bool ValidateQuickSelect5()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[4] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject6, KeyCode.F6, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject6 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject6, false, 745)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject6 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject6, false, 745)]
        public static void QuickSelect6()
        {
            Timeflow.QuickSelect(5);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject6 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject6, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject6 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject6, true)]
        public static bool ValidateQuickSelect6()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[5] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject7, KeyCode.F7, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject7 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject7, false, 746)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject7 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject7, false, 746)]
        public static void QuickSelect7()
        {
            Timeflow.QuickSelect(6);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject7 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject7, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject7 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject7, true)]
        public static bool ValidateQuickSelect7()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[6] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject8, KeyCode.F8, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject8 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject8, false, 747)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject8 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject8, false, 747)]
        public static void QuickSelect8()
        {
            Timeflow.QuickSelect(7);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject8 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject8, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject8 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject8, true)]
        public static bool ValidateQuickSelect8()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[7] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject9, KeyCode.F9, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject9 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject9, false, 748)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject9 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject9, false, 748)]
        public static void QuickSelect9()
        {
            Timeflow.QuickSelect(8);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject9 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject9, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject9 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject9, true)]
        public static bool ValidateQuickSelect9()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[8] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject10, KeyCode.F10, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject10 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject10, false, 749)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject10 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject10, false, 749)]
        public static void QuickSelect10()
        {
            Timeflow.QuickSelect(9);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject10 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject10, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject10 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject10, true)]
        public static bool ValidateQuickSelect10()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[9] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject11, KeyCode.F11, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject11 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject11, false, 750)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject11 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject11, false, 750)]
        public static void QuickSelect11()
        {
            Timeflow.QuickSelect(10);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject11 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject11, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject11 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject11, true)]
        public static bool ValidateQuickSelect11()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[10] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectObject12, KeyCode.F12, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject12 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject12, false, 751)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject12 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject12, false, 751)]
        public static void QuickSelect12()
        {
            Timeflow.QuickSelect(11);
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectObject12 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectObject12, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectObject12 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectObject12, true)]
        public static bool ValidateQuickSelect12()
        {
            return Timeflow.Active != null && Timeflow.Active.QuickSelectObjects != null && Timeflow.Active.QuickSelectObjects[11] != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject1, KeyCode.F1, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject1 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject1, false, 770)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject1 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject1, false, 770)]
        public static void QuickSelect1Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[0] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F1:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject1 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject1, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject1 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject1, true)]
        public static bool ValidateQuickSelect1Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject2, KeyCode.F2, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject2 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject2, false, 771)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject2 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject2, false, 771)]
        public static void QuickSelect2Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[1] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F2:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject2 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject2, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject2 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject2, true)]
        public static bool ValidateQuickSelect2Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject3, KeyCode.F3, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject3 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject3, false, 772)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject3 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject3, false, 772)]
        public static void QuickSelect3Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[2] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F3:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject3 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject3, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject3 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject3, true)]
        public static bool ValidateQuickSelect3Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject4, KeyCode.F4, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject4 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject4, false, 773)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject4 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject4, false, 773)]
        public static void QuickSelect4Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[3] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F4:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject4 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject4, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject4 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject4, true)]
        public static bool ValidateQuickSelect4Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject5, KeyCode.F5, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject5 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject5, false, 774)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject5 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject5, false, 774)]
        public static void QuickSelect5Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[4] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F5:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject5 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject5, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject5 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject5, true)]
        public static bool ValidateQuickSelect5Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject6, KeyCode.F6, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject6 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject6, false, 775)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject6 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject6, false, 775)]
        public static void QuickSelect6Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[5] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F6:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject6 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject6, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject6 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject6, true)]
        public static bool ValidateQuickSelect6Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject7, KeyCode.F7, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject7 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject7, false, 776)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject7 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject7, false, 776)]
        public static void QuickSelect7Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[6] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F7:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject7 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject7, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject7 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject7, true)]
        public static bool ValidateQuickSelect7Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject8, KeyCode.F8, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject8 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject8, false, 777)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject8 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject8, false, 777)]
        public static void QuickSelect8Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[7] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F8:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject8 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject8, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject8 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject8, true)]
        public static bool ValidateQuickSelect8Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject9, KeyCode.F9, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject9 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject9, false, 778)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject9 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject9, false, 778)]
        public static void QuickSelect9Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[8] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F9:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject9 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject9, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject9 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject9, true)]
        public static bool ValidateQuickSelect9Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject10, KeyCode.F10, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject10 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject10, false, 779)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject10 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject10, false, 779)]
        public static void QuickSelect10Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[9] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F10:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject10 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject10, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject10 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject10, true)]
        public static bool ValidateQuickSelect10Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject11, KeyCode.F11, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject11 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject11, false, 780)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject11 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject11, false, 780)]
        public static void QuickSelect11Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[10] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F11:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject11 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject11, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject11 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject11, true)]
        public static bool ValidateQuickSelect11Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        [Shortcut(TimeflowShortcutInfo.Path_QuickSelectAssignObject12, KeyCode.F12, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject12 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject12, false, 781)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject12 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject12, false, 781)]
        public static void QuickSelect12Assign()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.QuickSelectObjects == null) Timeflow.Active.QuickSelectObjects = new GameObject[12];
            Timeflow.Active.QuickSelectObjects[11] = Selection.activeGameObject;
            Debug.Log("Assigned Quick Select Object F12:" + Selection.activeGameObject.name);//--KEEP  
        }

        [UnityEditor.MenuItem(MenuPath + kQuickSelectAssignObject12 + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickSelectAssignObject12, true)]
        [UnityEditor.MenuItem(MenuPath2 + kQuickSelectAssignObject12 + TimeflowMenu.Tab2 + TimeflowShortcutBindings.QuickSelectAssignObject12, true)]
        public static bool ValidateQuickSelect12Assign()
        {
            return Timeflow.Active != null && Selection.activeGameObject != null;
        }

        #endregion // SELECT

        #region TRACKS-750

        public static void ResetSelectedTracksFullLength() => TimeflowObject.ResetSelectedTracksFullLength();

        [UnityEditor.MenuItem(MenuPath + kResetTracksSelected, true)]
        [UnityEditor.MenuItem(MenuPath2 + kResetTracksSelected, true)]
        public static bool ValidateResetSelectedTracksFullLength() => TimeflowObject.ValidateResetSelectedTracksFullLength();

        [UnityEditor.MenuItem(MenuPath + kResetAllTracks, false, 751)]
        [UnityEditor.MenuItem(MenuPath2 + kResetAllTracks, false, 751)]
        public static void ResetAllTracksFullLength() => TimeflowObject.ResetAllTracksFullLength();

        [UnityEditor.MenuItem(MenuPath + kJoinAdjacentTracks + TimeflowMenu.Tab + TimeflowShortcutBindings.JoinSelectedTracks, false, 752)]
        [UnityEditor.MenuItem(MenuPath2 + kJoinAdjacentTracks + TimeflowMenu.Tab2 + TimeflowShortcutBindings.JoinSelectedTracks, false, 752)]
        public static void JoinAdjacentTracks() => TimeflowObject.JoinAdjacentTracks();

        [UnityEditor.MenuItem(MenuPath + kJoinAdjacentTracks + TimeflowMenu.Tab + TimeflowShortcutBindings.JoinSelectedTracks, true)]
        [UnityEditor.MenuItem(MenuPath2 + kJoinAdjacentTracks + TimeflowMenu.Tab2 + TimeflowShortcutBindings.JoinSelectedTracks, true)]
        public static bool ValidateJoinAdjacentTracks() => TimeflowObject.ValidateJoinAdjacentTracks();

        [Shortcut(TimeflowShortcutInfo.Path_TransformReset, KeyCode.T, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kTransformReset + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformReset, false, 850)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformReset + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformReset, false, 850)]
        public static void ResetTransform() => AxonTools.ResetTransform();

        [UnityEditor.MenuItem(MenuPath + kTransformReset + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformReset, true)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformReset + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformReset, true)]
        public static bool ValidateResetTransform() => Selection.activeTransform;

        [Shortcut(TimeflowShortcutInfo.Path_TransformCopy, KeyCode.I, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kTransformCopy + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformCopy, false, 851)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformCopy + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformCopy, false, 851)]
        public static void CopyTransform() => AxonTools.CopyTransform();

        [UnityEditor.MenuItem(MenuPath + kTransformCopy + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformCopy, true)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformCopy + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformCopy, true)]
        public static bool ValidateCopyTransform() => Selection.activeTransform;

        [Shortcut(TimeflowShortcutInfo.Path_TransformPaste, KeyCode.O, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kTransformPaste + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformPaste, false, 852)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPaste + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformPaste, false, 852)]
        public static void PasteTransform() => AxonTools.PasteTransform(false);

        [UnityEditor.MenuItem(MenuPath + kTransformPaste + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformPaste, true)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPaste + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformPaste, true)]
        public static bool ValidatePasteTransform() => Selection.activeTransform;

        [Shortcut(TimeflowShortcutInfo.Path_TransformPasteResetScale)]
        [UnityEditor.MenuItem(MenuPath + kTransformPasteResetScale, false, 853)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPasteResetScale, false, 853)]
        public static void PasteTransformReformScale() => AxonTools.PasteTransform(true);

        [UnityEditor.MenuItem(MenuPath + kTransformPasteResetScale, true)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPasteResetScale, true)]
        public static bool ValidatePasteTransformReformScale() => Selection.activeTransform;

        [Shortcut(TimeflowShortcutInfo.Path_TransformPastePositionOnly, KeyCode.O, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kTransformPastePositionOnly + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformPastePositionOnly, false, 854)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPastePositionOnly + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformPastePositionOnly, false, 854)]
        public static void PastePosition() => AxonTools.PasteTransform(false, true, false, false);

        [UnityEditor.MenuItem(MenuPath + kTransformPastePositionOnly + TimeflowMenu.Tab + TimeflowShortcutBindings.TransformPastePositionOnly, true)]
        [UnityEditor.MenuItem(MenuPath2 + kTransformPastePositionOnly + TimeflowMenu.Tab2 + TimeflowShortcutBindings.TransformPastePositionOnly, true)]
        public static bool ValidatePastePosition() => Selection.activeTransform;

        [Shortcut(TimeflowShortcutInfo.Path_Activate, KeyCode.Alpha1, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityActivate + TimeflowMenu.Tab + TimeflowShortcutBindings.Activate, false, 900)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityActivate + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Activate, false, 900)]
        public static void Activate()
        {
            AxonTools.Activate();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityActivate + TimeflowMenu.Tab + TimeflowShortcutBindings.Activate, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityActivate + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Activate, true)]
        public static bool ValidateActivate()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_Deactivate, KeyCode.Alpha2, ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityDeactivate + TimeflowMenu.Tab + TimeflowShortcutBindings.Deactivate, false, 901)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDeactivate + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Deactivate, false, 901)]
        public static void Deactivate()
        {
            AxonTools.Deactivate();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityDeactivate + TimeflowMenu.Tab + TimeflowShortcutBindings.Deactivate, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDeactivate + TimeflowMenu.Tab2 + TimeflowShortcutBindings.Deactivate, true)]
        public static bool ValidateDeactivate()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_ActivateRecursive)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityActivateRecursive, false, 902)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityActivateRecursive, false, 902)]
        public static void ActivateRecursive()
        {
            AxonTools.ActivateRecursive();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityActivateRecursive, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityActivateRecursive, true)]
        public static bool ValidateActivateRecursive()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DeactivateRecursive)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityDeactivateRecursive, false, 903)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDeactivateRecursive, false, 903)]
        public static void DeactivateRecursive()
        {
            AxonTools.DeactivateRecursive();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityDeactivateRecursive, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDeactivateRecursive, true)]
        public static bool ValidateDeactivateRecursive()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_EnableRenderersRecursive, KeyCode.Alpha1, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityEnableRenderersRecursive + TimeflowMenu.Tab + TimeflowShortcutBindings.EnableRenderersRecursively, false, 904)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityEnableRenderersRecursive + TimeflowMenu.Tab2 + TimeflowShortcutBindings.EnableRenderersRecursively, false, 904)]
        public static void EnableRenderersRecursive()
        {
            AxonTools.EnableRenderersRecursive();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityEnableRenderersRecursive + TimeflowMenu.Tab + TimeflowShortcutBindings.EnableRenderersRecursively, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityEnableRenderersRecursive + TimeflowMenu.Tab2 + TimeflowShortcutBindings.EnableRenderersRecursively, true)]
        public static bool ValidateEnableRenderersRecursive()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_DisableRenderersRecursive, KeyCode.Alpha2, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kVisibilityDisableRenderersRecursive + TimeflowMenu.Tab + TimeflowShortcutBindings.DisableRenderersRecursively, false, 905)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDisableRenderersRecursive + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisableRenderersRecursively, false, 905)]
        public static void DisableRenderersRecursive()
        {
            AxonTools.DisableRenderersRecursive();
        }

        [UnityEditor.MenuItem(MenuPath + kVisibilityDisableRenderersRecursive + TimeflowMenu.Tab + TimeflowShortcutBindings.DisableRenderersRecursively, true)]
        [UnityEditor.MenuItem(MenuPath2 + kVisibilityDisableRenderersRecursive + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisableRenderersRecursively, true)]
        public static bool ValidateDisableRenderersRecursive()
        {
            return Selection.activeGameObject;
        }

        [Shortcut(TimeflowShortcutInfo.Path_EditorDebugMarkLine, KeyCode.M, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(MenuPath + kEditorDebugMarkLine + TimeflowMenu.Tab + TimeflowShortcutBindings.DebugMarkLineInConsole, false, 10040)]
        [UnityEditor.MenuItem(MenuPath2 + kEditorDebugMarkLine + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DebugMarkLineInConsole, false, 10040)]
        public static void DebugMarkLine()
        {
            Debug.Log("===================================: " + Time.time);//--KEEP  
        }

        [Shortcut(TimeflowShortcutInfo.Path_EditorDisableDebugAll, KeyCode.B, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(MenuPath + kEditorDisableDebugAll + TimeflowMenu.Tab + TimeflowShortcutBindings.DisableDebugForAllObjects, false, 10041)]
        [UnityEditor.MenuItem(MenuPath2 + kEditorDisableDebugAll + TimeflowMenu.Tab2 + TimeflowShortcutBindings.DisableDebugForAllObjects, false, 10041)]
        public static void DisableDebugAll()
        {
            AxonTools.DisableDebugAll();
        }

        [Shortcut(TimeflowShortcutInfo.Path_EditorListDependencies)]
        [UnityEditor.MenuItem(MenuPath + kEditorListDependencies, false, 10060)]
        [UnityEditor.MenuItem(MenuPath2 + kEditorListDependencies, false, 10060)]
        public static void ListDependencies()
        {
            if (Selection.objects != null) {
                string log = "";
                string[] paths = new string[Selection.objects.Length];

                int i = 0;
                foreach (UnityEngine.Object obj in Selection.objects) {
                    paths[i] = AssetDatabase.GetAssetPath(obj);
                    i++;
                }

                foreach (string path in paths) {
                    log += path + "\n";
                    string[] deps = AssetDatabase.GetDependencies(new string[] { path });
                    for (int d = 0; d < deps.Length; d++) {
                        log += " +" + deps[d] + "\n";
                    }
                    log += "\n";
                }
                Debug.Log(log);//--KEEP  
            }
        }

        [UnityEditor.MenuItem(MenuPath + kEditorListDependencies, true)]
        [UnityEditor.MenuItem(MenuPath2 + kEditorListDependencies, true)]
        public static bool ValidateListDependencies()
        {
            return Selection.objects != null;
        }

        public static void BackupScripts()
        {
            string baseName = Application.dataPath + "/";

            DateTime today = DateTime.Now;
            string dst = EditorUtil.ProjectPath + "/Timeflow_v" + Timeflow.Version + "_" + today.ToString("yyyy-MM-dd-H.mm") + ".unitypackage";

            exportFiles = new List<string>();
            string[] exclude = new string[1];
            exclude[0] = ".DS_Store";

            CollectDirectoryFiles(baseName + "AxonGenesis", exclude);

            string log = "BACKUP SCRIPTS\n" + dst + "\n";
            for (int x = 0; x < exportFiles.Count; x++) {
                exportFiles[x] = "Assets/" + exportFiles[x].Substring(baseName.Length);
                log += exportFiles[x] + "\n";
            }
            Debug.Log(log);//--KEEP

            string[] scripts = exportFiles.ToArray();

            AssetDatabase.ExportPackage(scripts, dst, ExportPackageOptions.Default);
        }

        public static void FindFilesInDirectory(string dir, List<string> found, string ext)
        {
            foreach (string d in Directory.GetDirectories(dir)) {
                FindFilesInDirectory(d, found, ext);
            }
            foreach (string f in Directory.GetFiles(dir)) {
                if (f.EndsWith(ext)) {
                    found.Add(f);
                }
            }
        }

        #endregion

        #region OBJECTS // Hidden and deprecated

        //[MenuItem(TimeflowToolsMenu.MenuPath + "➕ Add Behavior/Game Object", false, 149)]
        public static GameObject AddGameObject()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            GameObject sibling = null;

            gameObjectCount++;
            GameObject obj = new GameObject("GameObject" + StringUtil.PadNumber2(gameObjectCount));
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            if (transforms != null && transforms.Length > 0) {
                obj.transform.parent = transforms[0].parent;
                obj.layer = transforms[0].gameObject.layer;
            }
            if (sibling != null) {
                obj.transform.parent = sibling.transform.parent;
            }

            if (OnAddNull != null) OnAddNull(obj);

            if (Timeflow.Active != null) {
                obj.transform.parent = TimeflowContext.Obj == null ? Timeflow.Active.transform : TimeflowContext.Obj.transform.parent;
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
            }
            SelectionUtil.Select(obj);
            UndoUtil.UndoCreate(obj, "Add Game Object");
            return obj;
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "➕ Add Behavior/Child", false, 150)]
        public static GameObject AddChild()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);

            GameObject obj = null;
            foreach (Transform transform in transforms) {
                obj = new GameObject("Child");
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                obj.layer = transform.gameObject.layer;

                if (OnAddChild != null) OnAddChild(obj);

                SelectionUtil.Select(obj);
                UndoUtil.UndoCreate(obj, "Add Child");
            }

            if (Timeflow.Active != null) {
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
            }

            return obj;
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "➕ Add Behavior/Child", true)]
        public static bool ValidateAddChild()
        {
            return Selection.activeTransform;
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "👆 Selection/Group Objects (with extra xform)", false, 431)]
        public static GameObject GroupObjectsNull()
        {
            GameObject group = null;
            if (Selection.activeTransform) {
                Transform tmpParent = Selection.activeTransform.parent;

                group = new GameObject(Selection.activeTransform.gameObject.name + " Group");
                group.transform.position = Selection.activeTransform.position;
                group.transform.rotation = Selection.activeTransform.rotation;
                group.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                UndoUtil.UndoCreate(group, "Group Objects (with extra xform)");

                GameObject xformObj = new GameObject(Selection.activeTransform.gameObject.name + " Xform");
                xformObj.transform.position = Selection.activeTransform.position;
                xformObj.transform.rotation = Selection.activeTransform.rotation;
                xformObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                UndoUtil.UndoCreate(xformObj, "Group Objects (with extra xform)");

                Undo.SetTransformParent(Selection.activeTransform, xformObj.transform, "Group Objects (with extra xform)");
                Undo.SetTransformParent(group.transform, tmpParent, "Group Objects (with extra xform)");
                Undo.SetTransformParent(xformObj.transform, group.transform, "Group Objects (with extra xform)");

                if (Timeflow.Active != null) {
                    group.transform.parent = TimeflowContext.Obj == null ? Timeflow.Active.transform : TimeflowContext.Obj.transform.parent;
                    Timeflow.Active.View.Display.AddObjectToDisplay(group);
                }
            }
            return group;
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "👆 Selection/Group Objects (with extra xform)", true)]
        public static bool ValidateGroupObjectsNull()
        {
            return Selection.activeTransform;
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "👆 Selection/Simplify Hierarchy", false, 445)]
        public static void SimplifyHierarchy()
        {
            if (Selection.transforms != null) {
                List<Transform> selected = new List<Transform>();
                foreach (Transform t in Selection.transforms) {
                    selected.Add(t);
                }

                List<Transform> toDelete = new List<Transform>();
                foreach (Transform t in selected) {
                    SimplifyHierarchyRecursive(ref toDelete, t);
                }

                if (toDelete.Count > 0) {
                    foreach (Transform obj in toDelete) {
                        UndoUtil.UndoDestroy(obj.gameObject);
                    }
                }
            }
        }

        public static void SimplifyHierarchyRecursive(ref List<Transform> toDelete, Transform obj)
        {
            MonoBehaviour behavior;
            obj.TryGetComponent<MonoBehaviour>(out behavior);

            MeshRenderer renderer;
            obj.TryGetComponent<MeshRenderer>(out renderer);

            if (behavior == null && renderer == null) {
                // Only remove null objects that don't have monobehaviors 
                if (!toDelete.Contains(obj)) toDelete.Add(obj);

                List<Transform> children = new List<Transform>();
                foreach (Transform child in obj) {
                    children.Add(child);
                }
                foreach (Transform child in children) {
                    Undo.SetTransformParent(child, obj.parent, "Simplify Hierarchy");
                    SimplifyHierarchyRecursive(ref toDelete, child);
                }
            }
        }

        //[MenuItem(TimeflowToolsMenu.MenuPath + "👆 Selection/Simplify Hierarchy", true)]
        public static bool ValidateSimplifyHierarchy()
        {
            return Selection.activeTransform;
        }

        #endregion
    }

}//AxonGenesis

#endif

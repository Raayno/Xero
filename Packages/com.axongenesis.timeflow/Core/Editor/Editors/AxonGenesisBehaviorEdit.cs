// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// The base class for all AxonGenesisBehavior inspector windows. This provides an inspector template
    /// and generalizations for common functions.
    /// </summary>
    public class AxonGenesisBehaviorEdit<T> : AxonGenesisBaseEdit where T : AxonGenesisBehavior
    {
        public T target;

        public string DocumentationURL = "https://axongenesis.gitbook.io/";

        private GenericMenu dropdownMenu;

        /// <summary>
        /// Returns true if the current selection contains more than 1 object.
        /// </summary>
        public bool IsMultiObject {
            get {
                return Selection.gameObjects != null && Selection.gameObjects.Length > 1;
            }
        }

        /// <summary>
        /// Sets the target component being edited in the inspector. Must be a component type derrived from
        /// AxonGenesisBehavior.
        /// </summary>
        public virtual void SetTarget<T1>(T1 targ)
        {
            target = targ as T;
        }

        public override void SetTarget(AxonGenesisBehavior targ)
        {
            target = targ as T;
        }

        public virtual bool HasTarget()
        {
            return target != null;
        }

        public virtual Type GetTargetType()
        {
            return target != null ? target.GetType() : typeof(AxonGenesisBehavior);
        }

        public override void GUIBegin()
        {
            if (target.IsSelected && target.ShowSelected && TimeflowPreferences.Current.ShowTrackColorsInInspector) {

                Color c = target.GUIColor;
                c.a = 0.5f;
                GUI.color = c;
                AxonGUI.BeginVertical(AxonUI.HeaderStyleSelected);
            }
            else {
                AxonGUI.BeginVertical(AxonUI.HeaderStyle);
            }
            GUI.color = Color.white;
        }

        /// <summary>
        /// Displays a common header for all inspectors, including debugging option and link to
        /// documentation.
        /// </summary>
        /// <param name="typeName">Name of the class being displayed</param>
        /// <param name="dark">Set true if the header style should be dark</param>
        /// <returns>true if the target is valid, false if null</returns>
        public override bool GUIHeader(string typeName)
        {
            GUISetup();
            if (target == null) {
                //Debug.LogWarning("No target object defined.");
                return false;
            }

            if (!IsTimeflowInspector && TimeflowInspector.IsVisible && TimeflowInspector.IsShowing(target)) {
                AxonGUI.HelpBox("Hidden while the Behaviors window is open", MessageType.Info);
                return false;
            }

            if (target.IsSelected && target.ShowSelected && TimeflowPreferences.Current.ShowTrackColorsInInspector) {
                Color c = target.GUIColor;
                c.a = 0.5f;
                GUI.color = c;
                AxonGUI.BeginHorizontal(AxonUI.HeaderStyleSelected);
            }
            else {
                AxonGUI.BeginHorizontal(AxonUI.HeaderStyle);
            }
            GUI.color = AxonColor.Default;

            target.EditorShowUI = AxonGUI.FoldoutInline(target.EditorShowUI, "", new RectOffset(0, 0, 1, 0));
            AxonGUI.UndoName = "Set Enabled";
            AxonGUI.SetTooltip("Turn off to disable this behavior but keep the component active.");
            target.Enabled = AxonGUI.FieldToggleEnabled(target, target.Enabled, new RectOffset(1, 0, 2, 0));

            EditorGUI.BeginDisabledGroup(!target.Enabled);
            GUIMenu();
            EditorGUI.EndDisabledGroup();

            AxonGUI.FlexibleSpace();

            GUIMenuIcons();
            if (AxonGUI.ButtonRefresh("Refresh")) {
                Refresh();
            }
            target.DebugEnabled = AxonGUI.FieldToggleDebug(target.DebugEnabled);

            string tip = "Runtime & Editor: script executes equally in edit mode and play mode.\n" +
                "Editor Only: script only executes in edit mode and is destroyed immediately upon awake at runtime.\n" +
                "Runtime Only: this script does not execute in edit mode but only in play mode and application runtime.";
            Texture2D icon = target.IsEditorOnly ? AxonUI.Icons.EditorOnly : target.IsRuntimeOnly ? AxonUI.Icons.RuntimeOnly : AxonUI.Icons.EditorAndRuntime;
            if (AxonGUI.ButtonTexture(icon, tip, new RectOffset(0, 0, 2, 0))) {
                GenericMenu m = new GenericMenu();
                m.AddItem(new GUIContent("Editor and Runtime"), target.IsEditorAndRuntime, SetEditorAndRuntime, (object)target);
                m.AddItem(new GUIContent("Editor Only (destroyed at runtime)"), target.IsEditorOnly, SetEditorOnly, (object)target);
                m.AddItem(new GUIContent("Runtime Only"), target.IsRuntimeOnly, SetRuntimeOnly, (object)target);

                Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                m.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
            }

            GUIMenuOptions();

            _GUIDropDown();
            AxonGUI.EndHorizontal(false);

            return target.EditorShowUI && target.Enabled && target.enabled;
        }

        private void _GUIDropDown()
        {
            dropdownMenu = new GenericMenu();
            GUIDropDown(ref dropdownMenu);
            if (dropdownMenu.GetItemCount() > 0 && AxonGUI.ButtonInline("...")) {
                if (dropdownMenu != null) {
                    dropdownMenu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 25));
                }
            }
        }

        public virtual void GUIDropDown(ref GenericMenu menu)
        {
        }

        public static void SetEditorAndRuntime(object obj)
        {
            AxonGenesisBehavior target = (AxonGenesisBehavior)obj;
            if (target != null) {
                target.PlaybackMode = AxonGenesisBehavior.PlaybackModes.EditorAndRuntime;
            }
        }

        public static void SetEditorOnly(object obj)
        {
            AxonGenesisBehavior target = (AxonGenesisBehavior)obj;
            if (target != null) {
                target.PlaybackMode = AxonGenesisBehavior.PlaybackModes.EditorOnly;
            }
        }

        public static void SetRuntimeOnly(object obj)
        {
            AxonGenesisBehavior target = (AxonGenesisBehavior)obj;
            if (target != null) {
                target.PlaybackMode = AxonGenesisBehavior.PlaybackModes.RuntimeOnly;
            }
        }

        public override void Refresh()
        {
            target.Refresh();
            EditorUtil.SetDirty(target);
        }

        public virtual void GUIPresetsMenu()
        {
            if (AxonGUI.ButtonTexture(AxonUI.Icons.Presets, "Presets", new RectOffset(0, 0, 3, 0))) {
                AxonGUI.PresetsMenuPopup(target);
            }
#if TIMEFLOW_LEGACY_PRESETS
            GUI.color = Color.red;
            if (AxonGUI.ButtonTexture(AxonUI.Icons.Presets, "Legacy_Presets", new RectOffset(0, 0, 3, 0))) {
                AxonGUI.Legacy_PresetsMenuPopup(target);
            }
            GUI.color = Color.white;
#endif
        }

    }

}//AxonGenesis

#endif

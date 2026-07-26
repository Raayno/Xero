// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(TimeDisplay))]
    public class TimeDisplayEditor : AxonGenesisEditor<TimeDisplay, TimeDisplayEdit> { }

    sealed public class TimeDisplayEdit : AxonGenesisBehaviorEdit<TimeDisplay>
    {
#if TIMEFLOW_PRO
        public const string kAddTimeDisplay = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🔢 Time Display";
#else
        public const string kAddTimeDisplay = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Time Display";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Time Display";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddTimeDisplay, false, 201)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddTimeDisplay, false, 201)]
        public static void AddTimeDisplay()
        {
            ObjectUtil.GetOrAddComponent<TimeDisplay>(TimeflowMenu.GetSelectedOrNewGameObject("Time Display"));
        }

        public bool isBuilding = false;
        private SerializedProperty SetDisplay;

        public TimeDisplayEdit() { }

        public TimeDisplayEdit(TimeDisplay _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/trail-renderer-update";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            AxonGUI.Setup(120);
            SetDisplay = editor.serializedObject.FindProperty("SetDisplay");
        }

        public override void GUIMenu()
        {
            target.Mode = (TimeDisplay.Modes)AxonGUI.FieldEnumPopupInline(target, "Display Mode", target.Mode, GUILayout.Width(170));
            if(target.Mode == TimeDisplay.Modes.Custom) {
                target.CustomFormat = AxonGUI.FieldTextInline(target, "Format", target.CustomFormat, GUILayout.Width(120));
            }
            AxonGUI.FlexibleSpace();
            AxonGUI.FieldTextInline(target, "Output", target.Output, GUILayout.Width(150));
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            target.UseCustomTimeInput = AxonGUI.FieldToggle(target, "Custom Time", target.UseCustomTimeInput);
            if (target.UseCustomTimeInput) {
                target.CustomTimeInput = AxonGUI.FieldFloatInline(target, target.CustomTimeInput);
            }
            AxonGUI.EndHorizontal();

            if (target.Mode == TimeDisplay.Modes.Frames || (target.Mode == TimeDisplay.Modes.Timecode && !TimeflowPreferences.Current.UseFractionalTime)) {
                AxonGUI.BeginHorizontal();
                target.UseCustomFPS = AxonGUI.FieldToggle(target, "Custom Framerate", target.UseCustomFPS);
                if (target.UseCustomFPS) {
                    target.CustomFPS = AxonGUI.FieldFloatInline(target, target.CustomFPS);
                }
                AxonGUI.EndHorizontal();
            }

            if (target.Mode == TimeDisplay.Modes.Measures) {
                AxonGUI.BeginHorizontal();
                target.UseCustomBPM = AxonGUI.FieldToggle(target, "Custom BPM", target.UseCustomBPM);
                if (target.UseCustomBPM) {
                    target.CustomBPM = AxonGUI.FieldFloatInline(target, target.CustomBPM);
                    target.CustomBeatsPerBar = AxonGUI.FieldIntInline(target, "Beats/Bar", target.CustomBeatsPerBar);
                    target.CustomBeatNoteSize = AxonGUI.FieldIntInline(target, "Note Size", target.CustomBeatNoteSize);
                }
                AxonGUI.EndHorizontal();
            }

            if (SetDisplay != null) {
                EditorGUILayout.PropertyField(SetDisplay, new GUIContent("Set Display"));
            }

            if (GUI.changed) {
                editor.serializedObject.ApplyModifiedProperties();
                target.EditorUpdate();
            }
        }
    }

}//AxonGenesis 

#endif
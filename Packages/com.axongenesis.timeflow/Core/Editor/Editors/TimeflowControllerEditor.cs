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
    [CustomEditor(typeof(TimeflowController))]
    public class TimeflowControllerEditor : AxonGenesisEditor<TimeflowController, TimeflowControllerEdit> { }

    sealed public class TimeflowControllerEdit : AxonGenesisBehaviorEdit<TimeflowController>
    {
#if TIMEFLOW_PRO
        public const string kAddTimeflowController = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🕹️ Timeflow Controller";
#else
        public const string kAddTimeflowController = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Timeflow Controller";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Timeflow Controller";

        [Shortcut(kShortcut)]
        [MenuItem(TimeflowMenu.MenuPath + kAddTimeflowController, false, 241)]
        [MenuItem(TimeflowMenu.MenuPath2 + kAddTimeflowController, false, 241)]
        public static void AddTimeflowController()
        {
            GameObject obj = null;
            if (obj == null) {
                obj = new GameObject("TimeflowController");
                UndoUtil.UndoCreate(obj, "Add Timeflow Controller");
            }

            TimeflowController controller = ObjectUtil.AddComponent<TimeflowController>(obj);
            UndoUtil.UndoCreate(controller, "Add Timeflow Controller");

            if (Selection.activeGameObject != null) {
                controller.transform.SetParent(Selection.activeGameObject.transform);
                controller.transform.SetAsFirstSibling();

                if (Selection.activeGameObject.TryGetComponent<Timeflow>(out Timeflow timeflow)) {
                    controller.TimeflowParent = timeflow;
                }
            }

            ObjectUtil.ResetTransform(obj);
            SelectionUtil.Select(obj);
        }

        private SerializedProperty OnStartupEvent = null;
        private SerializedProperty OnPlayEvent = null;
        private SerializedProperty OnStopEvent = null;
        private SerializedProperty OnSkipEvent = null;
        private SerializedProperty OnRewindEvent = null;
        private SerializedProperty OnLoopEvent = null;

        public TimeflowControllerEdit() { }

        public TimeflowControllerEdit(TimeflowController _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/timeflow-controller";

            OnStartupEvent = editor.serializedObject.FindProperty("OnStartupEvent");
            OnPlayEvent = editor.serializedObject.FindProperty("OnPlayEvent");
            OnStopEvent = editor.serializedObject.FindProperty("OnStopEvent");
            OnSkipEvent = editor.serializedObject.FindProperty("OnSkipEvent");
            OnRewindEvent = editor.serializedObject.FindProperty("OnRewindEvent");
            OnLoopEvent = editor.serializedObject.FindProperty("OnLoopEvent");
        }

        public override void GUIMenu()
        {
            AxonGUI.SetTooltip("Determines whether playback loops, occurs once then stops (One Shot), or continues to play indefinitely (Continuous)");
            target.PlayMode = (TimeflowController.PlayModes)AxonGUI.FieldEnumPopup(target, target.PlayMode, GUILayout.Width(150));

            AxonGUI.SetTooltip("If enabled, the Timeflow instance will begin playback automatically when the scene starts. Otherwise playback will only begin when Play() is triggered by an event or script.");
            target.AutoPlay = AxonGUI.FieldToggleInline(target, "Auto Play", target.AutoPlay);

            AxonGUI.SetTooltip("Activates the work area in the timeline, used primarily to set playback and looping regions.");
            target.WorkAreaEnabled = AxonGUI.FieldToggleInline(target, "Work Area", target.WorkAreaEnabled);
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginBox();

            target.TimeflowParent = (Timeflow)AxonGUI.FieldObject(target, "Timeflow", target.TimeflowParent, typeof(Timeflow), true);
            if (target.TimeflowParent == null) {
                AxonGUI.HelpBox("A Timeflow instance must be assigned for the Timeflow Controller to function.", MessageType.Warning);
            }

            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("If set, playback will begin at the specified marker when Play() is invoked.");
            if (target.SetMarkerByName) {
                target.StartAtMarkerName = AxonGUI.FieldMarker(target, "Start At Marker", target.StartAtMarkerName);
            }
            else {
                target.StartAtMarkerIndex = AxonGUI.FieldInt(target, "Start At Marker", target.StartAtMarkerIndex);
            }
            target.SetMarkerByName = AxonGUI.FieldToggleInline(target, "Set by Name", target.SetMarkerByName);
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();


            OnEventsGUI();

            if (GUI.changed) {
                editor.serializedObject.ApplyModifiedProperties();
                EditorUtil.SetDirty(target);
            }
        }

        private void OnEventsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowEvents = AxonGUI.Foldout(target.EditorShowEvents, "Events");
            if (target.EditorShowEvents) {
                AxonGUI.BeginBoxPadded();
                EditorGUILayout.PropertyField(OnStartupEvent);
                EditorGUILayout.PropertyField(OnPlayEvent);
                EditorGUILayout.PropertyField(OnStopEvent);
                EditorGUILayout.PropertyField(OnSkipEvent);
                EditorGUILayout.PropertyField(OnRewindEvent);
                EditorGUILayout.PropertyField(OnLoopEvent);
                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }
    }

}//AxonGenesis

#endif
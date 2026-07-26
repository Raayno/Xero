// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(TimeflowEvent))]
    public class TimeflowEventEditor : AxonGenesisEditor<TimeflowEvent, TimeflowEventEdit> { }

    /// <summary>
    /// This acts as a wrapper to create the editor behavior from the base template while using the shared
    /// UI to handle the logic and drawing.
    /// </summary>
    sealed public class TimeflowEventEdit : AxonGenesisBehaviorEdit<TimeflowEvent>
    {
        TimeflowEventSharedEdit sharedUI;

        public TimeflowEventEdit() { }

        public TimeflowEventEdit(TimeflowEvent _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/event";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (sharedUI == null) sharedUI = new TimeflowEventSharedEdit(target, editor);
            sharedUI.GUISetup();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void GUIMenu()
        {
            sharedUI.GUIMenu();
        }

        public override void OnInspectorGUI()
        {
            sharedUI.OnInspectorGUI();
        }
    }

    /// <summary>
    /// This class decouples the view drawing from the editor definition so that any behaviors derriving
    /// from TimeflowEvent may incorporate this class to leverage the base class implementation. This is
    /// necessary since TimeflowEventUI cannot be overridden due to complexities with the template.
    /// </summary>
    sealed public class TimeflowEventSharedEdit
    {
        public Editor editor;
        public TimeflowEvent target;

        SerializedProperty OnTrigger;

        public TimeflowEventSharedEdit() { }

        public TimeflowEventSharedEdit(TimeflowEvent evt, Editor ed)
        {
            target = evt;
            editor = ed;
        }

        public void GUISetup()
        {
            OnTrigger = editor.serializedObject.FindProperty("OnTrigger");
        }

        public void GUIMenu()
        {
            if (string.IsNullOrEmpty(target.Name)) target.Name = target.Function;
            AxonGUI.UndoName = "Set Event Name";
            target.Name = AxonGUI.FieldTextInline(target, "Event Name", target.Name, GUILayout.Width(220));

            AxonGUI.UndoName = "Set Event Triggered";
            target.WasTriggered = AxonGUI.FieldToggleInline(target, "Triggered", target.WasTriggered, GUILayout.Width(100));
            if (AxonGUI.ButtonInline("Trigger Now")) {
                target.Trigger(true);
            }
        }

        public void OnInspectorGUI()
        {
            if (target.Enabled) {
                if (target.Obj == null) target.Obj = target.gameObject;
                if (target.Function == null) target.Function = "";
                if (target.Parameter == null) target.Parameter = "";

                AxonGUI.BeginHorizontalBox();
                EditorGUI.BeginDisabledGroup(target.LockTime);
                AxonGUI.UndoName = "Set Trigger Time";
                AxonGUI.SetTooltip("Sets the time the event occurs in the Timeflow timeline.");
                target.TriggerTime = AxonGUI.FieldFloat(target, "Trigger Time", target.TriggerTime);
                if (AxonGUI.ButtonInline("Set to Current Time", GUILayout.Width(100))) {
                    target.TriggerTime = Timeflow.Active.CurrentTime;
                }
                EditorGUI.EndDisabledGroup();
                AxonGUI.UndoName = "Set Lock Time";
                target.LockTime = AxonGUI.FieldToggleInline(target, "Lock", target.LockTime);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.SetTooltip("Limits the number of times this event can be triggered. Set to 0 for no limit.");
                if (target.TriggerLimit < 0) target.TriggerLimit = 0;
                AxonGUI.UndoName = "Set Trigger Limit";
                target.TriggerLimit = AxonGUI.FieldInt(target, "Trigger Limit", target.TriggerLimit);

                target.LogEnabled = AxonGUI.FieldToggleInline(target, "Log", target.LogEnabled);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Send Message To";
                AxonGUI.SetTooltip("The target object to receive the message when the event fires.");
                target.Obj = (GameObject)AxonGUI.FieldObject(target, "Send Message To", target.Obj, typeof(GameObject), true);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Function";
                AxonGUI.SetTooltip("The name of the method to call. Any script on the game object that implements this method will be called.");
                target.Function = AxonGUI.FieldText(target, "Function", target.Function);

                AxonGUI.UndoName = "Set Parameter";
                AxonGUI.SetTooltip("Optional parameter value to pass to the function.");
                target.Parameter = AxonGUI.FieldTextInline(target, "Parameter", target.Parameter);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginBoxPadded();
                EditorGUILayout.PropertyField(OnTrigger, new GUIContent("On Trigger"));
                AxonGUI.EndBoxPadded();
            }

            editor.serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }
    }

}//AxonGenesis

#endif
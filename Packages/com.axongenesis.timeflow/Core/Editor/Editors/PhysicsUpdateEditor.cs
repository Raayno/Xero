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
    [CustomEditor(typeof(PhysicsUpdate))]
    public class PhysicsUpdateEditor : AxonGenesisEditor<PhysicsUpdate, PhysicsUpdateEdit> { }

    sealed public class PhysicsUpdateEdit : AxonGenesisBehaviorEdit<PhysicsUpdate>
    {
#if TIMEFLOW_PRO
        public const string kAddPhysicsUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🏓 Physics Update";
#else
        public const string kAddPhysicsUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Physics Update";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Physics Update";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddPhysicsUpdate, false, 201)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddPhysicsUpdate, false, 201)]
        public static void AddPhysicsUpdate()
        {
            ObjectUtil.GetOrAddComponent<PhysicsUpdate>(TimeflowMenu.GetSelectedOrNewGameObject("Physics Update"));
        }

        public PhysicsUpdateEdit() { }

        public PhysicsUpdateEdit(PhysicsUpdate _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/physics-update";
        }

        public override void GUIMenu()
        {
            AxonGUI.SetTooltip("If enabled, the initial transform states of all rigidbodies are captured when this component awakes in the editor or during runtime.");
            target.SaveStatesOnAwake = AxonGUI.FieldToggleInline(target, "Save On Awake", target.SaveStatesOnAwake);

            if (target.HasInitialStates) {
                GUI.color = Color.green;
                AxonGUI.ButtonInline(target.HasInitialStates ? "States Saved" : "No States", GUI.skin.button);
                GUI.color = Color.white;
            }

            GUI.color = !target.HasInitialStates ? Color.green : Color.white;
            AxonGUI.SetTooltip("Captures the transform states of all rigidbodies as the restore point when resetting the simulation.");
            if (AxonGUI.ButtonInline("Save Initial States")) {
                target.SaveInitialStates();
            }
            GUI.color = Color.white;

            if (AxonGUI.ButtonInline("Restore")) {
                target.RestoreInitialStates();
            }
            if (AxonGUI.ButtonInline("Clear")) {
                target.ClearInitialStates();
            }
        }

        private Vector2 _Scroll;

        public override void OnInspectorGUI()
        {
            AxonGUI.SetTooltip("If enabled, all rigidbody transforms are restored to their initial states when this component is enabled.");
            target.RestoreStatesOnRewind = AxonGUI.FieldToggle(target, "Restore On Enable", target.RestoreStatesOnRewind);

            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("If enabled, all rigidbody transforms are restored to their initial states when time jumps back in time or restarts.");
            target.RestoreStatesOnRewind = AxonGUI.FieldToggle(target, "Restore On Rewind", target.RestoreStatesOnRewind);
            if (target.RestoreStatesOnRewind) {
                AxonGUI.SetTooltip("If set to a value greater than 0, restoring states will only occur if the time is rewinded before the Time Threshold");
                //target.RestoreTimeThreshold = AxonGUI.FieldFloatInline(target, "Time Threshold", target.RestoreTimeThreshold);
                target.RestoreTimeThreshold = AxonGUI.FieldTimeInline(target, "Time Threshold", target.RestoreTimeThreshold, GUILayout.Width(180));
            }
            AxonGUI.EndHorizontal();

            // Display list of rigidbodies with enable checkboxes
            GUIStates();

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
                //target.EditorUpdate();
            }
        }

        private void GUIStates()
        {
            AxonGUI.Space();
            AxonGUI.BeginHorizontal();
            target.EditorShowStates = AxonGUI.Foldout(target.EditorShowStates, "Initial States");
            AxonGUI.EndHorizontal();
            if (!target.EditorShowStates) return;

            AxonGUI.BeginBox();

            if(target.InitialStates == null || target.InitialStates.Count == 0) {
                AxonGUI.HelpBox("No initial states saved. Click 'Save Initial States' in the menu above to capture the current states of all rigidbodies in the scene.");
                AxonGUI.EndBox();
                return;
            }
            AxonGUI.BeginHorizontal();
            if (GUILayout.Button("Enable All", GUILayout.Width(100))) {
                for (int i = 0; i < target.InitialStates.Count; i++)
                    target.InitialStates[i].Enabled = true;
            }
            if (GUILayout.Button("Disable All", GUILayout.Width(100))) {
                for (int i = 0; i < target.InitialStates.Count; i++)
                    target.InitialStates[i].Enabled = false;
            }
            if (GUILayout.Button("Expand All", GUILayout.Width(100))) {
                for (int i = 0; i < target.InitialStates.Count; i++)
                    target.InitialStates[i].Foldout = true;
            }
            if (GUILayout.Button("Collapse All", GUILayout.Width(100))) {
                for (int i = 0; i < target.InitialStates.Count; i++)
                    target.InitialStates[i].Foldout = false;
            }
            GUILayout.FlexibleSpace();
            target.EditorShowStateValues = AxonGUI.FieldToggleInline(target, "Show Values", target.EditorShowStateValues);
            AxonGUI.EndHorizontal();

            _Scroll = EditorGUILayout.BeginScrollView(_Scroll, GUILayout.MaxHeight(220));

            foreach (var item in target.InitialStates) {
                AxonGUI.BeginHorizontal();
                if (target.EditorShowStateValues) {
                    item.Foldout = AxonGUI.FoldoutInline(item.Foldout);
                }
                item.Enabled = AxonGUI.FieldToggleInline(target, item.Enabled, GUILayout.Width(18));
                item.Rigidbody = (Rigidbody)AxonGUI.FieldObject(target, item.Rigidbody, typeof(Rigidbody), true);
                AxonGUI.EndHorizontal();
                if (target.EditorShowStateValues && item.Foldout) {
                    AxonGUI.BeginBox();
                    item.Position = AxonGUI.FieldVector3(target, "  Position", item.Position);
                    item.Rotation = Quaternion.Euler(AxonGUI.FieldVector3(target, "  Rotation", item.Rotation.eulerAngles));
                    item.Velocity = AxonGUI.FieldVector3(target, "  Velocity", item.Velocity);
                    item.AngularVelocity = AxonGUI.FieldVector3(target, "  Angular Velocity", item.AngularVelocity);
                    AxonGUI.EndBox();
                }
            }

            EditorGUILayout.EndScrollView();
            AxonGUI.EndBox();

        }
    }

}//AxonGenesis 

#endif
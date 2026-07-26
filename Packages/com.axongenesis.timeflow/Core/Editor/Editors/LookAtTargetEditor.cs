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
    [CustomEditor(typeof(LookAtTarget))]
    public class LookAtTargetEditor : AxonGenesisEditor<LookAtTarget, LookAtTargetEdit> { }

    sealed public class LookAtTargetEdit : AxonGenesisBehaviorEdit<LookAtTarget>
    {
#if TIMEFLOW_PRO
        public const string kAddLookAtTarget = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎯 Look At Target";
#else
        public const string kAddLookAtTarget = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Look At Target";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Look At Target";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddLookAtTarget, false, 126)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddLookAtTarget, false, 126)]
        public static void AddLookAtTarget()
        {
            ObjectUtil.GetOrAddComponent<LookAtTarget>(TimeflowMenu.GetSelectedOrNewGameObject("Look At Target"));
        }

        public LookAtTargetEdit() { }

        public LookAtTargetEdit(LookAtTarget _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            target.EditorShowUI = false; // nothing to see here
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/look-at-target";
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            bool active = target.transform == LookAt.GlobalTarget;
            if (!active) {
                AxonGUI.SetTooltip("Click to make this object the current global Look At Target. There can only be 1 global target active at a time.");
                if (AxonGUI.ButtonInline("Make Active Target")) {
                    LookAt.GlobalTarget = target.transform;
                }
                EditorGUI.BeginDisabledGroup(true);
                AxonGUI.FieldObjectInline(target, "Currently Active", LookAt.GlobalTarget, typeof(Transform), true);
                EditorGUI.EndDisabledGroup();
            }
            else {
                GUI.backgroundColor = AxonColor.TrackOrange;
                AxonGUI.SetTooltip("This object is the current global Look At Target.");
                if (GUILayout.Button("ACTIVE", GUILayout.Width(200))) {
                    LookAt.GlobalTarget = target.transform;
                }
                GUI.backgroundColor = Color.white;
            }
            GUI.color = AxonColor.Default;
            AxonGUI.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
        }
    }

}//AxonGenesis 

#endif
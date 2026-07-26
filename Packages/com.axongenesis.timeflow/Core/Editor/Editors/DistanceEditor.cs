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
    [CustomEditor(typeof(Distance))]
    public class DistanceEditor : AxonGenesisEditor<Distance, DistanceEdit> { }

    sealed public class DistanceEdit : AxonGenesisBehaviorEdit<Distance>
    {
#if TIMEFLOW_PRO
        public const string kAddDistance = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "📐 Distance";
#else
        public const string kAddDistance = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Distance";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Distance";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddDistance, false, 122)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddDistance, false, 122)]
        public static void AddDistance()
        {
            ObjectUtil.GetOrAddComponent<Distance>(TimeflowMenu.GetSelectedOrNewGameObject("Distance"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public DistanceEdit() { }
        public DistanceEdit(Distance _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/distance";
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set From";
            AxonGUI.SetTooltip("Distance is calculated between the world position of 2 game objects.");
            target.From = (Transform)AxonGUI.FieldObject(target, "From", target.From, typeof(Transform), true);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set To";
            target.To = (Transform)AxonGUI.FieldObject(target, "To", target.To, typeof(Transform), true);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Scale";
            AxonGUI.SetTooltip("Multiplies the distance calculated to scale the final result.");
            target.Scale = AxonGUI.FieldFloat(target, "Scale", target.Scale);

            AxonGUI.UndoName = "Set Apply to Transform";
            AxonGUI.SetTooltip("If enabled, the result is applied to the current objects transform scale as a uniform value.");
            target.ApplyToTransform = AxonGUI.FieldToggleInline(target, "Apply To Transform", target.ApplyToTransform);
            AxonGUI.EndHorizontal();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.UpdateTime();
                EditorUtil.SetDirty(target);
            }
        }
    }

}//AxonGenesis
#endif
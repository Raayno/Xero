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
    [CustomEditor(typeof(LoopTimeOffset))]
    public class LoopTimeOffsetEditor : AxonGenesisEditor<LoopTimeOffset, LoopTimeOffsetEdit> { }
    sealed public class LoopTimeOffsetEdit : AxonGenesisBehaviorEdit<LoopTimeOffset>
    {
#if TIMEFLOW_PRO
        public const string kLoopTimeOffset = "➕ Add Behavior/➰ Loop Time Offset";
#else
        public const string kLoopTimeOffset = "Add Behavior/Loop Time Offset";
#endif

        public const string kShortcut = "Timeflow/Add Behavior: Loop Time Offset";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kLoopTimeOffset, false, 127)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kLoopTimeOffset, false, 127)]
        static public void AddLoopTimeOffset()
        {
            ObjectUtil.GetOrAddComponent<LoopTimeOffset>(TimeflowMenu.GetSelectedOrNewGameObject("Loop Time Offset"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public LoopTimeOffsetEdit() { }

        public LoopTimeOffsetEdit(LoopTimeOffset _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/loop-time-offset";
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup(100);
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("If enabled, the duration is automatically determined by the object's track length. Turn this off for manual control.");
            target.AutoDuration = AxonGUI.FieldToggle(target, "Auto", target.AutoDuration);
            AxonGUI.BeginDisabledGroup(true);
            AxonGUI.FieldTimeInline(target, "", target.Duration.Time);
            AxonGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal();
            if (!target.AutoDuration) {
                AxonGUI.SetTooltip("Sets the period of time to loop");
                AxonGUI.FieldTimeValue(target, "Duration", target.Duration);
            }
            
            AxonGUI.SetTooltip("Specifies the time that this object will start looping. The track starting point is aligned to this time.");
            AxonGUI.FieldTimeValue(target, "Start At", target.StartAt);
            
            AxonGUI.SetTooltip("Specifies the time that looping stops. The track may extend beyond this end point, but will not loop any further past that.");
            AxonGUI.FieldTimeValue(target, "End At", target.EndAt);
 
            AxonGUI.EndBox();

            if (GUI.changed) {
                EditorUtil.SetDirty(target);
                target.Refresh();
            }
        }
    }

}//AxonGenesis

#endif
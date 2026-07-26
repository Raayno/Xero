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
    [CustomEditor(typeof(TrailRendererUpdate))]
    public class TrailRendererUpdateEditor : AxonGenesisEditor<TrailRendererUpdate, TrailRendererUpdateEdit> { }

    sealed public class TrailRendererUpdateEdit : AxonGenesisBehaviorEdit<TrailRendererUpdate>
    {
#if TIMEFLOW_PRO
        public const string kAddTrailRendererUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🌈 Trail Renderer Update";
#else
        public const string kAddTrailRendererUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Trail Renderer Update";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Trail Renderer Update";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddTrailRendererUpdate, false, 201)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddTrailRendererUpdate, false, 201)]
        public static void AddTrailRendererUpdate()
        {
            ObjectUtil.GetOrAddComponent<TrailRendererUpdate>(TimeflowMenu.GetSelectedOrNewGameObject("Trail Renderer Update"));
        }

        public bool isBuilding = false;

        public TrailRendererUpdateEdit() { }

        public TrailRendererUpdateEdit(TrailRendererUpdate _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/trail-renderer-update";
        }

        public override void GUIMenu()
        {
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.UndoName = "Set Trail Renderer";
            AxonGUI.SetTooltip("Assign the trail renderer to update with Timeflow.");
            target.Trail = (TrailRenderer)AxonGUI.FieldObject(target, "Trail Renderer", target.Trail, typeof(TrailRenderer), true);

            AxonGUI.UndoName = "Set Clear On Stop";
            AxonGUI.SetTooltip("If enabled, the trail renderer is cleared from view when Timeflow playback is stopped.");
            target.ClearOnStop = AxonGUI.FieldToggle(target, "Clear On Stop", target.ClearOnStop);

            AxonGUI.UndoName = "Set Clear On Rewind";
            AxonGUI.SetTooltip("If enabled, the trail renderer is cleared upon rewinding or looping time. Otherwise if off, the trail renderer displays continously. Turn off for seemless looping.");
            target.ClearOnRewind = AxonGUI.FieldToggle(target, "Clear On Rewind", target.ClearOnRewind);

            if (GUI.changed) {
                target.EditorUpdate();
            }
        }
    }

}//AxonGenesis 

#endif
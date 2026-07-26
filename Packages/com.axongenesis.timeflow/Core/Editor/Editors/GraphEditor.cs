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
    [CustomEditor(typeof(Graph))]
    public class GraphEditor : AxonGenesisEditor<Graph, GraphEdit> { }

    sealed public class GraphEdit : AxonGenesisBehaviorEdit<Graph>
    {
#if TIMEFLOW_PRO
        public const string kAddGraph = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "📈 Graph";
#else
        public const string kAddGraph = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Graph";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Graph";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddGraph, false, 127)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddGraph, false, 127)]
        public static void AddGraph()
        {
            ObjectUtil.GetOrAddComponent<Graph>(TimeflowMenu.GetSelectedOrNewGameObject("Graph"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public GraphEdit() { }
        public GraphEdit(Graph _target)
        {
            target = _target;
        }

        [UnityEditor.MenuItem("CONTEXT/Graph/Export JSON")]
        static void ExportJSON(MenuCommand command)
        {
            Graph graph = (Graph)command.context;
            graph.ExportData();
        }

        [UnityEditor.MenuItem("CONTEXT/Graph/Import JSON")]
        static void ImportJSON(MenuCommand command)
        {
            Graph graph = (Graph)command.context;
            graph.ImportData();
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/graph";
        }

        public override void GUIMenu()
        {
            GUI.color = target.IsRecording ? AxonColor.EditingOverride : AxonColor.Default;
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleDark);
            GUI.backgroundColor = GUI.color;
            GUI.color = AxonColor.Default;

            AxonGUI.BeginDisabledGroup(target.IsLocked);
            AxonGUI.SetTooltip("Enables new values to be recorded. If disabled, no values are added or modified, keeping the existing graph as-is.");
            if (AxonGUI.ButtonInline(target.IsRecording ? "Stop Recording" : "Start Recording", GUILayout.Width(120))) {
                if (target.IsRecording) {
                    target.StopRecording();
                }
                else {
                    target.StartRecording();
                }
            }
            GUI.backgroundColor = AxonColor.Default;

            AxonGUI.UndoName = "Set Work Area Only";
            AxonGUI.SetTooltip("Use this to record new values only in the Work Area (if enabled).");
            bool workArea = AxonGUI.FieldToggleInline(target, "Work Area Only", target.WorkAreaOnly);
            if (target.WorkAreaOnly != workArea) {
                target.WorkAreaOnly = workArea;
                Timeflow.Active.WorkAreaEnabled = target.WorkAreaOnly;
                Timeflow.Active.LoopEnabled = false;
            }

            AxonGUI.UndoName = "Set Resume Recording";
            AxonGUI.SetTooltip("If enabled the recording starts at the current time. Otherwise the playhead is rewound to the start.");
            target.ResumeRecording = AxonGUI.FieldToggleInline(target, "Resume", target.ResumeRecording);

            AxonGUI.SetTooltip("Clears all recorded data.");
            if (AxonGUI.ButtonInline("Clear Data")) {
                target.ClearData(true);
            }

            AxonGUI.UndoName = "Set Clear On Record";
            AxonGUI.SetTooltip("If enabled, the data is cleared each time playback begins. Turn this off to keep existing data until it is cleared manually, using the Clear Data button.");
            target.ClearOnRecord = AxonGUI.FieldToggleInline(target, "Clear On Record", target.ClearOnRecord);
            AxonGUI.EndDisabledGroup();

            AxonGUI.EndHorizontal();

            if (AxonGUI.ButtonLock(target.IsLocked, "If enabled, prevents data being written or cleared to preserve existing data")) {
                target.IsLocked = !target.IsLocked;
            }
        }

        public override void OnInspectorGUI()
        {
            target.EditorShowUI = true;// false;

            MainGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.UpdateTime();
                EditorUtil.SetDirty(target);
            }
        }

        private void MainGUI()
        {
            AxonGUI.BeginHorizontalBox();
            if (target.Channel != null) {
                AxonGUI.UndoName = "Set Channel Name";
                target.Channel.Name = AxonGUI.FieldTextInline(target, "Name", target.Channel.Name);

                AxonGUI.UndoName = "Save Play Mode Data";
                AxonGUI.SetTooltip("If enabled, data recording during playmode is automatically saved and reloaded in the editor upon existing play mode. Use this to record data during play mode.");
                target.SavePlayModeData = AxonGUI.FieldToggleInline(target, "Save Play Mode Data", target.SavePlayModeData);

                if (AxonGUI.ButtonInline("Reload")) {
                    target.LoadPlayModeData(false);
                }
            }
            AxonGUI.EndHorizontal();
        }
    }

}//AxonGenesis

#endif
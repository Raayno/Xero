// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{

    [CustomEditor(typeof(TimeflowBehavior))]
    public class TimeflowBehaviorEditor : AxonGenesisEditor<TimeflowBehavior, TimeflowBehaviorEdit> { }

    /// <summary>
    /// Provides the UI to TimeflowBehavior as a separate class from the main UI to allow for inheritance.
    /// Note that this is a base class only and TimeflowBehavior should not be added to game objects.
    /// </summary>
    public class TimeflowBehaviorSharedEdit
    {
        public Editor editor;
        public TimeflowBehavior target;

        public TimeflowBehaviorSharedEdit()
        {
        }

        public TimeflowBehaviorSharedEdit(TimeflowBehavior _target, Editor ed)
        {
            target = (TimeflowBehavior)_target;
            editor = ed;
        }

        public void MainGUI()
        {
            AxonGUI.SetLabelWidth(100f);
            AxonGUI.BeginBox();
            if (target.GetType() == typeof(TimeflowBehavior)) {
                AxonGUI.HelpBox("TimeflowBehavior is a base class and should not be used directly. Please remove this component.", MessageType.Error);
                return;
            }

            target.EditorShowTime = AxonGUI.Foldout(target.EditorShowTime, "Update Settings");
            if (target.EditorShowTime) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Update Frequency";
                AxonGUI.SetTooltip("Limit or customize how behaviors on this object are updated.\nEvery Frame: default behavior, repsects Track in-out.\nForce Framerate: Updates will only occur in specific intervals based on the frames per second set.\nUpdate After: to chain together behaviors in a specific order. This component updates after the one specififed.\nExplicit: call update via custom script");
                target.UpdateFrequency = (TimeflowBehavior.UpdateFrequencies)AxonGUI.FieldEnumPopup(target, "Update", target.UpdateFrequency, GUILayout.Width(AxonGUI.LabelWidth + 120));

                AxonGUI.UndoName = "Set Update Method";
                target.UpdateMethod = (TimeflowBehavior.UpdateMethods)AxonGUI.FieldEnumPopupInline(target, target.UpdateMethod, GUILayout.Width(120));

                if (target.UpdateFrequency == TimeflowBehavior.UpdateFrequencies.ForceFramerate) {
                    if (target.ForceFramerate <= 0f) target.ForceFramerate = 1f;
                    AxonGUI.UndoName = "Set Frames Per Second";
                    AxonGUI.SetTooltip("Reduce the frame rate to a fixed number of frames per second to simulate film or stop motion.");
                    target.ForceFramerate = AxonGUI.FieldFloatInline(target, "Frames Per Second", target.ForceFramerate);
                }
                else
                if (target.UpdateFrequency == TimeflowBehavior.UpdateFrequencies.TimeInterval) {
                    if (target.TimeInterval <= 0f) target.TimeInterval = 0.1f;
                    AxonGUI.UndoName = "Set Time in Seconds";
                    AxonGUI.SetTooltip("Set a specific time interval in seconds to update this behavior.");
                    target.TimeInterval = AxonGUI.FieldFloatInline(target, "Time in Seconds", target.TimeInterval);
                }
                else
                if (target.UpdateFrequency == TimeflowBehavior.UpdateFrequencies.UpdateAfter) {
                    AxonGUI.UndoName = "Set Update After";
                    target.UpdateAfter = (TimeflowBehavior)AxonGUI.FieldObjectInline(target, target.UpdateAfter, typeof(TimeflowBehavior), true);
                }

                AxonGUI.FlexibleSpace();
                AxonGUI.ButtonDocs("Update Settings Docs", "https://axongenesis.gitbook.io/timeflow/user-guide/timeflow-editor/update");
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Current Time";
                AxonGUI.SetTooltip("The current time for this object, taking into account any time offsets on this object or its parents. The value cannot be set directly.");
                AxonGUI.FieldTime(target, "Current Time", target.CurrentTime);

                AxonGUI.UndoName = "Set Time Scale";
                AxonGUI.SetTooltip("Scales the time relative to the parent object.");
                target.TimeScale = AxonGUI.FieldTimeInline(target, "Scale", target.TimeScale);

                AxonGUI.UndoName = "Set Time Offset";
                AxonGUI.SetTooltip("Offsets the time (in seconds) relative to the parent Timeflow or attached Director.");
                target.TimeOffset = AxonGUI.FieldTimeInline(target, "Time Offset", target.TimeOffset);

                AxonGUI.UndoName = "Set Drag Time Offset";
                AxonGUI.SetTooltip("If enabled, the track can be dragged in the Timeflow view to adjust the Time Offset. If disabled, tracks can be dragged independently of keyframes and will not affect the Time Offset.");
                target.CanDragTimeOffset = AxonGUI.FieldToggleInline(target, "Drag Time Offset", target.CanDragTimeOffset);


                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.BeginDisabledGroup(true);
                AxonGUI.SetTooltip("This displays the TimeflowObject which manages this behavior. This is a read-only property which is determined by its location in the scene.");
                AxonGUI.FieldObject(target, "Parent Object", target.ParentObject, typeof(TimeflowObject), true);
                AxonGUI.EndDisabledGroup();

                AxonGUI.SetTooltip("This resets the Update Settings to the default configuration.");
                if (AxonGUI.ButtonInline("Restore Default Update Settings")) {
                    UndoUtil.Undo(target, "Restore Default Update Settings", true);
                    target.UpdateMethod = TimeflowBehavior.UpdateMethods.Update;
                    target.UpdateFrequency = TimeflowBehavior.UpdateFrequencies.EveryFrame;
                    target.UpdateAfter = null;
                    target.TimeOffset = 0f;
                    target.CanDragTimeOffset = false;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();

                if (target.LinkedBehaviors != null && target.LinkedBehaviors.Count > 0) {
                    AxonGUI.BeginBoxPadded();
                    AxonGUI.Heading("Linked Behaviors");
                    foreach (TimeflowBehavior b in target.LinkedBehaviors) {
                        if (b != null) {
                            AxonGUI.SetTooltip("This is controlled by the behavior's Update Settings when using Update After.");
                            AxonGUI.FieldObject(target, b.Name, b, typeof(TimeflowBehavior), true);
                        }
                    }
                    AxonGUI.EndBoxPadded();
                }

            }
            AxonGUI.EndBox();
            if (GUI.changed) {
                target.UpdateTime();
            }
            AxonGUI.RestoreLabelWidth();
        }

        public void ChannelsGUI(bool allowAdd)
        {
            AxonGUI.BeginBox();
            target.EditorShowChannels = AxonGUI.Foldout(target.EditorShowChannels, "Channels");
            if (target.EditorShowChannels) {
                int x = 0;
                int y = 0;
                if (target.Channels != null) {
                    int moveUp = -1;
                    int moveDown = -1;
                    List<TimeflowChannel> toRemove = new List<TimeflowChannel>();
                    foreach (TimeflowChannel channel in target.Channels) {
                        if (channel.IsSelected && TimeflowPreferences.Current.ShowTrackColorsInInspector) {
                            Color c = channel.GUIColor;
                            c.a = 0.5f;
                            GUI.color = c;
                            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleSelected);
                        }
                        else {
                            AxonGUI.BeginHorizontal(AxonUI.HeaderStyle);
                        }
                        GUI.color = AxonColor.Default;

                        int indent = AxonGUI.Indent;
                        AxonGUI.Indent = 0;

                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                            moveUp = x;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                            moveDown = x;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Channel")) {
                            toRemove.Add(channel);
                        }

                        channel.InspectorGUI(null);

                        AxonGUI.EndHorizontal();
                        AxonGUI.Indent = indent;
                        x++;
                    }

                    bool updateSort = false;
                    if (moveUp > 0) {
                        y = moveUp - 1;
                        if (y >= 0) {
                            int order = target.Channels[moveUp].SortOrder;
                            target.Channels[moveUp].SortOrder = target.Channels[y].SortOrder;
                            target.Channels[y].SortOrder = order;

                            TimeflowChannel tmp = target.Channels[moveUp];
                            target.Channels[moveUp] = target.Channels[y];
                            target.Channels[y] = tmp;
                        }
                        updateSort = true;
                    }
                    if (moveDown > -1) {
                        y = moveDown + 1;
                        if (y < target.Channels.Count) {
                            int order = target.Channels[moveDown].SortOrder;
                            target.Channels[moveDown].SortOrder = target.Channels[y].SortOrder;
                            target.Channels[y].SortOrder = order;

                            TimeflowChannel tmp = target.Channels[moveDown];
                            target.Channels[moveDown] = target.Channels[y];
                            target.Channels[y] = tmp;
                        }
                        updateSort = true;
                    }
                    if (toRemove.Count > 0) {
                        foreach (TimeflowChannel channel in toRemove) {
                            target.RemoveChannelWithUndo(channel);
                        }
                    }
                    if (updateSort) {
                        target.ParentObject.SortChannels();
                    }
                }
                AxonGUI.Space();
                if (allowAdd && GUILayout.Button("Add Channel", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    UndoUtil.Undo(target, "Add Channel");
                    TimeflowChannel channel = new TimeflowChannel(target);
                    target.AddChannel(channel);
                    return;
                }
                AxonGUI.Space();
            }

            AxonGUI.EndBox();
        }

    }

    /// <summary>
    /// Wraps the above shared UI into an editor
    /// </summary>
    sealed public class TimeflowBehaviorEdit : AxonGenesisBehaviorEdit<TimeflowBehavior>
    {
        public TimeflowBehaviorSharedEdit ui;

        public TimeflowBehaviorEdit() { }

        public TimeflowBehaviorEdit(TimeflowBehavior _target)
        {
            target = _target;
            ui = new TimeflowBehaviorSharedEdit(_target, editor);
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (ui == null) ui = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            target.SortChannels();
        }

        public override void GUIMenu()
        {
        }

        public override void OnInspectorGUI()
        {
            ui.MainGUI();
        }

        public override void OnSceneGUI()
        {
        }
    }

}//AxonGenesis

#endif
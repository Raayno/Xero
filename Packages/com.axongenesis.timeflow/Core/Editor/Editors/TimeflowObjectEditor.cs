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
    [CustomEditor(typeof(TimeflowObject))]
    public class TimeflowObjectEditor : AxonGenesisEditor<TimeflowObject, TimeflowObjectEdit> { }

    public class TimeflowObjectEdit : AxonGenesisBehaviorEdit<TimeflowObject>
    {
        private static CopiedTracksData CopiedTracks;

        private class CopiedTracksData
        {
            public List<Vector2> Keys;
            public bool AutoFullLength;
        }

        public bool IsVisible = true;
        public TimeflowBehaviorSharedEdit behaviorUI;

        private SerializedProperty TrackOn;
        private SerializedProperty TrackOff;
        private SerializedProperty TrackVisibilityChanged;

        public TimeflowObjectEdit() { }

        public TimeflowObjectEdit(TimeflowObject _target, Editor _editor)
        {
            target = _target;
            editor = _editor;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/timeflow-object";
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);

            if (editor == null || editor.serializedObject == null) {
                //Debug.LogWarning($"editor is null");
                return;
            }
            TrackOn = editor.serializedObject.FindProperty("TrackOn");
            TrackOff = editor.serializedObject.FindProperty("TrackOff");
            TrackVisibilityChanged = editor.serializedObject.FindProperty("TrackVisibilityChanged");
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void GUIMenu()
        {
            TimeflowObject b = target;

            AxonGUI.SetTooltip("Rerun the setup routine to refresh channels and behaviors.");
            AxonGUI.BeginHorizontal();
            if (target.Track != null) {
                AxonGUI.UndoName = "Set Track Visibility Mode";
                AxonGUI.SetTooltip("Specifies how the track affects the visibility of the object.\nOn: has no effect on the object visibility or active state.\nActive: this object is enabled/disabled based on the track in and out points.\nRenderer: turns all Renderer components on/off on this object and its hierarchy (objects remain active, but not rendered).\nRenderer Independent: affects all Renderer components on this object and its children, disregarding any parent Timeflow object renderer states.\nActivate Children: this object remains active, but its children are enabled/disabled based on track in and out points.");

                target.Track.VisibilityMode = (TimeflowTrack.VisibilityModes)AxonGUI.FieldEnumPopupInline(target, "Visibility", target.Track.VisibilityMode, GUILayout.Width(200));
                target.TrackActivated = target.Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate;

                AxonGUI.FlexibleSpace();
                AxonGUI.UndoName = "Set Track Color";
                target.Track.GUIColor = AxonGUI.FieldColorInline(target, target.Track._GUIColor, false, GUILayout.Width(60));
            }
            AxonGUI.FlexibleSpace();
            AxonGUI.UndoName = "Set Show Children";
            AxonGUI.SetTooltip("If disabled, all children of this object remain hidden from view.");
            bool showChildren = AxonGUI.FieldToggleInline(target, "Show Children", target.ShowChildren);
            if (target.ShowChildren != showChildren) {
                target.ShowChildren = showChildren;
            }
            AxonGUI.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            if (target.Track == null) {
                target.SetupChannels(true);
            }
            behaviorUI.MainGUI();

            MainGUI();

            if (GUI.changed) {
                editor.serializedObject.ApplyModifiedProperties();
            }
        }

        public void MainGUI()
        {
            if (target == null) return;
            TracksGUI();
            BehaviorsGUI();
            EventsGUI();
            TimeflowGUI();
            ChannelsGUI();
        }

        public void TracksGUI()
        {
            AxonGUI.BeginBox();

            bool isTimeflow = target is Timeflow;

            if (!isTimeflow) {
                EditorGUI.BeginDisabledGroup(!target.Enabled);
                AxonGUI.BeginHorizontal();
                target.EditorShowTrack = AxonGUI.Foldout(target.EditorShowTrack, "Tracks");

                if (AxonGUI.ButtonTexture(AxonUI.Icons.More, "Options")) {
                    GenericMenu menu = new GenericMenu();
                    if (target.Track.AutoFullLength) {
                        menu.AddItem(new GUIContent("Add Track"), false, null);
                    }
                    else {
                        menu.AddItem(new GUIContent("Add Track"), false, AddKey, target.Track);
                    }
                    menu.AddItem(new GUIContent("Copy Tracks"), false, CopyTracks);

                    if (CopiedTracks != null) {
                        menu.AddItem(new GUIContent("Paste Tracks"), false, PasteTracks);
                        menu.AddItem(new GUIContent("Merge Tracks"), false, MergeTracks);
                    }
                    else {
                        menu.AddItem(new GUIContent("Paste Tracks"), false, null);
                        menu.AddItem(new GUIContent("Merge Tracks"), false, null);
                    }

                    Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                    menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
                }
                AxonGUI.EndHorizontal();

                if (target.EditorShowTrack) {
                    if (Timeflow.Active.Input.IsDragging) {
                        AxonGUI.HelpBox("Track lists are hidden during drag operations to avoid conflicting updates.", MessageType.Info);
                    }
                    else {
                        AxonGUI.BeginVertical("box");
                        AxonGUI.Indent++;
                        AxonGUI.Space();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.Space();

                        AxonGUI.UndoName = "Set Track Locked";
                        target.Track.IsLocked = AxonGUI.FieldToggleInline(target, "Locked", target.Track.IsLocked);
                        AxonGUI.BeginDisabledGroup(target.Track.IsLocked);

                        AxonGUI.UndoName = "Set Track Auto Full Length";
                        AxonGUI.SetTooltip("Automatically match the length of the track to the duration of the containing Timeflow group.");
                        bool autoFull = AxonGUI.FieldToggleInline(target, "Auto Full Length", target.Track.AutoFullLength);
                        if (autoFull != target.Track.AutoFullLength) {
                            ToggleAutoFullLength();
                        }

                        AxonGUI.UndoName = "Set Channel Height Locked";
                        AxonGUI.SetTooltip("If locked, the channel height in the Timeflow view cannot be altered.");
                        target.Track.GUIHeightLocked = AxonGUI.FieldToggleInline(target, "Lock Channel Height", target.Track.GUIHeightLocked);

                        AxonGUI.UndoName = "Set Show Children";
                        bool showChildren = AxonGUI.FieldToggleInline(target, "Show Children", target.ShowChildren);
                        if (target.ShowChildren != showChildren) {
                            target.ShowChildren = showChildren;
                        }

                        AxonGUI.UndoName = "Set Auto Work Area";
                        AxonGUI.SetTooltip("If enabled, the loop area in the Timeflow view will automatically be set to the current track in/out points. This only takes effect in the editor and can be useful for working with specific sections of time defined by a track.");
                        target.Track.AutoSetWorkArea = AxonGUI.FieldToggleInline(target, "Auto Set Work Area", target.Track.AutoSetWorkArea, GUILayout.Width(20));

                        target.Track.DebugEnabled = AxonGUI.FieldToggleDebug(target.Track.DebugEnabled);
                        AxonGUI.EndHorizontal();

                        AxonGUI.Space();

                        int remove = -1;
                        int insert = -1;
                        int lastKey = target.Track.Keys.Count - 1;
                        for (int i = 0; i < target.Track.Keys.Count; i++) {
                            Keyframe key = target.Track.Keys[i];
                            Keyframe prev = i == 0 ? null : target.Track.Keys[i - 1];
                            Keyframe next = i == lastKey ? null : target.Track.Keys[i + 1];

                            AxonGUI.BeginHorizontalIndent();

                            GUILayout.Space(4);

                            if (!target.Track.AutoFullLength) {
                                if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Key")) {
                                    insert = i;
                                }
                                if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Key")) {
                                    remove = i;
                                }
                            }
                            AxonGUI.UndoName = "Set Track Enabled";
                            key.IsKeyEnabled = AxonGUI.FieldToggleEnabled(target, key.IsKeyEnabled);
                            if (AxonGUI.ButtonTexture(key.LockTime ? AxonUI.Icons.LockOn : AxonUI.Icons.LockOff, "Lock to prevent changes")) {
                                key.LockTime = !key.LockTime;
                                key.LockValue = key.LockTime;
                            }
                            EditorGUI.BeginDisabledGroup(key.LockTime);
                            AxonGUI.UndoName = "Set Track Label";
                            key.KeyString = AxonGUI.FieldTextInline(target, "Label", key.KeyString);
                            target.Track.GUITextColor = AxonGUI.FieldColorInline(target, target.Track.GUITextColor, false, GUILayout.Width(60));
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.BeginDisabledGroup(key.LockTime || target.Track.AutoFullLength);
                            AxonGUI.SetTooltip("Start and end time of the track can only be set if the track is unlocked and Full Length is disabled.");
                            AxonGUI.UndoName = "Set Track Name";
                            key.KeyTime = AxonGUI.FieldTimeInline(target, "Start", key.KeyTime, false, GUILayout.Width(120));
                            if (prev != null && key.KeyTime < prev.KeyValue) {
                                key.KeyTime = prev.KeyValue;
                            }
                            if (key.KeyTime >= key.KeyValue) {
                                key.KeyTime = key.KeyValue - TimeflowPreferences.Current.KeyTolerance;
                            }

                            AxonGUI.UndoName = "Set Track End Time";
                            key.KeyValue = AxonGUI.FieldTimeInline(target, "End", key.KeyValue, false, GUILayout.Width(120));
                            if (key.KeyValue <= key.KeyTime) {
                                key.KeyValue = key.KeyTime + TimeflowPreferences.Current.KeyTolerance;
                            }
                            if (next != null && key.KeyValue > next.KeyTime) {
                                key.KeyValue = next.KeyTime;
                            }
                            EditorGUI.EndDisabledGroup();

                            AxonGUI.SetTooltip("Show the Work Area in the Timeflow view and set the in and out points to this track section.");
                            if (AxonGUI.ButtonInline("Set Work Area")) {
                                UndoUtil.Undo(target, "Set Work Area");
                                target.Timeflow.SetWorkArea(key.KeyTimeWorld, key.KeyEndTimeWorld, true);
                                target.Timeflow.LoopEnabled = true;
                            }

                            AxonGUI.EndHorizontal();

                            if (target.Track.AutoFullLength) break;
                        }
                        if (!target.Track.AutoFullLength) {
                            if (remove > -1) {
                                UndoUtil.Undo(target, "Remove Track Key");
                                target.Track.Keys.RemoveAt(remove);
                                EditorUtil.SetDirty(target);
                            }
                            if (insert > -1) {
                                UndoUtil.Undo(target, "Insert Track Key");
                                if (insert == 0) {
                                    target.Track.SetKey(target.Track.Keys[0].KeyTime - 1f, target.Track.Keys[0].KeyTime, true);
                                }
                                else
                                if (insert == lastKey) {
                                    target.Track.SetKey(target.Track.Keys[lastKey].KeyValue, target.Track.Keys[lastKey].KeyValue + 1f, true);
                                }
                                else {
                                    target.Track.SetKey(target.Track.Keys[insert - 1].KeyValue, target.Track.Keys[insert].KeyTime, true);
                                }
                            }

                        }

                        AxonGUI.EndDisabledGroup();
                        AxonGUI.Indent--;
                        AxonGUI.EndVertical();
                    }
                }
            }

            EditorGUI.EndDisabledGroup();

            AxonGUI.EndBox();
        }

        public void ChannelsGUI()
        {
            if (target.Enabled) {
                AxonGUI.BeginBox();
                AxonGUI.SetTooltip("Lists the TimeflowChannels from all behaviors of this object.");
                target.EditorShowChannels = AxonGUI.Foldout(target.EditorShowChannels, "Channels");
                if (target.EditorShowChannels) {
                    AxonGUI.BeginBoxPadded();
                    if (target.AllChannelsForDisplay == null || target.AllChannelsForDisplay.Count == 0) {
                        AxonGUI.Label("None", "");
                    }
                    else {
                        bool anyShown = false;
                        int moveUp = -1;
                        int moveDown = -1;
                        int x = 0;
                        List<TimeflowChannel> toRemove = new List<TimeflowChannel>();

                        foreach (TimeflowChannel channel in target.AllChannelsForDisplay) {
                            if (channel != target.Track) {
                                if (channel == null) {
                                    AxonGUI.Warning("Null channel reference! Press the Refresh button to clear. Please contact support if this issue persists.");
                                }
                                else {
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
                                    anyShown = true;

                                    AxonGUI.EndHorizontal();
                                }
                            }
                            x++;
                        }
                        if (!anyShown) {
                            AxonGUI.HelpBox("No channels have been created.", MessageType.Info);
                        }
                        else {
                            bool updateSort = false;
                            if (moveUp > 0) {
                                int y = moveUp - 1;
                                if (y >= 0) {
                                    int order = target.AllChannels[moveUp].SortOrder;
                                    target.AllChannels[moveUp].SortOrder = target.AllChannels[y].SortOrder;
                                    target.AllChannels[y].SortOrder = order;

                                    TimeflowChannel tmp = target.AllChannels[moveUp];
                                    target.AllChannels[moveUp] = target.AllChannels[y];
                                    target.AllChannels[y] = tmp;
                                }
                                updateSort = true;
                            }
                            if (moveDown > -1) {
                                int y = moveDown + 1;
                                if (y < target.AllChannels.Count) {
                                    int order = target.AllChannels[moveDown].SortOrder;
                                    target.AllChannels[moveDown].SortOrder = target.AllChannels[y].SortOrder;
                                    target.AllChannels[y].SortOrder = order;

                                    TimeflowChannel tmp = target.AllChannels[moveDown];
                                    target.AllChannels[moveDown] = target.AllChannels[y];
                                    target.AllChannels[y] = tmp;
                                }
                                updateSort = true;
                            }
                            if (toRemove.Count > 0) {
                                foreach (TimeflowChannel channel in toRemove) {
                                    channel.Behavior.RemoveChannelWithUndo(channel);
                                }
                            }
                            if (updateSort) {
                                target.SortChannels();
                            }
                        }
                    }

                    AxonGUI.EndBoxPadded();
                }
                AxonGUI.EndBox();
            }
        }

        public void ToggleAutoFullLength()
        {
            UndoUtil.Undo(target, "Auto Full Length");
            target.Track.AutoFullLength = !target.Track.AutoFullLength;
            if (target.Track.AutoFullLength) {
                target.ResetTrack();
            }
            else {
                target.Track.SetFullLength(false);
                target.Track.Keys[0].LockTime = false;
                target.Track.Keys[0].LockValue = false;
            }
        }

        public void AddKey(object obj)
        {
            TimeflowChannel channel = (TimeflowChannel)obj;
            if (channel != null) {
                UndoUtil.Undo(target, "Add Key");
                if (channel.Keys != null && channel.Keys.Count > 0) {
                    float t = channel.Keys[channel.Keys.Count - 1].KeyValue;
                    channel.SetKey(t, t + 1f, true);
                }
                else {
                    channel.SetKey(target.CurrentTime, target.CurrentTime + 1f, true);
                }
            }
            else Debug.LogWarning("Invalid channel object");
        }

        public void CopyTracks()
        {
            CopiedTracks = new CopiedTracksData();
            CopiedTracks.AutoFullLength = target.Track.AutoFullLength;
            CopiedTracks.Keys = new List<Vector2>();
            foreach (Keyframe k in target.Track.Keys) {
                CopiedTracks.Keys.Add(new Vector2(k.KeyTime, k.KeyValue));
            }
        }

        public void PasteTracks(bool merge)
        {
            if (CopiedTracks != null) {
                UndoUtil.Undo(target, "Paste Tracks");
                if (!merge && CopiedTracks.AutoFullLength) {
                    target.ResetTrack();
                }
                else
                if (CopiedTracks.Keys != null && CopiedTracks.Keys.Count > 0) {
                    target.Track.AutoFullLength = false;
                    if (!merge) target.Track.Keys = new List<Keyframe>();
                    foreach (Vector2 v in CopiedTracks.Keys) {
                        target.Track.SetKey(v.x, v.y, true);
                    }
                }
                else {
                    Debug.LogWarning("No tracks were pasted because no tracks have been copied.");
                }
            }
            else {
                Debug.LogWarning("No tracks were pasted because no tracks have been copied.");
            }
        }

        public void PasteTracks()
        {
            PasteTracks(false);
        }

        public void MergeTracks()
        {
            PasteTracks(true);
        }

        public void UpdateAnimationGUI()
        {
            if (target.HasAnimator || target.HasAnimation) {
                bool hasSequencer = target.TryGetComponent<AnimationClips>(out var clips);

                EditorGUI.BeginDisabledGroup(hasSequencer);
                AxonGUI.BeginHorizontalBox();
                AxonGUI.SetLabelWidth(120);
                AxonGUI.UndoName = "Set Update Animator";
                AxonGUI.SetTooltip("Use this to update Animator and Animation components on this object in sync wtih Timeflow. Turn this off if you are using another script to update animation.");
                target.UpdateAnimation = AxonGUI.FieldToggle(target, target.HasAnimator ? "Update Animator" : "Update Animation", target.UpdateAnimation);

                if (hasSequencer) {
                    target.UpdateAnimation = false;
                    AxonGUI.HelpBox("Animation Clips overrides this setting and handles all Animator and Animation updates.", MessageType.Info);
                }
                else
                if (target.UpdateAnimation) {
                    AxonGUI.UndoName = "Set Animation Speed";
                    AxonGUI.SetTooltip("Sets the speed of the animation. Example: 1 is normal speed, 0.5 would be half speed.");
                    target.AnimationSpeed = AxonGUI.FieldFloatInline(target, "Speed", target.AnimationSpeed);

                    if (!target.HasAnimator) {
                        AxonGUI.FieldFloatMinMax(target, "Start", ref target.AnimationOffset, ref target.AnimationRand, ref target.EditorAnimationOffsetMinMax,
                            "Shift the time of the animation or animator relative to the current object time.",
                            "Adds a random time offset to the animation, which can be used to create variability. Random values are initialized anew every run.");
                    }
                }
                AxonGUI.ResetLabelWidth();
                AxonGUI.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                AxonGUI.Space();
            }
        }

        public void BehaviorsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowBehaviors = AxonGUI.Foldout(target.EditorShowBehaviors, "Behaviors");
            AxonGUI.EndHorizontal();

            if (target.EditorShowBehaviors) {
                AxonGUI.Space();
                AxonGUI.Indent++;

                UpdateAnimationGUI();

                if (target.Behaviors == null) target.Behaviors = new List<TimeflowBehavior>();

                for (int x = 0; x < target.Behaviors.Count; x++) {
                    AxonGUI.BeginHorizontalIndent();

                    if (target.Behaviors[x] != null) {
                        AxonGUI.UndoName = "Set Enabled";
                        target.Behaviors[x].Enabled = AxonGUI.FieldToggleEnabled(target, target.Behaviors[x].Enabled);
                    }
                    AxonGUI.BeginDisabledGroup(true);
                    AxonGUI.FieldObjectInline(target, target.Behaviors[x], typeof(TimeflowBehavior), true);
                    AxonGUI.EndDisabledGroup();

                    AxonGUI.EndHorizontal();
                }

                AxonGUI.Space();
                AxonGUI.Space();
                AxonGUI.Indent--;
            }

            AxonGUI.EndBox();
        }

        public void TimeflowGUI()
        {
            if (Timeflow.Instances == null) Timeflow.GetAllInstances();
            if (Timeflow.Instances.Count < 2) return; // Only show when multiple instaces are present

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTimeflow = AxonGUI.Foldout(target.EditorShowTimeflow, "Timeflow");
            AxonGUI.EndHorizontal();

            if (target.EditorShowTimeflow) {
                AxonGUI.Space();
                AxonGUI.Indent++;

                // Don't allow child objects to specify their own timeflow
                if (!target.IsOrphaned) target.OverrideTimeflowParent = false;

                AxonGUI.BeginHorizontal();
                if (target.IsOrphaned) {
                    target.OverrideTimeflowParent = AxonGUI.FieldToggle(target, "Override", target.OverrideTimeflowParent);
                    AxonGUI.Info("The Timeflow that controls timing for this object is automatically determined based on the parent object, " +
                        "though for unparented objects Timeflow may be assigned explicitly. Note that precomps (child Timeflows) cannot be assigned, " +
                        "only root Timeflow instances are allowed.");
                }

                AxonGUI.BeginDisabledGroup(!target.OverrideTimeflowParent);
                if (target.OverrideTimeflowParent) {
                    target.TimeflowParentOverride = (Timeflow)AxonGUI.FieldObjectInline(target, "Timeflow", target.TimeflowParentOverride, typeof(Timeflow), true);
                }
                else {
                    AxonGUI.FieldObjectInline(target, "Timeflow", target.Timeflow, typeof(Timeflow), true);
                }
                AxonGUI.EndDisabledGroup();

                AxonGUI.EndHorizontal();

                AxonGUI.Space();
                AxonGUI.Space();
                AxonGUI.Indent--;
            }

            AxonGUI.EndBox();
        }

        public void EventsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowEvents = AxonGUI.Foldout(target.EditorShowEvents, "Events");
            AxonGUI.EndHorizontal();

            if (target.EditorShowEvents) {
                AxonGUI.Space();
                AxonGUI.Indent++;

                if (TrackOn != null) {
                    EditorGUILayout.PropertyField(TrackOn, new GUIContent("Track On"));
                    EditorGUILayout.PropertyField(TrackOff, new GUIContent("Track Off"));
                    EditorGUILayout.PropertyField(TrackVisibilityChanged, new GUIContent("Track Visibility Changed"));
                }
                AxonGUI.Space();
                AxonGUI.Space();
                AxonGUI.Indent--;
            }

            AxonGUI.EndBox();
        }


    }

}//AxonGenesis

#endif
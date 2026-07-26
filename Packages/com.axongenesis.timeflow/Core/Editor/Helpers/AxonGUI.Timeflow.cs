// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
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
    /// <summary>
    /// UI features specific to Timeflow
    /// </summary>
    public partial class AxonGUI
    {
        private static bool editChannelLink;

        #region TIME FIELD

        public static float FieldTime(Object target, string label, float value, params GUILayoutOption[] options)
        {
            return FieldTime(target, label, value, Timeflow.Active, options);
        }

        public static float FieldTime(Object target, string label, float value, bool isGlobal, params GUILayoutOption[] options)
        {
            return FieldTime(target, label, value, isGlobal, Timeflow.Active, options);
        }

        public static float FieldTime(Object target, string label, float value, bool isGlobal, Timeflow t, params GUILayoutOption[] options)
        {
            if (t == null) {
                t = Timeflow.Active;
            }
            if (t == null) {
                return FieldFloat(target, label, value);
            }
            else {
                return FieldTime(target, label, value, (TimeflowView.TimeDisplayModes)t.View.TimeDisplay, isGlobal, t.View.UseFractionalTime, t.FPS, options);
            }
        }

        public static float FieldTime(Object target, string label, float value, TimeflowView.TimeDisplayModes mode, bool isGlobal, bool fractionalSeconds, float fps, params GUILayoutOption[] options)
        {
            if (!IsRowHorizontal) RowCount++;
            ResetLabelWidth();

            bool isRow = IsRowHorizontal;
            if (!isRow) BeginHorizontal();
            EditorGUILayout.LabelField(GetLabelWithTooltip(label), GUILayout.Width(EditorGUIUtility.labelWidth));
            float v = FieldTimeInline(target, null, value, mode, isGlobal, fractionalSeconds, fps, options);
            if (value != v) {
                if (string.IsNullOrEmpty(label)) label = "Time Field";
                if (target != null) _Undo(target, label, $"{v}");
                value = v;
            }
            if (!isRow) EndHorizontal(false);

            RestoreLabelWidth();
            return value;
        }

        public static float FieldTimeInline(Object target, float value)
        {
            if (Timeflow.Active != null) {
                return FieldTimeInline(target, value, Timeflow.Active.View.TimeDisplay, false, Timeflow.Active.View.UseFractionalTime, Timeflow.Active.FPS);
            }
            else {
                return FieldFloatInline(target, value);
            }
        }

        public static float FieldTimeInline(Object target, string label, float value) { return FieldTimeInline(target, label, value, null); }

        public static float FieldTimeInline(Object target, string label, float value, params GUILayoutOption[] options)
        {
            return FieldTimeInline(target, label, value, false, null);
        }

        public static float FieldTimeInline(Object target, string label, float value, bool isGlobal, params GUILayoutOption[] options)
        {
            if (Timeflow.Active != null) {
                return FieldTimeInline(target, label, value, Timeflow.Active.View.TimeDisplay, isGlobal, Timeflow.Active.View.UseFractionalTime, Timeflow.Active.FPS, options);
            }
            else {
                return FieldFloatInline(target, label, value, isGlobal, options);
            }
        }

        public static float FieldTimeInline(Object target, float value, TimeflowView.TimeDisplayModes mode, bool isGlobal, bool fractionalSeconds, float fps, params GUILayoutOption[] options)
        { return FieldTimeInline(target, null, value, mode, isGlobal, fractionalSeconds, fps, options); }

        public static float FieldTimeInline(Object target, string label, float value, TimeflowView.TimeDisplayModes mode, bool isGlobal, bool fractionalSeconds, float fps, params GUILayoutOption[] options)
        {
            if (Timeflow.Active == null) return value;
            ClearIndent();
            ResetLabelWidth();

            if (!string.IsNullOrEmpty(label)) {
                EditorGUIUtility.labelWidth = CalculateWidth(label);
            }
            else {
                if (mode == TimeflowView.TimeDisplayModes.Frames || mode == TimeflowView.TimeDisplayModes.Seconds) {
                    EditorGUIUtility.labelWidth = 1; // Add a tiny label for value scrubbing feature
                    label = ":";
                }
            }

            if (!isGlobal && Timeflow.Active != null) {
                GUI.color = Timeflow.Active.IsTimeScopeLocalized ? AxonColor.TimeScope : GUI.color;
                if (Timeflow.Active.IsTimeScopeLocalized) {
                    value += Timeflow.Active.GlobalTimeOffset;
                    LabelInline("(Local)");
                }
            }
            // Conform time to allowable precision
            float orig = value;
            value = Timeflow.ApplyTimeTolerance(value);

            float v = value;
            if (fps <= 0) {
                Debug.LogWarning($"Invalid FPS value: {fps}");
                fps = 30;
            }
            if (mode == TimeflowView.TimeDisplayModes.Frames) {
                int frame = (int)Mathf.RoundToInt(value * fps);
                frame = FieldIntInline(target, label, frame, false, AddOptions(options, false, GUILayout.MaxWidth(180)));
                v = (float)frame / fps;
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Timecode) {
                string timecode = StringUtil.SecondsToTimecode(value, true, !fractionalSeconds, fps);
                string tv = timecode;
                timecode = FieldTextInline(target, label, timecode, true, AddOptions(options, false, GUILayout.MaxWidth(220)));
                if (timecode != tv) {
                    if (TimeflowPreferences.Current.AllowCommaForDecimalPoints) {
                        timecode = timecode.Replace(",", ".");
                    }
                    v = StringUtil.TimecodeToSeconds(timecode, true, !fractionalSeconds, fps);
                }
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Measures) {
                string timecode = StringUtil.SecondsToMeasures(value, Timeflow.Active.BPM, Timeflow.Active.BeatsPerBar, Timeflow.Active.BeatNoteSize);
                string tv = timecode;
                timecode = FieldTextInline(target, label, timecode, true, AddOptions(options, false, GUILayout.MaxWidth(220)));
                if (timecode != tv) {
                    if (TimeflowPreferences.Current.AllowCommaForDecimalPoints) {
                        timecode = timecode.Replace(",", ".");
                    }
                    v = StringUtil.MeasuresToSeconds(timecode, Timeflow.Active.BPM, Timeflow.Active.BeatsPerBar, Timeflow.Active.BeatNoteSize);
                }
            }
            else {
                //v = FieldFloatInline(target, label, value, true, AddOptions(options, false, GUILayout.MaxWidth(180)));
                //string time = value.ToString();
                //string tv = time;
                //time = FieldTextInline(target, label, time, true, AddOptions(options, false, GUILayout.MaxWidth(220)));
                //if (time != tv) {
                //    if (TimeflowPreferences.Current.AllowCommaForDecimalPoints) {
                //        time = time.Replace(",", ".");
                //    }
                //    v = StringUtil.ParseFloat(time);
                //}
                v = FieldFloatInline(target, label, value);
            }
            if (value != v) {
                //Debug.Log($"{mode} Changed:{value}=>{v}");
                if (string.IsNullOrEmpty(label)) label = "Time Field";
                if (target != null) _Undo(target, label, $"{v}");
                value = v;
            }
            else value = orig;

            if (!isGlobal && Timeflow.Active != null) {
                if (Timeflow.Active.IsTimeScopeLocalized) {
                    value -= Timeflow.Active.GlobalTimeOffset;
                }
                GUI.color = Color.white;
            }

            RestoreIndent();
            RestoreLabelWidth();
            return value;
        }

        public static float DisplayTimeField(Object target, Rect rect, float value, TimeflowView.TimeDisplayModes mode, GUIStyle style)
        {
            return DisplayTimeField(target, rect, value, false, mode, style);
        }

        public static float DisplayTimeField(Object target, Rect rect, float value, bool isGlobal, TimeflowView.TimeDisplayModes mode, GUIStyle style)
        {
            if (Timeflow.Active == null) return value;
            ClearIndent();

            if (!isGlobal && Timeflow.Active != null) {
                GUI.color = Timeflow.Active.IsTimeScopeLocalized ? AxonColor.TimeScope : AxonColor.Default;
                if (Timeflow.Active.IsTimeScopeLocalized) {
                    value += Timeflow.Active.GlobalTimeOffset;
                }
            }

            if (mode == TimeflowView.TimeDisplayModes.Seconds) {
                value = EditorGUI.FloatField(rect, GUIContent.none, value, style);
            }
            else {
                string time = TimeflowView.DisplayTime(value, mode);
                string newtime = EditorGUI.TextField(rect, GUIContent.none, time, style);

                if (TimeflowPreferences.Current.AllowCommaForDecimalPoints) {
                    newtime = newtime.Replace(",", ".");
                }

                if (!newtime.Equals(time)) {
                    // Conform time to allowable precision
                    float orig = value;
                    value = Timeflow.ApplyTimeTolerance(value);

                    float v = value;
                    if (mode == TimeflowView.TimeDisplayModes.Timecode) {
                        v = StringUtil.TimecodeToSeconds(newtime, true, true, Timeflow.Active.FPS);
                    }
                    else
                    if (mode == TimeflowView.TimeDisplayModes.Seconds) {
                        float nt = StringUtil.ParseFloat(newtime);
                        v = nt;
                    }
                    else
                    if (mode == TimeflowView.TimeDisplayModes.Frames) {
                        int f = StringUtil.ParseInt(newtime);
                        float ftime = (float)f / (float)Timeflow.Active.FPS;
                        v = ftime;
                    }
                    else
                    if (mode == TimeflowView.TimeDisplayModes.Measures) {
                        v = StringUtil.MeasuresToSeconds(newtime, Timeflow.Active.BPM, Timeflow.Active.BeatsPerBar, Timeflow.Active.BeatNoteSize);
                    }
                    if (!Mathf.Approximately(value, v)) {
                        //Debug.Log($"'{newtime}' {mode} Changed:{value}=>{v}");
                        if (target != null) _Undo(target, "Time Field", $"{v}");
                        value = v;
                    }
                    else value = orig;
                }
            }
            if (!isGlobal && Timeflow.Active != null) {
                if (Timeflow.Active.IsTimeScopeLocalized) {
                    value -= Timeflow.Active.GlobalTimeOffset;
                }
                GUI.color = Color.white;
            }

            RestoreIndent();
            return value;
        }

        #endregion

        #region SELECT MARKER

        public static string FieldMarker(Object target, string label, string markerName)
        {
            ResetLabelWidth();
            BeginHorizontal();

            string[] markers = Timeflow.Active.Markers.GetMarkersList(true);
            if (markers == null) {
                EditorGUILayout.LabelField(label, "No markers defined");
                Warning("No markers have been defined in the Timeline");
            }
            else {
                TimeflowMarker marker = Timeflow.Active.Markers.GetMarker(markerName);
                string m = markerName;
                int lastIndex = marker == null ? -1 : marker.Index;
                int index = EditorGUILayout.Popup(label, lastIndex, markers, GUILayout.Width(300));
                if (index != lastIndex) {
                    if (index == -1) {
                        m = "START";
                    }
                    else {
                        marker = Timeflow.Active.Markers.GetMarker(markers[index]);
                        if (marker != null) {
                            m = marker.Name;
                        }
                        else {
                            m = "END";
                        }
                    }
                }

                if (markerName != m) {
                    if (string.IsNullOrEmpty(label)) label = "Select Marker";
                    if (target != null) _Undo(target, label, $"{m}");
                    markerName = m;
                }

                if (marker == null) {
                    EditorGUILayout.LabelField("Select a Marker", "", GUILayout.Width(100));
                }
                else {
                    EditorGUILayout.LabelField("StartTime:" + marker.Time, "", GUILayout.Width(100));
                }
            }
            EditorGUILayout.EndHorizontal();

            RestoreLabelWidth();
            return markerName;
        }

        public static string FieldMarkerInline(Object target, string markerName)
        {
            return FieldMarkerInline(target, null, markerName);
        }

        public static string FieldMarkerInline(Object target, string label, string markerName)
        {
            if(Timeflow.Active == null || Timeflow.Active.Markers == null) return "";
            ClearIndent();
            ResetLabelWidth();

            string[] markers = null;
            markers = Timeflow.Active.Markers.GetMarkersList(true);
            if (markers == null) {
                LabelInline("No markers defined", "");
                Warning("No markers have been defined in the Timeline");
            }
            else {
                TimeflowMarker marker = Timeflow.Active.Markers.GetMarker(markerName);

                string m = markerName;
                int lastIndex = marker == null ? 0 : marker.Index;
                int index = 0;
                if (string.IsNullOrEmpty(label)) {
                    index = FieldPopupInline(null, lastIndex, markers, GUILayout.Width(160));
                }
                else {
                    index = FieldPopupInline(null, label, lastIndex, markers, GUILayout.Width(CalculateWidth(label) + 160));
                }
                if (index != lastIndex) {
                    if (index == -1) {
                        m = "";
                    }
                    else {
                        marker = Timeflow.Active.Markers.GetMarker(markers[index]);
                        if (marker != null) {
                            m = marker.Name;
                        }
                        else {
                            m = "";
                        }
                    }
                }

                if (markerName != m) {
                    if (string.IsNullOrEmpty(label)) label = "Select Marker";
                    if (target != null) _Undo(target, label, $"{m}");
                    markerName = m;
                }
                if (marker == null) {
                    LabelInline("Select a Marker", "", GUILayout.Width(100));
                }
            }

            RestoreIndent();
            RestoreLabelWidth();
            return markerName;
        }

        public static int FieldMarker(Object target, string label, int markerId, params GUILayoutOption[] options)
        {
            ResetLabelWidth();

            BeginHorizontal();
            string[] markers = Timeflow.Active.Markers.GetMarkersList(true);
            if (markers == null) {
                EditorGUILayout.LabelField(label, "No markers defined");
                Warning("No markers have been defined in the Timeline");
            }
            else {
                TimeflowMarker marker = markerId < 0 ? null : Timeflow.Active.Markers.GetMarker(markerId);

                int id = markerId;
                int lastIndex = marker == null ? markers.Length - 1 : marker.Index;
                int index = EditorGUILayout.Popup(label, lastIndex, markers, GUILayout.Width(300));
                if (index != lastIndex) {
                    if (index == markers.Length - 1) {
                        id = -1;
                    }
                    else {
                        marker = Timeflow.Active.Markers.GetMarker(markers[index]);
                        if (marker != null) {
                            id = marker.ID;
                        }
                        else {
                            id = 0;
                        }
                    }
                }
                if (id < 0) marker = Timeflow.Active.Markers.GetMarker("END");

                if (markerId != id) {
                    if (string.IsNullOrEmpty(label)) label = "Select Marker";
                    if (target != null) _Undo(target, label, $"{id}");
                    markerId = id;
                }

                if (marker == null) {
                    EditorGUILayout.LabelField("Select a Marker", "", GUILayout.Width(100));
                }
            }
            EditorGUILayout.EndHorizontal();

            RestoreLabelWidth();
            return markerId;
        }

        public static int FieldMarkerInline(Object target, int markerId)
        {
            return FieldMarkerInline(target, null, markerId);
        }

        public static int FieldMarkerInline(Object target, string label, int markerId)
        {
            if (Timeflow.Active == null || Timeflow.Active.Markers == null) return 0;
            ClearIndent();
            ResetLabelWidth();

            string[] markers = Timeflow.Active.Markers.GetMarkersList(true);
            if (markers == null) {
                Warning("No markers have been defined in the Timeline");
            }
            else {
                int[] ids = new int[markers.Length];
                ids[0] = 0;
                ids[ids.Length - 1] = -1;

                int index = 0;
                if (markerId == 0) {
                    index = 0;
                }
                else
                if (markerId == -1) {
                    index = markers.Length - 1;
                }

                if (Timeflow.Active.MarkerList != null) {
                    int i = 1;
                    foreach (TimeflowMarker m in Timeflow.Active.MarkerList) {
                        ids[i] = m.ID;
                        if (m.ID == markerId) {
                            index = i;
                        }
                        i++;
                    }
                }

                int id = markerId;
                int selectedIndex = index;
                if (string.IsNullOrEmpty(label)) {
                    selectedIndex = FieldPopupInline(null, index, markers, GUILayout.Width(160));
                }
                else {
                    selectedIndex = FieldPopupInline(null, label, index, markers, GUILayout.Width(CalculateWidth(label) + 160));
                }
                if (index != selectedIndex) {
                    index = selectedIndex;
                    if (selectedIndex == 0) {
                        id = 0; // START
                    }
                    else
                    if (selectedIndex == markers.Length - 1) {
                        id = -1; // END
                    }
                    else {
                        id = ids[selectedIndex]; // offset for START
                    }
                }
                if (markerId != id) {
                    if (string.IsNullOrEmpty(label)) label = "Select Marker";
                    if (target != null) _Undo(target, label, $"{id}");
                    markerId = id;
                }
            }

            RestoreIndent();
            RestoreLabelWidth();
            return markerId;
        }

        #endregion

        #region SELECT CHANNEL

        public static TimeflowChannel FieldChannelInline(Object target, TimeflowObject obj, TimeflowChannel channel)
        {
            return FieldChannel(target, null, obj, channel, true);
        }

        public static TimeflowChannel FieldChannel(Object target, string label, TimeflowObject obj, TimeflowChannel channel)
        {
            return FieldChannel(target, label, obj, channel, false);
        }

        public static TimeflowChannel FieldChannel(Object target, string label, TimeflowObject obj, TimeflowChannel channel, bool inline)
        {
            if (obj == null || obj.AllChannels == null || obj.AllChannels.Count == 0) {
                SetTooltip("There are no Timeflow channels on this object. ");
                if (inline) {
                    LabelInline(label, "No Channels");
                }
                else {
                    Label(label, "No Channels");
                }
                return channel;
            }

            int c = 1;
            int selected = 0;

            List<string> channelNames = new List<string> {
                "(Unassigned)"
            };
            foreach (TimeflowChannel ch in obj.AllChannels) {
                channelNames.Add(ch.Name);
                if (ch != null && ch == channel) {
                    selected = c;
                }
                c++;
            }

            SetTooltip("Select a channel to receive data from. This list is relative to the object select. Use the channel link tool to select a channel on a different object.");
            int sel;
            if (inline) {
                sel = FieldPopupInline(null, label, selected, channelNames.ToArray());
            }
            else {
                sel = FieldPopup(null, label, selected, channelNames.ToArray());
            }
            sel -= 1;

            TimeflowChannel v;
            if (sel < 0) {
                v = null;
            }
            else {
                v = obj.AllChannels[sel];
            }
            if (channel != v) {
                if (string.IsNullOrEmpty(label)) label = "Select Channel";
                if (target != null) _Undo(target, label, $"{(v == null ? "null" : v.Name)}");
                channel = v;
            }

            return channel;
        }

        #endregion

        #region CHANNEL LINK

        public static void FieldChannelLink(TimeflowBehavior target, TimeflowChannel channel)
        {
            if (target == null || channel == null) {
                return;
            }

            BeginVertical("box");
            BeginHorizontal();

            if (channel.Link == null) channel.Link = new TimeflowChannelLink(channel);

            bool hasLink = channel.IsLinked;

            GUI.color = channel.Link.GUIColor;
            if (channel.Link.Enabled) {
                if (ButtonTexture(AxonUI.ChannelLinkOnStyle.normal.background, "Channel Link On")) {
                    channel.Link.Enabled = false;
                }
            }
            else {
                if (ButtonTexture(hasLink ? AxonUI.ChannelLinkOnStyle.normal.background : AxonUI.ChannelLinkOffStyle.normal.background, channel.CanLink ? "Channel Link Off" : "Channel Linking not supported by this channel type")) {
                    channel.Link.Enabled = true;
                }
            }
            GUI.color = AxonColor.Default;
            if (!hasLink) {
                GUI.color = Color.gray;
                LabelInline("No channel link assigned");
                GUI.color = AxonColor.Default;
            }
            else {
                EditorGUI.BeginDisabledGroup(!channel.Link.Enabled);
                float blend = FieldSliderInline(channel.Behavior, "Link Blend", channel.Link.Blend, 0f, 1f);
                if (blend != channel.Link.Blend) {
                    channel.Link.Blend = blend;
                    channel.Behavior.UpdateTimeChannel(channel);
                }
                EditorGUI.EndDisabledGroup();

                if (editChannelLink) {
                    if (ButtonTexture(AxonUI.Icons.Remove, "Remove the channel link")) {
                        channel.RemoveLink();
                    }
                }
                if (ButtonTexture(editChannelLink ? AxonUI.Icons.EditOn : AxonUI.Icons.EditOff, "Edit the channel link")) {
                    editChannelLink = !editChannelLink;
                }
                if (channel.Link != null) channel.Link.DebugEnabled = FieldToggleDebug(channel.Link.DebugEnabled);

                if (editChannelLink) {
                    EndHorizontal(false);
                    BeginHorizontalBox();

                    if (channel.Link == null) channel.Link = new TimeflowChannelLink(channel);
                    SetTooltip("The object providing data to this channel link.");
                    TimeflowObject p = (TimeflowObject)FieldObjectInline(target, "Source", channel.Link.Provider, typeof(TimeflowObject), true);
                    if (p != channel.Link.Provider) {
                        if (p != channel.Object) {
                            channel.RemoveLink();
                            if (p != null) {
                                channel.Link = new TimeflowChannelLink(channel, p);
                            }
                        }
                        DragAndDrop.AcceptDrag();
                        if (Timeflow.Active != null) Timeflow.Active.OnDragCancel();
                    }

                    if (channel.Link.Provider != null && channel.Link.Provider.AllChannels != null && channel.Link.Provider.AllChannels.Count > 0) {
                        // Display a menu list of compatible channels
                        List<TimeflowChannel> channels = new List<TimeflowChannel>();
                        foreach (TimeflowChannel ch in channel.Link.Provider.AllChannels) {
                            if (channel.IsLinkable(ch)) {
                                channels.Add(ch);
                            }
                        }
                        if (channels.Count <= 0) {
                            Warning("There are no compatible channels to link");
                        }
                        else {
                            int c = 0;
                            int selected = 0;
                            GenericMenu menu = new GenericMenu();

                            string name = "Not Assigned";
                            List<string> channelNames = new List<string>();
                            foreach (TimeflowChannel ch in channels) {
                                TimeflowChannelLinkMenuItem item = new TimeflowChannelLinkMenuItem();
                                item.Destination = channel;
                                item.Source = ch;
                                if (ch != channel && channel.IsEnabled && channel.IsLinkable(ch)) {
                                    channelNames.Add(ch.Name);
                                    bool isSelected = false;
                                    if (ch.UniqueID == channel.Link.Channel.UniqueID) {
                                        selected = c;
                                        isSelected = true;
                                        name = ch.Name;
                                    }
                                    menu.AddItem(new GUIContent(ch.Name), isSelected, AssignChannelLink, item);
                                }
                                else {
                                    menu.AddItem(new GUIContent(ch.Name), false, null);
                                }
                                c++;
                            }

                            SetTooltip("Select a channel to receive data from. This list is relative to the object select. Use the channel link tool to select a channel on a different object.");
                            if (ButtonInline(name)) {
                                Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                                menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y));
                            }

                            if (channel.Link.Channel != null && channel.Link.Channel.IsNumber) {
                                channel.Link.AttributeIn = PropertySelectAttribute("In:", channel.Link.Channel.KeyPropertyType, channel.Link.AttributeIn, true, true);

                                SetTooltip("Applies temporal smoothing (in seconds) to the input data");
                                channel.Link.TemporalSmoothing = FieldFloatInline(target, "Smooth", channel.Link.TemporalSmoothing);
                                if (channel.Link.TemporalSmoothing < 0f) channel.Link.TemporalSmoothing = 0f;
                            }
                            EndHorizontal(false);

                            BeginHorizontalBox();
                            channel.Link.Mode = (TimeflowChannelLink.Modes)FieldEnumPopupInline(target, channel.Link.Mode);
                            SetTooltip("Combines the channel values in the opposite order. This determines whether the assigned channel value goes over top or below the current channel.");
                            channel.Link.Reverse = FieldToggleInline(target, "Reverse", channel.Link.Reverse);

                            SetTooltip("If enabled, vector values are scaled to normalized range 0-1.");
                            channel.Link.Normalize = FieldToggleInline(target, "Normalize", channel.Link.Normalize);

                            SetTooltip("If enabled, linked channels are processed right now in world time, neutralizing all time offsets. " +
                                "If disabled, channels are linked in local time with time offsets applied. Turn this off to create time " +
                                "delays and other time-based effects.");
                            channel.Link.UseWorldTime = FieldToggleInline(target, "World Time", channel.Link.UseWorldTime);
                            EndHorizontal(false);

                            BeginHorizontalBox();
                            channel.Link.TimeOffset = FieldFloatInline(target, "Time Offset", channel.Link.TimeOffset, GUILayout.Width(120));

                            SetTooltip("If enabled, the first keyframe on the channel sets the offset time.");
                            channel.Link.TimeOffsetFirstKey = FieldToggleInline(target, "1st Key", channel.Link.TimeOffsetFirstKey);

                            if (channel.Link.TimeOffsetFirstKey) {
                                SetTooltip("If enabled, the first kefyrame time is applied as a negative value. Use this to more easily manage keyframes times in the timeline, instead of having them with negative times before the start time.");
                                channel.Link.TimeOffsetNegative = FieldToggleInline(target, "Negative", channel.Link.TimeOffsetNegative);
                            }

                            channel.Link.TimeScale = FieldFloatInline(target, "Time Scale", channel.Link.TimeScale, GUILayout.Width(120));
                            EndHorizontal(false);

                            if (channel.Link.Mode == TimeflowChannelLink.Modes.Remap) {
                                BeginBox();
                                channel.Link.RemapInRange = FieldVector2(target, "In Range", channel.Link.RemapInRange);
                                channel.Link.RemapOutRange = FieldVector2(target, "Out Range", channel.Link.RemapOutRange);
                                EndBox();
                            }

                            if (channel.Link.Mode == TimeflowChannelLink.Modes.Custom) {
                                BeginHorizontal();
                                channel.Link.CustomLink = (CustomChannelLink)FieldObjectInline(target, "Custom Link", channel.Link.CustomLink, typeof(CustomChannelLink), false);
                                EndHorizontal(false);
                            }

                            BeginHorizontal(); // Closed at end
                            GUI.color = AxonColor.Default;
                        }

                    }
                }
            }
            EndHorizontal(false);
            EndVertical();
        }

        public static void AssignChannelLink(object channel)
        {
            TimeflowChannelLinkMenuItem c = (TimeflowChannelLinkMenuItem)channel;
            if (c.Destination != null && c.Source != null) {
                if (c.Source != c.Destination && c.Destination.IsEnabled && c.Destination.IsLinkable(c.Source)) {
                    if (c.Destination.Link != null) {
                        c.Destination.RemoveLink();
                    }
                    c.Destination.Link = new TimeflowChannelLink(c.Destination, c.Source);
                }
            }
        }

        #endregion
    }

}//AxonGenesis
#endif

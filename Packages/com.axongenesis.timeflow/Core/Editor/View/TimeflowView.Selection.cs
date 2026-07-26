// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region ENUMS

        public enum SelectionModes
        {
            Any,
            KeyframesOnly,
            TracksOnly
        }

        #endregion

        #region PUBLIC

        public event WindowDelegate OnSelectedKeysChanged;

        public SelectionModes SelectionMode = SelectionModes.Any;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool HasFocus = false;

        [NonSerialized]
        public bool HasSelection = false;

        [NonSerialized]
        public int FirstSelectedObjectIndex;

        [NonSerialized]
        public int LastSelectedObjectIndex;

        [NonSerialized]
        public int IncrementSelectedObjectIndex;

        [NonSerialized]
        public List<TimeflowObject> SelectedObjects;

        [NonSerialized]
        private List<TimeflowChannel> _SelectedChannels = null;
        public List<TimeflowChannel> SelectedChannels {
            get {
                if (_SelectedChannels == null) {
                    _SelectedChannels = new List<TimeflowChannel>();
                }
                return _SelectedChannels;
            }
            set {
                _SelectedChannels = value;
            }
        }

        [NonSerialized]
        public List<TimeflowChannel> SelectedTrackChannels;

        [NonSerialized]
        public List<TimeflowBehavior> TouchedObjects;

        [NonSerialized]
        public List<Keyframe> SelectedKeys;

        [NonSerialized]
        public List<Keyframe> KeysToDeselect;

        [NonSerialized]
        public List<Keyframe> RelatedKeys;

        [NonSerialized]
        public List<Keyframe> RelatedTracks;

        [NonSerialized]
        public List<TimeflowEvent> RelatedEvents;

        [NonSerialized]
        public List<Keyframe> LastSelectedKeys = null;

        [NonSerialized]
        public List<TimeflowEvent> SelectedEvents;

        [NonSerialized]
        public List<TimeflowEvent> LastSelectedEvents = null;

        [NonSerialized]
        public List<List<Keyframe>> CopiedKeys = null;

        [NonSerialized]
        public List<List<Keyframe>> CopiedTracks = null;

        [NonSerialized]
        public List<List<TimeflowEvent>> CopiedEvents = null;

        [NonSerialized]
        public TimeflowMarker CopiedMarker = null;

        [NonSerialized]
        public Vector2 MarqueeStart = Vector2.zero;

        [NonSerialized]
        public Vector2 MarqueeEnd = Vector2.zero;

        [NonSerialized]
        public int selectOrder;

        [NonSerialized]
        public bool ObjectTouched;

        [NonSerialized]
        public bool KeyframesTouched = false;

        [NonSerialized]
        public bool AnyTangentTouched;

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private bool _KeyframeSelectionChanged;

        #endregion

        #region CALLBACKS

        public void OnSelectionChange()
        {
            Input.StopEditingName();

            if (Display.ObjectMode == TimeflowViewDisplay.ObjectModes.SelectedObject ||
                Display.ObjectMode == TimeflowViewDisplay.ObjectModes.SelectedGroup) {
                Display.DisplaySelectedObjects(false);
            }
            SelectAllObjects(false, false);
            SelectedObjects = new List<TimeflowObject>();
            if (SelectedChannels == null) SelectedChannels = new List<TimeflowChannel>();
            if (SelectedTrackChannels == null) SelectedTrackChannels = new List<TimeflowChannel>();

            float yScrollOffset = 0;

            if (Selection.gameObjects != null) {
                foreach (GameObject gobj in Selection.gameObjects) {
                    TimeflowObject obj;
                    gobj.TryGetComponent<TimeflowObject>(out obj);
                    if (obj != null && !obj.IsLocked) {
                        if (yScrollOffset == 0) {
                            yScrollOffset = obj.GUIRect.y;
                        }

                        obj.IsSelected = true;
                        obj.WasSelected = true;

                        if (!SelectedObjects.Contains(obj)) {
                            SelectedObjects.Add(obj);
                        }
                        if (!SelectedTrackChannels.Contains(obj.Track)) {
                            obj.Track.IsSelected = true;
                            SelectedTrackChannels.Add(obj.Track);
                        }

                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden && ch.IsSelected && !ch.IsTrack) {
                                    if (!ch.IsGraphLockedOverride && !SelectedChannels.Contains(ch)) {
                                        ch.IsSelected = true;
                                        SelectedChannels.Add(ch);
                                    }
                                }
                            }
                        }

                        Transform parent = obj.gameObject.transform.parent;
                        while (parent != null) {
                            if (parent.TryGetComponent<TimeflowObject>(out var o)) {
                                o.IsCollapsed = false;
                            }
                            parent = parent.parent;
                        }
                    }
                }
            }

            if (TimeflowPreferences.Current.AutoScrollToSelection && Layout != null && Layout.Hierarchy != null) {
                float max = Layout.Hierarchy.Height - 50;
                if (yScrollOffset < 0 || yScrollOffset > max) {
                    ScrollOffset = new Vector2(ScrollOffset.x, ScrollOffset.y - yScrollOffset);
                }
            }

            Info.OnSelectionChanged();
        }

        #endregion

        #region ACCESSORS

        public bool AnyTracksSelected {
            get {
                bool selected = false;
                if (SelectedKeys != null) {
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.IsTrack) {
                            selected = true;
                            break;
                        }
                    }
                }
                return selected;
            }
        }

        public int TracksSelectedCount {
            get {
                int count = 0;
                if (SelectedKeys != null) {
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.IsTrack) {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        #endregion

        #region SELECT OPERATIONS

        public void SelectAll()
        {
            //Debug.Log($"SelectAll FocusedControl:{GUI.GetNameOfFocusedControl()}");
            if (GUI.GetNameOfFocusedControl() == "EditObjectName") {
                AxonGUI.FocusControl("EditObjectName");
                EditorGUI.FocusTextInControl("EditObjectName");
            }
            else
            if (GUI.GetNameOfFocusedControl() == "SearchDisplay") {
                AxonGUI.FocusControl("SearchDisplay");
                EditorGUI.FocusTextInControl("SearchDisplay");
            }
            else {
                if (CurrentFocus == Layout.Hierarchy) {
                    SelectAllObjects(true, true, true);
                }
                else {
                    SelectAllKeys(IsAlt);
                }
                SetEventUsed();
            }
        }

        public void DeselectAllInternal()
        {
            //Input.CancelDrag();
            DeselectChannels(false);

            SetupSelectedKeys(true);
            SelectedEvents = new List<TimeflowEvent>();
            Markers.SelectedMarker = null;
            SelectedEvents = null;
        }

        public void DeselectAll()
        {
            DeselectAllInternal();
            CommitSelection();
        }

        public void SetStartOfSelection()
        {
            if (SelectedKeys == null) return;

            foreach (Keyframe k in SelectedKeys) {
                if (!k.IsTrack) continue;
                if (k.CanDragTimeOffset) {
                    k.Channel.TimeOffsetWorld = Timeflow.CurrentTime;
                }
                else {
                    k.KeyTimeWorld = Timeflow.CurrentTime;
                }
                KeyframesTouched = true;
                ObjectTouched = true;
            }
        }

        public void GotoStartOfSelection()
        {
            if (SelectedKeys == null) return;

            foreach (Keyframe k in SelectedKeys) {
                if (!k.IsTrack) continue;
                Timeflow.CurrentTimeExplicit = k.KeyTimeWorld;
                ScrollCenter();
                break;
            }
        }

        public void SetEndOfSelection()
        {
            if (SelectedKeys == null) return;

            foreach (Keyframe k in SelectedKeys) {
                if (!k.IsTrack) continue;
                k.KeyEndTimeWorld = Timeflow.CurrentTime;
                KeyframesTouched = true;
                ObjectTouched = true;
            }
        }

        public void GotoEndOfSelection()
        {
            if (SelectedKeys == null) return;

            foreach (Keyframe k in SelectedKeys) {
                if (!k.IsTrack) continue;
                Timeflow.CurrentTimeExplicit = k.KeyEndTimeWorld;
                ScrollCenter();
                break;
            }
        }

        public void DuplicateSelection()
        {
            if (Input.LastEventMode == TimeflowViewInput.EventModes.DragKeys ||
                Input.LastEventMode == TimeflowViewInput.EventModes.DragKeyMarquee ||
                Input.LastEventMode == TimeflowViewInput.EventModes.InsertKey ||
                Input.LastEventMode == TimeflowViewInput.EventModes.DragMarker) {
                DuplicateSelectedKeys(0, true);
            }
            else
            if (Input.LastEventMode == TimeflowViewInput.EventModes.ChannelSelect ||
                Input.LastEventMode == TimeflowViewInput.EventModes.DragChannelOrder) {
                DuplicateSelectedChannels();
            }
            else {
                DuplicateSelectedObjects();
            }
        }

        public void ValidateSelection()
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                SelectedChannels.RemoveNulls();
            }
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                List<Keyframe> keys = new List<Keyframe>();
                for (int i = 0; i < SelectedKeys.Count; i++) {
                    if (SelectedKeys[i] != null && SelectedKeys[i].Channel != null) {
                        keys.Add(SelectedKeys[i]);
                    }
                }
                SelectedKeys = keys;
            }
            if (SelectedEvents != null && SelectedEvents.Count > 0) {
                List<TimeflowEvent> events = new List<TimeflowEvent>();
                for (int i = 0; i < SelectedEvents.Count; i++) {
                    if (SelectedEvents[i] != null) {
                        events.Add(SelectedEvents[i]);
                    }
                }
                SelectedEvents = events;
            }
        }

        /// <summary>
        /// Processes the current selection and determines what objects need updating. Selection is
        /// determined by the objects IsSelected flag. 
        /// </summary>
        public void CommitSelection()
        {
            if (Input.IsDragging || Input.IsEditingName) return;

            SelectedObjects = new List<TimeflowObject>();
            //SelectedChannels = new List<TimeflowChannel>();
            SelectedTrackChannels = new List<TimeflowChannel>();
            TouchedObjects = new List<TimeflowBehavior>();
            FirstSelectedObjectIndex = -1;
            LastSelectedObjectIndex = -1;
            IncrementSelectedObjectIndex = 0;

            if (Display.Objects != null) {
                SelectionUtil.Clear(); // Deselect all
                List<UnityEngine.Object> selected = new List<UnityEngine.Object>();
                int i = 0;
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable) {
                        obj.WasSelected = obj.IsSelected;
                        if (obj.IsSelected) {
                            if (FirstSelectedObjectIndex == -1) {
                                FirstSelectedObjectIndex = i;
                            }
                            LastSelectedObjectIndex = i;
                            if (!IsGraphMode) {
                                obj.Track.IsSelected = true;
                            }
                            if (!SelectedTrackChannels.Contains(obj.Track)) {
                                SelectedTrackChannels.Add(obj.Track);
                            }
                        }
                        else
                        if (!IsGraphMode) {
                            obj.Track.IsSelected = false;
                        }
                        if (!obj.IsLocked && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden) {
                                    if (ch.IsTrack) {
                                        if (SelectedTrackChannels.Contains(ch)) {
                                            SelectedTrackChannels.Remove(ch);
                                        }
                                    }
                                    else
                                    if (!ch.IsSelected) {
                                        if (SelectedChannels.Contains(ch)) {
                                            SelectedChannels.Remove(ch);
                                        }
                                    }
                                    else {
                                        obj.IsChannelSelected = true;
                                        if (!ch.IsGraphLockedOverride) {
                                            if (!SelectedChannels.Contains(ch)) {
                                                SelectedChannels.Add(ch);
                                                if (!selected.Contains(obj.gameObject)) {
                                                    selected.Add(obj.gameObject);
                                                }
                                                TouchedObjects.Add(obj);
                                            }
                                            if (ObjectTouched && !TouchedObjects.Contains(obj)) {
                                                TouchedObjects.Add(obj);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else {
                        obj.IsSelected = false;
                        obj.IsChannelSelected = false;
                    }

                    if (obj.IsSelected) {
                        if (!SelectedObjects.Contains(obj)) {
                            SelectedObjects.Add(obj);
                        }
                        if (!selected.Contains(obj.gameObject)) {
                            selected.Add(obj.gameObject);
                        }
                    }
                    i++;
                }
                UnityEngine.Object[] objects = selected.ToArray();
                SelectionUtil.Select(objects);

                // Appears to be a bug in unity where even though multiple objects are defined, only 1 object is in the array afterward
                //Selection.instanceIDs = selectionIds.ToArray();
            }

            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                // Remove any selected keys that may be on a locked channel
                List<Keyframe> toRemove = new List<Keyframe>();
                foreach (Keyframe k in SelectedKeys) {
                    if (k != null && k.Channel != null && (k.Channel.IsLocked || ((!k.Channel.IsSelected && !k.Channel.IsGraphLocked) && IsGraphMode))) {
                        toRemove.Add(k);
                        if (SelectedTrackChannels.Contains(k.Channel)) {
                            SelectedTrackChannels.Remove(k.Channel);
                        }
                        if (SelectedChannels.Contains(k.Channel)) {
                            SelectedChannels.Remove(k.Channel);
                        }
                    }
                }
                if (toRemove.Count > 0) {
                    foreach (Keyframe k in toRemove) {
                        if (k != null) SelectedKeys.Remove(k);
                    }
                }

                foreach (Keyframe k in SelectedKeys) {
                    TimeflowChannel ch = k.Channel;
                    if (ch == null) continue;
                    if (IsGraphMode && !ch.IsTrack && !ch.IsGraphLockedOverride && !SelectedChannels.Contains(ch)) {
                        /// Auto select the channels from the keys selected, but only when viewing in
                        /// the graph mode.
                        ch.IsSelected = true;
                        SelectedChannels.Add(ch);
                    }
                    if (ch.IsTrack && typeof(TimeflowTrack).IsAssignableFrom(ch.GetType())) {
                        TimeflowTrack t = (TimeflowTrack)ch;
                        if (t != null && t.AutoSetWorkArea && !Application.isPlaying) {
                            Timeflow.SetWorkArea(k.KeyTimeWorld, k.KeyEndTimeWorld, false);
                        }
                    }
                    if (ch.ToProperty != null && ch.ToProperty.Comp != null) {
                        TimeflowBehavior t = null;
                        if (typeof(TimeflowBehavior).IsAssignableFrom(ch.ToProperty.Comp.GetType())) {
                            t = (TimeflowBehavior)ch.ToProperty.Comp;
                            TimeflowObject tobj;
                            ch.Behavior.TryGetComponent<TimeflowObject>(out tobj);
                            if (tobj != null && !TouchedObjects.Contains(tobj)) {
                                TouchedObjects.Add(tobj);
                            }
                            if (!TouchedObjects.Contains(t)) {
                                TouchedObjects.Add(t);
                            }
                        }
                    }
                    if (!TouchedObjects.Contains(k.Behavior)) {
                        TouchedObjects.Add(k.Behavior);
                    }
                }
            }

            Info.OnSelectionChanged();

            TimeflowInspector.Refresh();
        }

        public void DeselectObjects() => SelectAllObjects(false);

        public void SelectAllObjects(bool select = true, bool commit = true, bool includeChannels = false)
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable) {
                        obj.IsChannelSelected = select && includeChannels;
                        if (!obj.IsLocked) {
                            obj.IsSelected = select;
                        }
                        if (includeChannels && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden && !ch.IsTrack) ch.IsSelected = select;
                            }
                        }
                    }
                    else {
                        obj.IsSelected = false;
                        obj.IsChannelSelected = false;
                    }
                }

                if (commit) CommitSelection();
            }
        }

        public void SelectObject(TimeflowObject obj, bool select = true)
        {
            if (!obj.IsLocked) {
                obj.IsSelected = select;
                obj.WasSelected = select;
            }
            else {
                obj.IsSelected = false;
                obj.WasSelected = false;
            }
            obj.IsChannelSelected = false;

            if (obj.AllChannels != null && !select) {
                foreach (TimeflowChannel ch in obj.AllChannels) {
                    DeselectChannel(ch);
                }
            }
        }

        private void ApplySelectionModeToSelection()
        {
            if (SelectionMode == SelectionModes.TracksOnly) {

                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    List<Keyframe> tracks = new List<Keyframe>();
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.IsTrack) tracks.Add(k);
                    }
                    SelectedKeys = tracks;
                    SelectedKeysChanged();
                }
            }
            else
            if (SelectionMode == SelectionModes.KeyframesOnly) {
                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    List<Keyframe> keys = new List<Keyframe>();
                    foreach (Keyframe k in SelectedKeys) {
                        if (!k.IsTrack) keys.Add(k);
                    }
                    SelectedKeys = keys;
                    SelectedKeysChanged();
                }

            }
        }

        #endregion

        #region SORTING

        public void SortSelectedObjects()
        {
            if (SelectedObjects == null) return;
            SelectedObjects = SelectedObjects.OrderBy(o => o.GUIRect.y).ToList();
        }

        public void SortSelectedChannels()
        {
            if (SelectedChannels == null) return;
            SelectedChannels = SelectedChannels.OrderBy(o => o.GUIRect.y).ToList();
        }

        #endregion

        #region SELECT CHANNELS

        public void SelectChannel(TimeflowChannel channel, bool clear = true, bool allowGraphLocked = false)
        {
            if (clear) DeselectChannels();
            if (channel != null) {
                if (channel.IsTrack) {
                    if (SelectedTrackChannels == null) SelectedTrackChannels = new List<TimeflowChannel>();
                    if (!SelectedTrackChannels.Contains(channel)) {
                        channel.IsSelected = true;
                        SelectedTrackChannels.Add(channel);
                    }
                }
                else {
                    if (SelectedChannels == null) SelectedChannels = new List<TimeflowChannel>();
                    if ((allowGraphLocked || !channel.IsGraphLockedOverride) && !SelectedChannels.Contains(channel)) {
                        channel.IsSelected = true;
                        SelectedChannels.Add(channel);
                    }
                }
                CommitSelection();
            }
        }

        public void SelectChannels(List<TimeflowChannel> channels, bool clear = true)
        {
            if (clear) {
                DeselectAll();
            }
            if (channels != null && channels.Count > 0) {
                foreach (TimeflowChannel channel in channels) {
                    if (channel != null) {
                        if (channel.IsTrack) {
                            if (SelectedTrackChannels == null) SelectedTrackChannels = new List<TimeflowChannel>();
                            if (!SelectedTrackChannels.Contains(channel)) {
                                channel.IsSelected = true;
                                SelectedTrackChannels.Add(channel);
                            }
                        }
                        else {
                            if (SelectedChannels == null) SelectedChannels = new List<TimeflowChannel>();
                            if (!channel.IsGraphLockedOverride && !SelectedChannels.Contains(channel)) {
                                channel.IsSelected = true;
                                SelectedChannels.Add(channel);
                            }
                        }

                    }
                }
                CommitSelection();
            }
        }

        public void SelectKeysInChannel(TimeflowChannel ch)
        {
            SetupSelectedKeys();

            if (ch != null && ch.Keys != null && ch.IsEnabled) {
                foreach (Keyframe k in ch.Keys) {
                    if (!SelectedKeys.Contains(k)) {
                        k.UpdateSelectedAttributes(true);
                        SelectKeyClear(k, false);
                    }
                }
            }
            SelectedKeysChanged();
        }

        public void SelectChannelsInObjects(List<TimeflowObject> objects)
        {
            if (objects == null || objects.Count == 0) return;
            List<TimeflowChannel> selected = new List<TimeflowChannel>();
            foreach (TimeflowObject obj in objects) {
                if (obj.AllChannels != null && obj.AllChannels.Count > 0) {
                    foreach (TimeflowChannel ch in obj.AllChannels) {
                        if (!ch.IsLocked && !ch.IsHidden && !ch.IsTrack) {
                            selected.Add(ch);
                        }
                    }
                }
            }
            SelectChannels(selected, true);
        }

        public void SelectChannelsFromKeys()
        {
            foreach (Keyframe key in SelectedKeys) {
                Keyframer kf = key.Behavior as Keyframer;
                if (key.Behavior != null && key.Behavior.Channels != null) {
                    foreach (TimeflowChannel ch in key.Behavior.Channels) {
                        if (!ch.IsHidden) {
                            foreach (Keyframe k in ch.Keys) {
                                if (k == key) {
                                    ch.IsSelected = true;
                                }
                            }
                        }
                    }
                }
            }
            CommitSelection();
        }

        public void SelectKeysInSelectedChannels()
        {
            if (SelectedChannels != null) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch != null) {
                        SelectKeysInChannel(ch);
                    }
                }
            }
        }

        public void DeselectChannels(bool commit = true)
        {
            if (Display.Objects != null && Display.Objects.Count > 0) {
                foreach (TimeflowObject obj in Display.Objects) {
                    obj.IsSelected = false;
                    if (obj.AllChannels != null && obj.AllChannels.Count > 0) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            ch.IsSelected = false;
                        }
                    }
                }
            }
            SelectedChannels = null;
            if (commit) CommitSelection();
        }

        public void DeselectChannel(TimeflowChannel ch)
        {
            if (ch != null) {
                ch.IsSelected = false;
                if (SelectedChannels != null && SelectedChannels.Contains(ch)) {
                    SelectedChannels.Remove(ch);
                }
                DeselectKeysInChannel(ch);
            }
        }

        public void DeselectKeysInChannel(TimeflowChannel ch)
        {
            if (ch != null && SelectedKeys != null && ch.Keys != null) {
                bool changed = false;
                foreach (Keyframe k in ch.Keys) {
                    if (SelectedKeys.Contains(k)) {
                        SelectedKeys.Remove(k);
                        changed = true;
                    }
                }
                if (changed) SelectedKeysChanged();
            }
        }

        #endregion

        #region SELECT KEYS

        public void DeselectKey(Keyframe key)
        {
            SetupSelectedKeys();
            if (SelectedKeys != null && SelectedKeys.Contains(key)) {
                SelectedKeys.Remove(key);
            }
        }

        public bool SelectKey(Keyframe key, bool canClear = true)
        {
            if (key == null) return false;
            if (canClear && !IsGraphMode && TimeflowPreferences.Current.TracksSelectObjects) {
                if (!IsShift && !IsControl && Input.EventMode != TimeflowViewInput.EventModes.DragKeyMarquee) {
                    DeselectAll();
                }
            }
            SetupSelectedKeys();

            bool wasSelected = false;
            bool canSelect = true;
            if (IsGraphMode) {
                if (key.IsTrack) {
                    canSelect = false;
                }
            }
            else
            if (SelectionMode == SelectionModes.KeyframesOnly) {
                if (key.IsTrack) {
                    canSelect = false;
                }
            }
            else
            if (SelectionMode == SelectionModes.TracksOnly) {
                if (!key.IsTrack) {
                    canSelect = false;
                }
            }

            if (canSelect && !SelectedKeys.Contains(key)) {
                Info.Mode = key.IsTrack && !IsGraphMode ? TimeflowViewInfo.Modes.Tracks : TimeflowViewInfo.Modes.Keyframes;
                key.SelectOrder = selectOrder;
                selectOrder++;
                SelectedKeys.Add(key);
                wasSelected = true;
            }

            if (!IsGraphMode && canSelect && TimeflowPreferences.Current.TracksSelectObjects && key.Channel != null) {
                if (key.Channel.Object != null) {
                    SelectObject(key.Channel.Object);
                }
                if (!key.IsTrack) {
                    SelectChannel(key.Channel, false);
                }
            }

            return wasSelected;
        }

        public bool SelectKeyClear(Keyframe key, bool clear)
        {
            if (IsShift) clear = false;
            if (clear) DeselectKeys();
            bool wasSelected = SelectKey(key, clear);
            SelectedKeysChanged();

            if (clear && key.Channel != null) {
                // When key is individually selected, notify channel
                key.Channel.OnKeySelected(key);
            }
            if (clear) Info.Mode = key.IsTrack ? TimeflowViewInfo.Modes.Tracks : TimeflowViewInfo.Modes.Keyframes;
            return wasSelected;
        }

        public bool KeysSelected(bool isDoubleClick = false)
        {
            _KeyframeSelectionChanged = false;
            bool changed = false;
            Input.DragPrimaryKey = null;
            KeysToDeselect = null;

            if (Layout.TimeAreaInner.HitTest(Event.current.mousePosition)) {
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);
                int pad = 2;

                // First check to see if the user clicked a key that's already selected
                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    SelectedKeys.Sort(KeyframeSort.BySizeAsc);

                    foreach (Keyframe k in SelectedKeys) {
                        if (k.HasMultipleAttributes && IsGraphMode) {
                            int ac = k.AttributeCount;
                            bool selected0 = ShowChannel0 && k.GUIRect.Contains(p);
                            bool selected1 = !selected0 && ShowChannel1 && ac > 1 && k.GUIRect1.Contains(p);
                            bool selected2 = !selected0 && !selected1 && ShowChannel2 && ac > 2 && k.GUIRect2.Contains(p);
                            bool selected3 = !selected0 && !selected1 && !selected2 && ShowChannel3 && ac > 3 && k.GUIRect3.Contains(p);


                            if (selected0 || selected1 || selected2 || selected3) {
                                _KeyframeSelectionChanged = true;
                                changed = true;
                                Input.DragPrimaryKey = k;
                                if (Event.current.shift) {
                                    if (selected0) {
                                        k.AttributeSelected0 = !k.AttributeSelected0;
                                    }
                                    else
                                    if (selected1) {
                                        k.AttributeSelected1 = !k.AttributeSelected1;
                                    }
                                    else
                                    if (selected2) {
                                        k.AttributeSelected2 = !k.AttributeSelected2;
                                    }
                                    else
                                    if (selected3) {
                                        k.AttributeSelected3 = !k.AttributeSelected3;
                                    }
                                    if (!k.AttributeSelected0 && !k.AttributeSelected1 && !k.AttributeSelected2 && !k.AttributeSelected3 && SelectedKeys.Count > 1) {
                                        SelectedKeys.Remove(k);
                                    }
                                    break;
                                }

                                // If the selected attribute key wasn't selected already, then deselect the other ones so only this one is picked
                                if (selected0 && !k.AttributeSelected0) {
                                    k.AttributeSelected1 = k.AttributeSelected2 = k.AttributeSelected3 = false;
                                }
                                else
                                if (selected1 && !k.AttributeSelected1) {
                                    k.AttributeSelected0 = k.AttributeSelected2 = k.AttributeSelected3 = false;
                                }
                                else
                                if (selected2 && !k.AttributeSelected2) {
                                    k.AttributeSelected1 = k.AttributeSelected0 = k.AttributeSelected3 = false;
                                }
                                else
                                if (selected3 && !k.AttributeSelected3) {
                                    k.AttributeSelected1 = k.AttributeSelected2 = k.AttributeSelected0 = false;
                                }

                                if (selected0 || k.AttributeSelected0) {
                                    k.AttributeSelected0 = true;
                                    Input.DragChannelIndex = 0;
                                }
                                else {
                                    k.AttributeSelected0 = false;
                                }
                                if (selected1 || k.AttributeSelected1) {
                                    k.AttributeSelected1 = true;
                                    Input.DragChannelIndex = 1;
                                }
                                else {
                                    k.AttributeSelected1 = false;
                                }
                                if (selected2 || k.AttributeSelected2) {
                                    k.AttributeSelected2 = true;
                                    Input.DragChannelIndex = 2;
                                }
                                else {
                                    k.AttributeSelected2 = false;
                                }
                                if (selected3 || k.AttributeSelected3) {
                                    k.AttributeSelected3 = true;
                                    Input.DragChannelIndex = 3;
                                }
                                else {
                                    k.AttributeSelected3 = false;
                                }
                                break;
                            }
                        }
                        else
                        if (RectUtil.Contains(k.GUIRect, p, pad)) {
                            Input.DragPrimaryKey = k;
                            _KeyframeSelectionChanged = true;
                            if (Event.current.shift && SelectedKeys.Count > 1) {
                                if (KeysToDeselect == null) KeysToDeselect = new List<Keyframe>();
                                KeysToDeselect.Add(k);
                                changed = true;
                            }
                            break;
                        }
                    }
                }

                if (!_KeyframeSelectionChanged) {
                    if (Display.Objects == null) return false;
                    if (!Event.current.shift) {
                        SetupSelectedKeys(true);
                        changed = true;
                    }
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (!_KeyframeSelectionChanged && !obj.IsLocked && obj.IsSelectable && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsHidden && !ch.IsLocked && (!obj.IsCollapsed || ch.IsTrack) && ch.Keys != null) {

                                    int attributeCount = ch.AttributeCount;
                                    foreach (Keyframe k in ch.Keys) {
                                        if (k.IsTrack) {
                                            if (IsGraphMode) continue;
                                            if (SelectionMode == SelectionModes.KeyframesOnly) continue;
                                        }
                                        else
                                        if (!IsGraphMode) {
                                            if (SelectionMode == SelectionModes.TracksOnly) continue;
                                        }
                                        if (!IsGraphMode || ch.IsSelected || ch.IsGraphLocked) {
                                            if (IsGraphMode && IsGraphSolo && !ch.IsSelected) {
                                                if(!isDoubleClick) continue;

                                                //ch.IsSelected = true;
                                                SelectChannel(ch, true, true);
                                            }
                                            if (IsGraphMode && attributeCount > 1 && !ch.IsTrack) {
                                                int ac = attributeCount;
                                                bool k0 = ShowChannel0 && RectUtil.Contains(k.GUIRect, p, pad);
                                                bool k1 = ShowChannel1 && ac > 1 && RectUtil.Contains(k.GUIRect1, p, pad);
                                                bool k2 = ShowChannel2 && ac > 2 && RectUtil.Contains(k.GUIRect2, p, pad);
                                                bool k3 = ShowChannel3 && ac > 3 && RectUtil.Contains(k.GUIRect3, p, pad);

                                                if (k0 || k1 || k2 || k3) {
                                                    _KeyframeSelectionChanged = true;

                                                    // Selected keyframes are already handled above - so assume only deselected keys here
                                                    k.AttributeSelected0 = k0;
                                                    k.AttributeSelected1 = k1 && !k.AttributeSelected0;
                                                    k.AttributeSelected2 = k2 && !k.AttributeSelected0 && !k.AttributeSelected1;
                                                    k.AttributeSelected3 = k3 && !k.AttributeSelected0 && !k.AttributeSelected1 && !k.AttributeSelected2;

                                                    if (k.AttributeSelected0) {
                                                        Input.DragChannelIndex = 0;
                                                    }
                                                    else
                                                    if (k.AttributeSelected1) {
                                                        Input.DragChannelIndex = 1;
                                                    }
                                                    else
                                                    if (k.AttributeSelected2) {
                                                        Input.DragChannelIndex = 2;
                                                    }
                                                    else
                                                    if (k.AttributeSelected3) {
                                                        Input.DragChannelIndex = 3;
                                                    }

                                                    Input.DragPrimaryKey = k;
                                                    SelectKey(k);

                                                    changed = true;
                                                    break;
                                                }
                                            }
                                            else {
                                                _KeyframeSelectionChanged = k.GUIRect.Contains(p);
                                            }
                                            if (_KeyframeSelectionChanged) {
                                                Input.DragPrimaryKey = k;
                                                SelectKey(k);
                                                changed = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (_KeyframeSelectionChanged) break;
                            }
                        }
                        if (_KeyframeSelectionChanged) break;
                    }
                }
            }
            if (_KeyframeSelectionChanged && !Event.current.shift) {
                SelectedEvents = null;
            }
            if (changed) {
                SelectedKeysChanged();
                CommitSelection();
            }
            return _KeyframeSelectionChanged;
        }

        public bool TangentSelected()
        {
            AnyTangentTouched = false;
            bool selectionChanged = false;

            if (IsGraphMode && Input.GraphEditMode != TimeflowViewInput.GraphEditModes.KeysOnly && Layout.TimeAreaInner.HitTest(Event.current.mousePosition)) {
                int pad = 2;
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);
                if (Display.Objects != null) {
                    foreach (TimeflowObject item in Display.Objects) {
                        if (!AnyTangentTouched && item.AllChannels != null && item.IsSelectable) {
                            foreach (TimeflowChannel ch in item.AllChannels) {
                                if (ch.IsEnabled && ch.Interpolation == TimeflowChannel.Interpolations.Bezier && !ch.IsHidden && ch.IsDisplayed && !ch.IsTrack && ch.Keys != null &&
                                    (ch.IsSelected || ch.IsGraphLocked)) {
                                    int ki = 0; // use key index to ignore inTan for first key, and outTan for last key
                                    foreach (Keyframe k in ch.Keys) {
                                        if (k.Linear) continue;
                                        if (ki > 0 && RectUtil.Contains(k.InPointRect, p, pad)) {
                                            AnyTangentTouched = true;
                                            Input.DragTangent = k;
                                            Input.DragTangentIn = true;
                                            if (Input.GraphEditMode != TimeflowViewInput.GraphEditModes.TangentsOnly) {
                                                selectionChanged = SelectKeyClear(k, !Event.current.shift && (SelectedKeys == null || !SelectedKeys.Contains(k)));
                                            }
                                            break;
                                        }
                                        else
                                        if (ki < ch.Keys.Count - 1 && RectUtil.Contains(k.OutPointRect, p, pad)) {
                                            Input.DragTangent = k;
                                            AnyTangentTouched = true;
                                            Input.DragTangentIn = false;
                                            if (Input.GraphEditMode != TimeflowViewInput.GraphEditModes.TangentsOnly) {
                                                selectionChanged = SelectKeyClear(k, !Event.current.shift && (SelectedKeys == null || !SelectedKeys.Contains(k)));
                                            }
                                            break;
                                        }
                                        ki++;
                                    }
                                }
                                if (AnyTangentTouched) break;
                            }
                        }
                        if (AnyTangentTouched) break;
                    }
                }
            }

            if (selectionChanged) SelectedKeysChanged();
            return AnyTangentTouched;
        }

        public void SelectKeysAtCurrentTime(SelectionModes mode = SelectionModes.Any)
        {
            SetupSelectedKeys(true);

            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.IsEnabled) {
                                bool canSelect = true;
                                if (mode == SelectionModes.KeyframesOnly) {
                                    canSelect = !ch.IsTrack;
                                }
                                else
                                if (mode == SelectionModes.TracksOnly) {
                                    canSelect = ch.IsTrack;
                                }
                                if (canSelect) {
                                    foreach (Keyframe k in ch.Keys) {
                                        if (k.IsTrack) {
                                            if (k.KeyTime <= Timeflow.CurrentTime && k.KeyValue >= Timeflow.CurrentTime) {
                                                SelectKey(k);
                                            }
                                        }
                                        else
                                        if (!MathUtil.IsTimeDifferent(k.KeyTime, Timeflow.CurrentTime)) {
                                            SelectKey(k);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SelectKeysByValue()
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0 && Display.Objects != null) {
                if (SelectedKeys[0].IsColor) {
                    List<Color> values = new List<Color>();

                    foreach (Keyframe k in SelectedKeys) {
                        if (!k.IsTrack) {
                            values.Add(k.KeyColor);
                        }
                    }

                    SetupSelectedKeys(true);
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsHidden && ch != obj.Track && (!IsGraphMode || ch.IsSelected || ch.IsGraphLocked)) {
                                    foreach (Keyframe k in ch.Keys) {
                                        foreach (Color v in values) {
                                            bool add = true;
                                            if (k.AttributeSelected0 && v.r != k.KeyColor.r) {
                                                add = false;
                                            }
                                            if (k.AttributeSelected1 && v.g != k.KeyColor.g) {
                                                add = false;
                                            }
                                            if (k.AttributeSelected2 && v.b != k.KeyColor.b) {
                                                add = false;
                                            }
                                            if (k.AttributeSelected2 && v.a != k.KeyColor.a) {
                                                add = false;
                                            }
                                            if (add) {
                                                k.AttributeSelected0 = SelectedKeys[0].AttributeSelected0;
                                                k.AttributeSelected1 = SelectedKeys[0].AttributeSelected1;
                                                k.AttributeSelected2 = SelectedKeys[0].AttributeSelected2;
                                                k.AttributeSelected3 = SelectedKeys[0].AttributeSelected3;

                                                SelectKey(k);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                if (SelectedKeys[0].IsVector) {
                    List<Vector4> values = new List<Vector4>();

                    Keyframe src = SelectedKeys[0];

                    foreach (Keyframe k in SelectedKeys) {
                        if (!k.IsTrack) {
                            values.Add(k.KeyVector);
                        }
                    }

                    SetupSelectedKeys(true);
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsHidden && ch != obj.Track && (!IsGraphMode || ch.IsSelected || ch.IsGraphLocked)) {
                                    foreach (Keyframe k in ch.Keys) {
                                        foreach (Vector4 v in values) {
                                            bool add = true;
                                            if (src.AttributeSelected0 && v.x != k.KeyVector.x) {
                                                add = false;
                                            }
                                            if (src.AttributeSelected1 && v.y != k.KeyVector.y) {
                                                add = false;
                                            }
                                            if (src.AttributeSelected2 && v.z != k.KeyVector.z) {
                                                add = false;
                                            }
                                            if (src.AttributeSelected3 && v.w != k.KeyVector.w) {
                                                add = false;
                                            }
                                            if (add) {
                                                k.AttributeSelected0 = src.AttributeSelected0;
                                                k.AttributeSelected1 = src.AttributeSelected1;
                                                k.AttributeSelected2 = src.AttributeSelected2;
                                                k.AttributeSelected3 = src.AttributeSelected3;

                                                SelectKey(k);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    List<float> values = new List<float>();

                    foreach (Keyframe k in SelectedKeys) {
                        if (!k.IsTrack) {
                            values.Add(k.KeyValue);
                        }
                    }

                    SetupSelectedKeys(true);
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsHidden && ch != obj.Track && (!IsGraphMode || ch.IsSelected || ch.IsGraphLocked)) {
                                    foreach (Keyframe k in ch.Keys) {
                                        foreach (float v in values) {
                                            if (v == k.KeyValue) {
                                                SelectKey(k);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SelectTracksByColor()
        {
            if (SelectedKeys != null && Display.Objects != null) {
                List<Color> colors = new List<Color>();

                foreach (Keyframe k in SelectedKeys) {
                    if (k.IsTrack) {
                        colors.Add(k.OverrideGUIColor ? k.KeyColor : k.Channel.GUIColor);
                    }
                }

                SetupSelectedKeys(true);
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                        foreach (Keyframe k in obj.Track.Keys) {
                            if (colors.Contains(k.OverrideGUIColor ? k.KeyColor : k.Channel.GUIColor)) {
                                SelectKey(k);
                            }
                        }
                    }
                }
            }
        }

        public void SelectAllKeys(bool inWorkAreaOnly = false, SelectionModes mode = SelectionModes.Any)
        {
            if (!EditorUtil.ShiftKey) {
                SetupSelectedKeys(true);
                SelectedEvents = new List<TimeflowEvent>();
            }

            if (IsGraphMode) {
                if (Display.Objects != null) {
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (obj.IsSelectable && !obj.IsLocked) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsLocked && !ch.IsHidden && (ch.IsSelected || ch.IsGraphLocked) && ch.Keys != null) {
                                    bool canSelect = true;
                                    if (mode == SelectionModes.KeyframesOnly) {
                                        canSelect = !ch.IsTrack;
                                    }
                                    else
                                    if (mode == SelectionModes.TracksOnly) {
                                        canSelect = ch.IsTrack;
                                    }
                                    if (canSelect) {
                                        foreach (Keyframe k in ch.Keys) {
                                            if (inWorkAreaOnly) {
                                                if (k.KeyTime >= Timeflow.WorkAreaStart && k.KeyTime <= Timeflow.WorkAreaEndTimeExact) {
                                                    k.SelectAlLChannels(true);
                                                    if (!SelectedKeys.Contains(k)) {
                                                        k.AttributeSelected0 = ShowChannel0;
                                                        k.AttributeSelected1 = ShowChannel1;
                                                        k.AttributeSelected2 = ShowChannel2;
                                                        k.AttributeSelected3 = ShowChannel3;
                                                        SelectKey(k);
                                                    }
                                                }
                                            }
                                            else {
                                                k.SelectAlLChannels(true);
                                                if (!SelectedKeys.Contains(k)) {
                                                    k.AttributeSelected0 = ShowChannel0;
                                                    k.AttributeSelected1 = ShowChannel1;
                                                    k.AttributeSelected2 = ShowChannel2;
                                                    k.AttributeSelected3 = ShowChannel3;
                                                    SelectKey(k);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (mode == SelectionModes.Any) {
                        if (obj.Events != null && obj.Events.Count > 0 && obj.IsSelectable && !obj.IsLocked) {
                            foreach (TimeflowEvent evt in obj.Events) {
                                if (inWorkAreaOnly) {
                                    if (evt.TriggerTimeWorld >= Timeflow.WorkAreaStart && evt.TriggerTimeWorld <= Timeflow.WorkAreaEndTimeExact) {
                                        if(SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                                        SelectedEvents.Add(evt);
                                        evt.IsSelected = true;
                                    }
                                }
                                else {
                                    if (SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                                    SelectedEvents.Add(evt);
                                    evt.IsSelected = true;
                                }
                            }
                        }
                    }
                    if (obj.BehaviorsEnabled) {
                        if (!obj.IsLocked && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsEnabled && !ch.IsLocked && !ch.IsHidden && (!IsGraphMode || !ch.IsTrack)) {
                                    bool canSelect = true;
                                    if (mode == SelectionModes.KeyframesOnly) {
                                        canSelect = !ch.IsTrack;
                                    }
                                    else
                                    if (mode == SelectionModes.TracksOnly) {
                                        canSelect = ch.IsTrack;
                                    }
                                    if (canSelect) {
                                        foreach (Keyframe k in ch.Keys) {
                                            float keyTime = k.KeyTimeWorld;
                                            float keyValue = k.KeyEndTimeWorld;
                                            if (inWorkAreaOnly) {
                                                if (k.IsTrack) {
                                                    if (keyTime >= Timeflow.WorkAreaStart && keyValue <= Timeflow.WorkAreaEndTimeExact
                                                    ) {
                                                        SelectKey(k);
                                                    }
                                                }
                                                else
                                                if (keyTime >= Timeflow.WorkAreaStart && keyTime <= Timeflow.WorkAreaEndTimeExact) {
                                                    SelectKey(k);
                                                }
                                            }
                                            else {
                                                SelectKey(k);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SelectedKeysChanged();
        }

        public void SelectedKeysChanged()
        {
            AlignTools.Refresh();
            if (!Input.IsDragging) {
                /// only update the related keys if not dragging
                GetTrackRelatedKeys();
            }
            /// Get a list of all the behaviors affected by the key changes
            List<TimeflowBehavior> objects = new List<TimeflowBehavior>();

            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.Channel != null && k.Channel.Behavior != null && !objects.Contains(k.Channel.Behavior)) {
                        if (!Input.IsDragging) k.Channel.OnKeySelected(k);
                        objects.Add(k.Channel.Behavior);
                    }
                }
            }
            if (RelatedKeys != null && RelatedKeys.Count > 0) {
                foreach (Keyframe k in RelatedKeys) {
                    if (k.Channel != null && k.Channel.Behavior != null && !objects.Contains(k.Channel.Behavior)) {
                        objects.Add(k.Channel.Behavior);
                    }
                }
            }
            if (SelectedEvents != null && SelectedEvents.Count > 0) {
                foreach (TimeflowEvent ev in SelectedEvents) {
                    if (ev.ParentObject != null && !objects.Contains(ev.ParentObject)) {
                        objects.Add(ev.ParentObject);
                    }
                }
            }
            if (RelatedEvents != null && RelatedEvents.Count > 0) {
                foreach (TimeflowEvent ev in RelatedEvents) {
                    if (ev.ParentObject != null && !objects.Contains(ev.ParentObject)) {
                        objects.Add(ev.ParentObject);
                    }
                }
            }
            if (objects != null && objects.Count > 0) {
                /// Ensures that all touched objects are recorded for any prefabs
                foreach (TimeflowBehavior obj in objects) {
                    if (obj != null) {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                    }
                }
            }

            ObjectTouched = true;
            if (OnSelectedKeysChanged != null) OnSelectedKeysChanged();
        }

        public void DeselectKeys()
        {
            if (!IsGraphMode && TimeflowPreferences.Current.TracksSelectObjects) {
                DeselectObjects();
            }
            SetupSelectedKeys(true);
            SelectedEvents = new List<TimeflowEvent>();
            RelatedEvents = null;
            RelatedKeys = null;
            RelatedTracks = null;
            SelectedKeysChanged();

            if (!Input.IsDragging && LastSelectedKeys != null && LastSelectedKeys.Count > 0) {
                /// Notify affected channels of keyframe deselection 
                foreach (Keyframe k in LastSelectedKeys) {
                    if (k.Channel != null && k.Channel.Behavior != null) {
                        k.Channel.OnKeySelected(k);
                    }
                }
            }

        }

        public void SelectRelatedKeys()
        {
            GetTrackRelatedKeys(true);
            if (RelatedKeys != null && RelatedKeys.Count > 0) {
                SetupSelectedKeys();
                foreach (Keyframe k in RelatedKeys) {
                    if (!SelectedKeys.Contains(k)) {
                        SelectKey(k);
                    }
                }
                RelatedKeys = null;
            }
            if (RelatedTracks != null && RelatedTracks.Count > 0) {
                SetupSelectedKeys();
                foreach (Keyframe k in RelatedTracks) {
                    if (!SelectedKeys.Contains(k)) {
                        SelectKey(k);
                    }
                }
                RelatedTracks = null;
            }
            if (RelatedEvents != null && RelatedEvents.Count > 0) {
                if (SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                foreach (TimeflowEvent e in RelatedEvents) {
                    if (!SelectedEvents.Contains(e)) {
                        SelectedEvents.Add(e);
                        e.IsSelected = true;
                    }
                }
                RelatedEvents = null;
            }

            CommitSelection();
            SelectedKeysChanged();
        }

        #endregion

        #region RELATED KEYS

        /// <summary>
        /// Finds all tracks, keyframes, and events under a selection of tracks on the specified objects.
        /// </summary>
        /// <param name="tracks">The reference tracks providing the time ranges to search for keyframes and
        ///     events.</param>
        /// <param name="objects">A list of objects to compare against the tracks. This should be objects
        ///     and children related to the tracks.</param>
        /// <param name="relatedKeys">Output list for any new related keyframes found.</param>
        /// <param name="relatedTracks"></param>
        /// <param name="relatedEvents"></param>
        /// <param name="allowTimeOffset">If false, objects with Drag Time Offset enabled are ignored
        ///     </param>
        public void GetTrackRelatedKeys(
            List<Keyframe> tracks,
            List<TimeflowObject> objects,
            ref List<Keyframe> relatedKeys,
            ref List<Keyframe> relatedTracks,
            ref List<TimeflowEvent> relatedEvents,
            bool allowTimeOffset,
            bool includeHierarchy)
        {
            //Debug.Log($"GetTrackRelatedKeys: allowTimeOffset:{allowTimeOffset} includeHierarchy:{includeHierarchy} tracks:{(tracks == null ? "NULL" : tracks.Count)} objects:{(objects == null ? "NULL" : objects.Count)}");
            if (tracks != null && tracks.Count > 0 && objects != null && objects.Count > 0) {
                foreach (Keyframe track in tracks) {
                    if (!track.IsTrack) continue;

                    // If the track is using drag time offset, then adding the related keys cause a conflict
                    // when the keys are dragged. So we skip the related keys in this case because the drag
                    // time offset takes care of it implicitly.
                    //if (!allowTimeOffset && track.Channel.Object.CanDragTimeOffset) continue;

                    // Compare each object to find keys and events with the set of track ranges
                    foreach (TimeflowObject obj in objects) {
                        List<TimeflowEvent> events = obj.GetEvents();
                        if (events != null) {
                            foreach (TimeflowEvent ev in events) {
                                if (SelectedEvents != null && SelectedEvents.Contains(ev)) {
                                    // skip
                                }
                                else
                                if (!relatedEvents.Contains(ev)) {
                                    float keyTime = ev.TriggerTimeWorld;
                                    if (keyTime >= track.KeyTime && keyTime < track.KeyValue) {
                                        ev.GetTempValues();
                                        relatedEvents.Add(ev);
                                    }
                                }
                            }
                        }

                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch == null) continue;
                            if (ch.Object == null) {
                                continue;
                            }
                            if (track.Channel.Object == null) {
                                continue;
                            }
                            //if (!allowTimeOffset && ch.Object.CanDragTimeOffset) {
                            //    if (!track.Channel.Object.Children.Contains(ch.Object)) {
                            //        continue;
                            //    }
                            //}
                            bool isRelatable = track.Channel != null && (track.Channel.Object == ch.Object ||
                                (includeHierarchy && ObjectUtil.IsDescendant(ch.Object.gameObject, track.Channel.Object.gameObject)));

                            if (ch.IsTrack) {
                                // Don't allow tracks on the same object to be related
                                foreach (Keyframe t in tracks) {
                                    if (t.Channel == ch) {
                                        isRelatable = false;
                                        break;
                                    }
                                }
                            }

                            //Debug.Log($"{obj.name}.Track isRelatable:{isRelatable} isLocked:{ch.IsLocked}");
                            if (isRelatable && !ch.IsLocked && ch.Keys != null) {
                                int i = 0;
                                int count = ch.Keys.Count - 1;
                                foreach (Keyframe k in ch.Keys) {
                                    if (SelectedKeys != null && SelectedKeys.Contains(k)) {
                                        //Debug.Log($"{obj.name}.Track SKIP");
                                    }
                                    else
                                    if (ch.IsTrack) {
                                        if (!relatedTracks.Contains(k)) {
                                            float keyTime = k.KeyTime + ch.TimeOffset;// World;
                                            float keyEnd = k.KeyValue + ch.TimeOffset;
                                            bool endsMatch = keyTime <= track.KeyValue;
                                            if (i < count) {
                                                // Allow the next track section to own keys starting at the same time
                                                Keyframe next = ch.Keys[i + 1];
                                                if (keyTime >= next.KeyTime) {
                                                    endsMatch = false;
                                                }
                                            }
                                            if ((keyTime >= track.KeyTime && keyTime <= track.KeyValue) ||
                                                (keyEnd >= track.KeyValue && keyEnd <= track.KeyValue)) {
                                                //Debug.Log($"RELATED: {track.KeyTime} >= {keyTime} <= {track.KeyValue} endsMatch:{endsMatch}");
                                                k.StoreTempData();
                                                relatedTracks.Add(k);
                                            }
                                            else
                                            if (keyTime >= track.KeyTime && endsMatch) {
                                                //Debug.Log($"RELATED: {track.KeyTime} >= {keyTime} <= {track.KeyValue} endsMatch:{endsMatch}");
                                                k.StoreTempData();
                                                relatedTracks.Add(k);
                                            }
                                            //else {
                                            //    Debug.Log($"{track.KeyTime} >= {keyTime} <= {track.KeyValue} endsMatch:{endsMatch}");
                                            //}
                                        }
                                    }
                                    else
                                    if (!relatedKeys.Contains(k)) {
                                        float keyTime = k.KeyTime;// World;
                                        bool endsMatch = keyTime <= track.KeyValue;
                                        //Debug.Log($"{track.KeyTime} >= {keyTime} <= {track.KeyValue}");
                                        if (i < count) {
                                            // Allow the next track section to own keys starting at the same time
                                            Keyframe next = ch.Keys[i + 1];
                                            if (keyTime >= next.KeyTime) {
                                                endsMatch = false;
                                            }
                                        }
                                        if (keyTime >= track.KeyTime && endsMatch) {
                                            k.StoreTempData();
                                            relatedKeys.Add(k);
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
            }

        }

        public void GetTrackRelatedKeys(bool allowTimeOffset = false)
        {
            if (SelectedKeys == null || SelectedKeys.Count == 0) return;

            RelatedKeys = new List<Keyframe>();
            RelatedTracks = new List<Keyframe>();
            RelatedEvents = new List<TimeflowEvent>();

            List<TimeflowObject> relatedObjects = GetTrackRelatedObjects(SelectedKeys, allowTimeOffset);

            GetTrackRelatedKeys(SelectedKeys, relatedObjects, ref RelatedKeys, ref RelatedTracks, ref RelatedEvents, allowTimeOffset, true);
        }

        public List<TimeflowObject> GetTrackRelatedObjects(List<Keyframe> tracks, bool allowTimeOffset = false)
        {
            if (tracks == null || tracks.Count == 0) return null;

            List<TimeflowObject> relatedObjects = new List<TimeflowObject>();
            foreach (Keyframe key in tracks) {
                if (key.IsTrack && (allowTimeOffset || !key.Channel.Object.CanDragTimeOffset)) {
                    if (!relatedObjects.Contains(key.Channel.Object)) {
                        relatedObjects.Add(key.Channel.Object);
                    }
                }
            }
            // Find all the children related to the selected objects
            if (relatedObjects.Count > 0) {
                List<TimeflowObject> childObjects = new List<TimeflowObject>();
                foreach (TimeflowObject obj in relatedObjects) {
                    obj.GetObjectAndChildrenDisplayedRecursive(ref childObjects);
                }
                if (childObjects.Count > 0) {
                    foreach (TimeflowObject obj in childObjects) {
                        if (!relatedObjects.Contains(obj)) {
                            relatedObjects.Add(obj);
                        }
                    }
                }
            }
            return relatedObjects;
        }

        #endregion

        #region MARQUEE

        public Rect GetMarqueeRect(bool viewOffset)
        {
            Vector2 offset = Vector2.zero;
            if (viewOffset) {
                if (Input.EventMode == TimeflowViewInput.EventModes.DragKeyMarquee) {
                    offset = new Vector2(Layout.TimeAreaInner.Left, Layout.TimeAreaInner.Top);
                }
                else {
                    offset = new Vector2(Layout.Hierarchy.Left, Layout.Hierarchy.Top);
                }
            }
            float minX = Mathf.Min(MarqueeStart.x, MarqueeEnd.x) - offset.x;
            float maxX = Mathf.Max(MarqueeStart.x, MarqueeEnd.x) - offset.x;
            float minY = Mathf.Min(MarqueeStart.y, MarqueeEnd.y) - offset.y;
            float maxY = Mathf.Max(MarqueeStart.y, MarqueeEnd.y) - offset.y;
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public bool AnyMarqueSelectedObjects { get; private set; }

        public bool AnyMarqueeSelectedChannels { get; private set; }

        public void MarqueeSelectObjects(Rect rect, bool allowShiftSelect = true)
        {

            RectUtil.Correct(ref rect);
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsSelectable && !obj.IsLocked && !obj.GUICull) {
                    AnyMarqueSelectedObjects = true;
                    if (RectUtil.Overlaps(rect, obj.GUIRect)) {
                        if (allowShiftSelect && Event.current.shift) {
                            if (obj.WasSelected) {
                                obj.IsSelected = false;
                            }
                            else {
                                obj.IsSelected = true;
                            }
                        }
                        else {
                            obj.IsSelected = true;
                        }
                    }
                    else {
                        if (allowShiftSelect && Event.current.shift) {
                            obj.IsSelected = obj.WasSelected;
                        }
                        else {
                            obj.IsSelected = false;
                        }
                    }

                    if (!obj.IsLocked && !obj.IsCollapsed && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.IsHidden || ch == obj.Track || ch.IsLocked) {
                                ch.IsSelected = false;
                            }
                            else
                            if (RectUtil.Overlaps(rect, ch.GUIRect)) {
                                AnyMarqueeSelectedChannels = true;
                                if (allowShiftSelect && Event.current.shift) {
                                    if (obj.WasSelected) {
                                        ch.IsSelected = false;
                                    }
                                    else {
                                        ch.IsSelected = true;
                                    }
                                }
                                else {
                                    ch.IsSelected = true;
                                }
                            }
                            else {
                                if (allowShiftSelect && Event.current.shift) {
                                    ch.IsSelected = ch.WasSelected;
                                }
                                else {
                                    ch.IsSelected = false;
                                }
                            }
                            if (ch.IsSelected) {
                                obj.IsChannelSelected = true;
                            }
                        }
                    }

                }
                else {
                    obj.IsSelected = false;
                }
            }
        }

        public void MarqueeSelectKeys(Rect rect)
        {
            RectUtil.Correct(ref rect);

            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsSelectable && !obj.IsLocked && !obj.GUICull) {
                    if (!obj.IsLocked && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.IsEnabled && !ch.IsHidden && !ch.IsLocked && ch.Keys != null && !ch.GUICull) {
                                if (IsGraphMode && IsGraphLocked) {
                                    if (!ch.IsGraphLocked) continue;
                                }
                                if ((!obj.IsCollapsed || ch == obj.Track) && (!IsGraphMode || ((ch.IsSelected || ch.IsGraphLocked) && !ch.IsTrack))) {
                                    if (IsGraphMode && IsGraphLocked && IsGraphSolo) {
                                        if (!ch.IsSelected) continue;
                                    }
                                    bool selectionChanged = false;
                                    foreach (Keyframe k in ch.Keys) {
                                        float keyTime = k.KeyTimeWorld;
                                        if ((ch.IsTrack && !IsGraphMode) || (keyTime >= ScrollTimeMin && keyTime <= ScrollTimeMax)) {
                                            bool sel = RectUtil.Overlaps(rect, k.GUIRect);
                                            bool k0 = false;
                                            bool k1 = false;
                                            bool k2 = false;
                                            bool k3 = false;

                                            if (k.HasMultipleAttributes) {
                                                int ac = k.AttributeCount;
                                                k0 = ShowChannel0 && sel;
                                                k1 = ShowChannel1 && ac > 1 && RectUtil.Overlaps(rect, k.GUIRect1);
                                                k2 = ShowChannel2 && ac > 2 && RectUtil.Overlaps(rect, k.GUIRect2);
                                                k3 = ShowChannel3 && ac > 3 && RectUtil.Overlaps(rect, k.GUIRect3);
                                                if (!sel) sel = k0 || k1 || k2 || k3;
                                            }
                                            if (Event.current.shift) {
                                                if (sel) {
                                                    if (LastSelectedKeys != null && LastSelectedKeys.Contains(k)) {
                                                        bool canRemove = true;
                                                        if (k.HasMultipleAttributes) {
                                                            // Compare the selected channels and only remove it if all previously selected channels are in the set
                                                            canRemove = !k.LastAttributeSelected0 || (k.LastAttributeSelected0 && k0);
                                                            canRemove = canRemove && (!k.LastAttributeSelected1 || (k.LastAttributeSelected1 && k1));
                                                            canRemove = canRemove && (!k.LastAttributeSelected2 || (k.LastAttributeSelected2 && k2));
                                                            canRemove = canRemove && (!k.LastAttributeSelected3 || (k.LastAttributeSelected3 && k3));
                                                        }
                                                        if (canRemove) {
                                                            SelectedKeys.Remove(k);
                                                            selectionChanged = true;
                                                        }
                                                        else {
                                                            k.AttributeSelected0 = ShowChannel0 && k0 != k.LastAttributeSelected0;
                                                            k.AttributeSelected1 = ShowChannel1 && k1 != k.LastAttributeSelected1;
                                                            k.AttributeSelected2 = ShowChannel2 && k2 != k.LastAttributeSelected2;
                                                            k.AttributeSelected3 = ShowChannel3 && k3 != k.LastAttributeSelected3;
                                                        }
                                                    }
                                                    else {
                                                        k.AttributeSelected0 = k0;
                                                        k.AttributeSelected1 = k1;
                                                        k.AttributeSelected2 = k2;
                                                        k.AttributeSelected3 = k3;
                                                        SelectKey(k);
                                                        selectionChanged = true;
                                                    }
                                                }
                                            }
                                            else
                                            if (sel) {
                                                if (k.HasMultipleAttributes) {
                                                    k.AttributeSelected0 = k0;
                                                    k.AttributeSelected1 = k1;
                                                    k.AttributeSelected2 = k2;
                                                    k.AttributeSelected3 = k3;

                                                    Vector2 p = MarqueeStart;
                                                    float d = 0f;
                                                    if (k0) {
                                                        d = MathUtil.Distance(p, new Vector2(k.GUIRect.x, k.GUIRect.y));
                                                        Input.DragChannelIndex = 0;
                                                    }
                                                    if (k1) {
                                                        float d1 = MathUtil.Distance(p, new Vector2(k.GUIRect1.x, k.GUIRect1.y));
                                                        if (d1 < d || d == 0f) {
                                                            d = d1;
                                                            Input.DragChannelIndex = 1;
                                                        }
                                                    }
                                                    if (k2) {
                                                        float d2 = MathUtil.Distance(p, new Vector2(k.GUIRect2.x, k.GUIRect2.y));
                                                        if (d2 < d || d == 0f) {
                                                            d = d2;
                                                            Input.DragChannelIndex = 2;
                                                        }
                                                    }
                                                    if (k3) {
                                                        float d3 = MathUtil.Distance(p, new Vector2(k.GUIRect3.x, k.GUIRect3.y));
                                                        if (d3 < d || d == 0f) {
                                                            d = d3;
                                                            Input.DragChannelIndex = 3;
                                                        }
                                                    }
                                                }
                                                else {
                                                    Input.DragChannelIndex = 0;
                                                }
                                                SelectKey(k);
                                                selectionChanged = true;
                                            }
                                            else {
                                                if (LastSelectedKeys != null && !LastSelectedKeys.Contains(k)) {
                                                    SelectedKeys.Remove(k);
                                                    selectionChanged = true;
                                                }
                                            }
                                        }
                                    }

                                    if (selectionChanged) SelectedKeysChanged();
                                }
                            }
                        }
                    }

                    if (!IsGraphMode && obj.Events != null) {
                        if (SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                        foreach (TimeflowEvent evt in obj.Events) {
                            bool sel = RectUtil.Overlaps(rect, evt.GUIRect);
                            if (Event.current.shift) {
                                if (sel) {
                                    if (LastSelectedEvents != null && LastSelectedEvents.Contains(evt)) {
                                        SelectedEvents.Remove(evt);
                                        evt.IsSelected = false;
                                    }
                                    else {
                                        if (SelectedEvents != null && !SelectedEvents.Contains(evt)) {
                                        if(SelectedEvents == null) SelectedEvents = new List<TimeflowEvent>();
                                            SelectedEvents.Add(evt);
                                            evt.IsSelected = true;
                                        }
                                    }
                                }
                            }
                            else
                            if (sel) {
                                if (SelectedEvents != null && !SelectedEvents.Contains(evt)) {
                                    SelectedEvents.Add(evt);
                                    evt.IsSelected = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void MarqueeSelect()
        {
            if (Display.Objects != null && !Input.IsDragCanceled) {
                Rect rect = GetMarqueeRect(true);
                if (Input.EventMode == TimeflowViewInput.EventModes.DragObjectMarquee) {
                    MarqueeSelectObjects(rect);
                }
                else
                if (Input.EventMode == TimeflowViewInput.EventModes.DragKeyMarquee) {
                    MarqueeSelectKeys(rect);
                }
            }
        }

        public void GUIMarquee()
        {
            if (Input.IsDragCanceled) {
                return;
            }
            if (Input.EventMode == TimeflowViewInput.EventModes.DragKeyMarquee || Input.EventMode == TimeflowViewInput.EventModes.DragObjectMarquee) {
                if (MarqueeStart != MarqueeEnd) {
                    GUIRect rect = GetMarqueeRect(false);
                    GUI.Box(rect, "", AxonUI.MarqueeStyle);
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
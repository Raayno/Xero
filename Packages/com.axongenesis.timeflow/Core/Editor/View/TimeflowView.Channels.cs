// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region PUBLIC

        [NonSerialized]
        public bool ShowChannel0 = true;

        [NonSerialized]
        public bool ShowChannel1 = true;

        [NonSerialized]
        public bool ShowChannel2 = true;

        [NonSerialized]
        public bool ShowChannel3 = true;

        #endregion

        #region CHANNELS GUI

        public TimeflowChannel ChannelHit(Vector2 mousePosition)
        {
            TimeflowChannel hit = null;
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (!obj.IsLocked && obj.IsSelectable && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && ch != obj.Track) {
                                if (ch.HitTest(mousePosition) || ch.GUITrackRect.Contains(mousePosition)) {
                                    hit = ch;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return hit;
        }

        public TimeflowChannel ChannelTrackHit()
        {
            TimeflowChannel hit = null;
            if (Display.Objects != null) {
                Vector2 mousePosition = Input.GetMousePosition(Layout.TimeAreaInner);
                foreach (TimeflowObject obj in Display.Objects) {
                    if (!obj.IsLocked && obj.IsSelectable && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden) {
                                if (ch.GUITrackRect.Contains(mousePosition)) {
                                    hit = ch;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return hit;
        }

        public bool ChannelExpandHit()
        {
            bool hit = false;
            Input.DragChannel = null;
            if (Display.Objects != null) {
                Vector2 p = Input.GetMousePosition(Layout.Hierarchy);
                if (p.x < Timeflow.Layout.Switches.Width) return false;
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.AllChannels != null && obj.IsSelectable) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && ch.GUIExpandRect.Contains(p)) {
                                Input.DragChannel = ch;
                                hit = true;
                                break;
                            }
                        }
                    }
                }
            }
            return hit;
        }

        #endregion

        #region CHANNEL OPS

        public bool IsDragChannel(TimeflowChannel channel)
        {
            return Input.IsDragging && Input.EventMode == TimeflowViewInput.EventModes.DragChannelOrder && channel == Input.DragChannel;
        }

        public bool ChannelCustomHit()
        {
            Input.DragChannel = null;
            bool hit = false;
            if (Display.Objects != null) {
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);
                foreach (TimeflowObject obj in Display.Objects) {
                    if (!obj.IsLocked && obj.IsSelectable && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && ch != obj.Track && ch.GUICustomHit(p)) {
                                //Debug.Log($"ChannelCustomHit: {ch.Name} hidden:{ch.IsHidden} track:{ch == obj.Track} hit:{ch.GUICustomHit(p)}");
                                Input.DragChannel = ch;
                                hit = true;
                                break;
                            }
                        }
                    }
                    if (hit) break;
                }
            }
            return hit;
        }
        public void ChannelCustomHitEnded()
        {
            if (Input.DragChannel != null) {
                Input.DragChannel.GUICustomHitEnded();
            }
        }

        public bool ChannelLoopHandlesHit()
        {
            Input.DragChannel = null;
            bool hit = false;
            if (IsGraphMode && SelectedChannels != null) {
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden && ch.GUILoopHandlesHit(p)) {
                        Input.DragChannel = ch;
                        hit = true;
                        break;
                    }
                }
            }
            return hit;
        }

        public void DeleteSelectedChannels()
        {
            if (SelectedChannels != null) {
                //Debug.Log($"DeleteSelectedChannels:{SelectedChannels.Count}");
                // Copy the list to avoid errors about modifying the SelectedChannels collection
                List<TimeflowChannel> list = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch != null && !ch.IsHidden && ch.Behavior != null) {
                        list.Add(ch);
                    }
                }
                //Debug.Log($"DeleteSelectedChannels:{list.Count}");
                if (list.Count > 0) {
                    foreach (TimeflowChannel ch in list) {
                        if (ch == null || ch.Behavior == null) continue;
                        //Debug.Log($"deleting: {ch.Name}");
                        UndoUtil.Undo(ch.Behavior, "Delete Channel", true);
                        ch.Behavior.RemoveChannelWithUndo(ch);
                    }
                }
            }
            Display.ApplyFilter();
        }

        public void LockChannel(TimeflowChannel ch, bool islocked)
        {
            UndoUtil.Undo(ch.Behavior, "Toggle Lock");
            ch.IsLocked = islocked;
            if (ch.IsLocked) {
                if (SelectedChannels != null && SelectedChannels.Contains(ch)) {
                    SelectedChannels.Remove(ch);
                }
            }
            Display.ApplyFilter();
        }

        public void LockChannelToggle(TimeflowChannel ch)
        {
            UndoUtil.Undo(ch.Behavior, "Toggle Lock");
            ch.IsLocked = !ch.IsLocked;
            if (ch.IsLocked) {
                if (SelectedChannels != null && SelectedChannels.Contains(ch)) {
                    SelectedChannels.Remove(ch);
                }
            }
            Display.ApplyFilter();
        }

        public void LockSelectedChannels(bool locked)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                List<TimeflowChannel> listCopy = new List<TimeflowChannel>(SelectedChannels);
                foreach (TimeflowChannel ch in listCopy) {
                    if (ch.IsSelected) {
                        UndoUtil.Undo(ch.Behavior, "Set Locked");
                        ch.IsLocked = locked;
                    }
                }

            }
            Display.ApplyFilter();
        }

        public void CanDrawChannel(TimeflowChannel ch, bool isOn, bool locked)
        {
            UndoUtil.Undo(ch.Behavior, "Toggle Draw Graph");
            ch.GUICanDraw = isOn;
            ch.IsGraphLocked = locked;
        }

        public void CanDrawSelectedChannels(bool isOn, bool locked)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                List<TimeflowChannel> listCopy = new List<TimeflowChannel>(SelectedChannels);
                foreach (TimeflowChannel ch in listCopy) {
                    if (ch.IsSelected) {
                        UndoUtil.Undo(ch.Behavior, "Set Locked");
                        ch.GUICanDraw = isOn;
                        ch.IsGraphLocked = locked;
                    }
                }
            }
        }

        public void EnableSelectedChannels(bool enabled)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                // Make a copy of the list to avoid modifying the list in place
                List<TimeflowChannel> deselect = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch.IsSelected) {
                        deselect.Add(ch);
                    }
                }
                if (deselect.Count > 0) {
                    foreach (TimeflowChannel ch in deselect) {
                        UndoUtil.Undo(ch.Behavior, "Set Channel Enabled");
                        ch.IsEnabled = enabled;
                    }
                }
            }
            Display.ApplyFilter();
        }

        public void DuplicateSelectedChannels()
        {
            List<TimeflowChannel> newSelectedChannels = new List<TimeflowChannel>();

            foreach (TimeflowChannel ch in SelectedChannels.ToArray()) {
                TimeflowChannel copy = ch.Behavior.DuplicateChannel(ch);
                newSelectedChannels.Add(copy);
            }

            KeyframesTouched = true;
            ObjectTouched = true;
            SelectedChannels = newSelectedChannels;

            SelectedKeysChanged();
            Display.ApplyFilter();
        }

        #endregion

        #region INTERPOLATION

        public List<TimeflowChannel> TargetChannels {
            get {
                if (IsGraphMode && IsGraphLocked) {
                    return GraphLockedChannels;
                }
                else {
                    return SelectedChannels;
                }
            }
        }

        public TimeflowChannel.Interpolations GetChannelInterpolationOfTargetChannels()
        {
            TimeflowChannel.Interpolations interp = TimeflowChannel.Interpolations.None;
            if (TargetChannels == null || TargetChannels.Count == 0) return interp;

            bool isFirst = true;
            foreach (TimeflowChannel ch in TargetChannels) {
                if (!isFirst && !ch.IsHidden) {
                    if (interp != ch.Interpolation) {
                        interp = TimeflowChannel.Interpolations.None;
                        break;
                    }
                }
                interp = ch.Interpolation;
                isFirst = false;
            }
            return interp;
        }

        public void SetInterpolationForTargetChannels(TimeflowChannel.Interpolations interp)
        {
            if (TargetChannels == null || TargetChannels.Count == 0) return;
            foreach (TimeflowChannel ch in TargetChannels) {
                if (!ch.IsHidden) {
                    UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Interpolation");
                    ch.Interpolation = interp;
                    KeyframesTouched = true;
                    ObjectTouched = true;
                }
            }
        }

        #endregion

        #region LOOP

        public int GetChannelLoopMode()
        {
            int mode = -1; //off
            int m = -1;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        if (ch.EnableLoop) {
                            if (ch.LoopPingPong) {
                                m = 2;
                            }
                            else {
                                m = 1;
                            }
                        }
                        else {
                            m = 0;
                        }
                        if (isFirst) {
                            mode = m;
                            isFirst = false;
                        }
                        else
                        if (m != mode) {
                            mode = -1;
                            break;
                        }
                    }
                }
            }
            return mode;
        }

        public float GetChannelLoopLimit()
        {
            float limit = 0; //off
            float a = 0;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        a = ch.LoopLimit;
                        if (isFirst) {
                            limit = a;
                            isFirst = false;
                        }
                        else
                        if (a != limit) {
                            limit = 0;
                            break;
                        }
                    }
                }
            }
            return limit;
        }

        public bool GetChannelLoopAuto()
        {
            bool allow = false; //off
            bool a = false;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        a = ch.EnableLoop && ch.EnableAutoLoop;
                        if (isFirst) {
                            allow = a;
                            isFirst = false;
                        }
                        else
                        if (a != allow) {
                            allow = false;
                            break;
                        }
                    }
                }
            }
            return allow;
        }

        public bool GetChannelLoopIn()
        {
            bool allow = false; //off
            bool a = false;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        a = ch.EnableLoop && ch.EnableLoopIn;
                        if (isFirst) {
                            allow = a;
                            isFirst = false;
                        }
                        else
                        if (a != allow) {
                            allow = false;
                            break;
                        }
                    }
                }
            }
            return allow;
        }

        public bool GetChannelLoopOut()
        {
            bool allow = false; //off
            bool a = false;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        a = ch.EnableLoop && ch.EnableLoopOut;
                        if (isFirst) {
                            allow = a;
                            isFirst = false;
                        }
                        else
                        if (a != allow) {
                            allow = false;
                            break;
                        }
                    }
                }
            }
            return allow;
        }

        public int GetChannelLoopMatchMode()
        {
            int mode = -1; // mixed
            int m = -1;
            bool isFirst = true;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        if (ch.EnableLoop) {
                            if (ch.LoopMatchEnds) {
                                m = 1;
                            }
                            else {
                                m = 0;
                            }
                        }
                        else {
                            m = 0;
                        }
                        if (isFirst) {
                            mode = m;
                            isFirst = false;
                        }
                        else
                        if (m != mode) {
                            mode = -1;
                            break;
                        }
                    }
                }
            }
            return mode;
        }

        public void LoopSelectedChannels(bool selectedKeysOnly)
        {
            bool selectedKeys = selectedKeysOnly && SelectedKeys != null && SelectedKeys.Count > 1;

            List<TimeflowChannel> channels = SelectedChannels;
            if (selectedKeys) {
                /// rebuild list of channels from selected keys only
                channels = new List<TimeflowChannel>();
                foreach (Keyframe k in SelectedKeys) {
                    if (!channels.Contains(k.Channel) && k.Channel != null) {
                        channels.Add(k.Channel);
                    }
                }
            }

            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    if (!ch.IsTrack && ch.Keys != null && ch.Keys.Count > 0) {
                        float start = Timeflow.StartTime;
                        float end = EndTimePadded;
                        bool startSet = false;
                        bool endSet = false;

                        foreach (Keyframe k in ch.Keys) {
                            if (!selectedKeys || SelectedKeys.Contains(k)) {
                                if (!startSet) {
                                    startSet = true;
                                    start = k.KeyTime;
                                }
                                else
                                if (start > k.KeyTime) {
                                    start = k.KeyTime;
                                }
                                if (!endSet) {
                                    endSet = true;
                                    end = k.KeyTime;
                                }
                                else
                                if (end < k.KeyTime) {
                                    end = k.KeyTime;
                                }
                            }
                        }
                        if (start > end) {
                            float t = start;
                            start = end;
                            end = t;
                        }
                        ch.EnableLoop = true;
                        ch.EnableAutoLoop = !selectedKeysOnly;
                        ch.LoopStart = start;
                        ch.LoopEnd = end;
                        EditorUtility.SetDirty(ch.Behavior);
                    }
                }

                ObjectTouched = true;
            }
        }

        public void UnloopSelectedChannels()
        {
            if (SelectedChannels != null) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    ch.EnableLoop = false;
                }
                ObjectTouched = true;
            }
        }

        public void SetLoopLimitForSelectedChannels(float limit)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Interpolation");
                        ch.LoopLimit = limit;
                        ch.UpdateAutoLoop();
                        EditorUtility.SetDirty(ch.Behavior);
                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void SetAutoLoopForSelectedChannels(bool enabled)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Auto Loop");
                        ch.EnableAutoLoop = enabled;
                        if (enabled) {
                            ch.UpdateAutoLoop();
                        }
                        EditorUtility.SetDirty(ch.Behavior);
                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void SetLoopInForSelectedChannels(bool enabled)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Interpolation");
                        ch.EnableLoopIn = enabled;

                        EditorUtility.SetDirty(ch.Behavior);
                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void SetLoopOutForSelectedChannels(bool enabled)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Interpolation");
                        ch.EnableLoopOut = enabled;
                        EditorUtility.SetDirty(ch.Behavior);

                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void SetLoopModeForSelectedChannels(int mode)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Interpolation");
                        if (mode == 0) {
                            ch.EnableLoop = false;
                            ch.LoopPingPong = false;
                        }
                        else
                        if (mode == 1) {
                            ch.EnableLoop = true;
                            ch.LoopPingPong = false;
                        }
                        else
                        if (mode == 2) {
                            ch.EnableLoop = true;
                            ch.LoopPingPong = true;
                        }
                        if (ch.EnableLoop && ch.EnableAutoLoop) {
                            ch.UpdateAutoLoop();
                        }

                        EditorUtility.SetDirty(ch.Behavior);

                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        public void SetLoopMatchModeForSelectedChannels(int mode)
        {
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Set Channel Loop Match", true);
                        if (mode == 0) {
                            ch.LoopMatchEnds = false;
                        }
                        else
                        if (mode == 1) {
                            ch.LoopMatchEnds = true;
                        }
                        ch.PrepareLoop();

                        EditorUtility.SetDirty(ch.Behavior);

                        KeyframesTouched = true;
                        ObjectTouched = true;
                    }
                }
            }
        }

        #endregion

        #region GUI

        [NonSerialized]
        private GUIRect _channelValuesRect;

        [NonSerialized]
        private GUIRect _channelTimeOffsetRect;

        private void GUIChannelValues()
        {
            if (Timeflow.RootObjectsCached == null) return;
            if (IsLayout) {
                _channelValuesRect = new GUIRect(Layout.Values.Rect);
                _channelValuesRect.y += Layout.HierarchyTools.Height;
                _channelValuesRect.height = Layout.Hierarchy.Height;
            }
            GUIBeginGroup(_channelValuesRect);
            EditorGUI.BeginChangeCheck();
            foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                if (obj == null) continue;
                GUIChannelValuesRecursive(obj);
            }
            if (EditorGUI.EndChangeCheck()) {
                Timeflow.DoUpdate();
                AlignTools.Refresh();
            }
            GUIEndGroup();
        }

        private void GUIChannelValuesRecursive(TimeflowObject obj)
        {
            obj.GUIChannelValues();

            if (!obj.IsCollapsed) {
                if (obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (ch != obj.Track && !ch.IsHidden) {
                            ch.GUIChannelValues();
                        }
                    }
                }
                if (obj.ShowChildren && obj.Children != null) {
                    foreach (TimeflowObject t in obj.Children) {
                        GUIChannelValuesRecursive(t);
                    }
                }
            }
        }

        private void GUITimeOffsetColumn()
        {
            if (Timeflow.RootObjectsCached != null) {
                if (IsLayout) {
                    _channelTimeOffsetRect = new GUIRect(Layout.TimeOffset.Rect);
                    _channelTimeOffsetRect.y += Layout.HierarchyTools.Height;
                    _channelTimeOffsetRect.height -= Layout.HierarchyTools.Height;
                }
                GUIBeginGroup(_channelTimeOffsetRect);
                EditorGUI.BeginChangeCheck();
                foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                    if (obj == null) continue;
                    GUITimeOffsetColumnRecursive(obj);
                }
                if (EditorGUI.EndChangeCheck()) {
                    AlignTools.Refresh();
                }
                GUIEndGroup();
            }
        }

        private void GUITimeOffsetColumnRecursive(TimeflowObject obj)
        {
            obj.GUITimeOffsetColumn();

            if (!obj.IsCollapsed) {
                if (obj.HasBehaviors) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (ch != obj.Track && !ch.IsHidden) {
                            ch.GUITimeOffsetColumn();
                        }
                    }
                }
                if (obj.ShowChildren && obj.Children != null) {
                    foreach (TimeflowObject t in obj.Children) {
                        GUITimeOffsetColumnRecursive(t);
                    }
                }
            }
        }

        #endregion    
    }

}//AxonGenesis
#endif


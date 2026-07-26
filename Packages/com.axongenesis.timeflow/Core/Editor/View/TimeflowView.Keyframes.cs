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
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region STATIC

        public static bool UseRelatedKeys {
            get {
                return Event.current != null && Event.current.shift;
            }
        }

        #endregion

        #region PUBLIC

        [NonSerialized]
        public List<Keyframe> SelectedKeysUndo;

        #endregion

        #region GUI

        public bool TrackInOutHit()
        {
            bool isHit = false;
            Keyframe trackHit = null;
            Keyframe adjacentHit = null;
            bool isAdjacentHit = false;
            Input.IsTrackInAndOutPoint = false;

            if (SelectionMode != SelectionModes.KeyframesOnly && !IsGraphMode && Layout.TimeAreaInner.HitTest(Event.current.mousePosition)) {
                if (SelectionMode == SelectionModes.KeyframesOnly) {
                    return false;
                }
                Vector2 p = Input.GetMousePosition(Layout.TimeAreaInner);

                if (SelectedKeys != null) {
                    // First check to see if a selected track in/out has been hit
                    foreach (Keyframe t in SelectedKeys) {
                        if (t.IsTrack && !t.LockTime) {
                            if (!isHit) {
                                if (t.HandleRectLeft.Contains(p)) {
                                    isHit = true;
                                    trackHit = t;
                                    Input.IsTrackInPoint = true;
                                }
                                else
                                if (t.HandleRectRight.Contains(p)) {
                                    isHit = true;
                                    trackHit = t;
                                    Input.IsTrackInPoint = false;
                                }
                            }
                            else {
                                // If two side-by-side tracks are selected, adjust the end point simultaneously to keep the junction 
                                if (t.HandleRectLeft.Contains(p)) {
                                    isAdjacentHit = true;
                                    adjacentHit = t;
                                    Input.IsTrackInAndOutPoint = true;
                                }
                                else
                                if (t.HandleRectRight.Contains(p)) {
                                    isAdjacentHit = true;
                                    adjacentHit = t;
                                    Input.IsTrackInAndOutPoint = true;
                                }
                                if (isHit && isAdjacentHit) break;
                            }
                        }
                    }
                }
                if (!isHit && Display.Objects != null) {
                    // Check for unselected tracks next
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (!obj.IsLocked && obj.IsSelectable && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden && ch.IsTrack) {
                                    foreach (Keyframe t in ch.Keys) {
                                        if (t.HandleRectLeft.Contains(p)) {
                                            isHit = true;
                                            trackHit = t;
                                            Input.IsTrackInPoint = true;
                                            break;
                                        }
                                        else
                                        if (t.HandleRectRight.Contains(p)) {
                                            isHit = true;
                                            trackHit = t;
                                            Input.IsTrackInPoint = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (trackHit != null) {
                Input.DragPrimaryKey = trackHit;
                Input.DragSecondaryKey = adjacentHit;
                if (SelectedKeys == null || !SelectedKeys.Contains(trackHit)) {
                    SetupSelectedKeys(true);
                    SelectKey(trackHit);
                    SelectedKeysChanged();
                }
            }

            return isHit;
        }

        #endregion

        #region KEY OPERATIONS

        public Rect GetSelectedKeysBoundingBox()
        {
            Rect rect = new GUIRect(0, 0, 0, 0);
            List<TimeflowChannel> channels = new List<TimeflowChannel>();
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                Keyframe minT = null;
                Keyframe maxT = null;
                Keyframe minV = null;
                Keyframe maxV = null;
                Keyframe minY = null;
                Keyframe maxY = null;

                float rightEdge = 0;
                foreach (Keyframe k in SelectedKeys) {
                    if (!IsGraphMode && k.Channel != null && !channels.Contains(k.Channel)) channels.Add(k.Channel);

                    if (minY == null) {
                        minY = k;
                    }
                    else
                    if (minY.GUIRect.y > k.GUIRect.y) {
                        minY = k;
                    }

                    if (maxY == null) {
                        maxY = k;
                    }
                    else
                    if (maxY.GUIRect.y < k.GUIRect.y) {
                        maxY = k;
                    }

                    if (minT == null) {
                        minT = k;
                    }
                    else
                    if (minT.KeyTimeWorld > k.KeyTimeWorld) {
                        minT = k;
                    }

                    if (maxT == null) {
                        maxT = k;
                        rightEdge = k.KeyTimeWorld;
                        if (k.IsTrack) rightEdge = k.KeyEndTimeWorld;
                    }
                    else
                    if (k.IsTrack) {
                        float e = k.KeyEndTimeWorld;
                        if (rightEdge < e) {
                            maxT = k;
                            rightEdge = e;
                        }
                    }
                    else {
                        if (maxT.KeyTimeWorld < k.KeyTimeWorld) {
                            maxT = k;
                        }
                    }

                    if (minV == null) {
                        minV = k;
                    }
                    else
                    if (IsGraphMode) {
                        if (minV.MinValueSelected > k.MinValueSelected) {
                            minV = k;
                        }
                    }
                    else {
                        if (minV.GUIRect.y > k.GUIRect.y) {
                            minV = k;
                        }
                    }

                    if (maxV == null) {
                        maxV = k;
                    }
                    else
                    if (IsGraphMode) {
                        if (maxV.MaxValueSelected < k.MaxValueSelected) {
                            maxV = k;
                        }
                    }
                    else {
                        if (maxV.GUIRect.y < k.GUIRect.y) {
                            maxV = k;
                        }
                    }
                }
                float x = PositionOfTime(minT.KeyTimeWorld, true);
                float r = PositionOfTime(maxT.IsTrack ? maxT.KeyEndTimeWorld : maxT.KeyTimeWorld, true);
                float w = r - x;
                float h = 0;
                float y = 0;

                if (!IsGraphMode) {
                    y = minY.GUIRect.y;
                    h = maxY.GUIRect.y + maxY.GUIRect.height;
                    h -= y;
                }
                else {
                    h = IsGraphMode ? PositionOfValue(maxV.MaxValueSelected, true) : maxV.GUIRect.y;
                    h = h - y + 16f;
                    y = IsGraphMode ? PositionOfValue(minV.MinValueSelected, true) : minV.GUIRect.y;
                }

                rect = new GUIRect(x, y, w, h);
            }
            return rect;
        }

        public Vector2 GetSelectedKeysTimeRange()
        {
            if (SelectedKeys == null || SelectedKeys.Count == 0) return Vector2.zero;

            float minTime = 0f;
            float maxTime = 0f;
            bool isFirst = true;
            foreach (Keyframe key in SelectedKeys) {
                if (isFirst) {
                    isFirst = false;
                    minTime = key.KeyTime;
                    if (key.IsTrack) {
                        maxTime = key.KeyValue;
                    }
                    else {
                        maxTime = key.KeyTime;
                    }
                }
                else {
                    if (minTime > key.KeyTime) {
                        minTime = key.KeyTime;
                    }
                    if (key.IsTrack) {
                        if (maxTime < key.KeyValue) {
                            maxTime = key.KeyValue;
                        }
                    }
                    else {
                        if (maxTime < key.KeyTime) {
                            maxTime = key.KeyTime;
                        }
                    }
                }
            }
            return new Vector2(minTime, maxTime);
        }

        public void ResetupKeyframes()
        {
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.AllChannels != null && obj.IsDisplayed) {
                    foreach (TimeflowChannel ch in obj.AllChannels) {
                        ch.SetupKeyframes();
                    }
                }
            }
        }

        public void CopyKeyframes()
        {
            if (Timeflow.View.Markers.SelectedMarker == null || Timeflow.View.Markers.SelectedMarker.Locked || !Timeflow.View.Markers.SelectedMarker.IsSelected) {
                CopiedMarker = null;
            }
            else {
                CopiedMarker = new TimeflowMarker(Timeflow.View.Markers.SelectedMarker);
            }
            CopiedKeys = new List<List<Keyframe>>();
            CopiedTracks = new List<List<Keyframe>>();
            CopiedEvents = new List<List<TimeflowEvent>>();

            if (SelectedChannels == null) SelectedChannels = new List<TimeflowChannel>();
            if (SelectedTrackChannels == null) SelectedTrackChannels = new List<TimeflowChannel>();

            int copiedKeysCount = 0;

            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                // Build a list of channels for all of the selected keyframes
                List<TimeflowChannel> tracks = new List<TimeflowChannel>();
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                foreach (Keyframe k in SelectedKeys) {
                    if (!TimeflowPreferences.Current.CopyLockedTracksAndKeys && k.LockTime) continue;
                    if (k.Channel != null && !k.Channel.IsHidden) {
                        if (k.Channel.IsTrack) {
                            if (!tracks.Contains(k.Channel)) {
                                tracks.Add(k.Channel);
                                k.Channel.IsSelected = true;
                                if (!SelectedTrackChannels.Contains(k.Channel)) {
                                    SelectedTrackChannels.Add(k.Channel);
                                }
                            }
                        }
                        else
                        if (!channels.Contains(k.Channel)) {
                            channels.Add(k.Channel);
                            k.Channel.IsSelected = true;
                            if (!k.Channel.IsGraphLockedOverride && !SelectedChannels.Contains(k.Channel)) {
                                SelectedChannels.Add(k.Channel);
                            }
                        }
                    }
                }

                if (channels.Count > 0) {
                    // Copy the keys in lists per each channel
                    foreach (TimeflowChannel channel in channels) {
                        List<Keyframe> copy = new List<Keyframe>();
                        foreach (Keyframe k in channel.Keys) {
                            if (!TimeflowPreferences.Current.CopyLockedTracksAndKeys && k.LockTime) continue;
                            if (SelectedKeys.Contains(k)) {
                                Keyframe c = Keyframe.Clone(k, null);
                                c.CopiedFromChannel = k.Channel;
                                c.KeyTime = k.KeyTimeWorld;// make world time
                                copy.Add(c);
                                copiedKeysCount++;
                            }
                        }
                        if (copy.Count > 0) CopiedKeys.Add(copy);
                    }
                }
                if (tracks.Count > 0) {
                    // Copy the keys in lists per each channel
                    foreach (TimeflowChannel track in tracks) {
                        if (track.IsLocked) continue;
                        List<Keyframe> copy = new List<Keyframe>();
                        foreach (Keyframe k in track.Keys) {
                            if (!TimeflowPreferences.Current.CopyLockedTracksAndKeys && k.LockTime) continue;
                            if (SelectedKeys.Contains(k)) {
                                Keyframe c = Keyframe.Clone(k, null);
                                c.CopiedFromChannel = k.Channel;
                                c.KeyTime = k.KeyTimeWorld;// make world time
                                c.KeyValue = k.KeyEndTimeWorld;
                                copy.Add(c);
                            }
                        }
                        if (copy.Count > 0) CopiedTracks.Add(copy);
                    }
                }
            }

            if (SelectedEvents != null && SelectedEvents.Count > 0) {
                List<TimeflowEvent> copy = new List<TimeflowEvent>();
                foreach (TimeflowEvent evt in SelectedEvents) {
                    if (!TimeflowPreferences.Current.CopyLockedTracksAndKeys && evt.LockTime) continue;
                    copy.Add(evt);
                }
                if (copy.Count > 0) CopiedEvents.Add(copy);
            }
        }

        public void CutKeyframes()
        {
            CopyKeyframes();
            DeleteSelectedKeys();
        }

        public void PasteKeysPreserveTime() => PasteKeys(false);

        public void PasteKeysAtCurrentTime() => PasteKeys(true);

        public void PasteKeys(bool atCurrentTime)
        {
            float offset = 0f;

            float earliest = float.MaxValue;
            if (CopiedMarker != null) {
                TimeflowMarker marker = Timeflow.Markers.AddMarker(Timeflow.CurrentTime);
                if (marker != null) {
                    marker.Copy(CopiedMarker);
                    if (atCurrentTime) {
                        earliest = marker.Time; // original time, not the new time
                        marker.Time = Timeflow.CurrentTime;
                    }
                }
            }

            if (atCurrentTime) {
                if (CopiedKeys != null && CopiedKeys.Count > 0) {
                    foreach (List<Keyframe> layer in CopiedKeys) {
                        foreach (Keyframe k in layer) {
                            float keyTime = k.KeyTimeWorld;
                            if (keyTime < earliest) {
                                earliest = keyTime;
                            }
                        }
                    }
                }
                if (CopiedTracks != null && CopiedTracks.Count > 0) {
                    foreach (List<Keyframe> layer in CopiedTracks) {
                        foreach (Keyframe k in layer) {
                            float keyTime = k.KeyTimeWorld;
                            if (keyTime < earliest) {
                                earliest = keyTime;
                            }
                        }
                    }
                }
                if (CopiedEvents != null && CopiedEvents.Count > 0) {
                    foreach (List<TimeflowEvent> events in CopiedEvents) {
                        foreach (TimeflowEvent evt in events) {
                            if (evt.TriggerTimeWorld < earliest) {
                                earliest = evt.TriggerTimeWorld;
                            }
                        }
                    }
                }
                offset = Timeflow.CurrentTime - earliest;
            }
            if (CopiedKeys != null && CopiedKeys.Count > 0 && (SelectedChannels == null || SelectedChannels.Count == 0)) {
                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    SelectedChannels = new List<TimeflowChannel>();
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.Channel != null && !k.Channel.IsTrack) {
                            if (!k.Channel.IsGraphLockedOverride && !SelectedChannels.Contains(k.Channel)) {
                                SelectedChannels.Add(k.Channel);
                            }
                        }
                    }
                }
            }
            if (CopiedTracks != null && CopiedTracks.Count > 0) {
                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    SelectedTrackChannels = new List<TimeflowChannel>();
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.Channel != null && k.Channel.IsTrack) {
                            if (!SelectedTrackChannels.Contains(k.Channel)) {
                                SelectedTrackChannels.Add(k.Channel);
                            }
                        }
                    }
                }
            }

            SetupSelectedKeys(true);

            if (CopiedKeys != null && CopiedKeys.Count > 0 && SelectedChannels != null && SelectedChannels.Count > 0) {
                int i = 0;
                int totalCount = 0;
                int pastedCount = 0;
                foreach (List<Keyframe> layer in CopiedKeys) {
                    if (i < SelectedChannels.Count) {
                        TimeflowChannel ch = SelectedChannels[i];
                        if (ch.SupportsKeyframes) {
                            if (layer[0].CopiedFromChannel != null && layer[0].CopiedFromChannel != ch && layer[0].CopiedFromChannel.IsSelected) {
                                ch = layer[0].CopiedFromChannel; // Keep keys on same channel if selected
                            }
                            UndoUtil.Undo(ch.Behavior, "Paste Keys");
                            foreach (Keyframe k in layer) {
                                Keyframe nk = ch.CopyKey(k, offset - ch.TimeOffsetWorld, true, false);
                                SelectKey(nk);
                                pastedCount++;
                            }
                        }
                        i++;
                    }
                    totalCount += layer.Count;
                }

                if (pastedCount == 0) {
                    EditorUtility.DisplayDialog("No Keyframes Pasted", "No kefyrames were pasted. Please check that the selected channels support keyframes.", "Ok");
                }
                else
                if (pastedCount != totalCount) {
                    string warning = "The total number of keys pasted (" + pastedCount + ") does not match those copied (" + totalCount + "). Please check that the number of selected channels match those that were copied.";
                    EditorUtility.DisplayDialog("Keyframes Mismatched", warning, "Ok");
                    Debug.LogWarning(warning);
                }
                SelectedKeysChanged();
            }
            if (CopiedTracks != null && CopiedTracks.Count > 0 && SelectedTrackChannels != null && SelectedTrackChannels.Count > 0) {
                int i = 0;
                int totalCount = 0;
                int pastedCount = 0;
                foreach (List<Keyframe> layer in CopiedTracks) {
                    if (i < SelectedTrackChannels.Count) {
                        TimeflowChannel ch = SelectedTrackChannels[i];
                        if (layer[0].CopiedFromChannel != null && layer[0].CopiedFromChannel != ch && layer[0].CopiedFromChannel.IsSelected) {
                            ch = layer[0].CopiedFromChannel; // Keep keys on same channel if selected
                        }
                        UndoUtil.Undo(ch.Behavior, "Paste Tracks");
                        foreach (Keyframe k in layer) {
                            Keyframe nk = ch.CopyKey(k, offset - ch.TimeOffsetWorld, true, true);
                            SelectKey(nk);
                            pastedCount++;
                        }
                        i++;
                    }
                    totalCount += layer.Count;
                }

                if (pastedCount == 0) {
                    EditorUtility.DisplayDialog("No Keyframes Pasted", "No kefyrames were pasted. Please check that the selected channels support keyframes.", "Ok");
                }
                else
                if (pastedCount != totalCount) {
                    string warning = "The total number of keys pasted (" + pastedCount + ") does not match those copied (" + totalCount + "). Please check that the number of selected channels match those that were copied.";
                    EditorUtility.DisplayDialog("Keyframes Mismatched", warning, "Ok");
                    Debug.LogWarning(warning);
                }
                SelectedKeysChanged();
            }
            if (CopiedEvents != null && CopiedEvents.Count > 0) {
                SelectedEvents = new List<TimeflowEvent>();
                foreach (List<TimeflowEvent> events in CopiedEvents) {
                    foreach (TimeflowEvent evt in events) {
                        GameObject obj = null;
                        if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                            obj = Selection.gameObjects[0];
                        }
                        if (obj == null) obj = evt.gameObject;
                        if (obj == null) {
                            Debug.LogWarning("Null object");
                        }
                        else {
                            TimeflowEvent evtCopy = obj.AddComponent(evt.GetType()) as TimeflowEvent;
                            evtCopy.Copy(evt);
                            evtCopy.TriggerTime += offset;
                            UndoUtil.UndoCreate(evtCopy, "Paste Keys");
                            SelectedEvents.Add(evtCopy);
                            evtCopy.IsSelected = true;
                        }
                    }
                }
                SelectedKeysChanged();
            }
            KeyframesTouched = true;
            ObjectTouched = true;
            Timeflow.Active.Refresh(true);
        }

        public void PasteKeyTangents()
        {
            if (CopiedKeys != null && CopiedKeys.Count > 0 && SelectedKeys != null && SelectedKeys.Count > 0) {
                List<Keyframe> fromKeys = new List<Keyframe>();

                foreach (List<Keyframe> layer in CopiedKeys) {
                    foreach (Keyframe fromKey in layer) {
                        fromKeys.Add(fromKey);
                    }
                }

                /// make sure keys are in order by ascending time
                fromKeys.Sort(KeyframeSort.ByTimeAsc);
                SelectedKeys.Sort(KeyframeSort.ByTimeAsc);

                int i = 0;
                int pastedCount = 0;
                for (int k = 0; k < SelectedKeys.Count; k++) {
                    Keyframe key = SelectedKeys[k];
                    UndoUtil.Undo(key.Behavior, "Paste Key Tangents");
                    key.CopyTangents(fromKeys[i]);
                    pastedCount++;
                    i++;
                    if (i >= fromKeys.Count) {
                        i = 0;
                    }
                }

                if (pastedCount == 0) {
                    EditorUtility.DisplayDialog("No Keyframe Tangents Pasted", "No kefyrame tangents were pasted. Please select the kefyrames you wish to paste tangents to.", "Ok");
                }
            }

            KeyframesTouched = true;
        }

        public void ShowKeyTangents(bool show)
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe key in SelectedKeys) {
                    key.ShowTangents = show;
                }
            }

            KeyframesTouched = true;
        }

        public void ClearDuplicateKeys(bool undoable = false)
        {
            if (Display.Objects != null) {
                List<Keyframe> affectedKeys = null;
                if (SelectedKeys != null && SelectedKeys.Count > 0) {
                    affectedKeys = new List<Keyframe>(SelectedKeys);
                }
                else {
                    affectedKeys = new List<Keyframe>();
                }
                if (RelatedKeys != null && RelatedKeys.Count > 0) {
                    affectedKeys.AddRange(RelatedKeys);
                }
                if (RelatedTracks != null && RelatedTracks.Count > 0) {
                    affectedKeys.AddRange(RelatedTracks);
                }

                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.BehaviorsEnabled && !obj.IsLocked && obj.IsDisplayed && obj.AllChannels != null && obj.AllChannels.Count > 0) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (undoable && ch.Behavior != null) {
                                UndoUtil.Undo(ch.Behavior, "Clear Duplicate Keys", true);
                            }
                            List<Keyframe> dups = ch.ClearDuplicateKeys(affectedKeys);
                            if (dups != null && dups.Count > 0 && SelectedKeys != null && SelectedKeys.Count > 0) {
                                // Remove the defunct keys from the selection
                                foreach (Keyframe d in dups) {
                                    if (SelectedKeys.Contains(d)) {
                                        SelectedKeys.Remove(d);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DeleteKeys(List<Keyframe> keys, bool prepareUndo = true)
        {
            if (keys != null && keys.Count > 0) {
                if (prepareUndo) PrepareUndoForSelectedKeys();
                foreach (Keyframe k in keys) {
                    if (k.Channel == null) {
                        ResetupKeyframes();
                        if (k.Channel == null) {
                            Debug.LogError("Failed gathering keyframe KeyChannel reference");
                        }
                    }
                    if (!k.Channel.IsHidden) {
                        UndoUtil.Undo(k.Behavior, "Delete Keys");
                        k.Channel.UnsetKey(k);
                    }
                }
            }
        }

        public void DeleteKeysInChannel(TimeflowChannel ch)
        {
            if (ch != null) {
                PrepareUndoForSelectedKeys();
                UndoUtil.Undo(ch.Behavior, "Delete Keys In Channel");
                ch.ClearKeys(true);
            }
        }

        #endregion

        #region INSERT KEYS

        public Keyframe AddKeyframeAtTime(TimeflowChannel ch, float time, bool isLocalTime)
        {
            PrepareUndoForSelectedKeys();
            if (ch.Behavior != null) {
                UndoUtil.Undo(ch.Behavior, $"Added Keyframe at {time}");
            }
            Input.LastAddedKey = ch.SetKey(time, time, isLocalTime);
            if (Input.LastAddedKey == null) {
                return null;
            }

            float kx = PositionOfTime(Input.LastAddedKey.KeyTimeWorld, true);
            //Debug.Log($"Input.LastAddedKey.KeyTimeWorld:{Input.LastAddedKey.KeyTimeWorld}");
            float ky;
            if (IsGraphMode) {
                ky = PositionOfValue(Input.LastAddedKey.KeyValue, true);
            }
            else {
                ky = ch.GUIRect.y;
            }
            Input.LastAddedKey.GUIRect = new GUIRect(kx - 8, ky - 8, 16, 16);
            Input.LastAddedKey.UpdateSelectedAttributes(true);

            SetupSelectedKeys(true);
            SelectedKeys.Add(Input.LastAddedKey);
            SelectedKeysChanged();

            KeyframesTouched = true;
            ObjectTouched = true;
            return Input.LastAddedKey;
        }

        public Keyframe AddKeyframeAtPosition(Vector2 pos)
        {
            Input.InsertedKey = null;
            if (Display.Objects != null) {
                PrepareUndoForSelectedKeys();
                float time = TimeOfPosition(pos.x, true);

                if (IsGraphMode) {
                    float value = ValueOfPosition(pos.y, true);
                    float nearest = 0f;
                    TimeflowChannel nearestCh = null;

                    foreach (TimeflowObject obj in Display.Objects) {
                        if (!obj.IsLocked && obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (IsGraphLocked && IsGraphSolo && !ch.IsSelected) continue;
                                if (!ch.IsTrack && !ch.IsHidden && !ch.IsLocked && ch.IsEnabled && (ch.IsSelected || ch.IsGraphLocked)) {
                                    float channelTime = time + ch.TimeOffsetWorld;
                                    // Search for the nearest channel curve to the user click
                                    float cv = ch.InterpolateValue(channelTime, false, false);
                                    float dif = Mathf.Abs(value - cv);
                                    if (nearestCh == null || dif < nearest) {
                                        nearestCh = ch;
                                        nearest = dif;
                                    }
                                }
                            }
                        }
                    }
                    if (nearestCh != null) {
                        float channelTime = time - nearestCh.TimeOffsetWorld;
                        UndoUtil.Undo(nearestCh.Behavior, $"Added Keyframe at {time}");
                        if (nearestCh.IsColor) {
                            Input.InsertedKey = nearestCh.SetKeyColor(channelTime, new Color(value, value, value, value));
                        }
                        else
                        if (nearestCh.IsMultichannel && !nearestCh.IsSingleAttribute) {
                            Input.InsertedKey = nearestCh.SetKeyVector(channelTime, new Vector4(value, value, value, value));
                        }
                        else {
                            Input.InsertedKey = nearestCh.SetKeyValue(channelTime, value);
                        }
                    }
                }
                else {
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (!obj.IsLocked && obj.AllChannels != null && obj.IsDisplayed) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden && ch.HitTest(pos)) {
                                    UndoUtil.Undo(ch.Behavior, $"Added Keyframe at {time}");
                                    if (ch.IsTrack) {
                                        float endtime = time + Snap;
                                        Input.InsertedKey = ch.SetKey(time, endtime, false);
                                    }
                                    else {
                                        Input.InsertedKey = AddKeyframeAtTime(ch, time, false);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Input.InsertedKey != null) {
                    SetupSelectedKeys(true);
                    Input.InsertedKey.UpdateSelectedAttributes(true);
                    SelectKey(Input.InsertedKey);
                    SelectedKeysChanged();
                }
            }

            KeyframesTouched = true;
            ObjectTouched = true;
            return Input.InsertedKey;
        }

        public void AddKeyframeOnSelectedChannels()
        {
            if (SelectedChannels == null || SelectedChannels.Count == 0) return;
            PrepareUndoForSelectedKeys();
            float time = Timeflow.CurrentTime;
            foreach (TimeflowChannel ch in SelectedChannels) {
                if (IsGraphMode && ch.IsTrack) continue;
                Debug.Log($"Adding keyframe:{ch.Name}");
                AddKeyframeAtTime(ch, time, false);
            }
        }

        public void SetNewTrackEndAtPosition(Vector2 pos)
        {
            if (Input.InsertedKey != null && Input.InsertedKey.IsTrack) {
                PrepareUndoForSelectedKeys();
                float time = TimeOfPosition(pos.x, true);
                //Debug.Log($"SetNewTrackEndAtPosition:{time} pos:{pos.x}");
                Input.InsertedKey.SetOutTime(time, false);
                KeyframesTouched = true;
                ObjectTouched = true;
            }
        }

        #endregion

        #region SELECTED KEYS

        /// <summary>
        /// Toggles either the enabled or locked state for the selected keys and marker. The state is
        /// determined by the first key selected, inverting its current state, then setting all others in
        /// the selection to the same value.
        /// </summary>
        /// <param name="enableState">If true, enabled is toggled - or if false, locked is toggled</param>
        public void SelectedKeysToggleEnabled()
        {
            bool isFirst = true;
            bool enabled = false;

            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.Behavior != null) {
                        UndoUtil.Undo(k.Behavior, "Toggle Keys", true);
                        if (isFirst) {
                            isFirst = false;
                            enabled = !k.IsKeyEnabled;
                        }
                        k.IsKeyEnabled = enabled;
                        k.Behavior.Refresh();
                    }
                }
                ObjectTouched = true;
            }
            if (Timeflow.View.Markers.SelectedMarker != null) {
                UndoUtil.Undo(Timeflow, "Toggle Keys", true);
                if (isFirst) {
                    isFirst = false;
                    enabled = !Timeflow.View.Markers.SelectedMarker.Enabled;
                }
                Timeflow.View.Markers.SelectedMarker.Enabled = enabled;
            }
        }

        public void SelectedKeysToggleLocked()
        {
            bool isFirst = true;
            bool enabled = false;

            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.Behavior != null) {
                        UndoUtil.Undo(k.Behavior, "Toggle Keys", true);
                        if (isFirst) {
                            isFirst = false;
                            enabled = !k.LockTime;
                        }
                        k.LockTime = enabled;
                        k.LockValue = enabled;
                        k.Behavior.Refresh();
                    }
                }
                ObjectTouched = true;
            }
            if (Timeflow.View.Markers.SelectedMarker != null) {
                UndoUtil.Undo(Timeflow, "Toggle Keys", true);
                if (isFirst) {
                    isFirst = false;
                    enabled = !Timeflow.View.Markers.SelectedMarker.Locked;
                }
                Timeflow.View.Markers.SelectedMarker.Locked = enabled;
            }
        }

        public void EnableSelectedKeys(bool enable)
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.Behavior != null) {
                        UndoUtil.Undo(k.Behavior, "Toggle Keys");
                        k.IsKeyEnabled = enable;
                    }
                }
                ObjectTouched = true;
            }
        }

        public void UpdateSelectedKeys(bool force = false)
        {
            if (!Input.IsDragging && (KeyframesTouched || force)) {
                KeyframesTouched = false;
                if (SelectedKeys != null) {

                    // Remove any Keyframe with a null Channel or null Behavior  
                    SelectedKeys.RemoveAll(k => k == null || k.Channel == null || k.Behavior == null);

                    foreach (Keyframe k in SelectedKeys) {
                        // Clean up any keys occupying the same time  
                        if (k != null && k.Behavior != null) {
                            k.Behavior.CleanUp();
                        }
                    }
                }
            }
        }

        public void DuplicateSelectedKeys(float timeOffset = 0, bool calculateOffset = false)
        {
            PrepareUndoForSelectedKeys();

            if (Timeflow.View.Markers.SelectedMarker != null) {
                UndoUtil.Undo(Timeflow, "Duplicate Keys", true);
                Timeflow.View.Markers.SelectedMarker = Timeflow.Markers.AddMarker(Timeflow.View.Markers.SelectedMarker, Timeflow.View.Markers.SelectedMarker.Time + SnapUnit);
            }

            if (SelectedKeys == null || SelectedKeys.Count == 0) return;
            if (calculateOffset) {
                // Duplicate the keys immediately following the current selection
                Vector2 range = GetSelectedKeysTimeRange();
                timeOffset = range.y - range.x;
            }

            List<Keyframe> newSelectedKeys = new List<Keyframe>();
            List<TimeflowEvent> newSelectedEvents = new List<TimeflowEvent>();
            List<Keyframe> newRelatedKeys = new List<Keyframe>();
            List<Keyframe> newRelatedTracks = new List<Keyframe>();
            List<TimeflowEvent> newRelatedEvents = null;

            selectOrder = 0;

            foreach (Keyframe key in SelectedKeys) {
                UndoUtil.Undo(key.Behavior, "Duplicate Keys", true);
                Keyframe copy = key.Channel.CopyKey(key, timeOffset, false, true);
                if (Input.DragPrimaryKey == key) {
                    Input.DragPrimaryKey = copy;
                }
                copy.SelectOrder = selectOrder++;
                newSelectedKeys.Add(copy);
            }

            if (SelectedEvents != null && SelectedEvents.Count > 0) {
                foreach (TimeflowEvent ev in SelectedEvents) {
                    UndoUtil.Undo(ev.ParentObject, "Duplicate Keys", true);
                    TimeflowEvent copy = ev.gameObject.AddComponent(ev.GetType()) as TimeflowEvent;
                    copy.Copy(ev);
                    copy.TriggerTime += timeOffset;
                    UndoUtil.UndoCreate(copy, "Duplicate Events");

                    if (Input.DragPrimaryEvent == ev) Input.DragPrimaryEvent = copy;

                    if (newRelatedEvents == null) newRelatedEvents = new List<TimeflowEvent>();
                    copy.IsSelected = true;
                    newRelatedEvents.Add(copy);
                    newSelectedEvents.Add(copy);
                }
            }

            if (UseRelatedKeys) {
                if (RelatedKeys != null) {
                    foreach (Keyframe key in RelatedKeys) {
                        if (!SelectedKeys.Contains(key)) {
                            UndoUtil.Undo(key.Behavior, "Duplicate Keys", true);
                            Keyframe copy = key.Channel.CopyKey(key, timeOffset, false, true);
                            if (Input.DragPrimaryKey == key) Input.DragPrimaryKey = copy;
                            newRelatedKeys.Add(copy);
                        }
                    }
                }
                if (RelatedTracks != null) {
                    foreach (Keyframe key in RelatedTracks) {
                        if (!SelectedKeys.Contains(key)) {
                            UndoUtil.Undo(key.Behavior, "Duplicate Keys", true);
                            Keyframe copy = key.Channel.CopyKey(key, timeOffset, false, true);
                            if (Input.DragPrimaryKey == key) Input.DragPrimaryKey = copy;
                            newRelatedTracks.Add(copy);
                        }
                    }
                }
                if (RelatedEvents != null && RelatedEvents.Count > 0) {
                    foreach (TimeflowEvent ev in RelatedEvents) {
                        if (!SelectedEvents.Contains(ev)) {
                            UndoUtil.Undo(ev.ParentObject, "Duplicate Keys", true);
                            TimeflowEvent copy = ev.gameObject.AddComponent(ev.GetType()) as TimeflowEvent;
                            copy.Copy(ev);
                            copy.TriggerTime += timeOffset;
                            UndoUtil.UndoCreate(copy, "Duplicate Events");

                            if (Input.DragPrimaryEvent == ev) Input.DragPrimaryEvent = copy;

                            if (newRelatedEvents == null) newRelatedEvents = new List<TimeflowEvent>();
                            newRelatedEvents.Add(copy);
                        }
                    }
                }
            }

            KeyframesTouched = true;
            ObjectTouched = true;
            SelectedKeys = newSelectedKeys;
            SelectedEvents = newSelectedEvents;
            RelatedKeys = newRelatedKeys;
            RelatedTracks = newRelatedTracks;
            RelatedEvents = newRelatedEvents;

            SelectedKeysChanged();
        }

        public void DeleteSelectedKeys()
        {
            DeleteKeys(SelectedKeys, true);
            Timeflow.View.DeleteSelectedEvents();

            if (UseRelatedKeys) {
                DeleteKeys(RelatedKeys, false);
                DeleteKeys(RelatedTracks, false);
                RelatedKeys = null;
                RelatedTracks = null;
            }

            KeyframesTouched = true;
            ObjectTouched = true;
            SetupSelectedKeys(true);
            SelectedKeysChanged();
        }

        public void SetupSelectedKeys(bool clear = false)
        {
            if (SelectedKeys == null || clear) {
                selectOrder = 0;
                SelectedKeys = new List<Keyframe>();
            }
        }

        public void DeleteKeysInSelectedChannels()
        {
            if (SelectedChannels != null) {
                PrepareUndoForSelectedKeys();
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch != null && !ch.IsHidden && ch.Behavior != null) {
                        UndoUtil.Undo(ch.Behavior, "Delete Keys in Selected Channels");
                        ch.ClearKeys(true);
                    }
                }
            }
        }

        /// <summary>
        /// This method should be called before any operation that may add or remove keyframes affecting
        /// the current selection. This makes a copy of the selected keyframes to restore upon undo to fix
        /// the issue with new keyframes inserted being held in the selection when they were created by the
        /// operation being undone.
        /// </summary>
        public void PrepareUndoForSelectedKeys()
        {
            SelectedKeysUndo = null;
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                SelectedKeysUndo = new List<Keyframe>(SelectedKeys);
            }
        }

        /// <summary>
        /// Ensures that each keyframe in the selection is valid and belongs to a channel. When an
        /// operation is performed that inserts new keys, such as splitting a track section, this method
        /// makes sure when undone each keyframe in the selection is valid. Checking null isn't enough
        /// since orphaned keys can still exist in memory, so this refetches each key reference from the
        /// keyframe time. There can only be 1 key at each time in a channel. This can only restore key
        /// selection for 1 undo operation. Additional undos will work fine, but the selection will not be
        /// restored for subsequent undos.
        /// </summary>
        public void RestoreUndoForSelectedKeys()
        {

            LastSelectedKeys = null; /// Clear selection

            if (SelectedKeysUndo != null && SelectedKeysUndo.Count > 0) {
                /// Restore the previous selection and removing any null keys or those without a channel
                SelectedKeys = new List<Keyframe>();
                foreach (Keyframe k in SelectedKeysUndo) {
                    if (k != null && k.Channel != null) {
                        Keyframe k2 = k.Channel.GetKeyAtTime(k.KeyTime);
                        if (k2 != null) {
                            if (k2.Channel != k.Channel) {
                                k2.Channel = k.Channel; /// Just in case channel didn't get assigned
                            }
                            if (k2 != null && !SelectedKeys.Contains(k2)) {
                                SelectedKeys.Add(k2);
                            }
                        }
                    }
                }
            }

            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                /// Sometimes undo can disconnect keyframes from channels, so this corrects that.
                foreach (TimeflowChannel ch in Timeflow.Channels) {
                    if (ch.Keys != null && ch.Keys.Count > 0) {
                        foreach (Keyframe k in ch.Keys) {
                            k.Channel = ch;
                        }
                    }
                }
            }

            /// Rebuild selection to be sure there are no null values
            ValidateSelection();

            /// Reprepare for subsequent undo calls. It can't guarantee original selection but it's better
            /// than nothing.
            PrepareUndoForSelectedKeys();
        }

        #endregion

        #region SELECTED TRACKS

        public void ResetSelectedTracks()
        {
            if (SelectedKeys == null) return;
            PrepareUndoForSelectedKeys();
            foreach (Keyframe k in SelectedKeys) {
                if (k.IsTrack) {
                    TimeflowObject tobj = (TimeflowObject)k.Behavior;
                    if (tobj != null) {
                        tobj.ResetTrack();
                    }
                }
            }
        }

        public void SplitSelectedTracksAtTime(float time, bool selectNewKeys = true, bool prepareUndo = true)
        {
            if (Display.Objects != null) {
                if (prepareUndo) PrepareUndoForSelectedKeys();
                bool selectedOnly = SelectedKeys != null && SelectedKeys.Count > 0;
                List<Keyframe> tracks = new List<Keyframe>();

                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                        UndoUtil.Undo(obj, "Split Tracks", true);
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.Object != null) {
                                UndoUtil.Undo(ch.Object, "Split Tracks", true);
                                ch.Object.Track.AutoFullLength = false;
                            }
                            float timeLocal = time - (ch.Object == null ? 0 : ch.TimeOffsetWorld);
                            foreach (Keyframe k in ch.Keys) {
                                if (k.IsTrack && (!selectedOnly || SelectedKeys.Contains(k))) {
                                    if (k.KeyTime < timeLocal && k.KeyValue > timeLocal) {
                                        tracks.Add(k);
                                    }
                                }
                            }
                        }
                    }
                }

                if (tracks.Count > 0) {
                    if (selectNewKeys) {
                        SetupSelectedKeys(true);
                    }
                    foreach (Keyframe k in tracks) {
                        UndoUtil.Undo(k.Behavior, "Split Tracks", true);
                        if (k.Channel.Object != null) k.Channel.Object.Track.AutoFullLength = false;
                        k.LockTime = k.LockValue = false;
                        k.IsAutoTrackLength = false;
                        Keyframe split = new Keyframe(k);
                        k.Channel.KeysAdd(split);
                        split.LockTime = split.LockValue = false;
                        float timeLocal = time - (k.Channel == null ? 0 : k.Channel.TimeOffsetWorld);
                        k.KeyValue = timeLocal;
                        split.KeyTime = timeLocal;
                        if (selectNewKeys) {
                            SelectKey(split);
                            SelectedKeysChanged();
                        }
                        k.Channel.ClearDuplicateKeys();
                    }
                }
            }
        }

        public void SplitSelectedTracksByWorkArea()
        {
            PrepareUndoForSelectedKeys();
            SplitSelectedTracksAtTime(Timeflow.WorkAreaStart, true, false);
            SplitSelectedTracksAtTime(Timeflow.WorkAreaEnd, false, false);
        }

        public void SetSelectedTracksToWorkArea(bool selectNewKeys = true)
        {
            if (Display.Objects != null) {
                PrepareUndoForSelectedKeys();
                bool selectedOnly = SelectedKeys != null && SelectedKeys.Count > 0;
                List<Keyframe> tracks = new List<Keyframe>();

                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                        float timeOffset = obj.TimeOffsetWorld;
                        float startTime = Timeflow.WorkAreaStart - timeOffset;
                        float endTime = Timeflow.WorkAreaEnd - timeOffset;
                        foreach (Keyframe k in obj.Track.Keys) {
                            if (!selectedOnly || SelectedKeys.Contains(k)) {
                                if (MathUtil.Overlaps(k.KeyTime, k.KeyValue, startTime, endTime)) {
                                    UndoUtil.Undo(obj, "Set Tracks To Work Area", true);
                                    obj.Track.AutoFullLength = false;
                                    k.LockTime = false;
                                    k.LockValue = false;
                                    k.KeyTime = startTime;
                                    k.KeyValue = endTime;
                                    tracks.Add(k);
                                }
                            }
                        }
                    }
                }

                if (tracks.Count > 0 && selectNewKeys) {
                    SetupSelectedKeys(true);
                    foreach (Keyframe k in tracks) {
                        SelectKey(k);
                        SelectedKeysChanged();
                    }
                }
            }
        }

        public void JoinSelectedTracks()
        {
            if (SelectedKeys != null && Display.Objects != null) {
                PrepareUndoForSelectedKeys();
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked && obj.BehaviorsEnabled) {
                        Keyframe first = null;
                        float endTime = 0f;
                        List<Keyframe> selected = new List<Keyframe>();
                        foreach (Keyframe k in obj.Track.Keys) {
                            if (SelectedKeys.Contains(k)) {
                                if (first == null) {
                                    first = k;
                                }
                                else
                                if (k.KeyTime < first.KeyTime) {
                                    first = k;
                                }
                                selected.Add(k);

                                if (endTime < k.KeyValue) {
                                    endTime = k.KeyValue;
                                }
                            }
                        }

                        // Reduce the selected keys to just 1 that fills the same time span
                        if (first != null) {
                            obj.Track.JoinTracks(first, selected, endTime);

                            //// Find any other keyframes within the time range
                            //foreach (Keyframe k in obj.Track.Keys) {
                            //    if (k != null && !selected.Contains(k)) {
                            //        if (k.KeyTime >= first.KeyTime && k.KeyValue <= endTime) {
                            //            selected.Add(k);
                            //        }
                            //    }
                            //}
                            //UndoUtil.UndoRecord(first.Behavior, "Join Tracks");

                            //first.LockValue = false;
                            //first.KeyValue = endTime;
                            //foreach (Keyframe k in selected) {
                            //    if (k != first) {
                            //        k.Channel.KeysRemove(k);
                            //    }
                            //}
                        }
                    }
                }
            }
        }

        #endregion

        #region TANGENTS & INTEREPOLATION

        public void UpdateTangents()
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.AllChannels != null && obj.IsDisplayed) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && ch != obj.Track && !ch.IsLocked) {
                                ch.UpdateTangents();
                            }
                        }
                    }
                }
            }
        }

        public void SetUnifiedTangentsOfSelectedKeyframes(int locked)
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                bool a = locked > 0;
                bool b = locked > 1;

                foreach (Keyframe k in SelectedKeys) {
                    UndoUtil.Undo(k.Behavior, "Unify Tangents");
                    if (!a) {
                        k.UnifyTangents = a;
                        k.UnifyTangentLengths = a;
                    }
                    else
                    if (k.UnifyTangents != a || k.UnifyTangentLengths != b) {
                        k.UnifyTangents = a;
                        k.UnifyTangentLengths = b;
                        k.OutTangent = new Vector2(-k.InTangent.x, -k.InTangent.y);
                    }
                }
                KeyframesTouched = true;
                ObjectTouched = true;
            }
        }

        public void SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations interp)
        {
            SetInterpolationOfSelectedKeyframes(interp, false);
        }

        public void SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations interp, bool isAutoTangents)
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                foreach (Keyframe k in SelectedKeys) {
                    UndoUtil.Undo(k.Behavior, "Set Interpolation " + interp);
                    k.SetInterpolation(interp, (Event.current != null && Event.current.shift) || isAutoTangents);
                    k.IsAutoTangents = isAutoTangents;
                    if (k.Channel != null && !channels.Contains(k.Channel)) {
                        channels.Add(k.Channel);
                    }
                }

                if (channels.Count > 0) {
                    foreach (TimeflowChannel channel in channels) {
                        channel.UpdateTangents();
                    }
                }
                KeyframesTouched = true;
                ObjectTouched = true;
            }
        }

        public void SnapTimeOfSelectedKeyframes()
        {
            if (SelectedKeys != null) {
                foreach (Keyframe k in SelectedKeys) {
                    UndoUtil.Undo(k.Behavior, "Snap Time");
                    k.KeyTimeWorld = SnapTime(k.KeyTimeWorld, true);
                }
            }
            if (SelectedEvents != null) {
                foreach (TimeflowEvent k in SelectedEvents) {
                    UndoUtil.Undo(k, "Snap Time");
                    k.TriggerTimeWorld = SnapTime(k.TriggerTimeWorld, true);
                }
            }
            if (Timeflow.View.Markers.SelectedMarker != null) {
                UndoUtil.Undo(Timeflow, "Snap Time", true);
                Timeflow.View.Markers.SelectedMarker.Time = SnapTime(Timeflow.View.Markers.SelectedMarker.Time, true);
            }
            KeyframesTouched = true;
            ObjectTouched = true;
        }

        public void SnapValuesOfSelectedKeyframes()
        {
            if (SelectedKeys != null) {
                foreach (Keyframe k in SelectedKeys) {
                    if (!k.IsTrack) {
                        UndoUtil.Undo(k.Behavior, "Snap Values");
                        k.KeyValue = SnapValue(k.KeyValue, true);
                    }
                    else {
                        UndoUtil.Undo(k.Behavior, "Snap Values");
                        k.KeyEndTimeWorld = SnapTime(k.KeyEndTimeWorld, true);
                    }
                }
            }
            KeyframesTouched = true;
            ObjectTouched = true;
        }

        public enum KeyframeModifyModes
        {
            Mirror,
            Randomize
        }

        public void ModifyTimeOfSelectedKeyframes(KeyframeModifyModes mode)
        {
            /// Collect behaviors to prepare undo
            List<TimeflowBehavior> behaviors = new List<TimeflowBehavior>();

            /// Determine min and max time values from selection
            float minTime = float.MaxValue;
            float maxTime = float.MinValue;

            if (SelectedKeys != null) {
                foreach (Keyframe k in SelectedKeys) {
                    if (minTime > k.KeyTimeWorld) {
                        minTime = k.KeyTimeWorld;
                    }
                    if (maxTime < k.KeyTimeWorld) {
                        maxTime = k.KeyTimeWorld;
                    }
                    if (!behaviors.Contains(k.Behavior)) {
                        behaviors.Add(k.Behavior);
                    }
                }
            }

            if (SelectedEvents != null) {
                foreach (TimeflowEvent k in SelectedEvents) {
                    if (minTime > k.TriggerTimeWorld) {
                        minTime = k.TriggerTimeWorld;
                    }
                    if (maxTime < k.TriggerTimeWorld) {
                        maxTime = k.TriggerTimeWorld;
                    }
                    if (!behaviors.Contains(k)) {
                        behaviors.Add(k);
                    }
                }
            }

            if (maxTime > minTime) {
                if (behaviors.Count > 0) {
                    foreach (TimeflowBehavior b in behaviors) {
                        UndoUtil.Undo(b, "Modify Time", true);
                    }
                }
                if (SelectedKeys != null) {
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.IsTrack) {
                            if (mode == KeyframeModifyModes.Mirror) {
                                float start = k.KeyTimeWorld;
                                float end = k.KeyEndTimeWorld;

                                k.KeyTimeWorld = maxTime - (end - minTime);
                                k.KeyEndTimeWorld = maxTime - (start - minTime);
                            }
                            else
                            if (mode == KeyframeModifyModes.Randomize) {
                                float dur = k.KeyValue - k.KeyTime;
                                float t = minTime + (Random.value * (maxTime - minTime));
                                k.KeyTimeWorld = t;
                                k.KeyEndTimeWorld = t + dur;
                            }
                        }
                        else {
                            if (mode == KeyframeModifyModes.Mirror) {
                                k.KeyTimeWorld = maxTime - (k.KeyTimeWorld - minTime);
                                k.MirrorTangentsTime();
                            }
                            else
                            if (mode == KeyframeModifyModes.Randomize) {
                                k.KeyTimeWorld = minTime + (Random.value * (maxTime - minTime));
                            }
                        }
                    }
                }
                if (SelectedEvents != null) {
                    foreach (TimeflowEvent k in SelectedEvents) {
                        k.TriggerTimeWorld = maxTime - (k.TriggerTimeWorld - minTime);
                    }
                }
            }

            KeyframesTouched = true;
            ObjectTouched = true;
        }

        public void ModifyValuesOfSelectedKeyframes(KeyframeModifyModes mode)
        {
            /// Gather affected behaviors to prepare undo
            List<TimeflowBehavior> behaviors = new List<TimeflowBehavior>();

            /// Gather min and max values of selected keyframes across all attributes
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            if (mode == KeyframeModifyModes.Randomize && IsGraphMode) {
                /// Use the current graph view scale
                minValue = GraphMinValue;
                maxValue = GraphMaxValue;
            }
            if (SelectedKeys != null) {
                foreach (Keyframe k in SelectedKeys) {
                    if (k.IsTrack) continue;
                    if (mode == KeyframeModifyModes.Randomize && IsGraphMode) {
                    }
                    else {
                        if (k.IsVector || k.IsColor) {
                            if (minValue > k._KeyVector.x) {
                                minValue = k._KeyVector.x;
                            }
                            if (maxValue < k._KeyVector.x) {
                                maxValue = k._KeyVector.x;
                            }

                            if (minValue > k._KeyVector.y) {
                                minValue = k._KeyVector.y;
                            }
                            if (maxValue < k._KeyVector.y) {
                                maxValue = k._KeyVector.y;
                            }

                            if (minValue > k._KeyVector.z) {
                                minValue = k._KeyVector.z;
                            }
                            if (maxValue < k._KeyVector.z) {
                                maxValue = k._KeyVector.z;
                            }

                            if (minValue > k._KeyVector.w) {
                                minValue = k._KeyVector.w;
                            }
                            if (maxValue < k._KeyVector.w) {
                                maxValue = k._KeyVector.w;
                            }

                        }
                        else {
                            if (minValue > k.KeyValue) {
                                minValue = k.KeyValue;
                            }
                            if (maxValue < k.KeyValue) {
                                maxValue = k.KeyValue;
                            }
                        }
                    }
                    if (!behaviors.Contains(k.Behavior)) {
                        behaviors.Add(k.Behavior);
                    }
                }
            }

            if (maxValue > minValue) {
                if (behaviors.Count > 0) {
                    foreach (TimeflowBehavior b in behaviors) {
                        UndoUtil.Undo(b, "Modify Values", true);
                    }
                }
                if (SelectedKeys != null) {
                    foreach (Keyframe k in SelectedKeys) {
                        if (k.IsTrack) continue;
                        if (mode == KeyframeModifyModes.Mirror) {
                            if (k.IsVector || k.IsColor) {
                                k._KeyVector.x = maxValue - (k._KeyVector.x - minValue);
                                k._KeyVector.y = maxValue - (k._KeyVector.y - minValue);
                                k._KeyVector.z = maxValue - (k._KeyVector.z - minValue);
                                k._KeyVector.w = maxValue - (k._KeyVector.w - minValue);
                            }
                            else {
                                k.KeyValue = maxValue - (k.KeyValue - minValue);
                            }
                            k.MirrorTangentsValue();
                        }
                        else
                        if (mode == KeyframeModifyModes.Randomize) {
                            if (k.IsVector || k.IsColor) {
                                k._KeyVector.x = minValue + (Random.value * (maxValue - minValue));
                                k._KeyVector.y = minValue + (Random.value * (maxValue - minValue));
                                k._KeyVector.z = minValue + (Random.value * (maxValue - minValue));
                                k._KeyVector.w = minValue + (Random.value * (maxValue - minValue));
                            }
                            else {
                                k.KeyValue = minValue + (Random.value * (maxValue - minValue));
                            }
                        }
                    }
                }
            }

            KeyframesTouched = true;
            ObjectTouched = true;
        }

        #endregion

        #region GUI

        public void GUITracks()
        {
            if (Timeflow.RootObjectsCached != null) {
                GUIBeginGroup(Layout.TimeAreaInner);
                foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                    if (obj == null) continue;
                    GUITracksRecursive(obj);
                }
                GUIEndGroup();
            }
        }

        public void GUITracksRecursive(TimeflowObject obj)
        {
            if (obj.IsDisplayed) {
                Handles.BeginGUI();
                GUI.color = Timeflow.IsLocked ? AxonColor.LockedOverlay : AxonColor.Default;

                if (obj.AllChannelsForDisplay != null) {
                    float y = obj.GUIRect.y;

                    // First draw the main track governing object visibility and all behaviors on the objects
                    float height = TimeflowPreferences.Current.DefaultChannelHeight;

                    obj.Track.GUIRect = new GUIRect(0, y, Layout.TimeAreaInner.Width, height);
                    obj.Track.GUIChannel();
                    obj.Track.GUITracks();

                    y += height;
                    GUI.color = AxonColor.Default;

                    if (!obj.IsCollapsed) {
                        // Draw all subsequent tracks - assuming they are already ordered correctly
                        foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                            if (!ch.IsHidden && !ch.IsTrack) {
                                GUI.color = AxonColor.Default;
                                ch.GUIChannel();
                                y += ch.GUIHeight;
                            }
                        }
                    }

                    if (obj.Behaviors != null) {
                        foreach (TimeflowBehavior b in obj.Behaviors) {
                            if (b == null) continue;
                            b.GUIChannelOverlay();
                        }
                    }
                }
                Handles.EndGUI();
            }


            if ((!obj.IsCollapsed || Display.EnabledOnly) && obj.ShowChildren && obj.Children != null) {
                // Continue rendering children
                foreach (TimeflowObject child in obj.Children) {
                    GUITracksRecursive(child);
                }
            }
        }

        public void GUIKeyframes()
        {
            if (Timeflow.RootObjectsCached == null) return;
            GUIBeginGroup(Layout.TimeAreaInner);
            foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                if (obj == null) continue;
                GUIKeyframesRecursive(obj);
            }
            AlignTools.Draw();
            GUIEndGroup();
        }

        public void GUIKeyframesRecursive(TimeflowObject obj)
        {
            GUI.color = AxonColor.Default;
            if (obj.IsSelectable) {
                if (!obj.IsCollapsed && obj.AllChannelsForDisplay != null &&
                    Display.ChannelMode != TimeflowViewDisplay.ChannelModes.Objects && (obj.DisplayChannels || Display.ChannelMode == 0)) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        GUI.color = AxonColor.Default;

                        if (ch.IsTrack) {
                            ch.GUITracksShade(true);
                        }
                        else
                        if (ch.DisplayChannel || Display.ChannelMode == 0) {
                            ch.GUIKeyframes();
                            ch.GUITracksShade(false);
                            ch.GUITracksShade(true);
                        }
                    }
                }
                if (obj.Events != null && obj.Events.Count > 0) {
                    //_yOffset = obj.EventGUI.Top;
                    if (obj.EventGUI == null) {
                        obj.EventGUI = new GUIObject("EventGUI");
                    }
                    if (IsLayout) {
                        obj.EventGUI.Left = 0;
                        obj.EventGUI.Top = (int)obj.GUIRect.y;
                        obj.EventGUI.Width = Layout.Hierarchy.Width;
                        obj.EventGUI.Height = Layout.RowHeight;
                    }

                    foreach (TimeflowEvent evt in obj.Events) {
                        if (evt != null) {
                            evt.GUIRect = new GUIRect(PositionOfTime(evt.TriggerTimeWorld, true) - 7, obj.EventGUI.Top + 2,
                                TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize);
                            evt.GUIKeyframes();
                        }
                    }
                }
                obj.Track.GUITracksShade(true);
                GUI.color = AxonColor.Default;
            }

            if ((!obj.IsCollapsed || Display.EnabledOnly) && obj.ShowChildren && obj.Children != null) {
                foreach (TimeflowObject child in obj.Children) {
                    GUIKeyframesRecursive(child);
                }
            }
        }

        #endregion

    }
}//AxonGenesis

#endif

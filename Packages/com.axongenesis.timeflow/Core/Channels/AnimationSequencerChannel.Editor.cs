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
    public partial class AnimationSequencerChannel : TimeflowChannel
    {
        private enum DragModes
        {
            None,
            OutTime, // InTime is set by keyframe
            StartTime,
            EndTime,
            InBlend,
            OutBlend,
            Speed
        }

        private Vector2 dragStart;
        private Vector2 dragOffset;
        private AnimationSequencerKey dragKey;
        private AnimationSequencerKey replaceKey;
        private List<AnimationClip> dropKeys = new List<AnimationClip>();
        private float dragStartKeyTime = 0f;
        private float dragStartSpeed = 1f;
        private float dragStartDuration = 0f;
        private float doubleClickStartTime = 0f;
        private float _dropAtTime = 0;
        private Vector3[] curvePoints = new Vector3[64];
        private DragModes _DragMode = DragModes.None;
        private List<Keyframe> _SelectedKeys;

        private DragModes DragMode {
            get { return _DragMode; }
            set {
                if (_DragMode != value) {
                    _DragMode = value;
                    if (_DragMode == DragModes.None) {
                        if (dragKey != null) {
                            dragKey.OnDragEnded();
                            dragKey.ForceShowClipRanges = false;
                        }
                        dragKey = null;
                    }
                    else
                    if (dragKey != null) {
                        dragKey.ForceShowClipRanges = true;
                        dragKey.OnDragStart();
                    }

                }
            }
        }

        private List<Keyframe> SelectedKeys {
            get {
                if (Timeflow.Active != null && Timeflow.Active.View != null && Timeflow.Active.View.SelectedKeys != null) {

                    return Timeflow.Active.View.SelectedKeys;
                }

                if (_SelectedKeys == null) {
                    _SelectedKeys = new List<Keyframe>();
                }
                return _SelectedKeys;
            }
        }

        public override string DisplayName {
            get {
                string append = "";
                if (Mask != null) {
                    append = " [M]";
                }
                if (IsAdditive) {
                    append += " [A]";
                }
                return base.DisplayName + append;
            }
        }

        public override void ResetName()
        {
            Name = null;
            ValidateName();
        }

        public override bool GUICustomHit(Vector2 pos)
        {
            DragMode = DragModes.None;

            bool hit = false;
            for (int i = 0; i < Keys.Count; i++) {
                Keyframe k = Keys[i];

                var b = k.CustomKey as AnimationSequencerKey;
                if (b == null) continue;

                if (!k.IsKeyEnabled) continue;
                if (b.InBlendHandleGUIRect.Contains(pos)) {
                    dragKey = b;
                    DragMode = DragModes.InBlend;
                    hit = true;
                }
                else
                if (b.OutBlendHandleGUIRect.Contains(pos)) {
                    dragKey = b;
                    DragMode = DragModes.OutBlend;
                    hit = true;
                }
                else
                if (b.OutTimeHandleGUIRect.Contains(pos)) {
                    dragKey = b;
                    if (Event.current != null && Event.current.shift) {
                        DragMode = DragModes.Speed;
                    }
                    else {
                        DragMode = DragModes.OutTime;
                    }
                    hit = true;
                }
                else
                if (b.StartTimeHandleGUIRect.Contains(pos)) {
                    dragKey = b;
                    DragMode = DragModes.StartTime;
                    hit = true;
                }
                else
                if (k.GUIRect.Contains(pos)) {
                    if (Event.current != null & Event.current.control) {
                        // Double-click detection on the key body to open the clip in the Animation window
                        float dif = Time.realtimeSinceStartup - doubleClickStartTime;
                        if (doubleClickStartTime <= 0f || dif > 1f) {
                            doubleClickStartTime = Time.realtimeSinceStartup;
                        }
                        else {
                            doubleClickStartTime = 0f;
                            if (dif < 0.7f) {
                                if (b.Clip != null) {
                                    AnimationUtil.OpenAnimationClipInEditor(b.Clip);

                                    // Consume and stop further handling so no drag starts
                                    Event.current?.Use();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return hit;
        }

        public override void GUICustomHitEnded()
        {
            DragMode = DragModes.None;
        }

        public override void GUICustomDragStart(Vector2 pos)
        {
            if (dragKey == null || DragMode == DragModes.None) return;

            string action = "";

            if (DragMode == DragModes.OutTime) {
                action = "Drag Keyframe Out Time";
            }
            else
            if (DragMode == DragModes.Speed) {
                action = "Drag Keyframe Speed";
            }
            else
            if (DragMode == DragModes.StartTime) {
                action = "Drag Clip Start Time";
            }
            else
            if (DragMode == DragModes.EndTime) {
                action = "Drag Clip End Time";
            }
            else
            if (DragMode == DragModes.InBlend) {
                action = "Drag In Blend Time";
            }
            else
            if (DragMode == DragModes.OutBlend) {
                action = "Drag Out Blend Time";
            }

            UndoUtil.Undo(Behavior, action);
            dragStart = pos;
            dragOffset.x = dragStart.x - LoopOutDragRect.x;
            dragStartKeyTime = dragKey.Key.KeyTime - dragKey.StartTime;
            if (DragMode == DragModes.Speed) {
                dragStartSpeed = dragKey.Speed;
                // Starting desired duration is the current key display length (world end - start)
                dragStartDuration = Mathf.Max(0.0001f, (dragKey.Key.KeyTimeWorld + dragKey.Duration) - dragKey.Key.KeyTimeWorld);
            }
        }

        public override void GUICustomDrag(Vector2 pos)
        {
            float t = Timeflow.Active.View.TimeOfPosition(pos.x, false);
            t *= TimeScaleWorld;

            if (DragMode == DragModes.StartTime) {
                if (dragKey.Clip != null) {
                    float keyStartWorld = dragKey.Key.KeyTimeWorld;
                    float desiredTrackDuration = Mathf.Max(0.0001f, (keyStartWorld + dragKey.Duration) - t);
                    float newLocalLength = desiredTrackDuration * Mathf.Abs(dragKey.Speed);
                    // Compute new StartTime so that EndTime remains fixed in local clip space
                    float endLocal = dragKey.StartTime + dragKey.Duration;// dragKey.OutTime;
                    float newStart = endLocal - newLocalLength;
                    if (newStart < 0f) newStart = 0f;
                    if (newStart > endLocal - 0.0001f) newStart = endLocal - 0.0001f;
                    dragKey.StartTime = newStart;
                    dragKey.Key.KeyTime = Mathf.Max(t, dragStartKeyTime);
                    dragKey.OnValueChanged();
                }
            }
            else
            if (DragMode == DragModes.Speed) {
                if (dragKey.Clip != null) {
                    // Relative speed edit: scale from starting speed based on how much the desired
                    // key duration (start -> cursor) changes during drag.
                    float keyStartWorld = dragKey.Key.KeyTimeWorld;
                    float currentDesiredDuration = Mathf.Max(0.0001f, t - keyStartWorld);
                    float ratio = currentDesiredDuration / Mathf.Max(0.0001f, dragStartDuration); // >1 when dragging right
                    float newSpeed = dragStartSpeed / ratio; // right => slower; left => faster
                    newSpeed = Mathf.Clamp(newSpeed, 0.01f, 100f);

                    dragKey.Speed = newSpeed;
                    dragKey.OnValueChanged();
                }
            }
            else
            if (DragMode == DragModes.OutTime) {
                float keyStart = dragKey.Key.KeyTimeWorld;
                float newDuration = Mathf.Max(0.0001f, t - keyStart);
                if (dragKey.Clip != null && Event.current != null && Event.current.alt) {
                    float clipEnd = Mathf.Max(0.0001f, dragKey.EndTime - dragKey.StartTime) / dragKey.Speed;
                    float dif = Mathf.Abs(newDuration - clipEnd);
                    if (dif < lastSnapThreshold) newDuration = clipEnd;
                }
                //if (newEnd < dragKey.StartTime + 0.0001f) newEnd = dragKey.StartTime + 0.0001f;
                if (newDuration < 0.0001f) newDuration = 0.0001f;
                dragKey.Duration = newDuration;
                dragKey.OnValueChanged();
            }
            else
            if (DragMode == DragModes.InBlend) {
                if (dragKey.IsTransitionIn) {
                    dragKey.TransitionFromKey.OutTime = t;
                }
                else {
                    dragKey.InBlend = t - dragKey.Key.KeyTimeWorld;
                    if (dragKey.InBlend < 0f) dragKey.InBlend = 0f;
                }
            }
            else
            if (DragMode == DragModes.OutBlend) {
                if (dragKey.IsTransitionOut) {
                    dragKey.OutTime = t;
                }
                else {
                    // Drag OutBlend from left edge of tail region
                    float guiEndTime = dragKey.Key.KeyTimeWorld + dragKey.Duration;
                    // natural end of key based on trim
                    //float outStart = Mathf.Max(dragKey.Key.KeyTimeWorld, guiEndTime - dragKey.OutBlend);
                    // We use current cursor time t to define new out start; OutBlend = end - t
                    float newOut = guiEndTime - t;
                    if (newOut < 0f) newOut = 0f;
                    dragKey.OutBlend = newOut;
                }
            }
        }

        public override bool GUIDragAndHover()
        {
            bool show = base.GUIDragAndHover();
            if (show) return true;

            Vector2 pos = Timeflow.Active.Input.GetMousePosition(Timeflow.Active.Layout.TimeAreaInner);
            //Debug.Log($"pos:{pos} GUITrackRect:{GUITrackRect}");
            if (GUITrackRect.Contains(pos)) {
                //Debug.Log($"Hit channel {Name} @ {pos.x}");
                show = true;
                _dropAtTime = Timeflow.Active.View.TimeOfPosition(pos.x, true) * TimeScaleWorld;

                replaceKey = null;
                if (Event.current != null && Event.current.alt && Keys != null && Keys.Count > 0) {
                    foreach (var key in Keys) {
                        if (key.KeyTime <= _dropAtTime && key.KeyValue > _dropAtTime) {
                            _dropAtTime = key.KeyTime;
                            if (key.CustomKey is AnimationSequencerKey ak) {
                                replaceKey = ak;
                            }
                            break;
                        }
                    }
                }
            }
            else {
                pos = Timeflow.Active.Input.GetMousePosition(Timeflow.Active.Layout.Hierarchy.Rect);
                if (GUIRect.Contains(pos)) {
                    show = true;
                    _dropAtTime = CurrentTime;
                }
            }
            if (!show) {
                if (dropKeys != null) dropKeys.Clear();
                return false;
            }

            show = false; // reset and check for valid objects
            dropKeys = new List<AnimationClip>();
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                if (obj.GetType() == typeof(AnimationClip)) {
                    if (obj is AnimationClip clip) {
                        if (!dropKeys.Contains(clip)) dropKeys.Add(clip);
                    }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    show = true;
                }
            }
            return show;
        }

        public override bool GUIDragAndDrop(List<TimeflowObject> objects)
        {
            if (dropKeys != null) dropKeys.Clear();

            Undo.RegisterCompleteObjectUndo(Sequencer, "Drag and Drop Animation Clips");
            Vector2 pos = Timeflow.Active.Input.GetMousePosition(Timeflow.Active.Layout.TimeAreaInner);
            //Debug.Log($"{Name} GUIDragAndDrop pos:{pos} GUITrackRect:{GUITrackRect}");
            if (!GUITrackRect.Contains(pos)) {
                return false;
            }
            float startTime = _dropAtTime;// Timeflow.CurrentTime;
            bool handled = false;
            float dur = 0;
            AnimationSequencerKey k = null;
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                if (obj is AnimationClip clip) {
                    if (!Sequencer.AnimationClips.Contains(clip)) {
                        //Debug.Log($"GUIDragAndDrop:{clip.name} atTime:{_dropAtTime}", clip);
                        Sequencer.AnimationClips.Add(clip);
                        Sequencer.RebuildClipCache();
                    }
                    handled = true;

                    Keyframe key = null;
                    if (replaceKey != null) {
                        // Replace existing key
                        key = replaceKey.Key;
                        k = replaceKey;
                        dur = k.Duration;
                        replaceKey = null;
                    }
                    else {
                        // Insert new key
                        key = AddKey(startTime);
                        dur = Mathf.Max(clip.length, 1f);
                        k = Sequencer.SetupKey(key, true);
                        k.Duration = dur;
                    }

                    key.Channel = this;
                    k.Clip = clip;
                    key.KeyString = clip.name;

                    startTime += dur;
                }
            }

            if (handled) return true;
            return base.GUIDragAndDrop(objects);
        }

        public override void GUIDragAndDropEnded()
        {
            dropKeys = null;
            replaceKey = null;
            UpdateTransitionKeys();
        }

        public override void OnDragStart()
        {
            dropKeys = null;
            if (Keys == null || Keys.Count == 0) return;
            foreach (var key in Keys) {
                if (key.CustomKey is AnimationSequencerKey k) {
                    k.OnDragStart();
                }
            }
        }

        public override void OnDragUpdate()
        {
            //Debug.Log($"{Name}.OnDragUpdate()");
            UpdateTransitionKeys();
        }

        public override void OnDragEnded()
        {
            //Debug.Log($"{Name}.OnDragEnded()");
            UpdateTransitionKeys();
        }

        public override void OnDragCancel()
        {
            dropKeys = null;
            //Debug.Log($"{Name}.OnDragCancel()");
            if (Keys == null || Keys.Count == 0) return;
            foreach (var key in Keys) {
                if (key.CustomKey is AnimationSequencerKey k) {
                    k.OnDragCanceled();
                }
            }
            UpdateTransitionKeys();
        }

        protected override void GUIKeyframeLayout(Keyframe key, Keyframe nextKey)
        {
            base.GUIKeyframeLayout(key, nextKey);

            //UpdateKeyframeBounds();

            if (key.CustomKey is AnimationSequencerKey k) {
                float end = key.KeyTimeWorld + k.Duration;
                if (k.IsTransitionOut) {
                    end = k.TransitionToKey.Key.KeyTime;
                }
                key.GUIRect.width = Timeflow.Active.View.PositionOfTime(end, true) - key.GUIRect.x;
            }
        }

        public override void GUIKeyframesDraw(bool isLink, float timeOffset, Rect channelGUIRect)
        {
            base.GUIKeyframesDraw(isLink, timeOffset, channelGUIRect);

            float alpha = isLink ? 0.25f : 1f;
            Color c = GUIColor;
            c.a = 0.1f * alpha;

            Color mixColor = MixColor;
            mixColor.a *= alpha;

            SortKeys(false);

            for (int i = 0; i < Keys.Count; i++) {
                Keyframe k = Keys[i];
                //if (!k.IsKeyEnabled) continue;

                var b = k.CustomKey as AnimationSequencerKey;
                if (b == null) continue;

                Color selectedColor = replaceKey != null && k == replaceKey.Key ? AxonColor.ReplaceKeys : AxonColor.Selected;
                Color selectedKeyColor = replaceKey != null && k == replaceKey.Key ? AxonColor.ReplaceKeys : AxonColor.KeySelected;

                float keyTime = k.KeyTimeWorld - timeOffset;
                float clipEndTime = keyTime + b.ActualEditDuration; // end based on trim
                float naturalEndTime = keyTime + b.Duration; // end based on trim
                float endTime = naturalEndTime;
                float endLoopTime = endTime;
                float nextKeyTime = Timeflow.EndTime;
                bool isSelected = false;

                if (IsEnabled && Timeflow.Active != null && Timeflow.Active.View != null && SelectedKeys != null) {
                    isSelected = SelectedKeys.Contains(k);
                }

                float origEnd = Timeflow.Active.View.PositionOfTime(endTime, true);
                AnimationSequencerKey bn = b.TransitionToKey;
                Keyframe n = null;
                if (bn != null) {
                    n = bn.Key;
                    nextKeyTime = n.KeyTimeWorld - timeOffset;
                    if (endTime > nextKeyTime) {
                        endTime = nextKeyTime;
                    }
                    if (b.Loop || endLoopTime > nextKeyTime) endLoopTime = nextKeyTime; // draw up to next key
                }
                else {
                    if (endTime > Timeflow.EndTime) {
                        endTime = Timeflow.EndTime;
                    }
                    if (b.Loop || endLoopTime > Timeflow.EndTime) endLoopTime = Timeflow.EndTime;
                }

                if (b.Clip == null) {
                    GUI.color = AxonColor.MediumGrey;
                }
                else {
                    GUI.color = c;
                }

                float x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                float x2 = Timeflow.Active.View.PositionOfTime(endTime, true);
                float clipEnd = Timeflow.Active.View.PositionOfTime(clipEndTime, true);
                Rect r = new Rect(x, channelGUIRect.y, x2 - x, channelGUIRect.height);
                GUI.Box(r, GUIContent.none, AxonUI.TrackStyle);

                if (clipEndTime < endTime) {
                    GUI.color = Color.white;// AxonColor.DarkerGreyFaded;
                    x2 = clipEnd;
                    float x3 = Timeflow.Active.View.PositionOfTime(endTime, true);
                    float w = Timeflow.Active.View.PositionOfTime(keyTime + b.ActualEditDuration, true) - x;

                    // For very short clips, just draw a single block
                    if (!b.Loop || w < 20 || b.ActualDuration <= Timeflow.FrameDuration * 2) {
                        if (x2 > x + 50) {
                            r = new Rect(x2, channelGUIRect.y, x3 - x2, channelGUIRect.height);
                            GUI.Box(r, new GUIContent(b.IsEmpty ? "Empty" : !b.Loop ? "Hold" : "Looped"), AxonUI.TrackLoopStyle);
                        }
                    }
                    else {
                        int loopIndex = 1;
                        float t = clipEndTime + b.ActualEditDuration;
                        while (true) {
                            bool end = false;
                            if (t > endTime) {
                                t = endTime;
                                end = true;
                            }
                            // Respect LoopLimit (>0 means maximum number of repeats)
                            if (b.LoopLimit > 0 && loopIndex > b.LoopLimit) {
                                r = new Rect(x2, channelGUIRect.y, x3 - x2, channelGUIRect.height);
                                GUI.Box(r, new GUIContent("Hold"), AxonUI.TrackLoopStyle);

                                break;
                            }

                            x3 = Timeflow.Active.View.PositionOfTime(t, true);
                            w = x3 - x2 + 1;
                            r = new Rect(x2, channelGUIRect.y + 4, w, channelGUIRect.height - 8);
                            if (w > 20) {
                                GUI.Box(r, new GUIContent((w < 40 ? "L" : "Loop ") + loopIndex), AxonUI.TrackLoopStyle);
                            }
                            x2 = x3;
                            t += b.ActualEditDuration;
                            loopIndex++;
                            if (end) break;
                        }
                    }
                }
                if (!b.IsTransitionIn) {
                    float inBlendEnd = keyTime + b.InBlend;
                    GUI.color = AxonColor.Transition; // mixColor faded
                    x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    x2 = Timeflow.Active.View.PositionOfTime(inBlendEnd, true);
                    r = new Rect(x, channelGUIRect.y, x2 - x, channelGUIRect.height);

                    //DrawCurvePolygon(r, b.InBlendCurve, AxonColor.TransitionFaded);
                    DrawCurveLine(r, b.InBlendCurve, isSelected ? selectedColor : AxonColor.Faded);
                }
                else {
                    x2 = Timeflow.Active.View.PositionOfTime(b.TransitionFromKey.Key.KeyEndTimeWorld, true);
                }
                Vector2 v1 = new Vector2(x2, channelGUIRect.y);
                Vector2 v2 = new Vector2(x2, channelGUIRect.y + channelGUIRect.height);

                if (isSelected) {// && !b.IsTransitionIn
                    b.InBlendHandleGUIRect = k.GUIRect;
                    b.InBlendHandleGUIRect.x = (int)x2;
                    b.InBlendHandleGUIRect.width = b.InBlendHandleGUIRect.height = 12;

                    GUIStyle style = AxonUI.KeyframeHoldStyle;
                    GUI.color = k.OverrideGUIColor ? k.GUIColor : GUIColor;
                    GUI.Box(b.InBlendHandleGUIRect, GUIContent.none, style);
                    if (dragKey == b && DragMode == DragModes.InBlend) {
                        style = AxonUI.KeyframeHoldSelectedStyle;
                        GUI.color = selectedColor;
                        GUI.Box(b.InBlendHandleGUIRect, GUIContent.none, style);
                    }
                }
                // StartTime handle and clipped region at head (if StartTime > 0)
                float startClipWorld = keyTime;
                float startClipLocal = b.StartTime;
                if (isSelected) {
                    float xStartHandle = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    b.StartTimeHandleGUIRect = k.GUIRect;
                    b.StartTimeHandleGUIRect.x = (int)xStartHandle + 2;
                    b.StartTimeHandleGUIRect.width = 6;
                    if (GUIRect.Height > 32) {
                        b.StartTimeHandleGUIRect.y += 8;
                        b.StartTimeHandleGUIRect.height = GUIRect.Height - 16;
                    }
                    else {
                        b.StartTimeHandleGUIRect.y += 4;
                        b.StartTimeHandleGUIRect.height = GUIRect.Height - 8;
                    }

                    GUI.color = isSelected ? AxonColor.SelectedFaded : AxonColor.ExtraFaded;
                    GUI.Box(b.StartTimeHandleGUIRect, GUIContent.none, AxonUI.TrackStyle);
                    if (dragKey == b && DragMode == DragModes.StartTime) {
                        GUI.color = selectedColor;
                        GUI.Box(b.StartTimeHandleGUIRect, GUIContent.none, AxonUI.TrackSelectedStyle);
                    }
                }

                // Detect key dragging
                bool isDraggingKey = Timeflow.Input.IsDragging && Timeflow.Input.DragPrimaryKey == b.Key;
                isDraggingKey |= Timeflow.Input.IsDragging && (Timeflow.Input.DragPrimaryKey == b.TransitionToKey?.Key || Timeflow.Input.DragPrimaryKey == b.TransitionFromKey?.Key);
                if (b.TransitionFromKey != null) {
                    isDraggingKey |= Timeflow.Input.IsDragging && Timeflow.Input.DragPrimaryKey == b.TransitionFromKey.Key;
                }
                if (b.TransitionToKey != null) {
                    isDraggingKey |= Timeflow.Input.IsDragging && Timeflow.Input.DragPrimaryKey == b.TransitionToKey.Key;
                }
                //!b.IsTransitionOut && 
                if (b.Clip != null && b.StartTime > 0f && (b.ShowClipRanges || b.ForceShowClipRanges || isDraggingKey)) {
                    // Draw faded region before the key start to indicate clipped portion
                    float clipStartWorld = k.KeyTimeWorld - timeOffset; // start of displayed key
                    float origStartWorld = clipStartWorld - (b.StartTime / Mathf.Max(0.0001f, Mathf.Abs(b.Speed)));
                    float xOrigStart = Timeflow.Active.View.PositionOfTime(origStartWorld, true);
                    float xKeyStart = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    Rect clippedHead = new Rect(xOrigStart, channelGUIRect.y, Mathf.Max(0, xKeyStart - xOrigStart), channelGUIRect.height);
                    GUI.color = AxonColor.ExtraFaded;
                    //GUI.color = isSelected ? selectedColor : AxonColor.ExtraFaded;
                    GUI.Box(clippedHead, GUIContent.none, AxonUI.TrackDisabledStyle);

                    // Marker line at original (untrimmed) start
                    v1 = new Vector2(xOrigStart, channelGUIRect.y);
                    v2 = new Vector2(xOrigStart, channelGUIRect.y + channelGUIRect.height);
                    Handles.color = isSelected ? selectedColor : AxonColor.ExtraFaded;
                    Handles.DrawLine(v1, v2);
                    v1.x += 1;
                    v2.x += 1;
                    Handles.DrawLine(v1, v2);
                    Handles.color = Color.white;
                }

                float xEnd = Timeflow.Active.View.PositionOfTime(endTime, true);
                float outBlendStart = Mathf.Max(keyTime, naturalEndTime - b.OutBlend);
                float xOutStart = Timeflow.Active.View.PositionOfTime(outBlendStart, true);
                float xEndNatural = Timeflow.Active.View.PositionOfTime(naturalEndTime, true);

                if (!b.IsTransitionOut) {
                    // Blend-out region at tail
                    r = new Rect(xOutStart, channelGUIRect.y, xEndNatural - xOutStart, channelGUIRect.height);

                    //DrawCurvePolygon(r, b.OutBlendCurve, AxonColor.TransitionFaded);
                    DrawCurveLine(r, b.OutBlendCurve, isSelected ? selectedColor : AxonColor.Faded);
                }

                // EndTime handle (trim end) at natural end position
                b.OutTimeHandleGUIRect = k.GUIRect;
                b.OutTimeHandleGUIRect.x = (int)xEnd - 8;
                b.OutTimeHandleGUIRect.width = 6;
                if (GUIRect.Height > 32) {
                    b.OutTimeHandleGUIRect.y += 8;
                    b.OutTimeHandleGUIRect.height = GUIRect.Height - 16;
                }
                else {
                    b.OutTimeHandleGUIRect.y += 4;
                    b.OutTimeHandleGUIRect.height = GUIRect.Height - 8;
                }

                if (isSelected) {// && !b.IsTransitionOut
                    b.OutBlendHandleGUIRect = k.GUIRect;
                    if (b.IsTransitionOut) {
                        b.OutBlendHandleGUIRect.x = (int)xEndNatural - 10;
                    }
                    else {
                        b.OutBlendHandleGUIRect.x = (int)xOutStart - 10;
                    }
                    b.OutBlendHandleGUIRect.y -= 1;
                    b.OutBlendHandleGUIRect.width = b.OutBlendHandleGUIRect.height = 12;

                    GUI.color = k.OverrideGUIColor ? k.GUIColor : GUIColor;
                    GUI.Box(b.OutBlendHandleGUIRect, GUIContent.none, AxonUI.KeyframeHoldStyle);
                    if (dragKey == b && DragMode == DragModes.OutBlend) {
                        GUI.color = selectedColor;
                        GUI.Box(b.OutBlendHandleGUIRect, GUIContent.none, AxonUI.KeyframeHoldSelectedStyle);
                    }
                }

                GUI.color = isSelected ? AxonColor.SelectedFaded : AxonColor.ExtraFaded;
                GUI.Box(b.OutTimeHandleGUIRect, GUIContent.none, AxonUI.TrackStyle);
                if (dragKey == b && DragMode == DragModes.OutTime) {
                    GUI.color = selectedColor;
                    GUI.Box(b.OutTimeHandleGUIRect, GUIContent.none, AxonUI.TrackSelectedStyle);
                }

                if (k.IsKeyEnabled && IsEnabled) {
                    b.LoopGUIRect.x = (int)Mathf.Min(clipEnd, xEnd) - 24;// (int)xEnd - 24;
                    b.LoopGUIRect.y = b.OutTimeHandleGUIRect.y - 2;
                    b.LoopGUIRect.width = 16;
                    b.LoopGUIRect.height = 16;
                    GUI.color = Color.white;
                    if (GUI.Button(b.LoopGUIRect, AxonUI.ClipLoopLabel, b.Loop ? AxonUI.LoopOnWhiteStyle : AxonUI.LoopOffStyle)) {
                        b.Loop = !b.Loop;
                    }
                }

                // Draw small arrow indicator when clipped (EndTime < full clip length)
                if (b.Clip != null && (b.ShowClipRanges || b.ForceShowClipRanges || isDraggingKey)) {
                    bool isClipped = b.Duration < b.Clip.length; // early end
                    if (isClipped) {
                        if (!b.IsTransitionOut) {
                            // Draw a small left-pointing chevron near the handle
                            float cy = k.GUIRect.center.y;
                            float arrowX = xEndNatural + 6; // right of the vertical line
                            Vector2 p1 = new Vector2(arrowX, cy);
                            Vector2 p2 = new Vector2(arrowX - 6, cy - 4);
                            Vector2 p3 = new Vector2(arrowX - 6, cy + 4);

                            Handles.color = isSelected ? selectedColor : AxonColor.Faded;
                            Handles.DrawAAPolyLine(3f, new Vector3[] { p2, p1, p3 });
                            Handles.color = Color.white;
                        }
                        if (b.StartTime > 0) {
                            // Draw a small right-pointing chevron near the start
                            float cy = k.GUIRect.center.y;
                            float arrowX = Timeflow.Active.View.PositionOfTime(keyTime + 0.02f, true) - 14; // left of the key start
                            Vector2 p1 = new Vector2(arrowX, cy);
                            Vector2 p2 = new Vector2(arrowX + 6, cy - 4);
                            Vector2 p3 = new Vector2(arrowX + 6, cy + 4);
                            Handles.color = isSelected ? selectedColor : AxonColor.Faded;
                            Handles.DrawAAPolyLine(3f, new Vector3[] { p2, p1, p3 });
                            Handles.color = Color.white;
                        }
                    }

                    r = k.GUIRect;
                    r.x = xEnd;
                    float emptyEndTime = nextKeyTime <= 0f ? clipEndTime : Mathf.Min(clipEndTime, nextKeyTime);
                    r.width = Timeflow.Active.View.PositionOfTime(emptyEndTime, true) - r.x;
                    GUI.color = AxonColor.ExtraFaded;
                    GUI.Box(r, GUIContent.none, AxonUI.TrackDisabledStyle);

                    bool inside = clipEndTime < endTime;
                    float offset = inside ? 2 : 0;
                    v1 = new Vector2(inside ? xEndNatural : clipEnd, channelGUIRect.y + offset);
                    v2 = new Vector2(inside ? xEndNatural : clipEnd, channelGUIRect.y + channelGUIRect.height - offset);

                    Handles.color = isSelected ? selectedColor : AxonColor.ExtraFaded;
                    Handles.DrawLine(v1, v2);
                    //v1.x += 1;
                    //v2.x += 1;
                    //Handles.DrawLine(v1, v2);
                    Handles.color = Color.white;
                }

                if (b.IsTransitionOut) {
                    r = new Rect(xEnd, k.GUIRect.y, origEnd - xEnd, k.GUIRect.height);

                    Rect r2 = r;
                    r2.y = r.yMax - 2;
                    r2.height = -(r.height - 4);
                    DrawCurveLine(r2, b.TransitionToKey.InBlendCurve, isSelected ? selectedColor : AxonColor.Faded);

                    bool tselected = SelectedKeys.Contains(b.TransitionToKey.Key);
                    DrawCurveLine(r, b.TransitionToKey.InBlendCurve, tselected ? selectedColor : AxonColor.Faded);
                }
            }
            GUI.color = AxonColor.Default;

            if (dropKeys != null && dropKeys.Count > 0) {
                float startTime = _dropAtTime;
                if (replaceKey != null && replaceKey.Key != null) {
                    startTime = replaceKey.Key.KeyTime;
                }
                for (int d = 0; d < dropKeys.Count; d++) {
                    float dur = Mathf.Max(dropKeys[d].length, 1f);
                    float x = Timeflow.Active.View.PositionOfTime(startTime, true);
                    float e = Timeflow.Active.View.PositionOfTime(startTime + dur, true);
                    Rect r = new Rect(x, channelGUIRect.y, e - x, channelGUIRect.height);
                    GUI.color = replaceKey == null ? AxonColor.RelatedKeys : AxonColor.ReplaceKeys;
                    GUI.Box(r, new GUIContent(dropKeys[d].name), AxonUI.TrackSelectedStyle);
                    GUI.color = AxonColor.Default;

                    startTime += dur;
                }
            }
        }

        private void DrawCurveLine(GUIRect rect, AnimationCurve curve, Color color)
        {
            if (curve == null || curve.keys == null || curve.keys.Length == 0) return;
            Color c = color;
            c.a = 0.5f;
            Handles.color = c;
            rect.y += 2;
            rect.height -= 4;
            if (curvePoints == null) curvePoints = new Vector3[64];
            for (int i = 0; i < curvePoints.Length; i++) {
                float t = (float)i / (float)curvePoints.Length;
                float v = Mathf.Clamp(curve.Evaluate(t), 0f, 1f);
                curvePoints[i] = new Vector3(rect.x + (rect.width * t), rect.yMax - (v * rect.height), 0f);
            }
            Handles.color = color;
            Handles.DrawAAPolyLine(3f, curvePoints);
            Handles.color = Color.white;
        }

        private void DrawCurvePolygon(GUIRect rect, AnimationCurve curve, Color color)
        {
            if (curve == null || curve.keys == null || curve.keys.Length == 0) return;
            Color c = color;
            c.a = 0.5f;
            Handles.color = c;
            rect.y += 2;
            rect.height -= 4;
            int length = curvePoints.Length - 3;
            if (curvePoints == null) curvePoints = new Vector3[64];
            curvePoints[0] = new Vector3(rect.xMax, rect.y, 0f);
            curvePoints[1] = new Vector3(rect.x, rect.y, 0f);
            curvePoints[2] = new Vector3(rect.x, rect.yMax, 0f);
            for (int i = 3; i < curvePoints.Length; i++) {
                float t = (float)i / (float)(length + 1);
                float v = Mathf.Clamp(curve.Evaluate(t), 0f, 1f);
                curvePoints[i] = new Vector3(rect.x + (rect.width * t), rect.yMax - (v * rect.height), 0f);
            }
            curvePoints[curvePoints.Length - 1] = curvePoints[0];
            Handles.color = color;
            Handles.DrawAAConvexPolygon(curvePoints);
        }

        public static void GUIMenu_RenumberTracks(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects == null) return;

            foreach (TimeflowObject obj in objects) {
                obj.BehaviorsEnabled = true;
                if (obj.TryGetComponent(out AnimationSequencer sequencer))
                    sequencer.RenumberChannels();
            }
            Timeflow.Active.Refresh(true);
        }

        public static void GUIMenu_AddTrack(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects == null) return;

            foreach (TimeflowObject obj in objects) {
                obj.BehaviorsEnabled = true;

                if (!obj.TryGetComponent(out AnimationSequencer sequencer)) {
                    sequencer = Undo.AddComponent<AnimationSequencer>(obj.gameObject);
                    if (sequencer != null) {
                        sequencer.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(sequencer.Channels[0]);
                    }
                }
                else {
                    var ch = sequencer.AddChannel();
                    ch.GUIHeight = sequencer.Channels[0].GUIHeight;
                }
            }
            Timeflow.Active.Refresh(true);
        }

        public override void GUIChannelValues()
        {
            float time = CurrentTime;

            float labelWidth = AxonGUI.LabelWidth;
            AxonGUI.SetLabelWidth(5);

            Rect rect = new Rect(GUIRect) { x = 8, height = 16 };
            rect = GUIChannelValuesLinkMenu(rect);

            string label = IsLinked ? Link.GetModeLabel() : "";

            float w = rect.width;
            rect.width = 10;
            GUI.Label(rect, label);

            rect.x += rect.width;
            rect.width = w - rect.width;
            rect.y = GUIRect.y + (GUIRect.height - rect.height) / 2;

            EditorGUI.BeginChangeCheck();

            Keyframe key = GetKeyAtTime(time);
            AnimationSequencerKey sk = Sequencer.SetupKey(key);
            if (sk == null) return;

            string value = CurrentAnimation;
            if (string.IsNullOrEmpty(value)) value = "Empty";

            string newValue = AxonGUI.FieldPopupString(Sequencer, rect, null, value, Sequencer.AnimationNames);
            if (value != newValue) {
                if (newValue == "Empty") newValue = null;
                CurrentAnimation = newValue;

                if (key != null) {
                    key.KeyString = newValue;
                    // Keep the key's clip in sync with the selection
                    var kk = key.CustomKey as AnimationSequencerKey ?? Sequencer.SetupKey(key);
                    if (kk != null) {
                        kk.Clip = string.IsNullOrEmpty(newValue) ? null : Sequencer.GetClipByName(newValue);
                        kk.OnValueChanged();
                    }
                }
            }

            rect.x += rect.width;

            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) {
                if (key == null) {
                    Keyframe newKey = SetKey(time);
                    if (newKey != null) {
                        newKey.KeyString = CurrentAnimation;
                        var kk = newKey.CustomKey as AnimationSequencerKey ?? Sequencer.SetupKey(newKey);
                        if (kk != null) {
                            kk.Clip = string.IsNullOrEmpty(CurrentAnimation) ? null : Sequencer.GetClipByName(CurrentAnimation);
                            kk.OnValueChanged();
                        }
                    }
                }
            }
        }

        public override void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
            base.GUIInfo(selectedChannels);

            AxonGUI.BeginBoxPadded();
            AxonGUI.BeginHorizontal();
            if (Sequencer != null && Index < 10) {
                float w = Sequencer.GetMappedChannelWeight(Index, Weight);
                float weight = AxonGUI.FieldSliderInline(Sequencer, "Weight", w, 0f, 1f);
                if (weight != w) {
                    Sequencer.SetMappedChannelWeight(Index, weight);
                }
            }
            else {
                Weight = AxonGUI.FieldSliderInline(Sequencer, "Weight", Weight, 0f, 1f);
            }
            bool add = AxonGUI.FieldToggleInline(Sequencer, "Additive", IsAdditive);
            if (add != IsAdditive) {
                IsAdditive = add;
                // Rebuild connections so SetLayerAdditive uses the new value
                //Sequencer?.RebuildLayerMixerFromChannels();
            }
            AxonGUI.EndHorizontal();

            // Optional per-channel AvatarMask selector
            AxonGUI.BeginHorizontal();
            var newMask = (AvatarMask)AxonGUI.FieldObjectInline(Sequencer, "Mask", Mask, typeof(AvatarMask), false);
            if (newMask != Mask) {
                Mask = newMask;
                Sequencer.ForceRefresh();
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBoxPadded();
            AxonGUI.Space();

        }

        public override void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            base.GUIInfoValues(selectedKeys, tracksOnly);
            Sequencer.GUIInfoValues(selectedKeys, tracksOnly);
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Animation Sequencer Track"), false, GUIMenu_AddTrack, null);
            menu.AddItem(new GUIContent("Renumber Animation Sequencer Tracks"), false, GUIMenu_RenumberTracks, null);
        }

        public override void GUISelectedKeysContextMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Animation Sequencer/Edit Animation Clip"), false, EditKeyAnimationClip);
            menu.AddItem(new GUIContent("Animation Sequencer/Select Animation Clip"), false, SelectKeyAnimationClip);
            menu.AddItem(new GUIContent("Animation Sequencer/Reset Selected Keys"), false, ResetSelectedKeys);
            menu.AddItem(new GUIContent("Animation Sequencer/Reset Blend In Curve"), false, ResetSelectedKeysBlendInCurve);
            menu.AddItem(new GUIContent("Animation Sequencer/Reset Blend Out Curve"), false, ResetSelectedKeysBlendOutCurve);
        }

        public void SelectKeyAnimationClip()
        {
            foreach (Keyframe k in SelectedKeys) {
                if (k.CustomKey is AnimationSequencerKey ak) {
                    if (ak.Clip != null) {
                        EditorGUIUtility.PingObject(ak.Clip);
                        Selection.activeObject = ak.Clip;
                        break;
                    }
                }
            }
        }

        public void EditKeyAnimationClip()
        {
            foreach (Keyframe k in SelectedKeys) {
                if (k.CustomKey is AnimationSequencerKey ak) {
                    if (ak.Clip != null) {
                        AnimationUtil.OpenAnimationClipInEditor(ak.Clip);
                        break;
                    }
                }
            }
        }

        public void ResetSelectedKeys()
        {
            Undo.RegisterCompleteObjectUndo(Sequencer, "Reset Selected Keys");
            foreach (Keyframe k in SelectedKeys) {
                if (k.CustomKey is AnimationSequencerKey ak) {
                    ak.Reset();
                }
            }
        }

        public void ResetSelectedKeysBlendInCurve()
        {
            Undo.RegisterCompleteObjectUndo(Sequencer, "Reset Blend In Curve");
            foreach (Keyframe k in SelectedKeys) {
                if (k.CustomKey is AnimationSequencerKey ak) {
                    ak.ResetBlendInCurve();
                }
            }
        }

        public void ResetSelectedKeysBlendOutCurve()
        {
            Undo.RegisterCompleteObjectUndo(Sequencer, "Reset Blend Out Curve");
            foreach (Keyframe k in SelectedKeys) {
                if (k.CustomKey is AnimationSequencerKey ak) {
                    ak.ResetBlendOutCurve();
                }
            }
        }
    }
} //AxonGenesis

#endif
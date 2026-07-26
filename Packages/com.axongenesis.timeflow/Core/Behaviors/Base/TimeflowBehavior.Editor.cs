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
    /// <summary>
    /// Implements custom Timeflow GUI and menu options.
    /// </summary>
    public partial class TimeflowBehavior : AxonGenesisBehavior
    {
        #region PUBLIC

        [HideInInspector]
        public bool EditorShowTime;

        [HideInInspector]
        public bool EditorShowChannels = true;

        [NonSerialized]
        public bool IsGraphLocked;

        #endregion

        #region PRIVATE

        protected float _dragTime;
        protected int _guiID = -1;

        #endregion

        #region ACCESSORS

        public override bool IsSelected {
            get {
                bool sel = false;

                if (Channels != null) {
                    foreach (TimeflowChannel ch in Channels) {
                        if (ch.IsSelected) {
                            sel = true;
                            break;
                        }
                    }
                }

                return sel;
            }
            set {
                if (Channels != null && Channels.Count > 0) {
                    Channels[0].IsSelected = value;
                }
            }
        }

        public virtual Texture2D Icon => AxonUI.Icons.Keyframer;

        public override Color GUIColor {
            get {
                if (Channels != null && Channels.Count > 0) {
                    return Channels[0].GUIColor;
                }
                // No channels means something is wrong. The first channel is always the track channel.
                if (Timeflow.Active != null && Timeflow.Active.View != null) {
                    Timeflow.Active.View.NeedsRefresh = true;
                }
                return Color.black;
            }
            set {
                if (Channels != null && Channels.Count > 0) {
                    Channels[0].GUIColor = value;
                }
            }
        }

        public bool GUIColorAuto {
            get {
                if (Channels != null && Channels.Count > 0) {
                    return Channels[0].GUIColorAuto;
                }
                return false;
            }
            set {
                if (Channels != null && Channels.Count > 0) {
                    Channels[0].GUIColorAuto = value;
                }
            }
        }

        #endregion

        #region EDITOR CALLBACKS

        /// <summary>
        /// Behaviors should override this to implement any special setup when a new instance is created.
        /// </summary>
        public virtual void OnNewInstance()
        {
            CanDragTimeOffset = TimeflowPreferences.Current.DefaultCanDragTimeOffset;
        }

        public virtual void OnNewGUIColor()
        {
            // Override to customize initialization color for specific channels
        }

        public virtual void OnKeyChange()
        {
            // Override to perform addtional setup after keyframe changes
        }

        public virtual void OnTrackChange()
        {
            // Override to perform addtional setup after keyframe changes
            CurrentTime = GetTime();
        }

        /// <summary>
        /// This callback is registered through the main Timeflow instance and gives all behaviors a chance
        /// to update after changes have been made to the hierarchy in the editor. This might result in
        /// reparenting associated items or other setup required.
        /// </summary>
        public virtual void OnHierarchyChange()
        {
            /// The base behavior does nothting
        }

        public override void OnPropertyChanged(Property property, Property.PropertyTypes originalType, int originalAttribute)
        {
            if (property == null) return;
            //if (DebugEnabled) Debug.Log(name + ".OnPropertyChanged:" + property.Name);
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch.ToProperty == property) {
                        ch.OnPropertyChanged(originalType, originalAttribute);
                    }
                }

                OnVectorChanged();
            }
        }

        public override void ResetName()
        {
            if (Channels != null && Channels.Count > 0) {
                foreach (TimeflowChannel ch in Channels) {
                    if (ch.ToProperty != null) ch.Name = ch.ToProperty.GetNameAndAttribute("(Unassigned)", true, true, false);
                }
            }
        }

        #endregion

        #region CHANNELS

        #endregion

        #region TIME EDIT

        public virtual void InsertTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in Channels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Insert Time", true);
                    ch.InsertTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public virtual void DuplicateTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in Channels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Duplicate Time in Work Area", true);
                    ch.DuplicateTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public virtual void DeleteTime(float start, float end, bool isLocalTime, bool isGlobal)
        {
            foreach (TimeflowChannel ch in Channels) {
                if (isGlobal || !ch.IsLocked) {
                    UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                    ch.DeleteTimeRange(start, end, isLocalTime, isGlobal);
                }
            }
        }

        public virtual void ClearTime(float start, float end, bool isLocalTime, bool isGlobal, TimeflowView.SelectionModes mode = TimeflowView.SelectionModes.Any)
        {
            foreach (TimeflowChannel ch in Channels) {
                if (isGlobal || !ch.IsLocked) {
                    bool canSelect = true;
                    if (!isGlobal) {
                        if (mode == TimeflowView.SelectionModes.KeyframesOnly) {
                            canSelect = !ch.IsTrack;
                        }
                        else
                        if (mode == TimeflowView.SelectionModes.TracksOnly) {
                            canSelect = ch.IsTrack;
                        }
                    }
                    if (canSelect) {
                        UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                        ch.ClearTimeRange(start, end, isLocalTime, isGlobal);
                    }
                }
            }
        }

        public virtual void ScaleTime(float scale)
        {
            if (Channels != null) {
                UndoUtil.Undo(this, "Scale Time");
                foreach (TimeflowChannel ch in Channels) {
                    if (ch != null) {
                        ch.ScaleTime(scale);
                    }
                }
            }
        }

        #endregion

        #region DRAG OPS

        public virtual void OnDragStart()
        {
            _dragTime = TimeOffsetWorld;
        }

        public virtual void OnDragUpdate()
        {
            //if (Timeflow == null) return;
            DoUpdate();
        }

        public virtual void OnDragEnded()
        {
            //if (Timeflow == null) return;
            DoUpdate();
        }

        public virtual void OnDragCancel()
        {
        }

        public virtual float DragTimeOffset(float offset, bool canSnap)
        {
            float time = _dragTime + offset;
            TimeOffsetWorld = time;
            float offsetTime = TimeOffsetWorld - _dragTime;
            if (canSnap && Timeflow.Active != null) {
                TimeOffsetWorld = Timeflow.Active.View.SnapTime(TimeOffsetWorld);
                offsetTime = Timeflow.Active.View.SnapTime(offsetTime);
                //Debug.Log($"{name}.DragTimeOffset:{offsetTime} TimeOffsetWorld:{TimeOffsetWorld} canSnap:{canSnap}");
            }
            return offsetTime; // return an updated offset
        }

        #endregion

        #region PRESETS

        public override void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            base.OnBeforeSavePreset(ref items);
            if (items == null || items.Count == 0) return;
            List<ComponentPresetListItem> toremove = new List<ComponentPresetListItem>();
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "CurrentTime" || item.Name == "IsGraphLocked" || item.Name.Contains("Channel ID")) {
                    toremove.Add(item);
                }
                if (item.Name == "Time Offset" || item.Name == "Time Scale" || item.Name.StartsWith("Update")) {
                    item.IsSelected = false;
                }
            }

            if (toremove.Count > 0) {
                foreach (ComponentPresetListItem item in toremove) {
                    items.Remove(item);
                }
            }
        }

        #endregion

        #region GUI

        public virtual void GUIGraph(Rect rect) { }

        public virtual void GUIGraphFit(bool init, bool selectedOnly) { }

        public virtual void GUIChannelOverlay() { }

        public virtual void ViewInspector() { }

        public virtual bool HasChannelsToDraw()
        {
            if (Channels == null || Channels.Count == 0) return false;

            bool draw = false;
            foreach (TimeflowChannel ch in Channels) {
                if (ch.IsGraphLocked) {
                    draw = true;
                    break;
                }
            }
            return draw;
        }

        public override void DrawGizmos()
        {
            if (Channels != null) {
                // Draw the motion path by sampling the independent XYZ axis
                TimeflowChannel xPos = null;
                TimeflowChannel yPos = null;
                TimeflowChannel zPos = null;
                TimeflowChannel combined = null;

                bool xLocal = true;
                bool yLocal = true;
                bool zLocal = true;

                bool startSet = false;
                bool endSet = false;
                Vector2 tempRange = Vector2.zero;
                Vector2 timeRange = Vector2.zero;

                Vector3 localOffset = Vector3.zero;
                if (gameObject.transform.parent != null) {
                    localOffset = gameObject.transform.parent.position;
                }

                bool hasPos = false;

                foreach (TimeflowChannel channel in Channels) {
                    if (channel.IsEnabled && channel.DrawPath && channel.HasProperty) {
                        if (channel.ToProperty.Name.Equals("Position")) {
                            Handles.color = GUIColor;
                            hasPos = true;
                            if (channel.ToProperty.IsCombinedValue) {
                                combined = channel;
                                xLocal = false;
                            }
                            else
                            if (channel.ToProperty.Attribute == 0) {
                                xPos = channel;
                                xLocal = false;
                            }
                            else
                            if (channel.ToProperty.Attribute == 1) {
                                yPos = channel;
                                yLocal = false;
                            }
                            else
                            if (channel.ToProperty.Attribute == 2) {
                                zPos = channel;
                                zLocal = false;
                            }
                        }
                        else
                        if (channel.ToProperty.Name.Equals("Local Position")) {
                            Handles.color = GUIColor;
                            hasPos = true;
                            if (channel.ToProperty.IsCombinedValue) {
                                combined = channel;
                                xLocal = true;
                            }
                            else
                            if (channel.ToProperty.Attribute == 0) {
                                xPos = channel;
                                xLocal = true;
                            }
                            else
                            if (channel.ToProperty.Attribute == 1) {
                                yPos = channel;
                                yLocal = true;
                            }
                            else
                            if (channel.ToProperty.Attribute == 2) {
                                zPos = channel;
                                zLocal = true;
                            }
                            else {

                            }
                        }
                        if (hasPos) {
                            tempRange = channel.GetKeyTimeRange();
                            if (!startSet || tempRange.x < timeRange.x) {
                                startSet = true;
                                timeRange.x = tempRange.x;
                            }
                            if (!endSet || tempRange.y > timeRange.y) {
                                endSet = true;
                                timeRange.y = tempRange.y;
                            }
                        }
                    }
                }
                if (hasPos) {
                    int resolution = 100;
                    float inc = (timeRange.y - timeRange.x) / (float)resolution;

                    Vector3[] path = new Vector3[resolution];

                    float t = tempRange.x;
                    for (int i = 0; i < resolution; i++) {
                        if (combined != null) {
                            path[i] = combined.InterpolateVector3(t, false, true);
                            if (xLocal) path[i] += localOffset;
                        }
                        else {
                            if (xPos != null) {
                                path[i].x = xPos.InterpolateValue(t, false, true);
                                if (xLocal) path[i].x += localOffset.x;
                            }
                            else {
                                path[i].x = 0f;
                            }
                            if (yPos != null) {
                                path[i].y = yPos.InterpolateValue(t, false, true);
                                if (yLocal) path[i].y += localOffset.y;
                            }
                            else {
                                path[i].y = 0f;
                            }
                            if (zPos != null) {
                                path[i].z = zPos.InterpolateValue(t, false, true);
                                if (zLocal) path[i].z += localOffset.z;
                            }
                            else {
                                path[i].z = 0f;
                            }
                        }
                        t += inc;
                    }
                    Handles.DrawAAPolyLine(3f, path);
                }
            }
        }

        #endregion

    }

}//AxonGenesis
#endif

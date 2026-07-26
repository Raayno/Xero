// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This channel type is used by behaviors that output read-only values which are calculated, not
    /// keyframed. A behavior may create and use any number of channels for any data type.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "DataChannel")]
    public class DataChannel : TimeflowChannel
    {
        [NonSerialized]
        public TimeflowBehavior DataParent;

        public DataChannel(TimeflowBehavior parent) : base(parent)
        {
            DataParent = parent;

#if UNITY_EDITOR
            GUICanDraw = false;
#endif
        }

        public override void SetParent(TimeflowBehavior parent)
        {
            base.SetParent(parent);
            DataParent = parent as TimeflowBehavior;
            ToProperty.Owner = DataParent;
        }

        /// <summary>
        /// Returns false since there are no keyframes generated on a DataChannel
        /// </summary>
        public override bool SupportsKeyframes {
            get {
                return false;
            }
        }

        public override bool IsLoopSupported {
            get {
                return false;
            }
        }

        public override void OnSetup(TimeflowBehavior parent)
        {
            if (ToProperty == null) ToProperty = new Property();
            ToProperty.IsDataOnly = true;
            base.OnSetup(parent);
        }

        #region INTERPOLATION

        /// <summary>
        /// Overrided to disable
        /// </summary>
        public override bool CanInterpolate {
            get {
                if (_Interpolation != Interpolations.None) _Interpolation = Interpolations.None;
                return false;
            }
        }

        public override float InterpolateValue(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedValue(time)) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateValue:" + time + " cached:" + CurrentValue);
                return CurrentValue;
            }

            float value = 0;
            if (DataParent != null) {
                value = DataParent.InterpolateValue(this, time, apply);
                if (IsLinkEnabled) {
                    value = Link.GetValue(value, WorldTime(intime, true));
                }
            }
            SetCurrentValue(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateValue:" + time + " value:" + CurrentValue);
            return value;
        }

        public override Vector2 InterpolateVector2(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedVector(time)) return CurrentVector;

            Vector2 value = Vector2.zero;
            if (DataParent != null) {
                value = DataParent.InterpolateVector2(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetVector2(value, WorldTime(intime, true));
                }
            }
            SetCurrentVector(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector2:" + time + " value:" + CurrentVector);
            return value;
        }

        public override Vector3 InterpolateVector3(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedVector(time)) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " Cached:" + CurrentVector);
                return CurrentVector;
            }
            else {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + intime + " CurrentVector:" + CurrentVector);
            }

            Vector3 value = Vector3.zero;
            if (DataParent != null) {
                value = DataParent.InterpolateVector3(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetVector3(value, WorldTime(intime, true));
                }
            }
            SetCurrentVector(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " value:" + CurrentVector);
            return value;
        }

        public override Vector4 InterpolateVector4(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedVector(time)) return CurrentVector;

            Vector4 value = Vector4.zero;
            if (DataParent != null) {
                value = DataParent.InterpolateVector4(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetVector4(value, WorldTime(intime, true));
                }
            }
            SetCurrentVector(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector4:" + time + " value:" + CurrentVector);
            return value;
        }

        public override Color InterpolateColor(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedVector(time)) return CurrentColor;

            Color value = Color.black;
            if (DataParent != null) {
                value = DataParent.InterpolateColor(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetColor(value, WorldTime(intime, true));
                }
            }
            SetCurrentColor(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateColor:" + time + " value:" + CurrentColor);
            return value;
        }

        public override string InterpolateString(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedValue(time)) return cache_StringValue;

            string value = "";
            if (DataParent != null) {
                value = DataParent.InterpolateString(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetStringValue(value, WorldTime(intime, true));
                }
            }
            SetCurrentString(value, time, apply);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateString:" + time + " value:" + CurrentString);
            return value;
        }

        public override Component InterpolateComponent(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (IsCachedValue(time)) return CurrentComponent;

            Component value = null;
            if (DataParent != null) {
                value = DataParent.InterpolateComponent(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetComponentValue(value, WorldTime(intime, true));
                }
            }
            SetCurrentComponent(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateComponent:" + time + " value:" +
            //(CurrentComponent == null ? "NULL" : CurrentComponent.name + "(" + CurrentComponent.GetType() + ")"));
            return value;
        }

        public override GameObject InterpolateGameObject(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedValue(time)) return CurrentGameObject;

            GameObject value = null;
            if (DataParent != null) {
                value = DataParent.InterpolateGameObject(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetGameObjectValue(value, WorldTime(intime, true));
                }
            }
            SetCurrentGameObject(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateGameObject:" + time + " value:" +
            //(CurrentGameObject == null ? "NULL" : CurrentGameObject.name));
            return value;
        }

        public override UnityEngine.Object InterpolateObject(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);

            if (IsCachedValue(time)) return CurrentObject;

            UnityEngine.Object value = null;
            if (DataParent != null) {
                value = DataParent.InterpolateObject(this, time, apply);

                if (IsLinkEnabled) {
                    value = Link.GetObjectValue(value, WorldTime(intime, true));
                }
            }
            SetCurrentObject(value, time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateObject:" + time + " value:" +
            //(CurrentObject == null ? "NULL" : CurrentObject.name));
            return value;
        }

        #endregion

#if UNITY_EDITOR

        /// <summary>
        /// Data channels are named by their parent behavior since the names are usually custom and not
        /// derrived from property values as with standard channels.
        /// </summary>
        public override void ResetName()
        {
            if (Behavior != null) {
                Behavior.ResetName();
            }
        }

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUIHierarchyControls()
        {
            if (IsHidden || !IsSelectable) return;
            GUI.color = AxonColor.Default;
            GUIChannelLink();
            GUIExpandRegion();
        }

        public override void GUIKeyframes()
        {
            if ((GUICanDraw || IsGraphLocked) && DataParent != null && DataParent.Timeflow != null) {
                Handles.BeginGUI();
                Vector3[] points = null;

                //TODO: Optimize so the curve is only redrawn when needed
                float viewTotalTime = Timeflow.Active.View.ViewEndTime - Timeflow.Active.View.ViewStartTime;
                if (viewTotalTime <= 0) return;

                float startTime = 0f;
                float endTime = Timeflow.EndTime;

                bool drawMain = true;
                if (startTime > Timeflow.Active.View.ViewEndTime || endTime < Timeflow.Active.View.ViewStartTime) {
                    // The track area is out of view
                    drawMain = false;
                }

                float startPos = 0f;
                float endPos = GUIRect.width;
                float height = GUIRect.height - 4f;
                float offsety = GUIRect.y + 2f;

                if (Timeflow.Active.View.IsGraphMode) {
                    float min = Mathf.Min(DataParent.MinValue, DataParent.MaxValue);
                    float max = Mathf.Max(DataParent.MinValue, DataParent.MaxValue);
                    float y = Timeflow.Active.View.PositionOfValue(min, true);
                    float h = Timeflow.Active.View.PositionOfValue(max, true);

                    offsety = y;
                    height = h - y;
                }

                Rect track = new Rect(GUIRect);
                if (startTime < Timeflow.Active.View.ViewStartTime) {
                    track.x = -10f; // Extend off screen
                    startTime = Timeflow.Active.View.ViewStartTime;
                }
                else {
                    startPos = track.x = ((startTime - Timeflow.Active.View.ViewStartTime) / viewTotalTime) * GUIRect.width;
                }

                if (endTime > Timeflow.Active.View.ViewEndTime) {
                    track.width = GUIRect.width + 10f; // Extend off screen
                    endTime = Timeflow.Active.View.ViewEndTime;
                }
                else {
                    endPos = track.width = (((endTime - Timeflow.Active.View.ViewStartTime) / viewTotalTime) * GUIRect.width) - track.x;
                }
                endPos = Timeflow.Active.View.PositionOfTime(endTime, true);
                float trackTotalTime = endTime - startTime;
                if (trackTotalTime == 0) {
                    // nothing to do
                }
                else
                if (drawMain && ToProperty != null && !ToProperty.IsColor) {
                    // Draws a curve to fit the view of the timeline or graph
                    // Note that color gradients are drawn by GUIDrawColorGradient

                    float min = 0;
                    float max = 0;
                    bool isFirst = true;
                    float width = endPos - startPos;

                    int count = 64;
                    if (width > 128) count = 128;
                    if (width > 256) count = 256;
                    if (width > 512) count = 512;
                    if (width > 768) count = 768;
                    if (width > 1024) count = 1024;

                    float timeOffset = -TimeOffsetWorld;

                    //TODO: Cache this so it doesn't continually rebuild
                    points = new Vector3[count];
                    for (int i = 0; i < count; i++) {
                        float p = (float)i / (float)count;
                        float t = (startTime + (p * trackTotalTime)) + timeOffset;

                        points[i].x = startPos + (p * width);
                        points[i].y = DataParent.InterpolateValue(this, t, false);
                        points[i].z = 0f;

                        if (isFirst) {
                            isFirst = false;
                            min = max = points[i].y;
                        }
                        else {
                            if (min > points[i].y) min = points[i].y;
                            if (max < points[i].y) max = points[i].y;
                        }
                    }
                    if (min == max) max = min + 1f;
                    float size = max - min;
                    for (int i = 0; i < count; i++) {
                        float n = 1f - ((points[i].y - min) / size);    // Normalize and invert (view is -y up)
                        points[i].y = offsety + (n * height);           // Fit to view
                    }

                    Handles.color = GUIColor;
                    if (!IsEnabled) Handles.color = ColorUtil.SetAlpha(Handles.color, 0.2f);
                    else
                    if (!IsSelected && Timeflow.View.IsGraphSolo) Handles.color = ColorUtil.SetAlpha(Handles.color, 0.5f);
                    Handles.DrawAAPolyLine(3f, points);
                }

                Handles.EndGUI();
            }
        }

        public override void GUIGraphPass2()
        {
            GUIKeyframes();
            base.GUIGraphPass2();
        }

        public override void GUIGraphFit(bool init, bool selectedOnly)
        {
            if (DataParent.Timeflow != null) {
                if (init) {
                    Timeflow.Active.View.GraphMinValue = Mathf.Min(DataParent.MinValue, DataParent.MaxValue);
                    Timeflow.Active.View.GraphMaxValue = Mathf.Max(DataParent.MinValue, DataParent.MaxValue);
                }
                else {
                    Timeflow.Active.View.GraphMinValue = Mathf.Min(Timeflow.Active.View.GraphMinValue, Mathf.Min(DataParent.MinValue, DataParent.MaxValue));
                    Timeflow.Active.View.GraphMaxValue = Mathf.Max(Timeflow.Active.View.GraphMaxValue, Mathf.Max(DataParent.MinValue, DataParent.MaxValue));
                }
            }
        }
#endif
    }

}//AxonGenesis
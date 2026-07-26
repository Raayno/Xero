// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using AxonGenesis;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [AddComponentMenu("Timeflow/Loop Time Offset")]
    [RequireComponent(typeof(TimeflowObject))]
    public partial class LoopTimeOffset : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        public bool AutoDuration = true;
        public float StartLoopingAt = 0f;
        public float StopLoopingAter = 1000f;

        [SerializeField]
        public TimeValue Duration = null;

        [SerializeField]
        public TimeValue StartAt = null;

        [SerializeField]
        public TimeValue EndAt = null;

        private TimeflowObject targetObject;
        private bool isValid = false;

        protected override void OnAwake()
        {
            base.OnAwake();
            Validate();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Validate();
        }

        public override void Refresh()
        {
            base.Refresh();
            Validate();
            UpdateTime();
        }

        private bool Validate()
        {
            isValid = false;
            if (targetObject == null) {
                targetObject = GetComponent<TimeflowObject>();
                if (targetObject == null) {
                    Debug.LogError($"{name}.LoopTimeOffset: No TimeflowObject found on this GameObject.", gameObject);
                    return isValid;
                }
            }
            if (targetObject.TimeflowParent == null) {
                targetObject.TimeflowParent = Timeflow.Active;
            }

            CalculateTimes();

            isValid = true;
            return isValid;
        }

        public override void OnUpdateTimingMode()
        {
            Validate();
        }

        public void CalculateTimes()
        {
            if (Duration == null || Duration.IsUninitialized) Duration = new TimeValue(TimeValue.DurationTypes.Beats);
            if (StartAt == null || StartAt.IsUninitialized) StartAt = new TimeValue(TimeValue.TimeTypes.Start);
            if (EndAt == null || EndAt.IsUninitialized) EndAt = new TimeValue(TimeValue.TimeTypes.End);

            Duration.Object = targetObject;

            Duration.Mode = TimeValue.Modes.Duration;
            StartAt.Mode = TimeValue.Modes.Time;
            EndAt.Mode = TimeValue.Modes.Time;

            Duration.Calculate();
            StartAt.Calculate();
            EndAt.Calculate();

            if (targetObject != null) {
                if (AutoDuration) Duration.Time = targetObject.TrackDuration;
                else {
                    targetObject.TrackDuration = Duration.Time;
                }
            }

            //Debug.Log($"{name}.LoopTimeOffset.CalculateTimes() Duration:{Duration.Time}, StartAt:{StartAt.Time}, EndAt:{EndAt.Time}");
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            CalculateTimes();
        }

        public override void UpdateTime()
        {
            if (!isValid || targetObject == null) return;
            base.UpdateTime();

            float duration = Duration.Time;
            if (AutoDuration) duration = targetObject.TrackDuration;

            if (duration <= 0f) return; // Avoid division by zero

            // Get the current time from the parent or containing Timeflow 
            float time = targetObject.ParentObject == null ? targetObject.Timeflow.GetTime() : targetObject.ParentObject.GetTime();
            time -= StartAt.Time;

            if (time < 0) time = 0;

            float end = EndAt.Time - StartAt.Time;
            float frame = 1f / Timeflow.Active.FPS;
            end -= frame; // Prevent jumping one frame past the end time
            if (time > end) time = end;

            // Calculate the interval based on the duration  
            float intervalStart = Mathf.Floor(time / duration) * duration;

            // Set the time of the target object to the beginning of the interval  
            targetObject.TimeOffset = intervalStart + StartAt.Time;
            //Debug.Log($"{name}.LoopTimeOffset: {targetObject.TimeOffset} time:{time} StartAt:{StartAt.Time}, EndAt:{EndAt.Time} Duration:{duration})");
        }

#if UNITY_EDITOR

        private List<float> drawTimeOffsets = new List<float>();

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Loop Time Offset"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<LoopTimeOffset>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public override void OnTrackChange()
        {
            base.OnTrackChange();
            CalculateTimes();
        }

        public override void GUIChannelOverlay()
        {
            if (targetObject == null) return;
            _GUITracks();
        }

        private void _GUITracks()
        {
            if (targetObject.Track.IsHidden || !enabled || !Enabled) return;
            bool isGhost = true;

            float inTime = Timeflow.Active.View.TimeOfPosition(0, true);
            float outTime = Timeflow.Active.View.TimeOfPosition(Timeflow.Active.Layout.TimeAreaInner.Width, true);

            drawTimeOffsets.Clear();
            float duration = Duration.Time;
            if (AutoDuration) duration = targetObject.TrackDuration;

            float t = targetObject.TimeOffset - duration;
            if(Mathf.Approximately(t, StartAt.Time)) {
                drawTimeOffsets.Add(t);
                t -= duration;
            }
            while (t >= inTime && t >= StartAt.Time) {
                drawTimeOffsets.Add(t);
                t -= duration;
            }
            t += duration; // Skip current track already drawn
            float frame = 1f / Timeflow.Active.FPS;
            while (t < outTime && t < EndAt.Time - frame) {
                drawTimeOffsets.Add(t);
                t += duration;
            }

            foreach (Keyframe key in targetObject.Track.Keys) {
                GUI.color = AxonColor.White;
                GUIRect rect = key.GUIRect;
                rect.x = rect.x + (rect.width - 18);
                rect.width = rect.height = 16;
                GUI.DrawTexture(rect, AxonUI.Icons.LoopTimeOffset);
                break;
            }

            foreach (float timeOffset in drawTimeOffsets) {

                foreach (Keyframe key in targetObject.Track.Keys) {
                    if (key == null || key.Channel == null) {
                        continue;
                    }
                    float keyTime = timeOffset;
                    float keyValue = keyTime + duration;

                    if (keyTime > outTime || keyValue < inTime) {
                        continue;
                    }

                    GUIRect keyGUIRect = key.GUIRect;

                    float inPoint = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    float outPoint = Timeflow.Active.View.PositionOfTime(keyTime + (1f / Timeflow.Active.FPS), true);
                    if (keyValue > keyTime) {
                        outPoint = Timeflow.Active.View.PositionOfTime(keyValue, true);
                        outPoint -= inPoint;
                    }
                    keyGUIRect = new GUIRect(inPoint, targetObject.Track.GUIRect.y, outPoint, targetObject.Track.GUIHeight - 2);

                    bool isLocked = targetObject.Track.IsLocked;
                    bool isSelected = !isLocked && Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key);

                    // Adjust the color based on status of being locked or selected
                    Color white = AxonColor.Ghost;
                    Color keyColor = white;
                    Color color = white;

                    if (key.IsKeyEnabled) {
                        keyColor = key.OverrideGUIColor ? key.KeyColor : key.Channel.GUIColor;
                    }
                    else {
                        keyColor = Color.gray;
                    }
                    if (isGhost) {
                        keyColor.a = 0.3f;
                    }
                    color = MathUtil.Interpolate(keyColor, white, 0.25f);

                    GUI.color = color;
                    GUI.Box(keyGUIRect, GUIContent.none, AxonUI.TrackStyle);

                    if (isSelected || targetObject.Track.Object.IsSelected) {
                        color = key.IsKeyEnabled ? isSelected ? AxonColor.KeySelected : AxonColor.TrackPartialSelection : AxonColor.Default;

                        color.a = isSelected ? TimeflowPreferences.Current.TrackSelectedPattern * 0.5f : TimeflowPreferences.Current.TrackSelectedPattern * 0.5f;
                        // Adds to the regular track box drawn above
                        GUIRect r = keyGUIRect;
                        GUI.color = color;
                        GUI.DrawTextureWithTexCoords(r, AxonUI.TrackSelectedPatternStyle.normal.background, new Rect(0, 0, r.width / 16f, 1f));
                        GUI.Box(r, new GUIContent(), AxonUI.TrackSelectedStyle);
                    }

                }
            }

        }
#endif
    }
}
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is a utility behavior that can be added to any TimeflowObject to display the current time in various formats.
    /// Use the SetDisplay UnityEvent to connect to a UI Text component or other display method.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    [AddComponentMenu("Timeflow/Time Display")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/time-display")]
    sealed public class TimeDisplay : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC

        public enum Modes
        {
            Seconds,
            Frames,
            Timecode,
            Measures,
            Custom
        }
        public Modes Mode = Modes.Timecode;

        public bool UseCustomTimeInput = false;
        public float CustomTimeInput = 0f;

        public bool UseCustomFPS = false;
        public float CustomFPS = 30f;

        public bool UseCustomBPM = false;
        public float CustomBPM = 120f;
        public int CustomBeatsPerBar = 4;
        public int CustomBeatNoteSize = 4;

        [Tooltip("Custom time format. Examples: \"hh\\:mm\\:ss\", \"mm\\:ss\", \"ss\\.ff\"")]
        public string CustomFormat = @"mm\:ss";

        public UnityEvent<string> SetDisplay;
        public string Output { get; private set; }

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        public override void UpdateTime()
        {
            if (!CanUpdate || SetDisplay == null) return;
            base.UpdateTime();

            float time = UseCustomTimeInput ? CustomTimeInput : Timeflow.CurrentTime;
            Output = DisplayTime(time, Mode);
            SetDisplay.Invoke(Output);
        }

        public string DisplayTime(float value, Modes mode)
        {
            float fps = UseCustomFPS ? CustomFPS : Timeflow.Active.FPS;
            string time = null;
            if (mode == Modes.Timecode) {
                time = StringUtil.SecondsToTimecode(value, true, !TimeflowPreferences.Current.UseFractionalTime, fps);
            }
            else
            if (mode == Modes.Seconds) {
                time = "" + value;
            }
            else
            if (mode == Modes.Frames) {
                time = "" + Mathf.RoundToInt((float)fps * value);
            }
            else
            if (mode == Modes.Measures) {
                float bpm = UseCustomBPM ? CustomBPM : Timeflow.Active.BPM;
                int beatsPerBar = UseCustomBPM ? CustomBeatsPerBar : Timeflow.Active.BeatsPerBar;
                int beatNoteSize = UseCustomBPM ? CustomBeatNoteSize : Timeflow.Active.BeatNoteSize;
                time = StringUtil.SecondsToMeasures(value, bpm, beatsPerBar, beatNoteSize);
            }
            else {
                TimeSpan timeSpan = TimeSpan.FromSeconds(value);

                try {
                    time = timeSpan.ToString(CustomFormat);
                }
                catch (FormatException e) {
                    Debug.LogError($"Invalid time format: {CustomFormat}. Error: {e.Message}");
                    time = "--INVALID FORMAT--";
                }
            }
            return time;
        }

#if UNITY_EDITOR

        public override Texture2D Icon => AxonUI.Icons.TimeDisplay;

        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Time Display"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<TimeDisplay>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }

}//AxonGenesis

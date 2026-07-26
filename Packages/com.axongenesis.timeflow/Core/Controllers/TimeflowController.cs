// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class TimeflowController : TimeflowPlayback
    {
        public enum PlayModes
        {
            OneShot,
            Looped,
            Continuous
        }

        public PlayModes PlayMode = PlayModes.OneShot;
        public bool WorkAreaEnabled = true;
        public bool AutoPlay = true;

        public bool SetMarkerByName = false;
        public int StartAtMarkerIndex = 1;
        public string StartAtMarkerName = "";

        [Tooltip("Perform any one-time setup operations at startup.")]
        [FormerlySerializedAs("OnStartup")]
        public UnityEvent OnStartupEvent = null;

        [Tooltip("This event fires when Timeflow starts to play.")]
        [FormerlySerializedAs("OnPlay")]
        public UnityEvent OnPlayEvent = null;

        [Tooltip("This event fires when the Timeflow target stops playing. This will happen automatically when " +
            "using OneShot mode or the end of Timeflow is reached and loop is disabled.")]
        [FormerlySerializedAs("OnStop")]
        public UnityEvent OnStopEvent = null;

        [Tooltip("This event fires when Skip() is called from this controller. Use this event to perform UI or player actions to return to gameplay.")]
        [FormerlySerializedAs("OnSkip")]
        public UnityEvent OnSkipEvent = null;

        [Tooltip("This event fires when .")]
        [FormerlySerializedAs("OnRewind")]
        public UnityEvent OnRewindEvent = null;

        [Tooltip("This event fires when Skip() is called from this controller. Use this event to perform UI or player actions to return to gameplay.")]
        public UnityEvent OnLoopEvent = null;

        public bool EditorShowEvents = true;

        private bool hasTimeflow = false;

        private bool CanExecute => (Application.isPlaying && PlaybackMode != PlaybackModes.EditorOnly) || (!Application.isPlaying && PlaybackMode != PlaybackModes.RuntimeOnly);

        public float EndTime {
            get {
                if (TimeflowParent.WorkAreaEnabled) return TimeflowParent.WorkAreaEndTimeExact;
                return TimeflowParent.EndTime;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (!CanExecute) return;
            if (TimeflowParent != null) {
                TimeflowParent.WorkAreaDisableOnStart = false;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (TimeflowParent == null) TimeflowParent = Timeflow.Active;
            if (TimeflowParent == null) {
                hasTimeflow = false;
                Debug.LogWarning("No Timeflow instance found in the scene");
                return;
            }
            if (!CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.Start");
            hasTimeflow = true;
            TimeflowParent.AutoPlay = AutoPlay;
            TimeflowParent.WorkAreaEnabled = WorkAreaEnabled;
            TimeflowParent.PlayPastEnd = PlayMode == PlayModes.Continuous;


            TimeflowMarker marker = null;
            if (SetMarkerByName) {
                marker = TimeflowParent.Markers.GetMarker(StartAtMarkerName);
                if (marker != null) {
                    TimeflowParent.Markers.GotoMarker(marker);
                }
                else {
                    Debug.LogWarning($"Invalid marker name {StartAtMarkerName}");
                }
            }
            else {
                marker = TimeflowParent.Markers.GetMarker(StartAtMarkerIndex);
                if (marker != null) {
                    TimeflowParent.Markers.GotoMarker(marker);
                }
                else {
                    Debug.LogWarning($"Invalid marker index {StartAtMarkerIndex}");
                }
            }

            if (!AutoPlay) {
                if (TimeflowParent.IsPlaying) TimeflowParent.Stop();
            }
            else {
                TimeflowParent.Play(TimeflowParent.PlayFromStart);
            }
            OnStartupEvent?.Invoke();
        }

        private void UpdatePlayMode()
        {
            hasTimeflow = TimeflowParent != null;
            if (!hasTimeflow) return;
            if (!CanExecute) return;
            TimeflowParent.WorkAreaEnabled = WorkAreaEnabled;
            TimeflowParent.LoopEnabled = PlayMode == PlayModes.Looped;
        }

        public void SetWorkAreaStart(float startTime)
        {
            if (TimeflowParent == null) return;
            if (DebugEnabled) Debug.Log($"{name}.SetWorkAreaStart:{startTime}");
            TimeflowParent.WorkAreaStart = startTime;
            TimeflowParent.ValidateWorkArea();
            UpdatePlayMode();
        }

        public void SetWorkAreaEnd(float endTime)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.SetWorkAreaEnd:{endTime}");
            TimeflowParent.WorkAreaEnd = endTime;
            TimeflowParent.ValidateWorkArea();
            UpdatePlayMode();
        }

        public void SetWorkArea(float startTime, float endTime)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.SetWorkArea:{startTime}-{endTime}");
            TimeflowParent.SetWorkArea(startTime, endTime, true);
            UpdatePlayMode();
        }

        public void PlayFromMarkerIndex(int index)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.PlayFromMarkerIndex:{index}");
            PlayFromMarker(TimeflowParent.Markers.GetMarker(index));
        }

        public void PlayFromMarker(string markerName)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (string.IsNullOrEmpty(markerName)) return;
            if (DebugEnabled) Debug.Log($"{name}.PlayFromMarker:{markerName}");
            PlayFromMarker(TimeflowParent.Markers.GetMarker(markerName));
        }

        public void PlayFromMarker(TimeflowMarker marker)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (marker == null) return;
            if (DebugEnabled) Debug.Log($"{name}.PlayFromMarker:{marker.Name}");
            UpdatePlayMode();
            TimeflowParent.Markers.GotoMarker(marker);
            Play();
        }

        public void PlayFromTime(float time)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.PlayFromTime:{time}");
            TimeflowParent.SetTime(time);
            Play();
        }

        public void PlayFromStart()
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.PlayFromStart");
            UpdatePlayMode();
            TimeflowParent.Play(true);
        }

        public void Play()
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.Play");
            UpdatePlayMode();
            TimeflowParent.Play(false);
        }

        /// <summary>
        /// Use this to skip to the end of the current playback region, with an optional lead out time.
        /// </summary>
        /// <param name="padSecondsFromEnd">To play the end of the current region, set this to the desired duration. Otherwise set to 0 for an immediate jump to the end.</param>
        public void Skip(float padSecondsFromEnd = 0)
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (DebugEnabled) Debug.Log($"{name}.Skip");
            float endTime = EndTime - padSecondsFromEnd;
            if (TimeflowParent.CurrentTime <= endTime) {
                TimeflowParent.SetTime(endTime);
                OnSkipEvent?.Invoke();
            }
        }

        public void Stop()
        {
            if (TimeflowParent == null || !CanExecute) return;
            if (!TimeflowParent.IsPlaying) return;
            if (DebugEnabled) Debug.Log($"{name}.Stop");
            TimeflowParent.Stop();
        }

        public void SetPlayModeOneShot()
        {
            if (DebugEnabled) Debug.Log($"{name}.SetPlayModeOneShot");
            PlayMode = PlayModes.OneShot;
            UpdatePlayMode();
        }

        public void SetPlayModeContinuous()
        {
            if (DebugEnabled) Debug.Log($"{name}.SetPlayModeContinuous");
            PlayMode = PlayModes.Continuous;
            UpdatePlayMode();
        }

        public void SetPlayModeLooped()
        {
            if (DebugEnabled) Debug.Log($"{name}.SetPlayModeLooped");
            PlayMode = PlayModes.Looped;
            UpdatePlayMode();
        }

        public override void OnPlay()
        {
            if (DebugEnabled) Debug.Log($"{name}.OnPlay");
            OnPlayEvent?.Invoke();
        }

        public override void OnUpdate()
        {
            if (!hasTimeflow || PlayMode != PlayModes.OneShot) return;
            if (TimeflowParent.CurrentTime >= EndTime) {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (DebugEnabled) Debug.Log($"{name}.OnStop");
            OnStopEvent?.Invoke();
        }

        public override void OnRewind()
        {
            if (DebugEnabled) Debug.Log($"{name}.OnRewind");
            OnRewindEvent?.Invoke();
        }

        public override void OnLoop()
        {
            if (DebugEnabled) Debug.Log($"{name}.OnLoop");
            OnLoopEvent?.Invoke();
        }
    }
}

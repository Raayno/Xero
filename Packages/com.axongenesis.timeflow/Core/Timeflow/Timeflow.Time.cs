// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {

        #region PUBLIC NON-SERIALIZED

        /// <summary>
        /// Timeflow defines its own delta time which may differ from Unity's Time.deltaTime due to the
        /// ability to customize update timing. For example, if the behavior or Timeflow instance is
        /// configured to force the frame rate for a stepped or special effect. Timeflow behaviors should
        /// use this DeltaTime instad of Time.deltaTime for proper function. 
        /// </summary>
        [NonSerialized]
        public float DeltaTime;

        [NonSerialized]
        public float SceneStartedAtTime;   // The time the scene started at when initially played

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        [TimeflowIgnore]
        private float _StartTime;

        [TimeflowIgnore]
        [SerializeField, FormerlySerializedAs("_TotalTime")]
        private float _EndTime = 10f;

        [SerializeField]
        [TimeflowIgnore]
        private float _GlobalTimeScale = 1f;

        [SerializeField]
        [TimeflowIgnore]
        private bool _CanSetGlobalTimeScale = false;

        [SerializeField]
        [TimeflowIgnore]
        private float _FPS = 60f;

#if UNITY_EDITOR
        [SerializeField]
        [TimeflowIgnore]
        private float _CustomSnap = 1f;
#endif

        [SerializeField]
        [TimeflowIgnore]
        private float _BPM = 120f;

        [SerializeField]
        [TimeflowIgnore]
        private int _BeatsPerBar = 4;

        /// <summary>
        /// 8 = sixteenth, 4 = quarter note, 2 = half note, 1 = bar
        /// </summary>
        [SerializeField]
        [TimeflowIgnore]
        private int _BeatNoteSize = 4;

        #endregion

        #region PRIVATE

        private DateTime startTime = DateTime.Now;
        private float timeStarted;
        private Stopwatch _Stopwatch;
        private uint _FrameID;

        #endregion

        #region TIME

        public uint FrameID {
            get {
#if UNITY_EDITOR
                // Force frame ID to continually change when capturing for render
                if (Time.captureFramerate != 0) {
                    _FrameID++;
                    if (_FrameID > 9999999) _FrameID = 1; // reset to avoid overflow
                }
#endif
                return _FrameID;
            }
            set { _FrameID = value; }
        }

        public void SetStartTime(float time)
        {
            TimeOffset = time;
            SetupChannels(true);
        }

        public override float GetTime()
        {
            if (!CanUpdate) return StartTime;

            float time;
            if (DirectorSyncEnabled) {
                time = DirectorTime + DirectorTimeStart;
            }
            else
            if (HasTimeflowParent && TimeflowParent != this && Active != this) {
                time = TimeflowParent.GetTime() * TimeScaleWorld - TimeOffset;
            }
            else {
                time = CurrentTime;
            }

            if (MathUtil.IsNaN(time)) {
                Debug.LogWarning("Time value is not a number or is infinity");
                time = 0;
            }

            if (TimeflowPreferences.Current.TimeToleranceStrict) {
                // Force time to nearest interval
                time = Timeflow.ApplyTimeTolerance(time);
            }

            bool hasLooped = false;
            float loopedTime = GetLoopedTime(time, out hasLooped);
            if (IsPlaying && hasLooped) {
                OnLooped();
            }

            return loopedTime;
        }

        public override void SetTime(float value) => SetTime(value, false);

        public void SetTime(float value, bool fromDirector)
        {
            if (!IsActive && HasTimeflowParent) {
                base.SetTime(value);
                return;
            }
            bool hasLooped = false;
            float v = GetLoopedTime(value, out hasLooped);

            if (CurrentTime != v) {
                //Debug.Log($"{name}.SetTime:{CurrentTime}");
                // Detect when the time has jumped
                if (v < StartTime) v = StartTime;
                DeltaTime = v - CurrentTime;

                float forceTime = TimeflowPreferences.Current.TimeToleranceStrict ? Timeflow.ApplyTimeTolerance(v) : v;
                if (UpdateFrequency == UpdateFrequencies.ForceFramerate) {
                    if (ForceFramerate < 0f) ForceFramerate = 1f;
                    forceTime = Mathf.RoundToInt(v * ForceFramerate) / ForceFramerate;
                }
                else
                if (UpdateFrequency == UpdateFrequencies.TimeInterval) {
                    if (TimeInterval < 0f) TimeInterval = 0.1f;
                    forceTime = MathUtil.Snap(v, TimeInterval);
                }
                v = forceTime;

                if (CurrentTime != v) {
                    //Debug.Log($"{name}.SetTime:{v} fromDirector:{fromDirector}");
                    if (MathUtil.IsNaN(v)) {
                        Debug.LogWarning("Time value is not a number or is infinity");
                        v = 0;
                    }
                    _SetCurrentTimeInternal(v);
                    timeStarted = v;

                    if (hasLooped || Mathf.Abs(DeltaTime) > 0.5f || DeltaTime < 0f) {
                        ResetElapsedTime(DeltaTime < 0f);
                    }
                    if (hasLooped) OnLooped();

                    FrameID++; // provides a unique id to each frame

                    // Set global shader variable values
                    UpdateShaderValues();

                    // Clamp delta time for any behaviors that reference it
                    if (DeltaTime > 1f || DeltaTime < 0f) DeltaTime = 0f;

                    if (!fromDirector) UpdateDirector();
                }
            }
        }

        public void SetTimeRelative(float timeOffset)
        {
            SetTime(CurrentTime + timeOffset);
        }

        public void ResetElapsedTime(bool rewind)
        {
            RestartElapsedTime();
            SceneStartedAtTime = CurrentTime;
            if (rewind) {
                _OnRewind();
            }
        }

        private void RestartElapsedTime()
        {
            Stopwatch.Restart();
            startTime = DateTime.Now;
            //Debug.Log($"RestartElapsedTime:{startTime}");
#if UNITY_EDITOR
            nextFixedUpdateTime = 0f;
#endif
        }

        public void _OnRewind()
        {
            RewindGroups();
            if (PlaybackListeners != null) foreach (var i in PlaybackListeners) i?.OnRewind();
#if UNITY_EDITOR
            View.ScrollCenter();
#endif
        }

        #endregion

        #region ACCESSORS

        public Stopwatch Stopwatch {
            get {
                if (_Stopwatch == null) _Stopwatch = new Stopwatch();
                return _Stopwatch;
            }
        }

        public override float CurrentTime {
            get {
                if (!IsActive) {
                    return base.CurrentTime * TimeScaleWorld;
                }
                return _CurrentTime * TimeScaleWorld;
            }
            set {
                base.CurrentTime = value;
                //Debug.Log($"{name}.CurrentTime{value}");

                if (PlaybackListeners != null) foreach (var i in PlaybackListeners) i?.OnUpdate();
            }
        }

        public override float TimeScaleWorld {
            get {
                if (IsActive) {
                    return 1f;
                }
                else {
                    return base.TimeScaleWorld;
                }
            }
        }

        public bool CanSetGlobalTimeScale {
            get {
                return _CanSetGlobalTimeScale;
            }
            set {
                if (_CanSetGlobalTimeScale != value) {
                    _CanSetGlobalTimeScale = value;
                }
            }
        }

        public float GlobalTimeScale {
            get {
                return _GlobalTimeScale;
            }
            set {
                if (_GlobalTimeScale != value) {
                    _GlobalTimeScale = value;
                    if (CanSetGlobalTimeScale) Time.timeScale = value;
                }
            }
        }

#if UNITY_EDITOR
        public bool UseCustomSnap {
            get {
                if (View == null) return false;
                return View.GridSnap == 22;
            }
            set {
                if (View == null) return;
                if (value && View.GridSnap != 22) View.GridSnap = 22;
                else
                if (View.GridSnap == 22) View.GridSnap = 4;
            }
        }

        public float CustomSnap {
            get {
                if (_CustomSnap <= 0) _CustomSnap = 1f / _FPS;
                return _CustomSnap;
            }
            set {
                if (_CustomSnap != value) {
                    _CustomSnap = value;
                    if (_CustomSnap <= 0f) _CustomSnap = 1f / _FPS;// to avoid NaN errors
                    if (UseCustomSnap && View != null) View.Snap = _CustomSnap;
                }
            }
        }
#endif
        public float FPS {
            get {
                if (_FPS <= 0) _FPS = 30;
                return _FPS;
            }
            set {
                if (_FPS != value) {
                    _FPS = value;
                    if (_FPS <= 0f) _FPS = 0.01f;// to avoid NaN errors
                    if (!Application.isPlaying) {
                        DoUpdate();
                    }
                    UpdateGroupsTimingMode();
                }
            }
        }

        public float BPM {
            get {
                if (_BPM <= 0) _BPM = 1;
                return _BPM;
            }
            set {
                if (_BPM != value) {
                    _BPM = value;
                    if (_BPM < 0.1f) _BPM = 0.1f;
#if UNITY_EDITOR
                    View.RecalculateSnap();
                    if (!Application.isPlaying) {
                        DoUpdate();
                    }
#endif
                    UpdateGroupsTimingMode();
                }
            }
        }

        public int BeatsPerBar {
            get {
                return _BeatsPerBar;
            }
            set {
                if (_BeatsPerBar != value) {
                    _BeatsPerBar = value;
                    if (!Application.isPlaying) {
                        DoUpdate();
                    }
                    UpdateGroupsTimingMode();
                }
            }
        }

        public int BeatNoteSize {
            get {
                return _BeatNoteSize;
            }
            set {
                if (_BeatNoteSize != value) {
                    _BeatNoteSize = value;
                    if (!Application.isPlaying) {
                        DoUpdate();
                    }
                    UpdateGroupsTimingMode();
                }
            }
        }

        public override int CurrentFrame {
            get {
                return Mathf.RoundToInt(CurrentTime * FPS);
            }
            set {
                CurrentTimeExplicit = value / FPS;
            }
        }

        public override float TimeOffsetWorld {
            get {
                if (!IsActive && HasTimeflowParent) {
                    return base.TimeOffsetWorld;
                }
                if (Active == this) {
                    return 0;
                }
                else {
                    float t = TimeOffset;
                    if (ParentObject != null && ParentObject != this) {
                        t += ParentObject.TimeOffsetWorld;
                    }
                    return t;
                }
            }
            set {
                if (!IsActive && HasTimeflowParent) {
                    base.TimeOffsetWorld = value;
                    return;
                }
                TimeOffset = value;
            }
        }

        public float CurrentTimeExplicit {
            set {
                if (!IsActive) return;
#if UNITY_EDITOR
                if (TimeflowPreferences.Current.UndoTimeChanges) UndoUtil.Undo(Timeflow, "Set Time");
                if (!IsPlaying && View.SnapTimeEnabled) {
                    value = View.SnapTime(value);
                }
                if (value < StartTime) value = StartTime;
                if (value > View.EndTimePadded) value = View.EndTimePadded;
#endif

                if (CurrentTime != value) {
                    _SetCurrentTimeExplicit(value);
                }
            }
        }

        private void _SetCurrentTimeExplicit(float time)
        {
            if (!IsActive) return;
            DeltaTime = time - CurrentTime;
            if (DeltaTime > 1f || DeltaTime < 0f) DeltaTime = 0f;

            if (time < StartTime) time = StartTime;
            _SetCurrentTimeInternal(time); // set directly to prevent looping for user direct control
            timeStarted = time;

            //Debug.Log(name + ".Timeflow._SetCurrentTimeExplicit:" + CurrentTime + " in:" + time + " StartTime:" + StartTime);

            FrameID++; // provides a unique id to each frame

            UpdateShaderValues();
            UpdateDirector();
            ResetElapsedTime(DeltaTime < -1f);
            UpdateGroups();

#if UNITY_EDITOR
            EditorUpdateFrame();
            View.ObjectTouched = true;
#endif
        }

        private void _SetCurrentTimeInternal(float value)
        {
            //Debug.Log($"{name}.SetTimeInternal:{value}");
            if (!IsPlaying && TimeflowPreferences.Current.TimeToleranceMode != TimeflowPreferences.TimeToleranceModes.Float) {
                value = MathUtil.RoundToInterval(value, TimeflowPreferences.Current.TimeTolerance);
            }

            SetGroupsTime(value);

            base.SetTime(value);
        }

        public float CurrentTimeExact {
            set {
                if (!IsActive) return;
                if (CurrentTime != value) {
                    DeltaTime = value - CurrentTime;
                    if (DeltaTime > 1f || DeltaTime < 0f) DeltaTime = 0f;
                    CurrentTimeExplicit = value;
                }
            }
        }

        public float FrameDuration {
            get {
                if (_FPS <= 0) _FPS = 1;
                return 1f / _FPS;
            }
        }

        public override float TimeScale {
            get {
                return base.TimeScale;
            }
            set {
                if (base.TimeScale != value) {
                    base.TimeScale = value;
                    Refresh();
                }
            }
        }

        public float GlobalTimeOffset {
            get {
                if (IsTimeScopeEnabled && IsTimeScopeLocalized) {
                    return -TimeScopeStart;
                }
                return 0;
            }
        }

        public float GlobalStartTime {
            get { return _StartTime * TimeScaleWorld; }
            set {
                if (_StartTime != value) {
                    _StartTime = value;
                    OnUpdateTimeflowTrack();
                }
            }
        }

        public float GlobalEndTime {
            get { return _EndTime * TimeScaleWorld; }
            set {
                if (_EndTime != value) {
                    _EndTime = value;
                    OnUpdateTimeflowTrack();
                }
            }
        }

        public float GlobalDuration {
            get {
                return GlobalEndTime - GlobalStartTime;
            }
            set {
                float newEndTime = _StartTime + value;
                bool canSet = true;
#if UNITY_EDITOR
                canSet = !View.LockDuration;
#endif
                if (canSet && _EndTime != newEndTime) {
                    _EndTime = newEndTime;
                    OnUpdateTimeflowTrack();
                }
            }
        }

        /// <summary>
        /// The StartTime is most often 0 but must not be assumed to always be zero. In special cases, a
        /// user may find it helpful to set the start time to a specific value.
        /// </summary>
        public override float StartTime {
            get {
                if (!IsActive) {
                    if (HasTimeflowParent) {
                        //Debug.Log($"{name}.base.StartTime{base.StartTime}");
                        return base.StartTime;
                    }
                    //Debug.Log($"{name}.StartTime Timeflow:{_StartTime}");
                    return _StartTime;
                }
                if (IsTimeScopeEnabled) {
                    //Debug.Log($"{name}.StartTime TimeScopeStart:{TimeScopeStart}");
                    return TimeScopeStart;
                }
                return _StartTime;
            }
            set {
#if UNITY_EDITOR
                if (!IsActive) {
                    if (HasTimeflowParent) {
                        base.StartTime = _StartTime = value;
                    }
                    else {
                        _StartTime = value;
                    }
                }
                else
                if (IsTimeScopeEnabled) {
                    TimeScopeStart = value;
                }

#endif
#if UNITY_EDITOR
                if (!View.LockDuration) {
#endif
                    if (_StartTime != value) {
                        _StartTime = value;
                        if (_StartTime > TimeflowPreferences.MaxDuration) _StartTime = TimeflowPreferences.MaxDuration;
                        if (_StartTime < -TimeflowPreferences.MaxDuration) _StartTime = -TimeflowPreferences.MaxDuration;

                        if (Track != null && Track.Keys.Count > 0) {
                            Track.Keys[0].KeyTime = _StartTime;
                        }

                        if (Markers.StartMarker != null) Markers.StartMarker.GlobalTime = _StartTime;
#if UNITY_EDITOR
                        Markers.GotoMarker(-1);
#endif
                        UpdateAutoFullLengthTracksRecursively();
                    }
#if UNITY_EDITOR
                }
#endif
            }
        }

        public override float EndTime {
            get {
                if (!IsActive) {
                    if (HasTimeflowParent) {
                        return base.EndTime;
                    }
                    return _EndTime;
                }
                if (IsTimeScopeEnabled) {
                    return TimeScopeEnd;
                }
                // Make sure it isn't zero other NaN errors may occur
                if (_EndTime == 0f) _EndTime = 1f;
                return _EndTime;
            }
            set {
                if (!IsActive) {
                    if (HasTimeflowParent) {
                        base.EndTime = _EndTime = value;
                    }
                    else {
                        _EndTime = value;
                    }
                }
#if UNITY_EDITOR
                else
                if (IsTimeScopeEnabled) {
                    TimeScopeEnd = value;
                }
#endif
#if UNITY_EDITOR
                if (!View.LockDuration) {
#endif
                    if (_EndTime != value) {
                        _EndTime = value;
                        //Debug.Log($"_EndTime:{value}");
                        float min = StartTime + 1f;
                        if (_EndTime < min) _EndTime = min;
                        if (_EndTime > TimeflowPreferences.MaxDuration) _EndTime = TimeflowPreferences.MaxDuration;

                        if (Markers.EndMarker != null) Markers.EndMarker.GlobalTime = _EndTime;

                        if (Track != null && Track.Keys.Count > 0) {
                            Track.Keys[0].KeyValue = _EndTime;
                        }
#if UNITY_EDITOR
                        Timeflow.View.ScrollZoomOutFull();
#endif
                        OnUpdateTimeflowTrack();
                        UpdateAutoFullLengthTracksRecursively();
                    }
#if UNITY_EDITOR
                }
#endif
            }
        }

        public override float StartTimeWorld {
            get {
                _BypassTimeScopeInternal = true;
                float time = StartTime;
                _BypassTimeScopeInternal = false;
                return time;
            }
            set {
                StartTime = value;
            }
        }

        public override float EndTimeWorld {
            get {
                _BypassTimeScopeInternal = true;
                float time = EndTime;
                _BypassTimeScopeInternal = false;
                return time;
            }
            set {
                EndTime = value;
            }
        }

        public float Duration {
            get {
                return EndTime - StartTime;
            }
            set {
#if UNITY_EDITOR
                if (!View.LockDuration && Duration != value) {
                    float end = StartTime + value;
                    if (EndTime != end) {
                        EndTime = end;
                        UpdateAutoFullLengthTracksRecursively();
                    }
                }
#else
                if (Duration != value) {
                    EndTime = StartTime + value;
                }
#endif
            }
        }


        public int TotalFrames {
            get {
                return Mathf.RoundToInt((EndTime - StartTime) * FPS);
            }
        }

        #endregion

        #region TIME METHODS

        /// <summary>
        /// This is only called for precomps (nested Timeflows) from the parent
        /// </summary>
        public override void UpdateTime()
        {
            if (TimeflowParent == null) return;
            //if (DebugEnabled) Debug.Log($"{name}.UpdateTime");
            base.UpdateTime();
            UpdateObjects();
            UpdateGroups();
        }

        /// <summary>
        /// This method applies looping behavior to time (if enabled). It does not force time tolerances
        /// and only alters value if outside of the designated range.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float GetLoopedTime(float time) { return GetLoopedTime(time, out var looped); }

        public float GetLoopedTime(float time, out bool hasLooped)
        {
#if UNITY_EDITOR
            if (View.Input.IsDragging) {
                // Don't loop time around when dragging in the view
                hasLooped = false;
                return time;
            }
#endif
            float original = time;

            hasLooped = false;

            float startTime;
            float endTime;
            bool leadIn;
            bool hasEnded = false;
            bool playPastEnd;

            if (WorkAreaEnabled && WorkAreaEnd > WorkAreaStart) {
                startTime = WorkAreaStart;
                endTime = WorkAreaEndTimeExact;
                leadIn = WorkAreaAllowsLeadIn;
                playPastEnd = WorkAreaPlayPastEnd;
            }
            else {
                startTime = StartTime;
                endTime = EndTime;
                leadIn = false;
                playPastEnd = PlayPastEnd;
            }

            if (time > endTime) {
                if (IsPlayReverse) {
                    if (!leadIn) {
                        time = endTime;
                    }
                }
                else {
                    if (LoopEnabled) {
                        time = startTime;
                        hasLooped = true;
                    }
                    else
                    if (!playPastEnd) {
                        time = endTime;
                        hasEnded = true;
                    }
                }
            }
            else
            if (IsPlayReverse) {
                if (time < startTime) {
                    if (LoopEnabled) {
                        time = endTime;
                        hasLooped = true;
                    }
                    else
                    if (!playPastEnd) {
                        time = startTime;
                        hasEnded = true;
                    }
                }
            }
            else
            if (!leadIn && time < startTime) {
                time = startTime;
            }

            if (hasEnded && IsPlaying) Stop();

            return time;
        }

        /// <summary>
        /// Returns the frame number for a time in seconds. While time may be based on a StartTime other
        /// than zero, frame numbers always start at 0 at the beginning of Timeflow.
        /// </summary>
        public int GetFrameFromSeconds(float seconds)
        {
            return Mathf.RoundToInt((seconds - StartTime) * FPS);
        }

        public float GetSecondsFromFrame(int frame)
        {
            return (float)(frame / FPS) + StartTime;
        }

        private void OnUpdateTimeflowTrack()
        {
            if (Track != null && Track.Keys.Count > 0) {
                Track.Keys[0].KeyTime = _StartTime;
                Track.Keys[Track.Keys.Count - 1].KeyValue = _EndTime;
            }
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR

        #region TIME GLOBAL EDIT

        public override void OnTrackChange()
        {
            base.OnTrackChange();
            if (HasTimeflowParent && Track != null && Track.Keys.Count > 0) {
                _StartTime = Track.Keys[0].KeyTime;
                _EndTime = Track.Keys[Track.Keys.Count - 1].KeyValue;
            }
        }

        public void ScaleTimeGlobal()
        {
            UndoUtil.Undo(this, "Scale Time", true);

            WorkAreaStart = WorkAreaStart * ScaleToolValue;
            WorkAreaEnd = WorkAreaEnd * ScaleToolValue;
            EndTime = EndTime * ScaleToolValue;

            if (MarkerList != null && MarkerList.Count > 0) {
                foreach (TimeflowMarker m in MarkerList) {
                    m.GlobalTime = m.GlobalTime * ScaleToolValue;
                }
            }
            ScaleTimeRecursive(gameObject);

            Refresh(true);
        }

        public void ScaleTimeRecursive(GameObject obj)
        {
            if (ScaleToolValue == 0f) {
                Debug.LogError("Please specify a non-zero value to scale the global time by");
                return;
            }
            UndoUtil.Undo(obj, "Scale Time", true);

            if (obj.TryGetComponent<TimeflowObject>(out var tobj)) {
                tobj.ScaleTime(ScaleToolValue);
            }

            TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
            if (events != null) {
                foreach (TimeflowEvent e in events) {
                    UndoUtil.Undo(e, "Scale Time");
                    e.TriggerTime = e.TriggerTime * ScaleToolValue;
                }
            }

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    ScaleTimeRecursive(child.gameObject);
                }
            }
        }

        public void CropTimeRecursive(GameObject obj)
        {
            UndoUtil.Undo(this, "Crop Time", true);
            if (obj.TryGetComponent<TimeflowObject>(out var tobj)) {
                tobj.GetBehaviors();
                if (tobj.AllChannels != null) {
                    foreach (TimeflowChannel ch in tobj.AllChannels) {
                        if (ch != null) {
                            UndoUtil.Undo(ch.Behavior, "Crop Timeflow Range");
                            ch.CropTimeRange(0f, EndTime, AddEndKeysOnCrop);
                        }
                    }
                }
            }

            TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
            if (events != null) {
                foreach (TimeflowEvent e in events) {
                    if (e.TriggerTimeWorld > EndTime || e.TriggerTimeWorld < 0f) {
                        UndoUtil.UndoDestroy(e);
                    }
                }
            }

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    CropTimeRecursive(child.gameObject);
                }
            }
        }

        #endregion

#endif
        #endregion
    }

}//AxonGenesis

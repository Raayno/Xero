// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        public delegate void TimeflowDelegate();
        public event TimeflowDelegate OnEndLoop;

        public delegate void AutoKeyframingDelegate(bool enabled);
        public event AutoKeyframingDelegate OnAutoKeyframingEnabled;

        #region PUBLIC

        [TimeflowIgnore]
        public bool AutoPlay = true;

        [TimeflowIgnore]
        public bool ContinuousPlay = true;

        [TimeflowIgnore]
        public bool PlayFromStart = true;

        [TimeflowIgnore]
        public bool IsPlayReverse = false;

        [TimeflowIgnore]
        public bool PlayPastEnd;

        public List<ITimeflowPlayback> PlaybackListeners = null;

        #endregion

        #region PRIVATE

        [SerializeField]
        private bool _LoopEnabled = true;

        [SerializeField]
        private bool _AutoKeyframingEnabled = false;

        private float lastOnEndLoop = -1;

        private bool _IsPlaying;

        #endregion

        #region ACCESSORS

        public bool IsPlaying {
            get {
                if (HasTimeflowParent && !IsActive) {
                    if (TimeflowParent == this) {
                        return _IsPlaying;
                    }
                    return TimeflowParent.IsPlaying;
                }
                return _IsPlaying;
            }
            set {
                if (HasTimeflowParent && !IsActive) {
                    if (TimeflowParent == this) {
                        if (_IsPlaying != value) {
                            _IsPlaying = value;
                        }
                    }
                    if (TimeflowParent.IsPlaying != value) {
                        TimeflowParent.IsPlaying = value;
                    }
                    return;
                }
                if (_IsPlaying != value) {
                    _IsPlaying = value;
                }
            }
        }

        public bool IsPlayingInHierarchy {
            get {
                bool playing = _IsPlaying;
                if (!playing) {
                    Timeflow p = TimeflowParent;
                    while (p) {
                        if (p.IsPlaying) {
                            playing = true;
                            break;
                        }
                        p = p.TimeflowParent;
                    }
                }
                return playing;
            }
        }

        public bool AutoKeyframingEnabled {
            get {
                return !Application.isPlaying && _AutoKeyframingEnabled;
            }
            set {
                if (_AutoKeyframingEnabled != value) {
                    // Timeflow must be enabled for autokeyframing
                    if (!enabled) enabled = true;
                    _AutoKeyframingEnabled = value;
                    _OnAutoKeyframingEnabled();
                }
            }
        }

        public bool LoopEnabled {
            get {
                return _LoopEnabled;
            }
            set {
                _LoopEnabled = value;
            }
        }

        #endregion

        #region PLAYBACK LISTENERS

        private void _OnAutoKeyframingEnabled()
        {
            OnAutoKeyframingEnabled?.Invoke(_AutoKeyframingEnabled);
        }

        public void RegisterPlaybackListener(ITimeflowPlayback listener)
        {
            //if (DebugEnabled) Debug.Log($"{name}.RegisterPlaybackListener");
            if (listener == null) return;
            if (PlaybackListeners == null) PlaybackListeners = new List<ITimeflowPlayback>();
            if (!PlaybackListeners.Contains(listener)) PlaybackListeners.Add(listener);
        }

        public void UnregisterPlaybackListener(ITimeflowPlayback listener)
        {
            if (listener == null) return;
            if (PlaybackListeners == null) return;
            if (PlaybackListeners.Contains(listener)) PlaybackListeners.Remove(listener);
        }

        #endregion

        #region PLAYBACK METHODS

        public void Play() { Play(PlayFromStart); }

        public void Play(bool fromStart)
        {
            IsPlayReverse = false;
            _Play(fromStart);
        }

        public void PlayReverse(bool fromStart)
        {
            IsPlayReverse = true;
            _Play(fromStart);
        }

        private void _Play(bool fromStart)
        {
            //Debug.Log($"{name}.Play: fromStart:{fromStart}");
            if (Active == null) Active = this;
            if (fromStart) {
                Stopwatch.Restart();
            }
            else {
                Stopwatch.Start();
            }
            if (Active != this && HasTimeflowParent) {
                IsPlaying = false;
                return;
            }
            IsPlaying = true;
            TimeflowGroup.RegisterAllGroups();

#if UNITY_EDITOR
            if (View.FollowPlayhead) {
                View.ScrollFollowPlayheadSetup();
            }
            if (!Application.isPlaying) {
                EditorApplication.update -= EditorSyncUpdateAll;
                EditorApplication.update += EditorSyncUpdateAll;
            }
            if (Event.current != null && Event.current.alt) fromStart = true;

            /// Restart in the editor if already at the end
            if (IsPlayReverse) {
                if (CurrentTime <= StartTime) fromStart = true;
            }
            else
            if (CurrentTime >= EndTime) fromStart = true;

            if (HideObjectOnPlay != null) {
                HideObjectOnPlay.SetActive(false);
            }
#endif

            if (fromStart) {
                if (WorkAreaEnabled) {
                    CurrentTimeExplicit = IsPlayReverse ? WorkAreaEnd : WorkAreaStart;
                }
                else {
                    CurrentTimeExplicit = IsPlayReverse ? EndTime : StartTime;
                }
#if UNITY_EDITOR
                View.ScrollCenter();
#endif
            }
            SetGroupsTime(CurrentTime);
            RestartElapsedTime();

            SceneStartedAtTime = CurrentTime;

            PlayGroups();
            PlayDirector();

            if (PlaybackListeners != null) foreach (var i in PlaybackListeners) i?.OnPlay();
        }

        public void Resume()
        {
            if (Active != this && HasTimeflowParent) {
                IsPlaying = false;
                return;
            }
            //if (DebugEnabled) Debug.Log($"{name}.Resume");
            if (!IsPlaying) Play(false);
        }

        public void Interrupt()
        {
            if (IsPlaying && !ContinuousPlay) {
                Stop();
            }
        }

        public void Stop(bool stopDirector = true)
        {
            Stopwatch.Stop();
            //Debug.Log($"{name}.Stop");
            IsPlaying = false;

            StopGroups();
            StopChildren();
            if (stopDirector) StopDirector();

#if UNITY_EDITOR
            if (HideObjectOnPlay != null) {
                HideObjectOnPlay.SetActive(true);
            }
#endif
            if (PlaybackListeners != null) foreach (var i in PlaybackListeners) i?.OnStop();
        }

        public void StopChildren()
        {
            if (TimeflowChildren == null) return;
            foreach (Timeflow child in TimeflowChildren) {
                if (child == null) continue;
                if (child != Active) {
                    //if (DebugEnabled) Debug.Log($"{name}.StopChildren: {child.name}");

                    child.Stop();
                }
            }
        }

        public void OnLooped()
        {
            float time = CurrentTime; // direct access to avoid self-referencing loop!

            //if (time == lastOnEndLoop) return;
            lastOnEndLoop = time;

            if (!LoopEnabled) {
                Stop();
            }
            else {
#if UNITY_EDITOR
                if (View.Input.IsDragging) {
                    time = EndTime;
                }
                else
#endif
                if (Duration > 0) {
                    while (time > EndTime) {
                        time -= Duration;
                    }
                }
                RestartElapsedTime();

                if (OnEndLoop != null) {
                    // Prevent recursion by forcing a time to elapse
                    OnEndLoop();
                }
            }
            if (PlaybackListeners != null) foreach (var i in PlaybackListeners) i?.OnLoop();
        }

        public void TogglePlay()
        {
            ContinuousPlay = false;
            _TogglePlay();
        }

        public void ToggleContinuousPlay()
        {
            ContinuousPlay = true;
            _TogglePlay();
        }

        private void _TogglePlay()
        {
            if (IsPlaying) {
                Stop();
            }
            else {
                Play(false);
            }
        }

        public void Rewind(bool useWorkArea = true)
        {
            bool wasPlaying = IsPlaying;
            if (IsPlaying && !ContinuousPlay) Stop();
            if (useWorkArea && WorkAreaEnabled) {
                CurrentTimeExplicit = WorkAreaStart;
            }
            else {
                CurrentTimeExplicit = StartTime;
            }
            _OnRewind();
            if (wasPlaying) Play();
        }

        public void GotoStart(bool useWorkArea = true)
        {
#if UNITY_EDITOR
            if (EditorInput.IsAlt) useWorkArea = false;
#endif
            Rewind(useWorkArea);
        }

        public void GotoEnd(bool useWorkArea = true)
        {
            if (IsPlaying && !ContinuousPlay) {
                Stop();
            }
            if (useWorkArea && WorkAreaEnabled) {
                CurrentTimeExplicit = WorkAreaEndTimeExact;
            }
            else {
                CurrentTimeExplicit = EndTime;
            }
#if UNITY_EDITOR
            View.ScrollCenter();
#endif
        }

        public void GotoPreviousFrame()
        {
            if (IsPlaying && !ContinuousPlay) {
                Stop();
            }
            CurrentTimeExplicit = GetLoopedTime(CurrentTime - FrameDuration);
        }

        public void GotoNextFrame()
        {
            if (IsPlaying && !ContinuousPlay) {
                Stop();
            }
            float t = GetLoopedTime(CurrentTime + FrameDuration);
            //Debug.Log($"GotoNextFrame: {t} FrameDuration:{FrameDuration} CurrentTime:{CurrentTime}");
            CurrentTimeExplicit = t;
        }

        #endregion
    }

}//AxonGenesis


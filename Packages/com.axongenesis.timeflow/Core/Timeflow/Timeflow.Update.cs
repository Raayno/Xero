// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {

        #region ACCESSORS

        public override bool CanUpdate {
            get {
                if (HasTimeflowParent) {
                    return base.CanUpdate;
                }
                return true;
            }
            protected set {
                if (HasTimeflowParent) {
                    base.CanUpdate = value;
                }
            }
        }

        public override bool UseUpdate {
            get {
                return UpdateMethod == UpdateMethods.Update;
            }
            set {
                if (value) {
                    UpdateMethod = UpdateMethods.Update;
                }
            }
        }

        #endregion

        #region UPDATE METHODS

        private int _frame = 0;
        private bool _isStartupDone = false;
        private int _startupFrame = 0;
        public int StartupFrameBuffer = 3;

        private static bool isApplicationPlaying = false;

        private void Update()
        {
            if (!IsAwake) {
                OnAwake();
            }
            if (Instances == null) {
                RegisterAllInstances();
            }

            if (!isApplicationPlaying && Application.isPlaying) {
                isApplicationPlaying = true;
                //if (DebugEnabled) Debug.Log($"Application is Playing ==============================================================================");
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                // Assists displaying prefabs in prefab mode
                Display.UpdatePrefabDisplay();
            }
#endif
            if (!_isStartupDone) {
                if (Application.isPlaying && StartupFrameBuffer > 0 && AutoPlay) {
                    if (_startupFrame < StartupFrameBuffer) {
                        if (_startupFrame == 0) {
                            if (IsPlaying) {
                                Stop();
                            }
                        }
                        _startupFrame++;
                        _isStartupDone = false;
                    }
                    else {
                        //if (DebugEnabled) Debug.Log($"_isStartupDone: AutoPlay:{AutoPlay} PlayFromStart:{PlayFromStart}");
                        _isStartupDone = true;
                        Play(PlayFromStart);
                    }
                }
                else _isStartupDone = true;
            }
#if UNITY_EDITOR
            if (NeedsGlobalRefresh) {
                GlobalRefreshImmediate();
            }
#endif
            if (_isStartupDone && UseUpdate && (Application.isPlaying || !IsPlaying) && !RenderToDisk.IsRendering) {
                OnUpdate();
            }

            // Safely reparent objects before intering the update loop
            Timeflow.UpdateDeferredReparentTransforms();
        }

        private void FixedUpdate()
        {
            if (_isStartupDone && UseFixedUpdate) {
                OnUpdate();
            }
            OnFixedUpdate();
        }

        private void LateUpdate()
        {
            if (_isStartupDone && UseLateUpdate) {
                OnLateUpdate();
            }
            OnFinalUpdate();
        }

        private float GetElapsedTime()
        {
            float e = ((float)Stopwatch.Elapsed.TotalSeconds * Time.timeScale * TimeScale);
            return SceneStartedAtTime + (IsPlayReverse ? -e : e);
        }

        protected override void OnUpdate()
        {
            if (!CanUpdate) return;
            base.OnUpdate();

            if (!HasTimeflowParent || Active == this) {
#if UNITY_EDITOR
                if (IsPlaying && EditorApplication.isPaused) IsPlaying = false;
#endif
                if (!IsPlaying && Time.captureFramerate == 0) {
                    if (DirectorSyncEnabled && Director != null && Director.state == UnityEngine.Playables.PlayState.Playing) {
                        CurrentTime = DirectorTime;
                    }
                }
                else {
                    //if (DebugEnabled) Debug.Log($"{name}.OnUpdate IsRendering:{RenderToDisk.IsRendering}");
                    if (Time.captureFramerate != 0 && !RenderToDisk.IsRendering) {
                        // Time is fixed in exact frame time increments when using capture mode (ie rendering frames)
                        SetTime((float)Time.frameCount / (float)Time.captureFramerate);
                        UpdateGroups();
                    }
                    else
                    if (HasTimeflowParent && TimeflowParent != this && Active != this) {
                        CurrentTime = TimeflowParent.GetTime() * TimeScaleWorld - TimeOffset;
                    }
                    else {
                        float t = GetElapsedTime();
                        bool hasLooped;
                        float currentTime = GetLoopedTime(t, out hasLooped);
                        //Debug.Log($"t:{t} currentTime:{currentTime} hasLooped:{hasLooped}");
                        if (t != currentTime) {
                            if (Audio != null && Audio.ForceSync && Audio.IsPlaying) {
                                Audio.SourceTime = currentTime - Audio.Channel.TimeOffsetWorld;
                            }
                        }
                        /// Audio sync accuracy highly depends on hardware. To correct for audio
                        /// drifting out of sync with the scene time, a tolerance amount is used to
                        /// bring the scene time back in alignment with the audio. This can cause
                        /// skipping if tolerance is set too low and/or the audio hardware cannot
                        /// provide acccurate timing (this has been seen on some android devices). If
                        /// audio sync is disabled in Timeflow, synchronization should still occur when
                        /// using optimal hardware for realtime playback.

                        if (Audio != null && Audio.ForceSync && Audio.IsPlaying && currentTime > Audio.Channel.TimeOffsetWorld) {
                            float time = Audio.SourceTime + Audio.Channel.TimeOffsetWorld;
                            float d = Mathf.Abs(time - currentTime);
                            if (d >= Audio.SyncTolerance) {
                                /// force Timeflow to match the audio playhead time
                                currentTime = Audio.SourceTime;
                            }
                        }

                        SetTime(currentTime);
                    }
                }
                if (Application.isPlaying) {
                    UpdateGroups();
                }

                if (IsPlaying) {
                    UpdateChildren();
                }
            }

            _frame++;
        }

        /// <summary>
        /// Override to pass late update to any behaviors that implement it. This is done on the Always
        /// call so as not to block the call for other objects.
        /// </summary>
        public override void OnFinalUpdate()
        {
            if (!HasTimeflowParent || Active == this) {
                LateUpdateGroups();
                LateUpdateChildren();
            }
            base.OnFinalUpdate();
        }

        /// <summary>
        /// This is called by the base class upon FixedUpdate() or when simulating fixed update in edit
        /// mode. Update calls are passed down through all behaviors, which decide to update based on their
        /// own settings.
        /// </summary>
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (!HasTimeflowParent || Active == this) {
                FixedUpdateGroups();
                FixedUpdateChildren();
            }
        }

        public void UpdateChildren()
        {
            //if (DebugEnabled) Debug.Log($"{name}.UpdateChildren");
            if (TimeflowChildren == null) return;
            foreach (Timeflow child in TimeflowChildren) {
                if (child == null) continue;
                if (child != Active) {
                    float trackOn = child.Track.InterpolateValue(CurrentTime, true, false);
                    if (trackOn > 0f) {
                        float t = CurrentTime - child.TimeOffsetWorld;
                        if (child.LoopInParent) {
                            t = MathUtil.Loop(t, child.TimeOffsetWorld, child.EndTime + child.TimeOffsetWorld);
                        }
                        if (t >= StartTime) {
                            child.SetTime(t);
                            child.UpdateGroups();
                            child.UpdateChildren();
                        }
                    }

                }
            }
        }

        public void LateUpdateChildren()
        {
            if (TimeflowChildren == null) return;
            foreach (Timeflow child in TimeflowChildren) {
                if (child == null) continue;
                child.LateUpdateGroups();
            }
        }

        public void FixedUpdateChildren()
        {
            if (TimeflowChildren == null) return;
            foreach (Timeflow child in TimeflowChildren) {
                if (child == null) continue;
                child.FixedUpdateGroups();
            }
        }

        #endregion

#if UNITY_EDITOR

        public static void EditorSyncUpdateAll()
        {
            if (Application.isPlaying) return;
            //Debug.Log($"EditorSyncUpdateAll");
            if (Timeflow.Instances == null || Timeflow.Instances.Count == 0) return;
            foreach (Timeflow tf in Timeflow.Instances) {
                if (tf == null) continue;
                tf.EditorSyncUpdate();
            }
        }

        private float lastDirectorTime = -1f;

        /// <summary>
        /// This updates the time when playing back in the editor. Instead of using Update to refresh, this
        /// uses a callback to hook into Unity's updating, which happens once per frame (instead of 100
        /// times per frame as with EditorUpdate).
        /// </summary>
        public void EditorSyncUpdate()
        {
            //Debug.Log($"{name}.EditorSyncUpdate");
            if (Application.isPlaying) return;
            if (!IsActive && HasTimeflowParent) return;

            float t = CurrentTime;
            bool doUpdate = false;
            if (DirectorSyncEnabled && Director != null) {// && Director.state == UnityEngine.Playables.PlayState.Playing
                // If controlled by a director, then the director will update the time
                t = DirectorTime;
                //Debug.Log($"{name}.EditorSyncUpdate DirectorTime:{t}");
                if (t != lastDirectorTime) {
                    lastDirectorTime = t;
                    doUpdate = true;
                }
            }
            else
            if (IsPlaying && (IsActive || !HasTimeflowParent)) {
                TimeSpan elapsed = Stopwatch.Elapsed;
                float e = (float)elapsed.TotalSeconds * Time.timeScale * TimeScale;
                t = SceneStartedAtTime + (IsPlayReverse ? -e : e);
                doUpdate = true;
            }

            if (doUpdate) {
                if (View != null) View.EditorSyncUpdate();
                // Setting the time forces an update that ripples through all Timeflow objects
                bool hasLooped = false;
                float lt = GetLoopedTime(t, out hasLooped);
                if (t != lt) {
                    ResetElapsedTime(false);
                }
                if (hasLooped) OnLooped();

                //Debug.Log($"{name}.EditorSyncUpdate t:{t} lt:{lt} hasLooped:{hasLooped}");
                SetTime(lt);

                UpdateGroups();
                EditorUpdateFrame();
            }
        }

        private void EditorUpdateFrame()
        {
            if (!Application.isPlaying) {

                // Simulate fixed update time in the editor for more accurate previews
                if (CurrentTime >= nextFixedUpdateTime || nextFixedUpdateTime == 0) {
                    nextFixedUpdateTime = CurrentTime + Time.fixedDeltaTime;
                    OnFixedUpdate();
                }


                // Late update simulated in edit mode
                LateUpdate();
            }
            if (IsPlaying && View.FollowPlayhead) {
                View.ScrollFollowPlayheadUpdate();
            }
        }

        public override void ForceUpdate()
        {
            FrameID++;
            DoUpdate();
            OnFinalUpdate();
            OnFixedUpdate();
        }
#endif
    }

}//AxonGenesis


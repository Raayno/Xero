// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace AxonGenesis
{
    /// <summary>
    /// This is a utility for working with VideoPlayer to synchronize video with Timeflow. Please note that
    /// the capabilites of this behavior are limited by the VideoPlayer API and implementation of video in
    /// Unity and may also be affected by the video codec in use. Therefore frame accurate synchronization
    /// may not be possible and should not be expected. Note also that this behavior does not have any
    /// channels in the Timeflow view and must be applied on an object with a Video Player component.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [RequireComponent(typeof(VideoPlayer))]
    [AddComponentMenu("Timeflow/Video Player Update")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/video-player-update")]
    sealed public class VideoPlayerUpdate : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC

        public VideoPlayer VideoPlayer;
        public float StartAtTimeInVideo;

        #endregion

        public bool IsEnabled => VideoPlayer != null && VideoPlayer.gameObject.activeInHierarchy;

        public bool IsPlaying {
            get {
                return VideoPlayer == null ? false : VideoPlayer.isPlaying;
            }
        }

        public float StartTime {
            get {
                float t = 0f;
                if (ParentObject != null) {
                    t += ParentObject.StartTime;
                }
                return t;
            }
        }

        /// <summary>
        /// Gets the local time within the video using the current behaviors local time with applied
        /// loopoing if loop is enabled for the video. This also supports time offets set in the Timeflow
        /// view on the object.
        /// </summary>
        public float GetVideoTime()
        {
            if (!IsEnabled) return 0f;
            float time = CurrentTime + StartAtTimeInVideo;
            if (VideoPlayer.isLooping) {
                time = MathUtil.Loop(time, StartAtTimeInVideo, (float)VideoPlayer.length);
            }
            else {
                if (time > (float)VideoPlayer.length) {
                    time = (float)VideoPlayer.length;
                    if (VideoPlayer.isPlaying) VideoPlayer.Stop();
                }
                else
                if (time < StartAtTimeInVideo) {
                    time = StartAtTimeInVideo;
                    if (VideoPlayer.isPlaying) VideoPlayer.Stop();
                }
            }
            if (time < 0f) {
                time = 0f; // just in case
                if (VideoPlayer.isPlaying) VideoPlayer.Stop();
            }
            //if (DebugEnabled) Debug.Log($"{name}.GetVideoTime:{time} CurrentTime:{CurrentTime} len:{VideoPlayer.length}");
            return time;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            VideoPlayer = GetComponent<VideoPlayer>();
            if (VideoPlayer != null) {
                /// Disable play on awake so that this behavior can control when it starts
                VideoPlayer.playOnAwake = false;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (Enabled && VideoPlayer != null) {
                VideoPlayer.Prepare();
            }
        }

        public override void OnPlay()
        {
            base.OnPlay();
            if (!IsEnabled) return;
            if (VideoPlayer == null || VideoPlayer.length == 0) return;

            float time = GetVideoTime();
            //if (DebugEnabled) Debug.Log(name + ".VideoPlayerUpdate.OnPlay:" + time);

            SyncVideo(time);
        }

        public override void OnStop()
        {
            base.OnStop();
            if (!IsEnabled) return;
            /// Use Pause to hold the last frame
            Pause();
        }

        public void Pause()
        {
            if (!IsEnabled) return;
            if (Enabled && VideoPlayer != null) {
                //if (DebugEnabled) Debug.Log(name + ".VideoPlayerUpdate.Pause");
                VideoPlayer.Pause();
            }
        }

        /// <summary>
        /// Syncs video at current time.
        /// </summary>
        private void SyncVideo()
        {
            if (!IsEnabled) return;
            SyncVideo(GetVideoTime());
        }

        float syncTime = 0f;
        const float syncFreq = 1f;

        /// <summary>
        /// Updates the video time and starts playback if not already started and the time is in range.
        /// </summary>
        private void SyncVideo(float time)
        {
            if (!IsEnabled) return;
            //if (DebugEnabled) Debug.Log($"SyncVideo:{time} isPlaying:{VideoPlayer.isPlaying}");
            if (Timeflow.IsPlaying) {
                if (!IsPlaying) {
                    /// Make sure video is playing if it is within time range
                    if (VideoPlayer.isLooping) {
                        VideoPlayer.Prepare();
                        VideoPlayer.Play();
                    }
                    else {
                        if (time > StartAtTimeInVideo && time < VideoPlayer.length) {
                            VideoPlayer.Prepare();
                            VideoPlayer.Play();
                        }
                    }
                }
                else
                if (Time.time > syncTime) {
                    syncTime = Time.time + syncFreq;
                    if (!Mathf.Approximately(time, (float)VideoPlayer.time)) {
                        /// Synchronize video player with current time
                        VideoPlayer.time = time;
                        VideoPlayer.StepForward();
                        if (IsPlaying) {
                            VideoPlayer.Play();
                        }
                        //if (DebugEnabled) Debug.Log($"VideoPlayer.time:{VideoPlayer.time} time:{time}");
                    }
                }
            }
            else {
                if (IsPlaying) {
                    VideoPlayer.Pause();
                }
                VideoPlayer.time = time;
                VideoPlayer.StepForward();
            }
        }

        public override void UpdateTime()
        {
            if (!IsEnabled) return;
            if (!CanUpdate) return;
            base.UpdateTime();

            if (VideoPlayer != null && VideoPlayer.length > 0) {
                bool isTrackOn = ParentObject.Track.IsTrackOn(ParentObject.CurrentTime);

                if (isTrackOn) {
                    SyncVideo();
                }
                else {
                    /// Only play video during the track sections
                    if (VideoPlayer.isPlaying) VideoPlayer.Stop();
                }
            }
        }

#if UNITY_EDITOR

        public override Texture2D Icon => AxonUI.Icons.VideoPlayerUpdate;

        /// <summary>
        /// Prevents component reference from being listed in property lists, since there's nothing to
        /// animate here.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Video Player Update"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<VideoPlayerUpdate>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }

}//AxonGenesis

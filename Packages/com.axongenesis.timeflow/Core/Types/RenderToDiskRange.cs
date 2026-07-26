// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "RenderToDiskRange")]
    public class RenderToDiskRange : SerializableObject
    {
        public string OutputPath = null;
        public string FileExtension = null;
        public float FrameRate = 60f;

        public string NamePrefix = "";
        public string Name = "";
        public bool EnableRender = true;

        [FormerlySerializedAs("StartFrame")]
        public int _OldStartFrame = -1;

        [FormerlySerializedAs("EndFrame")]
        public int _OldEndFrame = -1;

        public TimeValue StartTime = null;
        public TimeValue EndTime = null;

        public int FrameHandles = 0;
        public bool PerfectLoop = false;
        public bool PreRoll = false;
        public int PreRollStartFrame = 0;
        public bool RenumberFrames = false;
        public int RenumberStartingAt = 0;
        public int FrameNumberPadding = 4;
        public bool EditorShowSettings = true;

        #region VIDEO SETTINGS
        public bool EnableVideoEncoding = false;
        public VideoEncoding Encoding = null;
        public bool AutoOutputPath = true;
        public string VideoOutputPath = null;
        public string VideoFilename = null;
        public bool AutoVideoFilepath = true;
        public bool ShowProcessWindow = false;
        public string Metadata = null;
        public bool AutoMetadata = true;
        public bool OverwriteVideo = true;
        public bool VideoHasAudio = false;
        public bool AutoAudioStartTime = true;
        public float AudioStartTime = 0;
        public string AudioFilepath = null;

        [NonSerialized]
        public VideoQueueItem VideoItem = null;
        #endregion

        public void InitRange(bool isNew, RenderToDiskRange copy = null)
        {
            if (copy != null) {
                StartTime = new TimeValue(copy.StartTime, null);
            }
            else
            if (isNew || StartTime == null || StartTime.IsUninitialized) {
                StartTime = new TimeValue();
                StartTime.TimeType = TimeValue.TimeTypes.Marker;
                StartTime.Marker = 0;
            }

            if (copy != null) {
                EndTime = new TimeValue(copy.EndTime, null);
            }
            else
            if (isNew || EndTime == null || EndTime.IsUninitialized) {
                EndTime = new TimeValue();
                EndTime.TimeType = TimeValue.TimeTypes.Marker;
                EndTime.Marker = -1;
            }

            if (_OldStartFrame > -1) {
                // Timeflow v1.4.0 added the TimeValue formats, so this upgrades the old
                // frame numbers to preserve ranges saved in prior versions.
                if (StartTime == null || StartTime.IsUninitialized) StartTime = new TimeValue();
                if (EndTime == null || EndTime.IsUninitialized) EndTime = new TimeValue();

                StartTime.TimeType = TimeValue.TimeTypes.Frames;
                StartTime.Frame = _OldStartFrame;

                EndTime.TimeType = TimeValue.TimeTypes.Frames;
                EndTime.Frame = _OldEndFrame;

                _OldStartFrame = -1;
                _OldEndFrame = -1;
            }
        }

        public int RenumberFrameStart {
            get {
                if (RenderQueue.ForceFrameRenumbering) {
                    return RenderQueue.ForceFrameRenumberingStart;
                }
                if (RenumberFrames) {
                    return RenumberStartingAt;
                }
                return StartTime.Frame;
            }
        }

        #region VIDEO ENCODING

        public void SetupVideoEncoding()
        {
            if (!EnableVideoEncoding) return;
            if (Encoding == null) {
                Debug.LogWarning("No video encoding has been assigned.");
                return;
            }
            string path = OutputPath + $"{NamePrefix}{Name}_%0{FrameNumberPadding}d" + FileExtension;

            float audioStart = 0f;
            if (VideoHasAudio && AutoAudioStartTime) {
                audioStart = AudioStartTime + ((float)StartTime.Frame / FrameRate);
            }

            if (AutoVideoFilepath || string.IsNullOrEmpty(VideoFilename)) {
                VideoFilename = NamePrefix + Name + Encoding.Extension;
            }

            VideoItem = new VideoQueueItem {
                InputPath = path,
                Encoding = Encoding,
                ShowProcessWindow = ShowProcessWindow,
                Metadata = Metadata,
                OutputPath = VideoOutputPath,
                OutputName = VideoFilename,
                Overwrite = OverwriteVideo,
                Framerate = FrameRate,
                StartingFrameNumber = RenumberFrameStart,
                OutputAudio = VideoHasAudio,
                AudioStartTime = audioStart,
                AudioPath = AudioFilepath
            };

            VideoItem.BuildCommand();
        }

        public void QueueVideoEncoding()
        {
            SetupVideoEncoding();
            if (VideoItem != null) {
                VideoItem.BuildCommand();
                VideoQueue.Instantiate();
                VideoQueue.AddItem(VideoItem);
            }
            else {
                Debug.LogWarning("Video Item is NULL.");
            }
        }

        public float GetDuration()
        {
            if (_OldStartFrame > -1) {
                InitRange(false); // Update legacy frame range
            }
            StartTime.Calculate();
            EndTime.Calculate();

            return EndTime.Time - StartTime.Time;
        }

        #endregion

    }
}//AxonGenesis
#endif

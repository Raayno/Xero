// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor.Timeline;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This class is used by TimeflowControlTrack to interpolate and update the attached Timeflow.
    /// </summary>
    public class TimeflowControlMixerBehaviour : PlayableBehaviour
    {
        [NonSerialized]
        private Timeflow Timeflow;

        [NonSerialized]
        private bool doSetup = true;

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
#if UNITY_EDITOR
            if (Timeflow != null) {
                /// Remove delegate assignment
                Timeflow.OnEditorDirectorUpdate -= OnEditorDirectorUpdate;
            }
#endif
        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            doSetup = true;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Timeflow = playerData as Timeflow;
            if (Timeflow == null) return;

            if (Timeflow.Director == null) {
                Timeflow.SetupDirector();
            }

            if (Timeflow.Director != null) {

                if (doSetup) {
                    doSetup = false;
#if UNITY_EDITOR
                    // Delegate to force refresh the Timeline window when Timeflow updates. This doesn't
                    // get assigned on create since Timeflow may be null.
                    Timeflow.OnEditorDirectorUpdate -= OnEditorDirectorUpdate;
                    Timeflow.OnEditorDirectorUpdate += OnEditorDirectorUpdate;
#endif
                }

                bool isClipPlaying = false;

                // Gets the starting time of the clip in the Timeline window to offset the relative
                // time in the Timeflow view. This allows Timeline to sequence Timeflow instances.
                TimelineAsset ta = Timeflow.Director.playableAsset as TimelineAsset;
                if (ta != null) {
                    foreach (TrackAsset track in ta.GetOutputTracks()) {
                        if (track.muted) continue;

                        // Supports multiple clips, each starting from 0 or a specific time in Timeflow.
                        // Clip blending is not supported since there is no way to blend whole timelines.
                        TimeflowControlTrack timeflowTrack = track as TimeflowControlTrack;
                        if (timeflowTrack == null || timeflowTrack.Timeflow != Timeflow) {
                            continue;
                        }

                        // Set the length of each clip to match its Timeflow instance
                        int clipIndex = 0;
                        foreach (TimelineClip clip in track.GetClips()) {
                            //Debug.Log($"timeflowTrack:{timeflowTrack.ID} clip:{clip.displayName}");
                            TimeflowControlClip ca = (TimeflowControlClip)clip.asset;
                            if (ca != null) {
                                ca.TimeflowDuration = Timeflow.Duration;
                            }
                            if ((Timeflow.Director.time >= clip.start && Timeflow.Director.time <= clip.end)) {
                                ScriptPlayable<TimeflowControlBehaviour> inputPlayable = (ScriptPlayable<TimeflowControlBehaviour>)playable.GetInput(clipIndex);
                                TimeflowControlBehaviour input = inputPlayable.GetBehaviour();

                                if (input.AutoStartTime) input.StartTime = 0;

                                isClipPlaying = true;
#if UNITY_EDITOR
                                if (input.ActivateTimeflow) Timeflow.Active = Timeflow;
#endif
                                // Adjust the time offsets to map the clip range into the Timeflow range
                                Timeflow.DirectorTimeStart = (float)clip.start - input.StartTime;
                                Timeflow.DirectorTimeEnd = Timeflow.DirectorTimeStart + (float)clip.duration;
                                Timeflow.DirectorTime = (float)Timeflow.Director.time - Timeflow.DirectorTimeStart;
                            }
                            //UnityEngine.Debug.Log($"t:{Timeflow.Director.time} Timeflow.DirectorTime:{Timeflow.DirectorTime} clip.start:{clip.start} DirectorTimeStart:{Timeflow.DirectorTimeStart}");
                            clipIndex++;
                        }
                        break;
                    }
                }
                if (isClipPlaying) {
                    // Only update the time if a clip is playing (weight > 0)
                }
                else
                if (Timeflow.IsPlaying) {
                    Timeflow.Stop(false);
                }
            }
        }
#if UNITY_EDITOR

        /// <summary>
        /// The Timeline view doesn't automatically refresh so we need to inform it when Timeflow updates.
        /// </summary>
        private static void OnEditorDirectorUpdate()
        {
            TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
        }
#endif
    }

}//AxonGenesis

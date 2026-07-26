// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR


using System.Diagnostics;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        public void GotoPrevious()
        {
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToPreviousMarker)) {
                TimeflowCommands.GotoPreviousMarker();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToPreviousSnapTime)) {
                TimeflowCommands.GotoPreviousSnapTime();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToPreviousKeyframe)) {
                TimeflowCommands.GotoPreviousKeyframe();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToPreviousFrame)) {
                TimeflowCommands.GotoPreviousFrame();
            }
        }

        public void GotoNext()
        {
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToNextMarker)) {
                TimeflowCommands.GotoNextMarker();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToNextSnapTime)) {
                TimeflowCommands.GotoNextSnapTime();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToNextKeyframe)) {
                TimeflowCommands.GotoNextKeyframe();
            }
            else
            if (TimeflowShortcuts.IsModifierPressed(TimeflowShortcutInfo.Path_GoToNextFrame)) {
                TimeflowCommands.GotoNextFrame();
            }
        }

        public void GotoPreviousSnap()
        {
            Timeflow.Interrupt();
            Timeflow.CurrentTimeExact = SnapTime(Timeflow.CurrentTime - Snap, true);
        }

        public void GotoNextSnap()
        {
            Timeflow.Interrupt();
            Timeflow.CurrentTimeExact = SnapTime(Timeflow.CurrentTime + Snap, true);
        }

        public void GotoPreviousCustomStep()
        {
            Timeflow.Interrupt();
            Timeflow.CurrentTimeExact = Timeflow.CurrentTime - TimeflowPreferences.Current.CustomTimeStep;
        }

        public void GotoNextCustomStep()
        {
            Timeflow.Interrupt();
            Timeflow.CurrentTimeExact = Timeflow.CurrentTime + TimeflowPreferences.Current.CustomTimeStep;
        }

        public void GotoPreviousKeyframe()
        {
            Timeflow.Interrupt();

            float time = GetPrevNextKeyframeTimeOfAllDisplayed(false);
            Timeflow.CurrentTimeExact = time;
        }

        public void GotoNextKeyframe()
        {
            Timeflow.Interrupt();

            float time = GetPrevNextKeyframeTimeOfAllDisplayed(true);
            Timeflow.CurrentTimeExact = time;
        }

        public float GetPrevNextKeyframeTimeOfAllDisplayed(bool isForward)
        {
            float current = Timeflow.CurrentTime;
            float time = isForward ? Timeflow.EndTime : Timeflow.StartTime;
            if (IsGraphMode) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && obj.BehaviorsEnabled) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && !ch.IsTrack && (ch.IsSelected || ch.IsGraphLocked)) {
                                foreach (Keyframe k in ch.Keys) {
                                    float keyTimeWorld = k.KeyTimeWorld;
                                    if (isForward && keyTimeWorld > current && keyTimeWorld < time) {
                                        time = keyTimeWorld;
                                    }
                                    else
                                    if (!isForward && keyTimeWorld < current && keyTimeWorld > time) {
                                        time = keyTimeWorld;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && obj.BehaviorsEnabled) {
                        foreach (Keyframe k in Timeflow.Track.Keys) {
                            float keyTimeWorld = k.KeyTimeWorld;
                            if (isForward && keyTimeWorld > current && keyTimeWorld < time) {
                                time = keyTimeWorld;
                            }
                            else
                            if (!isForward && keyTimeWorld < current && keyTimeWorld > time) {
                                time = keyTimeWorld;
                            }
                        }

                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.IsEnabled && ch.Keys != null && ch.Keys.Count > 0) {
                                foreach (Keyframe k in ch.Keys) {
                                    float keyTimeWorld = k.KeyTimeWorld;
                                    if (isForward && keyTimeWorld > current && keyTimeWorld < time) {
                                        time = keyTimeWorld;
                                    }
                                    else
                                    if (!isForward && keyTimeWorld < current && keyTimeWorld > time) {
                                        time = keyTimeWorld;
                                    }
                                }
                            }
                        }
                        if (obj.Events != null && obj.Events.Count > 0) {
                            foreach (TimeflowEvent e in obj.Events) {
                                if (e.Enabled) {
                                    float keyTimeWorld = e.TriggerTimeWorld;
                                    if (isForward && keyTimeWorld > current && keyTimeWorld < time) {
                                        time = keyTimeWorld;
                                    }
                                    else
                                    if (!isForward && keyTimeWorld < current && keyTimeWorld > time) {
                                        time = keyTimeWorld;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return time;
        }
    }

}//AxonGenesis
#endif
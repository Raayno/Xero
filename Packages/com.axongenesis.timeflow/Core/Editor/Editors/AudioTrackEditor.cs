// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static AxonGenesis.AudioTrack;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioTrack))]
    public class AudioTrackEditor : AxonGenesisEditor<AudioTrack, AudioTrackEdit>
    {
    }
    sealed public class AudioTrackEdit : AxonGenesisBehaviorEdit<AudioTrack>
    {
#if TIMEFLOW_PRO
        public const string kAddAudioTrack = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🔉 Audio Track";
#else
        public const string kAddAudioTrack = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Audio Track";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Audio Track";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAudioTrack, false, 160)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAudioTrack, false, 160)]
        public static void AddAudioTrack()
        {
            GameObject obj = new GameObject("Audio Track");
            UndoUtil.UndoCreate(obj, "Add Audio Track");

            AudioTrack instance = AudioTrack.Instance;
            AudioTrack track = ObjectUtil.AddComponent<AudioTrack>(obj);

            if (instance != null) {
                track.SyncMode = SyncModes.SynchronizeToAudioTrack;
                track.SyncToTrack = instance;
            }
            else {
                track.SyncMode = SyncModes.SynchronizeTimeflow;
                if (track.Timeflow != null) {
                    track.Timeflow.Audio = track;
                }
            }

            if (track.Timeflow != null) {
                obj.transform.SetParent(track.Timeflow.gameObject.transform);
                obj.transform.SetAsFirstSibling();
            }
            ObjectUtil.ResetTransform(obj);
            SelectionUtil.Select(obj);
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        private TimeflowObject timelineObj;

        public AudioTrackEdit() { }

        public AudioTrackEdit(AudioTrack _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-track";
        }

        public override void GUISetup()
        {
            base.GUISetup();

            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            if (timelineObj == null) target.TryGetComponent<TimeflowObject>(out timelineObj);
        }

        public override void GUIMenu()
        {
            if (AxonGUI.ButtonInline("Play")) {
                target.Source.Play();
            }
            if (AxonGUI.ButtonInline("Stop")) {
                target.Source.Stop();
            }
            if (AxonGUI.ButtonInline(" |< ")) {
                target.Source.time = 0f;
            }
            if (AxonGUI.ButtonInline(" << ")) {
                target.Source.time -= 3f;
            }
            if (AxonGUI.ButtonInline(" >> ")) {
                target.Source.time += 3f;
            }
            if (AxonGUI.ButtonInline(" >| ")) {
                target.Source.time = target.Source.clip.length;
            }
            if (AxonGUI.ButtonInline("Set Timeflow Length")) {
                if (target.Parent != null) {
                    target.Parent.EndTime = target.Source.clip.length;
                    target.Parent.ResetTrack();
                    target.Parent.SetupChannels(true);
                }
                target.ParentObject.ResetTrack();
            }
            AxonGUI.UndoName = "Set Mute";
            target.Mute = AxonGUI.FieldToggleInline(target, "Mute", target.Mute);
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Audio Source";
            target.Source = AxonGUI.FieldObject(target, "Audio Source", target.Source, typeof(AudioSource), true) as AudioSource;
            AxonGUI.BeginHorizontal();
            if (target.Source != null) {
                AxonGUI.UndoName = "Set Audio Clip";
                AxonGUI.SetTooltip("Sets the audio input using a built-in AudioSource.");
                target.Source.clip = AxonGUI.FieldObject(target, "Audio Clip", target.Source.clip, typeof(AudioClip), true) as AudioClip;
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Sync Mode";
            AxonGUI.SetTooltip("Synchronization corrects any slippage in time to ensure that audio and animation stay in perfect time. Otherwise small time variances can accumulate over time causing the animation and audio to be out of synch with each other.");
            AudioTrack.SyncModes syncMode = (AudioTrack.SyncModes)AxonGUI.FieldEnumPopup(target, "Synchronization", target.SyncMode);
            if (target.SyncMode != syncMode) {
                target.SyncMode = syncMode;
                if (target.SyncMode == SyncModes.SynchronizeTimeflow) {
                    if (target.Timeflow != null) target.Timeflow.Audio = target;
                }
                else
                if (target.Timeflow != null && target.Timeflow.Audio == target) target.Timeflow.Audio = null;
            }

            if (target.SyncMode == SyncModes.SynchronizeToAudioTrack) {
                AxonGUI.UndoName = "Set Sync to Audio Track";
                AxonGUI.SetTooltip("If the scene uses more than one AudioTrack, assign the primary track to sync with as the Master. This is optional and can be left null.");
                target.SyncToTrack = AxonGUI.FieldObjectInline(target, target.SyncToTrack, typeof(AudioTrack), true) as AudioTrack;
            }
            if (target.SyncMode != SyncModes.NoSynchronization) {
                AxonGUI.UndoName = "Set Sync Tolerance";
                AxonGUI.SetTooltip("This specifies the time in seconds that time may slip before snapping back to sync. Some audio devices may not keep accurate time and too low of a setting may cause skipping.");
                target.SyncTolerance = AxonGUI.FieldFloatInline(target, "Tolerance", target.SyncTolerance, GUILayout.Width(140));
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Use Global Time Scale";
            AxonGUI.SetTooltip("If enabled, this pitches the audio up or down to match the current time scale, defined by Time.timesScale (also in the Timeflow settings as Time Scale). If disabled, audio plays at normal speed no matter what the current time scale is.");
            target.LinkPitchToTimeScale = AxonGUI.FieldToggle(target, "Use Global Time Scale", target.LinkPitchToTimeScale);
            AxonGUI.UndoName = "Set Local Time Scale";
            target.LocalTimeScale = AxonGUI.FieldFloatInline(target, "Local Time Scale", target.LocalTimeScale);
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Enable to display the audio track as a channel in the Timeflow view.");
            AxonGUI.UndoName = "Set Show in Timeflow";
            target.Channel.IsHidden = !AxonGUI.FieldToggle(target, "Show in Timeflow", !target.Channel.IsHidden);
            if (!target.Channel.IsHidden) {
                AxonGUI.UndoName = "Set Draw Waveform";
                AxonGUI.SetTooltip("Enables drawing the waveform preview. Disable this to reduce draw overhead in the window.");
                target.Channel.GUICanDraw = AxonGUI.FieldToggleInline(target, "Draw Waveform", target.Channel.GUICanDraw);
                if (target.Channel.GUICanDraw) {
                    AxonGUI.UndoName = "Set Scale";
                    target.WaveformScale = AxonGUI.FieldSliderInline(target, "Scale", target.WaveformScale, 0.01f, 10f);

                    AxonGUI.UndoName = "Set Draw Stereo";
                    target.WaveformStereo = AxonGUI.FieldToggleInline(target, "Draw Stereo", target.WaveformStereo);
                }
                AxonGUI.UndoName = "Set Audio Track Color";
                target.AudioTrackColor = AxonGUI.FieldColorInline(target, target.AudioTrackColor, false);
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            if (GUI.changed) {
                if (target.Attack < 0) target.Attack = 0f;
                if (target.Decay < 0) target.Decay = 0f;
                if (target.Sustain < 0) target.Sustain = 0f;
                if (target.Release < 0) target.Release = 0f;
            }
        }
    }

}//AxonGenesis

#endif
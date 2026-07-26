// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AudioTrackChannel")]
    sealed public class AudioTrackChannel : TimeflowChannel
    {
        [NonSerialized]
        public AudioTrack AudioParent;

        public AudioTrackChannel(AudioTrack parent) : base(parent)
        {
            SetParent(parent);
        }

        public override void SetParent(TimeflowBehavior parent)
        {
            base.SetParent(parent);
            AudioParent = parent as AudioTrack;
        }

        public override string Name {
            get {
                return AudioParent.Name;
            }
            set {
                AudioParent.Name = value;
            }
        }

        public override void OnSetup(TimeflowBehavior parent)
        {
            base.OnSetup(parent);

            PropertyType = Property.PropertyTypes.Float;
            SupportsKeyframes = false;
            CanAddRemoveKeys = false;
            IsDataOnly = true;
            LimitValue = true;
            MinValue = Vector4.zero;
            MaxValue = Vector4.one;
        }

        public override bool SupportsKeyframes {
            get {
                return false;
            }
        }

        public override void Interpolate(float time, bool apply, bool isLocalTime)
        {
            float value = 1f;
            if (Keys != null && Keys.Count > 0) {
                value = InterpolateValue(time, apply, isLocalTime);
            }
            if (AudioParent != null && AudioParent.Source != null) {
                AudioParent.Source.volume = value;
            }
            if (apply) {
                ToProperty.FloatValue = value;
            }
        }

        public override float WorldTime()
        {
            float time = CurrentTime - TimeOffsetWorld;

            time = MathUtil.Loop(time, 0f, AudioParent.ClipEndTime);

            return time;
        }

//#if AXON_EXPERIMENTAL
//        public void ConvertAudioToKeyframes()
//        {
//            if (AudioParent != null) {
//                AudioParent.ConvertAudioToKeyframes();
//            }
//        }
//#endif

#if UNITY_EDITOR

        [NonSerialized]
        public GUIContent MuteToggleLabel;

        [NonSerialized]
        public GUIContent WaveformToggleLabel;

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUIHierarchyControls()
        {
            base.GUIHierarchyControls();

            if (MuteToggleLabel == null) {
                MuteToggleLabel = new GUIContent();
                MuteToggleLabel.tooltip = "Mute Audio";
            }
            if (WaveformToggleLabel == null) {
                WaveformToggleLabel = new GUIContent();
                WaveformToggleLabel.tooltip = "Show Waveform";
            }

            Rect rect = new Rect(GUIRect);
            rect.y = GUIRect.y + 2;

            // Mute Toggle
            rect.x = GUIRect.width - 35;
            rect.width = rect.height = 16;
            rect.y += (GUIRect.height / 2f) - 11f;
            if (GUI.Button(rect, MuteToggleLabel, AudioParent.Mute ? AxonUI.AudioOffStyle : AxonUI.AudioOnStyle)) {
                AudioParent.Mute = !AudioParent.Mute;
            }
        }

        public void GUIDrawWaveform()
        {
            if (GUITrackRect.height == 0 || EditorInput.IsLayout) return;
            GUI.color = GUIColor;

            if (AudioParent.Source != null) {
                AudioClip audio = AudioParent.Source.clip;

                if (audio != null) {
                    GUI.Box(GUITrackRect, GUIContent.none, AxonUI.TrackOffStyle);
                    AudioParent.RenderAudioWaveform(GUITrackRect, 0f, Timeflow.Active.EndTime);
                }
                else {
                    GUI.Box(GUITrackRect, new GUIContent("No audio clip assigned"), AxonUI.TrackOffStyle);
                }
            }
            else {
                GUI.Box(GUITrackRect, new GUIContent("No AudioSource defined"), AxonUI.TrackOffStyle);
            }
        }

        public override void GUIChannel()
        {
            if (!IsHidden && (GUICanDraw || IsGraphLocked) && Timeflow.Active != null) {
                //float w = Timeflow.Active.View.TimeToViewScale(AudioParent.ClipEndTime);
                float start = Timeflow.Active.View.PositionOfTime(AudioParent.StartAtTime, true);
                float end = Timeflow.Active.View.PositionOfTime(AudioParent.ClipEndTime, true);
                GUITrackRect = new GUIRect(start, GUIRect.y, end - start, GUIRect.height);
                GUIDrawWaveform();
            }
        }

        public override void GUIGraphPass1()
        {
            if (!IsHidden && Timeflow.Active != null && IsSelected) {
                float w = Timeflow.Active.View.TimeToViewScale(AudioParent.ClipEndTime);
                GUITrackRect = new GUIRect(Timeflow.Active.View.PositionOfTime(AudioParent.StartAtTime, true), GUIRect.y, w, GUIRect.height);
                GUIDrawWaveform();
            }
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {

//#if AXON_EXPERIMENTAL
//            menu.AddItem(new GUIContent("Convert Audio to Keyframes"), false, ConvertAudioToKeyframes);
//#endif
        }

        public override bool GUIDragAndHover()
        {
            bool handled = false;

            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                if (obj.GetType() == typeof(AudioClip)) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    handled = true;
                    break;
                }
            }

            return handled;
        }

        public override bool GUIDragAndDrop(List<TimeflowObject> objects)
        {
            bool handled = false;

            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                if (obj.GetType() == typeof(AudioClip)) {
                    AudioClip clip = obj as AudioClip;
                    AudioParent.Source.clip = clip;
                    handled = true;
                    break;
                }
            }

            return handled;
        }

#endif
    }

}//AxonGenesis
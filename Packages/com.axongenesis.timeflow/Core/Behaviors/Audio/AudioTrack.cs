// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Audio Track")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-track")]
    sealed public class AudioTrack : TimeflowBehavior
    {
        public static AudioTrack Instance { get; private set; } // Reference to the primary audio track only

        #region PUBLIC

        public AudioTrackChannel Channel;
        public AudioSource Source;
        public AudioTrack SyncToTrack;

        public enum SyncModes
        {
            NoSynchronization,
            SynchronizeTimeflow,
            SynchronizeToAudioTrack
        }
        public SyncModes SyncMode = SyncModes.SynchronizeTimeflow;
        public float SyncTolerance = 0.1f; // If 0, forces exact time, otherwise allows time to be slightly off.

        //public float EndAtTime;
        public bool LinkPitchToTimeScale = true;

        [SerializeField]
        private float _LocalTimeScale = 1f;

        public bool UseEnvelope = true;
        public float Attack;
        public float Decay;
        public float Sustain = 1f;
        public float Release;

        [SerializeField] private string _Name;

        #endregion

        #region NON-SERIALIZED

        [NonSerialized]
        public int AudioChannels = 1;

        [NonSerialized]
        private Timeflow _parent;

        #endregion

        #region ACCESSORS

        public Timeflow Parent {
            get {
                if (_parent == null) _parent = Timeflow.Active;
                return _parent;
            }
            set {
                _parent = value;
                if (_parent == null) {
                    _parent = Timeflow.Active;
                    if (_parent == null) {
                        Debug.LogWarning("AudioTrack requires a Timeflow parent. Please make this object a child of a Timeflow instance.");
                    }
                }
            }
        }

        public override string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) {
                    if (Source != null && Source.clip != null) _Name = Source.clip.name;
                    else _Name = "Audio";
                }
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public bool IsPrimaryAudio {
            get {
                return Timeflow != null && Timeflow.Audio == this;
            }
        }

        public bool ForceSync {
            get {
                return SyncMode == SyncModes.SynchronizeTimeflow && IsPrimaryAudio;
            }
        }

        public float LocalTimeScale {
            get {
                if (_LocalTimeScale <= 0f) _LocalTimeScale = 0.001f;
                return _LocalTimeScale;
            }
            set {
                if (_LocalTimeScale != value) {
                    if (_LocalTimeScale <= 0f) _LocalTimeScale = 0.001f;
                    _LocalTimeScale = value;
                }
            }
        }

        public bool CanPlay {
            get {
                return Source != null && Source.clip != null && !Mute && (Parent != null && Parent.IsPlayingInHierarchy);
            }
        }

        public bool IsPlaying {
            get {
                return Source == null ? false : Source.isPlaying;
            }
        }

        public bool Mute {
            get {
                if (Source != null) return Source.mute;
                return false;
            }
            set {
                if (Source != null && Source.mute != value) {
                    Source.mute = value;
                    if (!IsPlaying && !Source.mute) {
                        PlayAudio();
                    }
                }
            }
        }

        public float SourceTime {
            get {
                if (LocalTimeScale <= 0f) LocalTimeScale = 0.001f;
                if (Source != null) return Source.time / LocalTimeScale;
                return Channel.CurrentTime / LocalTimeScale;
            }
            set {
                if (Source != null && Source.clip != null) {
                    value *= LocalTimeScale;
                    float inVal = value;
                    value = LoopAudioTime(value);
                    //if (DebugEnabled) 
                    //Debug.Log($"inVal:{inVal} time:{value} length:{Source.clip.length}");
                    if (value > Source.clip.length) {
                        value = Source.clip.length;
                    }
                    if (value < SyncTolerance) {
                        Source.time = value;
                        //if (DebugEnabled) Debug.Log("Source.time:" + value + " tolerance:" + SyncTolerance);
                    }
                    else {
                        // To prevent sound distortion, only update if exceeds allowed tolerance
                        float d = Mathf.Abs(Source.time - value);
                        if (d >= SyncTolerance) {
                            //if (DebugEnabled) Debug.Log("AudioTrack.Source inVal:" + inVal + " source:" + Source.time + " time:" + value + " tolerance:" + SyncTolerance + " d:" + d);
                            if (value < 0 || value >= Source.clip.length) value = 0;
                            Source.time = value;
                        }
                        else {
                            //if (DebugEnabled) Debug.Log("Source.time SKIPPING:" + value + " tolerance:" + SyncTolerance + " d:" + d);
                        }
                    }
                }

            }
        }

        public float StartAtTime {
            get {
                if (Channel != null) {
                    return Channel.TimeOffsetWorld;
                }
                return TimeOffsetWorld;
            }
        }

        public float ClipEndTime {
            get {
                float end = 0f;
                //if (EndAtTime > 0f) {
                //    end = EndAtTime - StartAtTime;
                //    if (Source != null && Source.clip != null) {
                //        if (end > Source.clip.length) {
                //            end = Source.clip.length;
                //        }
                //    }
                //}
                //else
                if (Source != null && Source.clip != null) {
                    end = StartAtTime + Source.clip.length;
                }
                return end;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            if (IsPrimaryAudio || Instance == null) {
                Instance = this;
            }

            Parent = GetComponentInParent<Timeflow>();

            bool isNew = false;
            if (Channel == null) {
                Channel = new AudioTrackChannel(this);
                AddChannel(Channel);
                isNew = true;
            }
            Channel.SetParent(this);
            if (Channel.ToProperty == null) {
                Channel.ToProperty = new Property();
                isNew = true;
            }
            if (isNew || string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Audio";
            }
            Channel.IsDataOnly = true;
            Channel.PropertyType = Property.PropertyTypes.Float;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Float;

            if (Source == null) {
                if (!TryGetComponent<AudioSource>(out Source)) {
                    Source = gameObject.AddComponent<AudioSource>();
                }
            }
            if (Source != null) {
                /// playback is controlled by this script rather than the audio source
                Source.playOnAwake = false;
            }
            //if (DebugEnabled) Debug.Log(name + ".AudioTrack.OnAwake");
            base.OnAwake();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (SyncMode == SyncModes.SynchronizeTimeflow && Timeflow != null) {
                if (Timeflow.Audio == null) Timeflow.Audio = this;
            }

            if (Source != null) {
                //if (DebugEnabled) Debug.Log("Timeflow.Audio.OnStart");
                if (Application.isPlaying) {
                    if (Mute) {
                        Source.Stop();
                    }
                    else {
                        float playhead = Channel.WorldTime();
                        if (playhead > 0) {
                            SourceTime = playhead;
                        }
                        else {
                            SourceTime = 0;
                        }
                        if (!Parent.IsPlaying) Source.Stop();
                        //if (DebugEnabled) Debug.Log("Timeflow.Audio.OnStart:" + playhead);
                    }
                }
                else {
                    Source.Stop();
                }
            }
        }

        protected override void OnDestruct()
        {
            TimeflowObject.UnregisterBehavior(this);
            if (Channel != null) {
                RemoveChannel(Channel);
            }
            if (Instance == this) Instance = null;
            base.OnDestruct();
        }


        public override void Refresh()
        {
            base.Refresh();
#if UNITY_EDITOR
            audioData = null;
#endif
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(gameObject, "Copy Audio Track", true);
#endif
            AudioTrack comp = (AudioTrack)src;
            if (comp != null) {
                base.Copy(src, false);
                //if (DebugEnabled) Debug.Log(name + ".AudioTrack.Copy:" + src.name);

                Channel.Copy(comp.Channel);
                Channel.AudioParent = this;

                // Make a copy of the audio source too so that it is fully separate
                TryGetComponent<AudioSource>(out Source);
                if (Source == null && comp.Source != null) {
                    Source = (AudioSource)ObjectUtil.CopyComponent(comp.Source, gameObject);
                }

                if (Source != null && comp.Source != null) {
                    Source.clip = comp.Source.clip;
                }

                // Sync the copy to the original
                SyncMode = SyncModes.SynchronizeToAudioTrack;
                SyncToTrack = comp;
            }
#if UNITY_EDITOR
            Undo.FlushUndoRecordObjects();
#endif
        }

        #endregion

        #region CHANNELS

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (Channel == null) {
                Channel = new AudioTrackChannel(this);
                AddChannel(Channel);
            }
            if (Channel != null) {
                Channel.OnSetup(this);
                Channels = new List<TimeflowChannel>();
                Channels.Add(Channel);
            }
            Channel.AudioParent = this;
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            obj.RegisterChannel(Channel);
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            base.RemoveChannel(channel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            base.RemoveChannelWithUndo(channel);
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#else
            DestroyImmediate(this);
#endif
        }

        #endregion

        #region DATA

        public void LoadSamples()
        {
#if UNITY_EDITOR
            if (Source != null && Source.clip != null) {
                //if (DebugEnabled) Debug.Log("LoadSamples:" + Source.clip.samples);
                EditorUtil.ShowProgress("Loading Samples", "", 0.25f);

                AudioChannels = Source.clip.channels;
                Samples = new float[Source.clip.samples * AudioChannels];

                //getData after the loadType changed
                Source.clip.LoadAudioData();

                EditorUtil.ShowProgress("Loading Samples", "", 0.5f);
                Source.clip.GetData(Samples, 0);

                EditorUtil.ClearProgress();
            }
#endif
        }

        public float GetVolume()
        {
            float volume = 0f;
            if (Source != null) {
                volume = Source.volume;
            }
            return volume;
        }

        public void SetVolume(float volume)
        {
            if (Source != null) {
                Source.volume = volume;
            }
        }

        public override void SetTime(float time)
        {
            if (Source != null) {
                //if (DebugEnabled) Debug.Log(name + ".SetTime:" + time);
                if (ParentObject.Track.IsTrackOn(time) && !IsPlaying) {
                    PlayAudio();
                }
            }
        }

        #endregion

        #region UPDATE

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (!CanUpdate || !CanPlay) {
                if (IsPlaying) StopAudio();
                return;
            }

            float time = channel.CurrentTime;
            //if (DebugEnabled) Debug.Log(name + ".UpdateTimeChannel isPlaying:" + IsPlaying + " time:" + time + " SourceTime:" + SourceTime);
            SourceTime = time;
            Source.pitch = LocalTimeScale * (LinkPitchToTimeScale ? Time.timeScale : 1f) * Timeflow.TimeScale * Channel.TimeScale;

            time *= LocalTimeScale;
            if (time < 0 || (time > Source.clip.length && !Source.loop)) {
                //if (DebugEnabled) Debug.Log("Timeflow.Audio.UpdateTime:Wait time:" + time + " StartAtTime:" + StartAtTime);
                Source.Stop();
            }
            else
            if (!IsPlaying && Parent.IsPlayingInHierarchy && time >= 0) {
                //if (DebugEnabled) Debug.Log(name + ".UpdateTimeChannel time:" + time);
                PlayAudio();
            }

            if (channel.Behavior == this && Enabled) {
                channel.Interpolate(time);
            }
        }

        #endregion

        #region PLAY

        /// <summary>
        /// Method called from Timeflow when playback starts
        /// </summary>
        public override void OnPlay()
        {
            base.OnPlay();
            //if (DebugEnabled) Debug.Log(name + ".Audio.OnPlay:" + SourceTime);
            PlayAudio();
        }

        /// <summary>
        /// Restricts time from 0 to the clip length range if loop is enabled.
        /// </summary>
        /// <param name="time">The input time (in audio clip local time)</param>
        /// <returns>Time from 0 to clip length, or -1 if out of range</returns>
        public float LoopAudioTime(float time)
        {
            float inTime = time;
            if (Source == null || Source.clip == null) {
                time = -1f;
            }
            else
            if (Source.loop) {
                time = MathUtil.Loop(time, 0f, Source.clip.length);
            }
            else
            if (time < 0f || time >= Source.clip.length) {
                time = 0f;
            }
            //if(inTime != time) Debug.Log($"LoopAudioTime inTime:{inTime} time:{time} length:{Source.clip.length}");
            return time;
        }

        /// <summary>
        /// Starts audio playback based on the current track channel time, or stops audio if time is out of
        /// range
        /// </summary>
        public void PlayAudio()
        {
            if (!Enabled || !enabled || !gameObject.activeInHierarchy) return;
            if (Source != null && !Mute && Parent.IsPlayingInHierarchy) {
                if (!Source.enabled) return;
                float time = LoopAudioTime(Channel.CurrentTime * LocalTimeScale);
                if (time >= 0) {
                    SourceTime = time;
                    if (!IsPlaying) {
                        Source.pitch = LocalTimeScale * (LinkPitchToTimeScale ? Time.timeScale : 1f) * Timeflow.TimeScale;
                        Source.Play();
                        //if (DebugEnabled) Debug.Log(name + ".Audio.Play:" + SourceTime + " Source.time:" + Source.time + " time:" + time);
                    }
                }
                else {
                    /// Time range is out of range of audio clip
                    if (IsPlaying) Source.Stop();
                }
            }
        }

        public void StopAudio()
        {
            if (Source != null) {
                //if (DebugEnabled) Debug.Log(name + ".Audio.Stop");
                Source.Stop();
            }
        }

        public override void OnStop()
        {
            //if (DebugEnabled) Debug.Log(name + ".Audio.OnStop");
            if (Source != null) Source.Stop();
        }

        public void Rewind()
        {
            //if (DebugEnabled) Debug.Log(name + ".Audio.Rewind");
            if (Source != null) {
                if (Channel.CurrentTime < 0) {
                    Source.Stop();
                }
                else {
                    SourceTime = Channel.CurrentTime;
                }
            }
        }

        public override void OnTrackStart()
        {
            if (IsTrackOn) return;
            base.OnTrackStart();
            PlayAudio();
        }

        public override void OnTrackEnd()
        {
            if (!IsTrackOn) return;
            base.OnTrackEnd();
            StopAudio();
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AudioTrack;

        [NonSerialized]
        public float[] Samples;

        [NonSerialized]
        private int drawCount = 0;

        [NonSerialized]
        private float[] audioData = null;

        public Color AudioTrackColor = Color.white;

        public float WaveformScale = 0.9f; // Keeps waveform comfortably within track area vertically
        public bool WaveformStereo = false; // Keeps waveform comfortably within track area vertically

        #region TIMEFLOW GUI

        public override bool IsSelected {
            get {
                bool sel = false;
                if (Channel != null && Channel.IsSelected) {
                    sel = true;
                }
                return sel;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Audio/Audio Track"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    AudioTrack comp = Undo.AddComponent<AudioTrack>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        private void GetAudioData()
        {
            if (audioData != null && audioData.Length > 1) return;
            string path = AssetDatabase.GetAssetPath(Source.clip);
            AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;

            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            MethodInfo method = audioUtilClass.GetMethod("GetMinMaxData", BindingFlags.Static | BindingFlags.Public);
            if (method == null) {
                Debug.LogWarning("Failed getting method GetWaveFormFast");
            }
            else
            if (audioImporter != null) {
                audioData = (float[])method.Invoke(null, new object[] { audioImporter });
            }
        }

        public void RenderAudioWaveform(Rect rect, float startTime, float endTime)
        {
            if (drawCount < 4) {
                // Hack to fix UI bug in Unity with the waveform drawing initially at 0,0 
                drawCount++;
                return;
            }

            //Debug.Log($"RenderAudioWaveform:{rect} start:{startTime} end:{endTime}");

            GetAudioData();

            AudioClip clip = Source.clip;
            int numChannels = clip.channels;
            if (numChannels <= 0) return;
            if (!WaveformStereo) numChannels = 1;

            int numSamples = (audioData != null) ? (audioData.Length / (2 * numChannels)) : 0;
            float num = rect.height / (float)numChannels;
            int channel;
            for (channel = 0; channel < numChannels; channel++) {
                if (rect.width > 20000) rect.width = 20000;
                Rect r = new Rect(rect.x, rect.y + num * (float)channel, rect.width / LocalTimeScale, num);
                AudioCurveRendering.DrawMinMaxFilledCurve(r, delegate (float x, out Color col, out float minValue, out float maxValue) {
                    col = AudioTrackColor;
                    if (numSamples <= 0) {
                        minValue = 0f;
                        maxValue = 0f;
                    }
                    else {
                        float f = Mathf.Clamp(x * (float)(numSamples - 2), 0f, (float)(numSamples - 2));
                        int num2 = (int)Mathf.Floor(f);
                        int num3 = (num2 * numChannels + channel) * 2;
                        int num4 = num3 + numChannels * 2;
                        minValue = Mathf.Min(audioData[num3 + 1], audioData[num4 + 1]) * WaveformScale;
                        maxValue = Mathf.Max(audioData[num3], audioData[num4]) * WaveformScale;
                        if (minValue > maxValue) {
                            float num5 = minValue;
                            minValue = maxValue;
                            maxValue = num5;
                        }
                    }
                });
            }
        }

        #endregion

#endif

    }

}//AxonGenesis
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if TMPRO_3_OR_NEWER
using TMPro;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Audio Spectrum")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-sampler")]
    sealed public class AudioSpectrum : TimeflowBehavior
    {
        #region STATIC

        public static AudioSpectrum Instance { get; private set; }

        #endregion

        #region PUBLIC

        public List<AudioSample> Samples;

        public bool Passive;
        public string DeviceName = "Microphone";

#if TMPRO_3_OR_NEWER
        public TextMeshPro DeviceNameText;
#endif

        public AudioSource Source;
        public int DeviceSampleRate = 44100;
        public bool FallbackToFirstDevice = true;

        public int SpectrumResolution = 512;
        public FFTWindow SpectrumWindow = FFTWindow.BlackmanHarris;

        public float DeviceTimeout = 4f;
        public bool Restart = true;
        public bool Stop;

        public enum AudioInputs
        {
            AudioSource,
            AudioListener,
            Device
        }
        public AudioInputs AudioInput = AudioInputs.AudioSource;

        public enum AudioChannels
        {
            Stereo,
            Left,
            Right
        }
        public AudioChannels AudioChannel = AudioChannels.Left;

        public delegate void SpectrumUpdateDelegate(ref float[] spectrum);
        public SpectrumUpdateDelegate OnSpectrumUpdate = null;

        public bool EnableVolume;
        public bool RMSRaw;
        public float RMSReference = 0.1f;
        public TimeflowChannel VolumeChannel;

        public bool EnableFrequency;
        public float FrequencyThreshold = 0.02f;
        public TimeflowChannel FrequencyChannel;

        public int SampleRate = 44100;
        public bool AutoSampleRate = true;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsListening;

        [NonSerialized]
        public bool HasDevice;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private float[] samplesL;

        [NonSerialized]
        private float[] samplesR;

        [NonSerialized]
        private float[] spectrumL;

        [NonSerialized]
        private float[] spectrumR;

        [NonSerialized]
        private bool isSetup;

        [NonSerialized]
        private bool isDeviceReady;

        [NonSerialized]
        private bool canStartDevicePlayback;

        [NonSerialized]
        private float deviceTimeout;

        #endregion

        #region ACCESSORS

        private float[] _spectrum {
            get {
                if (AudioChannel == AudioChannels.Right) {
                    return spectrumR;
                }
                return spectrumL;
            }
        }

        public bool UseAudioListener {
            get {
                return AudioInput == AudioInputs.AudioListener;
            }
        }

        public bool UseDevice {
            get {
                return AudioInput == AudioInputs.Device;
            }
        }

        #endregion

        public override void Refresh()
        {
            base.Refresh();
            SetupChannels(false);
            SetupAudio();
        }

        protected override void OnAwake()
        {
            Instance = this;
            base.OnAwake();
            if (Enabled) {
                if (Application.isPlaying && UseDevice) {
                    StartCoroutine(_GetPermission());
                }
                else {
                    SetupAudio();
                }
            }
        }

        protected override void OnDestruct()
        {
            Instance = null;
            base.OnDestruct();
        }

        public void Register(AudioSample sample)
        {
            if (Samples == null) Samples = new List<AudioSample>();
            if (!Samples.Contains(sample)) {
                Samples.Add(sample);
            }
        }

        public void Unregister(AudioSample sample)
        {
            if (Samples != null && Samples.Contains(sample)) {
                Samples.Remove(sample);
            }
        }

        public bool IsRegistered(AudioSample sample)
        {
            return (Samples != null && Samples.Contains(sample));
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);

            if (EnableVolume) {
                if (VolumeChannel == null) {
                    VolumeChannel = new TimeflowChannel();
                }
                VolumeChannel.IsEnabled = true;
                if (VolumeChannel.ToProperty == null) {
                    VolumeChannel.ToProperty = new Property();
                }
                VolumeChannel.ShowVector = false;
                VolumeChannel.IsDataOnly = true;
                VolumeChannel.SupportsKeyframes = false;
                if (AudioChannel == AudioChannels.Stereo) {
                    VolumeChannel.PropertyType = Property.PropertyTypes.Vector2;
                }
                else {
                    VolumeChannel.PropertyType = Property.PropertyTypes.Float;
                }

                VolumeChannel.ToProperty.IsEnabled = true;
                VolumeChannel.ToProperty.CanBeAssigned = false;
                VolumeChannel.ToProperty.PropertyType = VolumeChannel.PropertyType;
                VolumeChannel.ToProperty.IsDataOnly = true;
                VolumeChannel.ToProperty.IsCombinedValue = true;
                VolumeChannel.OnSetup(this);

                if (string.IsNullOrEmpty(VolumeChannel.Name) || string.IsNullOrEmpty(VolumeChannel.ToProperty.Name)) {
                    VolumeChannel.Name = VolumeChannel.ToProperty.Name = "Volume";
                }

                Channels.Add(VolumeChannel);
            }
            else {
                if (VolumeChannel != null) {
                    VolumeChannel.IsEnabled = false;
                }
                VolumeChannel = null;
            }

            if (EnableFrequency) {
                if (FrequencyChannel == null) {
                    FrequencyChannel = new TimeflowChannel();
                }
                FrequencyChannel.IsEnabled = true;
                if (FrequencyChannel.ToProperty == null) {
                    FrequencyChannel.ToProperty = new Property();
                }
                FrequencyChannel.ShowVector = false;
                FrequencyChannel.IsDataOnly = true;
                FrequencyChannel.SupportsKeyframes = false;
                if (AudioChannel == AudioChannels.Stereo) {
                    FrequencyChannel.PropertyType = Property.PropertyTypes.Vector2;
                }
                else {
                    FrequencyChannel.PropertyType = Property.PropertyTypes.Float;
                }

                FrequencyChannel.ToProperty.IsEnabled = true;
                FrequencyChannel.ToProperty.CanBeAssigned = false;
                FrequencyChannel.ToProperty.PropertyType = FrequencyChannel.PropertyType;
                FrequencyChannel.ToProperty.IsDataOnly = true;
                FrequencyChannel.ToProperty.IsCombinedValue = true;
                FrequencyChannel.OnSetup(this);

                if (string.IsNullOrEmpty(FrequencyChannel.Name) || string.IsNullOrEmpty(FrequencyChannel.ToProperty.Name)) {
                    FrequencyChannel.Name = FrequencyChannel.ToProperty.Name = "Frequency";
                }

                Channels.Add(FrequencyChannel);
            }
            else {
                if (FrequencyChannel != null && FrequencyChannel.IsEnabled) {
                    FrequencyChannel.IsEnabled = false;
                }
                FrequencyChannel = null;
            }
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.RegisterChannels");
            if (EnableVolume && VolumeChannel != null) obj.RegisterChannel(VolumeChannel);
            if (EnableFrequency && FrequencyChannel != null) obj.RegisterChannel(FrequencyChannel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":MotionPath.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

            if (channel == VolumeChannel) {
                VolumeChannel = null;
                EnableVolume = false;
            }
            else
            if (channel == FrequencyChannel) {
                FrequencyChannel = null;
                EnableFrequency = false;
            }
        }

        private IEnumerator _GetPermission()
        {
#if TIMEFLOW_DISABLE_MICROPHONE
            Debug.LogWarning("AudioSpectrum: Microphone support is disabled. It can be enabled in the player settings " +
                "by removing the scripting define symbol TIMEFLOW_DISABLE_MICROPHONE.");
            yield break;
#else
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone)) {
                //if (DebugEnabled) Debug.Log(name + ".GetPermission: Authorized to record");
                SetupAudio();
            }
            else {
                Debug.LogWarning(name + ".GetPermission: Not authorized to use microphone");
            }
#endif
        }

        public bool FindDevice()
        {
            HasDevice = false;
#if !TIMEFLOW_DISABLE_MICROPHONE
#if UNITY_WEBGL
            // Unity does not support the microphone for this platform
            Debug.LogWarning("AudioSpectrum cannot use the built-in Microphone since Unity doesn't support it on WebGL.");
#else
            if (Microphone.devices != null && Microphone.devices.Length > 0) {
                int i = 0;
#if TMPRO_3_OR_NEWER
                if (DeviceNameText != null) {
                    DeviceNameText.text = "";
                }
#endif
                foreach (string device in Microphone.devices) {
                    if (device.Equals(DeviceName)) {
                        HasDevice = true;
                        //if (DebugEnabled) Debug.Log(name + ".FindDevice: " + device + " FOUND:" + HasDevice);
                        break;
                    }
                    i++;
                }
                if (!HasDevice && FallbackToFirstDevice) {
                    HasDevice = true;
                    DeviceName = Microphone.devices[0];
                }
            }
            if (!HasDevice) {
                Debug.LogError(name + " Failed to find the microphone device '" + DeviceName + "'");
            }
#endif
#endif
            return HasDevice;
        }

        public void SetupAudio()
        {
            //if (DebugEnabled) Debug.Log(name + ".SetupAudio");
            isSetup = true;
            if (Source == null) Source = GetComponent<AudioSource>();
            if (Restart && UseDevice) {
                Source.clip = null;
            }
            Restart = false;

            SpectrumResolution = Mathf.ClosestPowerOfTwo(SpectrumResolution);
            if (SpectrumResolution <= 16) SpectrumResolution = 256;

            if (RMSReference < 0.001f) RMSReference = 0.001f;

            if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                if (spectrumL == null || spectrumL.Length != SpectrumResolution) {
                    spectrumL = new float[SpectrumResolution];
                }
                if (samplesL == null || samplesL.Length != SpectrumResolution) {
                    samplesL = new float[SpectrumResolution];
                }
            }
            if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Right) {
                if (spectrumR == null || spectrumR.Length != SpectrumResolution) {
                    spectrumR = new float[SpectrumResolution];
                }
                if (samplesR == null || samplesR.Length != SpectrumResolution) {
                    samplesR = new float[SpectrumResolution];
                }
            }

#if UNITY_WEBGL || TIMEFLOW_DISABLE_MICROPHONE

            // Not supported
#else
            if (UseDevice) {
                /// Always use device sampler rate otheriwse errors will occur
                SampleRate = DeviceSampleRate;
                int minFreq, maxFreq;
                Microphone.GetDeviceCaps(DeviceName, out minFreq, out maxFreq);
                if (SampleRate < minFreq) {
                    Debug.LogWarning("The input device '" + DeviceName + "' does not support the frequence " + SampleRate + " min:" + minFreq +
                        ". The sample rate has been automatically adjusted to match the device.");
                    SampleRate = minFreq;
                }
                else
                if (maxFreq != 0 && SampleRate > maxFreq) {
                    Debug.LogWarning("The input device '" + DeviceName + "' does not support the frequence " + SampleRate + " max:" + maxFreq +
                        ". The sample rate has been automatically adjusted to match the device.");
                    SampleRate = maxFreq;
                }
            }
            else
#endif

            if (AutoSampleRate) {
                SampleRate = AudioSettings.outputSampleRate;
            }

            if (!Application.isPlaying) {
                if (Source != null) Source.Stop(); // Prevent playback from continuing when exiting play mode
            }
            else {
                if (!UseDevice || FindDevice()) {
                    StartListening();
                }
                else {
                    Debug.LogWarning("AudioSpectrum: could not find device:" + DeviceName);
                }
            }
        }

        public void StartListening()
        {
            if (Enabled) {
                //if (DebugEnabled) Debug.Log(name + ".StartListening");
                if (UseAudioListener) {
                    IsListening = true;
                    //if (DebugEnabled) Debug.Log(name + ".StartListening: UseAudioListener");
                }
                else
                if (UseDevice && Application.isPlaying) {
#if !TIMEFLOW_DISABLE_MICROPHONE
#if UNITY_WEBGL
                // Not supported
                Debug.LogWarning("Please note that due to audio limitations with Unity and WebGL, sampling audio devices is not allowed.");
#else
                    if (!HasDevice || Microphone.devices == null) {
                        //if (DebugEnabled) Debug.LogWarning("AudioSpectrum.StartListening: invalid device: " + DeviceName);
                        IsListening = false;
                    }
                    else {
                        //if (DebugEnabled) Debug.Log(name + ".Microphone.Start:" + DeviceName + " SampleRate:" + SampleRate);
                        Source.clip = Microphone.Start(DeviceName, true, 1, SampleRate);
                        Source.loop = true;

#if TMPRO_3_OR_NEWER
                        if (DeviceNameText != null) {
                            DeviceNameText.text = "Mic:" + DeviceName;
                        }
#endif

                        if (Microphone.IsRecording(DeviceName)) {
                            //if (DebugEnabled) Debug.Log(name + " Preparing microphone for recording: " + DeviceName);

                            isDeviceReady = false;
                            canStartDevicePlayback = false;
                            deviceTimeout = Time.time + DeviceTimeout;
                            StartCoroutine(_WaitForDevice());
                        }
                    }
#endif
#endif
                }
                else {
                    if (Source != null && Source.clip != null) {
                        //if (DebugEnabled) Debug.Log("AudioSpectrum.StartListening: Using clip");
                        SourcePlay();
                    }
                }
            }
        }

        private IEnumerator _WaitForDevice()
        {
#if !TIMEFLOW_DISABLE_MICROPHONE
#if UNITY_WEBGL
            // Not supported
#else
            while (!isDeviceReady) {
                isDeviceReady = Microphone.GetPosition(DeviceName) > 0f;
                if (!isDeviceReady) {
                    if (Time.time > deviceTimeout) {
                        Debug.LogWarning("Timeout waiting for device: " + DeviceName);
                        yield break;
                    }
                }
                else {
                    //if (DebugEnabled) Debug.Log(name + " Microphone recording started: " + DeviceName);
                    canStartDevicePlayback = true;
                }
                yield return null;
            }
#endif
#endif
            yield break;
        }

        public void StopListening()
        {
            //if (DebugEnabled) Debug.Log(name + ".StopListening");
            Stop = false;
            IsListening = false;
#if !TIMEFLOW_DISABLE_MICROPHONE
#if UNITY_WEBGL
// Not supported
#else
            if (UseDevice) Microphone.End(DeviceName);
#endif
#endif
        }

        private void SourcePlay()
        {
            if (Enabled && enabled && gameObject.activeInHierarchy) {
                //if (DebugEnabled) Debug.Log(name + ".SourcePlay");
                if (Source.enabled && Source.clip.samples > 0 && Source.clip.channels > 0 && Source.clip.length > 0) {
                    IsListening = true;
                    //if (DebugEnabled) Debug.Log(name + ".SourcePlay: samples:" + Source.clip.samples + " channels:" + Source.clip.channels + " length:" + Source.clip.length);

                    if (!Source.isPlaying && !Passive) {
                        Source.Play();
                    }
                }
                else {
                    Debug.LogError("Invalid Source.clip");
                }
            }
            else {
                IsListening = false;
            }
        }

        public override void OnPlay()
        {
            //if (DebugEnabled) Debug.Log(name + ".OnPlay");
            base.OnPlay();
            StartListening();
        }

        public override void OnStop()
        {
            base.OnStop();
            StopListening();
        }

        public override void UpdateTime()
        {
            if (CanUpdate) {
                if (canStartDevicePlayback) {
                    SourcePlay();
                    canStartDevicePlayback = false;
                }

                if (Restart || !isSetup) {
                    SetupAudio();
                }
                else
                if (Stop) {
                    StopListening();
                }

                if (IsListening) {
                    Analyze();
                }

                if (EnableVolume && VolumeChannel != null) {
                    GetVolume();
                }
                if (EnableFrequency && FrequencyChannel != null) {
                    GetFrequency();
                }
                base.UpdateTime();
            }
            else {
                if (IsListening) StopListening();
            }
        }

        public void Analyze()
        {
            if (UseAudioListener) {
                if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                    AudioListener.GetSpectrumData(spectrumL, 0, SpectrumWindow);
                }
                if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Right) {
                    AudioListener.GetSpectrumData(spectrumR, 1, SpectrumWindow);
                }
                if (EnableFrequency || EnableVolume) {
                    if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                        AudioListener.GetOutputData(samplesL, 0);
                    }
                    if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Right) {
                        AudioListener.GetOutputData(samplesR, 1);
                    }
                }
            }
            else {
                if (Source == null) return;
                //if (DebugEnabled) Debug.Log(name + ".AnalyzeSound: sampleRate:" + SampleRate + " spectrumL:" + spectrumL.Length + " win:" + SpectrumWindow);

                if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                    Source.GetSpectrumData(spectrumL, 0, SpectrumWindow);
                }
                if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Right) {
                    Source.GetSpectrumData(spectrumR, 1, SpectrumWindow);
                }
                if (EnableFrequency || EnableVolume) {
                    if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                        Source.GetOutputData(samplesL, 0);
                    }
                    if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Right) {
                        Source.GetOutputData(samplesR, 1);
                    }
                }
            }

            if (OnSpectrumUpdate != null) {
                if (AudioChannel == AudioChannels.Stereo || AudioChannel == AudioChannels.Left) {
                    OnSpectrumUpdate(ref spectrumL);
                }
                else {
                    OnSpectrumUpdate(ref spectrumR);
                }
            }

            if (Samples != null) {
                int i = 0;
                foreach (AudioSample sample in Samples) {
                    if (sample != null && sample.gameObject.activeInHierarchy && sample.enabled && sample.Enabled) {
                        AnalyzeSample(sample);
                    }
                    i++;
                }
            }
        }

        public void AnalyzeSample(AudioSample sample)
        {
            float freqRate = SampleRate / 2f;
            if (freqRate <= 0f) return;
            int start = (int)((sample.StartFrequency / freqRate) * SpectrumResolution);
            int end = (int)((sample.EndFrequency / freqRate) * SpectrumResolution);

            start = MathUtil.Clamp(start, 0, SpectrumResolution);

            if (end <= start) end = start + 1;
            end = MathUtil.Clamp(end, 0, SpectrumResolution);

            int range = end - start;

            //if (DebugEnabled) Debug.Log(name + ".Sample: range:" + range + " start:" + start + " end:" + end + " res:" + SpectrumResolution);

            float v = 0f;
            float average = 0;
            float max = 0f;
            for (int j = start; j < end; j++) {
                if (AudioChannel == AudioChannels.Stereo) {
                    v = ((spectrumL[j] + spectrumR[j]) / 2f) * (j + 1);
                }
                else
                if (AudioChannel == AudioChannels.Left) {
                    v = spectrumL[j] * (j + 1);
                }
                else
                if (AudioChannel == AudioChannels.Right) {
                    v = spectrumR[j] * (j + 1);
                }
                if (sample.SumMode == AudioSample.SumModes.Average) {
                    average += v;
                }
                else {
                    max = Mathf.Max(v, max);
                }
            }
            if (range > 0 && sample.SumMode == AudioSample.SumModes.Average) {
                average = average / (float)range;
                sample.Amplitude = average;
            }
            else {
                sample.Amplitude = max;
            }
        }

        public void GetVolume()
        {
            if (!EnableVolume || VolumeChannel == null) return;

            if (AudioChannel == AudioChannels.Stereo) {
                float x = CalculateVolume(ref samplesL);
                float y = CalculateVolume(ref samplesR);
                VolumeChannel.CurrentVector = new Vector2(x, y);
                VolumeChannel.ToProperty.Vector2Value = VolumeChannel.CurrentVector;
            }
            else
            if (AudioChannel == AudioChannels.Left) {
                VolumeChannel.CurrentValue = CalculateVolume(ref samplesL);
                VolumeChannel.ToProperty.FloatValue = VolumeChannel.CurrentValue;
            }
            else
            if (AudioChannel == AudioChannels.Right) {
                VolumeChannel.CurrentValue = CalculateVolume(ref samplesR);
                VolumeChannel.ToProperty.FloatValue = VolumeChannel.CurrentValue;
            }
        }

        private float CalculateVolume(ref float[] samples)
        {
            float volume = -60f;
            if (samples == null) return volume;
            float sum = 0;
            for (int i = 0; i < SpectrumResolution; i++) {
                sum += samples[i] * samples[i];
            }

            float rms = Mathf.Sqrt(sum / SpectrumResolution);

            if (RMSRaw) {
                //if (DebugEnabled) Debug.Log(name + ".GetVolume: RMS:" + rms);
                volume = rms;
            }
            else {
                float db = 20f * Mathf.Log10(rms / RMSReference);
                if (db < -60f) db = -60f;

                //if (DebugEnabled) Debug.Log(name + ".GetVolume: db:" + db);

                volume = db;
            }
            return volume;
        }

        public void GetFrequency()
        {
            if (!EnableFrequency || FrequencyChannel == null) return;

            if (AudioChannel == AudioChannels.Stereo) {
                float x = CalculateFrequency(ref spectrumL);
                float y = CalculateFrequency(ref spectrumR);
                if (x == 0) x = FrequencyChannel.CurrentVector.x;
                if (y == 0) y = FrequencyChannel.CurrentVector.y;
                FrequencyChannel.CurrentVector = new Vector2(x, y);
                FrequencyChannel.ToProperty.Vector2Value = FrequencyChannel.CurrentVector;
            }
            else
            if (AudioChannel == AudioChannels.Left) {
                float p = CalculateFrequency(ref spectrumL);
                if (p == 0) p = FrequencyChannel.CurrentValue;
                FrequencyChannel.CurrentValue = p;
                FrequencyChannel.ToProperty.FloatValue = FrequencyChannel.CurrentValue;
            }
            else
            if (AudioChannel == AudioChannels.Right) {
                float p = CalculateFrequency(ref spectrumR);
                if (p == 0) p = FrequencyChannel.CurrentValue;
                FrequencyChannel.CurrentValue = p;
                FrequencyChannel.ToProperty.FloatValue = FrequencyChannel.CurrentValue;
            }
        }

        private float CalculateFrequency(ref float[] spectrum)
        {
            float freq = 0f;
            float average = 0f;
            float power = 0f;

            for (int i = 0; i < SpectrumResolution - 1; ++i) {
                float hz = i * (SampleRate * 0.5f) / (float)(SpectrumResolution-1);

                /// Filter out low level noise
                if (spectrum[i] > FrequencyThreshold) {
                    average += spectrum[i] * hz;
                    power += spectrum[i];
                }
            }

            if (power > 0.001f) {
                freq = average / power;
            }
            else {
                freq = 0;
            }
            return freq;
        }

        public float GetSampleRate()
        {
            return SampleRate;
        }

#if UNITY_EDITOR

        [SerializeField, FormerlySerializedAs("ShowGraph")]
        private bool _ShowGraph;

        public override Texture2D Icon => AxonUI.Icons.AudioSpectrum;

        public bool ShowGraph {
            get {
                return _ShowGraph;
            }
            set {
                if (_ShowGraph != value) {
                    _ShowGraph = value;
                }
            }
        }

        public float GraphScale = 1f;
        public float GraphThreshold;
        public float GraphThresholdMax = 0.1f;
        public Vector2 GraphRange = new Vector2(0f, 24000f);

        [NonSerialized]
        private float graphRange = 24000f;

        void OnGUI()
        {
            if (ShowGraph && Event.current.type == EventType.Repaint && _spectrum != null && graphRange > 0) {
                Handles.BeginGUI();
                Handles.color = Color.red;

                Vector2 view = GameViewUtil.GetSize();
                int min = Mathf.FloorToInt((float)SpectrumResolution * (GraphRange.x / graphRange));
                int max = Mathf.CeilToInt((float)SpectrumResolution * GraphRange.y / graphRange) + 1;
                if (min < 0) min = 0;
                if (max > SpectrumResolution) max = SpectrumResolution;
                if (min > max) min = 0;

                if (max > _spectrum.Length) max = _spectrum.Length;

                Color lineColor = new Color(1f, 1f, 1f, 0.1f);
                Color rangeColor = new Color(1f, 0f, 0f, 0.5f);
                Handles.color = lineColor;

                Vector2 from = new Vector2(GraphPosition(min, view.x), view.y);
                Vector2 to = from;
                from.y = 0f;
                to.y = view.y;

                Rect rect = new Rect(5, view.y - 30f, 100, 30);
                EditorGUI.LabelField(rect, new GUIContent("0"));
                Handles.DrawLine(from, to);


                /// FREQUENCY RANGE
                Handles.color = rangeColor;
                rect.y = 30f;

                if (min > 0) {
                    rect.x = GraphPosition(min, view.x);
                    EditorGUI.LabelField(rect, new GUIContent("Min " + Mathf.RoundToInt(GraphRange.x)));
                    from.x = to.x = rect.x;
                    Handles.DrawLine(from, to);
                }

                if (max < SpectrumResolution) {
                    rect.x = GraphPosition(max - 1, view.x);
                    EditorGUI.LabelField(rect, new GUIContent("Max " + Mathf.RoundToInt(GraphRange.y)));
                    from.x = to.x = rect.x;
                    Handles.DrawLine(from, to);
                }

                from.x = 0f;
                to.x = view.x;
                from.y = to.y = view.y - (GraphThreshold * view.y);
                Handles.DrawLine(from, to);

                Handles.color = lineColor;

                /// GRID
                rect.y = view.y - 30f;
                rect.x = GraphPosition(Mathf.RoundToInt((100 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("100"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((500 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("500"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((1000 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("1k"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((2000 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("2k"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((4000 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("4k"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((10000 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("10k"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                rect.x = GraphPosition(Mathf.RoundToInt((20000 / 24000f) * (float)SpectrumResolution), view.x);
                EditorGUI.LabelField(rect, new GUIContent("20k"));
                from.x = to.x = rect.x;
                Handles.DrawLine(from, to);

                /// VOLUME
                if (EnableVolume && VolumeChannel != null) {
                    if (AudioChannel == AudioChannels.Stereo) {
                        GraphVolume(VolumeChannel.CurrentVector.x, 0, view);
                        GraphVolume(VolumeChannel.CurrentVector.y, 1, view);
                    }
                    else
                    if (AudioChannel == AudioChannels.Left) {
                        VolumeChannel.ToProperty.FloatValue = VolumeChannel.CurrentValue;
                        GraphVolume(VolumeChannel.CurrentValue, 1, view);
                    }
                    else
                    if (AudioChannel == AudioChannels.Right) {
                        VolumeChannel.ToProperty.FloatValue = VolumeChannel.CurrentValue;
                        GraphVolume(VolumeChannel.CurrentValue, 1, view);
                    }
                }

                /// FREQUENCY PTICH & NOTE
                if (EnableFrequency && FrequencyChannel != null) {
                    Rect prect = new Rect(10f, 10f, 300, 30);
                    string val = "";
                    if (AudioChannel == AudioChannels.Stereo) {
                        val = "L:" + MusicUtil.FrequencyAndNote(FrequencyChannel.CurrentVector.x) + " R:" + MusicUtil.FrequencyAndNote(FrequencyChannel.CurrentVector.y);
                    }
                    else
                    if (AudioChannel == AudioChannels.Left) {
                        val = ":" + MusicUtil.FrequencyAndNote(FrequencyChannel.CurrentValue);
                    }
                    else
                    if (AudioChannel == AudioChannels.Right) {
                        val = ":" + MusicUtil.FrequencyAndNote(FrequencyChannel.CurrentValue);
                    }
                    EditorGUI.LabelField(prect, new GUIContent("Frequency " + val));
                }

                from = new Vector2(GraphPosition(min, view.x), view.y);
                to = from;

                /// GRAPH
                Handles.color = Color.white;
                Color ignoredColor = new Color(1f, 1f, 1f, 0.1f);

                for (int i = min; i < max; i++) {
                    float f = (float)i;
                    float p = f / (float)SpectrumResolution;
                    from = to;
                    float val = _spectrum[i] * f;
                    //if (val < GraphThreshold) val = GraphThreshold;
                    //val -= GraphThreshold;
                    Handles.color = val < GraphThreshold ? ignoredColor : ColorUtil.HLSColor(p, 1f, 1f);
                    to = new Vector2(GraphPosition(i, view.x), view.y - (GraphScale * view.y * val));
                    //from.y = to.y;
                    Handles.DrawLine(from, to);

                    from.y++;
                    to.y++;
                    Handles.DrawLine(from, to);
                }

                Handles.EndGUI();
            }
        }

        private void GraphVolume(float volume, int channel, Vector2 view)
        {
            if (AxonUI.SolidStyle == null) return;
            float value = RMSRaw ? volume : MathUtil.GetInterpolation(-60f, 6f, volume);
            float space = 25f;
            float height = view.y - (space * 2f);

            Vector2 from = new Vector2(view.x - 20f, view.y);
            Vector2 to = new Vector2(view.x, space + (height - (value * height)));

            if (channel == 0) {
                from.x -= 30f;
                to.x -= 30f;
            }

            GUI.color = Color.green;
            Rect vrect = new Rect(from.x, from.y, to.x - from.x, to.y - from.y);
            GUI.Box(vrect, "", AxonUI.SolidStyle);
            GUI.color = Color.white;
        }

        private float GraphPosition(int spectrumIndex, float viewSize)
        {
            return MathUtil.EaseOutExpo(0f, viewSize, (float)spectrumIndex / (float)SpectrumResolution);
            //return MathUtil.Interpolate(0f, viewSize, (float)spectrumIndex / (float)SpectrumResolution);
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Audio/Audio Spectrum"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    AudioSpectrum comp = Undo.AddComponent<AudioSpectrum>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }




#endif
    }

}//AxonGenesis

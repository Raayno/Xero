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
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Audio Sample")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-sample")]
    sealed public class AudioSample : TimeflowBehavior
    {
        #region STATIC

        public static bool HasAnyInstances()
        {
            AudioSample[] samples = GetAllAudioSampleInstances();
            return samples != null && samples.Length > 0;
        }

        public static AudioSample[] GetAllAudioSampleInstances()
        {
            return UnityEngine.Object.FindObjectsByType(typeof(AudioSample), FindObjectsInactive.Include, FindObjectsSortMode.None) as AudioSample[];
        }

        public static void SetIsBakingAll(bool startBaking, bool useBaking, bool clear)
        {
            AudioSample[] instances = GetAllAudioSampleInstances();
            if (instances != null && instances.Length > 0) {
                foreach (AudioSample instance in instances) {
                    if (clear) {
                        instance.ClearBakedData();
                    }
                    if (!useBaking) {
                        instance.UseBakedData = false;
                        instance.IsBaking = false;
                    }
                    else
                    if (startBaking) {
                        instance.StartBaking(false);
                    }
                    else {
                        instance.StopBaking();
                    }
                }

                if (Timeflow.Active != null) {
                    Timeflow.Active.Play(true);
                }
            }
        }

        #endregion

        #region PUBLIC

        public AudioSpectrum Spectrum;
        public AudioSampleChannel Channel;

        public bool FindSpectrumByName;
        public string SpectrumName = "";
        public float StartFrequency;
        public float EndFrequency = 100f;

        public enum SumModes
        {
            Average,
            Maximum
        }
        public SumModes SumMode = SumModes.Average;

        public float AmplitudeThreshold = 0.01f;
        public float AmplitudeThresholdMax = 0.1f;
        public float DecayRate = 0.1f;
        public float Multiply = 1f;

        public bool ApplyToScale = true;
        public bool ScaleUniform = true;
        public Vector3 BaseScale = new Vector3(1f, 1f, 1f);
        public Vector3 Scale = new Vector3(1f, 1f, 1f);

        public bool SetShaderProperty;
        public string ShaderPropertyName = "_Amplitude";

        public bool UseBakedData;
        public float[] BakedData;

        #endregion

        #region PRIVATE

        [SerializeField]
        private float _Amplitude;

        [NonSerialized]
        private int shaderPropertyID;

        [NonSerialized]
        private int lastBakedIndex = -1;

        [NonSerialized]
        private bool isBaking;

        #endregion

        #region ACCESSORS

        public bool IsBaking {
            get {
                return isBaking;
            }
            set {
                isBaking = value;
            }
        }

        public bool HasBakedData {
            get {
                return BakedData != null && BakedData.Length > 0;
            }
        }

        public float Amplitude {
            get {
                if (UseBakedData && !IsBaking && HasBakedData) {
                    _Amplitude = BakedData[BakedDataIndex];
                }
                return _Amplitude * Multiply;
            }
            set {
                if (AmplitudeThreshold > 0f && value < AmplitudeThreshold) value = 0f;
                if (DecayRate <= 0f || value > _Amplitude) {
                    _Amplitude = value;
                }
                else {
                    _Amplitude = MathUtil.Interpolate(_Amplitude, value, LocalDeltaTime / DecayRate);
                }
                if (UseBakedData && IsBaking && HasBakedData) {
                    int i = BakedDataIndex;
                    int range = i - lastBakedIndex;
                    if (lastBakedIndex != -1 && range > 1) {
                        for (int x = i - 1; x > lastBakedIndex; x--) {
                            BakedData[x] = BakedData[lastBakedIndex];
                        }
                    }
                    BakedData[i] = _Amplitude;
                    lastBakedIndex = i;
                }
            }
        }

        public int BakedDataIndex {
            get {
                if (!UseBakedData || !HasBakedData) return 0;

                int index = Mathf.RoundToInt(CurrentTime * Timeflow.FPS);
                if (index >= BakedData.Length) index = BakedData.Length - 1;
                if (index < 0) index = 0;
                return index;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Spectrum == null) {
                Spectrum = AudioSpectrum.Instance;
            }
            if (Spectrum == null) {
                Debug.LogWarning("Please assign an AudioSpectrum", gameObject);
            }
            else {
                Spectrum.Register(this);
            }
        }

        protected override void OnDestruct()
        {
            if (Spectrum != null) {
                Spectrum.Unregister(this);
            }
            if (Channel != null) {
                RemoveChannel(Channel);
            }
            base.OnDestruct();
        }

        public override void Refresh()
        {
            SetupChannels(true);
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (Channel == null) {
                Channel = new AudioSampleChannel();
                Channel.ToProperty = new Property(this);
                Channel.Name = "Amplitude";
            }
            if (string.IsNullOrEmpty(Channel.Name)) Channel.Name = "Amplitude";
            Channel.DebugEnabled = DebugEnabled;
            Channel.Behavior = this;
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.Comp = this;
            Channel.ToProperty.Handler = null;
            Channel.ToProperty.DebugEnabled = DebugEnabled;
            Channel.ToProperty.Name = "Amplitude";
            Channel.OnSetup(this);

            if (Channels == null) Channels = new List<TimeflowChannel>();
            if (!HasChannel(Channel)) Channels.Add(Channel);

            SetupBaking();
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false);
            //if (DebugEnabled) Debug.Log(name + ".AudioSample.Copy:" + src.name);
            if (includeChannels) {
                if (Channel != null) {
                    /// No need to copy the channel since it's a standardized setup
                    RemoveChannel(Channel);
                    Channel = null;
                }
                SetupChannels(true);
            }
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":AudioSample.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

            // Assume the component should also be removed
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#else
            UnityEngine.Object.DestroyImmediate(this);
#endif
        }

        #endregion

        #region UPDATE

        public override void UpdateTime()
        {
            if (!CanUpdate) return;
            base.UpdateTime();
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            base.UpdateTimeChannel(channel);
            //if (DebugEnabled) Debug.Log(name + ".AudioSample.UpdateTimeChannel:" + CurrentTime + " Amplitude:" + Amplitude);
            if (ApplyToScale) {
                transform.localScale = BaseScale + MathUtil.Multiply(Scale, Amplitude);
                //if (DebugEnabled) Debug.Log(name + ".AudioSample.localScale:" + transform.localScale);
            }
            if (SetShaderProperty) {
                if (shaderPropertyID == 0) shaderPropertyID = Shader.PropertyToID(ShaderPropertyName);
                Shader.SetGlobalFloat(ShaderPropertyName, Amplitude);
            }
        }

        public void SampleExplicit()
        {
            if (Spectrum == null) {
                Debug.LogWarning("Please assign an AudioSpectrum");
                return;
            }
            Spectrum.AnalyzeSample(this);
        }

        public override void OnStop()
        {
            base.OnStop();
            if (IsBaking) StopBaking();
        }

        public void OnEndLoop()
        {
            Timeflow.OnEndLoop -= OnEndLoop;
            if (IsBaking) StopBaking();
        }

        #endregion

        #region BAKING

        public void SetupBaking()
        {
            if (!UseBakedData) return;
            if (BakedData == null || BakedData.Length != Timeflow.TotalFrames) {
                //if (DebugEnabled) Debug.Log(name + ".SetupBaking:" + Timeflow.TotalFrames);
                BakedData = new float[Timeflow.TotalFrames];
            }
        }

        public void StartBaking(bool play)
        {
            UseBakedData = true;
            if (!IsBaking) {
                IsBaking = true;
            }
            SetupBaking();

            if (Timeflow != null) {
                Timeflow.OnEndLoop += OnEndLoop;
                if (play) Timeflow.Play(true);
            }
        }

        public void StopBaking()
        {
            if (IsBaking) {
                IsBaking = false;
            }
            if (Timeflow != null && Timeflow.IsPlaying) Timeflow.Stop();
        }

        public void ClearBakedData()
        {
            if (BakedData != null) {
                BakedData = null;
            }
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AudioSample;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Audio/Audio Sample"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    AudioSample comp = Undo.AddComponent<AudioSample>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }

}//AxonGenesis
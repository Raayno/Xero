// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
#endif

namespace AxonGenesis
{
    /// <summary>
    /// A Sequencer channel represents one animation track in Timeflow,
    /// targeting a single input on Sequencer's PlayableGraph (layer mixer).
    /// </summary>
    [Serializable]
    public partial class AnimationSequencerChannel : TimeflowChannel
    {
        #region STATIC

        public static float SkipTolerance = 1f;
        public static readonly Color MixColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

        #endregion

        #region SERIALIZED VARS

        public float Weight = 1f;

        // Optional per-channel AvatarMask to restrict this track's influence.
        [Tooltip("Optional AvatarMask to restrict this track's influence.")]
        public AvatarMask Mask;

        // New: controls whether this channel's AvatarMask layer is additive when applied.
        [Tooltip("If enabled, this channel's masked layer is blended additively.")]
        [SerializeField] private bool _IsAdditive = false;

        #endregion

        #region NON-SERIALIZED VARS

        [NonSerialized] private int _Index;
        [NonSerialized] public AnimationSequencer Sequencer;

        [NonSerialized] private string _CurrentAnimation;
        [NonSerialized] private float LastTime = 0f;
        [NonSerialized] public Keyframe LastKey = null;

        [NonSerialized] public AnimationLayerMixerPlayable LayerMixer;
        [NonSerialized] public AnimationMixerPlayable Mixer;
        [NonSerialized] public bool HasMixer = false;

        private float[] snapTimes = null;
        private float lastSnapThreshold = 0;

        #endregion

        #region ACCESSORS

        public int Index {
            get => _Index;
            set {
                if (_Index != value) {
                    _Index = value;
                }
            }
        }

        public string CurrentAnimation {
            get => _CurrentAnimation;
            set => _CurrentAnimation = value;
        }

        public bool IsAdditive {
            get => _IsAdditive;
            set {
                if (_IsAdditive != value) {
                    _IsAdditive = value;
                    if (LayerMixer.IsValid()) {
                        LayerMixer.SetLayerAdditive((uint)Index, _IsAdditive);
                    }
                }
            }
        }

        public override string Name {
            get => _Name;
            set {
                _Name = value;
                if (string.IsNullOrEmpty(_Name))
                    ValidateName();
            }
        }

        public bool HasStateChanged { get; set; }

        public override bool CanInterpolate {
            get {
                if (_Interpolation == Interpolations.Bezier) _Interpolation = Interpolations.Quadratic;
                return true;
            }
        }

        /// <summary>
        /// Cannnot support linking since keyframe interpolation isn't simply value based, but is
        /// based on playable graphs. This feature might be added in the future, but in the meantime
        /// keyframes must be copied manually to another channel.
        /// </summary>
        public override bool CanLink => false;

        public override bool CanHold {
            get {
                return false;
            }
        }

        #endregion

        #region SETUP

        public void ValidateName()
        {
            if (string.IsNullOrEmpty(Name))
                Name = $"Clip Track {Index}";

            ToProperty ??= new Property();
            ToProperty.Name = Name;
            IsNameCustom = true;
        }

        public AnimationSequencerChannel(AnimationSequencer sequencer) : base(sequencer)
        {
            _Interpolation = Interpolations.Quadratic;
        }

        public void InitGraph(AnimationLayerMixerPlayable layerMixer, int index)
        {
            if (Keys == null || Keys.Count == 0) {
                Mixer = default;
                HasMixer = false;
                return;
            }
            Index = index;
            HasMixer = true;
            LayerMixer = layerMixer;
            if (!LayerMixer.IsValid()) {
                return;
            }

            LayerMixer.SetLayerAdditive((uint)index, IsAdditive);
            if (Mask != null) LayerMixer.SetLayerMaskFromAvatarMask((uint)index, Mask);

            PlayableGraph graph = layerMixer.GetGraph();
            Mixer = AnimationMixerPlayable.Create(graph, Keys.Count);
            graph.Connect(Mixer, 0, layerMixer, index);

            int port = 0;
            foreach (var key in Keys) {
                if (key.CustomKey is AnimationSequencerKey k) {
                    key.Channel = this;
                    k.InitGraph(port);
                    port++;
                }
            }
        }

        public void DestroyGraph()
        {
            if (HasMixer) {
                if (Mixer.IsValid()) {
                    Mixer.Destroy();
                }
                HasMixer = false;
            }
            if (Keys != null) {
                foreach (Keyframe key in Keys) {
                    if(key.CustomKey is AnimationSequencerKey k) {
                        k.DestroyGraph();
                    }
                }
            }
        }

        public override void SetupKeyframes()
        {
            base.SetupKeyframes();
#if UNITY_EDITOR
            ValidateName();
#endif
            if (Keys != null) {
                CanAddRemoveKeys = true;
                SupportsKeyframes = true;

                // Use KeyString but hide it (we drive it via popup)
                ShowString = false;

                foreach (Keyframe key in Keys) {
                    var sk = Sequencer.SetupKey(key, true);
                    sk?.OnValueChanged();
                }
            }
            UpdateTransitionKeys();
        }

        protected override void OnKeyframeAdded(Keyframe key)
        {
            base.OnKeyframeAdded(key);
            Sequencer.ForceRefresh();
        }

        protected override void OnKeyframeRemoved(Keyframe key)
        {
            base.OnKeyframeRemoved(key);
            Sequencer.ForceRefresh();
        }

        public override void ReinstantiateCustomKey(Keyframe key)
        {
            if (key?.CustomKey is AnimationSequencerKey src) {
                key.CustomKey = new AnimationSequencerKey(key, src);
                Sequencer.SetupKey(key, true);
            }
        }

        public override void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            if (src is AnimationSequencerChannel ch) {
                Name = StringUtil.IncrementName(Name);

#if UNITY_EDITOR
                if (includeStyle) {
                    GUIColor = src.GUIColor;
                    GUIHeightOffset = src.GUIHeightOffset;
                }
#endif
                Weight = ch.Weight;
                IsAdditive = ch.IsAdditive;

                Keys = new List<Keyframe>();
                if (ch.Keys != null && ch.Keys.Count > 0) {
                    var copyKeys = new List<Keyframe>(ch.Keys);
                    foreach (Keyframe key in copyKeys)
                        CopyKey(key, 0, false, true);
                }

                OnSetup(Behavior);
            }
        }

        #endregion

        #region UPDATE

        public override void Interpolate(float time, bool apply, bool isLocalTime)
        {
            if (!apply || Sequencer == null || !Sequencer.HasGraph) return;

            float localTime = LoopTime(LocalTime(time, isLocalTime));

            Keyframe a = null;
            Keyframe b = null;
            if (IsLinked && Link.Mode != TimeflowChannelLink.Modes.Off && Link.Enabled) {
                float linkTime = localTime + Link.TimeOffsetWorld;
                linkTime = Link.Channel.LocalTime(linkTime, false);
                b = Link.Channel.GetCurrentOrPrevKey(linkTime, true);
                if (b != null) {
                    a = Link.Channel.GetCurrentOrPrevKey(b.KeyTime - Timeflow.FrameDuration, true);
                }
            }
            else {
                b = GetCurrentOrPrevKey(localTime, true);
                if (b != null) a = GetCurrentOrPrevKey(b.KeyTime - Timeflow.FrameDuration, true);
            }

            AnimationSequencerKey keyA = a == null ? null : a.CustomKey as AnimationSequencerKey ?? null;
            AnimationSequencerKey keyB = b == null ? null : b.CustomKey as AnimationSequencerKey ?? null;

            bool isKeyA = keyA != null && !keyA.IsEmpty;
            bool isKeyB = keyB != null && !keyB.IsEmpty;

            float channelWeight = 1f;
            if (!isKeyA && !isKeyB) {
                channelWeight = 0f;
            }
            else
            if (isKeyB && !isKeyA) {
                channelWeight = keyB.CalculateWeight(localTime);
                keyB.UpdateWeight(1f, localTime);
            }
            else {
                if (keyA.IsTransitionOut) {
                    // Both keys valid: handle crossfade
                    float blendT = keyB.CalculateWeight(localTime);
                    keyA.UpdateWeight(1f - blendT, localTime);
                    keyB.UpdateWeight(blendT, localTime);
                    channelWeight = 1f; // always full weight when crossfading
                }
                else {
                    // No crossfade, just full weight on B
                    channelWeight = keyB.CalculateWeight(localTime); ; // blend the channel weight
                    keyA.UpdateWeight(0f, localTime);
                    keyB.UpdateWeight(1f, localTime);
                }
            }

            Weight = channelWeight;

            // Update the other keys to zero weight
            foreach (var k in Keys) {
                var ak = k.CustomKey as AnimationSequencerKey;
                if (ak == null) continue;
                if (ak != keyA && ak != keyB) {
                    ak.UpdateWeight(0f, localTime);
                }
            }
        }

        public bool CanUpdate(AnimationSequencerKey key, float localTime)
        {
            bool canUpdate = HasStateChanged || HasTimeJumped(localTime) || (LastKey != key.Key);

            if (!canUpdate && CurrentAnimation != key.AnimationName)
                canUpdate = true;

            if (canUpdate)
                LastKey = key.Key;

            LastTime = localTime;
            return canUpdate;
        }

        public bool HasTimeJumped(float localTime)
        {
            if (LastTime > localTime) {
                LastKey = null;
                return true;
            }

            bool skipped = Mathf.Abs(LastTime - localTime) > SkipTolerance;
            if (skipped) LastKey = null;
            return skipped;
        }

        public override void OnRewind()
        {
            base.OnRewind();
            HasStateChanged = true;
        }

        public override bool CustomSnapTime(float time, ref float threshold, out float snapped)
        {
            snapped = time;
            lastSnapThreshold = threshold;
            if (Keys == null || Keys.Count == 0) {
                return false;
            }
            bool wasSnapped = false;

            if (snapTimes == null || snapTimes.Length < 4) snapTimes = new float[4];

            // Snap to the end time of each keyframe (the start time is automatically handled by Timeflow)
            foreach (Keyframe key in Keys) {
                if (key == null) continue;

                AnimationSequencerKey k = key.CustomKey == null ? null : (AnimationSequencerKey)key.CustomKey;
                if (k == null) continue;

                snapTimes[0] = key.KeyTime + k.InBlend;
                snapTimes[1] = key.KeyTime + k.Duration - k.OutBlend;
                snapTimes[2] = key.KeyTime + k.Duration;
                snapTimes[3] = key.KeyTime + k.ClipDuration;

                foreach (float t in snapTimes) {
                    float dif = Mathf.Abs(time - t);
                    if (dif <= threshold) {
                        threshold = dif;// Set new threshold to beat
                        lastSnapThreshold = threshold;
                        snapped = t;
                        wasSnapped = true;
                        // Keep checking in case of a closer match
                    }
                }
            }

            return wasSnapped;
        }

        private void UpdateTransitionKeys()
        {
            if (Keys == null || Keys.Count == 0) return;
            foreach (var key in Keys) {
                if (key.CustomKey is AnimationSequencerKey k) {
                    k.Validate();
                    k.TransitionToKey = null;
                    k.TransitionFromKey = null;
                }
            }

            for (int i = 0; i < Keys.Count; i++) {
                Keyframe k = Keys[i];
                if (k == null) continue;
                var b = k.CustomKey as AnimationSequencerKey;
                if (b == null) continue;

                if (i < Keys.Count - 1) {
                    Keyframe n = Keys[i + 1];
                    var bn = n.CustomKey as AnimationSequencerKey;
                    if (bn != null && n.KeyTimeWorld < k.KeyTimeWorld + b.Duration) {
                        b.TransitionToKey = bn;
                        bn.TransitionFromKey = b;
                    }
                    else {
                        b.TransitionToKey = null;
                        if (bn != null) bn.TransitionFromKey = null;
                    }
                }
                else {
                    b.TransitionToKey = null;
                }
            }
        }

        #endregion
    }
}

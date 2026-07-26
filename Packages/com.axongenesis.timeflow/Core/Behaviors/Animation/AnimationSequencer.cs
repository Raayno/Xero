// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

#if ANIMATIONRIGGING_1_OR_NEWER
using UnityEngine.Animations.Rigging;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [AddComponentMenu("Timeflow/AnimationSequencer")]
    public partial class AnimationSequencer : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        private class AnimationClipInfo
        {
            public AnimationClip Clip;
            public bool IsAdditive;
            public AnimationClip AdditiveReferencePose;

            public AnimationClipInfo(AnimationClip clip, bool additive)
            {
                Clip = clip;
                IsAdditive = additive;
            }
        }

        public enum RiggingModes
        {
            None,
            AnimationRigging,
            MechanimAvatar
        }

        #region SERIALIZED FIELDS

        [SerializeField] private RiggingModes _RiggingMode = RiggingModes.None;

        public AnimationClip DefaultClip;
        public List<AnimationClip> AnimationClips = new();
        public List<AnimationSequencerChannel> SequencerChannels;

        // Add this property to control the blend between original animator and sequencer
        [Range(0, 1)]
        public float BlendWeight = 1.0f;

        public float BlendWeight00 = 1.0f;
        public float BlendWeight01 = 1.0f;
        public float BlendWeight02 = 1.0f;
        public float BlendWeight03 = 1.0f;
        public float BlendWeight04 = 1.0f;
        public float BlendWeight05 = 1.0f;
        public float BlendWeight06 = 1.0f;
        public float BlendWeight07 = 1.0f;
        public float BlendWeight08 = 1.0f;
        public float BlendWeight09 = 1.0f;

        public bool ApplyPlayableIK = true;
        public bool ApplyFootIK = false;
        public bool ApplyRootMotion = false;
        public bool RootMotionSingleTrackOnly = true;
        public bool NormalizeTrackWeights = true;

        [SerializeField] private bool _LateUpdateChecked = false;

        #endregion

        #region NON-SERIALIZED FIELDS

        [NonSerialized] private Animator _Animator;
        [NonSerialized] private PlayableGraph _Graph;
        [NonSerialized] private RuntimeAnimatorController _AnimatorController;
        [NonSerialized] private AnimatorControllerPlayable _AnimatorControllerPlayable;
        [NonSerialized] private AnimationLayerMixerPlayable _LayerMixer; // unified top-level layer mixer
        [NonSerialized] private Playable _OutputToRig; // unified top-level layer mixer
        [NonSerialized] private AnimationPlayableOutput _Output;
        [NonSerialized] private AnimationClipPlayable _DefaultClipPlayable;

        public bool HasAnimatorController { get; private set; } = false;

        public bool HasDefaultClip { get; private set; } = false;

        [NonSerialized] private bool _GraphAutoUpdates;
        [NonSerialized] private bool _IsGraphInitialized;

        [NonSerialized] private string[] _AnimationNames;
        [NonSerialized] private Dictionary<string, AnimationClip> _ClipsByName;

#if ANIMATIONRIGGING_1_OR_NEWER
        [NonSerialized] private RigBuilder _RigBuilder;
        [NonSerialized] private List<MultiAimConstraint> _MultiAimConstraints;
#else
        private bool _WarnedAnimationRigging = false;
#endif
        private float _lastTime = 0f;
        private bool _AreChannelsSetup = false;
        private bool _IsRebinding = false;
        private bool _WarnedSetupKeyNull = false;
        private bool _IsGraphAllocated = false;

        struct ConstraintSnapshot { public Behaviour comp; public float weight; public bool enabled; }
        List<ConstraintSnapshot> constraintSnapshots = new List<ConstraintSnapshot>();

        #endregion

        #region ACCESSORS
        public RiggingModes RiggingMode {
            get => _RiggingMode;
            set {
                if (_RiggingMode != value) {
                    _RiggingMode = value;
                    InitGraph(true);
                }
            }
        }

        public bool IsAnimationRigging => RiggingMode == RiggingModes.AnimationRigging;

#if ANIMATIONRIGGING_1_OR_NEWER
        public bool IsRigBuilder => IsAnimationRigging && _RigBuilder != null;
#else
        public bool IsRigBuilder = false;
#endif
        public bool IsMechanimAvatar => RiggingMode == RiggingModes.MechanimAvatar;

        public bool HasGraph => _Graph.IsValid();

        public bool CanAnimate => Enabled;

        private bool _HasStarted = false;

        public bool HasStarted {
            get => _HasStarted;
            private set {
                _HasStarted = value;
            }
        }

        public override bool SupportsMultipleChannels() => true;

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=orange>OnAwake</color>");
            base.OnAwake();

            if (!_LateUpdateChecked) {
                // Default to LateUpdate for animation sequencing. This ensures that rigs and controllers are updated first
                // otherwise they may override our animation poses. The user can manually change the UpdateMethod after
                // first initialization if desired.
                _LateUpdateChecked = true;
                UpdateMethod = TimeflowBehavior.UpdateMethods.LateUpdate;
            }

            HasStarted = false;
            _IsGraphInitialized = false;
        }

        protected override void OnStart()
        {
            HasStarted = true;
            base.OnStart();
        }

        protected override void OnDestruct()
        {
            if (DebugEnabled) Debug.Log("{GetInstanceID()} <color=red>OnDestruct</color>");
            HasStarted = false;
            DestroyGraph();
            base.OnDestruct();
        }

        protected override void OnDisable()
        {
            DestroyGraph();
            base.OnDisable();
        }

        private void InitGraph(bool force = false)
        {
            if (!HasStarted && Application.isPlaying) {
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>InitGraph skipped - !HasStarted</color>");
                return;
            }
            if (!_AreChannelsSetup) {
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=orange>InitGraph skipped - channels not setup</color>");
                return;
            }
            if (_Animator == null) {
                _Animator = ObjectUtil.GetOrAddComponent<Animator>(gameObject);
            }

            ReloadAnimationClips();
            RebuildClipCache();

            if (_IsGraphInitialized && !force) {
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>InitGraph skipped - _IsGraphInitialized already</color>");
                return;
            }
            if (_IsGraphInitialized) DestroyGraph();

            _IsGraphInitialized = true;

            // Prevent culling (important in edit mode)
            _Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _Animator.applyRootMotion = ApplyRootMotion;
            _AnimatorController = _Animator.runtimeAnimatorController;
            HasAnimatorController = _AnimatorController != null;
            HasDefaultClip = DefaultClip != null;

            if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=orange>InitGraphIntegrated</color> force:{force} HasAnimatorController:{HasAnimatorController} HasDefaultClip:{HasDefaultClip}");

            if (IsAnimationRigging) {
#if ANIMATIONRIGGING_1_OR_NEWER
                if (_RigBuilder == null) {
                    _RigBuilder = gameObject.GetComponent<RigBuilder>();
                }
                if (_RigBuilder != null) {
                    _Graph = default;
                    _RigBuilder.Clear();
                    _RigBuilder.Build();
                }
#else
                if (!_WarnedAnimationRigging) {
                    _WarnedAnimationRigging = true;
                    Debug.LogWarning("Animation Rigging package not installed. Cannot apply RigBuilder graph.", this);
                }
#endif
            }

            if (IsRigBuilder) {
#if ANIMATIONRIGGING_1_OR_NEWER
                _Graph = _RigBuilder.graph;
                _IsGraphAllocated = true;
#endif
            }
            else {
                _Graph = PlayableGraph.Create($"{name}.AnimationSequencer");
                _IsGraphAllocated = true;
            }
            _GraphAutoUpdates = false;
            _Graph.SetTimeUpdateMode(_GraphAutoUpdates ? DirectorUpdateMode.GameTime : DirectorUpdateMode.Manual);

            int layerMixerInputs = Channels == null ? 0 : Channels.Count;
            int nextInputIndex = 0;
            if (HasAnimatorController || HasDefaultClip) {
                layerMixerInputs += 1; // base layer
                nextInputIndex = 1;
            }
            _LayerMixer = AnimationLayerMixerPlayable.Create(_Graph, layerMixerInputs);

            if (IsRigBuilder) {
                // The RigBuilder graph is already outputting to the animator
                _Output = default;

                for (int outIndex = 0; outIndex < _Graph.GetOutputCount(); outIndex++) {
                    var output = _Graph.GetOutput(outIndex);
                    Playable node = output.GetSourcePlayable();
                    if (!node.IsValid()) continue;

                    // we are specifically looking for the AnimationScriptPlayable (or any script-playable used by RigBuilder)
                    var playableType = node.GetPlayableType();
                    if (playableType == typeof(AnimationScriptPlayable)) {
                        // ensure the destination has at least one input
                        if (node.GetInputCount() > 0) continue;

                        // Connect the animation layer mixer to the rig graph
                        node.SetInputCount(1);
                        node.ConnectInput(0, _LayerMixer, 0, BlendWeight);

                        // make sure the weight is set
                        node.SetInputWeight(0, BlendWeight);

                        _OutputToRig = node;
                        //Debug.Log($"Connected layer mixer into rig graph at output index {outIndex}.");
                        break;
                    }
                }
            }
            else {
                // Connect output to the animator
                _Output = AnimationPlayableOutput.Create(_Graph, $"{name}.SequencerOutput", _Animator);
                _Output.SetSourcePlayable(_LayerMixer);
                _Output.SetWeight(BlendWeight);
            }

            int sequencerTopInput = HasAnimatorController || HasDefaultClip ? 1 : 0;
            if (HasAnimatorController) {
                _AnimatorControllerPlayable = AnimatorControllerPlayable.Create(_Graph, _AnimatorController);
                _Graph.Connect(_AnimatorControllerPlayable, 0, _LayerMixer, 0);
            }
            else
            if (HasDefaultClip) {
                _DefaultClipPlayable = AnimationClipPlayable.Create(_Graph, DefaultClip);
                _DefaultClipPlayable.SetApplyFootIK(ApplyFootIK);
                _DefaultClipPlayable.SetApplyPlayableIK(false);
                _Graph.Connect(_DefaultClipPlayable, 0, _LayerMixer, 0);
            }
            if (HasAnimatorController || HasDefaultClip) {
                _LayerMixer.SetInputWeight(0, 1f);
                _LayerMixer.SetLayerAdditive(0u, false);
            }

            if (Channels != null && Channels.Count > 0) {
                foreach (var channel in Channels) {
                    if (channel is AnimationSequencerChannel ch) {
                        ch.InitGraph(_LayerMixer, nextInputIndex);
                        nextInputIndex++;
                    }
                }
            }
            else {
                Debug.LogWarning("AnimationSequencer has no channels to initialize!", this);
            }

            if (gameObject.activeInHierarchy) {
                _Animator.Update(0f); // apply default pose immediately
                _Graph.Play();
            }

            UpdateTime();
        }

        private void DestroyGraph()
        {
            if (_IsGraphAllocated) {
                _IsGraphAllocated = false;
                if (_Graph.IsValid()) _Graph.Destroy();
            }
            if (Channels != null && Channels.Count > 0) {
                foreach (var channel in Channels) {
                    if (channel is AnimationSequencerChannel ch) {
                        ch.DestroyGraph();
                    }
                }
            }
#if ANIMATIONRIGGING_1_OR_NEWER
            if (IsAnimationRigging && _RigBuilder != null) {
                _RigBuilder.Clear();
            }
#endif

            _IsGraphInitialized = false;
            _Output = default;
        }

        public void ForceRefresh()
        {
            Refresh(); // Will init graph
            UpdateTime();
        }

        #endregion

        #region UPDATE

        public override void UpdateTime()
        {
            base.UpdateTime();
            if (!CanAnimate) {
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>UpdateTime skipped - !CanAnimate</color>");
                return;
            }
            if (_IsRebinding) {
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>UpdateTime skipped - _IsRebinding</color>");
                return;
            }
            if (!_IsGraphInitialized || !_LayerMixer.IsValid()) {
                InitGraph(true);
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>UpdateTime skipped - !GraphInitialized</color>");
                return;
            }
            if (ChannelsNeedSetup) {
                SetupChannels(true);
                if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=yellow>UpdateTime skipped - ChannelsNeedSetup</color>");
                return;
            }
            if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=green>UpdateTime</color> CurrentTime:{CurrentTime} lastTime:{_lastTime} BlendWeight:{BlendWeight}");
            if (IsRigBuilder && _OutputToRig.IsValid()) {
                _OutputToRig.SetInputWeight(0, BlendWeight);
            }
            else
            if (_Output.IsOutputValid()) {
                if (HasAnimatorController || HasDefaultClip) {
                    _Output.SetWeight(1f); // Full weight to allow animator controller
                }
                else {
                    _Output.SetWeight(BlendWeight); // Continually update in case changed
                }
            }

            float dtime = CurrentTime - _lastTime; // can be negative on reverse
            double deltaTime = dtime * TimeScale;

            int count = _LayerMixer.GetInputCount();
            float combinedWeight = 0f;
            if (Channels != null && Channels.Count > 0 && count > 1) {
                foreach (var channel in Channels) {
                    if (channel is AnimationSequencerChannel ch) {
                        if (count <= ch.Index) {
                            //Debug.LogWarning($"LayerMixer has insufficient inputs for channel index:{ch.Index}", this);
                            continue;
                        }
                        if (!ch.IsEnabled) {
                            _LayerMixer.SetInputWeight(ch.Index, 0f);
                        }
                        else {
                            float w = GetMappedChannelWeight(ch.Index, 1f) * ch.Weight;

                            if (HasAnimatorController || HasDefaultClip) {
                                w *= BlendWeight;
                            }
                            w = Mathf.Clamp(w, 0f, 1f);
                            _LayerMixer.SetInputWeight(ch.Index, w);
                            combinedWeight += w;
                        }
                    }
                }
            }
            if (HasAnimatorController || HasDefaultClip) {
                // Anything less than 1 introduces a hidden default pose underlying the animation
                _LayerMixer.SetInputWeight(0, 1f);
            }

            // Root Motion handling
            if (!ApplyRootMotion) {
                if (_Animator.applyRootMotion) _Animator.applyRootMotion = false;
            }
            else {
                bool allow = true;
                if (RootMotionSingleTrackOnly && _LayerMixer.IsValid()) {
                    int contributing = 0;
                    int ic = _LayerMixer.GetInputCount();
                    for (int i = 0; i < ic; i++) {
                        if (_LayerMixer.GetInputWeight(i) > 0.0001f) contributing++;
                        if (contributing > 1) break;
                    }
                    allow = (contributing == 1);
                }
                if (_Animator.applyRootMotion != allow) _Animator.applyRootMotion = allow;
            }

            float evalDt = Mathf.Abs((float)deltaTime);
            if (evalDt <= 0f) evalDt = 0.016f;

            // Always evaluate graph with non-negative step (we drive clip times manually)
            if (!_GraphAutoUpdates && _Graph.IsValid()) {
                _Graph.Evaluate(evalDt);
            }

            _lastTime = CurrentTime;
        }

        #endregion

        #region CHANNELS

        public bool ChannelsNeedSetup = false;

        public override void SetupChannels(bool forceSetup)
        {
            if (DebugEnabled) Debug.Log($"{GetInstanceID()} <color=orange>SetupChannels</color> forceSetup:{forceSetup} Enabled:{Enabled}");
            if (!Enabled) return;
            ChannelsNeedSetup = false;
            base.SetupChannels(forceSetup);

            if (SequencerChannels == null || SequencerChannels.Count == 0) {
                AddChannel();
            }

            var channels = new List<AnimationSequencerChannel>(SequencerChannels);
            foreach (var ch in channels) {
                AddChannel(ch);
            }

            ReindexChannels();

            Channels = new List<TimeflowChannel>(channels);
            _AreChannelsSetup = true;
            InitGraph(forceSetup);
        }

        public void ReindexChannels()
        {
            if (SequencerChannels == null || SequencerChannels.Count == 0) return;
            int i = 0;
            if (HasAnimatorController || HasDefaultClip) {
                i = 1;
            }
            foreach (var ch in SequencerChannels) {
                //Debug.Log($"ReindexChannels:{ch.Index} Name:{ch.Name}");
                ch.Index = i;
                i++;
            }
        }

        public void SetupChannel(AnimationSequencerChannel channel)
        {
            channel.SetParent(this);
            channel.Sequencer = this;
            channel.IsDataOnly = true;
            channel.IsCombinedValue = true;
            channel.CanAddRemoveKeys = true;
            channel.SupportsKeyframes = true;
            channel.PropertyType = Property.PropertyTypes.String;

            if (channel.ToProperty == null)
                channel.ToProperty = new Property();

            channel.ToProperty.Owner = this;
            channel.ToProperty.IsDataOnly = true;
            channel.ToProperty.IsCombinedValue = true;
            channel.ToProperty.PropertyType = Property.PropertyTypes.String;
            channel.SetupKeyframes();
        }

        /// <summary>
        /// Creates or updates a SequencerKey stored in the Timeflow Keyframe and resolves the AnimationClip from KeyString.
        /// </summary>
        public AnimationSequencerKey SetupKey(Keyframe key, bool rebuild = false)
        {
            if (key == null) {
                if (!_WarnedSetupKeyNull) {
                    _WarnedSetupKeyNull = true;
                    Debug.LogWarning("Cannot setup null key", this);
                }
                return null;
            }
#if UNITY_EDITOR
            key.IsTrackStyle = true;
#endif
            key.IsCustomType = true;

            var k = key.CustomKey as AnimationSequencerKey;
            if (k == null) {
                var nk = new AnimationSequencerKey(key);
                if (k != null) {
                    nk.Copy(k);
                }
                else {
                    if (key.Channel is AnimationSequencerChannel channel) {
                        var prev = channel.GetPrevKey(key.KeyTime);
                        if (prev != null) {
                            key.KeyString = prev.KeyString;
                            var p = prev.CustomKey as AnimationSequencerKey;
                            nk.Copy(p);
                        }
                        rebuild = true;
                    }
                }
                k = nk;
            }

            k.Key = key;
            k.AnimationSequencer = this;

            if (k.Clip == null && !string.IsNullOrEmpty(key.KeyString) && key.KeyString != "Empty")
                k.Clip = GetClipByName(key.KeyString);

            k.OnValueChanged();

            key.CustomKey = k;
            return k;
        }

        public AnimationSequencerChannel AddChannel()
        {
            var channel = new AnimationSequencerChannel(this);
            AddChannel(channel);
            return channel;
        }

        public override void AddChannel(TimeflowChannel channel)
        {
            if (channel is AnimationSequencerChannel seqCh) {
                if (SequencerChannels == null) SequencerChannels = new List<AnimationSequencerChannel>();
                SetupChannel(seqCh);

                if (!SequencerChannels.Contains(seqCh))
                    SequencerChannels.Add(seqCh);

                base.AddChannel(seqCh);
            }
            else {
                Debug.LogError($"Animation Animation Sequencer failed to add channel of type:{channel.GetType()}", gameObject);
            }
            RenumberChannels();
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            base.RemoveChannel(channel);

            if (channel is AnimationSequencerChannel ch && SequencerChannels != null && SequencerChannels.Contains(ch)) {
                SequencerChannels.Remove(ch);
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            if (src == null) {
                Debug.LogError("Cannot copy null channel");
                return null;
            }
            AnimationSequencerChannel copy = null;
            if (src is AnimationSequencerChannel ch) {
#if UNITY_EDITOR
                UndoUtil.Undo(this, "Duplicate Channels", true);
#endif
                copy = new AnimationSequencerChannel(this);
                copy.Sequencer = this;
                copy.Copy(ch);
            }
            else {
                Debug.LogError($"Sequencer cannot copy this channel type:{src.GetType()}");
            }
            return copy;
        }

        public override TimeflowChannel DuplicateChannel(TimeflowChannel channel, GameObject dstObject = null, bool deleteOriginal = false)
        {
            TimeflowChannel dup = base.DuplicateChannel(channel, dstObject, deleteOriginal);
            if (dup == null) {
                Debug.LogError($"Failed to duplicate channel:{channel.Name}", gameObject);
                return null;
            }
            dup.NewUniqueID();

            if (dstObject == null) {
                // Duplicate the channel to this same Sequencer
                AddChannel(dup);
                SetupChannels(true);
            }
            else {
                // Duplicate the channel to the other Sequencer
                AnimationSequencer sequencer;
                if (dstObject.TryGetComponent<AnimationSequencer>(out sequencer)) {
                    sequencer.AddChannel(dup);
                    sequencer.SetupChannels(true);
                }
                else {
                    Debug.LogError($"Failed to duplicate channel:{channel.Name}", gameObject);
                    return null;
                }
            }
            return dup;
        }

        public void AddAnimationClips(List<AnimationClip> clips, bool addKeyframes = true)
        {
            if (clips == null) return;

            // Ensure we have a channel to add keyframes to if needed
            if (addKeyframes) {
                if (SequencerChannels == null || SequencerChannels.Count == 0) {
                    AddChannel();
                }
                // Guarantee channel index 0 exists
                if (SequencerChannels == null || SequencerChannels.Count == 0) {
                    // If still none, bail out from keyframe creation but continue adding to library
                    addKeyframes = false;
                }
            }

            AnimationSequencerChannel targetChannel = null;
            if (addKeyframes) {
                targetChannel = SequencerChannels[0];
#if UNITY_EDITOR
                Undo.RecordObject(this, "Add Sequencer Clips");
                if (targetChannel != null) Undo.RecordObject(this, "Add Sequencer Clips");
#endif
            }

            // Start time for the sequence
            float startTime = Timeflow.CurrentTime;

            foreach (var c in clips) {
                if (c == null) continue;

                // Maintain the library of available clips
                if (!AnimationClips.Contains(c)) {
                    AnimationClips.Add(c);
                }

                if (!addKeyframes || targetChannel == null) continue;

                // Create a new keyframe at startTime and sequence it
                var key = new Keyframe();
                key.KeyTime = startTime;
                key.KeyString = c.name; // used by SetupKey to resolve the AnimationClip

                // Add to channel first so key.Channel is set (SetupKey may inspect prev keys)
                targetChannel.KeysAdd(key);

                // Setup custom key data for the clip
                var k = SetupKey(key, true);
                if (k == null) continue;

                k.Duration = c.length; // default to full clip length

                // Determine effective duration (fallback if needed)
                float duration = Mathf.Max(0f, k.Duration);
                if (duration <= 0f) {
                    // Fallback: compute from clip and trimmed range/speed
                    float clipLen = Mathf.Max(0f, c.length);
                    float segStart = Mathf.Max(0f, k.StartTime);
                    float segEnd = (k.EndTime > 0f) ? Mathf.Max(segStart, k.EndTime) : clipLen;
                    float segLen = Mathf.Max(0f, segEnd - segStart);
                    float speed = Mathf.Approximately(k.Speed, 0f) ? 1f : Mathf.Abs(k.Speed);
                    duration = Mathf.Max(0.0001f, segLen / speed);
                }

                // Track-style key uses KeyValue as end time on the track
                key.KeyValue = startTime + duration;

                // Advance start time for next key in the sequence
                startTime = key.KeyValue;
            }

            RebuildClipCache();
        }

        public void RenumberChannels()
        {
            if (SequencerChannels == null) return;

            int i = 0;
            foreach (var ch in SequencerChannels.OrderBy(x => x.SortOrder)) {
                if (ch.Name.StartsWith("Clip Track")) {
                    ch.Name = null;
#if UNITY_EDITOR
                    ch.ValidateName();
#endif
                }
                i++;
            }
        }

        public float GetMappedChannelWeight(int index, float defaultWeight)
        {
            switch (index) {
                case 0: return Mathf.Clamp01(BlendWeight00);
                case 1: return Mathf.Clamp01(BlendWeight01);
                case 2: return Mathf.Clamp01(BlendWeight02);
                case 3: return Mathf.Clamp01(BlendWeight03);
                case 4: return Mathf.Clamp01(BlendWeight04);
                case 5: return Mathf.Clamp01(BlendWeight05);
                case 6: return Mathf.Clamp01(BlendWeight06);
                case 7: return Mathf.Clamp01(BlendWeight07);
                case 8: return Mathf.Clamp01(BlendWeight08);
                case 9: return Mathf.Clamp01(BlendWeight09);
                default: return Mathf.Clamp01(defaultWeight);
            }
        }

        /// <summary>
        /// Sets the mapped weight for a zero-based channel index using BlendWeight01..BlendWeight10.
        /// Channels with index >= 10 are ignored.
        /// </summary>
        public void SetMappedChannelWeight(int index, float weight)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Set Channel Weight");
#endif
            weight = Mathf.Clamp01(weight);
            switch (index) {
                case 0: BlendWeight00 = weight; break;
                case 1: BlendWeight01 = weight; break;
                case 2: BlendWeight02 = weight; break;
                case 3: BlendWeight03 = weight; break;
                case 4: BlendWeight04 = weight; break;
                case 5: BlendWeight05 = weight; break;
                case 6: BlendWeight06 = weight; break;
                case 7: BlendWeight07 = weight; break;
                case 8: BlendWeight08 = weight; break;
                case 9: BlendWeight09 = weight; break;
                default: break;
            }
        }
        #endregion

        #region ANIMATION CLIPS

        /// <summary>
        /// Names displayed in the popup. First entry is always "Empty".
        /// </summary>
        public string[] AnimationNames {
            get {
                if (_AnimationNames == null) RebuildClipCache();
                return _AnimationNames;
            }
        }

        /// <summary>
        /// Try to get an AnimationClip by name (case-sensitive).
        /// </summary>
        public AnimationClip GetClipByName(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Empty") return null;
            if (_ClipsByName == null) RebuildClipCache();
            return _ClipsByName != null && _ClipsByName.TryGetValue(name, out var clip) ? clip : null;
        }

        public void Rebind()
        {
            StartCoroutine(_Rebind());
        }

        private System.Collections.IEnumerator _Rebind()
        {
            _IsRebinding = true;
            CaptureConstraintSnapshots();
            yield return null;

#if ANIMATIONRIGGING_1_OR_NEWER
            if (IsAnimationRigging && _RigBuilder != null) {
                _RigBuilder.Clear();
            }
#endif
            SetAllConstraintsWeight(0f);
            yield return new WaitForSeconds(1f);
            yield return null;
            if (_Animator != null) {
                if (DefaultClip != null) AnimationUtil.SamplePose(_Animator, DefaultClip, 0f);
                _Animator.Rebind();
                _Animator.Update(0f);
                yield return null;

                if (DefaultClip != null) AnimationUtil.SamplePose(_Animator, DefaultClip, 0f);
            }

            yield return new WaitForSeconds(1f);
            yield return null;
            SetAllConstraintsWeight(1f);

            yield return new WaitForSeconds(1f);
            yield return null;
            RestoreConstraintSnapshots();

            _IsRebinding = false;
            InitGraph(true);

            yield break;
        }

        private void CaptureConstraintSnapshots()
        {
            constraintSnapshots.Clear();
            // typical constraints: TwoBoneIKConstraint, MultiPositionConstraint, MultiRotationConstraint, etc.
            var constraints = GetComponentsInChildren<Behaviour>(true);
            foreach (var b in constraints) {
                // Quick filter: check known constraint type names or use interfaces in newer versions
                string n = b.GetType().Name;
                bool isConstraint = n.Contains("Constraint") || n.Contains("IK") || n.Contains("Rig");
                if (!isConstraint) continue;

                float weight = GetConstraintWeight(b);

                constraintSnapshots.Add(new ConstraintSnapshot { comp = b, weight = weight, enabled = b.enabled });
            }
        }

        private void SetAllConstraintsWeight(float w)
        {
            foreach (var snap in constraintSnapshots) {
                var b = snap.comp;
                if (b == null) continue;
                SetConstraintWeight(b, w);
            }
        }

        private float GetConstraintWeight(Behaviour b)
        {
#if ANIMATIONRIGGING_1_OR_NEWER
            if (b is RigBuilder rb) {
                return 1f;
            }
            else
            if (b is Rig rig) {
                return rig.weight;
            }
            else
            if (b is MultiParentConstraint mpc) {
                return mpc.weight;
            }
            else
            if (b is TwistChainConstraint tcc) {
                return tcc.weight;
            }
            else
            if (b is TwoBoneIKConstraint tbc) {
                return tbc.weight;
            }
            else
            if (b is MultiReferentialConstraint mrc) {
                return mrc.weight;
            }
            else
            if (b is MultiAimConstraint mac) {
                return mac.weight;
            }
            else
            if (b.GetType().Name.Equals("Rig_References")) {
                return 0;
            }
            else {
                Debug.LogWarning($"Unhandled constraint type:{b.GetType().Name}", this);
            }

            Debug.LogWarning($"Unhandled constraint type:{b.GetType().Name}", this);
#endif
            return 0f;
        }

        private void SetConstraintWeight(Behaviour b, float w)
        {
#if ANIMATIONRIGGING_1_OR_NEWER
            if (b is RigBuilder rb) {
                //rb.weight = w;
            }
            else
            if (b is Rig rig) {
                rig.weight = w;
            }
            else
            if (b is MultiParentConstraint mpc) {
                mpc.weight = w;
            }
            else
            if (b is TwistChainConstraint tcc) {
                tcc.weight = w;
            }
            else
            if (b is TwoBoneIKConstraint tbc) {
                tbc.weight = w;
            }
            else
            if (b is MultiReferentialConstraint mrc) {
                mrc.weight = w;
            }
            else
            if (b is MultiAimConstraint mac) {
                mac.weight = w;
            }
            else
            if (b.GetType().Name.Equals("Rig_References")) {
                // Ignore - not a real constraint
            }
            else {
                Debug.LogWarning($"Unhandled constraint type:{b.GetType().Name}", this);
            }
#endif
        }

        private void RestoreConstraintSnapshots()
        {
            foreach (var snap in constraintSnapshots) {
                var b = snap.comp;
                if (b == null) continue;
                var wField = b.GetType().GetField("weight");
                if (wField != null) {
                    wField.SetValue(b, snap.weight);
                }
                b.enabled = snap.enabled;
            }
        }

        public void RebuildClipCache()
        {
            _ClipsByName ??= new Dictionary<string, AnimationClip>();
            _ClipsByName.Clear();

            // 1) Start with the serialized library (inspector-curated)
            if (AnimationClips != null) {
                foreach (var c in AnimationClips) {
                    if (c == null) continue;
                    if (!_ClipsByName.ContainsKey(c.name))
                        _ClipsByName.Add(c.name, c);
                }
            }

            // 2) Merge in clips from Animator's RuntimeAnimatorController (if present)
            if (TryGetComponent(out Animator anim) && anim.runtimeAnimatorController != null) {
                foreach (var c in anim.runtimeAnimatorController.animationClips) {
                    if (c == null) continue;
                    if (!_ClipsByName.ContainsKey(c.name))
                        _ClipsByName.Add(c.name, c);
                }
            }

            // 3) Merge in clips from legacy Animation component (if present)
            if (TryGetComponent(out Animation legacyAnim)) {
                foreach (AnimationState st in legacyAnim) {
                    var c = st?.clip;
                    if (c == null) continue;
                    if (!_ClipsByName.ContainsKey(c.name))
                        _ClipsByName.Add(c.name, c);
                }
            }

            // Build the display array: "Empty" + sorted names
            var names = new List<string> { "Empty" };
            names.AddRange(_ClipsByName.Keys.OrderBy(n => n, StringComparer.Ordinal));
            _AnimationNames = names.ToArray();
        }

        public void ReloadAnimationClips()
        {
            if (AnimationClips == null) AnimationClips = new List<AnimationClip>();

            if (TryGetComponent(out Animator anim) && anim.runtimeAnimatorController != null) {
                foreach (var c in anim.runtimeAnimatorController.animationClips) {
                    if (c == null) continue;
                    if (!AnimationClips.Contains(c))
                        AnimationClips.Add(c); // keep inspector list in sync
                }
            }

            if (TryGetComponent(out Animation legacyAnim)) {
                foreach (AnimationState st in legacyAnim) {
                    var c = st?.clip;
                    if (c == null) continue;
                    if (!AnimationClips.Contains(c))
                        AnimationClips.Add(c); // keep inspector list in sync
                }
            }
        }


        #endregion
    }
}

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AnimationSequencerKey")]
    public class AnimationSequencerKey : CustomKey
    {
        #region STATIC

        private const float MinDuration = 0.01667f;
        private static AnimationSequencerKey _Default;

        public static AnimationSequencerKey Default {
            get {
                if (_Default == null) _Default = new AnimationSequencerKey(null);
                return _Default;
            }
        }

        #endregion

        #region SERIALIZED VARS

        [Header("Clip")]
        [Tooltip("Unity AnimationClip to play for this key.")]
        [SerializeField]
        [FormerlySerializedAs("Clip")]
        private AnimationClip _Clip;

        [Tooltip("Loop the clip within this key's duration.")]
        public bool Loop = false;

        [Tooltip("Sets the maximum number of loops allowed. A value of 0 is no limit")]
        public int LoopLimit = 0;

        [Tooltip("Playback speed multiplier. 1 = normal speed.")]
        [SerializeField] private float _Speed = 1f;

        [Tooltip("Local start time (seconds) within the AnimationClip.")]
        [SerializeField, FormerlySerializedAs("_ClipOffset")] private float _StartTime = 0f;

        [Tooltip("Local end time (seconds) within the AnimationClip. Defaults to the clip's duration.")]
        [SerializeField] private float _EndTime = 0f;

        [Header("Timing (Track Space)")]
        [Tooltip("How long this key contributes output (in seconds). If 0 or negative, duration is inferred from the trimmed clip length / speed.")]
        [SerializeField] private float _Duration = 1f;

        [Tooltip("Optional blend-in time (seconds).")]
        [SerializeField] private float _InBlend = 0f;

        [Tooltip("Optional blend-out time (seconds).")]
        [SerializeField] private float _OutBlend = 0f;

        [Header("Blend Curves")]
        [Tooltip("Curve to shape the fade-in (0->1). X: normalized time (0..1), Y: weight (0..1).")]
        public AnimationCurve InBlendCurve;

        [Tooltip("Curve to shape the fade-out (1->0). X: normalized time (0..1), Y: weight (0..1).")]
        public AnimationCurve OutBlendCurve;

        [Tooltip("Enables processing foot IK with this animation clip")]
        public bool ApplyFootIK = true;

        [Tooltip("Enables processing foot IK with this animation clip")]
        public bool ApplyPlayableIK = true;

        [Tooltip("Optional UnityEvent to fire while this key is active.")]
        public UnityEvent Event;

        #endregion

        #region NON-SERIALIZED VARS

        [NonSerialized]
        public AnimationSequencer AnimationSequencer;

        [NonSerialized]
        public AnimationSequencerKey TransitionFromKey = null;

        [NonSerialized]
        public AnimationSequencerKey TransitionToKey = null;

        [NonSerialized]
        public int Index = 0;

        private AnimationClipPlayable _Playable = default;

        #endregion

        #region ACCESSORS

        public AnimationClip Clip {
            get { return _Clip; }
            set {
                if (_Clip != value) {
                    _Clip = value;
                    EndTime = ClipDuration; // reset end time to full length
                    SetupPlayableClip(_Clip);
                }
            }
        }

        public float Speed {
            get {
                return _Speed;
            }
            set {
                _Speed = value;
            }
        }

        public float Duration {
            get {
                return _Duration;
            }
            set {
                value = Mathf.Max(0f, value);
                if (_Duration != value) {
                    _Duration = value;
                }
            }
        }

        public float ActualDuration {
            get {
                if (Speed == 0) return 0.001f;
                return _Duration / Speed;
            }
        }

        public float ClipDuration {
            get {
                if (Clip == null) return 1f;
                return Clip.length;
            }
        }

        public float EditDuration {
            get {
                return EndTime - StartTime;
            }
        }

        public float ActualEditDuration {
            get {
                if (Speed == 0) return 0.001f;
                return EditDuration / Speed;
            }
        }

        public float StartTime {
            get { return _StartTime; }
            set {

                value = Mathf.Min(_EndTime - MinDuration, Mathf.Max(0f, value));
                _StartTime = value;
            }
        }

        public float EndTime {
            get { return _EndTime; }
            set {
                value = Mathf.Min(ClipDuration, Mathf.Max(_StartTime + MinDuration, value));
                _EndTime = value;
            }
        }

        public float InBlend {
            get {
                if (IsTransitionIn) {
                    return TransitionFromKey.OutTime - Key.KeyTime;
                }
                return _InBlend;
            }
            set {
                if (IsTransitionIn) {
                    TransitionFromKey.OutTime = Key.KeyTime + value;
                    return;
                }
                _InBlend = Mathf.Min(Mathf.Max(0f, value), Duration - OutBlend);
            }
        }

        public float OutBlend {
            get {
                if (IsTransitionOut) {
                    return OutTime - TransitionToKey.Key.KeyTime;
                }
                return _OutBlend;
            }
            set {
                if (IsTransitionOut) {
                    TransitionToKey.Key.KeyValue = OutTime + value;
                    return;
                }
                _OutBlend = Mathf.Min(Mathf.Max(0f, value), Duration - InBlend);
            }
        }

        public float OutTime {
            get {
                return Key == null ? Duration : Key.KeyTime + Duration;
            }
            set {
                Duration = Mathf.Max(0f, value - (Key == null ? 0f : Key.KeyTime));
            }
        }

        public string AnimationName {
            get {
                if (Clip != null) return Clip.name;
                return Key == null ? null : Key.KeyString;
            }
        }

        public bool IsEmpty => Clip == null || Key == null || string.IsNullOrEmpty(Key.KeyString);

        public bool IsTransitionIn => TransitionFromKey != null;

        public bool IsTransitionOut => TransitionToKey != null;

        #endregion

        #region SETUP

        public AnimationSequencerKey(Keyframe baseKey)
        {
            Key = baseKey;
        }

        public AnimationSequencerKey(Keyframe baseKey, AnimationSequencerKey key)
        {
            Key = baseKey;
            Copy(key);
        }

        public void InitGraph(int index)
        {
            Index = index;
            SetupPlayableClip(Clip);
        }

        public void DestroyGraph()
        {
            if (_Playable.IsValid()) {
                _Playable.Destroy();
            }
        }

        private void SetupPlayableClip(AnimationClip newClip)
        {
            var ch = Key != null ? Key.Channel as AnimationSequencerChannel : null;

            if (!ch.Mixer.IsValid()) return;
            Validate();

            var graph = ch.Mixer.GetGraph();
            if (_Playable.IsValid()) _Playable.Destroy();

            _Playable = AnimationClipPlayable.Create(graph, newClip);
            _Playable.SetSpeed(0); // we drive time manually
            _Playable.SetTime(Mathf.Max(0f, StartTime));
            _Playable.SetApplyFootIK(ApplyFootIK && ch.Sequencer.ApplyFootIK);
            _Playable.SetApplyPlayableIK(ApplyPlayableIK && ch.Sequencer.ApplyPlayableIK);

            int inputs = ch.Mixer.GetInputCount();
            if (Index >= inputs) {
                ch.Mixer.SetInputCount(Index + 1);
            }
            ch.Mixer.ConnectInput(Index, _Playable, 0, 0f);
            ch.Mixer.SetInputWeight(Index, 1f);
        }

        public void UpdateWeight(float weight, float localTime)
        {
            var ch = Key != null ? Key.Channel as AnimationSequencerChannel : null;

            if (!ch.Mixer.IsValid()) {
                return;
            }
            weight = Mathf.Clamp(weight, 0f, 1f);

            ch.Mixer.SetInputWeight(Index, weight);
            if (weight == 0f) {
                return;
            }

            if (!_Playable.IsValid()) return;

            float clipTime = StartTime;

            if (localTime > Key.KeyTime) {
                clipTime = (localTime - Key.KeyTime) * Speed;
                if (Loop) {
                    float editDur = EditDuration;
                    if (editDur > 0f) {
                        while (clipTime > editDur) clipTime -= editDur;
                    }
                }
                clipTime += StartTime;
            }
            _Playable.SetTime(clipTime);
        }

        public float CalculateWeight(float localTime)
        {
            Validate();

            float weight = 0;
            if (localTime < Key.KeyTime || localTime > OutTime) {
                // outside key range
            }
            else
            if (IsTransitionIn && localTime >= Key.KeyTime) {
                float t = TransitionFromKey.OutTime - Key.KeyTime;
                t = (localTime - Key.KeyTime) / t;
                weight = InBlendCurve.Evaluate(t);
            }
            else
            if (InBlend > 0f && localTime < (Key.KeyTime + InBlend)) {
                float t = (localTime - Key.KeyTime) / InBlend;
                weight = InBlendCurve.Evaluate(t);
            }
            else
            if (OutBlend > 0f && localTime > (OutTime - OutBlend)) {
                float t = (localTime - (OutTime - OutBlend)) / OutBlend;
                weight = OutBlendCurve.Evaluate(t);
            }
            else {
                weight = 1f;
            }
            weight = Mathf.Clamp(weight, 0f, 1f);
            return weight;
        }

        public override void OnValueChanged()
        {
            Validate();
        }

        public void Validate()
        {
            if (Key != null && Key.KeyString == "Empty")
                Key.KeyString = null;

            if (Clip == null && AnimationSequencer != null && Key != null && !string.IsNullOrEmpty(Key.KeyString))
                Clip = AnimationSequencer.GetClipByName(Key.KeyString);

            // Ensure default curves
            if (InBlendCurve == null) InBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            if (OutBlendCurve == null) OutBlendCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

            // Clamp fields to sane ranges
            if (float.IsNaN(Speed)) Speed = 1f;
            if (float.IsNaN(_StartTime) || _StartTime < 0f) _StartTime = Mathf.Max(0f, _StartTime);
            if (float.IsNaN(_EndTime) || _EndTime < 0f) _EndTime = Mathf.Max(0f, _EndTime);
            if (float.IsNaN(_InBlend) || _InBlend < 0f) _InBlend = Mathf.Max(0f, _InBlend);
            if (float.IsNaN(_OutBlend) || _OutBlend < 0f) _OutBlend = Mathf.Max(0f, _OutBlend);
            if (LoopLimit < 0) LoopLimit = 0; // non-negative loops (0 = unlimited)

            float totalBlend = _InBlend + _OutBlend;
#if UNITY_EDITOR
            if (_IsDragging) {
                float cachedTotalBlend = _CachedInBlend + _CachedOutBlend;
                if (cachedTotalBlend > 0f && _Duration < cachedTotalBlend) {
                    // If the blends are longer than the duration, scale them down proportionally
                    float scale = _Duration / cachedTotalBlend;
                    _InBlend = _CachedInBlend * scale;
                    _OutBlend = _CachedOutBlend * scale;
                }
                else
                if (_CachedInBlend > _InBlend) {
                    _InBlend = Mathf.Min(_Duration, _CachedInBlend);
                }
            }
            else
#endif
            if ((_OutBlend > 0f || _InBlend > 0f) && _Duration < totalBlend) {
                // If the blends are longer than the duration, scale them down proportionally
                float scale = _Duration / totalBlend;
                _InBlend *= scale;
                _OutBlend *= scale;
            }
            else
            if (_InBlend > _Duration) {
                _InBlend = _Duration;
            }

            // Ensure key end time matches duration - bypass on change notification
            Key.SetKeyValueExplicit(OutTime);

            if (!IsEmpty && Clip != null) {
                var clipLen = Mathf.Max(0.0001f, Clip.length);

                if (_EndTime <= 0f) _EndTime = clipLen;

                // Clamp Start <= End and to clip bounds
                _StartTime = Mathf.Clamp(_StartTime, 0f, clipLen);
            }
        }

        public override void Copy(CustomKey from)
        {
            var orig = (AnimationSequencerKey)from;
            if (orig != null) {
                //Key = orig.Key;
                AnimationSequencer = orig.AnimationSequencer;

                // core
                _Clip = orig.Clip;
                Speed = orig.Speed;
                _StartTime = orig._StartTime;
                _EndTime = orig._EndTime;
                Loop = orig.Loop;
                LoopLimit = orig.LoopLimit;

                // timing
                Duration = orig.Duration;
                InBlend = orig.InBlend;
                OutBlend = orig.OutBlend;

                // curves
                InBlendCurve = orig.InBlendCurve != null ? new AnimationCurve(orig.InBlendCurve.keys) : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                OutBlendCurve = orig.OutBlendCurve != null ? new AnimationCurve(orig.OutBlendCurve.keys) : AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

                // ui/events
                Event = orig.Event;

                AnimationSequencer.ChannelsNeedSetup = true;
            }
            OnValueChanged();
        }

        public void Reset()
        {
            StartTime = 0;
            EndTime = ClipDuration;
            Speed = 1f;
            InBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            OutBlendCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            InBlend = 0f;
            OutBlend = 0f;

            OnValueChanged();
        }

        public void ResetBlendInCurve()
        {
            InBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            OnValueChanged();
        }

        public void ResetBlendOutCurve()
        {
            OutBlendCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            OnValueChanged();
        }

        #endregion

#if UNITY_EDITOR
        public GUIRect InBlendHandleGUIRect = new GUIRect(0, 0, 0, 0);
        public GUIRect OutBlendHandleGUIRect = new GUIRect(0, 0, 0, 0);
        public GUIRect OutTimeHandleGUIRect = new GUIRect(0, 0, 0, 0);
        public GUIRect StartTimeHandleGUIRect = new GUIRect(0, 0, 0, 0);
        public GUIRect LoopGUIRect = new GUIRect(0, 0, 0, 0);

        public bool ShowClipRanges = true;
        public bool ForceShowClipRanges = false;

        private bool _IsDragging = false;
        private float _CachedTime = 0;
        private float _CachedValue = 0;
        private float _CachedDuration = 0;
        private float _CachedStartTime = 0;
        private float _CachedEndTime = 0;
        private float _CachedInBlend = 0;
        private float _CachedOutBlend = 0;
        private AnimationSequencerKey _CachedTransitionFromKey = null;
        private AnimationSequencerKey _CachedTransitionToKey = null;

        public void OnDragStart()
        {
            _IsDragging = true;

            // Cache the original values
            _CachedTime = Key.KeyTime;
            _CachedValue = Key.KeyValue;
            _CachedDuration = Duration;
            _CachedStartTime = StartTime;
            _CachedEndTime = EndTime;
            _CachedInBlend = InBlend;
            _CachedOutBlend = OutBlend;
            _CachedTransitionFromKey = TransitionFromKey;
            _CachedTransitionToKey = TransitionToKey;
        }

        public void OnDragCanceled()
        {
            _IsDragging = false;

            // Restore the cached values
            Key.KeyTime = _CachedTime;
            Key.KeyValue = _CachedValue;
            _Duration = _CachedDuration;
            _StartTime = _CachedStartTime;
            _EndTime = _CachedEndTime;
            _InBlend = _CachedInBlend;
            _OutBlend = _CachedOutBlend;
            TransitionFromKey = _CachedTransitionFromKey;
            TransitionToKey = _CachedTransitionToKey;
        }

        public void OnDragEnded()
        {
            _IsDragging = false;
        }

#endif
    }
}

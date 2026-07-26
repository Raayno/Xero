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
using UnityEngine.Serialization;
using Spine.Unity;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [RequireComponent(typeof(ISkeletonAnimation))]
    [AddComponentMenu("Timeflow/Spine Animator")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/spine-animator")]
    public class SpineAnimator : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        [FormerlySerializedAs("TimeScale")]
        public float SpineTimeScale = 1f;
        public bool InitialFlipX = false;
        public bool InitialFlipY = false;
        public Vector2 Scale = Vector2.one;

        public List<SpineChannel> SpineChannels;

        [NonSerialized] public Spine.Skeleton Skeleton;
        [NonSerialized] public SkeletonAnimation _SkeletonAnimation;
        [NonSerialized] public SkeletonGraphic _SkeletonGraphic;
        [NonSerialized] public ISkeletonAnimation SkeletonAnimation;

        [NonSerialized] public string[] AnimationNames = null;
        [NonSerialized] public Spine.Animation[] Animations = null;

        private Vector2 initialScale;
        private Canvas canvas;

        private bool _IsFlipX = false;
        private bool _IsFlipY = false;
        private bool _IsFlipChanged = false;

        private bool _IsAnimationReady;

        public bool IsAnimationReady {
            get { return _IsAnimationReady; }
            private set {
                _IsAnimationReady = value;
            }
        }

        public Spine.AnimationState AnimationState {
            get {
                if (HasSkeletonGraphic) {
                    return _SkeletonGraphic.AnimationState;
                }
                else
                if (HasSkeletonAnimation) {
                    return _SkeletonAnimation.AnimationState;
                }
                return null;
            }
        }

        public bool HasSkeleton { get; private set; }

        public bool HasSkeletonAnimation { get; private set; }

        public bool HasAnimationState => AnimationState != null;

        public bool HasSkeletonGraphic { get; private set; }

        public bool CanAnimate => HasAnimationState && HasSkeleton && Enabled;

        public bool IsFlipX {
            get { return _IsFlipX; }
            set {
                if (_IsFlipX != value) {
                    _IsFlipX = value;
                    _IsFlipChanged = true;
                }
            }
        }

        public bool IsFlipY {
            get { return _IsFlipY; }
            set {
                if (_IsFlipY != value) {
                    _IsFlipY = value;
                    _IsFlipChanged = true;
                }
            }
        }

        protected override void OnAwake()
        {
            Init();
            base.OnAwake();
        }

        protected override void OnEnable()
        {
            Init();
            base.OnEnable();
        }

        private void Init(bool force = false)
        {
            bool awaitingRebuld = false;
            //if (DebugEnabled) Debug.Log($"{name}.Init");

            initialScale = new Vector2(InitialFlipX ? -1f : 1f, InitialFlipY ? -1f : 1f);

            if (force || _SkeletonAnimation == null) TryGetComponent<SkeletonAnimation>(out _SkeletonAnimation);
            HasSkeletonAnimation = _SkeletonAnimation != null;
            //if (DebugEnabled) Debug.Log($"{name}.Init: HasSkeletonAnimation:{HasSkeletonAnimation}");

            if (_SkeletonAnimation != null) {
                HasSkeletonGraphic = false;
                SkeletonAnimation = _SkeletonAnimation;

                // Allow Timeflow to control update timing
                _SkeletonAnimation.UpdateTiming = UpdateTiming.ManualUpdate;

                if (_SkeletonAnimation.valid) {
                    IsAnimationReady = true;
                    //if (DebugEnabled) Debug.Log($"{name}.Init: IsAnimationReady:{IsAnimationReady}");
                }
                else
                if (Application.isPlaying) {
                    //if (DebugEnabled) Debug.Log($"{name}.Init: OnAnimationRebuild");
                    _SkeletonAnimation.OnAnimationRebuild += OnAnimationReady;
                    awaitingRebuld = true;
                }
            }
            else {
                if (force || _SkeletonGraphic == null) {
                    if (!TryGetComponent<SkeletonGraphic>(out _SkeletonGraphic)) {
                        Debug.LogWarning("The Spine game object is missing an ISkeletonAnimation component");
                    }
                }
                HasSkeletonGraphic = _SkeletonGraphic != null;
                SkeletonAnimation = _SkeletonGraphic;


                canvas = ObjectUtil.GetComponentInSelfOrAncestors<Canvas>(gameObject);

                //if (DebugEnabled) Debug.Log($"{name}.Init: HasSkeletonGraphic:{HasSkeletonGraphic}");
                if (HasSkeletonGraphic) {
                    // Allow Timeflow to control update timing
                    _SkeletonGraphic.UpdateTiming = UpdateTiming.ManualUpdate;

                    if (_SkeletonGraphic.IsValid) {
                        IsAnimationReady = true;
                    }
                    else
                    if (Application.isPlaying) {
                        _SkeletonGraphic.OnAnimationRebuild += OnAnimationReady;
                        awaitingRebuld = true;
                    }
                }
            }
            if (SkeletonAnimation == null) {
                HasSkeleton = false;
                Debug.LogWarning("The Spine game object is missing an ISkeletonAnimation component");
            }
            else {
                if (Skeleton == null) Skeleton = SkeletonAnimation.Skeleton;
                HasSkeleton = Skeleton != null;
                //if (DebugEnabled) Debug.Log($"{name}.Init: HasSkeleton:{HasSkeleton}");
            }

            if (!awaitingRebuld) {
                OnAnimationReady(SkeletonAnimation);
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            _IsFlipChanged = true;
            Init(true);
            UpdateTime();
        }

        private void OnAnimationReady(ISkeletonAnimation animated)
        {
            IsAnimationReady = true;
            //if (DebugEnabled) Debug.Log($"{name}.OnAnimationReady isPlaying:{Application.isPlaying} HasAnimationState:{HasAnimationState}");
        }

        public void FlipHorizontal()
        {
            IsFlipX = !IsFlipX;
        }

        public void FlipVertical()
        {
            IsFlipY = !IsFlipY;
        }

        private void SetupAnimations()
        {
            if (!HasSkeleton || Animations != null) return;

            if (Skeleton.Data == null) {
                Debug.LogWarning($"Skeleton.Data is NULL");
                return;
            }
            if (Skeleton.Data.Animations == null) {
                Debug.LogWarning($"Skeleton.Data.Animations is NULL");
                return;
            }

            // Get the animation meta data from the skeleton to dislay in menus and for keyframe context
            Spine.ExposedList<Spine.Animation> animations = Skeleton.Data.Animations;
            if (animations == null) {
                Debug.LogWarning($"No animations were loaded from the Spine Skeleton");
                Animations = null;
                AnimationNames = null;
                return;
            }
            if (animations != null) {
                Animations = new Spine.Animation[animations.Count + 1];
                Animations[0] = null;
                AnimationNames = new string[animations.Count + 1];
                AnimationNames[0] = "Empty";

                int i = 1;
                foreach (Spine.Animation anim in animations) {
                    Animations[i] = anim;
                    AnimationNames[i] = anim.Name;
                    i++;
                }
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            Init();
            if (!Enabled) return;
            base.SetupChannels(forceSetup);
            //if (DebugEnabled) Debug.Log($"{name}.SetupChannels: forceSetup:{forceSetup}");

            if (SpineChannels == null || SpineChannels.Count == 0) {
                AddChannel();
            }

            int i = 0;
            List<SpineChannel> channels = new List<SpineChannel>(SpineChannels);
            foreach (SpineChannel ch in channels) {
                // Index specifies the Spine animation track
                ch.Index = i;
                AddChannel(ch);
                i++;
            }

            Channels = new List<TimeflowChannel>(channels);

            SetupAnimations();
        }

        public void SetupChannel(SpineChannel channel)
        {
            //if (DebugEnabled) Debug.Log($"{name}.SetupChannel:{channel.Name} Index:{channel.Index}");
            channel.SetParent(this);
            channel.Spine = this;
            channel.IsDataOnly = true;
            channel.IsCombinedValue = true;
            channel.CanAddRemoveKeys = true;
            channel.SupportsKeyframes = true;
            channel.PropertyType = Property.PropertyTypes.String;
            if (channel.ToProperty == null) {
                channel.ToProperty = new Property();
            }

            channel.ToProperty.Owner = this;
            channel.ToProperty.IsDataOnly = true;
            channel.ToProperty.IsCombinedValue = true;
            channel.ToProperty.PropertyType = Property.PropertyTypes.String;
            channel.SetupKeyframes();
        }

        public void AddChannel()
        {
            // Use a string data channel as the basis for a spine animation track channel
            SpineChannel channel = new SpineChannel(this);
            AddChannel(channel);
        }

        public override void AddChannel(TimeflowChannel channel)
        {
            if (channel is SpineChannel spineCh) {
                if (SpineChannels == null) SpineChannels = new List<SpineChannel>();
                SetupChannel(spineCh);

                if (!SpineChannels.Contains(spineCh)) SpineChannels.Add(spineCh);

                base.AddChannel(spineCh);
            }
            else {
                Debug.LogError($"SpineAnimator failed to add channel of type:{channel.GetType()}", gameObject);
            }
            RenumberChannels();
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            base.RemoveChannel(channel);
            if (channel is SpineChannel ch) {
                if (SpineChannels.Contains(ch)) {
                    SpineChannels.Remove(ch);
                }
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            if (src == null) {
                Debug.LogError("Cannot copy null channel");
                return null;
            }
            SpineChannel copy = null;
            if (src is SpineChannel spineCh) {
#if UNITY_EDITOR
                UndoUtil.Undo(this, "Duplicate Channels", true);
#endif
                copy = new SpineChannel(this);
                copy.Spine = this;
                copy.Copy(spineCh);
            }
            else {
                Debug.LogError($"Spine Animator cannot copy this channel type:{src.GetType()}");
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
                // Duplicate the channel to this same SpineAnimator
                AddChannel(dup);
                SetupChannels(true);
            }
            else {
                // Duplicate the channel to the other SpineAnimator
                SpineAnimator spineAnimator;
                if (dstObject.TryGetComponent<SpineAnimator>(out spineAnimator)) {
                    spineAnimator.AddChannel(dup);
                    spineAnimator.SetupChannels(true);
                }
                else {
                    Debug.LogError($"Failed to duplicate channel:{channel.Name}", gameObject);
                    return null;
                }
            }
            return dup;
        }

        public override void DeleteAllChannels()
        {
            base.DeleteAllChannels();
            SpineChannels = null;
        }

        public void RenumberChannels()
        {
            if (SpineChannels != null) {
                int i = 0;
                foreach (var ch in SpineChannels.OrderBy(x => x.SortOrder)) {
                    ch.Index = i;
                    if (ch.Name.StartsWith("Spine Track")) {
                        ch.Name = null;
#if UNITY_EDITOR
                        ch.ValidateName();
#endif
                    }
                    i++;
                }
            }
        }

        public SpineKey SetupKey(Keyframe key, bool rebuild = false)
        {
            if (key == null) return null;
            //if (DebugEnabled) Debug.Log($"{name}.SetupKey:{key.KeyString}");
            key.IsCustomType = true;

            SpineKey k = key.CustomKey == null ? null : (SpineKey)key.CustomKey;
            if (k == null) {
                k = new SpineKey();
                if (key.Channel is SpineChannel channel) {
                    if (channel != null) {
                        Keyframe prev = channel.GetPrevKey(key.KeyTime);
                        if (prev != null) {
                            // When inserting a new key, copy the settings from the last keyframe
                            key.KeyString = prev.KeyString;

                            SpineKey p = (SpineKey)prev.CustomKey;
                            if (p != null) {
                                k.Loop = p.Loop;
                                k.FlipX = p.FlipX;
                                k.FlipY = p.FlipY;
                                k.AllTracks = p.AllTracks;
                                k.TintColor = p.TintColor;
                            }
                        }
                    }
                    rebuild = true;
                }
            }
            k.Key = key;
            k.Spine = this;
            key.CustomKey = k;

            return k;
        }

        public void GetKeyDuration(SpineKey key)
        {
            if (key == null) return;
            if (key.IsEmpty) {
                key.Duration = key.MixDuration;
            }
            else {
                key.Duration = GetAnimationDuration(key.AnimationName);
            }
        }

        public int GetAnimationIndex(string animation)
        {
            if (AnimationNames == null || animation == "Empty") return 0;
            int index = 0;
            for (int i = 0; i < AnimationNames.Length; i++) {
                if (AnimationNames[i] == animation) {
                    index = i;
                    break;
                }
            }
            //if (DebugEnabled) Debug.Log($"{name}.GetAnimationIndex:{animation} index:{index}");
            return index;
        }

        public float GetAnimationDuration(string animation)
        {
            if (Animations == null) return 0;
            int i = GetAnimationIndex(animation);
            float dur = Animations[i].Duration;
            //if (DebugEnabled) Debug.Log($"{name}.GetAnimationDuration:{animation} dur:{dur}");
            return dur;
        }

        public override void UpdateTime()
        {
            base.UpdateTime();
            if (!CanAnimate) {
                //if (DebugEnabled) Debug.Log($"{name} !CanAnimate");
                // Prevents channels from being drawn or processed in any way
                if (Channels != null) Channels = null;
                return;
            }
            //if (DebugEnabled) Debug.Log($"{name} UpdateTime({LocalDeltaTime})");

            AnimationState.TimeScale = SpineTimeScale;

            if (_IsFlipChanged) {
                _IsFlipChanged = false;
                Vector3 localScale = transform.localScale;
                transform.localScale = new Vector3(
                    initialScale.x * Mathf.Abs(localScale.x) * (IsFlipX ? -1f : 1f),
                    initialScale.y * Mathf.Abs(localScale.y) * (IsFlipY ? -1f : 1f),
                    localScale.z);
            }

            if (HasSkeletonGraphic) {
                _SkeletonGraphic.Update(LocalDeltaTime);
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    if (canvas != null) {
                        EditorUtility.SetDirty(canvas);
                    }
                }
#endif
            }
            else
            if (HasSkeletonAnimation) {
                //if (DebugEnabled) Debug.Log($"{name} _SkeletonAnimation.Update({LocalDeltaTime})");
                _SkeletonAnimation.Update(LocalDeltaTime);
                _SkeletonAnimation.LateUpdate();
            }
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            //if (!CanAnimate) {
            //    //if (DebugEnabled) Debug.Log($"{name}.UpdateTimeChannel !CanAnimate");
            //    // Prevents channels from being drawn or processed in any way
            //    if (Channels != null) Channels = null;
            //    return;
            //}
            //if (DebugEnabled) Debug.Log($"{name}.UpdateTimeChannel:{channel.Name} time:{channel.CurrentTime}");
            base.UpdateTimeChannel(channel);
        }

        public override void OnPlay()
        {
            if (!CanAnimate) return;
            base.OnPlay();
            //if (DebugEnabled) Debug.Log($"{name}.OnPlay:{(SpineChannels == null ? "No Channels" : SpineChannels.Count)}");
            ClearState();
        }

        public override void OnRewind()
        {
            if (!CanAnimate) return;
            base.OnRewind();
            //if (DebugEnabled) Debug.Log($"{name}.OnRewind");
            ClearState();
        }

        public void ClearState()
        {
            if (!CanAnimate) return;
            //if (DebugEnabled) Debug.Log($"{name}.ClearState");

            // Clear all animations
            if (HasSkeletonAnimation) _SkeletonAnimation.ClearState();
            if (HasSkeletonGraphic) _SkeletonGraphic.Initialize(true);
            if (HasAnimationState) AnimationState.ClearTracks();

            // Force local scale to update
            _IsFlipChanged = true;

            if (SpineChannels == null) return;
            foreach (SpineChannel ch in SpineChannels) {
                ch.HasStateChanged = true;
            }
        }

        public void ClearTrack(int index)
        {
            if (!CanAnimate) return;
            if (HasAnimationState) AnimationState.ClearTrack(index);
        }

        public override bool SupportsMultipleChannels()
        {
            return true;
        }


#if UNITY_EDITOR
        public static void SetupTimeflowObject(GameObject obj)
        {
            if (obj.TryGetComponent<ISkeletonAnimation>(out var skeletonAnimation)) {
                if (!obj.TryGetComponent<SpineAnimator>(out var comp)) {
                    Undo.AddComponent<SpineAnimator>(obj);
                }
            }
        }

        public override Texture2D Icon => AxonUI.Icons.SpineAnimator;

        public void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            AxonGUI.BeginBox();

            int count = selectedKeys.Count;
            string animVal = null;
            bool loopVal = false;
            bool flipXVal = false;
            bool flipYVal = false;
            bool allVal = false;
            float durVal = 0f;
            float mixVal = 0f;
            Color tintVal = Color.white;

            bool first = true;
            bool isAnimSame = true;
            bool isLoopSame = true;
            bool isFlipXSame = true;
            bool isFlipYSame = true;
            bool isAllSame = true;
            bool isDurationSame = true;
            bool isTintSame = true;
            bool anyEmpty = false;

            foreach (Keyframe key in selectedKeys) {
                SpineKey k = SetupKey(key);
                if (k == null) continue;
                if (first) {
                    first = false;
                    animVal = key.KeyString;
                    loopVal = k.Loop;
                    flipXVal = k.FlipX;
                    flipYVal = k.FlipY;
                    mixVal = k.MixDuration;
                    allVal = k.AllTracks;
                    durVal = k.Duration;
                    tintVal = k.TintColor;
                }
                else {
                    if (isAnimSame && animVal != key.KeyString) {
                        isAnimSame = false;
                    }
                    if (isLoopSame && loopVal != k.Loop) {
                        isLoopSame = false;
                    }
                    if (isFlipXSame && flipXVal != k.FlipX) {
                        isFlipXSame = false;
                    }
                    if (isFlipYSame && flipYVal != k.FlipY) {
                        isFlipYSame = false;
                    }
                    if (isDurationSame && mixVal != k.MixDuration) {
                        isDurationSame = false;
                    }
                    if (isAllSame && allVal != k.AllTracks) {
                        isAllSame = false;
                    }
                    if (isTintSame && tintVal != k.TintColor) {
                        isTintSame = false;
                    }
                }
                if (k.IsEmpty) anyEmpty = true;
            }

            AxonGUI.BeginChangeCheck();
            AxonGUI.BeginHorizontal();

            string inAnim = animVal;
            if (string.IsNullOrEmpty(inAnim)) inAnim = "Empty";
            AxonGUI.UndoName = "Set Animation";
            AxonGUI.SetTooltip("Specifies the animation to play. These animations are defined in the Spine Skeleton Animation.");
            string outAnim = AxonGUI.FieldPopupStringInline(this, inAnim, AnimationNames, GUILayout.Width(150));
            if (inAnim != outAnim) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    key.KeyString = outAnim;
                }
            }

            AxonGUI.UndoName = "Set Mix Duration";
            AxonGUI.SetTooltip("Sets the duration in time to blend the animation in with the current state.");
            float outMix = AxonGUI.FieldTimeInline(this, "Mix", mixVal);
            if (mixVal != outMix) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.MixDuration = outMix;
                }
            }

            if (!anyEmpty || count > 1) { // Hide Loop for Empty animations but don't stop bulk edit
                bool inLoop = loopVal;
                AxonGUI.UndoName = "Set Loop";
                AxonGUI.SetTooltip("Sets whether the animation loops or plays just once.");
                bool outLoop = AxonGUI.FieldToggleInline(this, "Loop", inLoop);
                if (inLoop != outLoop) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        SpineKey k = SetupKey(key);
                        k.Loop = outLoop;
                    }
                }
            }

            bool inFlipX = flipXVal;
            AxonGUI.SetTooltip("");
            AxonGUI.UndoName = "Set Flip X";
            bool outFlipX = AxonGUI.FieldToggleInline(this, "Flip X", inFlipX);
            if (inFlipX != outFlipX) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.FlipX = outFlipX;
                }
            }

            bool inFlipY = flipYVal;
            AxonGUI.SetTooltip("");
            AxonGUI.UndoName = "Set Flip Y";
            bool outFlipY = AxonGUI.FieldToggleInline(this, "Flip Y", inFlipY);
            if (inFlipY != outFlipY) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.FlipY = outFlipY;
                }
            }
            AxonGUI.EndHorizontal(false);

            AxonGUI.BeginHorizontal();

            AxonGUI.SetTooltip("Displays the length of the selected animation. This value cannot be changed.");
            AxonGUI.BeginDisabledGroup(true);
            AxonGUI.FieldTime(this, "Duration", durVal);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set All Tracks";
            AxonGUI.SetTooltip("If enabled, all animation tracks in the skeleton animation are set by this keyframe.");
            bool outAll = AxonGUI.FieldToggleInline(this, "All Tracks", allVal);
            if (allVal != outAll) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.AllTracks = outAll;
                }
            }

            AxonGUI.UndoName = "Set Tint Color";
            AxonGUI.SetTooltip("Sets a delay time for the animation track, or sets the mix duration for an Empty animation.");
            Color outTint = AxonGUI.FieldColorInline(this, "Tint", tintVal, false);
            if (tintVal != outTint) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.TintColor = outTint;
                }
            }
            if (AxonGUI.ButtonRefresh("Reset the tint color to default for the selected keyframes.")) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    SpineKey k = SetupKey(key);
                    k.TintColor = SpineKey.DefaultTintColor;
                }
            }
            AxonGUI.EndHorizontal(false);

            AxonGUI.Space();
            AxonGUI.EndBox();
            if (AxonGUI.EndChangeCheck()) {
                Refresh();
            }
        }

        public static void AddMenuItem()
        {
            if (TimeflowContext.Obj == null) return;
            if (TimeflowContext.DisplayMode != TimeflowContext.DisplayModes.Object) return;

            SpineAnimator spineAnimator;
            TimeflowContext.Obj.TryGetComponent<SpineAnimator>(out spineAnimator);
            if (spineAnimator == null) {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Spine Animator"), false, GUIMenu_Add, null);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Spine Animator/Add Track"), false, SpineChannel.GUIMenu_AddTrack, null);
                TimeflowContext.Menu.AddSeparator("");
                TimeflowContext.Menu.AddItem(new GUIContent("Renumber Spine Tracks"), false, SpineChannel.GUIMenu_RenumberTracks, null);
            }
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;
                    if (!obj.TryGetComponent<SkeletonAnimation>(out var skel)) {
                        if (!obj.TryGetComponent<SkeletonGraphic>(out var grap)) {
                            Debug.LogWarning($"The game object {obj.name} is missing an ISkeletonAnimation component", obj);
                            continue;
                        }
                    }

                    SpineAnimator spineAnimator = Undo.AddComponent<SpineAnimator>(obj.gameObject);
                    if (spineAnimator != null) {
                        spineAnimator.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(spineAnimator.Channels[0]);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif
    }
}

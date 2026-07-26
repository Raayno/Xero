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
    /// <summary>
    /// This works with both the legacy Animation and Animator components. It creates an animateable
    /// channel for each animation track detected, allowing you to keyframe the playback of each channel.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Animation Clips")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/animation-clips")]
    sealed public class AnimationClips : TimeflowBehavior
    {
        public List<AnimationChannel> AnimationChannels;
        public int ActiveChannel = -1;

        [NonSerialized]
        private Animator _animator;

        [NonSerialized]
        private Animation _anim;

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log("AnimationClips.OnAwake");
            base.OnAwake();

            if (ParentObject != null) {
                ParentObject.UpdateAnimation = false;
            }

            TryGetComponent<Animator>(out _animator);
            TryGetComponent<Animation>(out _anim);

            // Disable the animator component (but don't remove it) in order to take over animation updates
            if (_animator != null) {
                _animator.enabled = false;
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + ".AnimationClips.SetupChannels");
            // Disable the parent animation update so only this component updates animation/animator components
            if (ParentObject != null) ParentObject.UpdateAnimation = false;

            Channels = new List<TimeflowChannel>();
            if (AnimationChannels != null && AnimationChannels.Count > 0) {
                int i = 0;
                foreach (AnimationChannel ch in AnimationChannels) {
                    if (ch != null) {
                        ch.OnSetup(this);
                        ch.Index = i;
                        Channels.Add(ch);
                    }
                    i++;
                }
            }
        }

        protected override void OnDestruct()
        {
            if (AnimationChannels != null && Channels != null) {
                // Remove from the base class list to avoid double serialization
                foreach (TimeflowChannel ch in AnimationChannels) {
                    if (HasChannel(ch)) Channels.Remove(ch);
                    base.RemoveChannel(ch);
                }
            }
            base.OnDestruct();
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false);
            //if (DebugEnabled) Debug.Log(name + ".AnimationClips.Copy:" + src.name);

            if (includeChannels) {
                AnimationChannels = new List<AnimationChannel>();
                AnimationClips srcClips = src as AnimationClips;
                if (srcClips != null && srcClips.AnimationChannels != null && srcClips.AnimationChannels.Count > 0) {
                    foreach (AnimationChannel ch in srcClips.AnimationChannels) {
                        if (ch != null && (ch.IsSelected || !includeChannels)) {
                            AnimationChannel newCh = new AnimationChannel(this, ch);
                            AnimationChannels.Add(newCh);
                        }
                    }
                }
                SetupChannels(true);
            }
        }

        #endregion

        #region CHANNELS

        public override bool SupportsMultipleChannels()
        {
            return true;
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(Name + ".AnimationChannel.CopyChannel:" + src.Name);
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
            UndoUtil.Undo(ParentObject, "Copy Channel", true);
#endif
            TimeflowChannel copy = null;
            AnimationChannel ch = (AnimationChannel)src;
            if (ch != null) {
                AnimationChannel newCh = new AnimationChannel(this, ch);
                AnimationChannels.Add(newCh);

                copy = newCh;
                AddChannel(copy);
            }
            return copy;
        }

        public void AddAllAnimationChannels()
        {
            AnimationChannels = new List<AnimationChannel>();
            if (_animator != null) {
                foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips) {
                    AddAnimationChannel(clip);
                }
            }
            else
            if (_anim != null) {
                AddAnimationChannel(_anim.clip);
            }
        }

        public void AddAnimationChannel(AnimationClip clip)
        {
            if (!HasAnimationChannel(clip)) {
                AnimationChannel channel = new AnimationChannel(this, clip);
                AddChannel(channel);
            }
        }

        public bool HasAnimationChannel(AnimationClip clip)
        {
            bool has = false;
            if (AnimationChannels != null && AnimationChannels.Count > 0) {
                foreach (AnimationChannel anim in AnimationChannels) {
                    if (anim != null && anim.Clip == clip) {
                        has = true;
                        break;
                    }
                }
            }
            return has;
        }

        public override void AddChannel(TimeflowChannel channel)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Add Animation Clip", true);
#endif
            AnimationChannel ch = channel as AnimationChannel;
            if(AnimationChannels == null) AnimationChannels = new List<AnimationChannel>();
            if (!AnimationChannels.Contains(ch)) {
                AnimationChannels.Add(ch);
            }
            base.AddChannel(channel);
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            AnimationChannel ch = channel as AnimationChannel;
            if (AnimationChannels != null && AnimationChannels.Contains(ch)) {
                AnimationChannels.Remove(ch);
            }
            base.RemoveChannel(channel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Remove Animation Clip", true);
#endif
            AnimationChannel ch = channel as AnimationChannel;
            if (AnimationChannels != null && AnimationChannels.Contains(ch)) {
                AnimationChannels.Remove(ch);
            }
            base.RemoveChannelWithUndo(channel);
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            channel.Interpolate(channel.CurrentTime);
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AnimationClips;

        /// <summary>
        /// Prevents component reference from being listed in property lists, since there's nothing to
        /// animate here.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        #region TIMEFLOW GUI

        public override void GUIGraph(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, AxonUI.TrackOffStyle);
        }

        #endregion

        #region MENU

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            if (AnimationUtil.HasAnimation(TimeflowContext.Obj.gameObject)) {
                List<AnimationClip> clips = AnimationUtil.GetAnimationClips(TimeflowContext.Obj.gameObject);
                if (clips != null && clips.Count > 0) {
                    AnimationClips seq;
                    TimeflowContext.Obj.TryGetComponent<AnimationClips>(out seq);
                    foreach (AnimationClip clip in clips) {
                        bool hasClip = seq != null && seq.HasAnimationChannel(clip);
                        if (hasClip) {
                            TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Clip/" + clip.name), hasClip, null);

                        }
                        else {
                            TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Clip/" + clip.name), false, GUIMenu_AddAnimationClip, new TimeflowContextMenuAnimationClip(TimeflowContext.Obj, clip));
                        }
                    }
                    TimeflowContext.Menu.AddSeparator("Add Animation/Clip/");
                    TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Clip/Add All"), false, GUIMenu_AddAllAnimationClips, new TimeflowContextMenuAnimationClip(TimeflowContext.Obj, null));
                }
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Clip/No Clips Available"), false, null);
            }
        }

        public static void GUIMenu_AddAnimationClip(object obj)
        {
            TimeflowContextMenuAnimationClip item = (TimeflowContextMenuAnimationClip)obj;
            item.Obj.BehaviorsEnabled = true;
            if (!item.Obj.TryGetComponent<AnimationClips>(out var seq)) {
                seq = Undo.AddComponent<AnimationClips>(item.Obj.gameObject);
            }
            seq.AddAnimationChannel(item.Clip);
        }

        public static void GUIMenu_AddAllAnimationClips(object obj)
        {
            TimeflowContextMenuAnimationClip item = (TimeflowContextMenuAnimationClip)obj;
            item.Obj.BehaviorsEnabled = true;
            if (!item.Obj.TryGetComponent<AnimationClips>(out var seq)) {
                seq = Undo.AddComponent<AnimationClips>(item.Obj.gameObject);
            }
            seq.AddAllAnimationChannels();
        }

        #endregion

#endif
    }

}//AxonGenesis
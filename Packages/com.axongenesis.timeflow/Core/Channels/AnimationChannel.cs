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
    /// This implements Unity's animation system as a Timeflow channel allowing an animation clip to be
    /// animated directly.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AnimationChannel")]
    sealed public class AnimationChannel : TimeflowChannel
    {
        public AnimationClip Clip;
        public bool UseClipName = true;
        public int Index = -1;

        [NonSerialized]
        public GameObject _obj;

        [NonSerialized]
        private bool displayedError;

        public AnimationChannel(TimeflowBehavior parent, AnimationChannel ch) : base(parent)
        {
            Behavior = parent;
            Clip = ch.Clip;
            UseClipName = ch.UseClipName;
            Name = ch.Clip.name;
            _obj = parent.gameObject;
            Interpolation = ch.Interpolation;

            Reset(false);
            DuplicateKeysFromChannel(ch);
            OnSetup(parent);
        }

        public AnimationChannel(TimeflowBehavior parent, AnimationClip clip) : base(parent)
        {
            Behavior = parent;
            Clip = clip;
            Name = clip.name;
            _obj = parent.gameObject;

            Reset(false);

            OnSetup(parent);
        }

        public override void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            AnimationChannel ch = (AnimationChannel)src;
            if (ch != null) {
                Clip = ch.Clip;
                Name = ch.Clip.name;

                DuplicateKeysFromChannel(ch);
                OnSetup(Behavior);
            }
        }

        public override Property.PropertyTypes GetPropertyType()
        {
            _PropertyType = Property.PropertyTypes.Float;
            return _PropertyType;
        }

        public void Reset()
        {
            Reset(true);
        }

        public void Reset(bool undoable)
        {
#if UNITY_EDITOR
            if (undoable) UndoUtil.Undo(Behavior, "Reset Animation Clip", true);
#endif
            ClearKeys(undoable);

            float length = 1f;
            if (Clip != null) length = Clip.length;

            // Insert default keyframs at current time
            float time = CurrentTime;
            SetKeyValue(time, 0f);
            SetKeyValue(time + length, 1f);

            Interpolation = Interpolations.Linear;
            OnSetup(Behavior);
        }

        public override void OnSetup(TimeflowBehavior parent)
        {
            base.OnSetup(parent);
            _obj = parent.gameObject;
            ToProperty = null;
            HasProperty = false;
            _PropertyType = Property.PropertyTypes.Auto;
            PropertyType = Property.PropertyTypes.Float;
            LimitValue = true;
            ShowValue = true;
            MinValue = Vector4.zero;
            MaxValue = Vector4.one;
            SupportsKeyframes = true;
            CanAddRemoveKeys = true;

            LimitValue = true;
            MinValue = Vector4.zero;
            MaxValue = Vector4.one;
        }

        public override string Name {
            get {
                if (UseClipName || string.IsNullOrEmpty(_Name)) {
                    if (Clip != null) {
                        _Name = Clip.name + " [" + Index + "]";
                    }
                    else {
                        _Name = "No clip assigned";
                    }
                }
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public override string PathName => Name;

        public override bool IsLoopSupported {
            get {
                return true;
            }
            set {
                _IsLoopSupported = true;
            }
        }

        public override Keyframe SetKey(float time) { return SetKey(time, 0f, true); }

        public override Keyframe SetKey(float time, float endTime, bool isLocalTime)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            float val = InterpolateValue(time, false, isLocalTime);
            Keyframe key = base.SetKey(time, endTime, isLocalTime);
            key.KeyValue = val;
            return key;
        }

        public override void Interpolate(float time) { Interpolate(time, true, true); }

        public override void Interpolate(float time, bool apply, bool isLocalTime)
        {
            if (IsInterpolatingOptimized(time, isLocalTime, apply)) {
                float interp = InterpolateValue(time, false, isLocalTime);

                if (_obj == null) {
                    // The Obj may be null during wakeup, so just ignore it
                    if (!displayedError && Application.isPlaying) {
                        Debug.LogError("AnimationChannel.Interpolate: The game object must be assigned");
                        displayedError = true;
                    }
                }
                else
                if (Clip != null && interp > 0f && interp < 1) {
                    float clipTime = Clip.length * interp;
                    //if (DebugEnabled) Debug.Log(Clip.name + " t:" + time + " animationTime:" + interp + " clipTime:" + clipTime);
                    CurrentValue = interp;

                    Clip.SampleAnimation(_obj, clipTime);
                }
            }
        }

#if UNITY_EDITOR

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        #region GUI

        public override void GUIDoubleClick()
        {
            base.GUIDoubleClick();
            if (Clip == null) return;
            SelectionUtil.Select(Clip);
            EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow"));
        }

        public override void InspectorCustomProperty()
        {
            if (Clip != null) {
                if (!UseClipName) {
                    AxonGUI.UndoName = "Set Channel Name";
                    Name = AxonGUI.FieldTextInline(Behavior, Name);
                }
                AxonGUI.UndoName = "Set Use Clip Name";
                UseClipName = AxonGUI.FieldToggleInline(Behavior, "Use Clip Name", UseClipName);
            }
            if (Behavior != null) {
                List<AnimationClip> clips = AnimationUtil.GetAnimationClips(Behavior.gameObject);
                if (clips == null || clips.Count == 0) {
                    AxonGUI.HelpBox("There are no animation clips on this game object.", MessageType.Warning);
                }
                else {
                    string selectedClip = Clip == null ? "" : Clip.name;
                    string clipName = EditorUtil.AnimationClipsMenu(Behavior.gameObject, selectedClip);
                    if (clipName != selectedClip) {
                        foreach (AnimationClip c in clips) {
                            if (c.name.Equals(clipName)) {
                                Clip = c;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
            if (!IsSelected || selectedChannels.Count > 1 || Behavior == null) {
                return;
            }
            AxonGUI.BeginHorizontalBox();
            InspectorCustomProperty();
            AxonGUI.EndHorizontal(false);
        }

        public override void GUITracks()
        {
            if (!IsHidden) {
                base.GUITracks();
            }
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reset Animation Clip"), false, Reset);
        }

        #endregion

#endif
    }

}//AxonGenesis

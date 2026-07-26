// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [Serializable]
    public class SpineChannel : TimeflowChannel
    {
        public static float SkipTolerance = 1f;
        public static Color MixColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

        [NonSerialized] public int Index;
        [NonSerialized] public SpineAnimator Spine;
        [NonSerialized] private string _CurrentAnimation;
        [NonSerialized] private float LastTime = 0;
        [NonSerialized] public Keyframe LastKey = null;

        public SpineChannel(SpineAnimator animator) : base(animator) { }

        public string CurrentAnimation {
            get { return _CurrentAnimation; }
            set {
                _CurrentAnimation = value;
            }
        }

        public bool HasStateChanged { get; set; }

        public override void SetupKeyframes()
        {
            base.SetupKeyframes();
#if UNITY_EDITOR
            ValidateName();
#endif
            if (Keys != null) {
                CanAddRemoveKeys = true;
                SupportsKeyframes = true;

                // Use but hide the string value to be set using a popup menu
                ShowString = false;

                foreach (Keyframe key in Keys) {
                    SpineKey sk = Spine.SetupKey(key, true);
                    sk.OnValueChanged();
                }
            }
        }

        public override void ReinstantiateCustomKey(Keyframe key)
        {
            key.CustomKey = new SpineKey((SpineKey)key.CustomKey);
        }

        public bool HasTimeJumped(float localTime)
        {
            if (LastTime > localTime) {
                // Each time this channel loops it needs to reset the track
                //if (DebugEnabled) Debug.Log($"{Name} time jumped LastTime:{LastTime} localTime:{localTime}");
                Spine.ClearTrack(Index);
                LastKey = null;
                return true;
            }

            // Use time difference to determine if time skipped
            bool skipped = Mathf.Abs(LastTime - localTime) > SkipTolerance;
            if (skipped) LastKey = null;
            if (skipped && DebugEnabled) Debug.Log($"{Name} skipped LastTime:{LastTime} localTime:{localTime}");
            return skipped;
        }

        public bool CanUpdate(SpineKey key, float localTime)
        {
            bool canUpdate = HasStateChanged;
            if (canUpdate) {
                //if (DebugEnabled) Debug.Log($"{Name}.OnInterpolate: HasStateChanged");
            }
            else {
                canUpdate = HasTimeJumped(localTime);
                if (canUpdate) {
                    //if (DebugEnabled) Debug.Log($"{Name}.OnInterpolate: Time Jumped");
                }
                else
                if (LastKey != key.Key) {
                    //if (DebugEnabled) Debug.Log($"{Name}.LastKey:{(LastKey == null ? "NULL" : LastKey.KeyTime)} key:{key.Key.KeyTime}");
                    canUpdate = true;
                }
                else {
                    //if (DebugEnabled) Debug.Log($"{Name}.OnInterpolate: {(LastKey == null ? "NULL" : LastKey.KeyTime)}=={key.Key.KeyTime}");
                }
            }

            if (!canUpdate) {
                if (CurrentAnimation != key.AnimationName) {
                    //if (DebugEnabled) Debug.Log($"{Name}.OnInterpolate:{CurrentAnimation} != {key.AnimationName}");
                    canUpdate = true;
                }
            }
            //if (DebugEnabled) Debug.Log($"{Name}.CanUpdate:{canUpdate} localTime:{localTime}");

            if (canUpdate) {
                LastKey = key.Key;
            }
            LastTime = localTime;
            return canUpdate;
        }

        public override void Interpolate(float time, bool apply, bool isLocalTime)
        {
            //if (DebugEnabled) Debug.Log($"{Name}.Interpolate:{time} apply:{apply} Spine.HasAnimationState:{Spine.HasAnimationState}");
            if (!apply || !Spine.HasAnimationState) return;
            float localTime = LoopTime(LocalTime(time, isLocalTime));

            Keyframe keyA = null;
            if (IsLinked && Link.Mode != TimeflowChannelLink.Modes.Off && Link.Enabled) {
                // Use the keyframes from the linked channel
                float linkTime = localTime + Link.TimeOffsetWorld;// - TimeOffsetWorld;
                linkTime = Link.Channel.LocalTime(linkTime, false);
                keyA = Link.Channel.GetCurrentOrPrevKey(linkTime, true);

                if (Link.Mode != TimeflowChannelLink.Modes.Overwrite) {
                    // Select the most recently played keyframe on the current or linked channel
                    Keyframe keyB = GetCurrentOrPrevKey(localTime, true);
                    if (keyA == null) {
                        keyA = keyB;
                    }
                    else
                    if (keyB != null && keyB.KeyTimeWorld < keyA.KeyTime - linkTime) {
                        keyA = keyB;
                    }
                }

            }
            else {
                keyA = GetCurrentOrPrevKey(localTime, true);
            }

            if (keyA == null) return;
            SpineKey key = (SpineKey)keyA.CustomKey;
            if (key == null) key = SpineKey.Default;

            if (CanUpdate(key, localTime)) {
                //if (DebugEnabled) Debug.Log($"{Spine.name}.OnInterpolate:{Name}  IsLinked:{IsLinked}");

                HasStateChanged = false;

                CurrentAnimation = key.AnimationName;
                key.IsEmpty = string.IsNullOrEmpty(CurrentAnimation);

                Spine.IsFlipX = key.FlipX;
                Spine.IsFlipY = key.FlipY;

                if (key.AllTracks) {
                    if (key.IsEmpty) {
                        Spine.AnimationState.SetEmptyAnimations(key.MixDuration);
                    }
                    else {
                        foreach (TrackEntry track in Spine.AnimationState.Tracks) {
                            TrackEntry trackEntry = Spine.AnimationState.SetAnimation(track.TrackIndex, CurrentAnimation, key.Loop);
                            trackEntry.MixDuration = key.MixDuration;
                        }
                    }
                }
                else {
                    if (key.IsEmpty) {
                        Spine.AnimationState.SetEmptyAnimation(Index, key.MixDuration);
                    }
                    else {
                        //if (DebugEnabled) Debug.Log($"{Spine.name}.OnInterpolate:{Name} track:{Index} SetAnimation:{CurrentAnimation} time:{localTime}");
                        TrackEntry trackEntry = Spine.AnimationState.SetAnimation(Index, CurrentAnimation, key.Loop);
                        trackEntry.MixDuration = key.MixDuration;
                    }
                }
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            //if (DebugEnabled) Debug.Log($"{Name}.OnRewind");
            Spine.ClearTrack(Index);
            HasStateChanged = true;
        }

        public override void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            SpineChannel ch = (SpineChannel)src;
            if (ch != null) {
                Name = StringUtil.IncrementName(Name);

#if UNITY_EDITOR
                if (includeStyle) {
                    GUIColor = src.GUIColor;
                    GUIHeightOffset = src.GUIHeightOffset;
                }
#endif

                Keys = new List<Keyframe>();
                if (ch.Keys != null && ch.Keys.Count > 0) {
                    // Make a copy of the list to avoid errors in case of modification
                    List<Keyframe> copyKeys = new List<Keyframe>();
                    foreach (Keyframe key in ch.Keys) {
                        copyKeys.Add(key);
                    }

                    foreach (Keyframe key in copyKeys) {
                        CopyKey(key, 0, false, true);
                    }
                }
                OnSetup(Behavior);
            }
        }

        public override bool CustomSnapTime(float time, ref float threshold, out float snapped)
        {
            snapped = time;
            if (Keys == null || Keys.Count == 0) {
                return false;
            }
            bool wasSnapped = false;

            // Snap to the end time of each keyframe (the start time is automatically handled by Timeflow)
            foreach (Keyframe key in Keys) {
                if (key == null) continue;
                SpineKey k = key.CustomKey == null ? null : (SpineKey)key.CustomKey;
                if (k == null) continue;
                float dif = Mathf.Abs(time - (key.KeyTime + k.Duration));
                if (dif <= threshold) {
                    threshold = dif;// Set new threshold to beat
                    snapped = key.KeyTime + k.Duration;
                    wasSnapped = true;
                    // Keep checking in case of a closer match
                }
            }

            return wasSnapped;
        }

#if UNITY_EDITOR

        public override string Name {
            get => _Name;
            set {
                _Name = value;
                if (string.IsNullOrEmpty(_Name)) {
                    ValidateName();
                }
            }
        }

        public override void ResetName()
        {
            Name = null;
            ValidateName();
        }

        public void ValidateName()
        {
            if (string.IsNullOrEmpty(Name)) {
                Name = $"Spine Track {Index}";
            }
            if (ToProperty == null) ToProperty = new Property();
            ToProperty.Name = Name;
            IsNameCustom = true;
        }

        public override GUIStyle GUIKeyframeStyle(Keyframe key, bool selected)
        {
            GUIStyle style = selected ? AxonUI.KeyframeObjectSelectedStyle : AxonUI.KeyframeObjectStyle;
            return style;
        }

        public override void GUIKeyframesDraw(bool isLink, float timeOffset, Rect channelGUIRect)
        {
            float alpha = isLink ? 0.25f : 1f;
            Color c = GUIColor;
            c.a = 0.25f * alpha;

            Color mixColor = MixColor;
            mixColor.a *= alpha;

            SortKeys(false);

            int next = 0;
            for (int i = 0; i < Keys.Count; i++) {
                next = i + 1;
                Keyframe k = Keys[i];
                if (k.IsKeyEnabled) {
                    SpineKey b = (SpineKey)k.CustomKey;
                    if (b == null) continue;

                    float keyTime = k.KeyTimeWorld - timeOffset;
                    float endTime = keyTime + b.Duration;

                    Keyframe n = next < Keys.Count ? Keys[next] : null;
                    if (n != null) {
                        float nextkeyTime = n.KeyTimeWorld - timeOffset;
                        if (b.IsEmpty || b.Loop || endTime > nextkeyTime) endTime = nextkeyTime; // draw up to next key
                    }
                    else
                    if (b.IsEmpty || b.Loop || endTime > Timeflow.EndTime) endTime = Timeflow.EndTime;

                    c = MathUtil.Interpolate(c, b.TintColor, 0.75f);
                    c.a = b.TintColor.a * 0.75f * alpha;
                    GUI.color = c;
                    float x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    float x2 = Timeflow.Active.View.PositionOfTime(endTime, true);
                    Rect r = new Rect(x, channelGUIRect.y, x2 - x, channelGUIRect.height);
                    GUI.Box(r, GUIContent.none, AxonUI.TrackStyle);

                    endTime = keyTime + b.MixDuration;

                    GUI.color = mixColor;
                    x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                    x2 = Timeflow.Active.View.PositionOfTime(endTime, true);
                    r = new Rect(x, channelGUIRect.y, x2 - x, channelGUIRect.height);
                    GUI.Box(r, GUIContent.none, AxonUI.TrackMixLinearStyle);
                }
            }
            GUI.color = AxonColor.Default;
            base.GUIKeyframesDraw(isLink, timeOffset, channelGUIRect);
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Spine Track"), false, GUIMenu_AddTrack, null);
            menu.AddItem(new GUIContent("Renumber Spine Tracks"), false, GUIMenu_RenumberTracks, null);
        }

        public static void GUIMenu_RenumberTracks(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    SpineAnimator spineAnimator;
                    TimeflowContext.Obj.TryGetComponent<SpineAnimator>(out spineAnimator);
                    if (spineAnimator != null) {
                        spineAnimator.RenumberChannels();
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public static void GUIMenu_AddTrack(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    SpineAnimator spineAnimator;
                    TimeflowContext.Obj.TryGetComponent<SpineAnimator>(out spineAnimator);
                    if (spineAnimator == null) {
                        spineAnimator = Undo.AddComponent<SpineAnimator>(obj.gameObject);
                        if (spineAnimator != null) {
                            spineAnimator.SetupChannels(true);
                            Timeflow.Active.View.SelectChannel(spineAnimator.Channels[0]);
                        }
                    }
                    else {
                        spineAnimator.AddChannel();
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public override void GUIChannelValues()
        {
            float time = CurrentTime;

            float labelWidth = AxonGUI.LabelWidth;
            AxonGUI.SetLabelWidth(5);

            Rect rect = new Rect(GUIRect);
            rect.x = 8;
            rect.height = 16;

            rect = GUIChannelValuesLinkMenu(rect);

            string label = IsLinked ? Link.GetModeLabel() : "";

            float w = rect.width;
            rect.width = 10;
            GUI.Label(rect, label);

            rect.x += rect.width;
            rect.width = w - rect.width;

            EditorGUI.BeginChangeCheck();

            Keyframe key = GetKeyAtTime(time);
            SpineKey k = Spine.SetupKey(key);

            string value = CurrentAnimation;
            if (string.IsNullOrEmpty(value)) value = "Empty";
            string newValue = AxonGUI.FieldPopupString(Spine, rect, null, value, Spine.AnimationNames);
            if (value != newValue) {
                if (newValue == "Empty") newValue = null;
                CurrentAnimation = newValue;
                if (key != null) {
                    key.KeyString = newValue;
                }
            }
            rect.x += rect.width;

            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) {
                if (key == null) {
                    Keyframe newKey = SetKey(time);
                    if (newKey != null) newKey.KeyString = newValue;
                }
            }
        }

        public override void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
            base.GUIInfo(selectedChannels);
        }

        public override void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            base.GUIInfoValues(selectedKeys, tracksOnly);
            Spine.GUIInfoValues(selectedKeys, tracksOnly);
        }

#endif
    }
}

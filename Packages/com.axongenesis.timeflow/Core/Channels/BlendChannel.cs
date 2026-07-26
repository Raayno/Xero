// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

namespace AxonGenesis
{

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "BlendChannel")]
    sealed public class BlendChannel : TimeflowChannel
    {
        public Blend Blend;

        public BlendChannel(Blend parent) : base(parent)
        {
            Behavior = parent;
            Blend = parent;
            ClearKeys(false);
            IsCustomType = true;
            CanAddRemoveKeys = true;

            ToProperty = null;
            HasProperty = false;
            PropertyType = Property.PropertyTypes.Float;

#if UNITY_EDITOR
            GUIColor = new Color(Random.value, Random.value, Random.value, 1f);
            if (EditorGUIUtility.isProSkin) {
                GUIColor = new Color(Mathf.Min(1f, GUIColor.r * 1.5f), Mathf.Min(1f, GUIColor.g * 1.5f), Mathf.Min(1f, GUIColor.b * 1.5f));
            }
            else {
                GUIColor = new Color(Mathf.Min(0.9f, GUIColor.r), Mathf.Min(0.9f, GUIColor.g), Mathf.Min(0.9f, GUIColor.b));
            }
#endif
        }

        public override bool IsEnabled {
            get {
                if (Blend != null) {
                    return Blend.Enabled;
                }
                return _IsEnabled;
            }
            set {
                if (Blend != null) {
                    Blend.Enabled = value;
                }
            }
        }

        public override bool IsLoopSupported { get => true; set => base.IsLoopSupported = value; }

        public override Keyframe SetKey(float time) { return SetKey(time, 0f, true); }

        public override Keyframe SetKey(float time, float endTime, bool isLocalTime)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            Keyframe key = base.SetKey(time, endTime, isLocalTime);
            if (key != null) Blend.SetupKey(key);
            return key;
        }

        public override Keyframe CopyKey(Keyframe key, float timeOffset = 0f, bool doSetup = true, bool forceCopy = false)
        {
            Keyframe copy = base.CopyKey(key, timeOffset, doSetup, forceCopy);
            if (key.CustomKey != null) {
                BlendKey blendKey = (BlendKey)key.CustomKey;
                if (blendKey != null) {
                    BlendKey blendKeyCopy = BlendKey.CreateCopy(blendKey);
                    blendKeyCopy.Blend = Blend;
                    copy.CustomKey = blendKeyCopy;
                }
            }
            KeysAdd(copy);
            Blend.SetupKey(copy);
            return copy;
        }

        public override void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            BlendChannel ch = (BlendChannel)src;
            if (ch != null) {
                Name = ch.Name;

                ClearKeys(false);
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

        public override Type GetDataType()
        {
            return typeof(float);
        }

        public override void UpdateTangents()
        {
            base.UpdateTangents();
            Blend.Setup();
        }

        public override void SetupKeyframes()
        {
            ShowValue = ShowColor = ShowVector = false;
            PropertyType = Property.PropertyTypes.Float;
            CanAddRemoveKeys = true;

            base.SetupKeyframes();
            if (Keys != null && Keys.Count > 0) {
                for (int i = 0; i < Keys.Count; i++) {
                    Keyframe k = Keys[i];
                    k.LockValue = false;
                    k.KeyValue = (float)i;
                    k.LockValue = true;
                    Blend.SetupKey(k);
                }
            }
        }

        public override void PasteKeys(bool merge)
        {
            base.PasteKeys(merge);
            Blend.Setup();
        }

        public override void PrepareLoop()
        {
            base.PrepareLoop();
            Blend.Setup();
        }

        public override bool HasValueChanged(float time)
        {
            bool changed = false;
            return changed;
        }

        public override bool UnsetKey(float localTime)
        {
            Keyframe key = GetKeyAtTime(localTime);
            return UnsetKey(key);
        }

        public override bool UnsetKey(Keyframe key)
        {
            //if (DebugEnabled) Debug.Log(Name + ".UnsetKey:" + key.KeyTime);
            bool isUnset = false;
            if (Keys != null && Keys.Contains(key)) {
#if UNITY_EDITOR
                UndoUtil.Undo(Behavior, "Unset Key", true);
#endif
                isUnset = KeysRemove(key);
                TangentsNeedUpdate = true;
                PrepareLoop();
            }
            return base.UnsetKey(key);
        }

        public override float InterpolateValue(float time, bool apply, bool isLocalTime)
        {
            if (Blend.ManualOverride) {
                /// Don't calculate keyframes when manual override is engaged
                return Blend.BlendAmount;
            }
            float value = base.InterpolateValue(time, false, isLocalTime);
            if (!isLocalTime) time -= TimeOffsetWorld;
            time = LoopTime(time);

            if (IsInterpolatingOptimized(time, isLocalTime, apply) && Behavior != null && Keys != null) {
                Keyframe keyA = null;
                Keyframe keyB = null;

                float prevTime = float.MaxValue;
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                        keyA = k;
                    }
                }
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                        keyB = k;
                        prevTime = keyB.KeyTime;
                    }
                }

                float v = value;
                if (keyA != null) v -= keyA.KeyValue;
                if (value < 0f) v = 0f;
                else
                if (v > 1f) v = 1f;
                v = Blend.Interpolate(keyA, keyB, time, v, apply);
                value = v;
                if (keyA != null) value += keyA.KeyValue;

            }

            return value;
        }

        public override void ReinstantiateCustomKey(Keyframe key)
        {
            if (key.CustomKey is BlendKey bkey) {
                key.CustomKey = BlendKey.CreateCopy(bkey);
            }
            else {
                Debug.LogWarning("Failed to duplicate BlendKey");
                key.CustomKey = new BlendKey();
            }
        }

#if UNITY_EDITOR

        public override void ResetName()
        {
            if (ToProperty == null) return;
            Name = ToProperty.DisplayName = "Blend";
        }

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        #region GUI

        public override GUIStyle GUIKeyframeStyle(Keyframe key, bool selected)
        {
            GUIStyle style = selected ? AxonUI.KeyframeObjectSelectedStyle : AxonUI.KeyframeObjectStyle;
            return style;
        }
        
        public override void GUIKeyframesDraw(bool isLink, float timeOffset, Rect channelGUIRect)
        {
            if (IsEnabled && !IsHidden && Timeflow != null && Keys != null && Blend != null) {
                Color c = GUIColor;
                c.a = 0.25f;

                int next = 0;
                for (int i = 0; i < Keys.Count; i++) {
                    next = i + 1;
                    Keyframe k = Keys[i];
                    if (k.IsKeyEnabled) {
                        Keyframe n = next < Keys.Count ? Keys[next] : null;

                        float keyTime = k.KeyTimeWorld;
                        float dur = 1f;

                        string keyValueLabel = null;
                        if (k.CustomKey != null) {
                            BlendKey b = (BlendKey)k.CustomKey;
                            if (b != null) {
                                keyValueLabel = b.Name;
                                if (b.AutoDuration) {
                                    if (n != null) {
                                        b.Duration = (n.KeyTime - k.KeyTime) - b.StartTime;
                                    }
                                    else {
                                        b.Duration = (Behavior.Timeflow.EndTime - k.KeyTime) - b.StartTime;
                                    }
                                }
                                dur = b.StartTime + b.Duration;
                            }
                        }

                        float endTime = keyTime + dur;
                        if (n != null && endTime > n.KeyTimeWorld) endTime = n.KeyTimeWorld; // draw up to next key

                        float x = Timeflow.Active.View.PositionOfTime(keyTime, true);
                        float x2 = Timeflow.Active.View.PositionOfTime(endTime, true);
                        float x3 = x2 - x;

                        Rect keyRect = new Rect(x, channelGUIRect.y, x3, GUIHeight);
                        Rect label = new Rect(x + 4, channelGUIRect.y, x3, GUIHeight);
                        GUIDrawKeyframeLabel(k, keyRect, label, keyValueLabel, false, isLink);

                        GUI.color = c;
                        GUI.Box(keyRect, GUIContent.none, AxonUI.TrackStyle);
                    }
                }

                GUI.color = AxonColor.Default;
                base.GUIKeyframesDraw(isLink, timeOffset, channelGUIRect);
            }
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {
        }

        public override void GUIChannelValues()
        {
            GUIChannelSelected();
            Blend.GUIChannelValues();
        }

        public override void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
        }

        public override void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            base.GUIInfoValues(selectedKeys, tracksOnly);
            Blend.GUIInfoValues(selectedKeys);
        }

        public override void GUIChannelLink()
        {
            // Override to disable channel link on this item.
        }

        #endregion

        #region INSPECTOR

        public override void InspectorChannelLinkGUI()
        {
            // Override to disable channel link on this item
        }

        public override void InspectorKeyframesGUI()
        {
            AxonGUI.BeginBox();
            EditorShowKeys = AxonGUI.Foldout(EditorShowKeys, "Keyframes");
            if (EditorShowKeys) {
                if (Keys.Count == 0) {
                    AxonGUI.HelpBox("No keyframes have been added to this channel yet.", MessageType.Info);
                }
                else {
                    AxonGUI.SetLabelWidth(80);

                    AxonGUI.BeginVertical("box");
                    AxonGUI.Indent++;
                    AxonGUI.Space();

                    string[] names = null;
                    if (Blend != null) {
                        names = Blend.SetNames.ToArray();
                    }


                    int i = 0;
                    int remove = -1;
                    int insert = -1;
                    int displayLimit = 20; // TODO: Implement paging controls for long keyframe lists
                    foreach (Keyframe key in Keys) {
                        key.Channel = this;

                        AxonGUI.BeginHorizontalBox();
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Key")) {
                            insert = i;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Key")) {
                            remove = i;
                        }

                        AxonGUI.UndoName = "Set Keyframe Enabled";
                        key.IsKeyEnabled = AxonGUI.FieldToggleInline(Behavior, key.IsKeyEnabled, GUILayout.Width(20));

                        AxonGUI.UndoName = "Set Keyframe Time";
                        key.KeyTime = AxonGUI.FieldFloatInline(Behavior, "Time", key.KeyTime, GUILayout.Width(120));

                        BlendKey k = key.CustomKey == null ? null : (BlendKey)key.CustomKey;
                        if (k != null && names != null) {
                            EditorGUI.BeginDisabledGroup(k.AutoDuration);
                            AxonGUI.UndoName = "Set Blend Key Duration";
                            k.Duration = AxonGUI.FieldFloatInline(Behavior, "Duration", k.Duration);
                            EditorGUI.EndDisabledGroup();
                            AxonGUI.UndoName = "Set KeBlend Keyyframe Auto Duration";
                            k.AutoDuration = AxonGUI.FieldToggleInline(Behavior, "Auto", k.AutoDuration);

                            AxonGUI.UndoName = "Set Blend Key From";
                            int fromIndex = AxonGUI.FieldPopupInline(Behavior, "From", Blend.GetIndex(k.FromSet), names);
                            k.FromSet = Blend.GetID(fromIndex);

                            AxonGUI.UndoName = "Set Blend Key To";
                            int toIndex = AxonGUI.FieldPopupInline(Behavior, "To", Blend.GetIndex(k.ToSet), names);
                            k.ToSet = Blend.GetID(toIndex);

                            AxonGUI.EndHorizontal();

                            AxonGUI.BeginHorizontal();
                            k.InterpolationMode = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(Behavior, k.InterpolationMode);
                            AxonGUI.UndoName = "Set Blend Key Start Time";
                            k.StartTime = AxonGUI.FieldFloatInline(Behavior, "Start", k.StartTime);

                            bool isExposed = key.ExposedID != 0;
                            AxonGUI.UndoName = "Set Keyframe Exposed";
                            AxonGUI.SetTooltip("Expose a keyframe to set its value remotely via scripting, referring to it by name using Keyframer.SetExposedKeyframe(). Be sure to use a unique name.");
                            bool ex = AxonGUI.FieldToggleInline(Behavior, "Expose", isExposed);
                            if (ex != isExposed) {
                                if (ex) {
                                    key.ExposedID = (int)(Random.value * 99999f);
                                    Keyframe.RegisterExposedKeyframe(key);
                                }
                                else {
                                    key.ExposedID = 0;
                                    Keyframe.UnregisterExposedKeyframe(key);
                                }
                            }
                            if (ex) {
                                if (key.ExposedID == 0) key.ExposedID = (int)(Random.value * 99999f);
                                AxonGUI.UndoName = "Set Keyframe Exposed ID";
                                key.ExposedID = AxonGUI.FieldIntInline(Blend, key.ExposedID);
                            }
                            AxonGUI.EndHorizontal();
                            AxonGUI.BeginHorizontal();//intentional
                        }

                        AxonGUI.EndHorizontal();
                        AxonGUI.Space();

                        i++;
                        if (i >= displayLimit) break;
                    }
                    if (remove > -1) {
                        UndoUtil.Undo(Blend, "Remove Key");
                        Keys.RemoveAt(remove);
                    }
                    if (insert > -1) {
                        UndoUtil.Undo(Blend, "Add Key");
                        Keyframe newKey = new Keyframe(Keys[insert]);
                        Keys.Insert(insert, newKey);
                    }

                    AxonGUI.Indent--;
                    AxonGUI.EndVertical();
                    AxonGUI.SetLabelWidth(140);
                }
            }
            AxonGUI.EndBox();
        }

        #endregion

#endif
    }

}//AxonGenesis
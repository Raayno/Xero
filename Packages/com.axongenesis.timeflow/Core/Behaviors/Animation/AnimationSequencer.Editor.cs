// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public partial class AnimationSequencer : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        public bool EditorShowAnimationClips = true;
        public bool EditorShowClipDetails = true;

        public override Texture2D Icon => AxonUI.Icons.AnimationSequencer;

        private void OnValidate()
        {
            RebuildClipCache();
        }

        public void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            if (tracksOnly) return;
            AxonGUI.BeginBox();

            int count = selectedKeys.Count;

            bool hasClip = false;
            float clipDuration = 0f;
            string animVal = null;
            bool loopVal = false;
            int loopLimitVal = 0;
            float durVal = 0f;
            float outTimeVal = 0f;
            float speedVal = 1f;
            float startVal = 0f;
            float endVal = 0f;
            float inBlendVal = 0f;
            float outBlendVal = 0f;
            bool showClipRanges = false;
            bool footIK = false;
            bool playableIK = false;
            AnimationCurve inBlendCurveVal = null;
            AnimationCurve outBlendCurveVal = null;
            Color tintVal = Color.white;

            bool first = true;
            bool isAnimSame = true;
            bool isLoopSame = true;
            bool isLoopLimitSame = true;
            bool isDurationSame = true;
            bool isOutTimeSame = true;
            bool isSpeedSame = true;
            bool isStartSame = true;
            bool isEndSame = true;
            bool isInBlendSame = true;
            bool isOutBlendSame = true;
            bool isInBlendCurveSame = true;
            bool isOutBlendCurveSame = true;
            bool isFootIKSame = true;
            bool isPlayableIKSame = true;
            bool anyEmpty = false;

            foreach (Keyframe key in selectedKeys) {
                var k = key?.CustomKey as AnimationSequencerKey;
                if (k == null) continue;

                hasClip |= (k.Clip != null);

                if (first) {
                    first = false;
                    animVal = key.KeyString;
                    loopVal = k.Loop;
                    loopLimitVal = k.LoopLimit;
                    durVal = k.Duration;
                    outTimeVal = k.OutTime;
                    speedVal = k.Speed;
                    startVal = k.StartTime;
                    endVal = k.EndTime;
                    inBlendVal = k.InBlend;
                    outBlendVal = k.OutBlend;
                    inBlendCurveVal = k.InBlendCurve;
                    outBlendCurveVal = k.OutBlendCurve;
                    clipDuration = k.ClipDuration;
                    showClipRanges = k.ShowClipRanges || k.ForceShowClipRanges;
                    footIK = k.ApplyFootIK;
                    playableIK = k.ApplyPlayableIK;
                }
                else {
                    if (isAnimSame && animVal != key.KeyString) isAnimSame = false;
                    if (isLoopSame && loopVal != k.Loop) isLoopSame = false;
                    if (isLoopLimitSame && loopLimitVal != k.LoopLimit) isLoopLimitSame = false;
                    if (isDurationSame && durVal != k.Duration) isDurationSame = false;
                    if (isOutTimeSame && outTimeVal != k.OutTime) isOutTimeSame = false;
                    if (isSpeedSame && !Mathf.Approximately(speedVal, k.Speed)) isSpeedSame = false;
                    if (isStartSame && !Mathf.Approximately(startVal, k.StartTime)) isStartSame = false;
                    if (isEndSame && !Mathf.Approximately(endVal, k.EndTime)) isEndSame = false;
                    if (isInBlendSame && !Mathf.Approximately(inBlendVal, k.InBlend)) isInBlendSame = false;
                    if (isOutBlendSame && !Mathf.Approximately(outBlendVal, k.OutBlend)) isOutBlendSame = false;
                    if (isInBlendCurveSame && inBlendCurveVal != k.InBlendCurve) isInBlendCurveSame = false;
                    if (isOutBlendCurveSame && outBlendCurveVal != k.OutBlendCurve) isOutBlendCurveSame = false;
                    if (isFootIKSame && footIK != k.ApplyFootIK) isFootIKSame = false;
                    if (isPlayableIKSame && playableIK != k.ApplyPlayableIK) isPlayableIKSame = false;
                }

                if (k.IsEmpty) anyEmpty = true;
            }

            AxonGUI.BeginChangeCheck();
            AxonGUI.BeginHorizontal();

            EditorShowClipDetails = AxonGUI.FoldoutInline(EditorShowClipDetails);

            string inAnim = string.IsNullOrEmpty(animVal) ? "Empty" : animVal;
            AxonGUI.UndoName = "Set Animation";
            AxonGUI.SetTooltip("Specifies the Unity AnimationClip to play.");
            string outAnim = AxonGUI.FieldPopupStringInline(this, inAnim, AnimationNames, GUILayout.Width(150));
            if (inAnim != outAnim) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    var sk = key.CustomKey as AnimationSequencerKey ?? SetupKey(key);
                    if (outAnim == "Empty" || string.IsNullOrEmpty(outAnim)) {
                        key.KeyString = null;
                        if (sk != null) {
                            sk.Clip = null;         // clear clip
                            sk.OnValueChanged();    // recompute duration for Empty, etc.
                        }
                    }
                    else {
                        key.KeyString = outAnim;
                        if (sk != null) {
                            sk.Clip = GetClipByName(outAnim); // assign clip to match popup
                            sk.OnValueChanged();
                        }
                    }
                }
            }

            foreach (Keyframe key in selectedKeys) {
                if (key == null) continue;
                if (key.CustomKey is AnimationSequencerKey k) {
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Select, "Select the animation clip asset", new RectOffset(0, 0, 3, 0), new Vector2(16, 16))) {
                        Selection.activeObject = k.Clip;
                        EditorGUIUtility.PingObject(k.Clip);
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.EditOff, "Open the animation clip asset for editing", new RectOffset(0, 0, 3, 0), new Vector2(16, 16))) {
                        AnimationUtil.OpenAnimationClipInEditor(k.Clip);
                    }
                    break;
                }
            }
            if (!anyEmpty || count > 1) {
                bool inLoop = loopVal;
                AxonGUI.UndoName = "Set Loop";
                AxonGUI.SetTooltip("Sets whether the animation loops.");
                bool outLoop = AxonGUI.FieldToggleInline(this, "Loop", inLoop);
                if (inLoop != outLoop) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.Loop = outLoop;
                        }
                    }
                }
                AxonGUI.UndoName = "Set Loop Limit";
                AxonGUI.SetTooltip("Sets the number of times the animation may loop. Set to 0 for infinite looping");
                int outLoopLimit = AxonGUI.FieldIntInline(this, "Limit", loopLimitVal);
                if (outLoopLimit != loopLimitVal) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.LoopLimit = outLoopLimit;
                            sk.OnValueChanged();
                        }
                    }
                }
            }

            AxonGUI.UndoName = "Set Speed";
            AxonGUI.SetTooltip("Playback speed multiplier for the clip (1 = normal speed).");
            float outSpeed = AxonGUI.FieldFloatInline(this, "Speed", speedVal);
            if (!Mathf.Approximately(outSpeed, speedVal)) {
                foreach (Keyframe key in selectedKeys) {
                    if (key == null) continue;
                    if (key.CustomKey is AnimationSequencerKey sk) {
                        sk.Speed = outSpeed;
                        sk.OnValueChanged();
                    }
                }
            }
            AxonGUI.EndHorizontal(false);

            if (EditorShowClipDetails) {
                if (hasClip) {
                    AxonGUI.BeginHorizontal();
                    float outStart = startVal;
                    float outEnd = endVal;

                    if (AxonGUI.ButtonTexture(showClipRanges ? AxonUI.Icons.AlignToolsOn : AxonUI.Icons.AlignToolsOff, "Show clip ranges in track view", new RectOffset(0, 0, 2, 0), new Vector2(16, 16))) {
                        first = true;
                        bool show = false;
                        foreach (Keyframe key in selectedKeys) {
                            if (key == null) continue;
                            if (key.CustomKey is AnimationSequencerKey sk) {
                                if (first) {
                                    show = !sk.ShowClipRanges;
                                    first = false;
                                }
                                sk.ShowClipRanges = show;
                            }
                        }
                    }

                    AxonGUI.UndoName = "Set Clip Range";
                    AxonGUI.FieldSliderMinMaxInline(this, "Clip Range", ref outStart, ref outEnd, 0f, clipDuration);
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Clip Start Time";
                    AxonGUI.SetTooltip("Local start and end time (seconds) within the AnimationClip.");
                    outStart = AxonGUI.FieldTimeInline(this, "Start", outStart);

                    AxonGUI.UndoName = "Set Clip End Time";
                    AxonGUI.SetTooltip("Local end time (seconds) within the AnimationClip.");
                    outEnd = AxonGUI.FieldTimeInline(this, "End", outEnd);
                    AxonGUI.EndHorizontal(false);

                    if (!Mathf.Approximately(outStart, startVal) || !Mathf.Approximately(outEnd, endVal)) {
                        foreach (Keyframe key in selectedKeys) {
                            if (key == null) continue;
                            if (key.CustomKey is AnimationSequencerKey sk) {
                                if (outStart < 0f) outStart = 0f;
                                sk.StartTime = outStart;
                                sk.EndTime = Mathf.Max(0f, outEnd);
                                sk.OnValueChanged();
                            }
                        }
                    }
                }

                // Third row: In/Out Blend
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Blend In";
                AxonGUI.SetTooltip("Blend-in time (seconds) for this clip.");
                float outInBlend = AxonGUI.FieldTimeInline(this, "Blend In", inBlendVal);
                if (!Mathf.Approximately(outInBlend, inBlendVal)) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.InBlend = outInBlend;
                            sk.OnValueChanged();
                        }
                    }
                }

                // Curve field for InBlend shape
                AxonGUI.UndoName = "Set Blend In Curve";
                AnimationCurve outInCurve = inBlendCurveVal ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                outInCurve = EditorGUILayout.CurveField(outInCurve);
                if (outInCurve != inBlendCurveVal) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.InBlendCurve = new AnimationCurve(outInCurve.keys);
                            sk.OnValueChanged();
                        }
                    }
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset Blend In Curve", new RectOffset(2, 0, 3, 0), new Vector2(16, 16))) {
                    Undo.RegisterCompleteObjectUndo(this, "Reset Blend In Curve");
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.InBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                            sk.OnValueChanged();
                        }
                    }
                }

                AxonGUI.UndoName = "Set Blend Out";
                AxonGUI.SetTooltip("Blend-out time (seconds) for this clip.");
                float outOutBlend = AxonGUI.FieldTimeInline(this, "Blend Out", outBlendVal);
                if (!Mathf.Approximately(outOutBlend, outBlendVal)) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.OutBlend = outOutBlend;
                            sk.OnValueChanged();
                        }
                    }
                }

                // Curve field for OutBlend shape
                AxonGUI.UndoName = "Set Blend Out Curve";
                AnimationCurve outOutCurve = outBlendCurveVal ?? AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                outOutCurve = EditorGUILayout.CurveField(outOutCurve);
                if (outOutCurve != outBlendCurveVal) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.OutBlendCurve = new AnimationCurve(outOutCurve.keys);
                            sk.OnValueChanged();
                        }
                    }
                }

                if (AxonGUI.ButtonTexture(AxonUI.Icons.Reset, "Reset Blend Out Curve", new RectOffset(2, 0, 3, 0), new Vector2(16, 16))) {
                    Undo.RegisterCompleteObjectUndo(this, "Reset Blend Out Curve");
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.OutBlendCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            sk.OnValueChanged();
                        }
                    }
                }

                AxonGUI.EndHorizontal(false);

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Displays the effective length of the trimmed animation clip (derived).");
                float outDurVal = AxonGUI.FieldTimeInline(this, "Duration", durVal);
                if (!Mathf.Approximately(outDurVal, durVal)) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.Duration = outDurVal;
                            sk.OnValueChanged();
                        }
                    }
                }

                AxonGUI.SetTooltip("Displays the effective length of the trimmed animation clip (derived).");
                float outTimeVal2 = AxonGUI.FieldTimeInline(this, "Out Time", outTimeVal);
                if (!Mathf.Approximately(outTimeVal2, outTimeVal)) {
                    foreach (Keyframe key in selectedKeys) {
                        if (key == null) continue;
                        if (key.CustomKey is AnimationSequencerKey sk) {
                            sk.OutTime = outTimeVal2;
                            sk.OnValueChanged();
                        }
                    }
                }

                AxonGUI.EndHorizontal(false);

                if (RiggingMode == RiggingModes.MechanimAvatar) {
                    AxonGUI.BeginDisabledGroup(!ApplyFootIK);
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Enables foot IK for this clip. This requires that Foot IK is enabled for the Animation Sequencer.");
                    bool outFootIK = AxonGUI.FieldToggleInline(this, "Foot IK", footIK && ApplyFootIK);
                    if (ApplyFootIK && outFootIK != footIK) {
                        foreach (Keyframe key in selectedKeys) {
                            if (key == null) continue;
                            if (key.CustomKey is AnimationSequencerKey sk) {
                                sk.ApplyFootIK = outFootIK;
                            }
                        }
                    }
                    AxonGUI.EndDisabledGroup();

                    AxonGUI.BeginDisabledGroup(!ApplyPlayableIK);
                    AxonGUI.SetTooltip("Enables playable IK for this clip. This requires that Playable IK is enabled for the Animation Sequencer.");
                    bool outPlayableIK = AxonGUI.FieldToggleInline(this, "Playable IK", playableIK && ApplyPlayableIK);
                    if (ApplyPlayableIK && outPlayableIK != playableIK) {
                        foreach (Keyframe key in selectedKeys) {
                            if (key == null) continue;
                            if (key.CustomKey is AnimationSequencerKey sk) {
                                sk.ApplyPlayableIK = outPlayableIK;
                            }
                        }
                    }
                    AxonGUI.EndHorizontal(false);
                    AxonGUI.EndDisabledGroup();
                }
            }
            AxonGUI.Space();
            AxonGUI.EndBox();

            if (AxonGUI.EndChangeCheck())
                Refresh();
        }

        public static void AddMenuItem()
        {
            if (TimeflowContext.Obj == null) return;
            if (TimeflowContext.DisplayMode != TimeflowContext.DisplayModes.Object) return;

            if (!TimeflowContext.Obj.TryGetComponent<AnimationSequencer>(out _)) {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Animation Sequencer"), false, GUIMenu_Add, null);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Animation Sequencer/Add Track"), false, AnimationSequencerChannel.GUIMenu_AddTrack, null);
                TimeflowContext.Menu.AddSeparator("");
            }
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects == null) return;

            foreach (TimeflowObject obj in objects) {
                obj.BehaviorsEnabled = true;

                AnimationSequencer sequencer = ObjectUtil.GetOrAddComponent<AnimationSequencer>(obj.gameObject);
                if (sequencer != null) {
                    sequencer.SetupChannels(true);
                    Timeflow.Active.View.SelectChannel(sequencer.Channels[0]);
                }
            }
            Timeflow.Active.Refresh(true);
        }
    }
}
#endif
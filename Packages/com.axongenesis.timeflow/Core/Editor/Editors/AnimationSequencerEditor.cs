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
    [CustomEditor(typeof(AnimationSequencer))]
    public class AnimationSequencerEditor : AxonGenesisEditor<AnimationSequencer, AnimationSequencerEdit> { }
    sealed public class AnimationSequencerEdit : AxonGenesisBehaviorEdit<AnimationSequencer>
    {
#if TIMEFLOW_PRO
        public const string kAddAnimationSequencer = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎠 Animation Sequencer";
#else
        public const string kAddAnimationSequencer = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Animation Sequencer";
#endif

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAnimationSequencer, false, 106)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAnimationSequencer, false, 106)]
        public static void AddAnimationSequencer()
        {
            AnimationSequencer a = ObjectUtil.GetOrAddComponent<AnimationSequencer>(TimeflowMenu.GetSelectedOrNewGameObject("Animation Sequencer"));
            a.UpdateMethod = TimeflowBehavior.UpdateMethods.LateUpdate;
        }

        private bool _DismissLateUpdateWarning = false;

        public TimeflowBehaviorSharedEdit behaviorUI;

        public AnimationSequencerEdit() { }

        public AnimationSequencerEdit(AnimationSequencer _target)
        {
            target = _target;
        }

        public override void Refresh()
        {
            target.ForceRefresh();
            EditorGUIUtility.ExitGUI();
        }

        public override void GUISetup()
        {
            base.GUISetup();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/clip-sequencer";
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            base.GUIMenu();
            if (target.UpdateMethod != TimeflowBehavior.UpdateMethods.LateUpdate && !_DismissLateUpdateWarning) {
                AxonGUI.FlexibleSpace();
                AxonGUI.Warning("Animation Sequencer works best when using Late Update as the update method to ensure animations are applied after all other updates. " +
                    "You can dismiss this warning by clicking the 'x' button.");
                if (AxonGUI.ButtonInline("Fix", GUI.skin.button)) {
                    target.UpdateMethod = TimeflowBehavior.UpdateMethods.LateUpdate;
                }
                if (AxonGUI.ButtonInline("Dismiss", GUI.skin.button)) {
                    _DismissLateUpdateWarning = true;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup(100);
            MainGUI();
            AnimationClipsGUI();
            TracksGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                if (!Application.isPlaying) target.UpdateTime();
            }
        }

        private void MainGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginChangeCheck();
            AxonGUI.BeginHorizontal();
            target.DefaultClip = (AnimationClip)AxonGUI.FieldObject(target, "Default Clip", target.DefaultClip, typeof(AnimationClip), false);
            if (target.HasAnimatorController) {
                AxonGUI.Info("The Default Clip is only applied as the base layer of animation when there is no Animator Controller assigned to the Animator. " +
                    "The Default Clip is also used to reset the character pose when using Rebind Animator");
            }
            if (AxonGUI.ButtonInline("Rebind Animator")) {
                target.Rebind();
            }
            AxonGUI.EndHorizontal();

            if (AxonGUI.EndChangeCheck()) {
                target.ForceRefresh();
            }

            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Sets the scale of time on this object affecting the speed of animation playback.");
            if (target.BlendWeight == 0) GUI.color = AxonColor.Warning;
            target.BlendWeight = AxonGUI.FieldSlider(target, "Blend Weight", target.BlendWeight, 0f, 1f);
            GUI.color = AxonColor.Default;
            if (target.HasAnimatorController) {
                AxonGUI.Info("Blending with the base Animator Controller. Set to 0 for no influence to give the Animator full control.");
            }
            else
            if (target.HasDefaultClip) {
                AxonGUI.Info("Blending with the default animation clip. Set to 0 for only the default animation clip to play.");
            }

            AxonGUI.BeginChangeCheck();

            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Determines which type of rigging is used. Animation Rigging requires the Animation Rigging package.");
            target.RiggingMode = (AnimationSequencer.RiggingModes)AxonGUI.FieldEnumPopup(target, "IK Constraints", target.RiggingMode, GUILayout.Width(230));

#if !ANIMATIONRIGGING_1_OR_NEWER
            if (target.IsAnimationRigging) {
                AxonGUI.Warning("Animation Rigging mode requires the Animation Rigging package to be installed.");
            }
#endif
            if (target.RiggingMode == AnimationSequencer.RiggingModes.MechanimAvatar) {
                AxonGUI.SetTooltip("Enables playable IK on animation clips. This requires the Mechanim Avatar IK system.");
                target.ApplyPlayableIK = AxonGUI.FieldToggleInline(target, "Playable IK", target.ApplyPlayableIK);

                AxonGUI.SetTooltip("Enables applying foot IK. This requires the Mechanim Avatar IK system.");
                target.ApplyFootIK = AxonGUI.FieldToggleInline(target, "Foot IK", target.ApplyFootIK);
            }

            if (target.RiggingMode == AnimationSequencer.RiggingModes.MechanimAvatar) {
                AxonGUI.EndHorizontal();
                AxonGUI.BeginHorizontal();
            }
            AxonGUI.FlexibleSpace();
            AxonGUI.SetTooltip("Enable applying root motion from the sequencer's animation output. ");
            target.ApplyRootMotion = AxonGUI.FieldToggleInline(target, "Apply Root Motion", target.ApplyRootMotion);
            if (target.ApplyRootMotion) {
                AxonGUI.SetTooltip("If enabled, root motion will only be applied when exactly one sequencer track is contributing (its mixer input weight > 0). This helps avoid unwanted positional drift when blending multiple tracks.");
                target.RootMotionSingleTrackOnly = AxonGUI.FieldToggleInline(target, "Single Track", target.RootMotionSingleTrackOnly);
            }

            AxonGUI.EndHorizontal();

            if (AxonGUI.EndChangeCheck()) {
                target.ForceRefresh();
            }

            AxonGUI.EndBox();
        }

        public void TracksGUI()
        {
            if (!target.Enabled) return;
            AxonGUI.BeginBox();
            AxonGUI.SetTooltip("Lists the Animation Sequencer Channels of this object.");
            target.EditorShowChannels = AxonGUI.Foldout(target.EditorShowChannels, "Tracks");
            if (target.EditorShowChannels) {
                AxonGUI.BeginBoxPadded();
                if (target.SequencerChannels == null || target.SequencerChannels.Count == 0) {
                    AxonGUI.Label("None", "");
                }
                else {
                    bool anyShown = false;
                    int moveUp = -1;
                    int moveDown = -1;
                    int x = 0;
                    List<AnimationSequencerChannel> toRemove = new List<AnimationSequencerChannel>();

                    foreach (AnimationSequencerChannel channel in target.SequencerChannels) {
                        if (channel == null) {
                            AxonGUI.Warning("Null channel reference! Press the Refresh button to clear. Please contact support if this issue persists.");
                        }
                        else {
                            if (channel.IsSelected && TimeflowPreferences.Current.ShowTrackColorsInInspector) {
                                Color c = channel.GUIColor;
                                c.a = 0.5f;
                                GUI.color = c;
                                AxonGUI.BeginVertical(AxonUI.HeaderStyleSelected);
                            }
                            else {
                                AxonGUI.BeginVertical(AxonUI.HeaderStyle);
                            }
                            AxonGUI.BeginHorizontal();
                            GUI.color = AxonColor.Default;

                            if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                                moveUp = x;
                            }
                            if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                                moveDown = x;
                            }
                            if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Channel")) {
                                toRemove.Add(channel);
                            }

                            channel.InspectorGUI(null);
                            anyShown = true;

                            AxonGUI.EndHorizontal();

                            AxonGUI.BeginHorizontal();

                            if (channel.Index < 10) {
                                float w = target.GetMappedChannelWeight(channel.Index, channel.Weight);
                                float weight = AxonGUI.FieldSliderInline(target, "Weight", w, 0f, 1f);
                                if (weight != w) {
                                    target.SetMappedChannelWeight(channel.Index, weight);
                                }
                            }
                            else {
                                channel.Weight = AxonGUI.FieldSliderInline(target, "Weight", channel.Weight, 0f, 1f);
                            }
                            AxonGUI.BeginChangeCheck();
                            AxonGUI.SetTooltip("If enabled, this track is blended additively on top of lower layers.");
                            channel.IsAdditive = AxonGUI.FieldToggleInline(target, "Additive", channel.IsAdditive);

                            AxonGUI.EndHorizontal();

                            AxonGUI.BeginHorizontal();

                            AxonGUI.SetTooltip("AvatarMask filter for this track. Only masked bones will be affected.");
                            AxonGUI.LabelInline("Mask");
                            channel.Mask = (AvatarMask)AxonGUI.FieldObject(target, channel.Mask, typeof(AvatarMask), false);

                            if (AxonGUI.EndChangeCheck()) {
                                target.ForceRefresh();
                            }

                            AxonGUI.EndHorizontal();
                            AxonGUI.EndVertical();
                        }
                        x++;
                    }
                    if (!anyShown) {
                        AxonGUI.HelpBox("No channels have been created.", MessageType.Info);
                    }
                    else {
                        bool updateSort = false;
                        if (moveUp > 0) {
                            int y = moveUp - 1;
                            if (y >= 0) {
                                int order = target.SequencerChannels[moveUp].SortOrder;
                                target.SequencerChannels[moveUp].SortOrder = target.SequencerChannels[y].SortOrder;
                                target.SequencerChannels[y].SortOrder = order;

                                AnimationSequencerChannel tmp = target.SequencerChannels[moveUp];
                                target.SequencerChannels[moveUp] = target.SequencerChannels[y];
                                target.SequencerChannels[y] = tmp;

                                target.SetupChannels(true);
                            }
                            updateSort = true;
                        }
                        if (moveDown > -1) {
                            int y = moveDown + 1;
                            if (y < target.SequencerChannels.Count) {
                                int order = target.SequencerChannels[moveDown].SortOrder;
                                target.SequencerChannels[moveDown].SortOrder = target.SequencerChannels[y].SortOrder;
                                target.SequencerChannels[y].SortOrder = order;

                                AnimationSequencerChannel tmp = target.SequencerChannels[moveDown];
                                target.SequencerChannels[moveDown] = target.SequencerChannels[y];
                                target.SequencerChannels[y] = tmp;

                                target.SetupChannels(true);
                            }
                            updateSort = true;
                        }
                        if (toRemove.Count > 0) {
                            foreach (AnimationSequencerChannel channel in toRemove) {
                                channel.Behavior.RemoveChannelWithUndo(channel);
                            }
                        }
                        if (updateSort) {
                            target.SortChannels();
                        }
                    }
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void AnimationClipsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowAnimationClips = AxonGUI.Foldout(target.EditorShowAnimationClips, "Animation Clips");
            AxonGUI.SetTooltip("Add a new animation clip slot.");
            if (AxonGUI.ButtonInline("+")) {
                target.AnimationClips.Add(null);
            }
            AxonGUI.FlexibleSpace();

            bool hasClips = target.AnimationClips != null && target.AnimationClips.Count > 0;

            AxonGUI.BeginDisabledGroup(!hasClips);
            AxonGUI.SetTooltip("Remove all the animation clips");
            if (AxonGUI.ButtonInline("Clear All")) {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all the animation clips?", "Yes", "No")) {
                    target.AnimationClips.Clear();
                }
            }
            AxonGUI.SetTooltip("Sort the animation clips by name alphabetically");
            if (AxonGUI.ButtonInline("Sort")) {
                target.AnimationClips.Sort((a, b) => a.name.CompareTo(b.name));
            }
            AxonGUI.EndDisabledGroup();

            AxonGUI.SetTooltip("Reloads all animation clips from the current Animation Controller.");
            if (AxonGUI.ButtonInline("Reload")) {
                target.ReloadAnimationClips();
            }
            AxonGUI.EndHorizontal();

            if (target.EditorShowAnimationClips) {
                AxonGUI.BeginBoxPadded();

                if (hasClips) {
                    int moveUp = -1;
                    int moveDown = -1;
                    int remove = -1;

                    for (int x = 0; x < target.AnimationClips.Count; x++) {
                        AxonGUI.BeginHorizontal();

                        AxonGUI.LabelInline(x + ":");
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove")) {
                            remove = x;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                            moveUp = x;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                            moveDown = x;
                        }

                        target.AnimationClips[x] = (AnimationClip)AxonGUI.FieldObjectInline(target, target.AnimationClips[x], typeof(AnimationClip), false);

                        AxonGUI.EndHorizontal();
                    }

                    if (remove > -1) {
                        target.AnimationClips.RemoveAt(remove);
                    }
                    if (moveUp > 0) {
                        UndoUtil.Undo(target, "Reorder", true);
                        AnimationClip a = target.AnimationClips[moveUp];
                        AnimationClip b = target.AnimationClips[moveUp - 1];
                        target.AnimationClips[moveUp] = b;
                        target.AnimationClips[moveUp - 1] = a;
                    }
                    if (moveDown >= 0 && moveDown < target.AnimationClips.Count - 1) {
                        UndoUtil.Undo(target, "Reorder", true);
                        AnimationClip a = target.AnimationClips[moveDown];
                        AnimationClip b = target.AnimationClips[moveDown + 1];
                        target.AnimationClips[moveDown] = b;
                        target.AnimationClips[moveDown + 1] = a;
                    }
                }
                else {
                    AxonGUI.HelpBox("No animation clips have been added yet.", MessageType.Info);
                }
                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }
    }

}//AxonGenesis

#endif
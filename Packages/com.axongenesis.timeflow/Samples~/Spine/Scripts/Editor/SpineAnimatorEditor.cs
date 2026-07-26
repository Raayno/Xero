// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if USING_SPINE
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(SpineAnimator))]
    public class SpineAnimatorEditor : AxonGenesisEditor<SpineAnimator, SpineAnimatorEdit> { }
    sealed public class SpineAnimatorEdit : AxonGenesisBehaviorEdit<SpineAnimator>
    {
#if TIMEFLOW_PRO
        public const string kAddSpineAnimator = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🤺 Spine Animator";
#else
        public const string kAddSpineAnimator = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Spine Animator";
#endif

        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddSpineAnimator, false, 106)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddSpineAnimator, false, 106)]
        public static void AddSpineTimeflow()
        {           
            ObjectUtil.GetOrAddComponent<SpineAnimator>(TimeflowMenu.GetSelectedOrNewGameObject("Spine Animator"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public SpineAnimatorEdit() { }

        public SpineAnimatorEdit(SpineAnimator _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/spine-animator";
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            base.GUIMenu();
            AxonGUI.UndoName = "Set Time Scale";
            AxonGUI.SetTooltip("Sets the scale of time on this object affecting the speed of animation playback.");
            target.SpineTimeScale = AxonGUI.FieldFloatInline(target, "Time Scale", target.SpineTimeScale);

            AxonGUI.UndoName = "Set Initial Flip X";
            AxonGUI.SetTooltip("If enabled, the object scale is inverted on the X axis. Note that this affects the local scale transform and is separate from Spine's Initial Flip settings.");
            target.InitialFlipX = AxonGUI.FieldToggleInline(target, "Initial Flip X", target.InitialFlipX);

            AxonGUI.UndoName = "Set Initial Flip Y";
            AxonGUI.SetTooltip("If enabled, the object scale is inverted on the Y axis. Note that this affects the local scale transform and is separate from Spine's Initial Flip settings.");
            target.InitialFlipY = AxonGUI.FieldToggleInline(target, "Initial Flip Y", target.InitialFlipY);
        }

        public override void OnInspectorGUI()
        {
            ChannelsGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.Refresh();
            }
        }

        public void ChannelsGUI()
        {
            if (!target.Enabled) return;
            AxonGUI.BeginBox();
            AxonGUI.SetTooltip("Lists the Spine Channels of this object.");
            target.EditorShowChannels = AxonGUI.Foldout(target.EditorShowChannels, "Channels");
            if (target.EditorShowChannels) {
                AxonGUI.BeginBoxPadded();
                if (target.SpineChannels == null || target.SpineChannels.Count == 0) {
                    AxonGUI.Label("None", "");
                }
                else {
                    bool anyShown = false;
                    int moveUp = -1;
                    int moveDown = -1;
                    int x = 0;
                    List<SpineChannel> toRemove = new List<SpineChannel>();

                    foreach (SpineChannel channel in target.SpineChannels) {
                        if (channel == null) {
                            AxonGUI.Warning("Null channel reference! Press the Refresh button to clear. Please contact support if this issue persists.");
                        }
                        else {
                            if (channel.IsSelected && TimeflowPreferences.Current.ShowTrackColorsInInspector) {
                                Color c = channel.GUIColor;
                                c.a = 0.5f;
                                GUI.color = c;
                                AxonGUI.BeginHorizontal(AxonUI.HeaderStyleSelected);
                            }
                            else {
                                AxonGUI.BeginHorizontal(AxonUI.HeaderStyle);
                            }
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
                                int order = target.SpineChannels[moveUp].SortOrder;
                                target.SpineChannels[moveUp].SortOrder = target.SpineChannels[y].SortOrder;
                                target.SpineChannels[y].SortOrder = order;

                                SpineChannel tmp = target.SpineChannels[moveUp];
                                target.SpineChannels[moveUp] = target.SpineChannels[y];
                                target.SpineChannels[y] = tmp;
                            }
                            updateSort = true;
                        }
                        if (moveDown > -1) {
                            int y = moveDown + 1;
                            if (y < target.SpineChannels.Count) {
                                int order = target.SpineChannels[moveDown].SortOrder;
                                target.SpineChannels[moveDown].SortOrder = target.SpineChannels[y].SortOrder;
                                target.SpineChannels[y].SortOrder = order;

                                SpineChannel tmp = target.SpineChannels[moveDown];
                                target.SpineChannels[moveDown] = target.SpineChannels[y];
                                target.SpineChannels[y] = tmp;
                            }
                            updateSort = true;
                        }
                        if (toRemove.Count > 0) {
                            foreach (SpineChannel channel in toRemove) {
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
    }

}//AxonGenesis

#endif
#endif
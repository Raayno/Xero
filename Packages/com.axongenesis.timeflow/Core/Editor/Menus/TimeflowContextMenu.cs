// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{

    /// <summary>
    /// Presents context menu options within the Timeflow view. The menu is context sensitive to what was
    /// clicked and what objects are selected.
    /// </summary>
    public static class TimeflowContextMenu
    {
        #region DISPLAY MODES

        private static void Show()
        {
            if (TimeflowContext.Menu != null) {
                Vector2 size = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("View"));
                TimeflowContext.Menu.DropDown(new Rect(TimeflowContext.MenuPosition.x - 100f, TimeflowContext.MenuPosition.y, size.x, size.y));
            }
        }

        public static void DisplayGeneral()
        {
            TimeflowContext.Init(null, TimeflowContext.DisplayModes.General);
            DisplayGeneral(null, true);
            Show();
        }

        public static void DisplayGeneral(TimeflowObject obj, bool init)
        {
            if (init) TimeflowContext.Init(obj, TimeflowContext.DisplayModes.General);

            TimeflowContext.Menu.AddSeparator("");
            SelectMenu();
            ObjectMenu();
            TimeflowContext.Menu.AddSeparator("");
        }

        public static void DisplayObject(TimeflowObject obj)
        {
            TimeflowContext.Init(obj, TimeflowContext.DisplayModes.Object);

            if (TimeflowContext.Obj != null) {
                TimeflowContext.Menu.AddItem(new GUIContent(TimeflowContext.Obj.name + ": " + (TimeflowContext.Obj.BehaviorsEnabled ? "Enabled" : "Disabled")), TimeflowContext.Obj.BehaviorsEnabled, ToggleBehaviors);
            }
            DisplayGeneral(obj, false);

            TimeflowContext.BehaviorMenus();

            Show();
        }

        public static void DisplayChannel(TimeflowObject obj, TimeflowChannel channel)
        {
            TimeflowContext.Init(obj, TimeflowContext.DisplayModes.Channel);
            TimeflowContext.Channel = channel;

            ChannelMenu();
            Show();
        }

        public static void DisplayKeys(TimeflowObject obj)
        {
            TimeflowContext.Init(obj, TimeflowContext.DisplayModes.Keys);

            if (TimeflowContext.HasKeys && !TimeflowContext.HasTracks) {//Event.current != null && Event.current.shift) {
                TimeflowContext.Channel = Timeflow.Active.View.SelectedKeys[0].Channel;
                SelectedKeysMenu(true);
            }
            else {
                SelectKeysMenu();
                if (TimeflowContext.HasTracks) {
                    TracksMenu();
                }
                SelectedKeysMenu();
                MarkersMenu();
                ViewMenu();
                WorkAreaMenu();
                TimeflowContext.BehaviorMenus();
            }
            Show();
        }

        public static void DisplayTimebar()
        {
            TimeflowContext.Init(null, TimeflowContext.DisplayModes.Timebar);
            MarkersMenu();
            ViewMenu();
            WorkAreaMenu();
            Show();
        }

        #endregion

        #region DISPLAY PARTS

        private static void ObjectMenu()
        {
            if (!Timeflow.Active.View.IsGraphMode) {
                string menuName = "Object/";
                TimeflowObject tobj = TimeflowContext.Obj != null ? TimeflowContext.Obj.GetComponent<TimeflowObject>() : null;
                if (tobj != null) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Track Visibility/On"), tobj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.On, TrackVisibilityOn);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Track Visibility/Activate"), tobj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate, TrackVisibilityActivate);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Track Visibility/Renderer"), tobj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.Renderer, TrackVisibilityRenderer);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Track Visibility/Renderer Independent"), tobj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.RendererIndependent, TrackVisibilityRendererIndependent);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Track Visibility/Activate Children"), tobj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.ActivateChildren, TrackVisibilityRendererActivateChildren);
                }
            }
            TimeflowContext.Menu.AddSeparator("");
            // Replaced by Sort Alphabetically
            //TimeflowContext.Menu.AddItem(new GUIContent("Object/Sort View"), false, SortView);
            if (TimeflowContext.AnyObjectsSelected) {
                if (TimeflowContext.AnyChildrenHidden) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Show Children"), false, ShowChildren);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Hide Children"), false, HideChildren);
                }
                TimeflowContext.Menu.AddItem(new GUIContent("Object/Sort Children"), false, SortChildren);
                TimeflowContext.Menu.AddSeparator("Object/");
                TimeflowContext.Menu.AddItem(new GUIContent("Object/New Child Object"), false, AddChild);
            }
            TimeflowContext.Menu.AddItem(new GUIContent("Object/New Game Object"), false, AddGameObject);
            if (TimeflowContext.AnyObjectsSelected) {
                TimeflowContext.Menu.AddSeparator("Object/");
                TimeflowContext.Menu.AddItem(new GUIContent("Object/Group Selected"), false, GroupObjects);
                TimeflowContext.Menu.AddItem(new GUIContent("Object/Ungroup"), false, UngroupObjects);

                TimeflowContext.Menu.AddSeparator("Object/");
                TimeflowContext.Menu.AddItem(new GUIContent("Object/Set as First Sibling"), false, SetAsFirstSibling);
                TimeflowContext.Menu.AddItem(new GUIContent("Object/Set as Last Sibling"), false, SetAsLastSibling);

                if (TimeflowContext.Obj != null) {
                    TimeflowContext.Menu.AddSeparator("Object/");
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Reset Tracks"), false, ResetObjectTracks);
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Join Adjacent Tracks"), false, TimeflowObject.JoinAdjacentTracks);
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Remove From View"), false, RemoveFromDisplay);
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Destroy All Timeflow Behaviors"), false, DeleteBehaviors);
                    TimeflowContext.Menu.AddItem(new GUIContent("Object/Delete Game Objects"), false, DeleteSelectedGameObjects);

                    if (TimeflowContext.Obj is Timeflow tf) {
                        TimeflowContext.Menu.AddItem(new GUIContent("Decompose"), false, Decompose);
                    }
                    else {
                        TimeflowContext.Menu.AddItem(new GUIContent("Precompose"), false, Precompose);
                    }
                    TimeflowContext.Menu.AddItem(new GUIContent("Unpack Prefab"), false, UnpackPrefab);
                    TimeflowContext.Menu.AddSeparator("/");
                }
            }
            TimeflowContext.Menu.AddItem(new GUIContent("Add New Precomp"), false, AddNewPrecomp);
            TimeflowContext.Menu.AddItem(new GUIContent("Sort Alphabetically"), false, SortAlphabetically);
            ChannelHeightsMenu();
        }

        private static void ChannelHeightsMenu()
        {
            bool hasUnlocked = TimeflowContext.HasUnlockedChannelHeights;
            TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/Reset"), false, hasUnlocked ? ResetSelectedChannelHeights : null);
            if (TimeflowPreferences.Current.ChannelHeights.Count > 0) {
                foreach (float height in TimeflowPreferences.Current.ChannelHeights) {
                    if (hasUnlocked) {
                        TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/" + height), false, SetSelectedChannelHeight, height);
                    }
                    else {
                        TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/" + height), false, null);
                    }
                }
            }
            TimeflowContext.Menu.AddSeparator("Set Channel Height/");
            TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/Locked"), TimeflowContext.HasLockedChannelHeights, ToggleSelectedChannelHeightLocked);
            TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/Increase"), false, hasUnlocked ? IncreaseSelectedChannelHeights : null);
            TimeflowContext.Menu.AddItem(new GUIContent("Set Channel Height/Decrease"), false, hasUnlocked ? DecreaseSelectedChannelHeights : null);
        }

        private static void ChannelMenu()
        {
            if (TimeflowContext.Channel == null) {
                if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    TimeflowContext.Channel = Timeflow.Active.View.SelectedChannels[0];
                }
            }
            if (TimeflowContext.Channel == null) return;

            string menuName = "";

            if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 1) {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + Timeflow.Active.View.SelectedChannels.Count + " Channels Selected"), false, null);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + TimeflowContext.Channel.Name), false, null);
            }

            // Allow the channel to add its own items to the menu
            TimeflowContext.Channel.GUIChannelContextMenu(TimeflowContext.Menu);

            AxonGUI.PresetsMenu(TimeflowContext.Menu, menuName, "Presets/", TimeflowContext.Channel.Behavior, TimeflowContext.Channel.Behavior.gameObject);
            TimeflowContext.Menu.AddSeparator(menuName);


            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Always Update"), TimeflowContext.Channel.AlwaysUpdate, ToggleAlwaysUpdate);
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Always Show Values"), TimeflowContext.Channel.AlwaysShowValues, ToggleAlwaysShowValues);
            if (!TimeflowContext.Channel.IsTrack) {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Remove Channel (" + TimeflowContext.Channel.Name + ")"), false, RemoveChannel);
            }
            if (TimeflowContext.HasChannels) {
                if (TimeflowContext.HasNoneTrackChannels) {
                    if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 1) {
                        TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Remove Selected Channels"), false, RemoveSelectedChannels);
                    }
                }
                if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Select Keys in Selected Channels"), false, SelectChannelKeyframes);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Select Keys in Selected Channels"), false, null);
                }

                if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Clear Keys in Selected Channels"), false, ClearChannelKeyframes);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Clear Keys in Selected Channels"), false, null);
                }
            }

            if (TimeflowContext.HasChannels && TimeflowContext.IsMultichannel && TimeflowContext.CanSeparate) {
                TimeflowContext.Menu.AddSeparator(menuName);
                if (TimeflowContext.IsCombined) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Combine Channels"), false, null);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Separate Channels"), false, SeparateChannels);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Combine Channels"), false, CombineChannels);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Separate Channels"), false, null);
                }
            }

            TimeflowContext.Menu.AddSeparator(menuName);
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Reset Channel Names"), false, ResetSelectedChannelNames);
            ChannelHeightsMenu();

            if (TimeflowContext.HasChannels) {
                TimeflowContext.Menu.AddSeparator(menuName);
#if AXON_DEVELOPMENT
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Import/Curve"), false, ImportCurve);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Import/Unity AnimationCurve"), false, ImportAnimationCurve);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Export/Curve"), false, ExportCurve);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Export/Unity AnimationCurve"), false, ExportAnimationCurve);
#endif
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Bake Keyframes"), false, BakeKeyframes);
                if (TimeflowContext.HasTimeOffsets) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Freeze Time Offset"), false, FreezeTimeOffset);
                }
                if (TimeflowContext.IsLoopExpandable) {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Expand Looped Keyframes"), false, ExpandLoopedKeyframes);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Expand Looped Keyframes"), false, null);
                }
            }

            TimeflowContext.Menu.AddSeparator(menuName);
            ChannelLinkMenu();
        }

        private static void ChannelLinkMenu()
        {
            if (TimeflowContext.Channel != null) {
                bool multiple = Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 1;
                bool isLinkedTo = TimeflowContext.Channel.IsLinked;
                bool isLinkedFrom = TimeflowContext.Channel.LinkedFrom != null && TimeflowContext.Channel.LinkedFrom.Count > 0;
                int linkedFromCount = isLinkedFrom ? TimeflowContext.Channel.LinkedFrom.Count : 0;

                List<TimeflowChannel> linkedFrom = new List<TimeflowChannel>();

                if (multiple) {
                    linkedFromCount = 0;
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch.IsLinked) {
                            isLinkedTo = true;
                            if (!TimeflowContext.Channel.IsLinked) TimeflowContext.Channel.Link = ch.Link;
                        }
                        if (ch.LinkedFrom != null && ch.LinkedFrom.Count > 0) {
                            isLinkedFrom = true;
                            linkedFromCount += ch.LinkedFrom.Count;

                            foreach (TimeflowChannel chl in ch.LinkedFrom) {
                                linkedFrom.Add(chl);
                            }
                        }
                    }
                }

                string baseMenuName = "";
                if (!isLinkedTo) {
                    TimeflowContext.Menu.AddItem(new GUIContent(baseMenuName + "Linked To None"), false, null);
                }
                else {
                    string menuName = baseMenuName + "Linked To/";
                    string name = TimeflowContext.Channel.Link.Channel.Name;

                    if (multiple) name = "(Multiple Selected)";

                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Enabled: " + name), TimeflowContext.Channel.Link.Enabled, ToggleChannelLink);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Remove: " + name), false, RemoveChannelLink);
                    TimeflowContext.Menu.AddSeparator(menuName);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Show and Select Channels"), false, RevealLinkedObjectsForSelectedChannels);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Insert Data Channel"), false, InsertDataChannelLink);
                    TimeflowContext.Menu.AddSeparator(menuName);

                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Overwrite"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Overwrite, SetChannelLinkModeOverwrite);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Add"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Add, SetChannelLinkModeAdd);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Subtract"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Subtract, SetChannelLinkModeSubtract);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Multiply"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Multiply, SetChannelLinkModeMultiply);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Max"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Max, SetChannelLinkModeMax);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Min"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Min, SetChannelLinkModeMin);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "One Minus"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.OneMinus, SetChannelLinkModeOneMinus);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Custom"), TimeflowContext.Channel.Link.Mode == TimeflowChannelLink.Modes.Custom, SetChannelLinkModeCustom);
                }

                if (isLinkedFrom) {
                    string menuName = baseMenuName + "Linked From " + linkedFromCount + "/";
                    if (multiple) TimeflowContext.Menu.AddItem(new GUIContent(menuName + "(Multiple Selected)"), false, null);

                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Remove All"), false, RemoveLinkedChannels);
                    TimeflowContext.Menu.AddSeparator(menuName);
                    TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Show and Select Channels"), false, RevealLinkedObjectsForSelectedChannels);

                    foreach (TimeflowChannel c in linkedFrom) {
                        TimeflowContext.Menu.AddSeparator(menuName);
                        TimeflowContext.Menu.AddItem(new GUIContent(menuName + c.Name), c.Link.Enabled, ToggleIndividualChannelLink, (object)c);
                    }
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(baseMenuName + "Linked From None"), false, null);
                }

            }
        }

        private static void SelectMenu()
        {
            TimeflowContext.Menu.AddItem(new GUIContent("Select/All"), false, SelectAll);
            if (TimeflowContext.AnyObjectsSelected) {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Children"), false, SelectChildren);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Descendants"), false, SelectDescendants);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Channels"), false, SelectChannels);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Children"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Channels"), false, null);
            }
            TimeflowContext.Menu.AddSeparator("Select/");
            if (TimeflowContext.AnyObjectsSelected) {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Parents"), false, SelectParents);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Ancestors"), false, SelectAncestors);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Parent"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Ancestors"), false, null);
            }
        }

        private static void WorkAreaMenu()
        {
            TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Enable"), Timeflow.Active.WorkAreaEnabled, ToggleWorkArea);
            TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Locked"), Timeflow.Active.WorkAreaLocked, ToggleWorkAreaToggleLock);
            TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Allow Lead-In"), Timeflow.Active.WorkAreaAllowsLeadIn, ToggleWorkAreaLeadIn);
            if (Timeflow.Active.WorkAreaLocked) {
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Selected"), false, WorkAreaWithSelected);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Visible View"), false, SetWorkAreaToScrollRegion);
            }
            else {
                if (TimeflowContext.HasKeys) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Selected"), false, WorkAreaWithSelected);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Selected"), false, null);
                }
                if (TimeflowContext.HasMarker) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Marker"), false, WorkAreaWithMarker);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Marker"), false, null);
                }
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Set to Visible View"), false, SetWorkAreaToScrollRegion);
            }

            if (Timeflow.Active.WorkAreaEnabled) {
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear All"), false, ClearTimeInWorkArea);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear Tracks"), false, ClearTracksInWorkArea);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear Keyframes"), false, ClearKeyframesInWorkArea);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Insert Time"), false, InsertTimeInWorkArea);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Duplicate Time"), false, DuplicateTimeInWorkArea);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Delete Time"), false, DeleteTimeInWorkArea);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Insert Time Global"), false, InsertTimeInWorkAreaGlobal);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Duplicate Time Global"), false, DuplicateTimeInWorkAreaGlobal);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Delete Time Global"), false, DeleteTimeInWorkAreaGlobal);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Reveal All Tracks"), false, RevealAllTracksInWorkAreaGlobal);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Reveal All Keyframes"), false, RevealAllKeyframesInWorkAreaGlobal);
            }
            else {
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear All"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear Tracks"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Clear Keyframes"), false, null);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Insert Time"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Duplicate Time"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Delete Time"), false, null);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Insert Time Global"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Duplicate Time Global"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Delete Time Global"), false, null);
                TimeflowContext.Menu.AddSeparator("Work Area/");
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Reveal All Tracks"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("Work Area/Reveal All Keyframes"), false, null);
            }
        }

        private static void SelectKeysMenu()
        {
            TimeflowContext.Menu.AddItem(new GUIContent("Select/All"), false, SelectAllKeys);
            if (!Timeflow.Active.View.IsGraphMode) {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/All Keys"), false, SelectAllKeysOnly);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/All Tracks"), false, SelectAllTracksOnly);

                TimeflowContext.Menu.AddSeparator("Select/");
                if (TimeflowContext.HasTracks) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys Related to Track"), false, SelectRelatedKeys);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys Related to Track"), false, null);
                }
                if (Timeflow.Active.View.Markers.SelectedMarker != null) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys Related to Marker " + Timeflow.Active.View.Markers.SelectedMarker.Name), false, SelectKeysRelatedToMarker);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys Related to Marker"), false, null);
                }
            }
            if (Timeflow.Active.WorkAreaEnabled) {
                TimeflowContext.Menu.AddSeparator("Select/");
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Work Area"), false, SelectWorkArea);
                if (!Timeflow.Active.View.IsGraphMode) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Work Area Keys"), false, SelectWorkAreaKeysOnly);
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Work Area Tracks"), false, SelectWorkAreaTracksOnly);
                }
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Work Area"), false, null);
            }
            TimeflowContext.Menu.AddSeparator("Select/");
            TimeflowContext.Menu.AddItem(new GUIContent("Select/All at Current Time"), false, SelectAllAtCurrentTime);
            if (!Timeflow.Active.View.IsGraphMode) {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys at Current Time"), false, SelectKeysAtCurrentTime);
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Tracks at Current Time"), false, SelectTracksAtCurrentTime);
            }
            TimeflowContext.Menu.AddSeparator("Select/");
            if (TimeflowContext.HasKeys) {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys By Value"), false, SelectKeysByValue);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Select/Keys By Value"), false, null);
            }
            if (!Timeflow.Active.View.IsGraphMode) {
                if (Timeflow.Active.View.AnyTracksSelected) {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Tracks By Color"), false, SelectTracksByColor);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent("Select/Tracks By Color"), false, null);
                }
            }
        }

        private static void SelectedKeysMenu(bool standalone = false)
        {
            bool keysCopied = Timeflow.Active.View.CopiedKeys != null && Timeflow.Active.View.CopiedKeys.Count > 0;

            string prefix = standalone ? "" : "Keyframes/";
            bool keysEnabled = true;
            if (TimeflowContext.HasKeys) {
                foreach (Keyframe k in Timeflow.Active.View.SelectedKeys) {
                    if (!k.IsKeyEnabled) {
                        keysEnabled = false;
                    }
                }
            }

            if (TimeflowContext.Channel != null) {
                TimeflowContext.Channel.GUISelectedKeysContextMenu(TimeflowContext.Menu);
            }

            //menu.AddSeparator("");
            //TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Select All"), false, SelectAllKeysDisplayed);
            if (TimeflowContext.HasChannels) {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Clear Selected Channels"), false, ClearChannelKeyframes);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Clear Selected Channels"), false, null);
            }
            TimeflowContext.Menu.AddSeparator(prefix + "");

            if (TimeflowContext.HasKeys) {
                bool isBezier = false;
                bool anyLinear = false;
                bool anyHold = false;
                if (Timeflow.Active.View.SelectedChannels != null) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch.Interpolation == TimeflowChannel.Interpolations.Bezier) {
                            isBezier = true;
                            break;
                        }
                    }
                }
                if (Timeflow.Active.View.SelectedKeys != null) {
                    foreach (var key in Timeflow.Active.View.SelectedKeys) {
                        if (key.Channel != null && key.Channel.Interpolation == TimeflowChannel.Interpolations.Bezier) {
                            isBezier = true;
                        }
                        if (key.Linear) {
                            anyLinear = true;
                        }
                        if (key.Hold) {
                            anyHold = true;
                        }
                    }
                }
                if (isBezier) {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Auto"), false, SetKeyInterpolationAuto);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Linear"), anyLinear, SetKeyInterpolationLinear);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Linear Left"), false, SetKeyInterpolationLinearLeft);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Linear Right"), false, SetKeyInterpolationLinearRight);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Hold"), anyHold, SetKeyInterpolationHold);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Flat"), false, SetKeyInterpolationFlat);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Flat Left"), false, SetKeyInterpolationFlatLeft);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Flat Right"), false, SetKeyInterpolationFlatRight);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Hide Tangents"), false, HideKeyTangents);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Show Tangents"), false, ShowKeyTangents);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Linear"), anyLinear, SetKeyInterpolationLinear);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Interpolation/Hold"), anyHold, SetKeyInterpolationHold);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Hide Tangents"), false, null);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Show Tangents"), false, null);
                }

                TimeflowContext.Menu.AddSeparator(prefix + "");
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Enable"), keysEnabled, EnableKeys);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Disable"), !keysEnabled, DisableKeys);


                TimeflowContext.Menu.AddSeparator(prefix + "");

                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Copy"), false, CopyKeys);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Cut"), false, CutKeys);
                if (keysCopied) {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste"), false, PasteKeys);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste (keep time)"), false, PasteKeysInTime);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste Tangents"), false, PasteKeyTangents);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste"), false, null);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste (keep time)"), false, null);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste Tangents"), false, null);
                }
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Delete"), false, DeleteKeys);

                TimeflowContext.Menu.AddSeparator(prefix + "");
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Snap Time"), false, SnapTimeOfSelected);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Snap Values"), false, SnapValuesOfSelected);

                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Modify Time/Mirror"), false, MirrorTimeOfSelected);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Modify Time/Randomize"), false, RandomizeTimeOfSelected);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Modify Values/Mirror"), false, MirrorValuesOfSelected);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Modify Values/Randomize"), false, RandomizeValuesOfSelected);

                TimeflowContext.Menu.AddSeparator(prefix + "");
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Loop Selected"), false, LoopSelectedKeys);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Loop All"), false, LoopSelectedChannels);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Clear Loop"), false, UnloopSelectedChannels);

            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Snap Selected"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Copy"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Cut"), false, null);
                if (keysCopied) {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste"), false, PasteKeys);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste (keep time)"), false, PasteKeysInTime);
                }
                else {
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste"), false, null);
                    TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Paste (keep time)"), false, null);
                }
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Delete"), false, null);

                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Loop Selected"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Loop All"), false, LoopSelectedChannels);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Clear Loop"), false, UnloopSelectedChannels);


                TimeflowContext.Menu.AddSeparator(prefix + "");
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Enable"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Disable"), false, null);
            }

            TimeflowContext.Menu.AddSeparator(prefix + "");
            TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Bake Keyframes"), false, BakeKeyframes);
            if (TimeflowContext.HasTimeOffsets) {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Freeze Time Offset"), false, FreezeTimeOffset);
            }
            if (TimeflowContext.IsLoopExpandable) {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Expand Looped Keyframes"), false, ExpandLoopedKeyframes);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(prefix + "Expand Looped Keyframes"), false, null);
            }
        }

        private static void TracksMenu()
        {
            if (Timeflow.Active.View.IsGraphMode) return;

            string menuName = "Tracks/";

            if (TimeflowContext.HasTracks) {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Go to Start"), false, TracksGotoStart);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set Start"), false, TracksSetStart);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Go to End"), false, TracksGotoEnd);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set End"), false, TracksSetEnd);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Go to Start"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set Start"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Go to End"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set End"), false, null);
            }

            TimeflowContext.Menu.AddSeparator(menuName);
            if (TimeflowContext.HasTracks) {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Join Tracks"), false, JoinSelectedTracks);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Join Tracks"), false, null);
            }
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Split Tracks"), false, SplitTracksAtCurrentTime);

            TimeflowContext.Menu.AddSeparator(menuName);
            if (Timeflow.Active.WorkAreaEnabled) {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Split Tracks by Work Area"), false, SplitAllTracksWithWorkArea);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set Track to Work Area"), false, SetTracksToWorkArea);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Split Tracks by Work Area"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Set Track to Work Area"), false, null);
            }

            TimeflowContext.Menu.AddSeparator(menuName);
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Reset Selected Tracks"), false, ResetSelectedTracks);

            TimeflowContext.Menu.AddSeparator(menuName);
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Drag Time Offset"), TimeflowContext.HasDraggableTracks, CanDragTimeOffset);
            TimeflowContext.Menu.AddItem(new GUIContent(menuName + "Loop Time Offset"), TimeflowContext.HasLoopTimeOffset, ToggleLoopTimeOffset);
        }

        private static void MarkersMenu()
        {
            TimeflowContext.Menu.AddItem(new GUIContent("Markers/Add Marker at Current Time"), false, AddTimeMarker);

            if (Timeflow.Active.View.Markers.SelectedMarker != null) {
                TimeflowContext.Menu.AddItem(new GUIContent("Markers/Remove Selected Marker"), false, RemoveTimeMarker);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Markers/Remove Selected Marker"), false, null);
            }
            //TimeflowContext.Menu.AddSeparator("Markers/");
        }

        private static void ViewMenu()
        {
            string fitName = "View/Fit All";
            if (Timeflow.Active.WorkAreaEnabled) {
                fitName += " (Work Area)";
            }
            TimeflowContext.Menu.AddItem(new GUIContent(fitName), false, FitAll);

            if (TimeflowContext.HasKeys || TimeflowContext.HasMarker) {
                TimeflowContext.Menu.AddItem(new GUIContent("View/Fit Selected"), false, FitSelected);
                TimeflowContext.Menu.AddItem(new GUIContent("View/Set Local Time Scope"), false, SetTimeScope);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("View/Fit Selected"), false, null);
                TimeflowContext.Menu.AddItem(new GUIContent("View/Set Local Time Scope"), false, null);
            }

            TimeflowContext.Menu.AddItem(new GUIContent("View/Local Time Scope"), Timeflow.Active.IsTimeScopeEnabled, ToggleTimeScope);

            TimeflowContext.Menu.AddItem(new GUIContent("View/Refresh View"), false, Refresh);
        }

        #endregion

        #region VIEW

        public static void Refresh()
        {
            Timeflow.GlobalRefresh();
        }

        public static void ShowSelected()
        {
            Timeflow.Active.View.Display.ObjectMode = TimeflowViewDisplay.ObjectModes.SelectedGroup;
            Timeflow.Active.Refresh(true);
            //Timeflow.Active.SaveSelectedToRecent();
            //Timeflow.Active.DisplaySelectedHierarchies();
        }

        public static void HideFromTimeflow()
        {
            Timeflow.Active.View.Display.RemoveObjectsFromDisplay(Selection.gameObjects);
        }

        public static void HideAllObjectsFromTimeflow()
        {
            Timeflow.Active.View.Display.DisplayNothing();
        }

        public static void ShowAllObjectsInTimeflow()
        {
            Timeflow.Active.View.Display.DisplayEverything();
        }

        #endregion

        #region SELECT

        public static void SelectAll()
        {
            Timeflow.Active.View.SelectAllObjects(true, true, true);
            Timeflow.Active.View.OnSelectionChange();
        }

        public static void SelectChildren()
        {
            TimeflowMenu.SelectChildren();
        }

        public static void SelectDescendants()
        {
            TimeflowMenu.SelectDescendants();
        }

        public static void SelectParents()
        {
            TimeflowMenu.SelectParents();
        }

        public static void SelectAncestors()
        {
            TimeflowMenu.SelectAncestors();
        }

        [Shortcut(TimeflowShortcutInfo.Path_SelectChannels)]
        public static void SelectChannels()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null && objects.Count > 0) {
                Timeflow.Active.View.SelectChannelsInObjects(objects);
            }
        }

        #endregion

        #region OBJECT

        public static void HideChildren()
        {
            if (TimeflowContext.Obj != null && !TimeflowContext.Obj.IsSelected) {
                UndoUtil.Undo(TimeflowContext.Obj, "Hide Children");
                TimeflowContext.Obj.ShowChildren = false;
                Timeflow.Active.View.NeedsRefresh = true;
            }
            else {
                Timeflow.Active.View.HideChildrenOfSelected(true);
            }
        }

        public static void ShowChildren()
        {
            if (TimeflowContext.Obj != null && !TimeflowContext.Obj.IsSelected) {
                UndoUtil.Undo(TimeflowContext.Obj, "Show Children");
                TimeflowContext.Obj.ShowChildren = true;
                Timeflow.Active.View.NeedsRefresh = true;
            }
            else {
                Timeflow.Active.View.HideChildrenOfSelected(false);
            }
        }

        public static void AddGameObject()
        {
            TimeflowMenu.AddGameObject();
        }

        public static void AddChild()
        {
            TimeflowMenu.AddChild();
        }

        public static void SetAsFirstSibling()
        {
            TimeflowMenu.SetAsFirstSibling();
        }

        public static void SetAsLastSibling()
        {
            TimeflowMenu.SetAsLastSibling();
        }

        public static void GroupObjects()
        {
            TimeflowMenu.GroupObjects();
        }

        public static void GroupObjectsNull()
        {
            TimeflowMenu.GroupObjectsNull();
        }

        public static void UngroupObjects()
        {
            TimeflowMenu.UngroupObjects();
            Timeflow.Active.Refresh(true);
        }

        public static void SortView()
        {
            if (Timeflow.Active.RootObjects == null || Timeflow.Active.RootObjects.Count == 0) return;

            Timeflow.Active.RootObjects.Sort((TimeflowObject t1, TimeflowObject t2) => { return t1.name.CompareTo(t2.name); });
            Timeflow.Active.OnRootObjectsChanged();

            int i = 1;
            foreach (TimeflowObject obj in Timeflow.Active.RootObjectsCached) {
                obj.SortOrder = i * 100;
                obj.transform.SetSiblingIndex(i);
                i++;
            }
            Timeflow.Active.View.Display.SaveSelected();
        }

        public static void SortChildren()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                foreach (GameObject obj in Selection.gameObjects) {
                    ObjectUtil.SortChildrenByName(obj);
                    if (obj.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                        tobj.Refresh();
                    }
                }
            }
        }

        public static void ToggleBehaviors()
        {
            bool enabled = !TimeflowContext.Obj.BehaviorsEnabled;
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Timeflow Enabled");
                    obj.BehaviorsEnabled = enabled;
                }
            }
        }

        public static void RemoveFromDisplay()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    Timeflow.Active.View.Display.RemoveObjectFromDisplayRecursive(obj.gameObject);
                }
            }
        }

        public static void AddNewPrecomp()
        {
            TimeflowEdit.AddPrecomp();
        }

        public static void Precompose()
        {
            TimeflowEdit.Precompose();
        }

        public static void UnpackPrefab()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    if (obj.gameObject != null && PrefabUtility.IsPartOfPrefabInstance(obj.gameObject)) {
                        UndoUtil.Undo(obj, "Unpack Prefab");
                        PrefabUtility.UnpackPrefabInstance(obj.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                }
            }
        }

        public static void Decompose()
        {
            TimeflowEdit.Decompose();
        }

        public static void DeleteBehaviors()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                int option = EditorUtility.DisplayDialogComplex("Delete Timeflow Behaviors", "Are you sure you want to remove all Timeflow Behaviors on this object and all of its descendants?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowObject obj in objects) {
                        DeleteBehaviorsRecursive(obj.gameObject);
                    }
                }
            }
        }

        public static void DeleteSelectedGameObjects()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;
            bool canDelete = true;
            if (TimeflowPreferences.Current.ShowDeleteObjectsWarning) {
                int option = EditorUtility.DisplayDialogComplex("Delete Game Objects", "Are you sure you want to delete the selected game objects and their children?", "Yes", "Cancel", "Yes, don't ask again");
                canDelete = option != 1;
                if (option == 2) {
                    TimeflowPreferences.Current.ShowDeleteObjectsWarning = false;
                }
            }
            if (canDelete) {
                foreach (GameObject obj in Selection.gameObjects) {
                    if (obj == null) continue;
                    UndoUtil.UndoDestroy(obj.gameObject);
                }
                Timeflow.GlobalRefresh();
            }
        }

        public static void DeleteBehaviorsRecursive(GameObject obj)
        {
            TimeflowObject tobj = obj.GetComponent<TimeflowObject>();
            TimeflowBehavior[] behaviors = obj.GetComponents<TimeflowBehavior>();
            foreach (TimeflowBehavior b in behaviors) {
                if (b != tobj) UndoUtil.UndoDestroy(b);
            }
            Rotator[] rotators = obj.GetComponents<Rotator>();
            foreach (Rotator r in rotators) {
                UndoUtil.UndoDestroy(r);
            }
            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    DeleteBehaviorsRecursive(child.gameObject);
                }
            }
            if (tobj != null) UndoUtil.UndoDestroy(tobj);
        }

        #endregion

        #region TRACKS

        public static void TrackVisibilityOn()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set On");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.On;
                    obj.TrackActivated = false;
                    obj.Refresh();
                }
            }
        }

        public static void TrackVisibilityOnSelfOnly()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set On Self Only");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.OnSelfOnly;
                    obj.TrackActivated = false;
                    obj.Refresh();
                }
            }
        }

        public static void TrackVisibilityActivate()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set Activate");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.Activate;
                    obj.TrackActivated = true;
                    obj.Refresh();
                }
            }
        }

        public static void TrackVisibilityRenderer()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set Renderer");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.Renderer;
                    obj.TrackActivated = false;
                    obj.Refresh();
                }
            }
        }

        public static void TrackVisibilityRendererIndependent()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set Renderer Independent");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.RendererIndependent;
                    obj.TrackActivated = false;
                    obj.Refresh();
                }
            }
        }

        public static void TrackVisibilityRendererActivateChildren()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Set Activate Children");
                    obj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.ActivateChildren;
                    obj.TrackActivated = false;
                    obj.Refresh();
                }
            }
        }

        #endregion

        #region CHANNEL

        public static void RemoveSelectedChannels()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                int option = EditorUtility.DisplayDialogComplex("Remove Selected Channels", "Are you sure you want to remove all the selected channels and their keyframes?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior, "Remove Selected Channels");
                        ch.Behavior.RemoveChannelWithUndo(ch);
                    }
                    Timeflow.Active.View.SelectedChannels = new List<TimeflowChannel>();
                }
            }
        }

        public static void ResetSelectedChannelNames()
        {
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Reset Channel Name");
                    ch.ResetName();
                }
            }
        }

        public static void ResetSelectedChannelHeights()
        {
            SetSelectedChannelHeights(0);
        }

        public static void SetSelectedChannelHeight(object height)
        {
            SetSelectedChannelHeights(Convert.ToInt32(height));
        }

        public const string _DecreaseSelectedChannelHeights = "Timeflow/Decrease Selected Channel Heights";
        [Shortcut(_DecreaseSelectedChannelHeights, typeof(TimeflowWindow), KeyCode.LeftBracket)]
        public static void DecreaseSelectedChannelHeights()
        {
            IncrementSelectedChannelHeight(false);
        }

        public const string _IncreaseSelectedChannelHeights = "Timeflow/Increase Selected Channel Heights";
        [Shortcut(_IncreaseSelectedChannelHeights, typeof(TimeflowWindow), KeyCode.RightBracket)]
        public static void IncreaseSelectedChannelHeights()
        {
            IncrementSelectedChannelHeight(true);
        }

        public static void IncrementSelectedChannelHeight(bool increase)
        {
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Increment Channel Height");
                    ch.GUIHeight = TimeflowPreferences.Current.GetNextChannelHeight(ch.GUIHeight, increase);
                }
            }

            if (Timeflow.Active.View.SelectedObjects != null) {
                foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                    obj.Track.GUIHeight = TimeflowPreferences.Current.GetNextChannelHeight(obj.Track.GUIHeight, increase);
                }
            }
        }

        public static void SetSelectedChannelHeights(int height)
        {
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Reset Channel Height");
                    ch.GUIHeight = height;
                }
            }

            if (Timeflow.Active.View.SelectedObjects != null) {
                foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                    obj.Track.GUIHeight = height;
                }
            }
        }

        public const string _ToggleSelectedChannelHeightLocked = "Timeflow/Toggle Lock Selected Channel Heights";
        [Shortcut(_ToggleSelectedChannelHeightLocked, typeof(TimeflowWindow), KeyCode.RightBracket, ShortcutModifiers.Action)]
        public static void ToggleSelectedChannelHeightLocked()
        {
            bool first = true;
            bool locked = false;// !TimeflowContext.HasLockedChannelHeights;
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    if (first) {
                        first = false;
                        locked = !ch.GUIHeightLocked;
                    }
                    UndoUtil.Undo(ch.Behavior, "Reset Channel Height");
                    ch.GUIHeightLocked = locked;
                }
            }

            if (Timeflow.Active.View.SelectedObjects != null) {
                foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                    if (first) {
                        first = false;
                        locked = !obj.Track.GUIHeightLocked;
                    }
                    obj.Track.GUIHeightLocked = locked;
                }
            }
        }

        public static void SortAlphabetically()
        {
            List<TimeflowObject> objects = Timeflow.Active.View.SelectedObjects;
            if (objects == null || objects.Count == 0) {
                objects = Timeflow.Active.View.Display.GetObjectsDisplayed();
            }
            if (objects == null || objects.Count == 0) {
                Debug.LogWarning("No objects selected to sort alphabetically.");
                return;
            }

            // Maintain the sorting order of non-selected objects by starting at
            // the first sort order.
            int i = objects[0].SortOrder;
            objects.Sort((x, y) => {
                return x.Name.CompareTo(y.Name);
            });

            //Debug.Log($"SortAlphabetically:{objects.Count}");   
            // Since sort orders are in 100s, we can increment by 1 to prevent the
            // change to selected orders potentially overrunning other objects. This
            // squeezes all the selected objects right up to the starting index. It
            // gets rexpanded in increments of 100 by the display setup.
            foreach (TimeflowObject obj in objects) {
                //Debug.Log($"{i} SortAlphabetically:{obj.name}");
                if (obj.IsLocked) continue;
                UndoUtil.Undo(obj, "Sort Alphabetically");
                if (obj.Behaviors != null && obj.Behaviors.Count > 0) {
                    foreach (TimeflowBehavior behavior in obj.Behaviors) {
                        behavior.SortAlphabetically();
                        i++;
                    }
                }
                obj.SortOrder = i;
                i++;
            }

            Timeflow.Active.Refresh();
        }

        private static TimeflowChannel GetTargetChannel()
        {
            if (TimeflowContext.Channel != null) return TimeflowContext.Channel;

            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null || channels.Count == 0) {
                Debug.LogWarning("Please select a channel with keyframes to export an Animation Curve");
                return null;
            }
            // Return the first selected channel
            TimeflowChannel channel = null;
            foreach (TimeflowChannel ch in channels) {
                if (ch.IsTrack || !ch.SupportsKeyframes || ch.Keys.Count == 0) continue;
                channel = ch;
                break;
            }
            return channel;
        }

        public static void ImportCurve()
        {
            TimeflowChannel channel = GetTargetChannel();
            if (channel == null) {
                Debug.LogWarning("Please select a channel to import an Animation Curve");
                return;
            }

            CurveAsset asset = Selection.activeObject as CurveAsset;
            if (asset == null) {
                string path = AssetDatabase.GUIDToAssetPath("039329be76c6b774fa7eb91f8299b24d");
                if (!string.IsNullOrEmpty(path)) {
                    asset = AssetDatabase.LoadAssetAtPath<CurveAsset>(path);
                }
                if (asset == null) {
                    Debug.LogWarning("Please select an Animation Curve Container to import");
                    return;
                }
            }
            if (asset.Curve == null) {
                Debug.LogWarning("Please assign an Animation Curve to the container");
                return;
            }

            channel.ImportCurve(asset.Curve);
        }

        public static void ExportCurve()
        {
            TimeflowChannel channel = GetTargetChannel();
            if (channel == null) {
                Debug.LogWarning("Please select a channel with keyframes to export an Animation Curve");
                return;
            }

            Curve curve = channel.ExportCurve();
            CurveAsset.SaveAsset(curve, "Assets/Ignore/TestCurve2.asset");
        }

        public static void ImportAnimationCurve()
        {
            TimeflowChannel channel = GetTargetChannel();
            if (channel == null) {
                Debug.LogWarning("Please select a channel to import an Animation Curve");
                return;
            }

            AnimationCurveContainer container = Selection.activeObject as AnimationCurveContainer;
            if (container == null) {
                string path = AssetDatabase.GUIDToAssetPath("a2407863f79470748aa815453b8eebbd");
                if (!string.IsNullOrEmpty(path)) {
                    container = AssetDatabase.LoadAssetAtPath<AnimationCurveContainer>(path);
                }
                if (container == null) {
                    Debug.LogWarning("Please select an Animation Curve Container to import");
                    return;
                }
            }
            if (container.Curve == null) {
                Debug.LogWarning("Please assign an Animation Curve to the container");
                return;
            }

            channel.ImportAnimationCurve(container.Curve);
        }

        public static void ExportAnimationCurve()
        {
            TimeflowChannel channel = GetTargetChannel();
            if (channel == null) {
                Debug.LogWarning("Please select a channel with keyframes to export an Animation Curve");
                return;
            }

            AnimationCurve curve = channel.ExportAnimationCurve();
            AnimationCurveUtil.SaveCurveAsAsset(curve, "Assets/Ignore/TestCurve.asset");
        }

        public static void BakeKeyframes()
        {
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null || channels.Count == 0) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                float startTime = Timeflow.Active.StartTime;
                float endTime = Timeflow.Active.EndTime;
                if (Timeflow.Active.WorkAreaEnabled) {
                    startTime = Timeflow.Active.WorkAreaStart;
                    endTime = Timeflow.Active.WorkAreaEnd;
                }

                int startFrame = Mathf.RoundToInt(startTime * Timeflow.Active.FPS);
                int endFrame = Mathf.RoundToInt(endTime * Timeflow.Active.FPS);
                int totalFrames = endFrame - startFrame;

                bool canContinue = true;
                if (totalFrames > 500) {
                    canContinue = EditorUtility.DisplayDialog("Large Number of Keyframes: " + totalFrames,
                        "If you proceed with this operation, " + totalFrames + " keyframes will be generated which may result in poor performance or even crash the editor. It is highly recommended to use the Work Area to reduce the amount of time converted, or to reduce the FPS in the Timeflow settings.",
                        "Continue Anyway", "Cancel");
                }

                if (canContinue) {
                    List<TimeflowChannel> newChannels = new List<TimeflowChannel>();

                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior.gameObject, "Bake Keyframes", true);
                        Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(ch.Behavior.gameObject);
                        if (kf != null) {
                            UndoUtil.Undo(kf, "Bake Keyframes", true);

                            TimeflowChannel keyCh = new TimeflowChannel(kf) {
                                ToProperty = new Property(ch.Behavior),
                                IsDataOnly = true,
                                DataType = ch.DataType,
                                Name = "* " + ch.Name,
                                Interpolation = TimeflowChannel.Interpolations.Linear
                            };

                            keyCh.ToProperty.Owner = kf;
                            keyCh.ToProperty.AssignToObject = kf.gameObject;
                            keyCh.ToProperty.Name = keyCh.Name;
                            keyCh.ToProperty.DisplayName = keyCh.Name;
                            keyCh.ToProperty.IsDataOnly = true;
                            if (ch.IsSingleAttribute) {
                                keyCh.ToProperty.PropertyType = Property.PropertyTypes.Float;
                                keyCh.ToProperty.Attribute = -1;
                            }
                            else {
                                keyCh.ToProperty.PropertyType = Property.DataTypeToPropertyType(ch.DataType);
                                keyCh.ToProperty.Attribute = ch.Attribute;
                            }
                            keyCh.ToProperty.GetDataType();
                            keyCh.PropertyType = keyCh.ToProperty.PropertyType;
                            keyCh.Attribute = keyCh.ToProperty.Attribute;

                            for (int frame = startFrame; frame <= endFrame; frame++) {
                                float time = (float)frame / Timeflow.Active.FPS;
                                Timeflow.Active.CurrentTimeExact = time;
                                Keyframe k = keyCh.SetKey(time);
                                if (k != null) {
                                    if (!ch.IsMultichannel || ch.IsSingleAttribute) {
                                        k.KeyValue = ch.InterpolateValue(time, false, false);
                                    }
                                    else
                                    if (ch.IsColor) {
                                        k.KeyColor = ch.InterpolateColor(time, false, false);
                                    }
                                    else
                                    if (ch.IsVector2 && ch.Attribute == -1) {
                                        k.KeyVector2 = ch.InterpolateVector2(time, false, false);
                                    }
                                    else
                                    if (ch.IsVector3 && ch.Attribute == -1) {
                                        k.KeyVector3 = ch.InterpolateVector3(time, false, false);
                                    }
                                    else
                                    if (ch.IsVector && ch.Attribute == -1) {
                                        k.KeyVector = ch.InterpolateVector4(time, false, false);
                                    }
                                    else
                                    if (ch.IsGameObject) {
                                        k.KeyGameObject = ch.InterpolateGameObject(time, false, false);
                                    }
                                    else
                                    if (ch.IsObject) {
                                        k.KeyObject = ch.InterpolateObject(time, false, false);
                                    }
                                    else
                                    if (ch.IsComponent) {
                                        k.KeyComponent = ch.InterpolateComponent(time, false, false);
                                    }
                                    else
                                    if (ch.IsString) {
                                        k.KeyString = ch.InterpolateString(time, false, false);
                                    }
                                }
                            }

                            kf.AddChannel(keyCh);
                            newChannels.Add(keyCh);
                        }
                    }

                    if (newChannels.Count > 0) {
                        Timeflow.Active.View.SelectChannels(newChannels, true);
                        Timeflow.Active.Refresh(true);
                        Timeflow.Active.View.FitGraph(false);
                    }
                }
            }
        }

        public static void ExpandLoopedKeyframes()
        {
            if (Timeflow.Active == null) return;
            if (Timeflow.Active.View.SelectedChannels == null || Timeflow.Active.View.SelectedChannels.Count == 0) {
                Debug.LogWarning("ExpandLoopedKeyframes no channels selected");
                return;
            }
            foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                if (typeof(TimeflowChannel).IsAssignableFrom(ch.GetType()) && ch.SupportsKeyframes && ch.EnableLoop && ch.Keys != null && ch.Keys.Count > 0) {
                    UndoUtil.Undo(ch.Behavior, "Expand Looped Keyframes", true);
                    float timeOffset = ch.TimeOffsetWorld;
                    float loopStart = ch.LoopStart + timeOffset;
                    float loopEnd = ch.LoopEnd + timeOffset;
                    float loopDuration = ch.LoopEnd - ch.LoopStart;

                    float timeMin = Timeflow.Active.WorkAreaEnabled ? Timeflow.Active.WorkAreaStart : Timeflow.Active.View.ScrollTimeMin;
                    float timeMax = Timeflow.Active.WorkAreaEnabled ? Timeflow.Active.WorkAreaEnd : Timeflow.Active.View.ScrollTimeMax;

                    timeMin = Mathf.Max(timeMin, Timeflow.Active.StartTime);
                    timeMax = Mathf.Min(timeMax, Timeflow.Active.EndTime);

                    List<Keyframe> copy = new List<Keyframe>();
                    if (loopDuration > 0 && (ch.EnableLoopIn || ch.EnableLoopOut)) {
                        int loopIndex = ch.EnableLoopIn ? -1 : 1;
                        bool forward = !ch.EnableLoopIn;
                        bool pong = true;
                        bool outofview = false;
                        while (true) {
                            float loopOffset = (float)loopIndex * loopDuration;
                            float keyTime = 0;
                            int keyCount = 0;
                            foreach (Keyframe k in ch.Keys) {
                                keyTime = k.KeyTimeWorld;
                                if (keyTime >= loopStart && keyTime <= loopEnd) {
                                    keyCount++;
                                    if (ch.LoopPingPong && pong) {
                                        keyTime = (loopEnd - (keyTime - loopStart));
                                    }
                                    keyTime += loopOffset;
                                    if (keyTime >= timeMin && keyTime <= timeMax) {
                                        //Debug.Log($"CLONE:keyTime:{keyTime} timeMin:{timeMin} timeMax:{timeMax}");
                                        Keyframe c = Keyframe.Clone(k, null);
                                        c.CopiedFromChannel = k.Channel;
                                        c.KeyTime = keyTime;
                                        copy.Add(c);
                                    }
                                    else {
                                        if (forward || !ch.EnableLoopOut) {
                                            /// Outside of view - end operation
                                            outofview = true;
                                        }
                                        else {
                                            /// Switch to loop out keys
                                            forward = true;
                                            loopIndex = 0;
                                        }
                                        break;
                                    }
                                }
                            }
                            pong = !pong;
                            if (forward) {
                                loopIndex++;
                            }
                            else {
                                loopIndex--;
                            }
                            if (outofview || keyCount == 0) break;
                            if (ch.LoopLimit != 0 && loopIndex > ch.LoopLimit) break;
                            //if (loopIndex > 1000) {
                            //    //Debug.LogWarning("Too many keyframes displayed");
                            //    break; // failsafe to prevent crash in case loop doesn't end for some reason
                            //}
                        }
                    }

                    if (TimeflowPreferences.Current.ExpandLoopedKeyframesOverwrite) {
                        /// Create a list of keyframes to remove. Any keys outside of the original loop
                        /// region and inside the affected time range are removed.
                        List<Keyframe> remove = new List<Keyframe>();
                        foreach (Keyframe k in ch.Keys) {
                            float kt = k.KeyTimeWorld;
                            if (kt >= timeMin && kt <= timeMax) {
                                if (ch.EnableLoopIn && kt < loopStart) {
                                    remove.Add(k);
                                }
                                else
                                if (ch.EnableLoopOut && kt > loopEnd) {
                                    remove.Add(k);
                                }
                            }
                        }
                        if (remove.Count > 0) {
                            Debug.Log(remove.Count + " keyframes outside of the loop region were deleted");//--KEEP
                            foreach (Keyframe k in remove) {
                                ch.UnsetKey(k);
                            }
                        }
                    }

                    if (copy.Count == 0) {
                        Debug.LogWarning("No keyframes were modified. Check that the looped region of the channel contains keyframes.");
                    }
                    else {
                        foreach (Keyframe k in copy) {
                            if (ch.GetKeyAtTime(k.KeyTime) != null) {
                                /// Since looping keyframes can result in keys on the same frame, nudge
                                /// the time forward or backward to prevent overwriting.
                                if (k.KeyTime <= loopStart) {
                                    k.KeyTime -= Timeflow.Active.FrameDuration;
                                }
                                else {
                                    k.KeyTime += Timeflow.Active.FrameDuration;
                                }
                            }
                            ch.CopyKey(k, -ch.TimeOffsetWorld, true, true);
                        }
                        ch.EnableLoop = false;
                        Debug.Log("Loop expanded " + copy.Count + " keyframes");//--KEEP

                        Timeflow.Active.View.SelectKeysInChannel(ch);
                    }
                }
            }
        }

        public static void FreezeTimeOffset()
        {
            List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
            if (channels == null) {
                channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
            }
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    if (ch.TimeOffset != 0f) {
                        UndoUtil.Undo(ch.Behavior, "Freeze Time Offset", true);
                        if (ch.Keys != null && ch.Keys.Count > 0) {
                            foreach (Keyframe k in ch.Keys) {
                                k.KeyTime += ch.TimeOffset;
                                if (k.IsTrack) {
                                    k.KeyValue += ch.TimeOffset;
                                }
                            }
                        }
                        ch.TimeOffset = 0f;
                    }
                }
            }
        }

        public static void RenameChannel()
        {
            if (TimeflowContext.Channel != null) {
                TimeflowContext.Channel.IsEditingName = true;
            }
        }

        public static void ToggleAlwaysUpdate()
        {
            if (TimeflowContext.Channel != null) {
                UndoUtil.Undo(TimeflowContext.Channel.Behavior, "Toggle Always Update");
                TimeflowContext.Channel.AlwaysUpdate = !TimeflowContext.Channel.AlwaysUpdate;

                List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
                if (channels == null) {
                    channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
                }
                if (channels != null) {
                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior, "Toggle Always Update");
                        ch.AlwaysUpdate = TimeflowContext.Channel.AlwaysUpdate;
                    }
                }
            }
        }

        public static void ToggleAlwaysShowValues()
        {
            if (TimeflowContext.Channel != null) {
                UndoUtil.Undo(TimeflowContext.Channel.Behavior, "Toggle Always Show Values");
                TimeflowContext.Channel.AlwaysShowValues = !TimeflowContext.Channel.AlwaysShowValues;

                List<TimeflowChannel> channels = Timeflow.Active.View.SelectedChannels;
                if (channels == null) {
                    channels = Timeflow.Active.View.Display.GetChannelsDisplayed();
                }
                if (channels != null) {
                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior, "Toggle Always Show Values");
                        ch.AlwaysShowValues = TimeflowContext.Channel.AlwaysShowValues;
                    }
                }
            }
        }

        public static void RemoveChannel()
        {
            if (TimeflowContext.Channel != null) {
                int option = EditorUtility.DisplayDialogComplex("Remove Channel", "Are you sure you want to remove this channel and its keyframes?", "Ok", "Cancel", "");
                if (option == 0) {
                    UndoUtil.Undo(TimeflowContext.Channel.Behavior, "Remove Channel");
                    TimeflowContext.Channel.Behavior.RemoveChannelWithUndo(TimeflowContext.Channel);
                    TimeflowContext.Channel = null;
                }
            }
        }

        public static void SelectChannelKeyframes()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Select Keyframes");
                    Timeflow.Active.View.SelectKeysInChannel(ch);
                }
            }
        }

        public static void ClearChannelKeyframes()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                int option = EditorUtility.DisplayDialogComplex("Clear Keyframes", "Are you sure you want to remove all keyframes on the selected channels?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowChannel ch in channels) {
                        Debug.Log("Clearing keyframes on channel " + ch.Name);//--KEEP
                        UndoUtil.Undo(ch.Behavior, "Clear Keyframes");
                        ch.ClearKeys(true);
                    }
                }
            }
        }

        public static void CombineChannels()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels == null) return;
            int option = EditorUtility.DisplayDialogComplex("Combine Channels?", "Are you sure you want to merge the keyframes of the selected channels into one? " +
                "This merges separate channels for each axis or attribute (X, Y, Z) into a single combined channel (XYZ).\n\n" +
                "Select only the channels you wish to merge.", "Ok", "Cancel", "");
            if (option == 0) {
                TimeflowChannel.CombineChannels(channels.ToArray());
            }
        }

        public static void SeparateChannels()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels == null) return;
            int option = EditorUtility.DisplayDialogComplex("Separate Into Multiple Channels?", "Are you sure you want to split the attributes of the " +
                "selected channels? This creates separate channels for each axis or attribute (ex. X, Y, Z).", "Ok", "Cancel", "");
            if (option == 0) {
                int anySkipped = 0;
                foreach (TimeflowChannel ch in channels) {
                    if (ch.SupportsKeyframes && !ch.IsTrack) {
                        if (ch.CanSeparateOrCombineChannel()) {
                            ch.SeparateChannel();
                        }
                        else {
                            anySkipped++;
                        }
                    }
                }
                if (anySkipped > 0) {
                    EditorUtility.DisplayDialog("Not All Channels Separated", anySkipped + " of the " + channels.Count + " selected channels were skipped since they do not support separating channels. You can work around this by adding each channel manually.", "Ok");
                }
            }
        }

        public static void RemoveAllProperties()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                int option = EditorUtility.DisplayDialogComplex("Remove All Animated Properties", "Are you sure you want to remove all of the selected keyframe channels?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowObject obj in objects) {
                        if (obj.Keyframer != null) {
                            UndoUtil.Undo(obj.Keyframer, "Remove All Animated Properties");
                            obj.Keyframer.DeleteAllChannels();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a new linked data channel between existing links on selected channels.
        /// </summary>
        public static void InsertDataChannelLink()
        {
            if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                    if (ch.Link != null && ch.Link.Channel != null) {
                        Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(ch.Behavior.gameObject);
                        if (kf != null) {
                            UndoUtil.Undo(kf, "Insert Channel Link", true);
                            TimeflowChannel data = new TimeflowChannel(kf) {
                                ToProperty = new Property()
                            };
                            data.ToProperty.IsDataOnly = true;
                            data.ToProperty.DataType = ch.DataType;
                            data.ToProperty.Owner = kf;
                            data.ToProperty.AssignToObject = kf.gameObject;
                            data.PropertyType = ch.PropertyType;
                            data.Attribute = ch.Attribute;
                            data.Name = data.ToProperty.Name = ch.Name + " Data";
                            data.Link = new TimeflowChannelLink(data, ch.Link.Channel);
                            //data.Link.Copy(ch.Link);
                            data.SortOrder = ch.SortOrder - 1;
                            kf.AddChannel(data);

                            ch.Link.Channel = data;
                            ch.Link.Provider = kf.ParentObject;
                        }
                    }
                }
            }
        }

        public static void RevealLinkedObjectsForSelectedChannels()
        {
            if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                // Make a copy of the selected list to avoid modifying it while iterating
                List<TimeflowChannel> list = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                    list.Add(ch);
                }

                foreach (TimeflowChannel ch in list) {
                    RevealLinkedObjects(ch);
                }
            }
            else {
                RevealLinkedObjects(TimeflowContext.Channel);
            }
        }

        public static void RevealLinkedObjects(TimeflowChannel channel)
        {
            if (channel == null) return;
            List<GameObject> objects = new List<GameObject>();
            if (channel.Link != null && channel.Link.Channel != null && channel.Link.Channel.Object != null) {
                channel.Link.Channel.Object.DisplayChannels = true;
                channel.Link.Channel.DisplayChannel = true;
                objects.Add(channel.Link.Channel.Object.gameObject);
                Timeflow.Active.View.SelectChannel(channel.Link.Channel, false);
            }
            if (channel.LinkedFrom != null && channel.LinkedFrom.Count > 0) {
                foreach (TimeflowChannel ch in channel.LinkedFrom) {
                    if (ch != null && ch.Object != null) {
                        ch.DisplayChannel = true;
                        ch.Object.DisplayChannels = true;
                        Timeflow.Active.View.SelectChannel(ch, false);
                        if (!objects.Contains(ch.Object.gameObject)) {
                            objects.Add(ch.Object.gameObject);
                        }
                    }
                }
            }
            if (objects.Count > 0) {
                foreach (GameObject obj in objects) {
                    Timeflow.Active.View.Display.AddObjectToDisplay(obj);
                }
            }
        }

        #endregion

        #region CHANNEL

        public static void ToggleIndividualChannelLink(object obj)
        {
            TimeflowChannel channel = (TimeflowChannel)obj;
            if (channel != null && channel.Link != null) {
                channel.Link.Enabled = !channel.Link.Enabled;
            }
        }

        public static void RemoveChannelLink()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                int option = EditorUtility.DisplayDialogComplex("Remove Link from Selected Channels?", "Are you sure you want to remove all links on the selected channels?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior, "Remove Channel Links");
                        ch.RemoveLink();
                    }
                }
            }
        }

        public static void RemoveLinkedChannels()
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                int option = EditorUtility.DisplayDialogComplex("Remove Links to the Selected Channels?", "Are you sure you want to remove all links referring to the selected channels?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowChannel ch in channels) {
                        UndoUtil.Undo(ch.Behavior, "Remove Channel Links");
                        ch.RemoveLinkedFrom();
                    }
                }
            }
        }

        public static void ToggleChannelLink()
        {
            TimeflowContext.Channel.Link.Enabled = !TimeflowContext.Channel.Link.Enabled;
            SetChannelLinkEnabled(TimeflowContext.Channel.Link.Enabled);
        }

        public static void SetChannelLinkEnabled(bool enabled)
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Enable Channel Link");
                    if (ch.Link != null) {
                        ch.Link.Enabled = enabled;
                    }
                }
            }
        }

        public static void SetChannelLinkModeOverwrite()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Overwrite);
        }

        public static void SetChannelLinkModeAdd()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Add);
        }

        public static void SetChannelLinkModeSubtract()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Subtract);
        }

        public static void SetChannelLinkModeMultiply()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Multiply);
        }

        public static void SetChannelLinkModeMax()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Max);
        }

        public static void SetChannelLinkModeMin()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Min);
        }

        public static void SetChannelLinkModeOneMinus()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.OneMinus);
        }

        public static void SetChannelLinkModeCustom()
        {
            SetChannelLinkMode(TimeflowChannelLink.Modes.Custom);
        }

        public static void SetChannelLinkMode(TimeflowChannelLink.Modes mode)
        {
            List<TimeflowChannel> channels = TimeflowContext.GetChannels();
            if (channels != null) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Set Channel Link Mode: " + mode);
                    ch.Link.Mode = mode;
                }
            }
        }

        #endregion

        #region WORK AREA

        public static void ToggleWorkArea()
        {
            Timeflow.Active.WorkAreaEnabled = !Timeflow.Active.WorkAreaEnabled;
            //Timeflow.Active.EnableWorkAreaLoop = true;
        }

        public static void ToggleWorkAreaLeadIn()
        {
            Timeflow.Active.WorkAreaAllowsLeadIn = !Timeflow.Active.WorkAreaAllowsLeadIn;
        }

        public static void ToggleWorkAreaToggleLock()
        {
            Timeflow.Active.WorkAreaLocked = !Timeflow.Active.WorkAreaLocked;
            //Timeflow.Active.EnableWorkAreaLoop = true;
        }

        public static void WorkAreaWithSelected()
        {
            Timeflow.Active.View.SetWorkAreaWithSelected();
        }

        public static void WorkAreaWithMarker()
        {
            Timeflow.Active.View.SetWorkAreaWithSelectedMarker();
        }

        public static void SetWorkAreaToScrollRegion()
        {
            if (Timeflow.Active != null) {
                Timeflow.Active.WorkAreaEnabled = true;
                //Timeflow.Active.EnableWorkAreaLoop = true;
                Timeflow.Active.SetWorkArea(Timeflow.Active.View.ScrollTimeMin, Timeflow.Active.View.ScrollTimeMax, true);
            }
        }

        public static void InsertTimeInWorkArea()
        {
            Timeflow.Active.View.InsertTimeInWorkArea();
        }

        public static void DuplicateTimeInWorkArea()
        {
            Timeflow.Active.View.DuplicateTimeInWorkArea();
        }

        public static void ClearTimeInWorkArea()
        {
            Timeflow.Active.View.ClearTimeInWorkArea();
        }

        public static void DeleteTimeInWorkArea()
        {
            Timeflow.Active.View.DeleteTimeInWorkArea();
        }

        public static void InsertTimeInWorkAreaGlobal()
        {
            Timeflow.Active.View.InsertTimeInWorkAreaGlobal();
        }

        public static void DuplicateTimeInWorkAreaGlobal()
        {
            Timeflow.Active.View.DuplicateTimeInWorkAreaGlobal();
        }

        public static void ClearTimeInWorkAreaGlobal()
        {
            Timeflow.Active.View.ClearTimeInWorkAreaGlobal();
        }

        public static void DeleteTimeInWorkAreaGlobal()
        {
            Timeflow.Active.View.DeleteTimeInWorkAreaGlobal();
        }

        public static void RevealAllTracksInWorkAreaGlobal()
        {
            Timeflow.Active.View.RevealAllInWorkAreaGlobal(false);
        }

        public static void RevealAllKeyframesInWorkAreaGlobal()
        {
            Timeflow.Active.View.RevealAllInWorkAreaGlobal(true);
        }

        public static void ClearTracksInWorkArea()
        {
            Timeflow.Active.View.ClearTimeInWorkArea(TimeflowView.SelectionModes.TracksOnly);
        }

        public static void ClearKeyframesInWorkArea()
        {
            Timeflow.Active.View.ClearTimeInWorkArea(TimeflowView.SelectionModes.KeyframesOnly);
        }

        #endregion

        #region KEYFRAMES

        public static void SelectAllKeys()
        {
            Timeflow.Active.View.SelectAllKeys(false, TimeflowView.SelectionModes.Any);
        }

        public static void SelectAllKeysOnly()
        {
            Timeflow.Active.View.SelectAllKeys(false, TimeflowView.SelectionModes.KeyframesOnly);
        }

        public static void SelectAllTracksOnly()
        {
            Timeflow.Active.View.SelectAllKeys(false, TimeflowView.SelectionModes.TracksOnly);
        }

        public static void SelectWorkArea()
        {
            Timeflow.Active.View.SelectAllKeys(true, TimeflowView.SelectionModes.Any);
        }

        public static void SelectWorkAreaKeysOnly()
        {
            Timeflow.Active.View.SelectAllKeys(true, TimeflowView.SelectionModes.KeyframesOnly);
        }

        public static void SelectWorkAreaTracksOnly()
        {
            Timeflow.Active.View.SelectAllKeys(true, TimeflowView.SelectionModes.TracksOnly);
        }

        public static void SelectAllAtCurrentTime()
        {
            Timeflow.Active.View.SelectKeysAtCurrentTime(TimeflowView.SelectionModes.Any);
        }

        public static void SelectKeysAtCurrentTime()
        {
            Timeflow.Active.View.SelectKeysAtCurrentTime(TimeflowView.SelectionModes.KeyframesOnly);
        }

        public static void SelectTracksAtCurrentTime()
        {
            Timeflow.Active.View.SelectKeysAtCurrentTime(TimeflowView.SelectionModes.TracksOnly);
        }

        public static void SelectKeysByValue()
        {
            Timeflow.Active.View.SelectKeysByValue();
        }

        public static void SelectTracksByColor()
        {
            Timeflow.Active.View.SelectTracksByColor();
        }

        #endregion

        #region SELECTED TRACKS

        public static void SplitTracksAtCurrentTime()
        {
            Timeflow.Active.View.SplitSelectedTracksAtTime(Timeflow.Active.CurrentTime);
        }

        public static void SetTracksToWorkArea()
        {
            Timeflow.Active.View.SetSelectedTracksToWorkArea(true);
        }

        public static void SplitAllTracksWithWorkArea()
        {
            Timeflow.Active.View.SplitSelectedTracksByWorkArea();
        }

        public static void SelectRelatedKeys()
        {
            Timeflow.Active.View.SelectRelatedKeys();
        }

        public static void TracksGotoStart()
        {
            TimeflowCommands.GotoStartOfSelection();
        }

        public static void TracksGotoEnd()
        {
            TimeflowCommands.GotoEndOfSelection();
        }

        public static void TracksSetStart()
        {
            TimeflowCommands.SetStartOfSelection();
        }

        public static void TracksSetEnd()
        {
            TimeflowCommands.SetEndOfSelection();
        }

        public static void JoinSelectedTracks()
        {
            Timeflow.Active.View.JoinSelectedTracks();
        }

        public static void CanDragTimeOffset()
        {
            bool canDrag = !TimeflowContext.HasDraggableTracks;
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    if (obj == null) {
                        Debug.LogWarning("Null object in TimeflowContext list");
                    }
                    else {
                        UndoUtil.Undo(obj, "Can Drag Time Offset");
                        obj.CanDragTimeOffset = canDrag;
                        if (canDrag && obj.Track != null) {
                            /// Turn off auto length because it is an odd default behavior, though still
                            /// allowed if a user wants to turn auto back on. When both are enabled, the
                            /// track doesn't move when dragged but still slides the time offset.
                            obj.Track.AutoFullLength = false;
                        }
                    }
                }
            }
        }

        public static void ToggleLoopTimeOffset()
        {
            bool canLoop = !TimeflowContext.HasLoopTimeOffset;
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    if (obj.TryGetComponent<LoopTimeOffset>(out var lto)) {
                        UndoUtil.Undo(lto, "Toggle Loop Time Offset");
                        lto.Enabled = !lto.Enabled;
                    }
                    else {
                        obj.gameObject.AddComponent<LoopTimeOffset>();
                    }
                }
            }
        }

        public static void ResetSelectedTracks()
        {
            Timeflow.Active.View.ResetSelectedTracks();
        }

        public static void ResetObjectTracks()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Reset Track");
                    obj.ResetTrack();
                }
            }
        }

        public static void SelectTrack()
        {
            Timeflow.Active.View.SetupSelectedKeys(true);
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    if (obj.BehaviorsEnabled) {
                        foreach (Keyframe key in obj.Track.Keys) {
                            Timeflow.Active.View.SelectKey(key);
                        }
                    }
                }
            }
        }

        public static void ResetTrackColor()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Reset Colors");
                    foreach (Keyframe k in obj.Track.Keys) {
                        k.KeyColor = obj.Track.GUIColor;
                    }
                }
            }
        }

        #endregion

        #region SELECTED KEYS

        public static void SetKeyInterpolationAuto()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Auto, true);
        }

        public static void SetKeyInterpolationLinear()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Linear);
        }

        public static void SetKeyInterpolationLinearLeft()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.LinearLeft);
        }

        public static void SetKeyInterpolationLinearRight()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.LinearRight);
        }

        public static void SetKeyInterpolationHold()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Hold);
        }

        public static void SetKeyInterpolationFlat()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.Flat);
        }

        public static void SetKeyInterpolationFlatLeft()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.FlatLeft);
        }

        public static void SetKeyInterpolationFlatRight()
        {
            Timeflow.Active.View.SetInterpolationOfSelectedKeyframes(Keyframe.Interpolations.FlatRight);
        }

        public static void CopyKeys()
        {
            Timeflow.Active.View.CopyKeyframes();
        }

        public static void CutKeys()
        {
            Timeflow.Active.View.CutKeyframes();
        }

        public static void PasteKeys()
        {
            Timeflow.Active.View.PasteKeys(true);
        }

        public static void PasteKeyTangents()
        {
            Timeflow.Active.View.PasteKeyTangents();
        }

        public static void ShowKeyTangents()
        {
            Timeflow.Active.View.ShowKeyTangents(true);
        }

        public static void HideKeyTangents()
        {
            Timeflow.Active.View.ShowKeyTangents(false);
        }

        public static void PasteKeysInTime()
        {
            Timeflow.Active.View.PasteKeys(false);
        }

        public static void DeleteKeys()
        {
            int option = EditorUtility.DisplayDialogComplex("Delete Selected Keys", "Are you sure you want to delete the selected keyframes?", "Ok", "Cancel", "");
            if (option == 0) {
                Timeflow.Active.View.DeleteSelectedKeys();
            }
        }

        public static void EnableKeys()
        {
            Timeflow.Active.View.EnableSelectedKeys(true);
        }

        public static void DisableKeys()
        {
            Timeflow.Active.View.EnableSelectedKeys(false);
        }

        public static void LoopSelectedKeys()
        {
            Timeflow.Active.View.LoopSelectedChannels(true);
        }

        public static void LoopSelectedChannels()
        {
            Timeflow.Active.View.LoopSelectedChannels(false);
        }

        public static void UnloopSelectedChannels()
        {
            Timeflow.Active.View.UnloopSelectedChannels();
        }

        public static void FitSelected()
        {
            Timeflow.Active.View.FitTime(true);
        }

        public static void SetTimeScope()
        {
            Timeflow.Active.SetTimeScope();
        }

        public static void ToggleTimeScope()
        {
            Timeflow.Active.ToggleLocalTimeScope();
        }

        public static void SelectAllKeysDisplayed()
        {
            Timeflow.Active.View.SelectAllKeys();
        }

        public static void DeleteKeysInSelectedChannels()
        {
            Timeflow.Active.View.DeleteKeysInSelectedChannels();
        }

        public static void SnapTimeOfSelected()
        {
            Timeflow.Active.View.SnapTimeOfSelectedKeyframes();
        }

        public static void SnapValuesOfSelected()
        {
            Timeflow.Active.View.SnapValuesOfSelectedKeyframes();
        }

        public static void MirrorTimeOfSelected()
        {
            Timeflow.Active.View.ModifyTimeOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Mirror);
        }

        public static void MirrorValuesOfSelected()
        {
            Timeflow.Active.View.ModifyValuesOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Mirror);
        }

        public static void RandomizeTimeOfSelected()
        {
            Timeflow.Active.View.ModifyValuesOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Randomize);
        }

        public static void RandomizeValuesOfSelected()
        {
            Timeflow.Active.View.ModifyValuesOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Randomize);
        }

        public static void FitLoop()
        {
            Timeflow.Active.View.FitTime(true);
        }

        public static void FitAll()
        {
            Timeflow.Active.View.FitTime(false, true);
        }

        #endregion

        #region TIMEBAR

        public static void AddTimeMarker()
        {
            if (TimeflowWindow.Instance != null) {
                //Timeflow.Active.AddMarkerAtPosition(MenuPosition.x);
                Timeflow.Active.Markers.AddMarker(Timeflow.Active.CurrentTime);
            }
        }

        public static void RemoveTimeMarker()
        {
            UndoUtil.Undo(Timeflow.Active, "Remove Time Marker");
            Timeflow.Active.Markers.DeleteMarker(Timeflow.Active.View.Markers.SelectedMarker);
        }

        public static void SelectKeysRelatedToMarker()
        {
            UndoUtil.Undo(Timeflow.Active, "Select Keys Related to Marker");
            Timeflow.Active.View.Markers.GetKeysRelatedToMarker();
            Timeflow.Active.View.SelectRelatedKeys();
        }

        #endregion

    }

}//AxonGenesis

#endif

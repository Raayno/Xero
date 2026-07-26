// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a static container to hold data for the Timeflow context menu for access across assemblies.
    /// </summary>
    public static class TimeflowContext
    {
        public static GenericMenu Menu;
        public static Vector2 MenuPosition = Vector2.zero;
        public static SDictionary<string, SDictionary<string, Type>> Materials;
        public static bool AnyObjectsSelected;
        public static bool AnyChildrenHidden;

        private static TimeflowObject _Obj;
        private static TimeflowChannel _Channel = null;

        public static TimeflowObject Obj {
            get { return _Obj; }
            set {
                _Obj = value;
            }
        }

        public static TimeflowChannel Channel {
            get { return _Channel; }
            set {
                if (_Channel != value) {
                    _Channel = value;
                    //Debug.Log($"TimeflowContext.Channel: {_Channel}");
                }
            }
        }

        public enum DisplayModes
        {
            General,
            Object,
            Channel,
            Keys,
            Timebar
        }
        public static DisplayModes DisplayMode = DisplayModes.General;

        public static AxonGenesisBehavior Owner {
            get {
                if (Channel != null) return Channel.Behavior;
                return null;
            }
        }

        public static string AltKey {
            get {
                if (Application.platform == RuntimePlatform.OSXEditor) {
                    return "Option";
                }
                else {
                    return "Alt";
                }
            }
        }

        public static string ControlKey {
            get {
                if (Application.platform == RuntimePlatform.OSXEditor) {
                    return "Command";
                }
                else {
                    return "Control";
                }
            }
        }

        public static void Init(TimeflowObject obj, DisplayModes display)
        {
            TimeflowContext.Menu = new GenericMenu();
            MenuPosition = Event.current.mousePosition;
            DisplayMode = display;

            Obj = obj;
            if (Obj != null) {
                Materials = PropertiesOfMaterial.GetMaterialProperties(Obj.GetComponent<Renderer>());
            }
            else {
                Materials = null;
            }

            AnyObjectsSelected = false;
            AnyChildrenHidden = false;
            if (obj == null || obj.IsSelected) {
                foreach (TimeflowObject o in Timeflow.Active.View.Display.Objects) {
                    if (o.IsSelected && o.IsDisplayed) {
                        AnyObjectsSelected = true;
                        AnyChildrenHidden = !o.ShowChildren;
                        if (AnyChildrenHidden) break;
                    }
                }
            }
            else {
                AnyChildrenHidden = !obj.ShowChildren;
            }
            if (!AnyObjectsSelected && Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                AnyObjectsSelected = true;
            }

        }

        /// <summary>
        /// Returns a list of objects selected for the current operation. If the user right clicks on an
        /// object that is not selected, then only that object will be processed. To process a selection of
        /// objects, the user must right click on one of the selected objects.
        /// </summary>
        public static List<TimeflowObject> GetObjects()
        {
            List<TimeflowObject> objects = null;

            if (TimeflowContext.Obj != null) {
                objects = new List<TimeflowObject> {
                    TimeflowContext.Obj
                };
                if (TimeflowContext.Obj.IsSelected) {
                    // Work with all selected objects
                    foreach (TimeflowObject obj in Timeflow.Active.View.Display.Objects) {
                        if (obj != null && obj.IsSelected && obj.IsDisplayed) {
                            if (!objects.Contains(obj)) objects.Add(obj);
                        }
                    }
                }
                else {
                    objects = new List<TimeflowObject>();
                }
            }

            return objects;
        }

        /// <summary>
        /// Check for any selected keyframes.
        /// </summary>
        public static bool HasKeys {
            get {
                return Timeflow.Active != null && Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Count > 0;
            }
        }

        /// <summary>
        /// Check for any selected channels.
        /// </summary>
        public static bool IsLoopExpandable {
            get {
                bool isLoopExpandable = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (typeof(TimeflowChannel).IsAssignableFrom(ch.GetType()) && ch.SupportsKeyframes && ch.EnableLoop) {
                            isLoopExpandable = true;
                            break;
                        }
                    }
                }
                return isLoopExpandable;
            }
        }

        /// <summary>
        /// Check for any selected channels.
        /// </summary>
        public static bool HasMarker {
            get {
                return Timeflow.Active != null && Timeflow.Active.View.Markers.SelectedMarker != null;
            }
        }

        /// <summary>
        /// Check for any selected channels.
        /// </summary>
        public static bool HasTracks {
            get {
                return Timeflow.Active != null && Timeflow.Active.View.AnyTracksSelected;
            }
        }

        public static int TracksCount {
            get {
                return Timeflow.Active == null ? 0 : Timeflow.Active.View.TracksSelectedCount;
            }
        }

        /// <summary>
        /// Check for any selected channels.
        /// </summary>
        public static bool HasChannels {
            get {
                return Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0;
            }
        }

        public static int ChannelsCount {
            get {
                return Timeflow.Active == null ? 0 : Timeflow.Active.View.SelectedChannels == null ? 0 : Timeflow.Active.View.SelectedChannels.Count;
            }
        }

        /// <summary>
        /// Check for any selected channels that are tracks
        /// </summary>
        public static bool HasTrackChannels {
            get {
                bool anyTracks = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch.IsTrack) {
                            anyTracks = true;
                            break;
                        }
                    }
                }
                return anyTracks;
            }
        }

        public static bool HasUnlockedChannelHeights {
            get {
                bool anyUnlocked = false;
                if (Timeflow.Active != null) {
                    if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                        foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                            if (ch == null) continue;
                            if (!ch.GUIHeightLocked) {
                                anyUnlocked = true;
                                break;
                            }
                        }
                    }
                    if (Timeflow.Active.View.SelectedObjects != null && Timeflow.Active.View.SelectedObjects.Count > 0) {
                        foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                            if (obj == null || obj.Track == null) continue;
                            if (!obj.Track.GUIHeightLocked) {
                                anyUnlocked = true;
                                break;
                            }
                        }
                    }
                }
                return anyUnlocked;
            }
        }

        public static bool HasLockedChannelHeights {
            get {
                bool anyLocked = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch.GUIHeightLocked) {
                            anyLocked = true;
                            break;
                        }
                    }
                }
                return anyLocked;
            }
        }

        /// <summary>
        /// Check for any selected channels that have a TimeOffset value other than 0
        /// </summary>
        public static bool HasTimeOffsets {
            get {
                bool anyOffsets = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch.TimeOffset != 0f) {
                            anyOffsets = true;
                            break;
                        }
                    }
                }
                return anyOffsets;
            }
        }

        /// <summary>
        /// Checks if the selected channels can be split
        /// </summary>
        public static bool CanSeparate {
            get {
                bool canSeparate = true;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (!ch.IsTrack && !ch.CanSeparateOrCombineChannel()) {
                            canSeparate = false;
                            break;
                        }
                    }
                }
                else {
                    canSeparate = false;
                }
                return canSeparate;
            }
        }

        /// <summary>
        /// Checks if the selected channels are combined values
        /// </summary>
        public static bool IsCombined {
            get {
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (!ch.IsTrack && ch.AttributeCount > 1 && ch.Attribute < 0) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public static bool IsMultichannel {
            get {
                bool anyMultichannel = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (!ch.IsTrack && ch.IsMultichannel) {
                            anyMultichannel = true;
                            break;
                        }
                    }
                }
                return anyMultichannel;
            }
        }

        public static bool HasDraggableTracks {
            get {
                bool anyDraggable = false;
                List<TimeflowObject> objects = GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        if (obj.CanDragTimeOffset && !obj.IsLocked && !obj.Track.IsLocked) {
                            anyDraggable = true;
                            break;
                        }
                    }
                }
                return anyDraggable;
            }
        }

        public static bool HasLoopTimeOffset {
            get {
                bool any = false;
                List<TimeflowObject> objects = GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        if (obj.TryGetComponent<LoopTimeOffset>(out var timeoffset)) {
                            if (timeoffset.Enabled) {
                                any = true;
                                break;
                            }
                        }
                    }
                }
                return any;
            }
        }

        public static bool HasNoneTrackChannels {
            get {
                bool anyNoneTracks = false;
                if (Timeflow.Active != null && Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (!ch.IsTrack) {
                            anyNoneTracks = true;
                            break;
                        }
                    }
                }
                return anyNoneTracks;
            }
        }

        /// <summary>
        /// In similar fashion to objects, channels can also be operated on individually or on all
        /// selected.
        /// </summary>
        /// <returns></returns>
        public static List<TimeflowChannel> GetChannels()
        {
            List<TimeflowChannel> channels = null;

            if (Timeflow.Active.View.SelectedChannels != null) {
                channels = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                    channels.Add(ch);
                }
            }
            else
            if (TimeflowContext.Channel != null) {
                channels = new List<TimeflowChannel>();
                channels.Add(TimeflowContext.Channel);
            }

            return channels;
        }

        private static void AddMenuItemType<T>() where T : Component
        {
            AddMenuItemType(typeof(T));
        }

        private static void AddMenuItemType(Type t)
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;

            MethodInfo m = t.GetMethod("AddMenuItem");
            if (m != null) {
                m.Invoke(null, null);
            }
            else
            if (inHierarchy) {
                m = t.GetMethod("AddMenuItemName");
                if (m != null) {
                    Menu.AddItem(new GUIContent((string)m.Invoke(null, null)), false, AddBehaviorByType, new TimeflowContextMenuBehaviorType(t));
                }
            }
        }

        public static void AddMenuItem(Type behaviorType, string menuPath)
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;
            TimeflowContext.Menu.AddItem(new GUIContent(menuPath), false, _AddMenuItem, behaviorType);
        }

        private static void _AddMenuItem(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;
                    Type behaviorType = (Type)info;
                    Undo.AddComponent(obj.gameObject, behaviorType);
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public static void BehaviorMenus()
        {
            if (Obj == null) return;
            bool isObject = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;

            Type[] behaviors = AppDomain.CurrentDomain.GetTypesWithInterface(typeof(ITimeflowBehaviorMenu));
            Array.Sort(behaviors, delegate (Type a, Type b) {
                return a.ToString().CompareTo(b.ToString());
            });

            // ANIMATION
            Keyframer.AddMenuItem();
            Blend.AddMenuItem();
            AnimationClips.AddMenuItem();
            AnimationSequencer.AddMenuItem();
            MotionPath.AddMenuItem();
            Tween.AddMenuItem();

            // AUTOMATION
            AddMenuItemType<AutoBank>();
            AddMenuItemType<AutoRotate>();
            AddMenuItemType<Distance>();
            AddMenuItemType<Flyby>();
            AddMenuItemType<Follow>();
            AddMenuItemType<LookAt>();
            AddMenuItemType<LookAtTarget>();
            AddMenuItemType<Noise>();
            AddMenuItemType<PlaceOnPath>();
            AddMenuItemType<PlaceOnSurface>();

            // AUDIO
            AddMenuItemType<AudioSpectrum>();
            AddMenuItemType<AudioSample>();
            AddMenuItemType<AudioTrack>();
            AudioReactive.AddMenuItem();

            // MIDI
            AddMenuItemType<MidiFile>();
            AddMenuItemType<MidiReceiver>();
            AddMenuItemType<MidiTween>();

            // EVENTS
            AddMenuItemType<TimeflowEvent>();

            AddMenuItem(typeof(PropertyLink), "Add Tool/Property Link");
            AddMenuItem(typeof(Comment), "Add Tool/Comment");

            foreach (Type t in behaviors) {
                AddMenuItemType(t);
            }
        }

        public static void AddBehaviorByType(object iobj)
        {
            if (Obj != null) {
                TimeflowContextMenuBehaviorType info = (TimeflowContextMenuBehaviorType)iobj;
                if (typeof(TimeflowObject).IsAssignableFrom(info.Type)) {
                    TimeflowObject obj;
                    if (Obj.TryGetComponent<TimeflowObject>(out obj)) {
                        UndoUtil.UndoDestroy(obj);
                    }
                }
                Component comp = Undo.AddComponent(Obj.gameObject, info.Type);
                if (typeof(TimeflowBehavior).IsAssignableFrom(comp.GetType())) {
                    TimeflowBehavior behavior = (TimeflowBehavior)comp;
                    if (behavior != null) {
                        behavior.OnNewInstance();
                    }
                }
            }
        }

    }

    #region MENU OBJECT TYPES

    public class TimeflowContextMenuObject
    {
        public TimeflowObject Obj;

        public TimeflowContextMenuObject(TimeflowObject obj)
        {
            Obj = obj;
        }
    }

    public class TimeflowContextMenuProperty
    {
        public TimeflowObject Obj = null;
        public Property ToProperty;
        public int Attribute;

        public TimeflowContextMenuProperty() { }
        public TimeflowContextMenuProperty(TimeflowContextMenuProperty copy)
        {
            ToProperty = copy.ToProperty;
            Attribute = copy.Attribute;
        }
    }

    public class TimeflowContextMenuBehaviorType
    {
        public Type Type;
        public TimeflowContextMenuBehaviorType() { }
        public TimeflowContextMenuBehaviorType(Type t) { Type = t; }
    }

    public class TimeflowContextMenuTrackColor
    {
        public Color Color = Color.white;
        public bool Random = false;
        public bool Sequential = false;
    }

    public class TimeflowContextMenuEventType
    {
        public Type EventType = null;
        public bool InHierarchy = false;
    }

    public class TimeflowContextMenuBlendType
    {
        public Type BlendType = null;
        public bool InHierarchy = false;
    }

    public class TimeflowContextMenuAnimationClip
    {
        public AnimationClip Clip;
        public TimeflowObject Obj;

        public TimeflowContextMenuAnimationClip(TimeflowObject obj, AnimationClip clip)
        {
            Obj = obj;
            Clip = clip;
        }
    }
    #endregion

}//AxonGenesis
#endif

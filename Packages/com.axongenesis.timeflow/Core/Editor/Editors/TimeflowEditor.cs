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
using UnityEngine.Playables;

namespace AxonGenesis
{
    [CustomEditor(typeof(Timeflow))]
    public class TimeflowEditor : AxonGenesisEditor<Timeflow, TimeflowEdit> { }

    sealed public class TimeflowEdit : AxonGenesisBehaviorEdit<Timeflow>
    {
#if TIMEFLOW_PRO
        public const string kAddTimeflow = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + TimeflowMenu.kTimeflow;
#else
        public const string kAddTimeflow = "Add Behavior/Timeflow";
#endif
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddTimeflow + TimeflowMenu.Tab + TimeflowShortcutBindings.AddNewTimeflow, false, 240)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddTimeflow, false, 240)]
        public static void _AddTimeflow() => AddTimeflow();


        public static Timeflow AddTimeflow(bool select = true, TimeflowViewDisplay.ObjectModes mode = TimeflowViewDisplay.ObjectModes.Everything)
        {
            Timeflow timeflow;
            GameObject obj = null;
            if (obj == null) {
                obj = new GameObject(TimeflowPreferences.Current.DefaultTimeflowName);
                UndoUtil.UndoCreate(obj, "Add Timeflow");
            }

            if (!obj.TryGetComponent<Timeflow>(out timeflow)) {
                timeflow = ObjectUtil.AddComponent<Timeflow>(obj);
            }

            timeflow.transform.SetParent(null);

            ObjectUtil.ResetTransform(obj);
            if (select) Selection.activeGameObject = obj;

            if (Timeflow.Active != null) {
                timeflow.Copy(Timeflow.Active, false);
            }


            EditorUtil.AssignUniqueGameObjectName(timeflow.gameObject, TimeflowPreferences.Current.DefaultTimeflowName, 1);

            Timeflow.Active = timeflow;
            timeflow.Display.ObjectMode = mode;
            timeflow.Refresh();

            return timeflow;
        }


        public static Timeflow AddPrecomp()
        {
            //Debug.Log($"AddPrecomp:{Time.time} ");
            if (Timeflow.Active.IsPlaying) {
                Timeflow.Active.Stop();
            }
            GameObject obj = Selection.activeGameObject;
            if (obj == null && Timeflow.Active != null) {
                obj = Timeflow.Active.gameObject;
            }

            string name = obj == null ? TimeflowPreferences.Current.DefaultTimeflowName : TimeflowPreferences.Current.DefaultPrecompName;
            name = EditorUtil.GetUniqueGameObjectName(name);

            Timeflow precomp = TimeflowEdit.AddTimeflow();

            precomp.gameObject.name = name;
            precomp.transform.SetParent(obj == null ? null : obj.transform);
            Timeflow.Active.View.Display.AddObjectToDisplay(precomp.gameObject);
            CopyTimeflowParentSettings(precomp);
            return precomp;
        }

        public static TimeflowObject GetPrecomposeContext(bool addTimeflowObject = false)
        {
            TimeflowObject context = TimeflowContext.Obj;
            if (context == null) {
                if (Selection.activeGameObject == null) return null;
                if (!Selection.activeGameObject.TryGetComponent(typeof(TimeflowObject), out Component comp)) {
                    if (addTimeflowObject) {
                        comp = Undo.AddComponent<TimeflowObject>(Selection.activeGameObject);
                    }
                    else return null;
                }
                if (comp is TimeflowObject tobj) {
                    context = tobj;
                }
                else return null;
            }
            return context;
        }

        public static void Precompose()
        {
            if (Timeflow.Active.IsPlaying) {
                Timeflow.Active.Stop();
            }
            TimeflowObject context = GetPrecomposeContext(true);
            if (context != null && context is Timeflow t) {
                AddPrecomp();
                return;
            }
            TimeflowObject.ObjectData data = context.GetObjectData();
            GameObject obj = Selection.activeGameObject;

            UndoUtil.Undo(obj, "Convert to Timeflow Instance", true);

            if (obj != null) {
                TimeflowMenu.GroupObjects(" " + TimeflowPreferences.Current.DefaultPrecompName, false);
                obj = Selection.activeGameObject;
            }
            if (obj.TryGetComponent<TimeflowObject>(out TimeflowObject tobj)) {
                Undo.DestroyObjectImmediate(tobj);
            }

            Timeflow timeflow = Undo.AddComponent<Timeflow>(obj);
            timeflow.ApplyObjectData(data);

            Timeflow parent = CopyTimeflowParentSettings(timeflow);
            Timeflow.Active.View.Display.AddObjectToDisplay(timeflow.gameObject);

            timeflow.GUIColor = data.Track.Channel.GUIColor;
            timeflow.Refresh(true);
            timeflow.View.Display.ObjectMode = TimeflowViewDisplay.ObjectModes.Everything;
            timeflow.AssignComponentIcon(timeflow);

            if (context != null) context.TimeOffset = 0f; // Localized to Timeflow now

            if (TimeflowPreferences.Current.OpenOnPrecompose) {
                timeflow.IsActive = true;
                timeflow.View.Display.DisplayEverything();
            }
            else
            if (!Timeflow.Active.View.Display.IsObjectDisplayed(obj)) {
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
            }
            Timeflow.GlobalRefresh();
        }

        public static void Decompose()
        {
            if (Timeflow.Active.IsPlaying) {
                Timeflow.Active.Stop();
            }
            TimeflowObject context = GetPrecomposeContext();
            if (context == null) return;
            if (context.GetType() == typeof(TimeflowObject)) {
                return;
            }

            if (PrefabUtil.IsPrefabInstance(context.gameObject)) {
                if (EditorUtil.ShowDialog("Prefab must be unpacked", "Cannot decompose a prefab instance. Would you like to unpack the prefab?", "Yes", "No")) {
                    PrefabUtil.UnpackPrefab(context.gameObject);
                }
                else return;
            }
            UndoUtil.Undo(context, "Convert to Timeflow Object", true);
            GameObject obj = context.gameObject;

            TimeflowObject.ObjectData data = context.GetObjectData();
            float startTime = context.StartTime;
            float endTime = context.EndTime;

            Timeflow.Active.View.Display.RemoveObjectFromDisplayRecursive(obj);
            Undo.DestroyObjectImmediate(context);

            context = Undo.AddComponent<TimeflowObject>(obj);
            context.ApplyObjectData(data);
            context.Setup();
            context.Track.AutoFullLength = false;
            context.Track.IsLocked = false;
            context.Track.LockKeys(false);
            context.Track.StartTime = startTime;
            context.Track.EndTime = endTime;

            obj.name = obj.name.Replace(" " + TimeflowPreferences.Current.DefaultPrecompName, "");

            if (!Timeflow.Active.View.Display.IsObjectDisplayed(obj)) {
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
            }
            Timeflow.GlobalRefresh();
        }

        public static Timeflow CopyTimeflowParentSettings(Timeflow timeflow)
        {
            Timeflow parent = ObjectUtil.GetComponentInParentOrAncestors<Timeflow>(timeflow.gameObject);
            if (parent != null) {
                timeflow.FPS = parent.FPS;
                timeflow.BPM = parent.BPM;
                timeflow.BeatNoteSize = parent.BeatNoteSize;
                timeflow.BeatsPerBar = parent.BeatsPerBar;
                timeflow.ForceFramerate = parent.ForceFramerate;
                timeflow.TimeInterval = parent.TimeInterval;
                timeflow.UpdateMethod = parent.UpdateMethod;
                timeflow.UpdateFrequency = parent.UpdateFrequency;

                if (timeflow.View != null && parent.View != null) {
                    timeflow.View.CopySettings(parent.View);
                }
            }

            return parent;
        }

        private const float _timeFieldWidth = 150f;
        private TimeflowGroupEdit TimeflowGroupUI;

        private Color HighlightColor = new Color(0.9f, 1f, 1f);
        private List<TimeflowObject> copies;

        public TimeflowEdit() { }

        public TimeflowEdit(Timeflow _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/user-guide/timeflow-editor";
        }

        public override void Refresh()
        {
            Timeflow.GlobalRefresh();
            GUIUtility.ExitGUI();
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (TimeflowGroupUI == null) TimeflowGroupUI = new TimeflowGroupEdit(target, editor);
            TimeflowGroupUI.GUISetup();
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontalBox();
            GUI.color = target.IsActive ? Color.green : AxonColor.Inactive;
            RectOffset margin = new RectOffset(0, 0, 1, 0);
            if (AxonGUI.ButtonTexture(target.IsActive ? AxonUI.ChannelLinkOnStyle : AxonUI.ChannelLinkOffStyle,
                "The active instance currently being displayed in the Timeflow window.", margin)) {
                target.IsActive = target.IsActive;
                TimeflowWindow.OpenWindow();
            }
            GUI.color = Color.white;// target.IsActive ? AxonColor.Active : AxonColor.Inactive;
            if (target.IsActive) {
                AxonGUI.LabelInline("Active");
            }
            else
            if (AxonGUI.ButtonInline("Click to make the active view")) {
                target.IsActive = !target.IsActive;
                TimeflowWindow.OpenWindow();
            }
            GUI.color = AxonColor.Default;

            AxonGUI.FlexibleSpace();
            AxonGUI.UndoName = "Set Show Children";
            AxonGUI.SetTooltip("If disabled, all children of this object remain hidden from view.");
            bool showChildren = AxonGUI.FieldToggleInline(target, "Show Children", target.ShowChildren);
            if (target.ShowChildren != showChildren) {
                target.ShowChildren = showChildren;
            }

            if (!TimeflowWindow.IsOpen) {
                if (AxonGUI.ButtonInline(TimeflowWindow.kOpenTimeflowWindow)) {
                    TimeflowWindow.OpenWindow();
                }
            }

            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuIcons()
        {
            base.GUIMenuIcons();
            if (AxonGUI.ButtonTexture(AxonUI.SettingsStyle.active.background, "Open Preferences", new RectOffset(0, 0, 2, 0))) {
                SettingsService.OpenUserPreferences("Preferences/TimeflowPreferences");
            }
        }

        public override void OnInspectorGUI()
        {
            MainGUI();
        }

        public void MainGUI()
        {
            AxonGUI.SetLabelWidth(160);

            TimeGUI();
            TimeScopeGUI();
            WorkAreaGUI();
            SettingsGUI();
            TimeflowGroupUI.TimeflowObjectUI.behaviorUI.MainGUI();
            DisplayGUI();
            MarkersGUI();
            QuickSelectGUI();
            TracksGUI();
            TimeflowGroupUI.ObjectsGUI();
            PrecompGUI();
            ToolsGUI();

            AxonGUI.ResetLabelWidth();

            if (GUI.changed) {
                // Validate and update preferences settings
                target.Preferences.OnValidate();
            }
        }

        public void TimeGUI()
        {
            AxonGUI.SetLabelWidth(120);
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTimeSettings = AxonGUI.Foldout(target.EditorShowTimeSettings, "Time");
            if (target.EditorShowTimeSettings) {
                if (Timeflow.Instances != null && Timeflow.Instances.Count > 1) {
                    if (AxonGUI.ButtonInline("Apply Settings to All")) {
                        foreach (Timeflow t in Timeflow.Instances) {
                            if (t == null) continue;
                            t.GlobalTimeScale = target.GlobalTimeScale;
                            t.FPS = target.FPS;
                            t.BPM = target.BPM;
                            t.View.TimeDisplay = target.View.TimeDisplay;
                        }
                    }
                }
            }
            AxonGUI.EndHorizontal();

            if (target.EditorShowTimeSettings) {
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Display Format";
                AxonGUI.SetTooltip("Sets how time is displayed in the Timeflow view. Has no effect on performance or behaviors.");
                target.View.TimeDisplay = (TimeflowView.TimeDisplayModes)AxonGUI.FieldEnumPopup(target, "Display Format", target.View.TimeDisplay, GUILayout.Width(300));
                if (target.View.TimeDisplay == TimeflowView.TimeDisplayModes.Timecode) {
                    AxonGUI.UndoName = "Set Use Fractional Seconds";
                    AxonGUI.SetTooltip("Only applies to timecode format. If enabled, that last digits of the timecode are displayed in values from .0 to .999. If disabled, the last digits are displayed as frame numbers");
                    target.View.UseFractionalTime = AxonGUI.FieldToggleInline(target, "Fractional Seconds", target.View.UseFractionalTime);
                }
                AxonGUI.EndHorizontal();


                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("The time now in this Timeflow.");
                float time = target.CurrentTime;
                AxonGUI.UndoName = "Set Current Time";
                time = AxonGUI.FieldTime(target, "Current Time", target.CurrentTime, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS);
                if (!target.IsPlaying) {
                    target.CurrentTimeExplicit = time; // To avoid interfering with playback
                }

                AxonGUI.EndHorizontal();

                if (target.Track != null) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Auto Duration";
                    AxonGUI.SetTooltip("If enabled this precomp will match the start, end, and duration of its parent Timeflow.");
                    target.Track.AutoFullLength = AxonGUI.FieldToggle(target, "Auto Duration", target.Track.AutoFullLength);
                    if (!target.Track.AutoFullLength) GUI.color = AxonColor.LightText;
                    AxonGUI.LabelInline("Match parent duration");
                    if (!target.Track.AutoFullLength) GUI.color = AxonColor.Default;
                    AxonGUI.EndHorizontal();
                }

                EditorGUI.BeginDisabledGroup(target.View.LockDuration || (target.Track != null && target.Track.AutoFullLength));
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Start Time";
                AxonGUI.SetTooltip("The start time in seconds. Time usually starts at 0, though can be set to any positive or negative value if desired.");
                target.GlobalStartTime = AxonGUI.FieldTime(target, "Start Time", target.GlobalStartTime, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(_timeFieldWidth));

                AxonGUI.UndoName = "Set Play From Start";
                AxonGUI.SetTooltip("If enabled, playback will always start from time 0.");
                target.PlayFromStart = AxonGUI.FieldToggleInline(target, "Play From Start", target.PlayFromStart);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set End Time";
                AxonGUI.SetTooltip("The total length in time of this Timeflow. Value is stored in seconds, but displayed based on the selected display format.");
                target.GlobalEndTime = AxonGUI.FieldTime(target, "End Time", target.GlobalEndTime, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(_timeFieldWidth));

                AxonGUI.BeginDisabledGroup(target.LoopEnabled);
                AxonGUI.UndoName = "Set Play Past End";
                AxonGUI.SetTooltip("Allows playback to continue beyond the end point, or work area end if enabled.");
                target.PlayPastEnd = AxonGUI.FieldToggleInline(target, "Play Past End", target.PlayPastEnd);
                AxonGUI.EndDisabledGroup();
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Duration";
                AxonGUI.SetTooltip("The total length in time of this Timeflow. Value is stored in seconds, but displayed based on the selected display format.");
                target.GlobalDuration = AxonGUI.FieldTime(target, "Duration", target.GlobalDuration, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(_timeFieldWidth));
                EditorGUI.EndDisabledGroup();

                AxonGUI.UndoName = "Set Lock Duration";
                AxonGUI.SetTooltip("Lock the start and end times to prevent accidental changes. Prevents dragging the end time marker in the Timeflow window.");
                target.View.LockDuration = AxonGUI.FieldToggleInline(target, "Lock", target.View.LockDuration);
                EditorGUI.BeginDisabledGroup(target.View.LockDuration);
                if (target.Audio != null && target.Audio.Source != null && target.Audio.Source.clip != null) {
                    if (AxonGUI.ButtonInline("Match Audio")) {
                        UndoUtil.Undo(target, "Match Audio Length", true);
                        target.EndTime = target.StartTime + target.Audio.Source.clip.length + target.Audio.StartAtTime;
                        target.ResetTrack();
                        target.SetupChannels(true);
                    }
                }
                AxonGUI.SetTooltip("Sets the start and end time to fit all keyframes and tracks currently displayed in the view.");
                if (AxonGUI.ButtonInline("Fit Display")) {
                    UndoUtil.Undo(target, "Fit Time to Displayed Objects", true);
                    float min = target.StartTime;
                    float max = target.EndTime;
                    target.View.Display.GetTimeRangeOfDisplayedObjects(out min, out max);
                    target.StartTime = min;
                    target.EndTime = max;
                    target.Refresh();
                }
                EditorGUI.EndDisabledGroup();

                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Enable Loop";
                AxonGUI.SetTooltip("Determines whether the playback continues from the start when it reaches the end of the work area, or full time range.");
                target.LoopEnabled = AxonGUI.FieldToggle(target, "Enable Looping", target.LoopEnabled);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set BPM";
                AxonGUI.SetTooltip("Sets beats per minute for music timing. Affects time display and the behaviors of BPM based animation.");
                target.BPM = AxonGUI.FieldFloat(target, "BPM", target.BPM);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set FPS";
                AxonGUI.SetTooltip("Frames per second affects how time is displayed but has no effect on performance or playback.");
                target.FPS = AxonGUI.FieldFloat(target, "FPS", target.FPS);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Custom Grid Increment";
                AxonGUI.SetTooltip("Sets a custom grid and snap increment. The Custom mode must be selected from the grid drop-down menu in the Timeflow view.");
                bool useCustomSnap = AxonGUI.FieldToggle(target, "Custom Grid", target.UseCustomSnap);
                if (useCustomSnap != target.UseCustomSnap) {
                    target.UseCustomSnap = useCustomSnap;
                }
                if (target.UseCustomSnap) {
                    target.CustomSnap = AxonGUI.FieldTimeInline(target, "Time Increment", target.CustomSnap);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.EndBox();

                if (target.HasTimeflowParent) {
                    // Precomps do not control playback
                    target.AutoPlay = false;
                }
                else {
                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Auto Play";
                    AxonGUI.SetTooltip("If enabled, this Timeflow will begin playing immediately during runtime.");
                    target.AutoPlay = AxonGUI.FieldToggle(target, "Auto Play", target.AutoPlay);

                    AxonGUI.BeginDisabledGroup(!target.AutoPlay);
                    AxonGUI.UndoName = "Set Startup Frame Buffer";
                    AxonGUI.SetTooltip("This settings is only applied when Auto Play is enabled. When a scene loads, it may take additional time to prepare assets. Set the Startup Frame Buffer to delay " +
                        "Timeflow playback until frames are playing back smoothly. The idea is that after the engine renders the first several frames, everything " +
                        "has finished initializing. If the value is set to 0, it may result in the animation skipping the start (according to how long it takes for the " +
                        "scene to initialize).");
                    target.StartupFrameBuffer = AxonGUI.FieldIntInline(target, "Startup Frame Buffer", target.StartupFrameBuffer);
                    if (target.StartupFrameBuffer < 0) target.StartupFrameBuffer = 0;
                    AxonGUI.EndDisabledGroup();
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();
                }


                GlobalTimeScaleGUI();
                AudioGUI();
            }
            AxonGUI.EndBox();
        }

        public void GlobalTimeScaleGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Global Time Scale Enabled";
            AxonGUI.SetTooltip("If enabled the global Time.timeScale value is set, affecting the entire scene and game engine.");
            target.CanSetGlobalTimeScale = AxonGUI.FieldToggle(target, "Global Time Scale", target.CanSetGlobalTimeScale);
            if (target.CanSetGlobalTimeScale) {
                AxonGUI.UndoName = "Global Time Scale";
                target.GlobalTimeScale = AxonGUI.FieldFloatInline(target, target.GlobalTimeScale);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Local Time Scale";
            AxonGUI.SetTooltip("Multiplies the speed of this Timeflow instance with the global time scale.");
            target.TimeScale = AxonGUI.FieldFloat(target, "Local Time Scale", target.TimeScale);
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();
        }

        public void AudioGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Audio";
            AxonGUI.SetTooltip("Assign an audio track to play sychncronized to this Timeflow.");
            target.Audio = AxonGUI.FieldObject(target, "Audio", target.Audio, typeof(AudioTrack), true, GUILayout.Width(200)) as AudioTrack;
            if (target.Audio == null) {
                if (AxonGUI.ButtonInline("Create Audio Track")) {
                    UndoUtil.Undo(target, "Create Audio Track", true);

                    GameObject obj = new GameObject("AudioTrack");
                    obj.transform.parent = target.transform;

                    AudioTrack audio = ObjectUtil.AddComponent<AudioTrack>(obj);
                    target.Audio = audio;

                    UndoUtil.UndoCreate(obj, "Create Audio Track");
                    SelectionUtil.Select(obj);

                    Debug.LogWarning("Please be sure to assign an Audio Clip to the new audio track.", audio);
                }
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();
        }

        public void TimeScopeGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTimeScope = AxonGUI.Foldout(target.EditorShowTimeScope, "Local Time Scope");
            AxonGUI.EndHorizontal();
            if (target.EditorShowTimeScope) {
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Time Scope Enabled";
                AxonGUI.SetTooltip("Places the Timeflow view in a temporary focused time state for editing objects in their local time.");
                target.IsTimeScopeEnabled = AxonGUI.FieldToggle(target, "Enabled", target.IsTimeScopeEnabled);

                AxonGUI.UndoName = "Set Time Scope Localize Time";
                AxonGUI.SetTooltip("If enabled, time is displayed starting from 0, otherwise the global (original) time is displayed.");
                target.IsTimeScopeLocalized = AxonGUI.FieldToggleInline(target, "Localize Time", target.IsTimeScopeLocalized);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Time Scope Start Time";
                AxonGUI.SetTooltip("The start time in seconds. Time usually starts at 0, though can be set to any positive or negative value if desired.");
                target.TimeScopeStart = AxonGUI.FieldTime(target, "Start Time", target.TimeScopeStart, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(_timeFieldWidth));
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Time Scope End Time";
                AxonGUI.SetTooltip("The total length in time of this Timeflow. Value is stored in seconds, but displayed based on the selected display format.");
                target.TimeScopeEnd = AxonGUI.FieldTime(target, "End Time", target.TimeScopeEnd, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(_timeFieldWidth));
                AxonGUI.EndHorizontal();

                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        public void WorkAreaGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowWorkArea = AxonGUI.Foldout(target.EditorShowWorkArea, "Work Area");
            if (AxonGUI.ButtonTexture(target.WorkAreaLocked ? AxonUI.LockBigOnStyle.normal.background : AxonUI.LockBigOffStyle.normal.background, "Lock Work Area", new RectOffset(0, 0, 2, 0))) {
                target.WorkAreaLocked = !target.WorkAreaLocked;
            }
            AxonGUI.EndHorizontal();
            if (target.EditorShowWorkArea) {
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Work Area Enabled";
                AxonGUI.SetTooltip("Defines a section of time in the Timeflow. This can be used work with a section of time and/or loop it during playback.");
                target.WorkAreaEnabled = AxonGUI.FieldToggle(target, "Enable Work Area", target.WorkAreaEnabled);

                AxonGUI.UndoName = "Set Work Area Disable On Start";
                AxonGUI.SetTooltip("If disabled on start, the work area is turned off when starting play mode or when running in a build. Use this to prevent the work area from unintentionally limiting the playback duration. However, when work area is used for interactive playback then this setting may be turned off to allow the work area to remain active when starting.");
                target.WorkAreaDisableOnStart = AxonGUI.FieldToggleInline(target, "Disable On Start", target.WorkAreaDisableOnStart);

                AxonGUI.EndHorizontal();

                if (target.WorkAreaEnabled) {
                    AxonGUI.BeginDisabledGroup(target.WorkAreaLocked);
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Work Area Start";
                    float start = AxonGUI.FieldTime(target, "Work Area Start", target.WorkAreaStart, target.View.TimeDisplay, false, target.View.UseFractionalTime, target.FPS);

                    AxonGUI.UndoName = "Set Allow Lead-In";
                    AxonGUI.SetTooltip("If enforce is enabled, playback begins at the Work Area Start only. Otherwise (if disabled) playback can play before and leading into the work area, then it will loop the work area if loop is enabled.");
                    target.WorkAreaAllowsLeadIn = AxonGUI.FieldToggleInline(target, "Allow Lead-In", target.WorkAreaAllowsLeadIn);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Work Area End";
                    float end = AxonGUI.FieldTime(target, "Work Area End", target.WorkAreaEnd, target.View.TimeDisplay, false, target.View.UseFractionalTime, target.FPS);
                    if (start != target.WorkAreaStart || end != target.WorkAreaEnd) {
                        target.SetWorkArea(start, end, true);
                    }

                    AxonGUI.BeginDisabledGroup(target.LoopEnabled);
                    AxonGUI.UndoName = "Set Play Past End";
                    AxonGUI.SetTooltip("This option is only avaialable when looping is off. If enabled and looping is off, playback continues beyond the end of the work area. " +
                        "If the work area is off, this option also allows Timeflow to continue playing beyond the end of its timeline");
                    target.WorkAreaPlayPastEnd = AxonGUI.FieldToggleInline(target, "Play Past End", target.WorkAreaPlayPastEnd);
                    AxonGUI.EndDisabledGroup();
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndDisabledGroup();
                }

                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        public void SettingsGUI()
        {
            AxonGUI.SetLabelWidth(140);
            AxonGUI.BeginBox();
            target.EditorShowSettings = AxonGUI.Foldout(target.EditorShowSettings, "Settings");
            if (target.EditorShowSettings) {
                ParentGUI();
                OptionsGUI();
            }
            AxonGUI.EndBox();
        }

        public void ParentGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            AxonGUI.SetTooltip("Timeflow automatically finds and assigns parent Timeflow instances based on the scene hierarchy. This field value is only for reference and should not be changed.");
            if (!target.HasTimeflowParent) {
                AxonGUI.Label("Parent", "None");
            }
            else {
                AxonGUI.FieldObject(target, "Parent", target.TimeflowParent, typeof(Timeflow), true);
                if (target.HasTimeflowParent) {
                    AxonGUI.UndoName = "Set Loop In Parent";
                    AxonGUI.SetTooltip("If enabled, this Timeflow instance will loop continuously within the parent Timeflow.");
                    target.LoopInParent = AxonGUI.FieldToggleInline(target, "Loop", target.LoopInParent);
                }
            }
            AxonGUI.EndHorizontal();

            AxonGUI.UndoName = "Set Display Color";
            target.GUIColor = AxonGUI.FieldColor(target, "Display Color", target.GUIColor, false);
            //AxonColor.TimeScope = AxonGUI.FieldColor(target, "TimeScope", AxonColor.TimeScope, false);

            AxonGUI.BeginDisabledGroup(target.HasTimeflowParent);
            if (target.HasTimeflowParent) {
                AxonGUI.HelpBox("Nested Timeflow instances get their timing from the parent Timeflow and therefore cannot be synced to a Timeline Director. " +
                    "To sync with a Timeline Director, this Timeflow must be unparented from all other Timeflow instances.", MessageType.Info);
            }
            AxonGUI.BeginHorizontal();
            bool attach = target.DirectorSyncEnabled;
            AxonGUI.UndoName = "Set Sync Timeline Director";
            AxonGUI.SetTooltip("Use this to link Unity's Timeline with Timeflow so that time is synchronized between both. This is not required and only needed if using Unity's builtin Timeline in conjunction with Timeflow.");
            attach = AxonGUI.FieldToggle(target, "Sync Timeline Director", attach);
            if (target.DirectorSyncEnabled != attach) {
                target.DirectorSyncEnabled = attach;
                target.SetupDirector();
            }
            if (target.DirectorSyncEnabled) {
                AxonGUI.EndHorizontal();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Timeline Director";
                AxonGUI.SetTooltip("Assign a Playable Director from a Timeline instance.");
                target.Director = AxonGUI.FieldObject(target, "Director", target.Director, typeof(PlayableDirector), true) as PlayableDirector;

                AxonGUI.UndoName = "Set Timeline Director Time";
                target.DirectorTime = AxonGUI.FieldTimeInline(target, "Time", target.DirectorTime, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS);
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndDisabledGroup();

            AxonGUI.EndBox();
        }

        public void OptionsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.Heading("Options");

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Shader Time";
            AxonGUI.SetTooltip("If enabled, the current time will be set as a global shader property named _TimeflowTime. This can be referenced in shaders and shader graphs. Shader graph property _TimeflowTime must NOT be exposed.");
            target.SetShaderTime = AxonGUI.FieldToggle(target, "Set Shader Time", target.SetShaderTime);
            if (target.SetShaderTime) {
                AxonGUI.UndoName = "Set Shader Time Property Name";
                if (string.IsNullOrEmpty(target.ShaderTimeName)) target.ShaderTimeName = "_TimeflowTime";
                target.ShaderTimeName = AxonGUI.FieldTextInline(target, target.ShaderTimeName);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Shader Frame";
            AxonGUI.SetTooltip("Sets the global shader property _TimeflowFrame to be referenced in shaders and shader graphs.");
            target.SetShaderFrame = AxonGUI.FieldToggle(target, "Set Shader Frame", target.SetShaderFrame);
            if (target.SetShaderFrame) {
                AxonGUI.UndoName = "Set Shader Frame Property Name";
                if (string.IsNullOrEmpty(target.ShaderFrameName)) target.ShaderFrameName = "_TimeflowFrame";
                target.ShaderFrameName = AxonGUI.FieldTextInline(target, target.ShaderFrameName);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();

            AxonGUI.Space();
            AxonGUI.HelpBox("See Preferences > Timeflow for additional global settings.", MessageType.Info);
        }

        public void MarkersGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowMarkers = AxonGUI.Foldout(target.EditorShowMarkers, "Markers");
            if (target.EditorShowMarkers) {
                AxonGUI.UndoName = "Set Time Mode";
                AxonGUI.SetTooltip("Use Global Time mode to goto the marker while keeping the full timeline. Select Local Time Scope to focus the timeline" +
                    " to the local time range of the marker.");
                target.MarkerTimeMode = (Timeflow.MarkerTimeModes)AxonGUI.FieldEnumPopupInline(target, "Time Mode", target.MarkerTimeMode, GUILayout.Width(250));

                AxonGUI.SetTooltip("If enabled and the work are is active, it is automaticaly set to the time range of the selected marker.");
                target.MarkersSetWorkArea = AxonGUI.FieldToggleInline(target, "Set Work Area", target.MarkersSetWorkArea);

                AxonGUI.UndoName = "Set Show Markers";
                AxonGUI.SetTooltip("Option to display markers in the Timeflow view.");
                target.ShowMarkers = AxonGUI.FieldToggleInline(target, "Show Markers", target.ShowMarkers);
                if (AxonGUI.ButtonInline("Copy Edit List")) {
                    target.View.Markers.CopyMarkerEditList();
                }
            }
            bool hasMarkers = target.MarkerList != null && target.MarkerList.Count > 0;
            if (hasMarkers) {
                bool locked = true;
                for (int x = 0; x < target.MarkerList.Count; x++) {
                    if (!target.MarkerList[x].Locked) {
                        locked = false;
                        break;
                    }
                }

                if (AxonGUI.ButtonTexture(locked ? AxonUI.Icons.LockBigOn : AxonUI.Icons.LockBigOff, "Lock All Markers")) {
                    for (int x = 0; x < target.MarkerList.Count; x++) {
                        target.MarkerList[x].Locked = !locked;
                    }
                }
            }
            else {
                target.MarkerList = null;
            }
            AxonGUI.EndHorizontal();

            if (target.EditorShowMarkers) {
                AxonGUI.BeginBox();

                if (hasMarkers) {
                    int i = 0;
                    int insert = -1;
                    int remove = -1;
                    bool isControl = Event.current != null && (Event.current.control || Event.current.command);

                    for (int x = 0; x < target.MarkerList.Count; x++) {
                        AxonGUI.BeginHorizontal();

                        if (AxonGUI.ButtonTexture(target.MarkerList[x].Locked ? AxonUI.Icons.LockOn : AxonUI.Icons.LockOff, "Lock marker to prevent changes")) {
                            target.MarkerList[x].Locked = !target.MarkerList[x].Locked;
                            if (isControl) {
                                for (int m = 0; m < target.MarkerList.Count; m++) {
                                    target.MarkerList[m].Locked = target.MarkerList[x].Locked;
                                }
                            }
                        }
                        EditorGUI.BeginDisabledGroup(target.MarkerList[x].Locked);
                        AxonGUI.UndoName = "Set Marker Enabled";
                        target.MarkerList[x].Enabled = AxonGUI.FieldToggleEnabled(target, target.MarkerList[x].Enabled);

                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Marker")) {
                            insert = i;
                        }
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Marker")) {
                            remove = i;
                        }

                        if (string.IsNullOrEmpty(target.MarkerList[x].Name)) {
                            target.MarkerList[x].Name = target.Markers.GetMarkerName(target.CurrentTime);
                        }

                        if (target.IsEditingMarkerIndices) {
                            AxonGUI.UndoName = "Set Marker ID";
                            target.MarkerList[x].ID = AxonGUI.FieldIntInline(target, target.MarkerList[x].ID, GUILayout.Width(20));
                        }
                        AxonGUI.UndoName = "Set Marker Name";
                        target.MarkerList[x].Name = AxonGUI.FieldTextInline(target, target.MarkerList[x].Name);

                        AxonGUI.UndoName = "Set Marker Color";
                        AxonGUI.SetTooltip("Show the marker label in the Timeflow view.");
                        bool showLabel = AxonGUI.FieldToggleInline(target, " ", target.MarkerList[x].ShowLabel);
                        if (target.MarkerList[x].ShowLabel != showLabel) {
                            target.MarkerList[x].ShowLabel = showLabel;
                        }
                        if (isControl) {
                            for (int m = 0; m < target.MarkerList.Count; m++) {
                                target.MarkerList[m].ShowLabel = showLabel;
                            }
                        }

                        if (target.MarkerTimeMode == Timeflow.MarkerTimeModes.GlobalTime) {
                            AxonGUI.UndoName = "Set Marker Global Time";
                            target.MarkerList[x].GlobalTime = AxonGUI.FieldTimeInline(target, "Time", target.MarkerList[x].GlobalTime, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS, GUILayout.Width(100));
                        }
                        else {
                            AxonGUI.UndoName = "Set Marker Time";
                            target.MarkerList[x].Time = AxonGUI.FieldTimeInline(target, "Time", target.MarkerList[x].Time, target.View.TimeDisplay, false, target.View.UseFractionalTime, target.FPS, GUILayout.Width(100));
                        }

                        AxonGUI.UndoName = "Set Marker Color";
                        target.MarkerList[x].LabelColor = AxonGUI.FieldColorInline(target, target.MarkerList[x].LabelColor, false, GUILayout.Width(40));

                        AxonGUI.UndoName = "Set Marker Tint";
                        AxonGUI.SetTooltip("Apply the marker color as a tint across its timespan in the timeline view");
                        bool tintSection = AxonGUI.FieldToggleInline(target, "Tint", target.MarkerList[x].TintSection);
                        if (target.MarkerList[x].TintSection != tintSection) {
                            target.MarkerList[x].TintSection = tintSection;
                        }
                        if (isControl) {
                            for (int m = 0; m < target.MarkerList.Count; m++) {
                                target.MarkerList[m].TintSection = tintSection;
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        if (AxonGUI.ButtonTexture(AxonUI.PlayerLastStyle.normal.background, "Goto time")) {
                            Timeflow.Active.Markers.GotoMarker(x, false);
                        }

                        AxonGUI.EndHorizontal();
                        i++;
                    }

                    if (remove > -1) {
                        UndoUtil.Undo(target, "Remove Marker", true);
                        target.MarkerList.RemoveAt(remove);
                    }
                    if (insert != -1) {
                        UndoUtil.Undo(target, "Add Marker", true);
                        target.MarkerList.Insert(insert + 1, new TimeflowMarker());
                    }
                    AxonGUI.Space();
                }

                if (target.IsEditingMarkerIndices) {
                    AxonGUI.HelpBox("Please be aware that changing index numbers affects scripts or behaviors that refer to makers by index.", MessageType.Warning);
                }

                AxonGUI.BeginHorizontal();
                AxonGUI.Label("", "", GUILayout.Width(24));
                if (hasMarkers && target.IsEditingMarkerIndices) {
                    if (hasMarkers && AxonGUI.ButtonInline("Re-Index")) {
                        UndoUtil.Undo(target, "Re-Index Markers", true);

                        for (int x = 0; x < target.MarkerList.Count; x++) {
                            target.MarkerList[x].ID = x + 1;
                        }
                    }
                    GUI.color = AxonColor.BrandRed;
                    if (AxonGUI.ButtonInline("Done Editing")) {
                        target.IsEditingMarkerIndices = false;
                    }
                    GUI.color = Color.white;
                }
                else {
                    if (AxonGUI.ButtonInline("Add Marker")) {
                        UndoUtil.Undo(target, "Add Marker", true);
                        target.Markers.AddMarker(0f, target.Markers.GetMarkerName(target.CurrentTime));
                    }
                    if (AxonGUI.ButtonInline("Clear")) {
                        UndoUtil.Undo(target, "Clear Markers", true);
                        target.MarkerList = null;
                    }
                    if (AxonGUI.ButtonInline("Sort")) {
                        UndoUtil.Undo(target, "Sort Markers", true);
                        target.Markers.SortMarkers();
                    }
                    AxonGUI.SetTooltip("Edit the index values uniquely identifying each marker.");
                    if (AxonGUI.ButtonInline("Edit Indices")) {
                        target.IsEditingMarkerIndices = true;
                    }

                    AxonGUI.UndoName = "Set Rename Markers Using Time";
                    target.NameMarkersWithTimecode = AxonGUI.FieldToggleInline(target, "Name Using Time", target.NameMarkersWithTimecode);
                    if (hasMarkers && AxonGUI.ButtonInline("Rename")) {
                        UndoUtil.Undo(target, "Rename Markers", true);

                        for (int x = 0; x < target.MarkerList.Count; x++) {
                            if (target.NameMarkersWithTimecode) {
                                target.MarkerList[x].Name = target.Markers.GetMarkerName(target.MarkerList[x].Time);
                            }
                            else {
                                target.MarkerList[x].Name = StringUtil.PadNumber2(x + 1);
                            }
                        }
                    }
                }
                AxonGUI.EndHorizontal();

                AxonGUI.Space();
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.Space();
                if (AxonGUI.ButtonInline("Assign New Colors")) {
                    if (target.MarkerList != null && target.MarkerList.Count > 0) {
                        foreach (TimeflowMarker marker in target.MarkerList) {
                            marker.LabelColor = TimeflowPreferences.GetRandomTrackColor();
                        }
                    }
                }
                if (AxonGUI.ButtonInline("Sequential")) {
                    if (target.MarkerList != null && target.MarkerList.Count > 0) {
                        int c = 1;
                        foreach (TimeflowMarker marker in target.MarkerList) {
                            marker.LabelColor = TimeflowPreferences.GetNextTrackColor();
                            c++;
                        }
                    }
                }
                if (AxonGUI.ButtonInline("Generate Markers")) {
                    UndoUtil.Undo(target, "Generate Markers", true);

                    if (target.AutoGenerateMarkersEvery <= 0f) {
                        Debug.LogError("Invalid time interval set for auto generating markers");
                    }
                    else {
                        target.MarkerList = new List<TimeflowMarker>();
                        float t = 0f;
                        int x = 1;
                        while (t < target.EndTime) {
                            TimeflowMarker marker = new TimeflowMarker {
                                Time = t,
                                ID = x,
                                Index = x
                            };

                            if (target.NameMarkersWithTimecode) {
                                marker.Name = target.Markers.GetMarkerName(t);
                            }
                            else {
                                marker.Name = StringUtil.PadNumber2(x + 1);
                            }

                            float h = t / target.EndTime;
                            marker.LabelColor = ColorUtil.HLSColor(h, 1f, 1f);

                            target.MarkerList.Add(marker);

                            t += target.AutoGenerateMarkersEvery;
                            x++;
                        }
                        target.Markers.SortMarkers();
                    }
                }

                AxonGUI.UndoName = "Set Auto Generate Markers Every (seconds)";
                target.AutoGenerateMarkersEvery = AxonGUI.FieldTimeInline(target, "Every", target.AutoGenerateMarkersEvery, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS);

                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        public void DisplayGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowDisplayLists = AxonGUI.Foldout(target.EditorShowDisplayLists, "Display Lists");
            if (AxonGUI.ButtonTexture(target.View.Display.IsLocked ? AxonUI.LockBigOnStyle.normal.background : AxonUI.LockBigOffStyle.normal.background, "Lock Display List")) {
                target.View.Display.IsLocked = !target.View.Display.IsLocked;
            }
            AxonGUI.EndHorizontal();


            if (target.EditorShowDisplayLists) {
                AxonGUI.Space();
                EditorGUI.BeginDisabledGroup(target.View.Display.IsLocked);
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Display Mode";
                TimeflowViewDisplay.ObjectModes m = (TimeflowViewDisplay.ObjectModes)AxonGUI.FieldEnumPopup(target, "Display Mode", target.View.Display.ObjectMode);
                if (target.View.Display.ObjectMode != m) {
                    target.View.Display.ObjectMode = m;
                }
                if (target.Displays == null) target.Displays = new List<TimeflowDisplayItem>();
                if (AxonGUI.ButtonInline("Add Selected Objects")) {
                    if (Selection.activeGameObject == target.gameObject) {
                        string msg = "To create display lists from objects in your scene, " +
                            "first lock this inspector view, then select the objects in your " +
                            "scene and press this button again. The selected objects will be added to " +
                            "a new display list, which can be edited further in the inspector.";
                        EditorUtility.DisplayDialog("Adding Selected Objects", msg, "OK");
                    }
                    else
                    if (Selection.gameObjects != null) {
                        target.View.Display.SaveSelected(Selection.activeGameObject.name);
                    }
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Enable Time Scope for Saved Displays";
                AxonGUI.SetTooltip("If enable, each saved display records the local time scope settings when the display is saved, and then it is recalled when viewed.");
                target.View.Display.EnableTimeScope = AxonGUI.FieldToggle(target, "Enable Time Scope", target.View.Display.EnableTimeScope);
                AxonGUI.EndHorizontal();

                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < target.Displays.Count; x++) {
                    AxonGUI.BeginVertical("box");
                    if (target.Displays[x] == null) target.Displays[x] = new TimeflowDisplayItem();

                    AxonGUI.BeginHorizontalIndent();

                    target.Displays[x].ShowObjects = AxonGUI.FoldoutInline(target.Displays[x].ShowObjects, "Show objects in view");

                    bool active = target.View.Display.Index == x;
                    GUI.backgroundColor = active ? HighlightColor : AxonColor.Default;

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove View")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add View")) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }
                    AxonGUI.UndoName = "Set Display Active";
                    bool isActive = AxonGUI.FieldToggleInline(target, active);
                    if (isActive && !active) {
                        target.View.Display.Load(x, false);
                    }

                    AxonGUI.UndoName = "Set Display Name";
                    target.Displays[x].Name = AxonGUI.FieldTextInline(target, target.Displays[x].Name);

                    if (target.View.Display.EnableTimeScope) {
                        AxonGUI.UndoName = "Display Set Time Scope Enabled";
                        AxonGUI.SetTooltip("If enabled, when activated the display will set the Local Time Scope to the specified range.");
                        target.Displays[x].IsTimeScopeEnabled = AxonGUI.FieldToggleInline(target, "Time Scope", target.Displays[x].IsTimeScopeEnabled);

                        if (target.Displays[x].IsTimeScopeEnabled) {
                            AxonGUI.UndoName = "Set Display Time Scope Start";
                            target.Displays[x].TimeScopeStart = AxonGUI.FieldTimeInline(target, target.Displays[x].TimeScopeStart, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS);

                            AxonGUI.UndoName = "Set Display Time Scope End";
                            target.Displays[x].TimeScopeEnd = AxonGUI.FieldTimeInline(target, "to", target.Displays[x].TimeScopeEnd, target.View.TimeDisplay, true, target.View.UseFractionalTime, target.FPS);

                            AxonGUI.UndoName = "Display Set Time Scope Localized";
                            AxonGUI.SetTooltip("If enabled, the time display is localized to the specified time scope specified range.");
                            target.Displays[x].IsTimeScopeLocalized = AxonGUI.FieldToggleInline(target, "Local Time", target.Displays[x].IsTimeScopeLocalized);
                        }
                    }
                    AxonGUI.EndHorizontal();

                    if (target.Displays[x].ShowObjects) {
                        ViewObjectsGUI(target.Displays[x]);
                    }

                    AxonGUI.EndVertical();
                }

                if (remove > -1) {
                    target.View.Display.Remove(remove);
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder View", true);
                    TimeflowDisplayItem a = target.Displays[moveUp];
                    TimeflowDisplayItem b = target.Displays[moveUp - 1];
                    target.Displays[moveUp] = b;
                    target.Displays[moveUp - 1] = a;
                }
                if (moveDown >= 0 && moveDown < target.Displays.Count - 1) {
                    UndoUtil.Undo(target, "Reorder View", true);
                    TimeflowDisplayItem a = target.Displays[moveDown];
                    TimeflowDisplayItem b = target.Displays[moveDown + 1];
                    target.Displays[moveDown] = b;
                    target.Displays[moveDown + 1] = a;
                }
                if (insert != -1) {
                    target.Displays.Insert(insert, new TimeflowDisplayItem());
                }

                AxonGUI.Space();
                AxonGUI.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(target.Displays == null || target.Displays.Count == 0);
                if (AxonGUI.ButtonInline("Clear All Display Lists")) {
                    if (EditorUtility.DisplayDialog("Clear All Display Lists?", "Are you sure you want to remove all of the saved display lists?", "Remove All", "Cancel")) {
                        UndoUtil.Undo(target, "Clear All Display Lists", true);
                        target.Displays = null;
                        target.View.Display.DisplayNothing();
                    }
                }
                if (AxonGUI.ButtonInline("Sort by Name")) {
                    UndoUtil.Undo(target, "Sort by Name", true);
                    target.Displays.Sort((TimeflowDisplayItem t1, TimeflowDisplayItem t2) => { return t1.Name.CompareTo(t2.Name); });
                }
                EditorGUI.EndDisabledGroup();

                AxonGUI.UndoName = "Set Automatically Save New Display Lists";
                target.AutoSaveDisplay = AxonGUI.FieldToggleInline(target, "Automatically Save New Lists", target.AutoSaveDisplay);

                AxonGUI.EndHorizontal();

                AxonGUI.EndBox();
                EditorGUI.EndDisabledGroup();
            }

            AxonGUI.EndBox();
        }

        public void ViewObjectsGUI(TimeflowDisplayItem view)
        {
            AxonGUI.BeginBox();

            if (view.Objects == null) view.Objects = new List<TimeflowObject>();
            if (view.Objects != null && view.Objects.Count > 0) {
                view.Objects.Sort((TimeflowObject t1, TimeflowObject t2) => { return t1 == null || t2 == null ? 0 : t1.SortOrder.CompareTo(t2.SortOrder); });

                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < view.Objects.Count; x++) {
                    AxonGUI.BeginHorizontalIndent();
                    AxonGUI.Label(" ", " ", GUILayout.Width(25));

                    bool active = target.View.Display.Index == x;
                    GUI.backgroundColor = active ? HighlightColor : AxonColor.Default;

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Object")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Object")) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }

                    AxonGUI.UndoName = "Set Display Object";
                    view.Objects[x] = (TimeflowObject)AxonGUI.FieldObjectInline(target, view.Objects[x], typeof(TimeflowObject), true);

                    AxonGUI.EndHorizontal();
                }

                if (remove > -1) {
                    view.Objects.RemoveAt(remove);
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Object", true);
                    if (Event.current.shift) {
                        view.Objects[moveUp].SortOrder = 0;
                    }
                    else {
                        view.Objects[moveUp].SortOrder = view.Objects[moveUp - 1].SortOrder - 10;
                    }
                }
                if (moveDown >= 0 && moveDown < view.Objects.Count - 1) {
                    UndoUtil.Undo(target, "Reorder Object", true);
                    if (Event.current.shift) {
                        view.Objects[moveDown].SortOrder = view.Objects[view.Objects.Count - 1].SortOrder + 10;
                    }
                    else {
                        view.Objects[moveDown].SortOrder = view.Objects[moveDown + 1].SortOrder + 10;
                    }
                }
                if (insert != -1) {
                    view.Objects.Insert(insert, Timeflow.SetupTimeflowObject(Selection.activeGameObject));
                }
            }
            AxonGUI.Space();
            AxonGUI.Space();
            AxonGUI.BeginHorizontal();
            AxonGUI.Label(" ", " ", GUILayout.Width(150));
            if (AxonGUI.ButtonInline("Select")) {
                GameObject[] objects = new GameObject[view.Objects.Count];
                int i = 0;
                foreach (TimeflowObject obj in view.Objects) {
                    objects[i] = obj.gameObject;
                    i++;
                }
                SelectionUtil.Select(objects);
            }
            if (AxonGUI.ButtonInline("Add Selected")) {
                if (Selection.gameObjects != null) {
                    UndoUtil.Undo(target, "Add Selected", true);
                    foreach (GameObject obj in Selection.gameObjects) {
                        TimeflowObject tobj = Timeflow.SetupTimeflowObject(obj);
                        if (!view.Objects.Contains(tobj)) {
                            view.Objects.Add(tobj);
                        }
                    }
                }
            }
            if (AxonGUI.ButtonInline("Add Empty")) {
                UndoUtil.Undo(target, "Add Empty", true);
                view.Objects.Add(null);
            }
            if (AxonGUI.ButtonInline("Sort by Name")) {
                UndoUtil.Undo(target, "Sort by Name", true);
                view.Objects.Sort((TimeflowObject t1, TimeflowObject t2) => { return t1.name.CompareTo(t2.name); });
            }
            if (AxonGUI.ButtonInline("Load Into View")) {
                target.View.Display.LoadDisplay(view);
            }
            AxonGUI.Space();
            if (AxonGUI.ButtonInline("Copy List")) {
                copies = new List<TimeflowObject>();
                foreach (TimeflowObject obj in view.Objects) {
                    copies.Add(obj);
                }
            }
            if (copies != null && AxonGUI.ButtonInline("Paste List")) {
                UndoUtil.Undo(target, "Paste List", true);
                foreach (TimeflowObject obj in copies) {
                    if (!view.Objects.Contains(obj)) {
                        view.Objects.Add(obj);
                    }
                }
            }
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();
        }

        public void TracksGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTimeflowObj = AxonGUI.Foldout(target.EditorShowTimeflowObj, "Object");
            if (target.EditorShowTimeflowObj) {
                AxonGUI.Indent++;

                TimeflowGroupUI.MainGUI();

                AxonGUI.Indent--;
            }
            AxonGUI.EndBox();
        }


        public void PrecompGUI()
        {
            if (Timeflow.Instances == null) Timeflow.GetAllInstances();
            if (Timeflow.Instances.Count < 2) return; // Only show when multiple instaces are present

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTimeflow = AxonGUI.Foldout(target.EditorShowTimeflow, "Precomposition");
            AxonGUI.EndHorizontal();

            if (target.EditorShowTimeflow) {
                AxonGUI.Space();
                AxonGUI.Indent++;

                // Don't allow Timeflow instances to specify their parent - it must be done by hierarchy. 
                // This is to prevent circular references and other complex issues that could arrise from chaotic setups.
                target.OverrideTimeflowParent = false;

                /*
                // Assign to self if null
                if (target.TimeflowParent == null) target.TimeflowParent = target;
                */
                if (target.TimeflowParent == target) {
                    //AxonGUI.HelpBox("This a master Timeflow instance, independent and not parented to any other Timeflow.");
                }
                else {
                    AxonGUI.HelpBox($"This Timeflow instance is a child of {(target.TimeflowParent == null ? "NULL" : target.TimeflowParent.name)}.");

                    AxonGUI.BeginHorizontalBox();
                    AxonGUI.BeginDisabledGroup(true);
                    AxonGUI.SetTooltip("The Master Timeflow drives the time for all Timeflow instances (precomps) within its hierarchy.");
                    AxonGUI.FieldObject(target, "Master Timeflow", target.Timeflow, typeof(Timeflow), true);
                    AxonGUI.EndDisabledGroup();
                    AxonGUI.EndHorizontal();
                }

                AxonGUI.Space();
                AxonGUI.Space();
                AxonGUI.Indent--;
            }

            AxonGUI.EndBox();
        }

        public void QuickSelectGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowQuickSelect = AxonGUI.Foldout(target.EditorShowQuickSelect, "Quick Select Objects");
            AxonGUI.EndHorizontal();

            if (target.EditorShowQuickSelect) {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                if (target.QuickSelectObjects == null) target.QuickSelectObjects = new GameObject[12];

                int moveUp = -1;
                int moveDown = -1;
                int remove = -1;

                for (int x = 0; x < target.QuickSelectObjects.Length; x++) {
                    AxonGUI.BeginHorizontalIndent();
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Object")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }

                    AxonGUI.UndoName = $"Set Quick Select Object {(x + 1)}";
                    AxonGUI.LabelInline("Shift + F" + (x + 1), "", GUILayout.Width(100));
                    target.QuickSelectObjects[x] = (GameObject)AxonGUI.FieldObjectInline(target, target.QuickSelectObjects[x], typeof(GameObject), true);
                    if (AxonGUI.ButtonInline("Select")) {
                        Timeflow.QuickSelect(x);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (remove > -1) {
                    target.QuickSelectObjects[remove] = null;
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Object", true);
                    GameObject a = target.QuickSelectObjects[moveUp];
                    GameObject b = target.QuickSelectObjects[moveUp - 1];
                    target.QuickSelectObjects[moveUp] = b;
                    target.QuickSelectObjects[moveUp - 1] = a;
                }
                if (moveDown >= 0 && moveDown < target.QuickSelectObjects.Length - 1) {
                    UndoUtil.Undo(target, "Reorder Object", true);
                    GameObject a = target.QuickSelectObjects[moveDown];
                    GameObject b = target.QuickSelectObjects[moveDown + 1];
                    target.QuickSelectObjects[moveDown] = b;
                    target.QuickSelectObjects[moveDown + 1] = a;
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                AxonGUI.UndoName = "Set View Quick Select Object in Timeflow";
                target.EditorViewQuickSelect = AxonGUI.FieldToggleInline(target, "View In Timeflow", target.EditorViewQuickSelect);

                if (AxonGUI.ButtonInline("Clear All")) {
                    UndoUtil.Undo(target, "Clear All Quick Select Objects", true);
                    target.QuickSelectObjects = new GameObject[12];
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            AxonGUI.EndBox();
        }

        public void ToolsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTools = AxonGUI.Foldout(target.EditorShowTools, "Tools");
            if (target.EditorShowTools) {
                AxonGUI.BeginBox();

                AxonGUI.UndoName = "Set Editor Tool";
                target.EditorTool = (Timeflow.EditorTools)AxonGUI.FieldEnumPopup(target, "Select", target.EditorTool);
                AxonGUI.Space();
                if (target.EditorTool == Timeflow.EditorTools.GenerateTitles) {

                    AxonGUI.HelpBox("Generate titles from Markers using TextMeshPro. If no prefab is assigned then titles are created from scratch. Note that you'll need to position the Titles group in the scene relative to the camera or wherever you wish to view them.", MessageType.Info);
                    AxonGUI.Space();
                    if (target.MarkerList == null || target.MarkerList.Count == 0) {
                        AxonGUI.HelpBox("Please add Markers to the Timeflow to generate titles from.", MessageType.Error);
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Title Prefab";
                        AxonGUI.SetTooltip("The title prefab can be left empty, or assign a game object with a TimeflowObject and TextMeshPro.");
                        target.TitlePrefab = (GameObject)AxonGUI.FieldObject(target, "Title Prefab", target.TitlePrefab, typeof(GameObject), true);

                        AxonGUI.UndoName = "Set Title Sequential";
                        AxonGUI.SetTooltip("If enabled, each title progresses in color through the spectrum over the duration of Timeflow.");
                        target.TitleRainbow = AxonGUI.FieldToggleInline(target, "Sequential", target.TitleRainbow);

                        AxonGUI.UndoName = "Set Title Canvas UI";
                        AxonGUI.SetTooltip("If enabled, titles are created using a Canvas and TextMeshProUGUI. Otherwise standard game objects are created using TextMeshPro.");
                        target.IsTitleUI = AxonGUI.FieldToggleInline(target, "Canvas UI", target.IsTitleUI);
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Title Container";
                        AxonGUI.SetTooltip("The target game object to hold the generated titles. This can be left empty to generate a new game object.");
                        target.TitleContainer = (GameObject)AxonGUI.FieldObject(target, "Container", target.TitleContainer, typeof(GameObject), true);

                        AxonGUI.UndoName = "Set Title Mode";
                        AxonGUI.SetTooltip("Clear and Regenerate: destroys any existing title objects and recreates them.\n\nCreate or Update Existing: creates new titles if they aren't create yet, otherwise updates existing objects.\n\nCreate New Only: leaves existing titles unchannged and only creates new ones if they haven't been created yet.");
                        target.TitleMode = (Timeflow.TitleModes)AxonGUI.FieldEnumPopupInline(target, target.TitleMode);
                        AxonGUI.EndHorizontal();

                        AxonGUI.Space();
                        if (AxonGUI.Button("Generate Titles")) {
                            target.GenerateTitles();
                        }
                    }
                }
                else
                if (target.EditorTool == Timeflow.EditorTools.AddTimeflow) {
                    //AxonGUI.HelpBox("Create another Timeflow to contain objects.\n - Add a precomp to create a sublayer of time.", MessageType.Info);
                    AxonGUI.Space();
                    AxonGUI.Space();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Space(70);
                    AxonGUI.SetTooltip("Create a new nested Timeflow. Add a precomp to create a sublayer of time with its own time, markers, and settings.");
                    if (AxonGUI.Button("Add New Precomp")) {
                        SelectionUtil.Select(target.gameObject);
                        TimeflowContextMenu.AddNewPrecomp();
                    }
                    AxonGUI.SetTooltip("This adds a Timeflow instance at the same hierarchical level.. ");
                    if (AxonGUI.Button("Add Timeflow Sibling")) {
                        target.AddTimeflow();
                    }
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.EditorTool == Timeflow.EditorTools.ScaleGlobalTime) {
                    AxonGUI.Heading("(Experimental)");
                    AxonGUI.HelpBox("Use this to scale the timing of all objects managed by this Timeflow instance. This attempts to scale the timing of all markers, behaviors, channels, keyframes, and tangents by the specified amount. This is an experimental feature and may produced unexpected results. Scaling time globally does not affect Time.timeScale nor does it affect Unity animation clips, Animators, or Unity Timeline. To scale a group of keyframes or to have more control, consider using the Keyframe Tools in the Timeflow view, which can be used to scale time on selected tracks and keys.", MessageType.Info);
                    AxonGUI.Space();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Global Scale Time";
                    target.ScaleToolValue = AxonGUI.FieldFloat(target, "Global Scale Time", target.ScaleToolValue);
                    if (AxonGUI.ButtonInline("Apply")) {
                        if (EditorUtility.DisplayDialog("Scale Global Time?", "Are you sure you want to scale the timing of all objects in this Timeflow? It is highly recommended that you save a backup of your scene first.", "YES CONTINUE", "CANCEL")) {
                            target.ScaleTimeGlobal();
                        }
                    }
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.EditorTool == Timeflow.EditorTools.CropTime) {
                    AxonGUI.HelpBox("Use this to remove keyframes outside of the timeline range. This can happen when groups of keyframes are shifted or scaled in time. There's no harm in leaving keyframes outside of the time range, but may be cleaned up using this operation. This affects all objects in the Timeflow hierarchy.", MessageType.Info);
                    AxonGUI.Space();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Crop Add End Keys";
                    AxonGUI.SetTooltip("Adding end keys attempts to preserve animation curves extending outside of the time range by inserting a keyframe at the end of each animated channel. This is only applied where keyframes are removed outside of the time range. If interpolation needs to be precice, you may want to disable this checkbox and add the keys manually first before using this operation to remove the extraneous keys.");
                    target.AddEndKeysOnCrop = AxonGUI.FieldToggle(target, "Add End Keys", target.AddEndKeysOnCrop);
                    if (AxonGUI.ButtonInline("Crop Time")) {
                        if (EditorUtility.DisplayDialog("Crop Time?", "Are you sure you want to delete all keyframes beyond the start and end of this Timeflow?", "YES", "NO")) {
                            target.CropTimeRecursive(target.gameObject);
                        }
                    }

                    AxonGUI.EndHorizontal();
                }

                AxonGUI.EndBox();
            }

            AxonGUI.EndBox();
        }
    }

}//AxonGenesis

#endif

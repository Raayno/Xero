// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

#if ANIMATIONRIGGING_1_OR_NEWER
using UnityEngine.Animations.Rigging;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        #region STATIC VARS

        public static readonly string Version = "1.9.3";

        public static List<Timeflow> Instances { get; private set; }
        public static bool ShowInfo = true;

        private static Timeflow _active;
        private static Timeflow _Active {
            get {
                return _active;
            }
            set {
                if (_active != value) {
                    // The Timeflow instance must be active and enabled to be set as the active instance
                    if (value != null && (!value.enabled || !value.gameObject.activeInHierarchy)) return;

                    // Set previous active instance to inactive
                    if (_active != null) _active.IsCurrentlyActive = false;

                    _active = value;

                    // Set new active instance to active if not null
                    if (_active != null) _active.IsCurrentlyActive = true;

                    //Debug.Log($"Active:{(_active == null ? "null" : _active.name)}");
#if UNITY_EDITOR
                    if (!Application.isPlaying && _active != null) _active.Refresh();
#endif
                }
            }
        }

        public static bool IsPrefabMode {
            get {
#if UNITY_EDITOR
                if (Timeflow.Active == null || Timeflow.Active.View == null || Timeflow.Active.View.Display == null) return false;
                if (Timeflow.Active.View.Display.IsDisplayingPrefab) return true;
#endif
                return false;
            }
        }

        #endregion

        #region REGISTER INSTANCES

        public static void RegisterAllInstances(bool force = false)
        {
            if (force || Instances == null || Instances.Count == 0) {
                Instances = new List<Timeflow>(UnityEngine.Object.FindObjectsByType(typeof(Timeflow), FindObjectsInactive.Include, FindObjectsSortMode.None) as Timeflow[]);
                if (Instances != null) {
                    foreach (Timeflow instance in Instances) {
                        if (instance.TimeflowParent == null || instance.TimeflowParent == instance) {
                            instance.Refresh();
                        }
                    }
                }
            }

            SortInstances();

            // This performs a 2nd pass which may repeat the object setups above but needed to get group instances
            TimeflowGroup.RegisterAllGroups(true);
        }

        public static void SortInstances()
        {
            List<Timeflow> instances = new List<Timeflow>();
            foreach (Timeflow instance in Instances) {
                if (instance != null) instances.Add(instance);
            }
            Instances = instances;
            Instances.Sort((Timeflow a, Timeflow b) => { return a.name.CompareTo(b.name); });
        }

        public static void RegisterInstance(Timeflow obj)
        {
            if (obj != null) {
                if (Instances == null) RegisterAllInstances(true);
                if (!Instances.Contains(obj)) {
                    Instances.Add(obj);
                    SortInstances();
                }
            }
        }

        public static void UnregisterInstance(Timeflow obj)
        {
            if (Instances == null) RegisterAllInstances(true);
            if (Instances != null) {
                if (Instances.Contains(obj)) {
                    Instances.Remove(obj);
                    SortInstances();
                }
                if (Instances.Count > 0) {
                    if (Active == obj) {
                        Active = Instances[0];
                    }
                }
            }
            else {
                Active = null;
            }
        }

        public static void RegisterGroup(TimeflowGroup group)
        {
            Timeflow timeflow = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(group.gameObject);
            if (timeflow == null) {
                Debug.LogWarning("Could not find Timeflow for group '" + group.gameObject.name + "'");
            }
            if (timeflow != null) {
                timeflow.AddGroup(group);
            }
        }

        public static void UnregisterGroup(TimeflowGroup group)
        {
            if (group != null && group.TimeflowParent != null) {
                group.TimeflowParent.RemoveGroup(group);
            }
        }

        #endregion

        [SerializeField]
        private bool _IsCurrentlyActive = false;

        #region ACCESSORS

        /// <summary>
        /// Always contains a reference to the current Timeflow being displayed - or null if none. This is only relevant
        /// in the editor and used as context for the currently viewed Timeflow instance.
        /// </summary>
        public static Timeflow Active {
            get {
                if (_Active == null) {
                    ActivateFirstParentTimeflow();
                }
                return _Active;
            }
            set {
                if (_Active != value && (value == null || value.enabled)) {
                    _Active = value;
                    //Debug.Log($"Active:{(_Active == null ? "null" : _Active.name)}");
                }
                if (_Active == null) ActivateFirstParentTimeflow();
            }
        }

        private static void ActivateFirstParentTimeflow()
        {
            if (Instances == null || Instances.Count == 0) return;
            foreach (Timeflow timeflow in Instances) {
                if (timeflow == null || !timeflow.enabled) continue;
                if (!timeflow.HasTimeflowParent && timeflow.gameObject.activeInHierarchy) {
                    // Select first root Timeflow instance that is active
                    _Active = timeflow;
                    break;
                    //Debug.Log("Active:" + (_Active == null ? "null" : _Active.name));
                }
            }
        }

        public static bool IsAutoKeyframingEnabled {
            get {
                if (Application.isPlaying) return false;// Cannot keyframe in play mode
                return Timeflow.Active == null ? false : Timeflow.Active.AutoKeyframingEnabled;
            }
            set {
                if (Timeflow.Active == null) return;
                Timeflow.Active.AutoKeyframingEnabled = value;
            }
        }

        public static bool IsAutoKeyframingInvalidThisFrame { get; set; }

        /// <summary>
        /// Remembers the last active instance of Timeflow so that when opening a scene or after script recompile the
        /// Timeflow view remains as the user last set it. Only applicable when multiple Timeflows are in the scene.
        /// </summary>
        public bool IsCurrentlyActive {
            get {
                return _IsCurrentlyActive;
            }
            set {
                _IsCurrentlyActive = value;
                if (value) {
                    _IsCurrentlyActive = this;
                    //Debug.Log("_IsCurrentlyActive:" + name);
                    if(Timeflow.Instances != null && Timeflow.Instances.Count > 1) {
                        foreach (Timeflow timeflow in Timeflow.Instances) {
                            if (timeflow != this) timeflow.IsCurrentlyActive = false;
                        }
                    }
                }
            }
        }

        public static float ApplyTimeTolerance(float time)
        {
            if (TimeflowPreferences.Current.TimeTolerance > 0f) {
                time = MathUtil.RoundToInterval(time, TimeflowPreferences.Current.TimeTolerance);
            }
            else {
                time = MathUtil.Validate(time);
            }
            return time;
        }

        #endregion

        #region PLAYBACK

        public static void PlayAll(bool fromStart = false)
        {
            if (Instances != null) {
                foreach (Timeflow t in Instances) {
                    if (t != null) t.Play(fromStart);
                }
            }
        }

        public static void StopAll()
        {
            if (Instances != null) {
                foreach (Timeflow t in Instances) {
                    if (t != null) t.Stop();
                }
            }
        }

        public static void StopAndRewindAll()
        {
            if (Instances != null) {
                foreach (Timeflow t in Instances) {
                    if (t != null) {
                        t.Stop();
                        t.Rewind();
                    }
                }
            }
        }

        #endregion

        #region TOOLS

#if UNITY_EDITOR

        public static bool IsSoloMode {
            get {
                if (Timeflow.Active == null || Timeflow.Active.Display == null) return false;
                return Timeflow.Active.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Solo;
            }
            set {
                if (Timeflow.Active == null || Timeflow.Active.Display == null) return;
                Timeflow.Active.Display.ChannelMode = value ? TimeflowViewDisplay.ChannelModes.Solo : TimeflowViewDisplay.ChannelModes.None;
                Timeflow.Active.Refresh();
            }
        }

        public static bool NeedsGlobalRefresh = false;

        public static void GlobalRefresh()
        {
            if (Timeflow.Active != null && Timeflow.Active.IsPlaying) {
                // During playback refresh must be deferred until it can be safely performed without modifying collections used in updating
                NeedsGlobalRefresh = true;
                return;
            }
            GlobalRefreshImmediate();
        }

        private static void GlobalRefreshImmediate()
        {
            //Debug.Log("Timeflow.GlobalRefresh");
            NeedsGlobalRefresh = false;
            RegisterAllInstances(true);
        }

        public static void CreateNewTimeflow()
        {
            GameObject go = GameObject.Find(TimeflowPreferences.Current.DefaultTimeflowName);
            Timeflow instance = null;
            if (go != null) instance = go.GetComponent<Timeflow>();
            if (instance == null) {
                go = new GameObject(TimeflowPreferences.Current.DefaultTimeflowName);
                UndoUtil.UndoCreate(go, TimeflowPreferences.Current.DefaultTimeflowName);
                Timeflow.Active = go.AddComponent<Timeflow>();
                SelectionUtil.Select(go);
            }
            else {
                instance.enabled = true;
            }
            TimeflowPreferences.ReviewCheck();
        }

#endif
        /// <summary>
        /// Adds an object to the hierarchy view, resolving any conflicts with existing objects if
        /// necessary.
        /// </summary>
        public static TimeflowObject SetupTimeflowObject(GameObject obj, bool addToDisplay = false)
        {
            if (obj == null) return null;
            TimeflowObject tobj = null;

            if (obj.TryGetComponent<Timeflow>(out var tm)) {
                tm.Setup();

                if (tm.HasTimeflowParent) {
                    //Debug.Log($"HasParent:{tm.Parent.name}", obj);
                    // Only allow nested Timeflows to display in the Timeflow view
                    tobj = tm;
                    tm.TimeflowParent.IsActive = true;
                }
                else {
                    //Debug.Log($"NO Parent:{tm.name}", obj);
                    tm.IsActive = true;
                }
            }
            else
            if (obj.TryGetComponent<TimeflowObject>(out tobj)) {
                //Debug.Log($"{obj.name}.SetupTimeflowObject");
                tobj.Setup();
            }
            else {
                //Debug.Log($"{obj.name}.SetupTimeflowObject NEW");
#if UNITY_EDITOR
                /// Each TimeflowObject sets up its own children on awake
                tobj = Undo.AddComponent<TimeflowObject>(obj);
                tobj.OnNewInstance();

                tobj.GUIColorAuto = TrackColorPalette.IsAutomaticColor;
                TrackColorPalette.UpdateTrackColor(tobj);

                /// Collapse inspector UI to reduce clutter as the default
                tobj.EditorShowUI = false;

                /// Automatically setup special updating tools when added to the view.
                if (obj.TryGetComponent<ParticleSystem>(out var particleSystem)) {
                    if (!obj.TryGetComponent<ParticleSystemUpdate>(out var comp)) {
                        Undo.AddComponent<ParticleSystemUpdate>(obj);
                    }
                }

                if (obj.TryGetComponent<TrailRenderer>(out var trailRenderer)) {
                    if (!obj.TryGetComponent<TrailRendererUpdate>(out var comp)) {
                        Undo.AddComponent<TrailRendererUpdate>(obj);
                    }
                }

//#if ANIMATIONRIGGING_1_OR_NEWER
//                if (obj.TryGetComponent<RigBuilder>(out var rigBuilder)) {
//                    if (!obj.TryGetComponent<RigUpdate>(out var comp)) {
//                        Undo.AddComponent<RigUpdate>(obj);
//                    }
//                }
//#endif
                if (obj.TryGetComponent<VideoPlayer>(out var videoPlayer)) {
                    if (!obj.TryGetComponent<VideoPlayerUpdate>(out var comp)) {
                        Undo.AddComponent<VideoPlayerUpdate>(obj);
                    }
                }

                Type type = Type.GetType("AxonGenesis.SpineAnimator, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (type != null) {
                    MethodInfo methodInfo = type.GetMethod("SetupTimeflowObject", BindingFlags.Static | BindingFlags.Public);
                    if (methodInfo != null) {
                        object[] parameters = new object[] { obj };
                        methodInfo.Invoke(null, parameters); // Since it's a static method, the first parameter is null.
                    }
                }

#else
                tobj = obj.AddComponent<TimeflowObject>();
#endif
                tobj.Enabled = true;
                tobj.BehaviorsEnabled = true;
            }

#if UNITY_EDITOR
            if (addToDisplay) {
                if (Timeflow.Active.Display.ChannelMode == TimeflowViewDisplay.ChannelModes.Solo) {
                    // Replace the soloed objects with the current selection
                    Timeflow.Active.Display.SoloObjectToggle(tobj, true);
                }
                Timeflow.Active.View.Display.AddObjectToDisplay(obj);
            }
#endif
            return tobj;
        }

        private static Dictionary<Transform, Transform> _reparentObjects = new Dictionary<Transform, Transform>();

        /// <summary>
        /// If parenting is changed within an animated hierarchy, it could cause unexpected behavior. To
        /// ensure proper setup of the hierarchy, use Reparent instead of transform.SetParent (nor
        /// transform.parent = someobject)
        /// </summary>
        public static bool Reparent(Transform child, Transform newParent, bool resetTransform = true)
        {
            if (child == null || child.parent == newParent) return false;
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfAnyPrefab(child.gameObject)) {
                Debug.LogWarning("Setting the parent of a transform which resides in a Prefab instance is not possible");
                return false;
            }
#endif
            if (Timeflow.Active != null && Timeflow.Active.IsPlaying) {
                if (_reparentObjects == null) _reparentObjects = new Dictionary<Transform, Transform>();
                if (!_reparentObjects.ContainsKey(child)) _reparentObjects.Add(child, newParent);
            }
            else {
                ReparentNow(child, newParent, resetTransform);
            }

            return true;
        }

        /// <summary>
        /// Performs reparenting immediately
        /// </summary>
        /// <param name="child">The child to move</param>
        /// <param name="newParent">The new parent to assign it to</param>
        /// <param name="resetTransform">Resets the local transforms to 0,0,0 scale 1,1,1</param>
        public static void ReparentNow(Transform child, Transform newParent, bool resetTransform = true)
        {
            if (child == null || child.parent == newParent) return;
            child.SetParent(newParent);
            if (resetTransform) ObjectUtil.ResetTransform(child);
            if (child.TryGetComponent<TimeflowObject>(out var tobj)) {
                tobj.Setup();
            }
        }

        /// <summary>
        /// Executes reparenting safely within the update loop, so as not to modify collections during object iterations.
        /// </summary>
        public static void UpdateDeferredReparentTransforms()
        {
            if (_reparentObjects == null || _reparentObjects.Count == 0) return;
            //Debug.Log($"UpdateDeferredReparentTransforms:{_reparentObjects.Count}");
            foreach (KeyValuePair<Transform, Transform> kvp in _reparentObjects) {
                ReparentNow(kvp.Key, kvp.Value);
            }
            _reparentObjects.Clear();
            _reparentObjects = null; // Clear for next request
#if UNITY_EDITOR
            Timeflow.GlobalRefresh();
#endif
        }

        #endregion

    }

}//AxonGenesis

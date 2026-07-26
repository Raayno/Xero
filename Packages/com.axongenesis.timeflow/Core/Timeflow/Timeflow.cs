// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is the core of Timeflow which controls all timing and animated behaviors. Since this class is
    /// necessarily quite large, it has been split across multiple partial class files to make it a bit
    /// more manageable. 
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/user-guide/timeflow-editor")]
    sealed public partial class Timeflow : TimeflowGroup
    {
        #region SERIALIZED

        [TimeflowIgnore]
        public TimeflowPreferences Preferences;

        [TimeflowIgnore]
        public bool LoopInParent;

        [TimeflowIgnore]
        public GameObject HideObjectOnPlay;

        [TimeflowIgnore]
        public AudioTrack Audio;

        #endregion

        #region NON-SERIALIZED

        [NonSerialized]
        public List<Timeflow> TimeflowChildren;

        [NonSerialized]
        public bool IsReady;

        [NonSerialized]
        public bool IsSetupRequired = true;

        #endregion

        #region ACCESSORS

        public bool HasTimeflowParent => TimeflowParent != null;

        public Timeflow Parent {
            get {
                return TimeflowParent;
            }
            set {
                if (TimeflowParent == value) return;
                TimeflowParent = value;

                // Don't allow assignment to self
                if (TimeflowParent == this) {
                    TimeflowParent = null;
                    return;
                }

                if (TimeflowParent != null) {
                    // Prevent circular loops by ensuring the target isn't a parent up the chain. This should never happen
                    // but is checked as a precaution in case Parent is assigned incorrectly.
                    int overflow = 0;
                    bool circular = false;
                    Timeflow t = TimeflowParent.TimeflowParent;
                    while (t) {
                        if (t == this) {
                            circular = true;
                            Debug.LogError($"Circular Loop Error:{name}");
                            break;
                        }
                        t = t.TimeflowParent;

                        overflow++;
                        if (overflow > 99999) {
                            Debug.LogError("Overflow Error: TimeflowUI.Parent: Circular loop");
                            break;
                        }
                    }
                    if (circular) TimeflowParent = null;
                }
            }
        }

        public override Timeflow TimeflowParent {
            get {
                return _TimeflowParent;
            }
            set {
                if (_TimeflowParent != value) {
                    if (value == null) {
                        _TimeflowParent = null;
                    }
                    else
                    if (ObjectUtil.IsDescendant(value.gameObject, gameObject)) {
                        if (!_shownHierarchyWarning) {
                            _shownHierarchyWarning = true;
                            Debug.LogWarning($"'{value.name}' is a descendant of '{name}' and cannot be set as its parented. Please check your object " +
                                $"hierarchy to ensure that each TimeflowObject has a Timeflow parent that is not a child or descendant of this game object.", gameObject);
                        }
                        _TimeflowParent = null;
                    }
                    else {
                        //Debug.Log($"{name}.{GetType().Name}._TimeflowParent:{value}", gameObject);
                        _TimeflowParent = value;
                    }
                }
            }
        }

        public int NestedDepth()
        {
            int depth = 0;
            Timeflow t = this;
            while (t.TimeflowParent != null && t.TimeflowParent != this) {
                t = t.TimeflowParent;
                depth++;
            }
            return depth;
        }

        public bool IsActive {
            get {
                return Active == this;
            }
            set {
                enabled = true;
                Active = this;
                gameObject.SetActive(true);
            }
        }

        public bool IsAudioSynced => Audio != null && Audio.ForceSync;

        #endregion

        #region SETUP

        /// <summary>
        /// If a new Timeflow instance wakes up while one already exists in the scene (presumbably by async
        /// loading a new scene), then the new Timeflow will take over as the current active instance and
        /// the old one will be destroyed.
        /// </summary>
        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].OnAwake");

            IsAwake = true;

            if (Active == null) {
                // The first root level Timeflow is the active instance by default
                if (IsCurrentlyActive) Active = this;

                if (WorkAreaStart == WorkAreaEnd) {
                    WorkAreaEnd = WorkAreaStart + 10f;
                    WorkAreaEnabled = false;
                }
            }

            base.OnAwake(); // Perform after above operation to init view
            RegisterInstance(this);
        }

        protected override void OnDestruct()
        {
            if (Active == this) Active = null;

            UnregisterInstance(this);
            IsReady = false;

            if (DirectorSyncEnabled && Director != null) {
                Director.played -= OnPlayableDirectorPlayed;
            }

            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].IsReady:" + IsReady);
            base.OnDestruct();
        }

        private void OnValidate()
        {
            enabled = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsAwake) return;
            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].OnEnable");

            if (Instances == null) {
                Timeflow.RegisterAllInstances(true);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorApplication.update -= EditorSyncUpdateAll;
                EditorApplication.update += EditorSyncUpdateAll;
            }
#endif
            AddAndRemoveObjects();

            SetupPreferences();

            Track?.UpdateAutoFullLength();
#if UNITY_EDITOR
            EditorSetup();
#endif
        }

        protected override void OnDisable()
        {
            //if(DebugEnabled) Debug.Log("Timeflow[" + name + "].OnDisable");
            if (!enabled) enabled = true;
            base.OnDisable();
#if UNITY_EDITOR
            EditorApplication.update -= EditorSyncUpdateAll;
#endif
#if UNITY_EDITOR
            EditorSetdown();
#endif
        }

        protected override void OnStart()
        {
            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].OnStart");

            _SetupObjectInstances();
            if (IsCurrentlyActive) {
                if (Application.isPlaying && WorkAreaDisableOnStart) {
                    WorkAreaEnabled = false;
                }
                StartPlayback();
            }
        }

        private void _SetupObjectInstances()
        {
            if (HasTimeflowParent && !IsActive) return; // Only perform for root level instance
            List<TimeflowObject> objects = TimeflowObject.GetAllInstances();
            TimeflowObject.SetupInstances(objects);
        }

        /// <summary>
        /// Preferences are available in the editor and can be modified in the the Preferences window. In a
        /// build, preferences are included as a scriptable object refereneced by all Timeflow instances.
        /// It is assumed and expected that one set of preferences affect all instances, however Timeflow
        /// instances in separate scenes could potentially reference a different preference object, so long
        /// as it is the only instance loaded.
        /// </summary>
        private void SetupPreferences()
        {
#if UNITY_EDITOR
            // Store a reference to prefences so that it is available at runtime
            Preferences = TimeflowPreferences.LoadOrCreateSettings();
#else
            // Preferences should already be stored, but if not get the current or default settings
            if(Preferences == null) Preferences = TimeflowPreferences.LoadOrCreateSettings();
#endif
            // Ensures that the current global preferences are the same
            TimeflowPreferences.Current = Preferences;
        }

        private void StartPlayback()
        {
            if (IsReady || HasTimeflowParent) return; // already setup
            IsReady = true;
            if (IsSetupRequired) {
                RunSetup();
            }

            if (Markers != null) {
                Markers.SetupMarkers();
            }
            if (Application.isPlaying && PlayFromStart) {
                SetTime(StartTime);
                if (Audio != null) Audio.Rewind();
            }

            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].OnAwake: StartTime:" + CurrentTime);
            SceneStartedAtTime = CurrentTime;

            StartPlaybackGroups();
            OnUpdate();

            if (Application.isPlaying) {
                if (AutoPlay) {
                    Play();
                }
            }
            else {
                IsPlaying = false;
                if (Audio != null) Audio.StopAudio();
            }
        }

        public override void OnPlay()
        {
            if (CanSetGlobalTimeScale) {
                Time.timeScale = GlobalTimeScale;
            }

            base.OnPlay();
        }

        public void RunSetup()
        {
            IsSetupRequired = false;

            // Ensure that a TimeflowObject component isn't on the same object
            TimeflowObject[] objects = gameObject.GetComponents<TimeflowObject>();
            if (objects.Length > 1) {
                foreach (TimeflowObject obj in objects) {
                    if (obj is Timeflow t) continue;
                    DestroyImmediate(obj);
                }
            }

            //CheckParent(true);

            // Children in this context refers to nest Timeflow instances
            if (Instances == null) {
                RegisterAllInstances(true);
            }
            else
            if (Instances.Count > 1) {
                TimeflowChildren = new List<Timeflow>();
                foreach (Timeflow t in Instances) {
                    if (t == this || t == null) continue;
                    if (t.TimeflowParent == this) {
                        TimeflowChildren.Add(t);
                        t.RunSetup();
                    }
                }
                if (TimeflowChildren.Count == 0) TimeflowChildren = null;
            }
            else {
                TimeflowChildren = null;
            }
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false); // Timeflow itself does not have any channels to copy

#if UNITY_EDITOR
            Displays = null;
#endif
            MarkerList = null;
        }

        #endregion

        public override float TrackDuration {
            get {
                return EndTime;
            }
            set {
                EndTime = value;
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            OnUpdateAutoFullLength();
        }

        public override void OnUpdateAutoFullLength()
        {
            base.OnUpdateAutoFullLength();
            if (Track != null && Track.Keys != null && Track.Keys.Count > 0) {
                Track.Keys[0].Channel = Track;

                // Timeflow tracks are linked to the start an end times
                if (Track.AutoFullLength) {
                    StartTime = Track.Keys[0].KeyTime;
                    EndTime = Track.Keys[0].KeyValue;
                }
                else {
                    Track.Keys[0].KeyTime = StartTime;
                    Track.Keys[0].LockValue = false;
                    Track.Keys[0].KeyValue = EndTime;
                }
                //Debug.Log($"Timeflow.SetupChannels: StartTime:{StartTime} EndTime:{EndTime} Track.Keys[0].KeyValue:{Track.Keys[0].KeyValue} IsTrack:{Track.Keys[0].IsTrack}", gameObject);
            }
        }

        /// <summary>
        /// This may be called after undo or other operations to give channels a chance to clean up any
        /// extraneous data. This method checks the channel links for any orphans and removes them. This
        /// can occur after undo because the LinkedFrom list is not serialized.
        /// </summary>
        public override void CleanUp()
        {
#if UNITY_EDITOR
            if (IsActive && View != null && View.Display != null && View.Display.Objects != null && View.Display.Objects.Count > 0) {
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj == null || obj.gameObject == gameObject) continue;
                    obj.CleanUp();
                }
            }
#endif
            base.CleanUp();
        }

    }

}//AxonGenesis

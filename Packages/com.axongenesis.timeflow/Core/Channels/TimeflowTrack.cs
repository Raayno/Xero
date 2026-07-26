// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeflowTrack")]
    sealed public partial class TimeflowTrack : TimeflowChannel
    {
        #region PUBLIC

        public enum VisibilityModes
        {
            On,
            Activate,
            Renderer,
            RendererIndependent,
            ActivateChildren,
            OnSelfOnly
        }

        private const float _TimeRounding = 1000f;


        [SerializeField]
        [FormerlySerializedAs("VisibilityMode")]
        private VisibilityModes _VisibilityMode = VisibilityModes.On;


        public List<GameObject> ViewObjects;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private bool _AutoFullLength = true;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private List<Renderer> renderers;

        [NonSerialized]
        private List<Collider> colliders;

        [NonSerialized]
        private List<Light> lights;

        [NonSerialized]
        private List<VisualEffect> vfx;

        [NonSerialized]
        private bool isViewingObjects;

        [NonSerialized]
        private bool _wasTrackLastOn = false;
        private bool wasTrackLastOn {
            get {
                return _wasTrackLastOn;
            }
            set {
                _wasTrackLastOn = value;
                //if(DebugEnabled) Debug.Log($"{Object.name} wasTrackLastOn:{_wasTrackLastOn}");
            }
        }

        [NonSerialized]
        private bool wasTrackLastOnFirst = true;

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private bool _isTrackOnCached = false;

        [NonSerialized]
        private float _isTrackOnTime = 0;

        #endregion

        #region CONSTRUCTOR

        public TimeflowTrack(TimeflowObject parent) : base(parent) { }

        #endregion

        #region ACCESSOR
        public override string Name {
            get {
                return "Track";
            }
            set {
                // Do nothing, track name is always the object name
            }
        }
        public override bool IsEnabled {
            get {
                return Object != null && Object.Enabled;
            }
            set {
            }
        }

        public override bool IsTrack {
            get {
                return true;
            }
        }

        public override bool IsNameCustom {
            get {
                return true;
            }
            set {
                return;
            }
        }

        public bool AutoFullLength {
            get {
                return _AutoFullLength;
            }
            set {
                if (_AutoFullLength != value) {
                    SetFullLength(value);
                }
            }
        }

        public VisibilityModes VisibilityMode {
            get {
                return _VisibilityMode;
            }
            set {
                if (_VisibilityMode != value) {
                    _VisibilityMode = value;
                    //if (DebugEnabled) Debug.Log($"{Object.name} VisibilityMode:{_VisibilityMode}");
                    Object.SetupChannels(true);
                }
            }
        }

        /// <summary>
        /// Tracks do not use the channel time offset and instead link with the parent TimeflowObject
        /// </summary>
        public override float TimeOffsetWorld {
            get {
                float t = 0;
                if (Object != null) {
                    t += Object.TimeOffsetWorld;
                }
                return t;
            }
            set {
                if (Object != null) {
                    Object.TimeOffsetWorld = value;
                }
            }
        }

        public override bool IsLoopSupported {
            get {
                return false;
            }
        }

        public override float TimeScale {
            get {
                return 1f;
            }
            set {
                // Tracks time is always 1:1 with the parent object
            }
        }

        public float StartTime {
            get {
                if (Keys != null && Keys.Count > 0) {
                    return Keys[0].KeyTime;
                }
                return 0f;
            }
            set {
                if (Keys != null && Keys.Count > 0) {
                    Keys[0].KeyTime = value;
                }
            }
        }

        public float EndTime {
            get {
                if (Keys != null && Keys.Count > 0) {
                    return Keys[Keys.Count - 1].KeyValue;
                }
                return Timeflow.EndTime;
            }
            set {
                if (Keys != null && Keys.Count > 0) {
                    Keys[Keys.Count - 1].KeyValue = value;
                }
            }
        }

        #endregion

        public override void OnSetup(TimeflowBehavior parent)
        {
            ToProperty = null;
            wasTrackLastOnFirst = true;

            base.OnSetup(parent);
            GatherRenderers(parent.gameObject);
        }

        public override void SetupKeyframes()
        {
            CanAddRemoveKeys = true;
            base.SetupKeyframes();
            UpdateAutoFullLength();
        }

        public void SetFullLength(bool isOn = true, bool resetTimeOffset = false)
        {
#if UNITY_EDITOR
            bool changed = _AutoFullLength != isOn;
            _AutoFullLength = isOn;
            //Debug.Log($"{Object.name}.AutoFullLength:{_AutoFullLength}");
            if (isOn) {
                if (resetTimeOffset) TimeOffset = 0f;
                UpdateAutoFullLength();
                if (changed && TimeflowPreferences.Current.TrackAutoLock) {
                    Keys[0].LockTime = Keys[0].LockValue = true; // lock to prevent user changes
                }
            }
            Object.OnUpdateAutoFullLength();
#endif
        }

        public void UpdateAutoFullLength()
        {
            if (!AutoFullLength) return;
            if (Timeflow.Active == null) return;
            if (Object == null) {
                // May be null during setup
                return;
            }

            // Fit the track to the parent object's time range
            TimeflowObject parent = Behavior.ParentObject == null ? null : Behavior.ParentObject;
            if (parent == null) return;

            Timeflow.BypassTimeScope = true;

            // Get the world start and end time of the parent object
            float start = parent.StartTimeWorld;
            float end = parent.EndTimeWorld;
            //if (parent.DebugEnabled) Debug.Log($"{parent.name} StartTime:{parent.StartTime}: range:{start}-{end}");

            // When the object is a Timeflow instance, use its start and end time
            if (Object is Timeflow timeflow) {
                if (timeflow.Parent != null) {
                    start = timeflow.Parent.StartTime;
                    end = timeflow.Parent.EndTime;
                }
                else {
                    start = timeflow.StartTime;
                    end = timeflow.EndTime;
                }
                //if (parent.DebugEnabled) Debug.Log($"{parent.name} timeflow:{timeflow.name} range:{start}-{end}", timeflow);
            }
            else
            if (parent.CanDragTimeOffset) {
                // Align the track to 0 in local time
                end = parent.StartTime + (end - start);
                start = parent.StartTime;
                //if (parent.DebugEnabled) Debug.Log($"{parent.name} CanDragTimeOffset:{parent.StartTime}: range:{start}-{end}");
            }

            end *= TimeScaleWorld;
            start *= TimeScaleWorld;

            //Debug.Log($"{parent.name} UpdateAutoFullLength:{_AutoFullLength}:{start}-{end} TimeScaleWorld:{TimeScaleWorld}", Behavior.ParentObject);

            if (Keys.Count == 0) {
                Keys = new List<Keyframe>();
                KeysAdd(new Keyframe());
            }
            if (Keys.Count > 1) {
                Keys.RemoveRange(1, Keys.Count - 1);
            }
            Keys[0].Channel = this;

            bool changed = false;
            if (!Mathf.Approximately(Keys[0].KeyTime, start)) {
                changed = true;
                Keys[0].SetKeyTimeExplicit(start);
            }
            if (!Mathf.Approximately(Keys[0].KeyValue, end)) {
                changed = true;
                Keys[0].SetKeyValueExplicit(end);
            }
            //Debug.Log($"{Object.name} start:{start} end:{end}", Object);
            if (changed) Keys[0].ValueChanged();

            _AutoFullLength = true; // Is triggered off by value change

            Timeflow.BypassTimeScope = false;
        }

        public bool IsTrackOn(float localTime) { return IsTrackOn(localTime, false); }

        public override bool IsTrackOn(float localTime, bool debug)
        {
            bool visible = true;
            if (_isTrackOnTime != 0 && _isTrackOnTime == localTime) {
                //if (DebugEnabled) Debug.Log($"{Object.name} _isTrackOnCached:{_isTrackOnCached} at {localTime}");
                return _isTrackOnCached;
            }
            if (IsEnabled && IsTrack && Keys != null && Keys.Count > 0) {
                float ltime = LoopTime(localTime);
                int ltimeInt = (int)Mathf.RoundToInt(ltime * _TimeRounding);
                Keyframe keyA = null;
                foreach (Keyframe k in Keys) {
                    int keyTime = (int)Mathf.RoundToInt(k.KeyTime * _TimeRounding);
                    if (k.IsKeyEnabled && ltimeInt >= keyTime && ltime < k.KeyValue) {
                        keyA = k;
                        break;
                    }
                }
                if (keyA != null) {
                    visible = true;
                }
                else {
                    visible = false;
                }
                if (VisibilityMode != VisibilityModes.RendererIndependent) {
                    if (Object.HasParentContainer && Object.Track.VisibilityMode != VisibilityModes.OnSelfOnly) {
                        float parentLocalTime = Object.ParentContainer.Track.LocalTime(WorldTime(localTime));
                        if (!Object.ParentContainer.IsGroup && !Object.ParentContainer.Track.IsTrackOn(parentLocalTime, debug)) {
                            //if (DebugEnabled) Debug.Log($"{Object.name} Parent Blocked:{Object.ParentContainer.name}");
                            visible = false;
                        }
                    }
                }
                _isTrackOnCached = visible;
            }

            //if(DebugEnabled) Debug.Log($"{Object.name} IsTrackOn:{visible} at {localTime}");
            _isTrackOnTime = localTime;
            return visible;
        }

        public void OnQueue()
        {
            wasTrackLastOn = false;
            wasTrackLastOnFirst = true;
        }

        public override float InterpolateValue(float time, bool apply, bool isLocalTime)
        {
            if (!isLocalTime) time -= Object.TimeOffsetWorld;
            time = LoopTime(time);
            float value = 0f;

            bool debug = Behavior.DebugEnabled;
            if (IsTrackOn(time)) {
                value = 1f;
                //Debug.Log($"{Object.name} InterpolateValue:{value} at {time} VisibilityMode:{VisibilityMode} apply:{apply} wasTrackLastOn:{wasTrackLastOn} wasTrackLastOnFirst:{wasTrackLastOnFirst}");
                if (apply && (!wasTrackLastOn || wasTrackLastOnFirst)) {
                    if (ViewObjects != null && isViewingObjects == false && ViewObjects.Count > 0) {
                        if (!Timeflow.Active.WorkAreaEnabled || !Timeflow.Active.WorkAreaLocked) {
                            isViewingObjects = true;
#if UNITY_EDITOR
                            Timeflow.Active.View.Display.DisplayHierarchies(ViewObjects);
                            if (!Application.isPlaying && AutoSetWorkArea && !Timeflow.Active.WorkAreaLocked) {
                                Keyframe k = GetPrevKey(time);
                                if (k != null) {
                                    Timeflow.Active.SetWorkArea(k.KeyTime, k.KeyValue, false);
                                }
                            }
#endif
                        }
                        else {
                            isViewingObjects = false;
                        }
                    }

                    if (VisibilityMode == VisibilityModes.Activate) {
                        if (!Behavior.gameObject.activeSelf) {
                            Behavior.gameObject.SetActive(true);
                        }
                    }
                    else
                    if (VisibilityMode == VisibilityModes.ActivateChildren) {
                        if (Behavior.transform.childCount > 0) {
                            ObjectUtil.SetChildrenActive(Behavior.gameObject, true);
                        }
                    }
                    else
                    if (VisibilityMode == VisibilityModes.Renderer || VisibilityMode == VisibilityModes.RendererIndependent) {
                        Behavior.OnRenderEnable();
                        EnableRenderers(true);
                    }

                    Behavior.OnTrackStart();
                    if (apply) wasTrackLastOn = true;
                }
            }
            else {
                //if(DebugEnabled) Debug.Log($"{Object.name} InterpolateValue:{value} at {time} VisibilityMode:{VisibilityMode} apply:{apply} wasTrackLastOn:{wasTrackLastOn} wasTrackLastOnFirst:{wasTrackLastOnFirst}");
                if (apply && (wasTrackLastOn || wasTrackLastOnFirst)) {
                    isViewingObjects = false;
                    if (VisibilityMode == VisibilityModes.Activate) {
                        if (Behavior.gameObject.activeSelf) {
                            Behavior.gameObject.SetActive(false);
                        }
                    }
                    else
                    if (VisibilityMode == VisibilityModes.ActivateChildren) {
                        if (Behavior.transform.childCount > 0) {
                            ObjectUtil.SetChildrenActive(Behavior.gameObject, false);
                        }
                    }
                    else
                    if (VisibilityMode == VisibilityModes.Renderer || VisibilityMode == VisibilityModes.RendererIndependent) {
                        EnableRenderers(false);
                    }
                    if (apply) {
                        Behavior.OnTrackEnd();
                    }
                    if (apply) wasTrackLastOn = false;
                }
            }
            if (apply) {
#if UNITY_EDITOR
                if (Timeflow.Active != null && Timeflow.Active.Input != null && Timeflow.Active.Input.IsScrubbing) {
                    wasTrackLastOnFirst = true;
                }
                else {
                    wasTrackLastOnFirst = lastTime > time;
                }
#else
					wasTrackLastOnFirst = lastTime > time;
#endif
                lastTime = time;
            }
            return value;
        }

        public void GatherRenderers(GameObject obj)
        {
            renderers = new List<Renderer>();
            colliders = new List<Collider>();
            lights = new List<Light>();
            vfx = new List<VisualEffect>();
            if (VisibilityMode == VisibilityModes.Renderer || VisibilityMode == VisibilityModes.RendererIndependent) {
                GatherRenderersRecursive(obj);
            }
        }

        private void GatherRenderersRecursive(GameObject obj)
        {
            if (obj != null) {
                Renderer renderer;
                if (obj.TryGetComponent<Renderer>(out renderer)) {
                    renderers.Add(renderer);
                }
                Collider collider;
                if (obj.TryGetComponent<Collider>(out collider)) {
                    colliders.Add(collider);
                }
                Light light;
                if (obj.TryGetComponent<Light>(out light)) {
                    lights.Add(light);
                }
                VisualEffect visualEffect;
                if (obj.TryGetComponent<VisualEffect>(out visualEffect)) {
                    vfx.Add(visualEffect);
                }

                foreach (Transform child in obj.transform) {
                    bool skip = false;
                    TimeflowObject tobj;
                    child.TryGetComponent<TimeflowObject>(out tobj);
                    if (tobj != null && tobj.Enabled && tobj.Track != null && tobj.Track.VisibilityMode != VisibilityModes.On && tobj.Track.VisibilityMode != VisibilityModes.OnSelfOnly) {
                        // Allow each TimeflowObject to manage its own visibility
                        skip = true;
                    }
                    if (!skip) {
                        GatherRenderersRecursive(child.gameObject);
                    }
                }
            }
        }

        public void EnableRenderers(bool show)
        {

            if (renderers != null && renderers.Count > 0) {
                foreach (Renderer r in renderers) {
                    if (r != null && r.enabled != show) {
                        r.enabled = show;
                    }
                }
            }
            if (colliders != null && colliders.Count > 0) {
                foreach (Collider c in colliders) {
                    if (c != null && c.enabled != show) c.enabled = show;
                }
            }
            if (lights != null && lights.Count > 0) {
                foreach (Light c in lights) {
                    if (c != null && c.enabled != show) c.enabled = show;
                }
            }
            if (vfx != null && vfx.Count > 0) {
                foreach (VisualEffect c in vfx) {
                    if (c != null && c.enabled != show) c.enabled = show;
                }
            }
        }

        /// <summary>
        /// Finds and splits each track that is intersected by the time specified.
        /// </summary>
        public List<Keyframe> SplitTrackAtTime(float time, bool isLocalTime)
        {
            if (!isLocalTime) {
                time -= Object.TimeOffsetWorld;
            }
            List<Keyframe> newKeys = null;
            List<Keyframe> tracks = new List<Keyframe>();
            foreach (Keyframe k in Keys) {
                if (k.KeyTime < time && k.KeyValue > time) {
                    tracks.Add(k);
                }
            }
            if (tracks.Count == 0) {
                //Debug.Log(Name + " No tracks to split");
            }
            else {
                newKeys = new List<Keyframe>();
                foreach (Keyframe k in tracks) {
                    float endTime = k.KeyValue;
                    k.LockValue = false;
                    k.KeyValue = time;

                    Keyframe split = SetKey(time, endTime, true);

                    newKeys.Add(split);
                }
            }
            return newKeys;
        }

        /// <summary>
        /// Joins tracks to create a single track section.
        /// </summary>
        /// <param name="first">The first keyframe which will be modified and remain as the final keyframe</param>
        /// <param name="selected">Keyframes included in the join operation. At least 1 keyframe must be in the list</param>
        /// <param name="endTime">If an end time is provided, all keyframes from the start to end are merged into one. 
        /// Otherwise, only the first listed keyframes are merged.</param>
        public void JoinTracks(Keyframe first, List<Keyframe> selected, float endTime = 0)
        {
            if (first == null || selected == null || selected.Count == 0) return;

            if (endTime == 0) {
                // Get the end time of the selected keys
                endTime = first.KeyValue;
                foreach (Keyframe k in selected) {
                    if (endTime < k.KeyValue) {
                        endTime = k.KeyValue;
                    }
                }
            }

            if (endTime > 0) {
                // Find any other keyframes within the time range from the first to end
                foreach (Keyframe k in Keys) {
                    if (k != null && !selected.Contains(k)) {
                        if (k.KeyTime >= first.KeyTime && k.KeyValue <= endTime) {
                            selected.Add(k);
                        }
                    }
                }
            }
#if UNITY_EDITOR
            UndoUtil.Undo(first.Behavior, "Join Tracks", true);
#endif
            first.LockValue = false;
            first.KeyValue = endTime;
            foreach (Keyframe k in selected) {
                if (k != first) {
                    k.Channel.KeysRemove(k);
                }
            }
        }

        public Keyframe GetSolidTrack(float start, float end, bool isLocalTime)
        {
            Keyframe track = null;
            if (!isLocalTime) {
                start -= Object.TimeOffsetWorld;
                end -= Object.TimeOffsetWorld;
            }
            foreach (Keyframe k in Keys) {
                if (k.KeyTime <= start && k.KeyValue >= end) {
                    track = k;
                    break;
                }
            }
            return track;
        }

        public override void InsertTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
            if (!AutoFullLength) {
                Keyframe track = GetSolidTrack(start, end, isLocalTime);
                if (track == null) {
                    SplitTrackAtTime(start, isLocalTime);
                }
                else {
                    if (track.KeyTime > start) {
                        // Offset the track by the inserted amount of time
                        track.KeyTime = track.KeyTime + (end - start);
                        track.KeyValue = track.KeyValue + (end - start);
                    }
                    else {
                        // Extend the end of the track
                        track.KeyValue = track.KeyValue + (end - start);
                    }
                }
                base.InsertTimeRange(start, end, isLocalTime, isGlobal);
            }
        }

        public override void DuplicateTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
            if (!AutoFullLength) {
                Keyframe track = GetSolidTrack(start, end, isLocalTime);
                if (track == null) {
                    SplitTrackAtTime(end, isLocalTime);
                    SplitTrackAtTime(start, isLocalTime);
                }
                base.DuplicateTimeRange(start, end, isLocalTime, isGlobal);
            }
        }

        public override void ClearTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
            if (!AutoFullLength) {
                SplitTrackAtTime(start, isLocalTime);
                SplitTrackAtTime(end, isLocalTime);
                base.ClearTimeRange(start, end, isLocalTime, isGlobal);
            }
        }

        public override void DeleteTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
            if (!AutoFullLength) {
                SplitTrackAtTime(end, isLocalTime);
                SplitTrackAtTime(start, isLocalTime);
                base.DeleteTimeRange(start, end, isLocalTime, isGlobal);
            }
        }

        /// <summary>
        /// Compares all track keys to find any overlapping ones that need to be split into multiple. Since
        /// this modifies the key list, it can only perform one split at a time and needs to be repeated
        /// until all splits have been cleared (returns false)
        /// </summary>
        /// <returns>True if splits were found, ortherwise false if no action required</returns>
        private bool ResolvingTrackSplits(List<Keyframe> selected = null)
        {
            if (Keys.Count < 2) return false;

            bool wasModified = false;
            Keyframe keyInserted = null;

            foreach (Keyframe keyA in Keys) {
                foreach (Keyframe keyB in Keys) {
                    if (keyA != keyB) {
                        /// Assumes duplicates have been removed but keys might still have same start time
                        if (keyA.KeyTime >= keyB.KeyTime && keyA.KeyValue < keyB.KeyValue) {
                            if (selected != null && selected.Contains(keyB) && !selected.Contains(keyA)) {
                                /// Preserve the selected keys first. If the user drags a larger track to
                                /// replace smaller ones, the latter should be removed
                                KeysRemove(keyA);
                            }
                            else {
                                /// keyA overlaps and is inside the keyB range so we need to split it No
                                /// need to check the opposite condition since all keys are compared
                                keyInserted = new Keyframe(keyB);
                                keyInserted.KeyTime = keyA.KeyValue;
                                keyInserted.KeyValue = keyB.KeyValue;

                                /// preserve keyA as it is but truncate keyB up to keyA's start
                                keyB.KeyValue = keyA.KeyTime;
                            }
                            wasModified = true;
                            break;
                        }
                    }
                }
                if (wasModified) break;
            }

            if (wasModified) {
                /// The new key is automatically inserted when created. Re-sort and then rerun this
                /// operation
                Keys.Sort(KeyframeSort.ByTimeAsc);
            }
            //Debug.Log($"ResolvingTrackSplits:{wasModified}");

            return wasModified;
        }

        /// <summary>
        /// This shores up the tracks so they do not overlap. Instead of removing duplicates, this splits
        /// tracks so that they do not overlap. Nothing is deleted.
        /// </summary>
        public override List<Keyframe> ClearDuplicateKeys(List<Keyframe> selected = null)
        {
            // Remove any invalid
            List<Keyframe> dups = new List<Keyframe>();
            foreach (Keyframe k in Keys) {
                if (k.KeyTime >= k.KeyValue) {
                    Debug.LogWarning("Invalid Key:" + k.KeyTime + " v:" + k.KeyValue);
                    dups.Add(k);
                }
            }

            // Resolve or remove any keys occupying the same time
            foreach (Keyframe keyA in Keys) {
                foreach (Keyframe keyB in Keys) {
                    if (keyA != keyB) {
                        if (!MathUtil.IsTimeDifferent(keyA.KeyTime, keyB.KeyTime) && !MathUtil.IsTimeDifferent(keyA.KeyValue, keyB.KeyValue)) {
                            /// Tracks have same start and end time and are true duplicates
                            if (selected != null && selected.Contains(keyB)) {
                                dups.Add(keyA);
                            }
                            else {
                                dups.Add(keyB);
                            }
                        }
                    }
                }
            }
            if (dups.Count > 0) {
                foreach (Keyframe dup in dups) {
                    KeysRemove(dup);
                }
                TangentsNeedUpdate = true;
            }

            /// Repeat operation until it returns false, indicating no more splits
            int count = 0;
            while (ResolvingTrackSplits(selected)) {
                count++;
                if (count > 999) {
                    Debug.LogWarning("Infinite loop detected");
                    break;
                }
            }

            // Rebuild the list to remove any duplicate instances of the same key
            List<Keyframe> keys = new List<Keyframe>();
            foreach (Keyframe k in Keys) {
                if (!keys.Contains(k)) {
                    keys.Add(k);
                }
            }
            Keys = keys;

            // Sort the keys so they are ordered in time ascending
            Keys.Sort(KeyframeSort.ByTimeAsc);

            // Shore up the track ends to prevent any overlap
            for (int i = 0; i < Keys.Count - 1; i++) {
                Keyframe a = Keys[i];
                Keyframe b = Keys[i + 1];

                if (a.KeyValue > b.KeyTime) {
                    if (selected != null && selected.Contains(a)) {
                        // favor the selected key
                        b.KeyTime = a.KeyValue;
                    }
                    else {
                        a.KeyValue = b.KeyTime;
                    }
                }
            }

            return dups;
        }

        public override Keyframe CopyKey(Keyframe key, float timeOffset = 0f, bool doSetup = true, bool forceCopy = false)
        {
            AutoFullLength = false;

            // Only allow keys to be copied at a different time, so there are no overlapping keys
            float t = key.KeyTime + timeOffset;
            Keyframe k = forceCopy ? null : GetKeyAtTime(t);
            if (k != null) {
                k.Copy(key, this);
                k.KeyTime = t;
                k.KeyValue = key.KeyValue + timeOffset;
            }
            else
            if (CanAddRemoveKeys) {
                List<Keyframe> overwrite = new List<Keyframe>();
                float v = key.KeyValue + timeOffset;
                foreach (Keyframe m in Keys) {
                    if (m.KeyTime < t && m.KeyValue > t) {
                        m.KeyValue = t;
                    }
                    else
                    if (m.KeyTime < v && m.KeyValue > v) {
                        m.KeyTime = v;
                    }
                    else
                    if (m.KeyTime >= t && m.KeyValue <= v) {
                        overwrite.Add(m);
                    }
                }
                if (overwrite.Count > 0) {
                    foreach (Keyframe m in overwrite) {
                        KeysRemove(m);
                    }
                }

                k = Keyframe.Clone(key, this);
                k.SetTrackTime(t, v);

                if (!Keys.Contains(k)) Keys.Add(k);
            }

            if (doSetup) {
                SetupKeyframes();
            }
            return k;
        }

        public override void ClearKeys(bool undoable)
        {
#if UNITY_EDITOR
            if (undoable) UndoUtil.Undo(Behavior, "Clear Keyframes");
#endif
            SetFullLength(true, true);
        }

        public override bool UnsetKey(float localTime)
        {
            if (AutoFullLength) return true;
            if (Keys != null && Keys.Count > 1) {
                /// Only allow keys to be removed if the track has multiple sections - at least 1 is needed
                return base.UnsetKey(localTime);
            }
            return false;
        }

        public override bool UnsetKey(Keyframe key)
        {
            if (AutoFullLength) return true;
            if (Keys != null && Keys.Count > 1) {
                /// Only allow keys to be removed if the track has multiple sections - at least 1 is needed
                return base.UnsetKey(key);
            }
            return false;
        }

#if UNITY_EDITOR

        public bool AutoSetWorkArea;

        #region ACCESSORS

        public bool CanDragTimeOffset {
            get {
                if (Behavior != null) {
                    return Behavior.CanDragTimeOffset;
                }
                return false;
            }
            set {
                if (Behavior != null) {
                    Behavior.CanDragTimeOffset = value;
                }
            }
        }

        /// <summary>
        /// Track channels can only be locked if the parent object is locked.
        /// </summary>
        public override bool IsLocked {
            get {
                if (Object != null) return Object.IsLocked;
                return false;
            }
            set {
                base.IsLocked = false;
            }
        }

        public bool IsTrackLocked {
            get {
                if (Keys.Count > 0) return Keys[0].LockTime;
                return false;
            }
            set {
                if (Keys.Count > 0) {
                    Keys[0].LockTime = value;
                }
            }
        }

        public override bool SupportsKeyframes {
            get {
                return true;
            }
        }

        #endregion

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUITracks()
        {
            if (IsHidden) return;
            bool isGhost = !Object.BehaviorsEnabled;

            float inTime = Timeflow.Active.View.TimeOfPosition(0, true);
            float outTime = Timeflow.Active.View.TimeOfPosition(Timeflow.Active.Layout.TimeAreaInner.Width, true);

            foreach (Keyframe key in Keys) {
                if (key == null || key.Channel == null) {
                    continue;
                }
                float keyTime = key.KeyTimeWorld;
                float keyValue = key.KeyEndTimeWorld;

                if (keyTime > outTime || keyValue < inTime) {
                    continue;
                }

                float inPoint = Timeflow.Active.View.PositionOfTime(keyTime, true);
                float outPoint = Timeflow.Active.View.PositionOfTime(keyTime + (1f / Timeflow.Active.FPS), true);
                if (keyValue > keyTime) {
                    outPoint = Timeflow.Active.View.PositionOfTime(keyValue, true);
                    outPoint -= inPoint;
                }
                key.GUIRect = new GUIRect(inPoint, GUIRect.y, outPoint, GUIHeight - 2);

                if (key.KeyColor.a <= 0f) {
                    key.KeyColor = new Color(key.KeyColor.r, key.KeyColor.g, key.KeyColor.b, 1f);
                }

                string name = key.KeyString;
                bool isLocked = IsLocked || isGhost;
                bool isSelected = !isLocked && Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key);
                bool isRelated = TimeflowView.UseRelatedKeys && Timeflow.Active.View.RelatedTracks != null && Timeflow.Active.View.RelatedTracks.Contains(key);

                // Adjust the color based on status of being locked or selected
                Color white = isGhost ? AxonColor.Ghost : isLocked ? AxonColor.LockedOverlay : AxonColor.Default;
                Color keyColor = white;
                Color color = white;

                if (isRelated) {
                    keyColor = keyColor * AxonColor.RelatedKeys;
                    if (!key.IsKeyEnabled) {
                        keyColor.a = 0.35f;
                    }
                }
                else
                if (key.IsKeyEnabled) {
                    keyColor = key.OverrideGUIColor ? key.KeyColor : key.Channel.GUIColor;
                }
                else {
                    keyColor = Color.gray;
                }
                if (isGhost) {
                    keyColor.a = 0.2f;
                }
                color = MathUtil.Interpolate(keyColor, white, 0.25f);

                if (!isSelected && Timeflow.Active.Input.IsDragging) {
                    // Indicate whether any track sections are being overwritten during a drag operation by slightly ghosting them
                    foreach (Keyframe k in Keys) {
                        if (k != key) {
                            if (MathUtil.OverlapsNoCoincidence(k.KeyTime, k.KeyValue, key.KeyTime, key.KeyValue)) {
                                color.a = 0.35f;
                                break;
                            }
                        }
                    }
                }

                GUI.color = color;
                GUI.Box(key.GUIRect, GUIContent.none, AxonUI.TrackStyle);
                GUI.color = color;

                if (key.IsTrack && key.Channel.Object.Track.AutoFullLength) {
                    GUI.color = Timeflow.Active.Input.IsDragging ? Color.white : AxonColor.ExtraFaded;
                    GUIRect lockRect = key.GUIRect;
                    lockRect.width = lockRect.height = 16;
                    lockRect.x -= 12;
                    GUI.Label(lockRect, "A", AxonUI.SmallLabelStyle);
                }
                if (key.LockTime) {
                    GUI.color = Timeflow.Active.Input.IsDragging ? Color.white : AxonColor.ExtraFaded;
                    GUIRect lockRect = key.GUIRect;
                    lockRect.width = lockRect.height = 16;
                    GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                }

                if (isSelected || Object.IsSelected) {
                    color = key.IsKeyEnabled ? isSelected ? AxonColor.KeySelected : AxonColor.TrackPartialSelection : AxonColor.Default;

                    color.a = isSelected ? TimeflowPreferences.Current.TrackSelectedPattern : TimeflowPreferences.Current.TrackSelectedPattern * 0.75f;
                    // Adds to the regular track box drawn above
                    GUIRect r = key.GUIRect;
                    GUI.color = color;
                    GUI.DrawTextureWithTexCoords(r, AxonUI.TrackSelectedPatternStyle.normal.background, new Rect(0, 0, r.width / 16f, 1f));
                    GUI.Box(r, new GUIContent(), AxonUI.TrackSelectedStyle);

                    if ((isSelected || isRelated) && Timeflow.Active.Input.IsDraggingCopy && Timeflow.Active.Input.DraggingTimeOffset != 0f) {
                        GUIRect r2 = new GUIRect(r);
                        r2.x = Timeflow.Active.View.PositionOfTime(keyTime + Timeflow.Active.Input.DraggingTimeOffset, true);
                        GUI.Box(r2, GUIContent.none, AxonUI.TrackSelectedCopyStyle);
                    }

                    if (key.IsEditingName) {
                        GUI.SetNextControlName("EditObjectName");

                        GUIRect rect = key.GUIRect;
                        Vector2 size = rect.size;
                        if (size.x < 30f) {
                            size.x = 30f;
                        }
                        rect.size = size;
                        GUI.color = AxonColor.Default;
                        GUIStyle labelStyle = new GUIStyle(GUI.skin.textField);
                        labelStyle.alignment = TextAnchor.MiddleLeft;
                        key.TempEditName = GUI.TextField(rect, key.TempEditName, GUI.skin.textField);
                        name = key.TempEditName;
                        if (!wasEditingName) {
                            wasEditingName = true;
                            Timeflow.Active.Input.StartEditingName(key);
                            AxonGUI.FocusControl("EditObjectName");
                            EditorGUI.FocusTextInControl("EditObjectName");
                        }
                    }
                    else wasEditingName = false;
                }

                if (!key.IsEditingName) {
                    GUI.color = GUITextColor;
                    GUI.Box(key.GUIRect, new GUIContent(name), AxonUI.SmallLabelStyle);
                }
                GUI.color = color;

                if (Event.current.control || Event.current.command) {
                    EditorGUIUtility.AddCursorRect(key.GUIRect, MouseCursor.ArrowPlus);
                }

                key.HandleRectLeft = new GUIRect(key.GUIRect.x - 8, key.GUIRect.y, 8, GUIHeight);
                EditorGUIUtility.AddCursorRect(key.HandleRectLeft, MouseCursor.SplitResizeLeftRight);

                key.HandleRectRight = new GUIRect(key.GUIRect.x + key.GUIRect.width - 6, key.GUIRect.y, 8, GUIHeight);
                EditorGUIUtility.AddCursorRect(key.HandleRectRight, MouseCursor.SplitResizeLeftRight);
            }

            GUITrackTicks();
        }

        public override void GUIGraphPass2()
        {
            GUITracksShade(true);
        }

        public void GUITrackTicks()
        {
            if (!TimeflowPreferences.Current.EnableKeyframeTicks) return;
            if (Object.AllChannelsForDisplay != null) {
                GUIRect rect = GUIRect;
                rect.width = 2;
                rect.height = 2 + (GUIHeight / 3);
                rect.y = rect.y + (GUIHeight - rect.height);

                bool isRelated = TimeflowView.UseRelatedKeys && Timeflow.Active.View.RelatedKeys != null;
                foreach (TimeflowChannel ch in Object.AllChannelsForDisplay) {
                    if (!ch.IsTrack && ch.SupportsKeyframes && ch.Keys != null && ch.Keys.Count > 0) {
                        foreach (Keyframe k in ch.Keys) {
                            int x = Timeflow.Active.View.PositionOfTime(k.KeyTimeWorld, true);
                            if (isRelated && Timeflow.Active.View.RelatedKeys.Contains(k)) {
                                rect.x = x - 2;
                                rect.width = 4;
                                GUI.color = AxonColor.RelatedKeys;
                                GUI.Box(rect, "", AxonUI.SolidStyle);
                            }
                            rect.x = x - 1;
                            rect.width = 2;
                            GUI.color = Color.black;
                            GUI.Box(rect, "", AxonUI.SolidStyle);

                        }
                    }
                }
                GUI.color = AxonColor.Default;
            }
        }

        public override void GUIInfoValueChanged(List<Keyframe> selectedKeys)
        {
            Object.OnTrackChange();
        }

        public override void CropTimeRange(float start, float end, bool addEndKeys)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Crop Time", true);
#endif
            start -= TimeOffsetWorld;
            end -= TimeOffsetWorld;

            if (Keys != null && start < end) {
                List<Keyframe> toDelete = new List<Keyframe>();
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime < start && k.KeyValue > start) {
                        k.KeyTime = start; // truncate start
                    }
                    else
                    if (k.KeyTime < end && k.KeyValue > end) {
                        k.KeyValue = end; // truncate to end
                    }
                    else
                    if (k.KeyTime < start && k.KeyValue < start) {
                        toDelete.Add(k);
                    }
                    else
                    if (k.KeyTime > end) {
                        toDelete.Add(k);
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (Keyframe k in toDelete) {
                        KeysRemove(k);
                    }
                }
            }
        }

#endif
    }

}//AxonGenesis

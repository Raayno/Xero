// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// A group represents a sub section of time that controls all time-based behaviors in its hierarchy.
    /// The main purpose is to affect the timing of a whole group of objects in unison. This establishes
    /// the core timeline behavior upon which Timeflow is built and adds the possibility of layering and
    /// grouping time-based behaviors. This component may be but is not typically used directly in a scene,
    /// but rather users should instead create additional instances of Timeflow with each having it's own
    /// local time and timeline view in the Timeflow window.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/timeflow-object")]
    public class TimeflowGroup : TimeflowObject
    {
        #region STATIC VARS

        // If a crash occurs due to a circular loop, enable this to help troubleshooting
        private static bool _CircularLoopInterrupt = false;

        public static TimeflowGroup[] AllGroups { get; private set; }

        public static void RegisterAllGroups(bool force = false)
        {
            if (force || AllGroups == null || AllGroups.Length == 0) {
                if (Application.isPlaying) {
                    AllGroups = UnityEngine.Object.FindObjectsByType(typeof(TimeflowGroup), FindObjectsInactive.Include, FindObjectsSortMode.None) as TimeflowGroup[];
                }
                else {
#if UNITY_EDITOR
                    AllGroups = UnityEngine.Object.FindObjectsByType(typeof(TimeflowGroup), FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<TimeflowGroup>()
                            .Where(go => go.gameObject.scene == EditorSceneManager.GetActiveScene()) as TimeflowGroup[];
#endif
                }
                if (AllGroups != null) {
                    foreach (TimeflowGroup group in AllGroups) {
                        Timeflow.RegisterGroup(group);
                        group.GetObjects();
                    }
                }
            }
        }

        public static void RegisterObject(TimeflowObject obj)
        {
            // Find the TimeflowGroup the object belongs to
            TimeflowGroup grp = ObjectUtil.GetComponentInParent<TimeflowGroup>(obj.gameObject);
            if (grp == null) {
                grp = Timeflow.Active;
            }
            if (grp != null) {
                grp.AddObject(obj);
            }

            if (obj.TryGetComponent<TimeflowObject>(out var t)) {
                // Force the TimeflowObject to regather behaviors
                t.Behaviors = null;
                t.HasBehaviors = false;
            }
        }

        public static void UnregisterObject(TimeflowObject obj)
        {
            if (obj != null) {
                if (obj.Group != null) {
                    obj.Group.RemoveObject(obj);
                }

#if UNITY_EDITOR
                if (obj.Timeflow != null && obj.Timeflow.View != null && obj.Timeflow.View.Display != null) {
                    obj.Timeflow.View.Display.RemoveObjectFromDisplayRecursive(obj.gameObject);
                }
#endif
            }
        }

        #endregion

        #region PUBLIC

        public bool ResetObjects;

        [NonSerialized]
        private List<TimeflowObject> _Objects;
        public List<TimeflowObject> Objects {
            get { return _Objects; }
            set {
                _Objects = value;
                //Debug.Log($"{name}.Objects:{(value == null ? "NULL" : value.Count)}");
            }
        }

        [NonSerialized]
        public bool HasObjects = false;

        #endregion

        #region PRIVATE

        protected List<TimeflowObject> _objectsToAdd;
        protected List<TimeflowObject> _objectsToRemove;

        #endregion

        #region ACCESSORS

        public override float CurrentTime {
            get {
                return base.CurrentTime;
            }
            set {
                if (_CircularLoopInterrupt) {
                    Debug.LogWarning($"{name}.TimeflowGroup.CurrentTime: Circular loop detected. ", gameObject);
                    return;
                }
                //Debug.Log($"{name}.TimeflowGroup.CurrentTime:{value}", gameObject); 
                base.CurrentTime = value;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            base.OnAwake();
            IsAwake = true;
            IsGroup = true;
            Timeflow.RegisterGroup(this);
        }

        protected override void OnDestruct()
        {
            Timeflow.UnregisterGroup(this);
            base.OnDestruct();
        }

        public override void OnStartPlayback()
        {
            base.OnStartPlayback();
            //if (DebugEnabled) Debug.Log("TimeflowGroup[" + name + "].OnTimeflowStart: Objects:" + (Objects == null ? 0 : Objects.Count));
            if (HasObjects && !_CircularLoopInterrupt) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null || obj == this) continue;
                    obj.OnStartPlayback();
                }
            }
        }

        #endregion

        #region OBJECTS

        public void UpdateHasObjects()
        {
            HasObjects = Objects != null && Objects.Count > 0;
        }

        public override void CheckParent(bool force)
        {
            base.CheckParent(force);
            if (gameObject.transform.parent == null) {
                TimeflowParent = null;
            }
            else {
                TimeflowParent = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(gameObject.transform.parent.gameObject);
                //Debug.Log($"{name}.CheckParent: {(TimeflowParent == null ? "NULL" : TimeflowParent.name)}");
            }
        }

        public void GetObjects()
        {
            ResetObjects = false;

            GetAllInstances(true);
            Objects = new List<TimeflowObject>();

            if (AllInstances != null && AllInstances.Count > 0) {
                foreach (TimeflowObject obj in AllInstances) {
                    if (obj != null && obj != this && obj.Timeflow == this && !Objects.Contains(obj)) {
                        //Debug.Log($"{name}.Objects.Add({obj.name})");
                        Objects.Add(obj);
                    }
                }
            }

            _GetObjectsRecursive();

            if (HasObjects && !_CircularLoopInterrupt) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null || obj == this) continue;
                    obj.Group = this;
                }
            }
            //if (DebugEnabled) Debug.Log($"{name}.GetObjects:{Objects.Count}", gameObject);
            UpdateHasObjects();
        }

        protected void _GetObjectsRecursive()
        {
            if (AllInstances != null && AllInstances.Count > 0) {
                foreach (TimeflowObject obj in AllInstances) {
                    if (obj != null && obj != this && obj.Timeflow == this && !Objects.Contains(obj)) {
                        Objects.Add(obj);
                    }
                }
            }

            UpdateHasObjects();
        }

        public void AddObject(TimeflowObject obj)
        {
            if (Objects == null) GetObjects();
            if (obj != null && obj != this && !Objects.Contains(obj)) {
                //Debug.Log($"{name}.AddObject:{obj.name}", obj);
                if (_objectsToAdd == null) _objectsToAdd = new List<TimeflowObject>();

                TimeflowGroup group = obj as TimeflowGroup;
                if (group == null) {
                    obj.Group = this;
                }
                if (!_objectsToAdd.Contains(obj)) {
                    _objectsToAdd.Add(obj);
                }
                UpdateHasObjects();
            }
        }

        public void RemoveObject(TimeflowObject obj)
        {
            if (obj != null && HasObjects && Objects.Contains(obj)) {
                if (_objectsToRemove == null) _objectsToRemove = new List<TimeflowObject>();
                obj.Group = null;
                _objectsToRemove.Add(obj);
                UpdateHasObjects();
            }
        }

        public void AddAndRemoveObjects()
        {
            if (Objects == null || gameObject == null) return;

            Objects = Objects.RemoveNulls().ToList();

            foreach (TimeflowObject obj in Objects) {
                if (obj == null) continue;
                // Detect and remove any circular loops
                if (obj == this || ObjectUtil.IsDescendant(gameObject, obj.gameObject)) {
                    if (_objectsToRemove == null) _objectsToRemove = new List<TimeflowObject>();
                    if (!_objectsToRemove.Contains(obj)) _objectsToRemove.Add(obj);
                }
            }
            if (_objectsToAdd != null) {
                foreach (TimeflowObject obj in _objectsToAdd) {
                    if (obj != null && !Objects.Contains(obj)) {
                        Objects.Add(obj);
                    }
                }
                _objectsToAdd = null;
            }

            if (_objectsToRemove != null && _objectsToRemove.Count > 0) {
                foreach (TimeflowObject obj in _objectsToRemove) {
                    if (obj != null) {
                        Objects.Remove(obj);
                    }
                }
                _objectsToRemove = null;
            }

            UpdateHasObjects();
        }

        public void RewindObjects()
        {
            if (!Enabled || !HasObjects) return;
            if (_CircularLoopInterrupt) return;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj == this || obj.IsGroup) continue;
                bool objEnabled = obj.Enabled && obj.enabled;
                if (objEnabled) {
                    bool canUpdate = obj.gameObject.activeInHierarchy;
                    if (obj.Track.VisibilityMode == TimeflowTrack.VisibilityModes.Activate) {
                        canUpdate = true;
                    }
                    if (canUpdate) {
                        obj.OnRewind();
                    }
                }
            }
        }

        public void UpdateObjects()
        {
            if (!Enabled || !CanUpdate) return;
            if (ResetObjects) {
                GetObjects();
            }
            if (!HasObjects || _CircularLoopInterrupt) return;

            //if (DebugEnabled) Debug.Log(" --- " + name + $".UpdateObjects:{Objects.Count} {CurrentTime}");

            int i = 0;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj == this) continue;
                if (obj.CanUpdate) {
                    //if (DebugEnabled) Debug.Log($" --- - {i} {CurrentTime}: {obj.name} id:{obj.GetInstanceID()}", obj);
                    obj.DoUpdate();
                }
                i++;
            }
        }

        public void RefreshObjects()
        {
            if (!Enabled || !HasObjects || _CircularLoopInterrupt) return;
            //if (DebugEnabled) Debug.Log($"{name}.RefreshGroups:{(Objects.Count)}");
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj.IsGroup) continue;
                obj.Refresh();
            }
        }

        #endregion

        #region UPDATE

        public void LateUpdateObjects()
        {
            if (!Enabled) return;
            AddAndRemoveObjects();

            if (HasObjects && !_CircularLoopInterrupt) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null || obj.IsGroup) continue;
                    if (obj.CanUpdate) {
                        obj.LateUpdateTime();
                    }
                }
            }
        }

        public override void OnFinalUpdate()
        {
            //Debug.Log($"{name}.OnFinalUpdate");
            if (!Enabled) return;
            AddAndRemoveObjects();

            if (HasObjects && !_CircularLoopInterrupt) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null || obj.IsGroup) continue;
                    obj.OnFinalUpdate();
                }
            }
            base.OnFinalUpdate();
        }

        public void FixedUpdateObjects()
        {
            if (!Enabled) return;
            if (HasObjects && !_CircularLoopInterrupt) {
                foreach (TimeflowObject obj in Objects) {
                    if (obj == null || obj.IsGroup) continue;
                    if (obj.CanUpdate) {
                        obj.FixedUpdateBehaviors();
                    }
                }
            }

            if (UseFixedUpdate) UpdateTimeLinked();
        }

        public void UpdateTimingMode()
        {
            if (!HasObjects || _CircularLoopInterrupt) return;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj.IsGroup) continue;
                if (obj.Enabled && obj.Behaviors != null) {
                    foreach (TimeflowBehavior b in obj.Behaviors) {
                        b.OnUpdateTimingMode();
                    }
                }
            }
        }

        #endregion

        #region PLAYBACK

        public override void OnPlay()
        {
            if (!HasObjects || _CircularLoopInterrupt) return;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj.IsGroup) continue;
                if (obj.Enabled) {
                    obj.OnPlay();
                }
            }
        }

        public override void OnStop()
        {
            if (!HasObjects || _CircularLoopInterrupt) return;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj.IsGroup) continue;
                if (obj.Enabled) {
                    obj.OnStop();
                }
            }
        }

        public override float GetTime()
        {
            if (TimeflowParent != null) {
                return TimeflowParent.GetTime() * TimeScaleWorld - TimeOffset;
            }
            else {
                Debug.LogWarning("The TimeflowGroup '" + name + "' does not belong to a Timeflow parent.");
                return 0f;
            }
        }

        public override void SetTime(float time)
        {
            //Debug.Log($"{name}.TimeflowGroup.SetTime:{time}", gameObject);
            base.SetTime(time);
            if (!HasObjects || _CircularLoopInterrupt) return;

            // Only propogate time to unparented objects. All children will inherit time from the parent.
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj == this || !obj.IsOrphaned || !obj.Enabled) continue;
                float t = time - obj.TimeOffset;
                //Debug.Log($"-->{obj.name}.SetTime:{t} IsOrphaned:{obj.IsOrphaned}", gameObject);
                obj.SetTime(t);
            }
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR

        public bool EditorShowObjects;

        public override void OnHierarchyChange()
        {
            base.OnHierarchyChange();
            if (!HasObjects || _CircularLoopInterrupt) return;
            foreach (TimeflowObject obj in Objects) {
                if (obj == null || obj.IsGroup) continue;
                obj.OnHierarchyChange();
            }
        }

#endif
        #endregion
    }

}//AxonGenesis

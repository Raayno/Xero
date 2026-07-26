// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        [TimeflowIgnore]
        [FormerlySerializedAs("RootObjects")]
        [SerializeField] private List<TimeflowObject> _RootObjects;

        public List<TimeflowObject> RootObjects {
            get {
                if (_RootObjects == null) _RootObjects = new List<TimeflowObject>();
                return _RootObjects;
            }
            set {
                if (_RootObjects == value) return;
                _RootObjects = value;
                OnRootObjectsChanged();
            }
        }

        [TimeflowIgnore]
        public TimeflowObject[] RootObjectsCached;

        [NonSerialized]
        private List<TimeflowGroup> Groups;

        [NonSerialized]
        public bool HasGroups = false;

        public void OnRootObjectsChanged()
        {
            //Debug.Log($"{name}.OnRootObjectsChanged");
            RootObjectsCached = RootObjects.ToArray();
        }

        public void AddRootObject(TimeflowObject obj)
        {
            if (obj == null) return;
            if (RootObjects != null && RootObjects.Contains(obj)) return;
            if (obj.gameObject == Timeflow.gameObject) return;
            if (obj.Timeflow != this) return;

            bool canAdd = true;

            // Only display objects that belong to the current timeflow instance
            if (obj.Timeflow == Timeflow) {
                canAdd = true;
            }
            else {
                foreach (TimeflowObject root in RootObjects) {
                    // Only add the object if there is no parent TimeflowObject
                    if (ObjectUtil.IsDescendant(obj.gameObject, root.gameObject)) {
                        if (obj.transform.parent.TryGetComponent<TimeflowObject>(out var pobj)) {
                            canAdd = false;
                            break;
                        }
                    }
                }
            }

            if (canAdd && !RootObjects.Contains(obj)) {
                RootObjects.Add(obj);
                OnRootObjectsChanged();
                //Debug.Log($"AddRootObject:{obj.name}");
            }
        }

        #region GROUPS

        public void AddGroup(TimeflowGroup group)
        {
            if (group == this || group == null) return;
            //if (DebugEnabled) Debug.Log("Timeflow[" + name + "].AddGroup:" + group.name);
            if (Groups == null) Groups = new List<TimeflowGroup>();
            if (!Groups.Contains(group)) {
                Groups.Add(group);
            }
            HasGroups = Groups.Count > 0;
            group.TimeflowParent = this;
        }

        public void RemoveGroup(TimeflowGroup group)
        {
            if (group == this || group == null) return;
            if (Groups != null && Groups.Contains(group)) {
                Groups.Remove(group);
            }
            HasGroups = Groups != null && Groups.Count > 0;
            group.TimeflowParent = null;
        }

        public void StartPlaybackGroups()
        {
            OnStartPlayback();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.OnStartPlayback();
                }
            }
        }

        public void PlayGroups()
        {
            OnPlay();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.OnPlay();
                }
            }
        }

        public void StopGroups()
        {
            OnStop();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.OnStop();
                }
            }
        }

        public void RewindGroups()
        {
            DoUpdate();
            RewindObjects();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.RewindObjects();
                }
            }
            UpdateGroups();
        }

        public void RefreshGroups()
        {
            //Debug.Log($"{name}.RefreshGroups:{(HasGroups ? Groups.Count : "0")}");
            RefreshObjects();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.RefreshObjects();
                }
            }
        }

        public void UpdateGroups()
        {
            if (!CanUpdate) return;
            //if (DebugEnabled) Debug.Log(name + ".UpdateGroups:" + CurrentTime);
            UpdateObjects();
            if (Groups != null && Groups.Count > 0) {
                foreach (TimeflowGroup group in Groups) {
                    //if (DebugEnabled) Debug.Log(" --- " + name + ".UpdateGroup:" + group.name);
                    group.UpdateObjects();
                }
            }
            TimeBehavior.UpdateAll(CurrentFrame);
        }

        public void LateUpdateGroups()
        {
            //Debug.Log($"{name}.LateUpdateGroups: HasGroups:{HasGroups} :{(Groups == null ? "NULL" : Groups.Count)}");
            LateUpdateObjects();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    if (group == null) continue;
                    group.LateUpdateObjects();
                    group.OnFinalUpdate();
                }
            }
            TimeBehavior.LateUpdateAll(CurrentFrame);
        }

        public void FixedUpdateGroups()
        {
            FixedUpdateObjects();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.FixedUpdateObjects();
                }
            }
            TimeBehavior.FixedUpdateAll(CurrentFrame);
        }

        public void UpdateGroupsTimingMode()
        {
            UpdateTimingMode();
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.UpdateTimingMode();
                }
            }
        }

        public void SetGroupsTime(float time)
        {
            if (HasGroups) {
                foreach (TimeflowGroup group in Groups) {
                    group.SetTime(time - group.TimeOffset);
                }
            }
        }

        #endregion
    }

}//AxonGenesis

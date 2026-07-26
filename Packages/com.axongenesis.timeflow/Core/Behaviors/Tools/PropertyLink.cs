// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [AddComponentMenu("Timeflow/Property Link")]
    public class PropertyLink : TimeflowBehavior
    {
        public TimeflowChannel Channel = null;

        public List<Property> Properties = null;

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log(name + ".PropertyLink.OnAwake:" + Name);
            base.OnAwake();
            Reset();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ".PropertyLink.OnDestruct");
            if (Channel != null) {
                base.RemoveChannel(Channel);
            }
            base.OnDestruct();
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup && Channel != null) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + ":PropertyLink.SetupChannels");
            if (Channel == null) {
                Channel = new TimeflowChannel();
            }
            Channel.SupportsKeyframes = false;

            if (Channels == null || !HasChannel(Channel)) {
                Channels = new List<TimeflowChannel> { Channel };
            }
            Channel.SetParent(this);
            Channel.OnSetup(this);

        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":PropertyLink.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#endif
        }

        public virtual void Copy(PropertyLink copy)
        {
            Name = copy.Name;
            Enabled = copy.Enabled;
        }

        #endregion

        #region UPDATE

        public virtual void Reset()
        {
        }

        public override void UpdateTime()
        {
            if (!CanUpdate) return;
            //if (DebugEnabled) Debug.Log(name + ".PropertyLink.UpdateTime:" + CurrentTime);
            if (Channel != null && Channel.ToProperty != null && Properties != null && Properties.Count > 0) {
                foreach (Property property in Properties) {
                    property.CopyValue(Channel.ToProperty);
                }
            }
            base.UpdateTime();
        }

        public override void OnRewind()
        {
            base.OnRewind();
        }

        public Property GetPropertyToCopy()
        {
            Property copy = new Property();
            if (Properties != null && Properties.Count > 0) {
                copy.Copy(Properties[0]);
            }
            else {
                copy.Copy(Channel.ToProperty);
            }
            return copy;
        }

        public void GatherChildren()
        {
            if (transform.childCount == 0) {
                Debug.LogWarning($"The game object {name} has no children to add");
                return;
            }
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Gather Children", true);
#endif
            int i = 0;
            Transform[] children = new Transform[transform.childCount];
            foreach (Transform t in transform) {
                children[i] = t;
                i++;
            }
            AddObjects(children);
        }

        public void AddObjects(Transform[] transforms)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Add Objects", true);
#endif
            Property copy = GetPropertyToCopy();

            if (Properties == null) Properties = new List<Property>();
            foreach (Transform child in transforms) {
                Property property = new Property(copy);
                if (!property.SwitchGameObject(child.gameObject)) {
                    Debug.Log($"The object {child.name} could not be added");//--KEEP
                }
                Properties.Add(property);
            }
        }

        internal void AddProperty()
        {
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Add Property", true);
#endif
            if (Properties == null) Properties = new List<Property>();
            Property prop = GetPropertyToCopy();
#if UNITY_EDITOR
            prop.ShowPropertyObject = true;
#endif
            prop.Comp = transform;
            Properties.Add(prop);
        }

        #endregion

#if UNITY_EDITOR

        public bool EditorShowObjects = true;

        public override Texture2D Icon => AxonUI.Icons.PropertyLink;

        public void AddSelectedObjects()
        {
            UndoUtil.Undo(this, "Resetup Objects", true);
            if (Selection.transforms == null) {
                Debug.LogWarning("No objects are selected");
                return;
            }

            AddObjects(Selection.transforms);
        }

        public void ResetupObjects()
        {
            UndoUtil.Undo(this, "Resetup Objects", true);
            if (Properties == null || Properties.Count < 2) return;
            List<Transform> objects = new List<Transform>();
            List<Property> props = new List<Property>(Properties);
            bool isFirst = true;
            foreach (Property property in props) {
                if (isFirst) {
                    isFirst = false;
                    continue;
                }
                if (property.Comp == null) continue;
                objects.Add(property.Comp.transform);
                Properties.Remove(property);
            }

            if (objects.Count > 0) {
                AddObjects(objects.ToArray());
            }
        }

        public void ClearAll()
        {
            UndoUtil.Undo(this, "Clear All Properties", true);
            Properties = null;
        }

        public void ClearAllKeepFirst()
        {
            UndoUtil.Undo(this, "Clear All Keep First", true);
            if (Properties == null || Properties.Count < 2) return;
            List<Property> props = new List<Property>(Properties);
            bool isFirst = true;
            foreach (Property property in props) {
                if (isFirst) {
                    isFirst = false;
                    continue;
                }
                Properties.Remove(property);
            }
        }
#endif

    }
}

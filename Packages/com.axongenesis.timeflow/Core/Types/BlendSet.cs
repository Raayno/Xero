// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AxonGenesis
{
    /// <summary>
    /// Defines a specific blend target including position, rotation, scale and whether or not it is in
    /// world space. 
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "BlendSet")]
    public class BlendSet : SerializableObject
    {
        #region PUBLIC

        public int ID = -1;
        public string Name = "";

        public enum TransformTypes
        {
            Local,
            Parent,
            World
        }
        public TransformTypes TransformType = TransformTypes.Local;

        /// <summary>
        /// The target Position of the node.
        /// </summary>
        public Vector3 Position = Vector3.zero;
        public bool ApplyPosition = true;

        /// <summary>
        /// The target euler rotation of the node.
        /// </summary>
        public Vector3 Rotation = Vector3.zero;
        public bool ApplyRotation = true;

        /// <summary>
        /// The target local scale of the node.
        /// </summary>
        public Vector3 Scale = Vector3.one;
        public bool ApplyScale = true;

        /// <summary>
        /// Instead of manually entering transform coordinates, you can also specify a specific transform
        /// in the scene to match. When set to true, transforms are always processed in world position.
        /// </summary>
        public bool UseTransform;
        public Transform Transform;

        /// <summary>
        /// Nodes can dynamically change the parent of the target object in both edit and play mode. This
        /// is used in situations where the camera or object affected needs to transfer from one platform
        /// to another. 
        /// </summary>
        public Transform Parent;

        public bool SetFieldOfView;
        public float FieldOfView = 60f;

        public List<PropertyValue> Values;
        public List<bool> Activates;

        public UnityEvent OnExit;
        public UnityEvent OnEnter;

        [NonSerialized]
        public bool HasEntered;

        [NonSerialized]
        public bool HasExited;

        #endregion

        public Rotator Rotator { get; set; }

        #region ACCESSORS

        public bool IsWorld {
            get {
                return TransformType == TransformTypes.World;
            }
        }

        public bool IsLocal {
            get {
                return TransformType == TransformTypes.Local;
            }
        }

        public bool SetParent {
            get {
                return TransformType == TransformTypes.Parent;
            }
        }

        #endregion

        public BlendSet()
        {
            Name = "New Set";
        }

        public BlendSet(BlendSet copy)
        {
#if UNITY_EDITOR
            EditorShow = true;
#endif
            Copy(copy);
        }

        public void Copy(BlendSet copy)
        {
            Name = copy.Name;
            TransformType = copy.TransformType;
            Parent = copy.Parent;
            Position = copy.Position;
            ApplyPosition = copy.ApplyPosition;
            Rotation = copy.Rotation;
            ApplyRotation = copy.ApplyRotation;
            Scale = copy.Scale;
            ApplyScale = copy.ApplyScale;
            UseTransform = copy.UseTransform;
            Transform = copy.Transform;
            SetFieldOfView = copy.SetFieldOfView;
            FieldOfView = copy.FieldOfView;
            OnExit = copy.OnExit;
            OnEnter = copy.OnEnter;

            if(Transform != null) {
                Rotator = Rotator.Setup(Transform);
            }


            if (copy.Values == null || copy.Values.Count == 0) {
                Values = null;
            }
            else {
                Values = new List<PropertyValue>();
                foreach (PropertyValue value in copy.Values) {
                    Values.Add(new PropertyValue(value));
                }
            }

            if (copy.Activates == null || copy.Activates.Count == 0) {
                Activates = null;
            }
            else {
                Activates = new List<bool>();
                for (int i = 0; i < copy.Activates.Count; i++) {
                    Activates.Add(copy.Activates[i]);
                }
            }
        }

        public Vector3 GetPosition(bool world)
        {
            if (UseTransform) {
                if (Transform != null) {
                    if (world) {
                        return Transform.TransformPoint(Position);
                    }
                    else {
                        return Transform.localPosition + Position;
                    }
                }
                else {
                    return Vector3.zero;
                }
            }
            else {
                if (world && TransformType == TransformTypes.Parent && Parent != null) {
                    return Parent.TransformPoint(Position);
                }
                else {
                    return Position;
                }
            }
        }

        public Vector3 GetRotation(bool world)
        {
            if (UseTransform) {
                if (Transform != null) {
                    if (Rotator == null) {
                        Rotator = Rotator.Setup(Transform);
                    }
                    if (Rotator == null) {
                        return Vector3.zero;
                    }
                    else {
                        Rotator.IsWorldSpace = world;
                        return Rotator.Euler + Rotation;
                        //if (world) {
                        //    return Transform.eulerAngles + Rotation;
                        //}
                        //else {
                        //    return Transform.localEulerAngles + Rotation;
                        //}
                    }
                }
                else {
                    return Vector3.zero;
                }
            }
            else {
                if (world && TransformType == TransformTypes.Parent && Parent != null) {
                    return Parent.TransformVector(Rotation);
                }
                else {
                    return Rotation;
                }
            }
        }

        public void Enter()
        {
            if (!HasEntered) {
                HasEntered = true;
                HasExited = false;
                if (OnEnter != null) OnEnter.Invoke();
            }
        }

        public void Exit()
        {
            if (!HasExited) {
                HasExited = true;
                HasEntered = false;
                if (OnExit != null) OnExit.Invoke();
            }
        }

#if UNITY_EDITOR

        public bool EditorShow = true;
        public static bool EnableGUIColor = true;
        public bool EnableColorShading = true;

        public Color GUIColor {
            get {
                Color shade = Color.white;
                if (EnableGUIColor) {
                    if (TransformType == TransformTypes.Local) {
                        shade = new Color(0.95f, 0.95f, 1f, 1);
                    }
                    else
                    if (TransformType == TransformTypes.Parent) {
                        shade = new Color(0.8f, 1f, 0.8f, 1);
                    }
                    else
                    if (TransformType == TransformTypes.World) {
                        shade = new Color(0.8f, 0.8f, 1f, 1);
                    }
                }
                return shade;
            }
        }

#endif

    }

    [Serializable]
    public class BlendObjectActivate : SerializableObject
    {
        public GameObject Object;
        public bool Default = true;

        /// <summary>
        /// Since active states are either on or off and cannot be blended, the following modes provide
        /// options for how the active state is transitioned.
        /// </summary>
        public enum Transitions
        {
            Midpoint, // Change at halfway point
            EitherOn, // Stay active if either from or to are active
            EitherOff, // Deactivate if either from or to are deactivated
            ActivateAtEnd, // Change object active state at end of transition
            ActivateAtStart // Change object active state at start of transition
        }
        public Transitions Transition = Transitions.Midpoint;
        public float Midpoint = 0.5f;
    }

}//AxonGenesis

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a special class type that handles Property behaviors for Animator components. This exposes
    /// Animator values so that they can be controlled and animated by Timeflow and other components, and
    /// accessible via property drop down menus. This requires the Animator component to be active to
    /// function. It cannot be mixed with animation clips on the same object, which requires the Animator
    /// component to be off for directly interpolating clips.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfAnimator")]
    public class PropertiesOfAnimator : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        public Animator Animator;

        private bool AnimatorHasParameters => Animator != null && Animator.isActiveAndEnabled && Animator.runtimeAnimatorController != null && Animator.parameters != null && (_List == null || _List.Count != Animator.parameterCount);

        [NonSerialized]
        private AnimatorControllerParameter _parameter;

        public AnimatorControllerParameter Parameter {
            get {
                if (Animator == null) Animator = Object as Animator;
                if (AnimatorHasParameters && !string.IsNullOrEmpty(Name)) {
                    if (!Animator.isActiveAndEnabled) {
                        Animator.enabled = true;
                        Animator.StartPlayback();
                    }
                    AnimatorInfo.Init(Animator.gameObject, true);
                    foreach (AnimatorControllerParameter param in Animator.parameters) {
                        if (param.name.Equals(Name)) {
                            _parameter = param;
                            break;
                        }
                    }
                }
                return _parameter;
            }
        }

        public PropertiesOfAnimator() { }

        public override SDictionary<string, Type> List {
            get {
                if (AnimatorHasParameters) {
                    _List = new SDictionary<string, Type>();

                    foreach (AnimatorControllerParameter param in Animator.parameters) {
                        bool canUse = false;
                        Type paramType = typeof(bool);
                        if (param.type == AnimatorControllerParameterType.Bool) {
                            canUse = true;
                        }
                        else
                        if (param.type == AnimatorControllerParameterType.Float) {
                            canUse = true;
                            paramType = typeof(float);
                        }
                        else
                        if (param.type == AnimatorControllerParameterType.Int) {
                            canUse = true;
                            paramType = typeof(int);
                        }
                        if (canUse) {
                            _List.Add(param.name, paramType);
                        }
                    }
                }
                return _List;
            }
        }

        public override bool HasProperty(string name)
        {
            bool has = false;

            if (AnimatorHasParameters && !string.IsNullOrEmpty(name)) {
                foreach (AnimatorControllerParameter param in Animator.parameters) {
                    if (param.name.Equals(name)) {
                        has = true;
                        break;
                    }
                }
            }
            return has;
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
                Animator v = _Object as Animator;
                if (Animator != v) {
                    Animator = v;
                    if (!Animator.enabled) Animator.enabled = true;
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(Animator);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (Parameter != null) {
                if (Parameter.type == AnimatorControllerParameterType.Float) {
                    value.x = Animator.GetFloat(Parameter.nameHash);
                }
                else
                if (Parameter.type == AnimatorControllerParameterType.Int) {
                    value.x = (float)Animator.GetInteger(Parameter.nameHash);
                }
                else
                if (Parameter.type == AnimatorControllerParameterType.Bool) {
                    value.x = Animator.GetBool(Parameter.nameHash) ? 1f : 0f;
                }
            }
            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (Parameter != null) {
                if (Parameter.type == AnimatorControllerParameterType.Float) {
                    Animator.SetFloat(Parameter.nameHash, value.x);
                }
                else
                if (Parameter.type == AnimatorControllerParameterType.Int) {
                    Animator.SetInteger(Parameter.nameHash, (int)value.x);
                }
                else
                if (Parameter.type == AnimatorControllerParameterType.Bool) {
                    Animator.SetBool(Parameter.nameHash, value.x > 0.5f);
                }
                /// There is no Vector type for AnimatorControllerParameterType. This also does not
                /// implement Trigger parameters, since those should be set with an event and not animated
                /// as a channel.
            }
        }

        public override void SetFloat(float value)
        {
            if (Parameter != null) {
                Animator.SetFloat(Parameter.nameHash, value);
            }
        }

        public override void SetBool(bool value)
        {
            if (Parameter != null) {
                Animator.SetBool(Parameter.nameHash, value);
            }
        }

        public override void SetInt(int value)
        {
            if (Parameter != null) {
                Animator.SetInteger(Parameter.nameHash, value);
            }
        }

        public override float GetFloat()
        {
            if (Parameter != null) {
                return Animator.GetFloat(Parameter.nameHash);
            }
            return 0f;
        }

        public override bool GetBool()
        {
            if (Parameter != null) {
                return Animator.GetBool(Parameter.nameHash);
            }
            return false;
        }

        public override int GetInt()
        {
            if (Parameter != null) {
                return Animator.GetInteger(Parameter.nameHash);
            }
            return 0;
        }

    }

}//AxonGenesis

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.


#if URP_10_OR_NEWER || HDRP_10_OR_NEWER

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Defines custom handling of Volume component properties
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfVolume")]
    public class PropertiesOfVolume : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        public Volume Vol;
        public VolumeComponent Component;
        public VolumeParameter Parameter;

        private VolumeProfile _profile;
        private List<VolumeComponent> _comps;
        private FieldInfo[] _fields;

        public PropertiesOfVolume() { }

        public override SDictionary<string, Type> List {
            get {
                if (_List == null && Vol != null) {
                    _comps = new List<VolumeComponent>();
                    Vol.profile.TryGetAllSubclassOf<VolumeComponent>(typeof(VolumeComponent), _comps);

                    if (_comps == null) {
                        LogWarning("Volume is null", "Volume has no volume components");
                    }
                    else {
                        _List = new SDictionary<string, Type>();
                        foreach (VolumeComponent v in _comps) {
                            Type volumeType = v.GetType();

                            _fields = volumeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (_fields != null) {
                                for (int f = 0; f < _fields.Length; f++) {
                                    bool canAdd = true;

                                    Type type = _fields[f].FieldType;
                                    Type basicType = typeof(float);
                                    if (typeof(BoolParameter).IsAssignableFrom(type) || typeof(bool).IsAssignableFrom(type)) {
                                        basicType = typeof(bool);
                                    }
                                    else
                                    if (typeof(IntParameter).IsAssignableFrom(type)) {
                                        basicType = typeof(int);
                                    }
                                    else
                                    if (typeof(FloatParameter).IsAssignableFrom(type)) {
                                        basicType = typeof(float);
                                    }
                                    else
                                    if (typeof(Vector2Parameter).IsAssignableFrom(type)) {
                                        basicType = typeof(Vector2);
                                    }
                                    else
                                    if (typeof(Vector3Parameter).IsAssignableFrom(type)) {
                                        basicType = typeof(Vector3);
                                    }
                                    else
                                    if (typeof(Vector4Parameter).IsAssignableFrom(type)) {
                                        basicType = typeof(Vector4);
                                    }
                                    else
                                    if (typeof(ColorParameter).IsAssignableFrom(type)) {
                                        basicType = typeof(Color);
                                    }
                                    else {
                                        canAdd = false;
                                    }

                                    if (canAdd) {
                                        string item = v.name.Replace("(Clone)", "") + "/" + _fields[f].Name;
                                        _List.Add(item, basicType);
                                    }
                                }
                            }
                        }
                    }
                }
                return _List;
            }
        }

        public override bool ShowDefaultProperties {
            get {
                return true;
            }
        }

        public override bool HasProperty(string name)
        {
            bool has = false;
            if (Vol == null) {
                LogWarning("Volume is null", "Missing Volume component reference");
            }
            else
            if (!string.IsNullOrEmpty(name)) {
                if (Parameter != null && name.Equals(name)) {
                    has = true;
                }
                else {
                    VolumeComponent comp = null;
                    VolumeParameter param = null;
                    ParseName(name, out comp, out param);
                    if (comp != null && param != null) {
                        has = true;
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
                Volume v = _Object as Volume;
                if (Vol != v) {
                    Vol = v;
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(Volume);
            }
        }

        private void ParseName(string name, out VolumeComponent comp, out VolumeParameter param)
        {
            comp = null;
            param = null;
            if (Vol == null) {
                LogWarning("Volume is null", "Missing Volume component reference");
            }
            else
            if (string.IsNullOrEmpty(name)) {
                LogWarning("Name is null", "Parameter name is not set");
            }
            else {
                int sep = name.IndexOf('/');
                if (sep > 0) {
                    string componentName = name.Substring(0, sep).Replace("(Clone)", "");
                    string parameterName = name.Substring(sep + 1);

                    if (_comps == null) {
                        /// Force rebuilding of components list
                        _List = null;
                        SDictionary<string, Type> list = List;
                    }

                    if (_comps == null) {
                        LogWarning("Volume is null", "No volume components found:" + name);
                    }
                    else {
                        foreach (VolumeComponent v in _comps) {
                            //Debug.Log($"<color='cyan'>Checking component:{v.name}</color>");
                            if (v.name.Replace("(Clone)", "").Equals(componentName)) {
                                comp = v;
                                break;
                            }
                        }
                    }

                    if (comp == null) {
                        LogWarning("Volume is null", "No volume component found matching name:" + componentName);
                    }
                    else {
                        _fields = comp.GetType().GetFields();
                        if (_fields != null) {
                            for (int f = 0; f < _fields.Length; f++) {
                                if (_fields[f].Name.Equals(parameterName)) {
                                    object obj = _fields[f].GetValue(comp);
                                    if (typeof(VolumeParameter).IsAssignableFrom(obj.GetType())) {
                                        param = (VolumeParameter)obj;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Component GetComponentForType(UnityEngine.Object obj)
        {
            //Debug.Log("PropertiesOfVolume.GetComponentForType:" + obj.GetType());
            if (obj is Volume volume) {
                return volume;
            }
            else
            if (obj is VolumeComponent comp) {
#if UNITY_EDITOR
                // There is no way to get the component from the settings, so as a work around
                // we will search the selected game objects for a volume component
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                    foreach (GameObject gameObject in Selection.gameObjects) {
                        Volume v = gameObject.GetComponent<Volume>();
                        if (v != null) {
                            return v;
                        }
                    }
                }
#endif
            }
            return null;
        }

        public static Type GetFieldType(Type fieldType)
        {
            if (typeof(BoolParameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(bool);
            }
            else
                if (typeof(IntParameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(int);
            }
            else
                if (typeof(FloatParameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(float);
            }
            else
                if (typeof(Vector2Parameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(Vector2);
            }
            else
                if (typeof(Vector3Parameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(Vector3);
            }
            else
                if (typeof(Vector4Parameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(Vector4);
            }
            else
                if (typeof(ColorParameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(Color);
            }
            else
            if (typeof(ObjectParameter<Texture>).IsAssignableFrom(fieldType)) {
                fieldType = typeof(UnityEngine.Object);
            }
            return fieldType;
        }

        public override string Name {
            get {
                CheckParameter();
                return _Name;
            }
            set {
                _Name = value;
                CheckParameter(true);
            }
        }

        private bool CheckParameter(bool changed = false)
        {
            if (Parameter != null && !changed) return true;
            if (Vol != null && !string.IsNullOrEmpty(_Name)) {
                ParseName(_Name, out Component, out Parameter);
            }
            return Parameter != null;
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (!CheckParameter()) return value;

            Type type = Parameter.GetType();
            if (typeof(Vector2Parameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<Vector2>();
            }
            else
            if (typeof(Vector3Parameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<Vector3>();
            }
            else
            if (typeof(Vector4Parameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<Vector4>();
            }
            else
            if (typeof(ColorParameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<Color>();
            }
            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (!CheckParameter()) return;
            Type type = Parameter.GetType();
            if (typeof(Vector2Parameter).IsAssignableFrom(type)) {
                if (attribute < 0) {
                    Parameter.SetValue(new Vector2Parameter(value));
                }
                else {
                    Vector2 v = Parameter.GetValue<Vector2>();
                    if (attribute == 0) {
                        v = new Vector2(value.x, v.y);
                    }
                    else {
                        v = new Vector2(v.x, value.x);
                    }
                    Parameter.SetValue(new Vector2Parameter(value));
                }
            }
            if (typeof(Vector3Parameter).IsAssignableFrom(type)) {
                if (attribute < 0) {
                    Parameter.SetValue(new Vector3Parameter(value));
                }
                else {
                    Vector3 v = Parameter.GetValue<Vector3>();
                    if (attribute == 0) {
                        v = new Vector3(value.x, v.y, v.z);
                    }
                    else
                    if (attribute == 1) {
                        v = new Vector3(v.x, value.x, v.z);
                    }
                    else {
                        v = new Vector3(v.x, v.y, value.x);
                    }
                    Parameter.SetValue(new Vector3Parameter(value));
                }
            }
            else
            if (typeof(Vector4Parameter).IsAssignableFrom(type)) {
                if (attribute < 0) {
                    Parameter.SetValue(new Vector4Parameter(value));
                }
                else {
                    Vector4 v = Parameter.GetValue<Vector4>();
                    if (attribute == 0) {
                        v = new Vector4(value.x, v.y, v.z, v.w);
                    }
                    else
                    if (attribute == 1) {
                        v = new Vector4(v.x, value.x, v.z, v.w);
                    }
                    else
                    if (attribute == 2) {
                        v = new Vector4(v.x, v.y, value.x, v.w);
                    }
                    else {
                        v = new Vector4(v.x, v.y, v.z, value.x);
                    }
                    Parameter.SetValue(new Vector4Parameter(value));
                }
            }
            else
            if (typeof(ColorParameter).IsAssignableFrom(type)) {
                if (attribute < 0) {
                    Parameter.SetValue(new ColorParameter(value));
                }
                else {
                    Color v = Parameter.GetValue<Color>();
                    if (attribute == 0) {
                        v = new Color(value.x, v.g, v.b, v.a);
                    }
                    else
                    if (attribute == 1) {
                        v = new Color(v.r, value.x, v.b, v.a);
                    }
                    else
                    if (attribute == 2) {
                        v = new Color(v.r, v.g, value.x, v.a);
                    }
                    else {
                        v = new Color(v.r, v.g, v.b, value.x);
                    }
                    Parameter.SetValue(new ColorParameter(v));
                }
            }
        }

        public override void SetFloat(float value)
        {
            if (!CheckParameter()) return;
            Type type = Parameter.GetType();
            if (typeof(FloatParameter).IsAssignableFrom(type)) {
                Parameter.SetValue(new FloatParameter(value));
            }
            else {
                LogWarning("SetFloat", "Failed to set property for type:" + type);
            }
        }

        public override void SetBool(bool value)
        {
            if (!CheckParameter()) return;
            Type type = Parameter.GetType();
            if (typeof(BoolParameter).IsAssignableFrom(type)) {
                Parameter.SetValue(new BoolParameter(value));
            }
            else {
                LogWarning("SetBool", "Failed to set property for type:" + type);
            }
        }

        public override void SetInt(int value)
        {
            if (!CheckParameter()) return;
            Type type = Parameter.GetType();
            if (typeof(IntParameter).IsAssignableFrom(Parameter.GetType())) {
                Parameter.SetValue(new IntParameter(value));
            }
            else {
                LogWarning("SetInt", "Failed to set property for type:" + type);
            }
        }

        public override void SetColor(Color value)
        {
            if (!CheckParameter()) return;
            Type type = Parameter.GetType();
            if (typeof(ColorParameter).IsAssignableFrom(Parameter.GetType())) {
                Parameter.SetValue(new ColorParameter(value));
            }
            else {
                LogWarning("SetColor", "Failed to set property for type:" + type);
            }
        }

        public override float GetFloat()
        {
            float value = 0f;
            if (!CheckParameter()) return value;
            Type type = Parameter.GetType();
            if (typeof(FloatParameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<float>();
            }
            else {
                LogWarning("GetFloat", "Failed to get property for type:" + type);
            }
            return value;
        }

        public override int GetInt()
        {
            int value = 0;
            if (!CheckParameter()) return value;
            Type type = Parameter.GetType();
            if (typeof(IntParameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<int>();
            }
            else {
                LogWarning("GetInt", "Failed to get property for type:" + type);
            }
            return value;
        }

        public override bool GetBool()
        {
            bool value = false;
            if (!CheckParameter()) return value;
            Type type = Parameter.GetType();
            if (typeof(BoolParameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<bool>();
            }
            else {
                LogWarning("GetBool", "Failed to get property for type:" + type);
            }
            return value;
        }

        public override Color GetColor()
        {
            Color value = Color.black;
            if (!CheckParameter()) return value;
            Type type = Parameter.GetType();
            if (typeof(ColorParameter).IsAssignableFrom(type)) {
                value = Parameter.GetValue<Color>();
            }
            else {
                LogWarning("GetColor", "Failed to get property for type:" + type);
            }
            return value;
        }

        override public void SetObject(UnityEngine.Object value)
        {
            if (!CheckParameter()) return;
            if (Parameter is TextureParameter param) {
                param.Override(value as Texture);
            }
            else {
                LogWarning("SetObject", "Failed to set property for type:" + Parameter.GetType());
            }
        }

        public override UnityEngine.Object GetObject()
        {
            UnityEngine.Object value = null;
            if (!CheckParameter()) return value;
            if (Parameter is TextureParameter param) {
                value = param.value;
            }
            else {
                LogWarning("GetObject", "Failed to get property for type:" + Parameter.GetType());
            }
            return value;
        }
    }
}//AxonGenesis
#endif

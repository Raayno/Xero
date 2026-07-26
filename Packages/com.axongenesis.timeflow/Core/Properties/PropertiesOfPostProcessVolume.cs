// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if POSTPROCESSING_1_OR_NEWER && !URP_10_OR_NEWER && !HDRP_10_OR_NEWER
// Supports built-in post processing only

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Defines custom handling of Volume component properties
    /// </summary>
    [Serializable]
    public class PropertiesOfPostProcessVolume : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        private FieldInfo[] _fields;

        public PostProcessVolume Vol;
        public PostProcessEffectSettings Component;
        public ParameterOverride Parameter;

        private List<PostProcessEffectSettings> _settings;

        public PropertiesOfPostProcessVolume() { }

        public override SDictionary<string, Type> List {
            get {
                if (Vol == null) {
                    Debug.LogWarning("Missing Volume component reference");
                }
                else
                if (_List == null) {
                    _settings = Vol.profile.settings;

                    if (_settings == null) {
                        Debug.LogWarning("Volume has no post processing effects");
                    }
                    else {
                        //Debug.Log($"<color='red'>REBUILD LIST {_settings.Count} post processing effects</color>");
                        _List = new SDictionary<string, Type>();
                        foreach (PostProcessEffectSettings v in _settings) {
                            Type volumeType = v.GetType();
                            AddToList(v.name, volumeType);
                        }
                    }
                }

                return _List;
            }
        }

        private void AddToList(string volumeName, Type volumeType)
        {
            _fields = volumeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_fields == null) return;

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
                else
                if (typeof(TextureParameter).IsAssignableFrom(type)) {
                    basicType = typeof(Texture);
                }
                else {
                    canAdd = false;
                }

                if (canAdd) {
                    string itemName = volumeName.Replace("(Clone)", "") + "/" + _fields[f].Name;
                    _List.Add(itemName, basicType);
                    //Debug.Log($"<color='green'>AddToList:{itemName}:{basicType}</color>");
                }
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
                Debug.LogWarning("Missing Volume component reference");
            }
            else
            if (!string.IsNullOrEmpty(name)) {
                if (Parameter != null && name.Equals(name)) {
                    has = true;
                }
                else {
                    PostProcessEffectSettings comp = null;
                    ParameterOverride param = null;
                    ParseName(name, out comp, out param);
                    if (comp != null && param != null) {
                        has = true;
                    }
                }
            }
            //Debug.Log("HasProperty:" + name + ":" + has);
            return has;
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
                PostProcessVolume v = _Object as PostProcessVolume;
                if (Vol != v) {
                    Vol = v;
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(PostProcessVolume);
            }
        }

        public static Component GetComponentForType(UnityEngine.Object obj)
        {
            if (obj is PostProcessVolume volume) {
                return volume;
            }
            else
            if (obj is PostProcessEffectSettings setting) {
#if UNITY_EDITOR
                // There is no way to get the component from the settings, so as a work around
                // we will search the selected game objects for a volume component
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                    foreach (GameObject gameObject in Selection.gameObjects) {
                        PostProcessVolume v = gameObject.GetComponent<PostProcessVolume>();
                        if (v != null) {
                            return v;
                        }
                    }
                }
#endif
            }
            return null;
        }

        private void ParseName(string name, out PostProcessEffectSettings setting, out ParameterOverride param)
        {
            setting = null;
            param = null;
            if (Vol == null) {
                Debug.LogWarning("Missing Volume component reference");
            }
            else
            if (string.IsNullOrEmpty(name)) {
                Debug.LogWarning("Parameter name is null");
            }
            else {
                int sep = name.IndexOf('/');
                if (sep > 0) {
                    string settingName = name.Substring(0, sep);
                    string parameterName = name.Substring(sep + 1);
                    if (_settings == null) {
                        /// Force rebuilding of components list
                        _List = null;
                        SDictionary<string, Type> list = List;
                    }
                    if (_settings == null) {
                        Debug.LogWarning("No volume settings found:" + name);
                    }
                    else {
                        foreach (PostProcessEffectSettings v in _settings) {
                            if (v.name.Equals(settingName) || v.name.Replace("(Clone)", "").Equals(settingName)) {
                                setting = v;
                                break;
                            }
                        }
                    }
                    if (setting == null) {
                        Debug.LogWarning("No volume setting found matching name:" + settingName);
                    }
                    else {
                        _fields = setting.GetType().GetFields();
                        if (_fields != null) {
                            for (int f = 0; f < _fields.Length; f++) {
                                if (_fields[f].Name.Equals(parameterName)) {
                                    object obj = _fields[f].GetValue(setting);
                                    if (typeof(ParameterOverride).IsAssignableFrom(obj.GetType())) {
                                        param = (ParameterOverride)obj;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
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

            if (Parameter != null) {
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
            }
            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (!CheckParameter()) return;
            //Debug.Log($"SetVector: {Name} {value} {attribute}");
            Type type = Parameter.GetType();
            if (typeof(Vector2Parameter).IsAssignableFrom(type)) {
                if (attribute >= 0) {
                    Vector2Parameter v = Parameter.GetValue<Vector2Parameter>();
                    if (attribute == 0) {
                        v.value = new Vector2(value.x, v.value.y);
                    }
                    else
                    if (attribute == 1) {
                        v.value = new Vector2(v.value.x, value.x);
                    }
                }
                if (Parameter is Vector2Parameter param) {
                    param.Override(value);
                }
            }
            if (typeof(Vector3Parameter).IsAssignableFrom(type)) {
                if (attribute >= 0) {
                    Vector3Parameter v = Parameter.GetValue<Vector3Parameter>();
                    if (attribute == 0) {
                        v.value = new Vector3(value.x, v.value.y, v.value.z);
                    }
                    else
                    if (attribute == 1) {
                        v.value = new Vector3(v.value.x, value.x, v.value.z);
                    }
                    else
                    if (attribute == 2) {
                        v.value = new Vector3(v.value.x, v.value.y, value.x);
                    }
                }
                if (Parameter is Vector3Parameter param) {
                    param.Override(value);
                }
            }
            else
            if (typeof(Vector4Parameter).IsAssignableFrom(type)) {
                if (attribute >= 0) {
                    Vector4Parameter v = Parameter.GetValue<Vector4Parameter>();
                    if (attribute == 0) {
                        v.value = new Vector4(value.x, v.value.y, v.value.z, v.value.w);
                    }
                    else
                    if (attribute == 1) {
                        v.value = new Vector4(v.value.x, value.x, v.value.z, v.value.w);
                    }
                    else
                    if (attribute == 2) {
                        v.value = new Vector4(v.value.x, v.value.y, value.x, v.value.w);
                    }
                    else
                    if (attribute == 3) {
                        v.value = new Vector4(v.value.x, v.value.y, v.value.z, value.x);
                    }
                }
                if (Parameter is Vector4Parameter param) {
                    param.Override(value);
                }
            }
            else
            if (typeof(ColorParameter).IsAssignableFrom(type)) {
                if (attribute >= 0) {
                    ColorParameter v = Parameter.GetValue<ColorParameter>();
                    if (attribute == 0) {
                        v.value = new Color(value.x, v.value.g, v.value.b, v.value.a);
                    }
                    else
                    if (attribute == 1) {
                        v.value = new Color(v.value.r, value.x, v.value.b, v.value.a);
                    }
                    else
                    if (attribute == 2) {
                        v.value = new Color(v.value.r, v.value.g, value.x, v.value.a);
                    }
                    else
                    if (attribute == 3) {
                        v.value = new Color(v.value.r, v.value.g, v.value.b, value.x);
                    }
                }
                if (Parameter is ColorParameter param) {
                    param.Override(value);
                }
            }
        }

        public override void SetFloat(float value)
        {
            if (!CheckParameter()) return;
            if (Parameter is FloatParameter param) {
                param.Override(value);
            }
            else {
                Debug.LogWarning("Failed to set property for type:" + Parameter.GetType());
            }
        }

        public override void SetBool(bool value)
        {
            if (!CheckParameter()) return;
            if (Parameter is BoolParameter param) {
                param.Override(value);
            }
            else {
                Debug.LogWarning("Failed to set property for type:" + Parameter.GetType());
            }
        }

        public override void SetInt(int value)
        {
            if (!CheckParameter()) return;
            if (Parameter is IntParameter param) {
                param.Override(value);
            }
            else {
                Debug.LogWarning("Failed to set property for type:" + Parameter.GetType());
            }
        }

        public override void SetColor(Color value)
        {
            if (!CheckParameter()) return;
            if (Parameter is ColorParameter param) {
                param.Override(value);
            }
            else {
                Debug.LogWarning("Failed to set property for type:" + Parameter.GetType());
            }
        }

        public override float GetFloat()
        {
            float value = 0f;
            if (!CheckParameter()) return value;
            if (Parameter is FloatParameter param) {
                value = param.value;
            }
            else {
                Debug.LogWarning("Failed to get property for type:" + Parameter.GetType());
            }
            return value;
        }

        public override int GetInt()
        {
            int value = 0;
            if (!CheckParameter()) return value;
            if (Parameter is IntParameter param) {
                value = param.value;
            }
            else {
                Debug.LogWarning("Failed to get property for type:" + Parameter.GetType());
            }
            return value;
        }

        public override bool GetBool()
        {
            bool value = false;
            if (!CheckParameter()) return value;
            if (Parameter is BoolParameter param) {
                value = param.value;
            }
            else {
                Debug.LogWarning("Failed to get property for type:" + Parameter.GetType());
            }
            return value;
        }

        public override Color GetColor()
        {
            Color value = Color.black;
            if (!CheckParameter()) return value;
            if (Parameter is ColorParameter param) {
                value = param.value;
            }
            else {
                Debug.LogWarning("Failed to get property for type:" + Parameter.GetType());
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
                Debug.LogWarning("Failed to set property for type:" + Parameter.GetType());
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
                Debug.LogWarning("Failed to get property for type:" + Parameter.GetType());
            }
            return value;
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
            if (typeof(TextureParameter).IsAssignableFrom(fieldType)) {
                fieldType = typeof(UnityEngine.Object);
            }
            return fieldType;
        }
    }
}//AxonGenesis
#endif
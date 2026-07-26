// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Handles material property caching to support auto-keyframing
    /// </summary>
    public partial class TimeflowObject : TimeflowBehavior
    {
        public class MaterialChange
        {
            public object CurrentValue;
            public Type Type;
            public int Attribute;

            public MaterialChange() { }

            public MaterialChange(object currentValue, Type type, int attribute)
            {
                CurrentValue = currentValue;
                Type = type;
                Attribute = attribute;
            }
        }

        private Renderer _Renderer;
        private List<Material> _Materials;
        private Dictionary<Material, Dictionary<string, MaterialChange>> _MaterialProperties;
        private float _MaterialCacheTime = -1f;

        protected override void AutoKeyframingDetect()
        {
            _SetupAutoKeyframing();
            _CheckForMaterialChanges();
        }

        private void _SetupAutoKeyframing()
        {
            if (_Renderer == null || _CacheIsInvalid()) {
                _Renderer = GetComponent<Renderer>();
                _CacheMaterialProperties();
            }
        }

        private bool _CacheIsInvalid()
        {
            if (Timeflow.IsAutoKeyframingInvalidThisFrame) return true;
            if (_Materials == null || _Materials.Count != _Renderer.sharedMaterials.Length) return true;

            if (_MaterialCacheTime != CurrentTime) return true;

            // Compare materials to detect any new assignments
            for (int i = 0; i < _Materials.Count; i++) {
                if (_Materials[i] != _Renderer.sharedMaterials[i]) {
                    Debug.Log($"New material assignment detected");//--KEEP
                    return true;
                }
            }

            return false;
        }

        private void _CacheMaterialProperties()
        {
            if (_Renderer == null) return;

            _Materials = new List<Material>();
            _MaterialProperties = new Dictionary<Material, Dictionary<string, MaterialChange>>();

            foreach (Material mat in _Renderer.sharedMaterials) {
                if (mat == null) continue;
                _Materials.Add(mat);

                var properties = new Dictionary<string, MaterialChange>();

                // Cache float, vector, color, and texture properties
#if UNITY_6000_2_OR_NEWER
                int count = mat.shader.GetPropertyCount();
                for (int i = 0; i < count; i++) {
                    string property = mat.shader.GetPropertyName(i);
                    properties[property] = new MaterialChange();

                    UnityEngine.Rendering.ShaderPropertyType type = mat.shader.GetPropertyType(i);
                    //Debug.Log($"{property} {type}");
                    switch (type) {
                        case UnityEngine.Rendering.ShaderPropertyType.Color:
                            properties[property].Type = typeof(Color);
                            properties[property].CurrentValue = mat.GetColor(property);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                            properties[property].Type = typeof(Vector4);
                            properties[property].CurrentValue = mat.GetVector(property);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Float:
                        case UnityEngine.Rendering.ShaderPropertyType.Range:
                            properties[property].Type = typeof(float);
                            properties[property].CurrentValue = mat.GetFloat(property);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                            properties[property].CurrentValue = mat.GetTexture(property);

                            string p = property + PropertiesOfMaterial.TextureOffsetTag;
                            properties[p] = new MaterialChange();
                            properties[p].Type = typeof(Vector2);
                            properties[p].CurrentValue = mat.GetTextureOffset(property);

                            p = property + PropertiesOfMaterial.TextureScaleTag;
                            properties[p] = new MaterialChange();
                            properties[p].Type = typeof(Vector2);
                            properties[p].CurrentValue = mat.GetTextureScale(property);
                            break;
                    }
#else
                int count = ShaderUtil.GetPropertyCount(mat.shader);
                for (int i = 0; i < count; i++) {
                    string property = ShaderUtil.GetPropertyName(mat.shader, i);
                    properties[property] = new MaterialChange();

                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(mat.shader, i);
                    //Debug.Log($"{property} {type}");
                    switch (type) {
                        case ShaderUtil.ShaderPropertyType.Color:
                            properties[property].Type = typeof(Color);
                            properties[property].CurrentValue = mat.GetColor(property);
                            break;
                        case ShaderUtil.ShaderPropertyType.Vector:
                            properties[property].Type = typeof(Vector4);
                            properties[property].CurrentValue = mat.GetVector(property);
                            break;
                        case ShaderUtil.ShaderPropertyType.Float:
                        case ShaderUtil.ShaderPropertyType.Range:
                            properties[property].Type = typeof(float);
                            properties[property].CurrentValue = mat.GetFloat(property);
                            break;
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            properties[property].CurrentValue = mat.GetTexture(property);

                            string p = property + PropertiesOfMaterial.TextureOffsetTag;
                            properties[p] = new MaterialChange();
                            properties[p].Type = typeof(Vector2);
                            properties[p].CurrentValue = mat.GetTextureOffset(property);

                            p = property + PropertiesOfMaterial.TextureScaleTag;
                            properties[p] = new MaterialChange();
                            properties[p].Type = typeof(Vector2);
                            properties[p].CurrentValue = mat.GetTextureScale(property);
                            break;
                    }
#endif
                }

                _MaterialProperties[mat] = properties;
            }

            _MaterialCacheTime = CurrentTime;
        }

        private void _CheckForMaterialChanges()
        {
            if (_Renderer == null || _Renderer.sharedMaterials == null || _Renderer.sharedMaterials.Length == 0) return;
            if (_MaterialProperties == null || _MaterialProperties.Count == 0) return;

            bool debug = false;
            var changes = new Dictionary<string, MaterialChange>();

            foreach (Material mat in _Renderer.sharedMaterials) {
                if (!_MaterialProperties.ContainsKey(mat)) continue;

                var properties = _MaterialProperties[mat];

                foreach (var property in properties.Keys) {
                    Type type = properties[property].Type;
                    object cachedValue = properties[property].CurrentValue;

                    // Compare based on type
                    int attribute = -1;
                    object currentValue = null;

                    if (property.Contains(PropertiesOfMaterial.TextureOffsetTag)) {
                        currentValue = mat.GetTextureOffset(property.Replace(PropertiesOfMaterial.TextureOffsetTag, ""));
                        if (cachedValue is Vector2 v2) {
                            attribute = Property.GetAttributeChanged((Vector2)currentValue, v2);
                        }
                        if (debug && !currentValue.Equals(cachedValue)) {
                            Debug.Log($"TextureOffset:'{mat.name}.{property} currentValue:{currentValue} cachedValue:{cachedValue}'");//--KEEP
                        }
                    }
                    else
                    if (property.Contains(PropertiesOfMaterial.TextureScaleTag)) {
                        currentValue = mat.GetTextureScale(property.Replace(PropertiesOfMaterial.TextureScaleTag, ""));
                        if (cachedValue is Vector2 v2) {
                            attribute = Property.GetAttributeChanged((Vector2)currentValue, v2);
                        }
                        if (debug && !currentValue.Equals(cachedValue)) {
                            Debug.Log($"TextureScale:'{mat.name}.{property} currentValue:{currentValue} cachedValue:{cachedValue}'");//--KEEP
                        }
                    }
                    else
                    if (cachedValue is Color c1) {
                        currentValue = mat.GetColor(property);
                        if (currentValue is Color c2) {
                            attribute = Property.GetAttributeChanged(c1, c2);
                        }
                    }
                    else
                    if (cachedValue is Vector4 v1) {
                        currentValue = mat.GetVector(property);
                        if (currentValue is Vector4 v2) {
                            attribute = Property.GetAttributeChanged(v1, v2);
                        }
                    }
                    else
                    if (cachedValue is float) {
                        currentValue = mat.GetFloat(property);
                    }
                    else
                    if (cachedValue is Texture) {
                        currentValue = mat.GetTexture(property);
                    }

                    if (currentValue == null && cachedValue == null) {
                        // No change
                    }
                    else
                    if ((currentValue == null && currentValue != null) || !currentValue.Equals(cachedValue)) {
                        attribute = TimeflowWindow.GetAutoKeyframeAttributeModifiers(type, attribute);
                        changes[property] = new MaterialChange(currentValue, type, attribute);
                    }
                }

                // Assign the new changed values in a second pass to avoid iteration conflict
                foreach (var property in changes.Keys) {
                    properties[property].CurrentValue = changes[property].CurrentValue;
                    TimeflowWindow.RecordPropertyChange(transform, changes[property].Type, property, property, changes[property].Attribute, mat);
                    if (debug) Debug.Log($"Changed detected '{mat.name}.{property} {changes[property].Attribute}'");//--KEEP
                }

                if (changes.Keys.Count > 0) {
                    TimeflowWindow.HandlePropertyModifications();
                }
            }


        }
    }
}
#endif
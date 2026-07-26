// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is a special class type that handles Property behaviors for Material components. This exposes
    /// Material values so that they can be controlled and animated by Timeflow and other components, and
    /// accessible via the property drop down menus.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfMaterial")]
    public class PropertiesOfMaterial : PropertiesHandler
    {
        #region STATIC HELPERS

        public const string TextureOffsetTag = " TextureOffset";
        public const string TextureScaleTag = " TextureScale";

        private static bool IsForceSharedMaterials(Renderer renderer)
        {
            bool forceSharedMaterials = false;
            if (Application.isPlaying && renderer != null) {
                if (TimeflowPreferences.Current.ForceSharedMaterials) {
                    forceSharedMaterials = true;
                }
                else
                if (renderer.gameObject.TryGetComponent<MaterialPropertyOptions>(out var options)) {
                    forceSharedMaterials = options.ForceSharedMaterial;
                }
            }
            return forceSharedMaterials;
        }

        /// <summary>
        /// Returns an indexed list of all the materials and shader properties they contain. 
        /// </summary>
        public static SDictionary<string, SDictionary<string, Type>> GetMaterialProperties(Renderer renderer)
        {
            if (renderer == null) return null;

            bool forceSharedMaterials = IsForceSharedMaterials(renderer);

            //Debug.Log($"<color=orange><PropertiesOfMaterial.GetMaterialProperties></color> for '{renderer.gameObject.name}' (forceSharedMaterials={forceSharedMaterials})");
            SDictionary<string, SDictionary<string, Type>> materials = null;
            int count = 0;
            if (Application.isPlaying && !forceSharedMaterials) {
                count = renderer.materials.Length;
            }
            else {
                count = renderer.sharedMaterials.Length;
            }
            if (count > 0) {
                materials = new SDictionary<string, SDictionary<string, Type>>();
                for (int i = 0; i < count; i++) {
                    Material mat = null;
                    if (Application.isPlaying && !forceSharedMaterials) {
                        mat = renderer.materials[i];
                    }
                    else
                    if (renderer.sharedMaterials[i] != null) {
                        mat = renderer.sharedMaterials[i];
                    }
                    if(mat != null) materials.Add(mat.name, GetMaterialProperties(mat));
                }
            }
            if (materials != null) materials.Sort();
            return materials;
        }

        /// <summary>
        /// Returns a list of property names for the material/shader on the object
        /// </summary>
        public static SDictionary<string, Type> GetMaterialProperties(Material mat)
        {
            if (mat == null) {
                Debug.LogWarning("A null material was encountered. Please check for a missing property assignment.");
                return null;
            }
            if (mat.shader == null) {
                Debug.LogWarning("The material does not have a shader:" + mat.name);
                return null;
            }

            SDictionary<string, Type> properties = null;

#if UNITY_EDITOR
            properties = new SDictionary<string, Type>();

#if UNITY_6000_2_OR_NEWER
            int count = mat.shader.GetPropertyCount();
            for (int i = 0; i < count; i++) {
                UnityEngine.Rendering.ShaderPropertyType stype = mat.shader.GetPropertyType(i);
                string n = mat.shader.GetPropertyName(i);

                Type type = typeof(float);
                bool supported = false;
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Float) {
                    type = typeof(float);
                    supported = true;
                }
                else
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Int) {
                    type = typeof(int);
                    supported = true;
                }
                else
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Color) {
                    type = typeof(Color);
                    supported = true;
                }
                else
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Vector) {
                    type = typeof(Vector4);
                    supported = true;
                }
                else
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Range) {
                    if (mat.HasFloat(n)) {
                        /// float slider values are incorrectly reported as ranges but must actually be
                        /// handled as floats, otherwise the material can't find the property.
                        type = typeof(float);
                    }
                    else {
                        type = typeof(Vector2);
                    }
                    supported = true;
                }
                else
                if (stype == UnityEngine.Rendering.ShaderPropertyType.Texture) {
                    type = typeof(Vector2);
                    properties.Add(n + TextureOffsetTag, type);
                    properties.Add(n + TextureScaleTag, type);
                    type = typeof(Texture);
                    supported = true;
                }

                if (supported) {
                    properties.Add(n, type);
                }
            }
#else
            int count = ShaderUtil.GetPropertyCount(mat.shader);
            for (int i = 0; i < count; i++) {
                ShaderUtil.ShaderPropertyType stype = ShaderUtil.GetPropertyType(mat.shader, i);
                string n = ShaderUtil.GetPropertyName(mat.shader, i);

                Type type = typeof(float);
                bool supported = false;
                if (stype == ShaderUtil.ShaderPropertyType.Float) {
                    type = typeof(float);
                    supported = true;
                }
#if UNITY_2021_1_OR_NEWER
                else
                if (stype == ShaderUtil.ShaderPropertyType.Int) {
                    type = typeof(int);
                    supported = true;
                }
#endif
                else
                if (stype == ShaderUtil.ShaderPropertyType.Color) {
                    type = typeof(Color);
                    supported = true;
                }
                else
                if (stype == ShaderUtil.ShaderPropertyType.Vector) {
                    type = typeof(Vector4);
                    supported = true;
                }
                else
                if (stype == ShaderUtil.ShaderPropertyType.Range) {
#if UNITY_2021_1_OR_NEWER
                    if (mat.HasFloat(n)) {
#else
                    if (mat.HasProperty(n)) {
#endif
                        /// float slider values are incorrectly reported as ranges but must actually be
                        /// handled as floats, otherwise the material can't find the property.
                        type = typeof(float);
                    }
                    else {
                        type = typeof(Vector2);
                    }
                    supported = true;
                }
                else
                if (stype == ShaderUtil.ShaderPropertyType.TexEnv) {
                    type = typeof(Vector2);
                    properties.Add(n + TextureOffsetTag, type);
                    properties.Add(n + TextureScaleTag, type);
                    type = typeof(Texture);
                    supported = true;
                }

                if (supported) {
                    properties.Add(n, type);
                }
            }
#endif
#endif

            if (properties != null) properties.Sort();
            return properties;
        }

        #endregion

        #region ENUMS

        public enum PropertyTypes
        {
            Other,
            TextureOffset,
            TextureScale
        }

        #endregion

        #region PUBLIC

        public GameObject GameObject;
        public Renderer Renderer;
        public Material Material;
        public string MaterialName;
        public PropertyTypes PropertyType = PropertyTypes.Other;

        #endregion

        #region PRIVATE

        private int _nameHash;
        private string _nameInternal;
        private bool _isInitialized;
        private SDictionary<string, Type> _List;

        #endregion

        #region CONSTRUCTORS

        public PropertiesOfMaterial() { }

        public PropertiesOfMaterial(PropertiesOfMaterial copy)
        {
            MaterialName = copy.MaterialName;
            Renderer = copy.Renderer;
            Material = copy.Material;
            Name = copy.Name;
        }

        #endregion

        #region PROPERTIES

        public override SDictionary<string, Type> List {
            get {
                if (_List == null || _List.Count == 0) {
                    if (!_isInitialized || Material == null) GetMaterial();
                    if (Material == null) {
                        _List = null;
                    }
                    else {
                        _List = GetMaterialProperties(Material);
                    }
                }
                return _List;
            }
        }

        public override bool HasProperty(string name)
        {
            bool has = false;

            if (!string.IsNullOrEmpty(name)) {
                if (name.Contains(TextureOffsetTag)) {
                    has = true;
                }
                else
                if (name.Contains(TextureScaleTag)) {
                    has = true;
                }

                if (!has) {
                    has = Material != null && Material.HasProperty(name);
                }
            }
            return has;
        }

        #endregion

        #region ACCESSORS

        private int NameID {
            get {
                if (_nameHash == 0) {
                    GetNameHash();
                }
                return _nameHash;
            }
            set {
                _nameHash = value;
            }
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                if (_Object != value) {
                    _Object = value;
                    if (_Object == null) {
                        Renderer = null;
                    }
                    else {
                        _Object.TryGetComponent<Renderer>(out Renderer);
                    }
                    GameObject = _Object.gameObject;
                    GetMaterial();
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(Material);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                if (_Name != value) {
                    _Name = value;
                    GetNameHash();
                }
            }
        }

        #endregion

        #region MATERIALS

        public override void Refresh()
        {
            //Debug.Log($"<color=orange><PropertyHandler.Refresh (base)</color>");
            GetMaterial();
        }

        private void GetNameHash()
        {
            NameID = 0;
            if (!string.IsNullOrEmpty(Name)) {
                _nameInternal = Name;
                if (Name.Contains(TextureOffsetTag)) {
                    PropertyType = PropertyTypes.TextureOffset;
                    _nameInternal = Name.Replace(TextureOffsetTag, "");
                }
                else
                if (Name.Contains(TextureScaleTag)) {
                    PropertyType = PropertyTypes.TextureScale;
                    _nameInternal = Name.Replace(TextureScaleTag, "");
                }
                else {
                    PropertyType = PropertyTypes.Other;
                }
                if (Material != null && Material.shader != null) {
                    NameID = Shader.PropertyToID(_nameInternal);
                }
            }
        }

        /// <summary>
        /// This looks at all materials on the game object and determines the current material target by
        /// matching its name. Note that Name refers to the target property name, so MaterialName is used
        /// to identify the material.
        /// </summary>
        public Material GetMaterial()
        {
            _isInitialized = true;
            Material = null;
            //if (GameObject == null && Object == null) return null;
            if (GameObject == null) {
                LogWarning("GetMaterial", "GameObject is null");
                return null;
            }

            if (Renderer == null) GameObject.TryGetComponent<Renderer>(out Renderer);
            if (Renderer != null) {
                Material[] materials = null;

                bool forceSharedMaterials = IsForceSharedMaterials(Renderer);

                /// Make sure to repsect how unity handles materials differently in play mode than in edit
                /// mode. It's important to note as well that any shared materials will be updated
                /// simulataneously in edit mode, however in runtime will only affect the specific material
                /// instance it targets. This can be solved by linking channels or better yet use a shader
                /// global to set material properties globally to affect all instances.
                if (Application.isPlaying && !forceSharedMaterials) {
                    materials = Renderer.materials;
                }
                else {
                    materials = Renderer.sharedMaterials;
                }

                if (materials == null) {
                    LogWarning("No Materials", "Failed getting materials for:" + GameObject.name);
                    return null;
                }
                if (materials.Length == 1 || string.IsNullOrEmpty(MaterialName)) {
                    Material = materials[0];
                    if (Material != null) {
                        MaterialName = Material.name;
                    }
                }
                else {
                    for (int i = 0; i < materials.Length; i++) {
                        // Look for the material using StartsWith since Unity may add "(Instance)" to the name
                        if (materials[i] != null && materials[i].name.StartsWith(MaterialName)) {
                            Material = materials[i];
                            break;
                        }
                    }
                    if (Material == null) {
                        // default to the first material, helpful when switching source objects
                        Material = materials[0];
                    }
                }
                if (Material == null) {
                    LogWarning("Failed to find Material", MaterialName + " (" + Name + ")", GameObject);
                    NameID = 0;
                }
                else {
                    // Fetch the name hash to optimize read write ops
                    GetNameHash();

#if AXON_EXPERIMENTAL
                    /// This is a hack for emission in attempt to fix a problem in Unity where sometimes
                    /// emission won't turn on in the shader even though emissive values are set.
                    if (!string.IsNullOrEmpty(Name) && Name.Contains("Emiss")) {
                        Material.EnableKeyword("_EMISSION");
                    }
#endif
                }
            }

            if (Material == null) {
                SDictionary<string, Material> mats = GetExtraMaterialsForObject(GameObject);
                if (mats != null && mats.Count > 0) {
                    foreach (KeyValuePair<string, Material> m in mats) {
                        if (m.Key.StartsWith(MaterialName)) {
                            Material = m.Value;
                            break;
                        }
                    }
                }
            }
            return Material;
        }

        /// <summary>
        /// Returns an indexed list of all the materials and shader properties they contain. 
        /// </summary>
        public SDictionary<string, SDictionary<string, Type>> GetMaterialPropertiesForObject(GameObject obj)
        {
            SDictionary<string, SDictionary<string, Type>> materials = null;
            if (obj.TryGetComponent<Renderer>(out Renderer)) {
                materials = GetMaterialProperties(Renderer);
            }
            return materials;
        }

        public SDictionary<string, Material> GetExtraMaterialsForObject(GameObject obj)
        {
            SDictionary<string, Material> materials = null;

            // Find any additional materials that may be referenced in fields
            Component[] comps = obj.GetComponents<Component>();
            if (comps != null && comps.Length > 0) {
                foreach (Component comp in comps) {
                    if (comp == null) continue;
                    SDictionary<string, Material> mats = Property.GetMaterialProperties(comp, null);
                    if (mats != null && mats.Count > 0) {
                        if (materials == null) {
                            materials = mats;
                        }
                        else {
                            foreach (KeyValuePair<string, Material> m in mats) {
                                if (m.Value != null) {
                                    materials.Add(m.Key, m.Value);
                                }
                            }
                        }
                    }
                }
            }
            return materials;
        }

        #endregion

        #region GET VALUE

        public override bool GetBool()
        {
            bool value = false;
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
                    value = Material.GetFloat(NameID) > 0;
                }
                else {
                    LogWarning("Float Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                }
            }
            return value;
        }

        public override int GetInt()
        {
            int value = 0;
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
                    value = Material.GetInt(NameID);
                }
                else {
                    LogWarning("Int Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                }
            }
            return value;
        }

        public override float GetFloat()
        {
            float value = 0f;
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.Other) {
                    if (Material.HasProperty(NameID)) {
                        value = Material.GetFloat(NameID);
                    }
                    else {
                        LogWarning("Float Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                    }
                }
            }
            return value;
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.TextureOffset) {
                    value = Material.GetTextureOffset(NameID);
                }
                else
                if (PropertyType == PropertyTypes.TextureScale) {
                    value = Material.GetTextureScale(NameID);
                }
                else
                if (PropertyType == PropertyTypes.Other) {
                    if (Material.HasProperty(NameID)) {
                        value = Material.GetVector(NameID);
                    }
                    else {
                        LogWarning("Vector Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                    }
                }
            }
            return value;
        }

        public override Color GetColor()
        {
            Color value = Color.black;
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
                    value = Material.GetColor(NameID);
                }
                else {
                    LogWarning("Color Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                }
            }
            return value;
        }

        public override UnityEngine.Object GetObject()
        {
            UnityEngine.Object value = null;
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.Other) {
                    value = Material.GetTexture(NameID);
                }
            }
            return value;
        }

        #endregion

        #region SET VALUE
        /// <summary>
        /// Note that in URP and HDRP it is not longer optimal to use MaterialPropertyBlock so therefore it
        /// is not used here.
        /// </summary>

        public override void SetBool(bool value)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
                    Material.SetFloat(NameID, value ? 1f : 0f);
                }
                else {
                    LogWarning("Bool Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                }
            }
        }

        public override void SetInt(int value)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
#if UNITY_2021_1_OR_NEWER
                    Material.SetInteger(NameID, value);
#else
                    Material.SetFloat(NameID, value);
#endif
                }
                else {
                    LogWarning("Int Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                }
            }
        }

        public override void SetFloat(float value)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.Other) {
                    if (Material.HasProperty(NameID)) {
                        //Debug.Log($"{Material.name}.{Name} => {value} ({NameID})");
                        Material.SetFloat(NameID, value);
                    }
                    else {
                        LogWarning("Float Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                    }
                }
            }
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.TextureOffset) {
                    if (Material.HasTexture(NameID)) {
                        if (attribute <= -1) {
                            Material.SetTextureOffset(NameID, (Vector2)value);
                        }
                        else {
                            Vector2 v = Material.GetTextureOffset(NameID);
                            if (attribute == 0) {
                                v.x = value.x;
                            }
                            else
                            if (attribute == 1) {
                                v.y = value.x;
                            }
                            Material.SetTextureOffset(_nameInternal, v);
                        }
                    }
                }
                else
                if (PropertyType == PropertyTypes.TextureScale) {
                    if (Material.HasTexture(NameID)) {
                        if (attribute <= -1) {
                            Material.SetTextureScale(NameID, value);
                        }
                        else {
                            Vector2 v = Material.GetTextureScale(NameID);
                            if (attribute == 0) {
                                v.x = value.x;
                            }
                            else
                            if (attribute == 1) {
                                v.y = value.x;
                            }
                            Material.SetTextureScale(NameID, v);
                        }
                    }
                }
                else
                if (PropertyType == PropertyTypes.Other) {
                    if (Material.HasProperty(NameID)) {
                        if (attribute < 0) {
                            Material.SetVector(NameID, value);
                        }
                        else {
                            Vector4 v = Material.GetVector(NameID);
                            if (attribute == 0) {
                                v.x = value.x;
                            }
                            else
                            if (attribute == 1) {
                                v.y = value.x;
                            }
                            else
                            if (attribute == 2) {
                                v.z = value.x;
                            }
                            else
                            if (attribute == 3) {
                                v.w = value.x;
                            }
                            Material.SetVector(NameID, v);
                        }
                    }
                    else {
                        LogWarning("Vector Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                    }
                }
            }
        }

        public override void SetColor(Color value)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (Material.HasProperty(NameID)) {
                    Material.SetColor(NameID, value);
                }
                else {
                    LogWarning("Color Property Not Found", ObjectUtil.GetPath(GameObject) + "." + Material.name + ": missing shader property '" + Name + "' [" + NameID + "]:" + Renderer.name, GameObject);
                }
            }
            else {
                LogWarning("Missing Material", "The target object does not have a renderer or is missing a material.", GameObject);
            }
        }

        public override void SetObject(UnityEngine.Object value)
        {
            if (!_isInitialized) GetMaterial();
            if (Material != null && Renderer != null) {
                if (PropertyType == PropertyTypes.Other) {
                    if (Material.HasProperty(NameID)) {
                        Material.SetTexture(NameID, (Texture)value);
                    }
                    else {
                        LogWarning("Texture Property Not Found", ObjectUtil.GetPath(GameObject) + ": missing shader property '" + Name + "':" + Renderer.name, GameObject);
                    }
                }
            }
        }

        #endregion
    }

}//AxonGenesis

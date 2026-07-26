// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// A set of utility methods for working with Materials
    /// </summary>
    public static class MaterialUtil
    {
        public static void SetOpacity(GameObject obj, float value) { SetOpacity(obj, value, true); }

        public static void SetOpacity(GameObject obj, float value, bool recursive)
        {
            if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
                if (Application.isPlaying && renderer.material) {
                    renderer.material.SetFloat("_Opacity", value);
                }
                else
                if (renderer.sharedMaterial) {
                    renderer.sharedMaterial.SetFloat("_Opacity", value);
                }
            }
            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    SetOpacity(child.gameObject, value);
                }
            }
        }

        public static void SetShaderColor(GameObject obj, Color color, string colorChannel, bool recursive)
        {
            if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
                if (Application.isPlaying && renderer.material) {
                    renderer.material.SetColor(colorChannel, color);
                }
                else
                if (renderer.sharedMaterial) {
                    renderer.sharedMaterial.SetColor(colorChannel, color);
                }
            }
            if (recursive && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    SetShaderColor(child.gameObject, color, colorChannel, recursive);
                }
            }
        }

        public static void SetShaderFloat(GameObject obj, float value, string valueChannel, bool recursive)
        {
            if (obj.activeInHierarchy) {
                if (obj.TryGetComponent<Renderer>(out Renderer renderer)) {
                    if (Application.isPlaying && renderer.material) {
                        renderer.material.SetFloat(valueChannel, value);
                    }
                    else
                    if (renderer.sharedMaterial) {
                        renderer.sharedMaterial.SetFloat(valueChannel, value);
                    }
                }
                if (recursive && obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        SetShaderFloat(child.gameObject, value, valueChannel, recursive);
                    }
                }
            }
        }

        public static ArrayList GetMaterials(GameObject obj)
        {
            ArrayList mats = new ArrayList();

            Material mat = null;
            if (obj.TryGetComponent<Renderer>(out Renderer renderer)) mat = renderer.material;
            if (mat) {
                mats.Add(mat);
            }

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    ArrayList cms = GetMaterials(child.gameObject);
                    foreach (Material m in cms) {
                        mats.Add(m);
                    }
                }
            }
            return mats;
        }

#if UNITY_EDITOR

#if UNITY_6000_2_OR_NEWER
        public static SDictionary<string, UnityEngine.Rendering.ShaderPropertyType> GetMaterialPropertyNames(Material material)
        {
            if (material == null) return null;
            if (material.shader == null) {
                Debug.LogError("The material does not have a shader", material);
                return null;
            }

            SDictionary<string, UnityEngine.Rendering.ShaderPropertyType> properties = new SDictionary<string, UnityEngine.Rendering.ShaderPropertyType>();

            string log = "";
            int count = material.shader.GetPropertyCount();
            for (int i = 0; i < count; i++) {
                string n = material.shader.GetPropertyName(i);
                UnityEngine.Rendering.ShaderPropertyType type = material.shader.GetPropertyType(i);
                properties.Add(n, type);
                log += n + ":" + type + "\n";
            }
            Debug.Log("MATERIAL PROPERTIES: " + material.name + ":\n" + log);//--KEEP

            return properties;
        }
#else
        public static SDictionary<string, ShaderUtil.ShaderPropertyType> GetMaterialPropertyNames(Material material)
        {
            if (material == null) return null;
            if (material.shader == null) {
                Debug.LogError("The material does not have a shader", material);
                return null;
            }

            SDictionary<string, ShaderUtil.ShaderPropertyType> properties = new SDictionary<string, ShaderUtil.ShaderPropertyType>();

            string log = "";
            int count = ShaderUtil.GetPropertyCount(material.shader);
            for (int i = 0; i < count; i++) {
                string n = ShaderUtil.GetPropertyName(material.shader, i);
                ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(material.shader, i);
                properties.Add(n, type);
                log += n + ":" + type + "\n";
            }
            Debug.Log("MATERIAL PROPERTIES: " + material.name + ":\n" + log);//--KEEP

            return properties;
        }
#endif
#endif

    }

}//AxonGenesis
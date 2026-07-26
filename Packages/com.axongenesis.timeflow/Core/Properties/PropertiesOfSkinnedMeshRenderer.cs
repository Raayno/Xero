// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This provides access to blendshape weights on SkinnedMeshRenderer components.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfSkinnedMeshRenderer")]
    public class PropertiesOfSkinnedMeshRenderer : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        public int Index = -1;
        public SkinnedMeshRenderer Renderer;
        public SDictionary<string, int> Indices;

        public PropertiesOfSkinnedMeshRenderer() { }

        public override SDictionary<string, Type> List {
            get {
                if (_List == null && Renderer != null) {
                    _List = new SDictionary<string, Type>();
                    Mesh mesh = Renderer == null ? null : Renderer.sharedMesh;
                    if (mesh != null) {
                        if (mesh.blendShapeCount > 0) {
                            for (int i = 0; i < mesh.blendShapeCount; i++) {
                                _List.Add(mesh.GetBlendShapeName(i), typeof(float));
                            }
                        }
                    }
                }
                return _List;
            }
        }

        public override bool HasProperty(string name)
        {
            bool has = false;
            if (List != null && List.Count > 0) {
                has = List.ContainsKey(name);
            }
            return has;
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                if (_Object != value) {
                    _List = null;
                    _Object = value;
                    Renderer = _Object as SkinnedMeshRenderer;
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(SkinnedMeshRenderer);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
                Index = -1;
                if (string.IsNullOrEmpty(Name)) return;
                if (List != null && List.Count > 0) {
                    int i = 0;
                    foreach (KeyValuePair<string, Type> k in List) {
                        if (k.Key == _Name) {
                            Index = i;
                            break;
                        }
                        i++;
                    }
                }
                if (Index == -1) {
                    LogWarning("Missing Blendshape", "PropertiesOfSkinnedMeshRenderer failed to locate the blendshape named '" + _Name + "'");
                }
            }
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (Index >= 0 && Renderer != null) {
                value.x = Renderer.GetBlendShapeWeight(Index);
            }
            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (Index >= 0 && Renderer != null) {
                Renderer.SetBlendShapeWeight(Index, value.x);
            }
        }

        public override void SetFloat(float value)
        {
            if (Index >= 0 && Renderer != null) {
                Renderer.SetBlendShapeWeight(Index, value);
            }
        }

        public override void SetBool(bool value)
        {
            if (Index >= 0 && Renderer != null) {
                Renderer.SetBlendShapeWeight(Index, value ? 1 : 0);
            }
        }

        public override void SetInt(int value)
        {
            if (Index >= 0 && Renderer != null) {
                Renderer.SetBlendShapeWeight(Index, value);
            }
        }

        public override float GetFloat()
        {
            if (Index >= 0 && Renderer != null) {
                return Renderer.GetBlendShapeWeight(Index);
            }
            return 0f;
        }

        public override bool GetBool()
        {
            if (Index >= 0 && Renderer != null) {
                return Renderer.GetBlendShapeWeight(Index) > 0f;
            }
            return false;
        }

        public override int GetInt()
        {
            if (Index >= 0 && Renderer != null) {
                return (int)Renderer.GetBlendShapeWeight(Index);
            }
            return 0;
        }

    }

}//AxonGenesis

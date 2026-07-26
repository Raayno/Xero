// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AxonGenesis
{
    public class ComponentPresetPropertyTester : MonoBehaviour
    {
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue;
        public UnityEngine.Object ObjectReference;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public Vector4 Vector4Value;
        public Color ColorValue;
        public LayerMask LayerMaskValue;
        public Enum EnumValue;
        public Rect RectValue;
        public Vector2Int Vector2IntValue;
        public Vector3Int Vector3IntValue;
        public RectInt RectIntValue;
        public Bounds BoundsValue;
        public BoundsInt BoundsIntValue;
    }

}//AxonGenesis

#endif
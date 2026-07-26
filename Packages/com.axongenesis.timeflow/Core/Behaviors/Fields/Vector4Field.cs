// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [AddComponentMenu("Timeflow/Field/Vector4")]
    public class Vector4Field : FieldBase
    {
        [FormerlySerializedAs("Value")]
        public Vector4 Vector4 = Vector4.zero;
    }
}

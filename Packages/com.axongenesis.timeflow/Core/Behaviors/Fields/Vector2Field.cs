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
    [AddComponentMenu("Timeflow/Field/Vector2")]
    public class Vector2Field : FieldBase
    {
        [FormerlySerializedAs("Value")]
        public Vector2 Vector2 = Vector2.zero;
    }
}

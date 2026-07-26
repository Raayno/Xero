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
    [Serializable]
    public class Curve
    {
        [SerializeField]
        private List<Keyframe> _Keys = null;

        public List<Keyframe> Keys {
            get {
                if (_Keys == null) _Keys = new List<Keyframe>();
                return _Keys;
            }
            set {
                _Keys = value;
            }
        }

    }
}

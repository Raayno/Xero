// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

#if UNITY_EDITOR
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Base class to implement a custom path provider for interpolation. This is used
    /// by PlaceOnPath to animate splines and other custom paths.
    /// </summary>
    public class PathProvider : AxonGenesisBehavior
    {

        public virtual float Length {
            get {
                return 1f;
            }
        }

        public virtual void Interpolate(float amount, out Vector3 position, out Quaternion rotation)
        {
            position = MathUtil.Interpolate(Vector3.zero, Vector3.forward, amount);
            rotation = Quaternion.identity;
        }
    }

}//AxonGenesis
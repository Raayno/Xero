// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{

    /// <summary>
    /// Use this behavior to apply constant rotation to a single axis. Use this instead of
    /// Timeflow animation for optimal performance where addditional interpolation isn't needed.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Timeflow/Optimized/Rotation")]
    public class OptimizedRotation : OptimizedBehavior
    {
        public enum Axes
        {
            X,
            Y,
            Z
        }
        public Axes Axis = Axes.Y;

        public float Speed = 10f;

        public float rotate = 0f;

        void Update()
        {
            if (!CanUpdate) return;

            rotate = Speed * Time.deltaTime;
            Vector3 rotation = Vector3.zero;

            switch (Axis) {
                case Axes.X:
                    rotation.x = rotate;
                    break;
                case Axes.Y:
                    rotation.y = rotate;
                    break;
                default:
                    rotation.z = rotate;
                    break;
            }
            //if (DebugEnabled) Debug.Log($"{name}.OptimizedRotation:{rotate} deltaTime:{Time.deltaTime} timeScale:{Time.timeScale}");

            transform.Rotate(rotation);
        }
    }
}

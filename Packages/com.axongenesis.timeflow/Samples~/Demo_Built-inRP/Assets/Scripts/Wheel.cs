// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class Wheel : MonoBehaviour
    {
        public float Radius = 1f;

        private Vector3 lastPosition = Vector3.zero;
        private Vector3 euler = Vector3.zero;

        private void Update()
        {
            // Calculate the world distance traveled since the last update
            float deltaDistance = MathUtil.Distance(transform.position, lastPosition);

            // Calculate the amount of rotation based on the wheel radius
            float circumference = 2f * Radius * Mathf.PI;
            float spin = (deltaDistance / circumference) * 360f;

            // Apply the total rotation to the X axis
            euler.x += spin;
            transform.localEulerAngles = euler;

            lastPosition = transform.position;
        }
    }
}
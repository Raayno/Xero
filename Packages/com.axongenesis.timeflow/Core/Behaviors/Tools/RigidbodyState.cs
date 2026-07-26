// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    public class RigidbodyState
    {
        [SerializeField] public bool Enabled = true;
        [SerializeField] public bool Foldout = false;
        [SerializeField] public Rigidbody Rigidbody;
        [SerializeField] public Vector3 Position;
        [SerializeField] public Quaternion Rotation;
        [SerializeField] public Vector3 Velocity;
        [SerializeField] public Vector3 AngularVelocity;
    }
}//AxonGenesis

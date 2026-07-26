// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This class abstracts Rigidbody and Rigidbody2D operations
    /// </summary>
    public class RigidbodyHelper
    {
        public Rigidbody Rigidbody;
        public Rigidbody2D Rigidbody2D;

        public RigidbodyHelper(GameObject obj)
        {
            GetRigidbody(obj);
        }

        public bool HasBody {
            get {
                return Rigidbody != null || Rigidbody2D != null;
            }
        }

        public bool Is2D {
            get {
                return Rigidbody2D != null;
            }
        }

        public bool useGravity {
            get {
                if (Rigidbody != null) return Rigidbody.useGravity;
                if (Rigidbody2D != null) return Rigidbody2D.gravityScale > 0;
                return false;
            }
            set {
                if (Rigidbody != null) Rigidbody.useGravity = value;
                if (Rigidbody2D != null) Rigidbody2D.gravityScale = value ? 1 : 0;
            }
        }

        public Vector3 velocity {
            get {
#if UNITY_6000_0_OR_NEWER
                if (Rigidbody != null) return Rigidbody.linearVelocity;
                if (Rigidbody2D != null) return Rigidbody2D.linearVelocity;
#else
                if (Rigidbody != null) return Rigidbody.velocity;
                if (Rigidbody2D != null) return Rigidbody2D.velocity;
#endif
                return Vector3.zero;
            }
            set {
#if UNITY_6000_0_OR_NEWER
                if (Rigidbody != null) Rigidbody.linearVelocity = value;
                if (Rigidbody2D != null) Rigidbody2D.linearVelocity = value;
#else
                if (Rigidbody != null) Rigidbody.velocity = value;
                if (Rigidbody2D != null) Rigidbody2D.velocity = value;
#endif
            }
        }

        public Vector3 angularVelocity {
            get {
                if (Rigidbody != null) return Rigidbody.angularVelocity;
                if (Rigidbody2D != null) return new Vector3(Rigidbody2D.angularVelocity, 0, 0f);
                return Vector3.zero;
            }
            set {
                if (Rigidbody != null) Rigidbody.angularVelocity = value;
                if (Rigidbody2D != null) Rigidbody2D.angularVelocity = value.x;
            }
        }

        public void GetRigidbody(GameObject obj)
        {
            if (obj == null) return;
            obj.TryGetComponent<Rigidbody>(out Rigidbody);
            obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D);
        }

        public void AddRigidbody(GameObject obj)
        {
            if (obj == null) return;
            if (!obj.TryGetComponent<Rigidbody>(out Rigidbody) && !obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D)) {
                bool is2d = false;

#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView.in2DMode) {
                    is2d = true;
                }
                if (is2d) {
                    Rigidbody2D = Undo.AddComponent<Rigidbody2D>(obj);
                    Rigidbody = null;
                }
                else {
                    Rigidbody = Undo.AddComponent<Rigidbody>(obj);
                    Rigidbody2D = null;
                }
#else
                if (is2d) {
                    Rigidbody2D = obj.AddComponent<Rigidbody2D>();
                    Rigidbody = null;
                }
                else {
                    Rigidbody = obj.AddComponent<Rigidbody>();
                    Rigidbody2D = null;
                }
#endif
            }
        }

        public void MovePosition(Vector3 move)
        {
            if (Rigidbody != null) {
                Rigidbody.MovePosition(move);
            }
            else
            if (Rigidbody2D != null) {
                Rigidbody2D.MovePosition(move);
            }
        }

        public void MoveRotation(Quaternion rotate)
        {
            if (Rigidbody != null) {
                Rigidbody.MoveRotation(rotate);
            }
            else
            if (Rigidbody2D != null) {
                Rigidbody2D.MoveRotation(rotate);
            }
        }

        public void AddForce(Vector3 force, ForceMode mode)
        {
            if (Rigidbody != null) {
                Rigidbody.AddForce(force, mode);
            }
            else
            if (Rigidbody2D != null) {
                Rigidbody2D.AddForce(force, ConvertForceModeTo2D(mode));
            }

        }


        public void AddTorque(Vector3 force, ForceMode mode)
        {
            if (Rigidbody != null) {
                Rigidbody.AddTorque(force, mode);
            }
            else
            if (Rigidbody2D != null) {
                Rigidbody2D.AddTorque(force.x, ConvertForceModeTo2D(mode));
            }

        }

        public static ForceMode2D ConvertForceModeTo2D(ForceMode mode)
        {
            ForceMode2D mode2D = ForceMode2D.Force;

            if (mode == ForceMode.Impulse) {
                mode2D = ForceMode2D.Impulse;
            }
            return mode2D;
        }
    }

}//AxonGenesis

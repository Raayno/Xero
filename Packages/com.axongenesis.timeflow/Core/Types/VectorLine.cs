// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;

namespace AxonGenesis
{
    public struct VectorLine
    {
        public Vector2 A;
        public Vector2 B;

        public VectorLine(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public VectorLine(float ax, float ay, float bx, float by)
        {
            A.x = ax;
            A.y = ay;
            B.x = bx;
            B.y = by;
        }

        public VectorLine(int ax, int ay, int bx, int by)
        {
            A.x = ax;
            A.y = ay;
            B.x = bx;
            B.y = by;
        }

        public float x {
            get {
                return A.x;
            }
            set {
                A.x = B.x = value;
            }
        }

        public float y {
            get {
                return A.y;
            }
            set {
                A.y = B.y = value;
            }
        }
    }

}//AxonGenesis

#endif

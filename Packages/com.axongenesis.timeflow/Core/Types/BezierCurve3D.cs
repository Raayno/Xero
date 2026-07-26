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
    /// Defines a 3D bezier curve used for interpolating in 3D space.
    /// </summary>
    public class BezierCurve3D
    {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        public Vector3 P3;

        public bool IsBaked;
        public int BakeResolution = 128;
        public Vector3[] BakedPath;
        public float[] BakedLengths;

        private float _BakedLength;
        public float BakedLength {
            get {
                if (!IsBaked) Bake();
                return _BakedLength;
            }
        }

        public BezierCurve3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public Vector3 GetPointAtTime(float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * P0; //first term
            p += 3 * uu * t * P1; //second term
            p += 3 * u * tt * P2; //third term
            p += ttt * P3; //fourth term

            return p;
        }

        public Vector3 GetPointAtLinearTime(float t)
        {
            Vector3 point = Vector3.zero;
            if (!IsBaked) {
                Bake();
            }

            float length = 0f;
            float linearDistance = t * BakedLength;
            for (int i = 0; i < BakeResolution; i++) {
                if (i > 0) {
                    float n = length + BakedLengths[i];
                    if (n > linearDistance) {
                        float d = linearDistance - length;
                        Vector3 vec = BakedPath[i] - BakedPath[i - 1];
                        point = BakedPath[i - 1] + (vec.normalized * d);
                        break;
                    }
                    length += BakedLengths[i];
                }
            }

            return point;
        }

        public void Bake()
        {
            IsBaked = true;
            _BakedLength = 0f;
            BakedPath = new Vector3[BakeResolution];
            BakedLengths = new float[BakeResolution];
            BakedLengths[0] = 0;
            float res = (float)BakeResolution - 1f;
            if (res <= 0) return;

            for (int i = 0; i < BakeResolution; i++) {
                float t = (float)i / res;
                BakedPath[i] = GetPointAtTime(t);
                if (i > 0) {
                    BakedLengths[i] = MathUtil.Distance(BakedPath[i], BakedPath[i - 1]);
                    _BakedLength += BakedLengths[i];
                }
            }

        }
    }

}//AxonGenesis
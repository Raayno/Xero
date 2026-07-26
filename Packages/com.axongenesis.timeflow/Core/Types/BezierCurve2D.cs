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
    /// Defines a 2D bezier curve used in GUI drawing.
    /// </summary>
    public class BezierCurve2D
    {
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 P3;

        public BezierCurve2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        /// <summary>
        /// Value t is not normalized, but relative to input range (P0.x to P3.x)
        /// </summary>
        public float GetAverage(float t, int samples = 32)
        {
            if (samples <= 0) return t;
            float avg = 0;
            float inc = (t - P0.x) / (float)samples;
            float samp = P0.x;

            for (int i = 0; i <= samples; i++) {
                avg += GetValue(samp);
                samp += inc;
            }
            avg /= (float)samples + 1f;

            return avg;
        }

        /// <summary>
        /// Value t is not normalized, but relative to input range (P0.x to P3.x)
        /// </summary>
        public float GetValue(float t)
        {
            float x = 0;
            if (t == P0.x) {
                x = 0;
            }
            else
            if (t == P3.x) {
                x = 1f;
            }
            else {
                float a = -P0.x + 3f * P1.x - 3f * P2.x + P3.x;
                float b = 3f * P0.x - 6f * P1.x + 3f * P2.x;
                float c = -3f * P0.x + 3f * P1.x;
                float d = P0.x - t;
                x = SolveCubic(a, b, c, d);
            }

            return Cubed(1f - x) * P0.y
                + 3f * x * Squared(1f - x) * P1.y
                + 3f * Squared(x) * (1f - x) * P2.y
                + Cubed(x) * P3.y;
        }

        private static float SolveCubic(float a, float b, float c, float d)
        {
            if (a == 0) return SolveQuadratic(b, c, d);
            if (d == 0) return 0;

            b /= a;
            c /= a;
            d /= a;
            float q = (3f * c - Squared(b)) / 9f;
            float r = (-27f * d + b * (9f * c - 2f * Squared(b))) / 54f;
            float disc = Cubed(q) + Squared(r);
            float term1 = b / 3f;

            if (disc > 0) {
                float s = r + Mathf.Sqrt(disc);
                s = (s < 0) ? -CubicRoot(-s) : CubicRoot(s);
                float t = r - Mathf.Sqrt(disc);
                t = (t < 0) ? -CubicRoot(-t) : CubicRoot(t);

                float result = -term1 + s + t;
                if (result >= 0 && result <= 1) return result;
            }
            else if (disc == 0) {
                float r13 = (r < 0) ? -CubicRoot(-r) : CubicRoot(r);

                float result = -term1 + 2f * r13;
                if (result >= 0 && result <= 1) return result;

                result = -(r13 + term1);
                if (result >= 0 && result <= 1) return result;
            }
            else
            if (q != 0) {
                q = -q;
                float dum1 = q * q * q;
                dum1 = Mathf.Acos(r / Mathf.Sqrt(dum1));
                float r13 = 2f * Mathf.Sqrt(q);

                float result = -term1 + r13 * Mathf.Cos(dum1 / 3f);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Mathf.Cos((dum1 + 2f * Mathf.PI) / 3f);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Mathf.Cos((dum1 + 4f * Mathf.PI) / 3f);
                if (result >= 0 && result <= 1) return result;
            }

            return 0;
        }

        private static float SolveQuadratic(float a, float b, float c)
        {
            if (a == 0) return 0;
            float a2 = 2f * a;
            float bsq = Mathf.Sqrt(Squared(b) - 4f * a * c);

            float result = (-b + bsq) / a2;
            if (result >= 0 && result <= 1f) return result;

            result = (-b - bsq) / a2;
            if (result >= 0 && result <= 1f) return result;

            return 0;
        }

        private static float Squared(float f) { return f * f; }

        private static float Cubed(float f) { return f * f * f; }

        private static float CubicRoot(float f) { return Mathf.Pow(f, 1f / 3f); }

        public static void CalculateTangents(Vector2 p0, Vector2 p1, Vector2 p2,
            Vector2 p0_OutTan, Vector2 p2_InTan, ref Vector2 inTangent, ref Vector2 outTangent, bool lockAngle)
        {
            float inLength = MathUtil.Distance(p0, p1) * 0.25f;
            float outLength = MathUtil.Distance(p1, p2) * 0.25f;

            // Prevent tangent lengths from crossing over neighbors
            float inMax = Mathf.Abs(p1.x - p0.x) * 0.5f;
            float outMax = Mathf.Abs(p2.x - p1.x) * 0.5f;
            inLength = Mathf.Min(inLength, inMax);
            outLength = Mathf.Min(outLength, outMax);
            inLength = Mathf.Min(inLength, outMax);
            outLength = Mathf.Min(outLength, inMax);

            // Calculate a standard length 20% of the smaller side of the curve so that tangents
            // will be smaller in denser packed areas
            float length = Mathf.Min(Mathf.Abs(p1.x - p0.x), Mathf.Abs(p2.x - p1.x)) * 0.2f;

            Vector2 t0 = new Vector2(p0.x + p0_OutTan.x, p0.y + p0_OutTan.y);
            Vector2 t2 = new Vector2(p2.x + p2_InTan.x, p2.y + p2_InTan.y);

            // To prevent tangents from reversing when tangents overshoot
            if (t0.x > p1.x) t0 = p0;
            if (t2.x < p1.x) t2 = p2;

            float tmin = p1.x - (inLength * 2f);
            if (tmin < t0.x) {
                inLength = (p1.x - t0.x) * 0.25f;
            }
            float tmax = p1.x + (outLength * 2f);
            if (tmax > t2.x) {
                outLength = (t2.x - p1.x) * 0.25f;
            }

            float angle1 = MathUtil.Angle(p1, t0);
            Vector2 inTemp = MathUtil.VectorFromAngle(angle1);

            float angle2 = MathUtil.Angle(p1, t2);
            Vector2 outTemp = MathUtil.VectorFromAngle(angle2);

            // When the 2 end points are both above or below the midpoint, flatten them
            if ((p0.y < p1.y && p2.y < p1.y) || (p0.y > p1.y && p2.y > p1.y)) {
                inTemp.x = -inLength;
                outTemp.x = outLength;

                float avgy = 0f;
                outTemp.y = inTemp.y = avgy;
                inTangent = inTemp;
                outTangent = outTemp;
            }
            else {
                // Average the in and out tangents calculations for unified direction
                Vector2 avg = Vector2.zero;
                avg.x = (inTemp.x - outTemp.x) * 0.5f;
                avg.y = (inTemp.y - outTemp.y) * 0.5f;

                if (avg.x == 0) {
                    inTemp.y = 0;
                    inTemp.x = 0;

                    outTemp.y = 0;
                    outTemp.x = 0;
                }
                else {
                    inTemp.y *= -inLength / avg.x;
                    inTemp.x = -inLength;

                    outTemp.y *= outLength / -avg.x;
                    outTemp.x = outLength;
                }

                inTangent = inTemp;

                if (lockAngle) {
                    // Find a balance between the two directions tangents want to go
                    outTangent = MathUtil.Average(outTemp, outTangent);
                }
                else {
                    outTangent = outTemp;
                }
            }
        }
    }

}//AxonGenesis
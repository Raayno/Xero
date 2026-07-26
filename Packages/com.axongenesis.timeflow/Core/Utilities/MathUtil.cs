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
    /// A large collection of math utility functions used extensively throughout the AxonGenesis code base.
    /// </summary>
    public static class MathUtil
    {
        #region RANDOM

        private static readonly System.Random random = new System.Random();

        public static float Random(float min = 0f, float max = 1f)
        {
            float v = _Random(min, max);
            while (v == min) v = _Random(min, max);
            while (v == max) v = _Random(min, max);
            return v;
        }

        public static int Random(int min, int max)
        {
            int v = Mathf.RoundToInt(_Random(min, max));
            while (v == min) v = Mathf.RoundToInt(_Random(min, max));
            while (v == max) v = Mathf.RoundToInt(_Random(min, max));
            return v;
        }

        public static float _Random(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        #endregion

        #region VALIDATION

        public static bool IsOdd(float v)
        {
            return v % 2 != 0;
        }

        public static bool IsSameSign(float a, float b)
        {
            return (a <= 0 && b <= 0) || (a >= 0 && b >= 0);
        }

        public static bool IsNaN(float v)
        {
            return float.IsNaN(v) || float.IsInfinity(v);
        }

        public static bool IsNaN(Vector2 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.x) || float.IsInfinity(v.y);
        }

        public static bool IsNaN(Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
        }

        public static bool IsNaN(Vector4 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsNaN(v.w) ||
                float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z) || float.IsInfinity(v.w);
        }

        public static bool IsNaN(Rect v)
        {
            return float.IsNaN(v.xMin) || float.IsNaN(v.yMin) || float.IsNaN(v.xMax) || float.IsNaN(v.yMax) ||
                float.IsInfinity(v.xMin) || float.IsInfinity(v.yMin) || float.IsInfinity(v.xMax) || float.IsInfinity(v.xMax);
        }

        public static bool IsNaN(RectOffset v)
        {
            return float.IsNaN(v.left) || float.IsNaN(v.right) || float.IsNaN(v.top) || float.IsNaN(v.bottom) ||
                float.IsInfinity(v.left) || float.IsInfinity(v.right) || float.IsInfinity(v.top) || float.IsInfinity(v.bottom);
        }

        public static bool IsNaN(Color v)
        {
            return float.IsNaN(v.r) || float.IsNaN(v.g) || float.IsNaN(v.b) || float.IsNaN(v.a) || float.IsInfinity(v.r) || float.IsInfinity(v.g) || float.IsInfinity(v.b) || float.IsInfinity(v.a);
        }

        public static float Validate(float value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = 0;
            }
            return value;
        }

        public static Vector4 Validate(Vector2 value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = Vector2.zero;
            }
            return value;
        }

        public static Vector3 Validate(Vector3 value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = Vector3.zero;
            }
            return value;
        }

        public static Vector4 Validate(Vector4 value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = Vector4.zero;
            }
            return value;
        }

        public static Rect Validate(Rect value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = Rect.zero;
            }
            return value;
        }

        public static RectOffset Validate(RectOffset value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = new RectOffset(0, 0, 0, 0);
            }
            return value;
        }

        public static Color Validate(Color value)
        {
            if (MathUtil.IsNaN(value)) {
                Debug.LogWarning("NaN or infinite value encountered:" + value);
                value = Color.black;
            }
            return value;
        }

        public static bool IsDifferent(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) > tolerance;
        }

        public static bool IsDifferent(Vector2 a, Vector2 b, float tolerance)
        {
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance);
        }

        public static bool IsDifferent(Vector3 a, Vector3 b, float tolerance)
        {
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance) || IsDifferent(a.z, b.z, tolerance);
        }

        public static bool IsDifferent(Vector4 a, Vector4 b, float tolerance)
        {
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance) || IsDifferent(a.z, b.z, tolerance) || IsDifferent(a.w, b.w, tolerance);
        }

        public static bool IsDifferent(Color a, Color b, float tolerance)
        {
            return IsDifferent(a.r, b.r, tolerance) || IsDifferent(a.g, b.g, tolerance) || IsDifferent(a.b, b.b, tolerance) || IsDifferent(a.a, b.a, tolerance);
        }

        public static bool IsDifferent(Rect a, Rect b, float tolerance)
        {
            return IsDifferent(a.xMin, b.xMin, tolerance) || IsDifferent(a.yMin, b.yMin, tolerance) || IsDifferent(a.xMax, b.xMax, tolerance) || IsDifferent(a.yMax, b.yMax, tolerance);
        }

        public static bool IsDifferent(RectOffset a, RectOffset b, float tolerance)
        {
            return IsDifferent(a.left, b.left, tolerance) || IsDifferent(a.right, b.right, tolerance) || IsDifferent(a.top, b.top, tolerance) || IsDifferent(a.bottom, b.bottom, tolerance);
        }

        public static bool Overlaps(float aStart, float aEnd, float bStart, float bEnd)
        {
            return aStart >= bStart && aStart <= bEnd ||
                aEnd >= bStart && aEnd <= bEnd ||
                aStart <= bStart && aEnd >= bEnd;
        }

        public static bool OverlapsNoCoincidence(float aStart, float aEnd, float bStart, float bEnd)
        {
            return aStart > bStart && aStart < bEnd ||
                aEnd > bStart && aEnd < bEnd ||
                aStart < bStart && aEnd > bEnd;
        }

        #endregion

        #region RANDOM

        public static int RandomSeed; // Use NextRandomSeed to get a globally new random seed

        public static int NextRandomSeed()
        {
            return RandomSeed++;
        }

        public static float Random(float plusminus)
        {
            return (UnityEngine.Random.value - 0.5f) * 2.0f * plusminus;
        }

        public static Vector2 Random(Vector2 randVector)
        {
            Vector2 v;
            v.x = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            return v;
        }

        public static Vector3 Random(Vector3 randVector)
        {
            Vector3 v;
            v.x = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            v.z = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.z;
            return v;
        }

        public static Vector4 Random(Vector4 randVector)
        {
            Vector4 v;
            v.x = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            v.z = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.z;
            v.w = (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.w;
            return v;
        }

        public static float RandomRange(float min, float max)
        {
            float range = max - min;
            return (UnityEngine.Random.value * range) + min;
        }

        public static float Randomize(float input, float random)
        {
            return input + ((UnityEngine.Random.value - 0.5f) * random);
        }

        public static float RandomizeScale(float inValue, float randValue)
        {
            if (randValue == 0f) return inValue;
            float r = (UnityEngine.Random.value - 0.5f) * 2.0f * randValue;
            inValue += r * inValue;
            return inValue;
        }

        public static Vector3 RandomizeScale(Vector3 inVector, float randValue)
        {
            Vector3 v = inVector;
            float r = (UnityEngine.Random.value - 0.5f) * 2.0f * randValue;
            v.x += r * v.x;
            v.y += r * v.y;
            v.z += r * v.z;
            return v;
        }

        public static Vector3 RandomizeScale(Vector3 inVector, Vector3 randVector)
        {
            Vector3 v = inVector;
            v.x += v.x * (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y += v.y * (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            v.z += v.z * (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.z;
            return v;
        }

        public static Vector3 Randomize(Vector3 inVector, Vector3 randVector)
        {
            Vector3 v = inVector;
            v.x += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            v.z += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.z;
            return v;
        }

        public static Vector4 Randomize(Vector4 inVector, Vector4 randVector)
        {
            Vector4 v = inVector;
            v.x += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.x;
            v.y += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.y;
            v.z += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.z;
            v.w += (UnityEngine.Random.value - 0.5f) * 2.0f * randVector.w;
            return v;
        }

        public static Vector4 RandomRange(Vector4 min, Vector4 max)
        {
            min.x += UnityEngine.Random.value * (max.x - min.x);
            min.y += UnityEngine.Random.value * (max.y - min.y);
            min.z += UnityEngine.Random.value * (max.z - min.z);
            min.w += UnityEngine.Random.value * (max.w - min.w);
            return min;
        }

        #endregion

        #region LIMITS & RULES

        public static Vector3 Abs(Vector3 v)
        {
            v.x = Mathf.Abs(v.x);
            v.y = Mathf.Abs(v.y);
            v.z = Mathf.Abs(v.z);
            return v;
        }

        public static Vector3 Invert(Vector3 v)
        {
            v.x = -v.x;
            v.y = -v.y;
            v.z = -v.z;
            return v;
        }

        public static float Round(float value, int precision)
        {
            float p1 = 1.0f;
            float p2 = 1.0f;

            if (precision > 0) {
                p1 = Mathf.Pow(10.0f, precision);
                p2 = Mathf.Pow(0.1f, precision);
            }

            return Mathf.Round(value * p1) * p2;
        }

        public static float RoundToDecimal(float value, float dec)
        {
            if (dec == 0f) {
                return 0f;
            }
            return Mathf.Floor(value * dec) / dec;
        }

        public static float RoundToInterval(float value, float interval)
        {
            if (interval <= 0f) return Mathf.Floor(value);
            return Mathf.Round(value / interval) * interval;
        }

        public static float Loop(float v, float min, float max)
        {
            if (v == min || v == max) return v;
            v = Mathf.Repeat(v - min, max - min) + min;
            return v;
        }

        public static int Loop(int v, int min, int max)
        {
            if (v == min || v == max) return v;
            if (min >= max) return min;
            v = (int)Mathf.Repeat(v - min, max - min) + min;
            return v;
        }

        public static Vector2 UniformVector2(float value)
        {
            return new Vector2(value, value);
        }

        public static Vector3 UniformVector3(float value)
        {
            return new Vector3(value, value, value);
        }

        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            a.x = Mathf.Min(a.x, b.x);
            a.y = Mathf.Min(a.y, b.y);
            return a;
        }

        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            a.x = Mathf.Min(a.x, b.x);
            a.y = Mathf.Min(a.y, b.y);
            a.z = Mathf.Min(a.z, b.z);
            return a;
        }

        public static Vector4 Min(Vector4 a, Vector4 b)
        {
            a.x = Mathf.Min(a.x, b.x);
            a.y = Mathf.Min(a.y, b.y);
            a.z = Mathf.Min(a.z, b.z);
            a.w = Mathf.Min(a.w, b.w);
            return a;
        }

        public static Rect Min(Rect a, Rect b)
        {
            a.xMin = Mathf.Min(a.xMin, b.xMin);
            a.yMin = Mathf.Min(a.yMin, b.yMin);
            a.xMax = Mathf.Min(a.xMax, b.xMax);
            a.yMax = Mathf.Min(a.yMax, b.yMax);
            return a;
        }

        public static RectOffset Min(RectOffset a, RectOffset b)
        {
            a.left = Mathf.Min(a.left, b.left);
            a.right = Mathf.Min(a.right, b.right);
            a.top = Mathf.Min(a.top, b.top);
            a.bottom = Mathf.Min(a.bottom, b.bottom);
            return a;
        }

        public static Color Min(Color a, Color b)
        {
            a.r = Mathf.Min(a.r, b.r);
            a.g = Mathf.Min(a.g, b.g);
            a.b = Mathf.Min(a.b, b.b);
            a.a = Mathf.Min(a.a, b.a);
            return a;
        }

        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            a.x = Mathf.Max(a.x, b.x);
            a.y = Mathf.Max(a.y, b.y);
            return a;
        }

        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            a.x = Mathf.Max(a.x, b.x);
            a.y = Mathf.Max(a.y, b.y);
            a.z = Mathf.Max(a.z, b.z);
            return a;
        }

        public static Vector4 Max(Vector4 a, Vector4 b)
        {
            a.x = Mathf.Max(a.x, b.x);
            a.y = Mathf.Max(a.y, b.y);
            a.z = Mathf.Max(a.z, b.z);
            a.w = Mathf.Max(a.w, b.w);
            return a;
        }

        public static Rect Max(Rect a, Rect b)
        {
            a.xMin = Mathf.Max(a.xMin, b.xMin);
            a.yMin = Mathf.Max(a.yMin, b.yMin);
            a.xMax = Mathf.Max(a.xMax, b.xMax);
            a.yMax = Mathf.Max(a.yMax, b.yMax);
            return a;
        }

        public static RectOffset Max(RectOffset a, RectOffset b)
        {
            a.left = Mathf.Max(a.left, b.left);
            a.right = Mathf.Max(a.right, b.right);
            a.top = Mathf.Max(a.top, b.top);
            a.bottom = Mathf.Max(a.bottom, b.bottom);
            return a;
        }

        public static Color Max(Color a, Color b)
        {
            a.r = Mathf.Max(a.r, b.r);
            a.g = Mathf.Max(a.g, b.g);
            a.b = Mathf.Max(a.b, b.b);
            a.a = Mathf.Max(a.a, b.a);
            return a;
        }

        public static float MinMax(float v, float min, float max)
        {
            return Mathf.Min(max, Mathf.Max(min, v));
        }

        public static Vector2 MinMax(Vector2 v, float min, float max)
        {
            v.x = Mathf.Min(max, Mathf.Max(min, v.x));
            v.y = Mathf.Min(max, Mathf.Max(min, v.y));
            return v;
        }

        public static Vector3 MinMax(Vector3 v, Vector3 min, Vector3 max)
        {
            v.x = Mathf.Min(max.x, Mathf.Max(min.x, v.x));
            v.y = Mathf.Min(max.y, Mathf.Max(min.y, v.y));
            v.z = Mathf.Min(max.z, Mathf.Max(min.z, v.z));
            return v;
        }

        public static float Wrap(float v)
        {
            return v - Mathf.Floor(v);
        }

        public static float Wrap90(float v)
        {
            while (v > 180f) {
                v -= 180f;
            }
            while (v < -180f) {
                v += 180f;
            }
            if (v > 90) {
                v -= 180f;
            }
            else
            if (v < -90) {
                v += 180f;
            }
            return v;
        }

        public static Vector3 Wrap90(Vector3 v)
        {
            v.x = Wrap90(v.x);
            v.y = Wrap90(v.y);
            v.z = Wrap90(v.z);
            return v;
        }

        public static float Wrap180(float v)
        {
            while (v > 180f) {
                v -= 180f;
            }
            while (v < -180f) {
                v += 180f;
            }
            return v;
        }

        public static Vector3 Wrap180(Vector3 v)
        {
            v.x = Wrap180(v.x);
            v.y = Wrap180(v.y);
            v.z = Wrap180(v.z);
            return v;
        }

        public static float Wrap360(float v)
        {
            while (v > 360f) {
                v -= 360f;
            }
            while (v < 0f) {
                v += 360f;
            }
            return v;
        }

        public static Vector3 Wrap360(Vector3 v)
        {
            v.x = Wrap360(v.x);
            v.y = Wrap360(v.y);
            v.z = Wrap360(v.z);
            return v;
        }

        public static float Correct180(float v)
        {
            v = Wrap360(v);
            if (v < -180f) {
                v = 360f + v;
            }
            else
            if (v > 180f) {
                v = -(360f - v);
            }
            return v;
        }

        public static Vector3 Correct180(Vector3 v)
        {
            v.x = Correct180(v.x);
            v.y = Correct180(v.y);
            v.z = Correct180(v.z);
            return v;
        }

        public static float PingPong(float v, float min, float max)
        {
            v = Mathf.PingPong(v - min, max - min) + min;
            return v;
        }

        public static int Clamp(int a, int min, int max)
        {
            if (a < min) a = min;
            else
            if (a > max) a = max;
            return a;
        }

        public static Vector3 Clamp(Vector3 a, Vector3 minmax)
        {
            a.x = Mathf.Clamp(a.x, -minmax.x, minmax.x);
            a.y = Mathf.Clamp(a.y, -minmax.y, minmax.y);
            a.z = Mathf.Clamp(a.z, -minmax.z, minmax.z);
            return a;
        }

        public static Vector3 Clamp(Vector3 a, Vector3 min, Vector3 max)
        {
            a.x = Mathf.Clamp(a.x, min.x, max.x);
            a.y = Mathf.Clamp(a.y, min.y, max.y);
            a.z = Mathf.Clamp(a.z, min.z, max.z);
            return a;
        }

        public static float Clamp360(float a, float min, float max)
        {
            a = Wrap360(a);
            if (a < min) {
                float d = min - a;
                if (d > 180f) {
                    a += 360f;
                }
                else {
                    return min;
                }
            }
            if (a > max) {
                float d = a - max;
                if (d > 180f) {
                    a -= 360f;
                    float d2 = min - a;
                    if (d2 < d) {
                        return min;
                    }
                    else return max;
                }
                else {
                    return max;
                }
            }

            return a;
        }

        public static Vector3 Clamp360(Vector3 a, Vector3 min, Vector3 max)
        {
            a.x = MathUtil.Clamp360(a.x, min.x, max.x);
            a.y = MathUtil.Clamp360(a.y, min.y, max.y);
            a.z = MathUtil.Clamp360(a.z, min.z, max.z);
            return a;
        }

        public static Vector4 Clamp(Vector4 a, Vector4 min, Vector4 max)
        {
            a.x = Mathf.Clamp(a.x, min.x, max.x);
            a.y = Mathf.Clamp(a.y, min.y, max.y);
            a.z = Mathf.Clamp(a.z, min.z, max.z);
            a.w = Mathf.Clamp(a.w, min.w, max.w);
            return a;
        }

        public static Quaternion Clamp(Quaternion q, float min, float max)
        {
            return Clamp(q, new Vector3(min, min, min), new Vector3(max, max, max));
        }

        public static Quaternion Clamp(Quaternion q, Vector3 min, Vector3 max)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1f;

            float x = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            x = Mathf.Clamp(x, min.x, max.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * x);

            float y = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            y = Mathf.Clamp(y, min.y, max.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * y);

            float z = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            z = Mathf.Clamp(z, min.z, max.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * z);

            return q;
        }

        public static Quaternion ClampPitch(Quaternion q, Quaternion t, float variance)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1f;

            float tx = 2.0f * Mathf.Rad2Deg * Mathf.Atan(t.x);
            float x = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            x = Mathf.Clamp(x, tx - variance, tx + variance);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * x);

            float n = q.x + q.y + q.z + q.w;
            q.x /= n;
            q.y /= n;
            q.z /= n;
            q.w /= n;

            return q;
        }

        public static float Snap(float value, float snap)
        {
            if (snap != 0f) {
                value = Mathf.Round(value / snap) * snap;
            }
            return value;
        }

        public static Vector2 Snap(Vector2 a, float snap)
        {
            if (snap != 0.0f) {
                a.x = Mathf.Round(a.x / snap) * snap;
                a.y = Mathf.Round(a.y / snap) * snap;
            }
            return a;
        }

        public static Vector3 Snap(Vector3 a, float snap)
        {
            if (snap != 0.0f) {
                a.x = Mathf.Round(a.x / snap) * snap;
                a.y = Mathf.Round(a.y / snap) * snap;
                a.z = Mathf.Round(a.z / snap) * snap;
            }
            return a;
        }

        public static Vector4 Snap(Vector4 a, float snap)
        {
            if (snap != 0.0f) {
                a.x = Mathf.Round(a.x / snap) * snap;
                a.y = Mathf.Round(a.y / snap) * snap;
                a.z = Mathf.Round(a.z / snap) * snap;
                a.w = Mathf.Round(a.w / snap) * snap;
            }
            return a;
        }

        public static Vector2 Normalize(Vector2 v)
        {
            float d = Mathf.Abs(v.x) + Mathf.Abs(v.y);
            if (d == 0) {
                return v;
            }
            v.x = v.x / d;
            v.y = v.y / d;
            return v;
        }


        #endregion

        #region MATH OPS

        public static Vector2 Add(Vector2 a, Vector2 b)
        {
            a.x += b.x;
            a.y += b.y;
            return a;
        }

        public static Vector3 Add(Vector3 a, Vector3 b)
        {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            return a;
        }

        public static Vector4 Add(Vector4 a, Vector4 b)
        {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            a.w += b.w;
            return a;
        }

        public static Rect Add(Rect a, Rect b)
        {
            a.xMin += b.xMin;
            a.yMin += b.yMin;
            a.xMax += b.xMax;
            a.yMax += b.yMax;
            return a;
        }

        public static RectOffset Add(RectOffset a, RectOffset b)
        {
            a.left += b.left;
            a.right += b.right;
            a.top += b.top;
            a.bottom += b.bottom;
            return a;
        }

        public static Vector3 Add(Vector3 a, float b)
        {
            a.x += b;
            a.y += b;
            a.z += b;
            return a;
        }

        public static Vector4 Add(Vector4 a, float b)
        {
            a.x += b;
            a.y += b;
            a.z += b;
            a.w += b;
            return a;
        }

        public static Color Add(Color a, Color b)
        {
            a.r += b.r;
            a.g += b.g;
            a.b += b.b;
            a.a += b.a;
            return a;
        }

        public static Vector2 Subtract(Vector2 a, Vector2 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            return a;
        }

        public static Vector3 Subtract(Vector3 a, Vector3 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;
            return a;
        }

        public static Vector4 Subtract(Vector4 a, Vector4 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;
            a.w -= b.w;
            return a;
        }

        public static Rect Subtract(Rect a, Rect b)
        {
            a.xMin -= b.xMin;
            a.yMin -= b.yMin;
            a.xMax -= b.xMax;
            a.yMax -= b.yMax;
            return a;
        }

        public static RectOffset Subtract(RectOffset a, RectOffset b)
        {
            a.left -= b.left;
            a.right -= b.right;
            a.top -= b.top;
            a.bottom -= b.bottom;
            return a;
        }

        public static Color Subtract(Color a, Color b)
        {
            a.r -= b.r;
            a.g -= b.g;
            a.b -= b.b;
            a.a -= b.a;
            return a;
        }

        public static Vector3 Difference(Vector3 a, Vector3 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;
            a.x = Mathf.Abs(a.x);
            a.y = Mathf.Abs(a.y);
            a.z = Mathf.Abs(a.z);
            return a;
        }

        public static Vector2 Multiply(Vector2 a, Vector2 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            return a;
        }

        public static Vector3 Multiply(Vector3 a, Vector3 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;
            return a;
        }

        public static Vector4 Multiply(Vector4 a, Vector4 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;
            a.w *= b.w;
            return a;
        }

        public static Rect Multiply(Rect a, Rect b)
        {
            a.xMin *= b.xMin;
            a.yMin *= b.yMin;
            a.xMax *= b.xMax;
            a.yMax *= b.yMax;
            return a;
        }

        public static RectOffset Multiply(RectOffset a, RectOffset b)
        {
            a.left *= b.left;
            a.right *= b.right;
            a.top *= b.top;
            a.bottom *= b.bottom;
            return a;
        }

        public static Vector2 Multiply(Vector2 a, float b)
        {
            a.x *= b;
            a.y *= b;
            return a;
        }

        public static Vector3 Multiply(Vector3 a, float b)
        {
            a.x *= b;
            a.y *= b;
            a.z *= b;
            return a;
        }

        public static Vector4 Multiply(Vector4 a, float b)
        {
            a.x *= b;
            a.y *= b;
            a.z *= b;
            a.w *= b;
            return a;
        }

        public static Color Multiply(Color a, Color b)
        {
            a.r *= b.r;
            a.g *= b.g;
            a.b *= b.b;
            a.a *= b.a;
            return a;
        }

        public static Color Multiply(Color a, float b)
        {
            a.r *= b;
            a.g *= b;
            a.b *= b;
            //a.a *= b.a;
            return a;
        }

        public static Vector2 Divide(Vector2 a, Vector2 b)
        {
            if (b.x != 0.0f) a.x /= b.x;
            else a.x = 0.0f;

            if (b.y != 0.0f) a.y /= b.y;
            else a.y = 0.0f;

            return a;
        }

        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            if (b.x != 0.0f) a.x /= b.x;
            else a.x = 0.0f;

            if (b.y != 0.0f) a.y /= b.y;
            else a.y = 0.0f;

            if (b.z != 0.0f) a.z /= b.z;
            else a.z = 0.0f;

            return a;
        }

        public static Vector3 Divide(Vector3 a, float b)
        {
            if (b != 0.0f) {
                a.x /= b;
                a.y /= b;
                a.z /= b;
            }
            else {
                a = Vector3.zero;
            }
            return a;
        }

        public static Vector4 Divide(Vector4 a, Vector4 b)
        {
            if (b.x != 0.0f) a.x /= b.x;
            if (b.y != 0.0f) a.y /= b.y;
            if (b.z != 0.0f) a.z /= b.z;
            if (b.w != 0.0f) a.w /= b.w;
            return a;
        }

        public static float Sin(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Sin(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Cos(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Cos(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Tan(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Tan(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Asin(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Asin(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Acos(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Acos(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Atan(float v, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Atan(v);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Atan2(float v, float y, bool degrees)
        {
            if (degrees) v *= Mathf.Deg2Rad;
            v = Mathf.Atan2(v, y);
            if (degrees) v *= Mathf.Rad2Deg;
            return v;
        }

        public static float Fibonacci(float value)
        {
            return value * 1.61803398875f;
        }

        #endregion

        #region CALCULATE

        public static float Average(float a, float b)
        {
            return (a + b) * 0.5f;
        }

        public static Vector2 Average(Vector2 a, Vector2 b)
        {
            Vector2 c;
            c.x = (a.x + b.x) * 0.5f;
            c.y = (a.y + b.y) * 0.5f;
            return c;
        }

        public static Vector3 Average(Vector3 a, Vector3 b)
        {
            Vector3 c;
            c.x = (a.x + b.x) * 0.5f;
            c.y = (a.y + b.y) * 0.5f;
            c.z = (a.z + b.z) * 0.5f;
            return c;
        }

        public static Vector4 Average(Vector4 a, Vector4 b)
        {
            Vector4 c;
            c.x = (a.x + b.x) * 0.5f;
            c.y = (a.y + b.y) * 0.5f;
            c.z = (a.z + b.z) * 0.5f;
            c.w = (a.w + b.w) * 0.5f;
            return c;
        }

        public static Color Average(Color a, Color b)
        {
            Color c;
            c.r = (a.r + b.r) * 0.5f;
            c.g = (a.g + b.g) * 0.5f;
            c.b = (a.b + b.b) * 0.5f;
            c.a = (a.a + b.a) * 0.5f;
            return c;
        }

        /// Calculate the difference between 2 euler angles returning the shortest value Assumes values do
        /// not exceed -360 to 360
        public static Vector3 RotationDifference(Vector3 from, Vector3 to)
        {
            Vector3 v = Vector3.zero;
            if (from != to) {
                v = to - from;
                if (v.x > 180f) v.x -= 360f;
                else
                if (v.x < -180f) v.x += 360f;

                if (v.y > 180f) v.y -= 360f;
                else
                if (v.y < -180f) v.y += 360f;

                if (v.z > 180f) v.z -= 360f;
                else
                if (v.z < -180f) v.z += 360f;
            }

            return v;
        }

        /// <summary>
        /// This calculates a target rotation so that values can be interpolated without flipping. This
        /// determines the shortest rotation between from and to, returning the new value as a relative
        /// offset to from, which can then be used to interpolate smoothly. Note that since this method
        /// always shortens rotations to the smallest degree, full or multiple rotations cannot occur. Any
        /// additional spins therefore are collapsed to a range between -360 and 360.
        /// </summary>
        /// <param name="from">The original or base rotation</param>
        /// <param name="to">The desired target rotation.</param>
        /// <returns>Modified 'from' rotation with relative offset of 'to'</returns>
        public static Vector3 RotationTarget(Vector3 from, Vector3 to)
        {
            Vector3 f = Wrap360(from);
            Vector3 t = Wrap360(to);
            Vector3 d = RotationDifference(from, to);
            return from + d;
        }


        /// <summary>
        /// This compares 2 quaternion rotations to determine whether they approximate the same 360 degree
        /// rotation, reducing spins to 360 degrees and accounting for any inversions.
        /// </summary>
        /// <returns>True if the rotations are more or less equivalent.</returns>
        public static bool IsRotationSimilar(Quaternion a, Quaternion b)
        {
            Vector3 av = Wrap360(a.eulerAngles);
            Vector3 bv = Wrap360(b.eulerAngles);

            return Mathf.Approximately(av.x, bv.x) && Mathf.Approximately(av.y, bv.y) && Mathf.Approximately(av.z, bv.z);
        }

        public static float Distance(float pos1, float pos2)
        {
            return Mathf.Abs(pos1 - pos2);
        }

        public static float Distance(Vector2 pos1, Vector2 pos2)
        {
            float x = pos1.x - pos2.x;
            float y = pos1.y - pos2.y;
            float dist = Mathf.Sqrt((x * x) + (y * y));
            return dist;
        }

        public static float Distance(Vector3 pos1, Vector3 pos2)
        {
            float x = pos1.x - pos2.x;
            float y = pos1.y - pos2.y;
            float z = pos1.z - pos2.z;
            float dist = Mathf.Sqrt((x * x) + (y * y) + (z * z));
            return dist;
        }

        public static float Distance(Vector4 pos1, Vector4 pos2)
        {
            float x = pos1.x - pos2.x;
            float y = pos1.y - pos2.y;
            float z = pos1.z - pos2.z;
            float w = pos1.w - pos2.w;
            float dist = Mathf.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
            return dist;
        }

        public static float Slope(Vector2 a, Vector2 b)
        {
            float slope;
            if (a.x != b.x) {
                slope = (a.y - b.y) / (a.x - b.x);
            }
            else {
                slope = 10000.0f;
            }
            return slope;
        }

        public static void Wobble(ref float value, float max, float rate)
        {
            if (rate > 0f) {
                float loop = MathUtil.Loop(Time.time, 0f, rate);
                value = MathUtil.Interpolate(-max, max, loop);
            }
        }

        public static void Wobble(ref Vector3 value, Vector3 max, Vector3 rate)
        {
            Wobble(ref value.x, max.x, rate.x);
            Wobble(ref value.y, max.y, rate.y);
            Wobble(ref value.z, max.z, rate.z);
        }

        public static Vector2 VectorFromAngle(float radians)
        {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public static float Angle(Vector2 v)
        {
            return Vector2.Angle(Vector2.zero, v);
        }

        public static float Angle(Vector2 a, Vector2 b)
        {
            float angle = Mathf.PI * 0.5f;
            if (a.x != b.x) {
                angle = Mathf.Atan((a.y - b.y) / (a.x - b.x));
            }
            else
            if (b.y < a.y) {
                angle = -angle;
            }
            if (b.x < a.x) {
                angle -= Mathf.PI;
            }
            return angle;
        }

        public static float Angle(Vector2 a, Vector2 b, Vector2 c)
        {
            float lenghtA = Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
            float lenghtB = Mathf.Sqrt(Mathf.Pow(c.x - b.x, 2) + Mathf.Pow(c.y - b.y, 2));
            float lenghtC = Mathf.Sqrt(Mathf.Pow(c.x - a.x, 2) + Mathf.Pow(c.y - a.y, 2));

            float d = lenghtA * lenghtB;
            if (d == 0) {
                return 0;
            }
            else {
                float calc = ((lenghtA * lenghtA) + (lenghtB * lenghtB) - (lenghtC * lenghtC)) / (d * 2f);
                if (calc < -1f) calc = -1f;
                else
                if (calc > 1f) calc = 1f;
                return Mathf.Acos(calc) * Mathf.Rad2Deg;
            }
        }

        #endregion

        #region SCREEN & WORLD

        public static float HorizontalFOV(float fov, float aspect)
        {
            float a = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
            if (a == 0f) a = 1f;
            float d = 1f / a;
            float h = Mathf.Atan(aspect / d) * 2f * Mathf.Rad2Deg;
            return h;
        }

        public static Vector3 ScreenToWorld(Camera cam, Vector3 coord) { return ScreenToLocal(cam, coord, null); }

        public static Vector3 ScreenToLocal(Camera cam, Vector3 coord, GameObject obj)
        {
            if (cam != null) {
                if (Screen.height == 0) return Vector3.zero;
                Vector3 scale = Vector3.one;
                if (cam.orthographic) {
                    scale.x = (cam.orthographicSize * 2.0f) / Screen.height;
                    scale.y = scale.z = scale.x;
                }
                else {
                    float d = coord.z - cam.transform.position.z;
                    float size = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * d * 2.0f;
                    Vector3 parentLossyScale = new Vector3(1.0f, 1.0f, 1.0f);
                    if (obj != null && obj.transform.parent != null) {
                        parentLossyScale = obj.transform.parent.lossyScale;
                    }

                    if (parentLossyScale.x != 0) {
                        scale.x = (size / Screen.height) * (1.0f / parentLossyScale.x);
                    }
                    if (parentLossyScale.y != 0) {
                        scale.y = (size / Screen.height) * (1.0f / parentLossyScale.y);
                    }
                    if (parentLossyScale.z != 0) {
                        scale.z = (size / Screen.height) * (1.0f / parentLossyScale.z);
                    }
                }
                coord = MathUtil.Multiply((Vector3)coord, scale);
            }
            return coord;
        }

        public static Vector3 WorldToScreen(Camera cam, GameObject obj, Vector3 coord, bool toLocal) { return LocalToScreen(cam, coord, null); }

        public static Vector3 LocalToScreen(Camera cam, Vector3 coord, GameObject obj)
        {
            if (cam != null && obj != null) {
                Vector3 scale;
                if (cam.orthographic) {
                    scale.x = scale.y = scale.z = Screen.height / (cam.orthographicSize * 2.0f);
                }
                else {
                    float d = coord.z - cam.transform.position.z;
                    float size = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * d * 2.0f;
                    Vector3 parentLossyScale = new Vector3(1.0f, 1.0f, 1.0f);
                    if (obj != null && obj.transform.parent != null) {
                        parentLossyScale = obj.transform.parent.lossyScale;
                    }

                    if (parentLossyScale.x == 0f) parentLossyScale.x = 1f;
                    if (parentLossyScale.y == 0f) parentLossyScale.y = 1f;
                    if (parentLossyScale.z == 0f) parentLossyScale.z = 1f;
                    if (size == 0f) size = 1f;

                    scale.x = Screen.height / (size / parentLossyScale.x);
                    scale.y = Screen.height / (size / parentLossyScale.y);
                    scale.z = Screen.height / (size / parentLossyScale.z);
                }
                coord = MathUtil.Multiply((Vector3)coord, scale);
            }
            return coord;
        }

        #endregion

        #region GEOMETRIC

        public static Vector2 GetPointByDistanceAndAngle(float dist, float angle)
        {
            Vector2 v = Vector2.zero;
            v.x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
            v.y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
            return v;
        }

        /// <summary>
        /// Returns a new point that has the same offset from b as b from a
        /// </summary>
        public static Vector2 ExtendPoints(Vector2 a, Vector2 b)
        {
            return new Vector2(b.x + (b.x - a.x), b.y + (b.y - a.y));
        }

        /// <summary>
        /// Returns a new point that has the same offset from b as b from a
        /// </summary>
        public static Vector3 ExtendPoints(Vector3 a, Vector3 b)
        {
            return new Vector3(b.x + (b.x - a.x), b.y + (b.y - a.y), b.z + (b.z - a.z));
        }

        public static float IsInsideTriangleSide(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        /// <summary>
        /// Returns true if the point p is within the triangle a-b-c
        /// </summary>
        /// <param name="a">Triangle point 1</param>
        /// <param name="b">Triangle point 2</param>
        /// <param name="c">Triangle point 3</param>
        /// <param name="d">Point to check</param>
        public static bool IsInsideTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = IsInsideTriangleSide(pt, v1, v2);
            d2 = IsInsideTriangleSide(pt, v2, v3);
            d3 = IsInsideTriangleSide(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public static bool IsPointInQuad(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Vector2 point)
        {
            Vector2[] poly = new Vector2[4];
            poly[0] = v1;
            poly[1] = v2;
            poly[2] = v3;
            poly[3] = v4;

            return IsPointInPolygon(point, poly);
        }

        public static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            int polygonLength = polygon.Length, i = 0;
            bool inside = false;
            float pointX = point.x, pointY = point.y;

            // start / end point for the current polygon segment.
            float startX, startY, endX, endY;
            Vector2 endPoint = polygon[polygonLength - 1];
            endX = endPoint.x;
            endY = endPoint.y;
            while (i < polygonLength) {
                startX = endX; startY = endY;
                endPoint = polygon[i++];
                endX = endPoint.x; endY = endPoint.y;
                float rY = startY - endY;
                if (rY != 0) {
                    inside ^= (endY > pointY ^ startY > pointY) &&
                              ((pointX - endX) < (pointY - endY) * (startX - endX) / rY);
                }
            }
            return inside;
        }

        public static float AreaOfTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((a.x * (b.y - c.y)) + (b.x * (c.y - a.y)) + (c.x * (a.y - b.y))) / 2f;
        }

        public static Vector2 PerpendicularPoint(Vector2 a, Vector2 b, float distance, bool center = false, float shift = 0f)
        {
            Vector2 point;

            if (center) {
                // Calculate the center of the line
                a = Interpolate(a, b, 0.5f);
            }
            float angle = Angle(a, b);

            float px = Mathf.Sin(angle) * distance;
            float py = Mathf.Cos(angle) * distance;

            point = new Vector2(a.x - px, a.y + py);

            if (shift != 0.0) {
                point.x += (b.x - a.x) * shift;
                point.y += (b.y - a.y) * shift;
            }

            return point;
        }

        /// <summary>
        /// Returns true if the point p is between the corner points a and b. This acts more like a rect
        /// bounds check rather than a coincident line check
        /// </summary>
        /// <param name="a">The start point of a line</param>
        /// <param name="b">The end point of a line</param>
        /// <param name="p">The point to test</param>
        public static bool IsPointBetween(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 min = a;
            Vector2 max = b;
            if (min.x > b.x) {
                min.x = b.x;
                max.x = a.x;
            }
            if (min.y > b.y) {
                min.y = b.y;
                max.y = a.y;
            }

            return p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y;
        }

        /// <summary>
        /// Returns true if p has crossed over the perpendicular line of a_b
        /// </summary>
        /// <param name="a">Start point of line</param>
        /// <param name="b">End point of line</param>
        /// <param name="p">Point to check</param>
        public static bool CrossedLine(Vector2 a, Vector2 b, Vector2 p)
        {
            return ((p.x - a.x) * (b.y - a.y) - (p.y - a.y) * (b.x - a.x)) > 0f;
        }

        /// <summary>
        /// Returns true if p has crossed over the perpendicular line of a_b
        /// </summary>
        /// <param name="a">Start point of line</param>
        /// <param name="b">End point of line</param>
        /// <param name="p">Point to check</param>
        public static bool CrossedPerpendicular(Vector2 a, Vector2 b, Vector2 p)
        {
            return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) > 0f;
        }

        /// <summary>
        /// Calculates the intersection of perpendicular lines from point a and c
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public static Vector2 GetFocalPoint(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 a2 = MathUtil.PerpendicularPoint(a, b, 10f, false, 0f);
            Vector2 c2 = MathUtil.PerpendicularPoint(c, b, 10f, false, 0f);
            return MathUtil.Intersect(a, a2, c, c2);
        }

        /// <summary>
        /// Scales the point from the focus to match the distance of the target
        /// </summary>
        /// <param name="point">The point to reposition</param>
        /// <param name="focus">The anchor point to scale from</param>
        /// <param name="target">The goal position to match the distance of</param>
        public static Vector2 ScaleFromFocalPoint(Vector2 focus, Vector2 point, Vector2 target, float scale = 1f)
        {
            float fd = MathUtil.Distance(focus, point);
            if (fd == 0f) {
                return point;
            }
            else {
                float ft = MathUtil.Distance(focus, target);
                if (ft == 0f) {
                    return point;
                }
                else {
                    return MathUtil.Multiply(point - focus, (ft / fd) * scale) + focus;
                }
            }
        }

        /// <summary>
        /// Creates a new point along the line from focus to point with the given length
        /// </summary>
        /// <param name="focus">Origin point of the line</param>
        /// <param name="point">End point of the line</param>
        /// <param name="length">Length along the line of the new point to create</param>
        public static Vector2 ScaleFromFocalPoint(Vector2 focus, Vector2 point, float length)
        {
            float fd = MathUtil.Distance(focus, point);
            if (fd == 0f) {
                return point;
            }
            else {
                if (length == 0f) {
                    return point;
                }
                else {
                    return MathUtil.Multiply(point - focus, length / fd) + focus;
                }
            }
        }

        public static bool IsCollinear(Vector2 a, Vector2 b, Vector2 c)
        {
            float x = a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
            return x == 0;
        }

        public static bool IsCollinear(Vector3 a, Vector3 b, Vector3 c)
        {
            float x = a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
            if (x == 0) {
                float y = a.y * (b.z - c.z) + b.y * (c.z - a.z) + c.y * (a.z - b.z);
                if (y == 0) {
                    float z = a.z * (b.x - c.x) + b.z * (c.x - a.x) + c.z * (a.x - b.x);
                    if (z == 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Vector2 NearestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 ap = MathUtil.Subtract(point, a);
            Vector2 ab = MathUtil.Subtract(b, a);

            float ab2 = (ab.x * ab.x) + (ab.y * ab.y);
            float dot = (ap.x * ab.x) + (ap.y * ab.y);
            float t = 1f;
            if (ab2 != 0f) t = dot / ab2;

            return new Vector2(a.x + ab.x * t, a.y + ab.y * t);
        }

        public static Vector2 NearestPointOnLine(Vector2 a, Vector2 b, Vector2 point, bool limited, out float completion)
        {
            Vector2 ap = MathUtil.Subtract(point, a);
            Vector2 ab = MathUtil.Subtract(b, a);

            float ab2 = (ab.x * ab.x) + (ab.y * ab.y);
            float dot = (ap.x * ab.x) + (ap.y * ab.y);
            if (ab2 == 0f) {
                completion = 1f;
            }
            else {
                completion = dot / ab2;
            }
            if (limited) {
                completion = Mathf.Min(1f, Mathf.Max(0f, completion));
            }

            return new Vector2(a.x + ab.x * completion, a.y + ab.y * completion);
        }

        public static Vector3 NearestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = MathUtil.Subtract(point, a);
            Vector3 ab = MathUtil.Subtract(b, a);

            float ab2 = (ab.x * ab.x) + (ab.y * ab.y) + (ab.z * ab.z);
            float dot = (ap.x * ab.x) + (ap.y * ab.y) + (ap.z * ab.z);
            float t = ab2 == 0f ? 1f : dot / ab2;

            return new Vector3(a.x + ab.x * t, a.y + ab.y * t, a.z + ab.z * t);
        }

        public static Vector3 NearestPointOnLine(Vector3 a, Vector3 b, Vector3 point, bool limited, out float completion)
        {
            Vector3 ap = MathUtil.Subtract(point, a);
            Vector3 ab = MathUtil.Subtract(b, a);

            float ab2 = (ab.x * ab.x) + (ab.y * ab.y) + (ab.z * ab.z);
            float dot = (ap.x * ab.x) + (ap.y * ab.y) + (ap.z * ab.z);
            if (ab2 == 0f) {
                completion = 1f;
            }
            else {
                completion = dot / ab2;
            }
            if (limited) {
                completion = Mathf.Min(1f, Mathf.Max(0f, completion));
            }

            return new Vector3(a.x + ab.x * completion, a.y + ab.y * completion, a.z + ab.z * completion);
        }

        public static Vector2 Intersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 c = Interpolate(a2, b1, 0.5f);

            float dn = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
            float na = (b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x);
            float nb = (a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x);

            if (Mathf.Abs(na) < Mathf.Epsilon && Mathf.Abs(nb) < Mathf.Epsilon && Mathf.Abs(dn) < Mathf.Epsilon) {
                // The lines are coincident
                c.x = (a1.x + a2.x) / 2f;
                c.y = (a1.y + a2.y) / 2f;
            }
            else
            if (Mathf.Abs(dn) < Mathf.Epsilon || dn == 0) {
                // The lines are parallel
                c.x = 0;
                c.y = 0;
            }
            else {
                float mua = na / dn;
                c.x = a1.x + mua * (a2.x - a1.x);
                c.y = a1.y + mua * (a2.y - a1.y);
            }
            return c;
        }

        public static Vector2[] ArcSteps(Vector2 a, Vector2 b, int steps)
        {
            if (steps == 0) return null;

            float height = Mathf.Abs(b.y - a.y);
            float width = Mathf.Abs(b.x - a.x);
            Vector2 corner = a;
            Vector2 radius = new Vector2(width, height);

            b = Subtract(b, a);
            a = Vector2.zero;

            float angle = Mathf.Deg2Rad * 0.0f;
            float range = Mathf.Deg2Rad * 90.0f;

            float step = (range - angle) / (1.0f * steps);

            Vector2[] arcPoints = new Vector2[steps];

            for (int i = 0; i < steps; i++) {
                if (b.x > a.x && b.y < a.y) {
                    arcPoints[i].x = (radius.x * Mathf.Sin(angle));
                    arcPoints[i].y = (radius.y * Mathf.Cos(angle)) - height;
                }
                else
                if (b.x < a.x && b.y < a.y) {
                    arcPoints[i].x = (radius.x * Mathf.Cos(angle)) - width;
                    arcPoints[i].y = (height - (radius.y * Mathf.Sin(angle))) - height;
                }
                else
                if (b.x < a.x && b.y > a.y) {
                    arcPoints[i].x = (width - (radius.x * Mathf.Sin(angle))) - width;
                    arcPoints[i].y = height - (radius.y * Mathf.Cos(angle));
                }
                else {
                    arcPoints[i].x = width - (radius.x * Mathf.Cos(angle));
                    arcPoints[i].y = (radius.y * Mathf.Sin(angle));
                }
                arcPoints[i].x += corner.x;
                arcPoints[i].y += corner.y;

                angle += step;
            }

            return arcPoints;
        }

        public static Vector2[] Semicircle(Vector2 center, Vector2 a, Vector2 b, int steps, int mode)
        {
            float radius = MathUtil.Distance(center, a);
            Vector2[] points = new Vector2[steps + 1];

            float a1 = Angle(center, a);
            float a2 = Angle(center, b);

            float circ = a2 - a1;
            float step = 1.0f;
            float pi2 = Mathf.PI * 2.0f;

            while (a1 > pi2) a1 -= pi2;
            while (a1 < -pi2) a1 += pi2;
            while (a2 > pi2) a2 -= pi2;
            while (a2 < -pi2) a2 += pi2;

            if (mode == 0) {
                // Create the circle arc from point a to point b in one direction.
                if (circ < 0.0f) {
                    circ = pi2 + circ;
                }
                circ = pi2 - circ;
                step = -1.0f;
            }
            else
            if (mode == 1) {
                // Create the circle arc from point a to point b in one direction.
                if (circ < 0.0f) {
                    circ = pi2 + circ;
                }
            }
            else
            if (mode == 2) {
                // Use the shortest arc
                if (circ > Mathf.PI) {
                    circ = Mathf.PI - (circ - Mathf.PI);
                    step = -1.0f;
                }
            }

            for (int i = 0; i <= steps; i++) {
                float r = a1 + (((step * i) / (1.0f * steps)) * circ);
                float x = Mathf.Cos(r) * radius;
                float y = Mathf.Sin(r) * radius;
                points[i].x = center.x + x;
                points[i].y = center.y + y;
            }
            return points;
        }

        #endregion

        #region REMAP

        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            if (inMin == inMax || outMin == outMax) return outMin;
            float r = (inMax - inMin);
            if (r == 0f) r = 1f;
            float i = (value - inMin) / r;
            return Interpolate(outMin, outMax, i * blend);
        }

        public static Vector2 Remap(Vector2 value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.x = Remap(value.x, inMin, inMax, outMin, outMax, blend);
            value.y = Remap(value.y, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        public static Vector3 Remap(Vector3 value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.x = Remap(value.x, inMin, inMax, outMin, outMax, blend);
            value.y = Remap(value.y, inMin, inMax, outMin, outMax, blend);
            value.z = Remap(value.z, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        public static Vector4 Remap(Vector4 value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.x = Remap(value.x, inMin, inMax, outMin, outMax, blend);
            value.y = Remap(value.y, inMin, inMax, outMin, outMax, blend);
            value.z = Remap(value.z, inMin, inMax, outMin, outMax, blend);
            value.w = Remap(value.w, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        public static Color Remap(Color value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.r = Remap(value.r, inMin, inMax, outMin, outMax, blend);
            value.g = Remap(value.g, inMin, inMax, outMin, outMax, blend);
            value.b = Remap(value.b, inMin, inMax, outMin, outMax, blend);
            value.a = Remap(value.a, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        public static Rect Remap(Rect value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.xMin = Remap(value.xMin, inMin, inMax, outMin, outMax, blend);
            value.xMax = Remap(value.xMax, inMin, inMax, outMin, outMax, blend);
            value.yMin = Remap(value.yMin, inMin, inMax, outMin, outMax, blend);
            value.yMax = Remap(value.yMax, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        public static RectOffset Remap(RectOffset value, float inMin, float inMax, float outMin, float outMax, float blend)
        {
            value.left = (int)Remap((float)value.left, inMin, inMax, outMin, outMax, blend);
            value.right = (int)Remap((float)value.right, inMin, inMax, outMin, outMax, blend);
            value.top = (int)Remap((float)value.top, inMin, inMax, outMin, outMax, blend);
            value.bottom = (int)Remap((float)value.bottom, inMin, inMax, outMin, outMax, blend);
            return value;
        }

        #endregion

        #region INTERPOLATION

        public enum LegacyInterpolationModes
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCircle,
            EaseOutCircle,
            EaseInOutCircle,
            AnimationCurve,
            UseChannelCurve
        }


        public enum InterpolationModes
        {
            None,
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCircle,
            EaseOutCircle,
            EaseInOutCircle,
            AnimationCurve,
            UseChannelCurve,
            Switch
        }

        /// <summary>
        /// Interpolation for audio curve. Given an input time and ADSR params, it returns the value (0-1)
        /// at the specified time
        /// </summary>
        /// <param name="t">time to sample at (locally always starts at 0, so time = currentTime -
        ///     note.StartTime)</param>
        /// <param name="a">attack in seconds</param>
        /// <param name="d">decay in seconds (ramps down to sustain value)</param>
        /// <param name="s">the sustain level (0-1) to hold once decay is time is reached</param>
        /// <param name="r">release time in seconds, fading back to 0</param>
        /// <param name="l">length of note - set to 0 for automatic length, or set to -1 for unknown length
        ///     to hold the note on indefinitely</param>
        /// <returns>It is recommended to apply a threshold for sustain if notes should not exceed loudness
        ///     of midi note velocity. value = Mathf.Min(value, Sustain);</returns>

        public static float ADSR(float t, float a, float d, float s, float r, float l)
        {
            return ADSR(t, a, d, s, r, l, InterpolationModes.Linear, InterpolationModes.Linear, InterpolationModes.Linear);
        }

        public static float ADSR(float t, float a, float d, float s, float r, float l,
            InterpolationModes attackEase, InterpolationModes decayEase, InterpolationModes releaseEase)
        {
            if (l == 0) l = a + d;
            else
            if (l > 0) l = Mathf.Max(l, a); // Extend length of note to account for attack
            float v = 0;

            if (t <= a) {
                if (a == 0f) {
                    v = 1f;
                }
                else {
                    v = t / a;
                }
                v = MathUtil.InterpolateMode(0, 1f, v, attackEase);
                if (d == 0) v *= s; // Sustain is the max level if there is no decay
            }
            else
            if (t > a && t <= (a + d)) {
                if (d == 0f) {
                    v = s;
                }
                else {
                    v = (t - a) / d;
                }
                v = MathUtil.InterpolateMode(1f, s, v, decayEase);
            }
            else
            if (l < 0) {
                // length is unknown so hold the note indefinitely
                v = s;
            }
            else
            if (t <= l) {
                v = s;
            }
            else
            if (t <= (l + r)) {
                if (r == 0f) {
                    v = 1f;
                }
                else {
                    v = (t - l) / r;
                }
                v = MathUtil.InterpolateMode(s, 0, v, releaseEase);
            }
            return v;
        }

        public static Vector3 SmoothApproach(Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float speed)
        {
            if (speed <= 0f) return targetPosition;

            float t = Time.smoothDeltaTime * speed;
            if (t <= 0f) {
                return targetPosition;
            }
            Vector3 v = (targetPosition - pastTargetPosition) / t;
            Vector3 f = pastPosition - pastTargetPosition + v;
            return targetPosition - v + f * Mathf.Exp(-t);
        }

        public static float GetInterpolation(float min, float max, float value)
        {
            if (value <= min) {
                return 0;
            }
            else
            if (value >= max) {
                return 1f;
            }
            float r = (max - min);
            if (r == 0f) r = 1f;
            return (value - min) / r;
        }

        public static Vector4 GetInterpolation(Vector3 min, Vector3 max, Vector3 value)
        {
            value.x = GetInterpolation(min.x, max.x, value.x);
            value.y = GetInterpolation(min.y, max.y, value.y);
            value.z = GetInterpolation(min.z, max.z, value.z);
            return value;
        }

        public static Vector4 GetInterpolation(Vector4 min, Vector4 max, Vector4 value)
        {
            value.x = GetInterpolation(min.x, max.x, value.x);
            value.y = GetInterpolation(min.y, max.y, value.y);
            value.z = GetInterpolation(min.z, max.z, value.z);
            value.w = GetInterpolation(min.w, max.w, value.w);
            return value;
        }

        public static float InterpolateMode(float start, float end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }

        public static float InterpolateMode(float start, float end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }

        /// <summary>
        /// Interpolate between values using the specified mode/curve. Use the polarize option to invert
        /// outgoing modes.
        /// </summary>
        public static float InterpolateMode(float start, float end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            float value = start;
            if (mode == InterpolationModes.None) {
                // hold the value - no interpolation
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                // amount acts as on/off switch - assumes amount is value 0-1
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                float curveAmount = curve.Evaluate(invert ? 1f - amount : amount);
                if (curve != null) value = Interpolate(start, end, curveAmount, clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static Vector2 InterpolateMode(Vector2 start, Vector2 end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static Vector2 InterpolateMode(Vector2 start, Vector2 end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static Vector2 InterpolateMode(Vector2 start, Vector2 end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            Vector2 value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static Vector3 InterpolateMode(Vector3 start, Vector3 end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static Vector3 InterpolateMode(Vector3 start, Vector3 end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static Vector3 InterpolateMode(Vector3 start, Vector3 end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            Vector3 value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static Vector4 InterpolateMode(Vector4 start, Vector4 end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static Vector4 InterpolateMode(Vector4 start, Vector4 end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static Vector4 InterpolateMode(Vector4 start, Vector4 end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            Vector4 value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static Rect InterpolateMode(Rect start, Rect end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static Rect InterpolateMode(Rect start, Rect end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static Rect InterpolateMode(Rect start, Rect end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            Rect value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static RectOffset InterpolateMode(RectOffset start, RectOffset end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static RectOffset InterpolateMode(RectOffset start, RectOffset end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static RectOffset InterpolateMode(RectOffset start, RectOffset end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            RectOffset value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static Quaternion InterpolateMode(Quaternion start, Quaternion end, float amount, InterpolationModes mode)
        {
            return InterpolateMode(start, end, amount, mode, null, false, false);
        }
        public static Quaternion InterpolateMode(Quaternion start, Quaternion end, float amount, InterpolationModes mode, AnimationCurve curve)
        {
            return InterpolateMode(start, end, amount, mode, curve, false, false);
        }
        public static Quaternion InterpolateMode(Quaternion start, Quaternion end, float amount, InterpolationModes mode, AnimationCurve curve, bool polarize, bool invert, bool clamped = true)
        {
            Quaternion value = start;
            if (mode == InterpolationModes.None) {
                value = start;
            }
            else
            if (mode == InterpolationModes.Switch) {
                value = (invert ? 1f - amount : amount) < 0.5f ? start : end;
            }
            else
            if (mode == InterpolationModes.Linear) {
                value = Interpolate(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseIn) {
                value = EaseInQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOut) {
                if (polarize) amount = 1f - amount;
                value = EaseOutQuad(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOut) {
                if (polarize) {
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInQuad(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutQuad(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInExpo) {
                value = EaseInExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutExpo) {
                if (polarize) amount = 1f - amount;
                value = EaseOutExpo(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutExpo) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInExpo(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutExpo(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.EaseInCircle) {
                value = EaseInCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseOutCircle) {
                if (polarize) amount = 1f - amount;
                value = EaseOutCircle(start, end, (invert ? 1f - amount : amount), clamped);
            }
            else
            if (mode == InterpolationModes.EaseInOutCircle) {
                if (polarize) {
                    amount = 1f - amount;
                    if (amount > 0.5f) {
                        amount = 2f * (amount - 0.5f);
                        value = EaseOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                    }
                    else {
                        amount *= 2f;
                        value = EaseInCircle(end, start, invert ? 1f - amount : amount, clamped);
                    }
                }
                else {
                    value = EaseInOutCircle(start, end, invert ? 1f - amount : amount, clamped);
                }
            }
            else
            if (mode == InterpolationModes.AnimationCurve) {
                if (curve != null) value = Interpolate(start, end, curve.Evaluate((invert ? 1f - amount : amount)), clamped);
                else value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            else {
                value = Interpolate(start, end, invert ? 1f - amount : amount, clamped);
            }
            return value;
        }

        public static float Interpolate(float a, float b, float amount, bool clamped)
        {
            if (clamped) return Interpolate(a, b, amount);
            else return InterpolateUnclamped(a, b, amount);
        }

        public static float Interpolate(float a, float b, float amount)
        {
            return amount <= 0f ? a : amount >= 1f ? b : ((1f - amount) * a) + (b * amount);
        }

        public static float InterpolateUnclamped(float a, float b, float amount)
        {
            return ((1f - amount) * a) + (b * amount);
        }

        public static Color Interpolate(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            a.r = Interpolate(a.r, b.r, amount, clamped);
            a.g = Interpolate(a.g, b.g, amount, clamped);
            a.b = Interpolate(a.b, b.b, amount, clamped);
            a.a = Interpolate(a.a, b.a, amount, clamped);
            return a;
        }

        public static Vector2 Interpolate(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            Vector2 c;
            c.x = ((1.0f - amount) * a.x) + (b.x * amount);
            c.y = ((1.0f - amount) * a.y) + (b.y * amount);
            return c;
        }

        public static Vector3 Interpolate(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            Vector3 c;
            c.x = ((1.0f - amount) * a.x) + (b.x * amount);
            c.y = ((1.0f - amount) * a.y) + (b.y * amount);
            c.z = ((1.0f - amount) * a.z) + (b.z * amount);
            return c;
        }

        public static Vector4 Interpolate(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            Vector4 c;
            c.x = ((1.0f - amount) * a.x) + (b.x * amount);
            c.y = ((1.0f - amount) * a.y) + (b.y * amount);
            c.z = ((1.0f - amount) * a.z) + (b.z * amount);
            c.w = ((1.0f - amount) * a.w) + (b.w * amount);
            return c;
        }

        public static Rect Interpolate(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            Rect c = new Rect(
                ((1.0f - amount) * a.xMin) + (b.xMin * amount),
                ((1.0f - amount) * a.yMin) + (b.yMin * amount),
                ((1.0f - amount) * a.xMax) + (b.xMax * amount),
                ((1.0f - amount) * a.yMax) + (b.yMax * amount));
            return c;
        }

        public static RectOffset Interpolate(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            RectOffset c = new RectOffset(
                (int)(((1.0f - amount) * (float)a.left) + ((float)b.left * amount)),
                (int)(((1.0f - amount) * (float)a.right) + ((float)b.right * amount)),
                (int)(((1.0f - amount) * (float)a.top) + ((float)b.top * amount)),
                (int)(((1.0f - amount) * (float)a.bottom) + ((float)b.bottom * amount)));
            return c;
        }

        //public static Vector4 Interpolate(Vector4 a, Vector4 b, Vector4 amount)
        //{
        //    Vector4 c;
        //    c.x = amount.x <= 0 ? a.x : amount.x >= 1f ? b.x : ((1.0f - amount.x) * a.x) + (b.x * amount.x);
        //    c.y = amount.y <= 0 ? a.y : amount.y >= 1f ? b.y : ((1.0f - amount.y) * a.y) + (b.y * amount.y);
        //    c.z = amount.z <= 0 ? a.z : amount.z >= 1f ? b.z : ((1.0f - amount.z) * a.z) + (b.z * amount.z);
        //    c.w = amount.w <= 0 ? a.w : amount.w >= 1f ? b.w : ((1.0f - amount.w) * a.w) + (b.w * amount.w);
        //    return c;
        //}

        public static Quaternion Interpolate(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Lerp(a, b, amount);
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static float EaseInQuad(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b -= a;
            return b * amount * amount + a;
        }

        public static Vector2 EaseInQuad(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x -= a.x;
            b.y -= a.y;
            b.x = b.x * amount * amount + a.x;
            b.y = b.y * amount * amount + a.y;
            return b;
        }

        public static Vector3 EaseInQuad(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x -= a.x;
            b.y -= a.y;
            b.z -= a.z;
            b.x = b.x * amount * amount + a.x;
            b.y = b.y * amount * amount + a.y;
            b.z = b.z * amount * amount + a.z;
            return b;
        }

        public static Vector4 EaseInQuad(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x -= a.x;
            b.y -= a.y;
            b.z -= a.z;
            b.w -= a.w;
            b.x = b.x * amount * amount + a.x;
            b.y = b.y * amount * amount + a.y;
            b.z = b.z * amount * amount + a.z;
            b.w = b.w * amount * amount + a.w;
            return b;
        }

        public static Rect EaseInQuad(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin -= a.xMin;
            b.yMin -= a.yMin;
            b.xMax -= a.xMax;
            b.yMax -= a.yMax;
            b.xMin = b.xMin * amount * amount + a.xMin;
            b.yMin = b.yMin * amount * amount + a.yMin;
            b.xMax = b.xMax * amount * amount + a.xMax;
            b.yMax = b.yMax * amount * amount + a.yMax;
            return b;
        }

        public static RectOffset EaseInQuad(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left -= a.left;
            b.right -= a.right;
            b.top -= a.top;
            b.bottom -= a.bottom;
            b.left = (int)((float)b.left * amount * amount + (float)a.left);
            b.right = (int)((float)b.right * amount * amount + (float)a.right);
            b.top = (int)((float)b.top * amount * amount + (float)a.top);
            b.bottom = (int)((float)b.bottom * amount * amount + (float)a.bottom);
            return b;
        }

        public static Quaternion EaseInQuad(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseInQuad(0f, 1f, amount));
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Color EaseInQuad(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r -= a.r;
            b.g -= a.g;
            b.b -= a.b;
            b.a -= a.a;
            b.r = b.r * amount * amount + a.r;
            b.g = b.g * amount * amount + a.g;
            b.b = b.b * amount * amount + a.b;
            b.a = b.a * amount * amount + a.a;
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static float EaseOutQuad(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b -= a;
            return -b * amount * (amount - 2) + a;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector2 EaseOutQuad(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseOutQuad(a.y, b.y, amount, clamped);
            return b;
        }

        public static Vector3 EaseOutQuad(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseOutQuad(a.y, b.y, amount, clamped);
            b.z = EaseOutQuad(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector4 EaseOutQuad(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseOutQuad(a.y, b.y, amount, clamped);
            b.z = EaseOutQuad(a.z, b.z, amount, clamped);
            b.w = EaseOutQuad(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseOutQuad(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseOutQuad(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseOutQuad(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseOutQuad(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseOutQuad(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseOutQuad(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseOutQuad(a.left, b.left, amount, clamped);
            b.right = (int)EaseOutQuad(a.right, b.right, amount, clamped);
            b.top = (int)EaseOutQuad(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseOutQuad(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Quaternion EaseOutQuad(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseOutQuad(0f, 1f, amount));
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Color EaseOutQuad(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseOutQuad(a.r, b.r, amount, clamped);
            b.g = EaseOutQuad(a.g, b.g, amount, clamped);
            b.b = EaseOutQuad(a.b, b.b, amount, clamped);
            b.a = EaseOutQuad(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static float EaseInOutQuad(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            amount *= 2f;
            b -= a;
            if (amount < 1) return b / 2 * amount * amount + a;
            amount--;
            return -b / 2 * (amount * (amount - 2) - 1) + a;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area with additional bias control
        /// </summary>
        public static float EaseInOutQuadBias(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            amount *= 2f;
            b -= a;
            if (amount < 1) return b / 2 * amount * amount + a;
            amount--;
            return -b / 2 * (amount * (amount - 2) - 1) + a;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static float EaseInOutQuadInvert(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }

            b -= a;
            if (amount <= 0.5f) {
                return -b * amount * (amount - 2) + a; //EaseOutQuad
            }
            else {
                return b * amount * amount + a; // EaseInQuad
            }
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector2 EaseInOutQuad(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseInOutQuad(a.y, b.y, amount, clamped);
            return b;
        }

        public static Vector3 EaseInOutQuad(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseInOutQuad(a.y, b.y, amount, clamped);
            b.z = EaseInOutQuad(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector4 EaseInOutQuad(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutQuad(a.x, b.x, amount, clamped);
            b.y = EaseInOutQuad(a.y, b.y, amount, clamped);
            b.z = EaseInOutQuad(a.z, b.z, amount, clamped);
            b.w = EaseInOutQuad(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseInOutQuad(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseInOutQuad(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseInOutQuad(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseInOutQuad(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseInOutQuad(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseInOutQuad(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseInOutQuad(a.left, b.left, amount, clamped);
            b.right = (int)EaseInOutQuad(a.right, b.right, amount, clamped);
            b.top = (int)EaseInOutQuad(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseInOutQuad(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Quaternion EaseInOutQuad(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, amount);
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static float EaseInExpo(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b -= a;
            return b * Mathf.Pow(2f, 10f * (amount - 1f)) + a;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Vector2 EaseInExpo(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInExpo(a.x, b.x, amount, clamped);
            b.y = EaseInExpo(a.y, b.y, amount, clamped);
            return b;
        }

        public static Vector3 EaseInExpo(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInExpo(a.x, b.x, amount, clamped);
            b.y = EaseInExpo(a.y, b.y, amount, clamped);
            b.z = EaseInExpo(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Vector4 EaseInExpo(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInExpo(a.x, b.x, amount, clamped);
            b.y = EaseInExpo(a.y, b.y, amount, clamped);
            b.z = EaseInExpo(a.z, b.z, amount, clamped);
            b.w = EaseInExpo(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseInExpo(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseInExpo(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseInExpo(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseInExpo(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseInExpo(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseInExpo(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                else
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseInExpo(a.left, b.left, amount, clamped);
            b.right = (int)EaseInExpo(a.right, b.right, amount, clamped);
            b.top = (int)EaseInExpo(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseInExpo(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Quaternion EaseInExpo(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, Mathf.Pow(amount, 2));
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Color EaseInExpo(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseInExpo(a.r, b.r, amount, clamped);
            b.g = EaseInExpo(a.g, b.g, amount, clamped);
            b.b = EaseInExpo(a.b, b.b, amount, clamped);
            b.a = EaseInExpo(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static float EaseOutExpo(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b -= a;
            return b * (-Mathf.Pow(2f, -10f * amount) + 1) + a;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector2 EaseOutExpo(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseOutExpo(a.y, b.y, amount, clamped);
            return b;
        }

        public static Vector3 EaseOutExpo(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseOutExpo(a.y, b.y, amount, clamped);
            b.z = EaseOutExpo(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector4 EaseOutExpo(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseOutExpo(a.y, b.y, amount, clamped);
            b.z = EaseOutExpo(a.z, b.z, amount, clamped);
            b.w = EaseOutExpo(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseOutExpo(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseOutExpo(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseOutExpo(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseOutExpo(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseOutExpo(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseOutExpo(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseOutExpo(a.left, b.left, amount, clamped);
            b.right = (int)EaseOutExpo(a.right, b.right, amount, clamped);
            b.top = (int)EaseOutExpo(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseOutExpo(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Quaternion EaseOutExpo(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseOutExpo(0f, 1f, amount));
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Color EaseOutExpo(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseOutExpo(a.r, b.r, amount, clamped);
            b.g = EaseOutExpo(a.g, b.g, amount, clamped);
            b.b = EaseOutExpo(a.b, b.b, amount, clamped);
            b.a = EaseOutExpo(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static float EaseInOutExpo(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            amount /= .5f;
            b -= a;
            if (amount < 1) return b / 2 * Mathf.Pow(2, 10 * (amount - 1)) + a;
            amount--;
            return b / 2 * (-Mathf.Pow(2, -10 * amount) + 2) + a;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector2 EaseInOutExpo(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseInOutExpo(a.y, b.y, amount, clamped);
            return b;
        }
        public static Vector3 EaseInOutExpo(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseInOutExpo(a.y, b.y, amount, clamped);
            b.z = EaseInOutExpo(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector4 EaseInOutExpo(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutExpo(a.x, b.x, amount, clamped);
            b.y = EaseInOutExpo(a.y, b.y, amount, clamped);
            b.z = EaseInOutExpo(a.z, b.z, amount, clamped);
            b.w = EaseInOutExpo(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseInOutExpo(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseInOutExpo(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseInOutExpo(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseInOutExpo(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseInOutExpo(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseInOutExpo(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseInOutExpo(a.left, b.left, amount, clamped);
            b.right = (int)EaseInOutExpo(a.right, b.right, amount, clamped);
            b.top = (int)EaseInOutExpo(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseInOutExpo(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Quaternion EaseInOutExpo(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseInOutExpo(0f, 1f, amount));
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Color EaseInOutExpo(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseInOutExpo(a.r, b.r, amount, clamped);
            b.g = EaseInOutExpo(a.g, b.g, amount, clamped);
            b.b = EaseInOutExpo(a.b, b.b, amount, clamped);
            b.a = EaseInOutExpo(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static float EaseInCircle(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b -= a;
            return -b * (Mathf.Sqrt(1 - amount * amount) - 1) + a;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Vector2 EaseInCircle(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInCircle(a.x, b.x, amount, clamped);
            b.y = EaseInCircle(a.y, b.y, amount, clamped);
            return b;
        }
        public static Vector3 EaseInCircle(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInCircle(a.x, b.x, amount, clamped);
            b.y = EaseInCircle(a.y, b.y, amount, clamped);
            b.z = EaseInCircle(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Vector4 EaseInCircle(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInCircle(a.x, b.x, amount, clamped);
            b.y = EaseInCircle(a.y, b.y, amount, clamped);
            b.z = EaseInCircle(a.z, b.z, amount, clamped);
            b.w = EaseInCircle(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseInCircle(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseInCircle(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseInCircle(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseInCircle(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseInCircle(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseInCircle(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseInCircle(a.left, b.left, amount, clamped);
            b.right = (int)EaseInCircle(a.right, b.right, amount, clamped);
            b.top = (int)EaseInCircle(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseInCircle(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Quaternion EaseInCircle(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseInCircle(0f, 1f, amount));
        }

        /// <summary>
        /// Bias towards start: more slowly ramping up then ending quickly
        /// </summary>
        public static Color EaseInCircle(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseInCircle(a.r, b.r, amount, clamped);
            b.g = EaseInCircle(a.g, b.g, amount, clamped);
            b.b = EaseInCircle(a.b, b.b, amount, clamped);
            b.a = EaseInCircle(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static float EaseOutCircle(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            amount--;
            b -= a;
            return b * Mathf.Sqrt(1 - amount * amount) + a;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector2 EaseOutCircle(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseOutCircle(a.y, b.y, amount, clamped);
            return b;
        }
        public static Vector3 EaseOutCircle(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseOutCircle(a.y, b.y, amount, clamped);
            b.z = EaseOutCircle(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Vector4 EaseOutCircle(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseOutCircle(a.y, b.y, amount, clamped);
            b.z = EaseOutCircle(a.z, b.z, amount, clamped);
            b.w = EaseOutCircle(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseOutCircle(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseOutCircle(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseOutCircle(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseOutCircle(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseOutCircle(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseOutCircle(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseOutCircle(a.left, b.left, amount, clamped);
            b.right = (int)EaseOutCircle(a.right, b.right, amount, clamped);
            b.top = (int)EaseOutCircle(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseOutCircle(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Quaternion EaseOutCircle(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseOutCircle(0f, 1f, amount));
        }

        /// <summary>
        /// Bias towards end: ramping up quickly then approaching end slowly
        /// </summary>
        public static Color EaseOutCircle(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseOutCircle(a.r, b.r, amount, clamped);
            b.g = EaseOutCircle(a.g, b.g, amount, clamped);
            b.b = EaseOutCircle(a.b, b.b, amount, clamped);
            b.a = EaseOutCircle(a.a, b.a, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static float EaseInOutCircle(float a, float b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            amount /= .5f;
            b -= a;
            if (amount < 1) return -b / 2 * (Mathf.Sqrt(1 - amount * amount) - 1) + a;
            amount -= 2;
            return b / 2 * (Mathf.Sqrt(1 - amount * amount) + 1) + a;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector2 EaseInOutCircle(Vector2 a, Vector2 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseInOutCircle(a.y, b.y, amount, clamped);
            return b;
        }
        public static Vector3 EaseInOutCircle(Vector3 a, Vector3 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseInOutCircle(a.y, b.y, amount, clamped);
            b.z = EaseInOutCircle(a.z, b.z, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Vector4 EaseInOutCircle(Vector4 a, Vector4 b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.x = EaseInOutCircle(a.x, b.x, amount, clamped);
            b.y = EaseInOutCircle(a.y, b.y, amount, clamped);
            b.z = EaseInOutCircle(a.z, b.z, amount, clamped);
            b.w = EaseInOutCircle(a.w, b.w, amount, clamped);
            return b;
        }

        public static Rect EaseInOutCircle(Rect a, Rect b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.xMin = EaseInOutCircle(a.xMin, b.xMin, amount, clamped);
            b.yMin = EaseInOutCircle(a.yMin, b.yMin, amount, clamped);
            b.xMax = EaseInOutCircle(a.xMax, b.xMax, amount, clamped);
            b.yMax = EaseInOutCircle(a.yMax, b.yMax, amount, clamped);
            return b;
        }

        public static RectOffset EaseInOutCircle(RectOffset a, RectOffset b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.left = (int)EaseInOutCircle(a.left, b.left, amount, clamped);
            b.right = (int)EaseInOutCircle(a.right, b.right, amount, clamped);
            b.top = (int)EaseInOutCircle(a.top, b.top, amount, clamped);
            b.bottom = (int)EaseInOutCircle(a.bottom, b.bottom, amount, clamped);
            return b;
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Quaternion EaseInOutCircle(Quaternion a, Quaternion b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            return Quaternion.Slerp(a, b, EaseInOutCircle(0f, 1f, amount));
        }

        /// <summary>
        /// Ramps from start and approaches end slowly with faster middle area
        /// </summary>
        public static Color EaseInOutCircle(Color a, Color b, float amount, bool clamped = true)
        {
            if (clamped) {
                if (amount <= 0) return a;
                if (amount >= 1f) return b;
            }
            b.r = EaseInOutCircle(a.r, b.r, amount, clamped);
            b.g = EaseInOutCircle(a.g, b.g, amount, clamped);
            b.b = EaseInOutCircle(a.b, b.b, amount, clamped);
            b.a = EaseInOutCircle(a.a, b.a, amount, clamped);
            return b;
        }

        #endregion

        #region TIMEFLOW

        /// <summary>
        /// This compares 2 time values to determine if they represent the same time. This is needed to
        /// apply time tolerances and avoid rounding errors that otherwise may result in many keyframes
        /// overlapping the same time with microsecond offsets.
        /// </summary>
        public static bool IsTimeDifferent(float a, float b)
        {
            /// Use half tolerance to avoid coincident values
            return IsDifferent(a, b, TimeflowPreferences.Current.TimeTolerance * 0.5f);
        }

        /// <summary>
        /// This compares 2 keyframe values to determine whether they are different within the tolerance
        /// settings (set in preferences). This is primarily needed to avoid micro variations that may
        /// occur from floating point inaccuracies that can lead to unwanted keyframes or changes detected
        /// in the where none occurred (due to tiny rounding errors in UI processes).
        /// </summary>
        public static bool IsKeyDifferent(float a, float b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return Mathf.Abs(a - b) > tolerance;
        }

        public static bool IsKeyDifferent(Vector2 a, Vector2 b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance);
        }

        public static bool IsKeyDifferent(Vector3 a, Vector3 b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance) || IsDifferent(a.z, b.z, tolerance);
        }

        public static bool IsKeyDifferent(Vector4 a, Vector4 b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.x, b.x, tolerance) || IsDifferent(a.y, b.y, tolerance) || IsDifferent(a.z, b.z, tolerance) || IsDifferent(a.w, b.w, tolerance);
        }

        public static bool IsKeyDifferent(Color a, Color b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.r, b.r, tolerance) || IsDifferent(a.g, b.g, tolerance) || IsDifferent(a.b, b.b, tolerance) || IsDifferent(a.a, b.a, tolerance);
        }

        public static bool IsKeyDifferent(Rect a, Rect b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.xMin, b.xMin, tolerance) || IsDifferent(a.yMin, b.yMin, tolerance) || IsDifferent(a.xMax, b.xMax, tolerance) || IsDifferent(a.yMax, b.yMax, tolerance);
        }

        public static bool IsKeyDifferent(RectOffset a, RectOffset b)
        {
            float tolerance = TimeflowPreferences.Current.KeyTolerance;
            return IsDifferent(a.left, b.left, tolerance) || IsDifferent(a.right, b.right, tolerance) || IsDifferent(a.top, b.top, tolerance) || IsDifferent(a.bottom, b.bottom, tolerance);
        }

        #endregion
    }

}//AxonGenesis

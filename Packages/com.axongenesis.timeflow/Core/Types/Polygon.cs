// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Defines a polygon shape with unlimited vertices, and utility functions for working with them.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "Polygon")]
    public class Polygon : SerializableObject
    {
        #region PUBLIC

        public bool IsClosed;
        public bool IsExtended = true;
        public bool UseQuaternions = true;
        public bool InterpolateRotation = true;

        public Vector3[] Vertices;
        public Vector3[] EulerAngles;
        public Quaternion[] Quaternions;

        public enum Orientations
        {
            XZ,
            XY,
            ZY
        }

        public Orientations Orientation = Orientations.XZ;

        public enum Interpolations
        {
            Linear,
            Bezier
        }
        public Interpolations _Interpolation = Interpolations.Linear;

        public Interpolations Interpolation {
            get {
                return _Interpolation;
            }
            set {
                _Interpolation = value;
            }
        }

        /// <summary>
        /// Stores the computed length (distance) of the entire path.
        /// </summary>
        [NonSerialized]
        public float Length;

        /// <summary>
        /// An array of Distances is stored if needed for interpolation by GetPointAtPercent, otherwise
        /// this defaults off.
        /// </summary>
        [NonSerialized]
        public bool StoreDistances;

        /// <summary>
        /// Precomputed distance values between each node. This is not stored and regenerated when
        /// preparing for interpolation.
        /// </summary>
        [NonSerialized]
        public float[] Distances;

        #endregion

        #region PRIVATE
        #endregion

        public int Size {
            get {
                return Vertices == null ? 0 : Vertices.Length;
            }
        }

        public Polygon() { }

        // TODO: Optimize distance calculations for GetIndexAtPercent. Perhaps add an Optimize function which sets up the polygon to
        // precalculate the Distances, then use that array for calculations. Must be careful however to adjust correctly when 
        // vertices are changed.

        public Vector3 StartPoint {
            get {
                if (Vertices != null && Vertices.Length > 0) {
                    return Vertices[0];
                }
                else return Vector3.zero;
            }
            set {
                SetVertex(0, value);
            }
        }

        public Vector3 EndPoint {
            get {
                if (Vertices != null && Vertices.Length > 0) {
                    return Vertices[Vertices.Length - 1];
                }
                else return Vector3.zero;
            }
            set {
                if (Vertices.Length < 2) {
                    InsertVertex(1, value);
                }
                else {
                    SetVertex(Vertices.Length - 1, value);
                }
            }
        }

        public void Clear()
        {
            Vertices = null;
            EulerAngles = null;
            Quaternions = null;
            Distances = null;
            Length = 0;
        }

        /// <summary>
        /// Call Prepare after setting vertices and before any operations.
        /// </summary>
        public void Prepare()
        {
            CalculateLength();
        }

        /// <summary>
        /// PreCalculates an array of distances from one point to the next, used for interpolation
        /// calculations.
        /// </summary>
        public void PrepareForInterpolation()
        {
            Distances = null;
            CalculateLength();
        }

        /// <summary>
        /// Calculates and returns the sum total length of the polygon by measuring the distance between
        /// each point. Computed distance values are stored in memory if required for GetPointAtPercent.
        /// </summary>
        /// <returns></returns>
        public float CalculateLength()
        {
            Length = 0f;
            if (Vertices != null && Vertices.Length > 0) {
                if (StoreDistances) {
                    Distances = new float[Vertices.Length + 1];
                    Distances[0] = 0f;
                }
                for (int i = 1; i < Vertices.Length; i++) {
                    float d = MathUtil.Distance(Vertices[i], Vertices[i - 1]);
                    Length += d;
                    if (StoreDistances) Distances[i] = d;
                }
                if (IsClosed) {
                    float d = MathUtil.Distance(Vertices[0], Vertices[Vertices.Length - 1]);
                    Length += d;
                    if (StoreDistances) Distances[Distances.Length - 1] = d;
                }
            }
            return Length;
        }

        /// <summary>
        /// Checks whether the end points of the polygon match position, and if not then a new point is
        /// added at the end matching the starting point. The IsClosed flag is set to true, so any other
        /// processes that change the vertex positions can correctly handle the last point.
        /// </summary>
        public void Close()
        {
            IsClosed = true;
            Vector2 a = Vertices[Vertices.Length - 1];
            Vector2 b = Vertices[0];
            if (a.x != b.x || a.y != b.y) {
                Vector3[] newVerts = new Vector3[Vertices.Length + 1];
                for (int x = 0; x < Vertices.Length; x++) {
                    newVerts[x] = Vertices[x];
                }
                newVerts[Vertices.Length] = Vertices[0];
                Vertices = newVerts;
            }
        }

        /// <summary>
        /// Searches the vertices list for a vertex exactly matching the position, returning its index
        /// value.
        /// </summary>
        public int FindVertex(Vector3 point)
        {
            int index = -1;
            for (int i = 0; i < Vertices.Length; i++) {
                if (Vertices[i] == point) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        /// <summary>
        /// Inserts a new vertex at the index position.
        /// </summary>
        public void InsertVertex(int index, Vector3 point)
        {
            InsertVertex(index, point, false, Vector3.zero);
        }

        public void InsertVertex(int index, Vector3 point, bool applyRotation, Quaternion rotation)
        {
            InsertVertex(index, point, applyRotation, Vector3.zero, rotation);
        }

        public void InsertVertex(int index, Vector3 point, bool applyRotation, Vector3 euler)
        {
            InsertVertex(index, point, applyRotation, euler, Quaternion.identity);
        }

        public void InsertVertex(int index, Vector3 point, bool applyRotation, Vector3 euler, Quaternion rotation)
        {
            if (Vertices == null) {
                Vertices = new Vector3[1];
                Vertices[0] = point;

                if (applyRotation) {
                    if (UseQuaternions) {
                        Quaternions = new Quaternion[1];
                        Quaternions[0] = rotation;
                    }
                    else {
                        EulerAngles = new Vector3[1];
                        EulerAngles[0] = euler;
                    }
                }
            }
            else {
                Vector3[] newVertices = new Vector3[Vertices.Length + 1];
                Vector3[] newEuler = null;
                Quaternion[] newQuaternion = null;

                if (applyRotation) {
                    if (UseQuaternions) {
                        newQuaternion = new Quaternion[Vertices.Length + 1];
                    }
                    else {
                        newEuler = new Vector3[Vertices.Length + 1];
                    }
                }

                if (index < 0) {
                    index = Vertices.Length;
                }

                int x = 0;
                for (int i = 0; i < Vertices.Length + 1; i++) {
                    if (i == index) {
                        newVertices[x] = point;
                        if (applyRotation) {
                            if (UseQuaternions) {
                                newQuaternion[x] = rotation;
                            }
                            else {
                                newEuler[x] = euler;
                            }
                        }
                        x++;
                    }
                    if (i < Vertices.Length) {
                        newVertices[x] = Vertices[i];
                        if (applyRotation) {
                            if (UseQuaternions) {
                                newQuaternion[x] = Quaternions[i];
                            }
                            else {
                                newEuler[x] = EulerAngles[i];
                            }
                        }
                    }
                    x++;
                }
                if (index == 0 && IsClosed) {
                    // For a closed shape make the last point match the new first one
                    newVertices[Vertices.Length] = newVertices[0];
                    if (applyRotation) {
                        if (UseQuaternions) {
                            newQuaternion[Vertices.Length] = newQuaternion[0];
                        }
                        else {
                            newEuler[Vertices.Length] = newEuler[0];
                        }
                    }
                }

                Vertices = newVertices;
                if (applyRotation) {
                    if (UseQuaternions) {
                        Quaternions = newQuaternion;
                    }
                    else {
                        EulerAngles = newEuler;
                    }
                }
            }
        }

        public void SetVertex(int index, Vector3 point)
        {
            SetVertex(index, point, false, Vector3.zero);
        }

        public void SetVertex(int index, Vector3 point, bool applyRotation, Quaternion rotation)
        {
            SetVertex(index, point, applyRotation, Vector3.zero, rotation);
        }

        public void SetVertex(int index, Vector3 point, bool applyRotation, Vector3 euler)
        {
            SetVertex(index, point, applyRotation, euler, Quaternion.identity);
        }

        public void SetVertex(int index, Vector3 point, bool applyRotation, Vector3 euler, Quaternion rotation)
        {
            if (Vertices != null && index >= 0 && index <= Vertices.Length - 1) {
                Vertices[index] = point;

                if (applyRotation) {
                    if (UseQuaternions) {
                        Quaternions[index] = rotation;
                    }
                    else {
                        EulerAngles[index] = euler;
                    }
                }
            }
        }

        public void RemoveVertex(int index)
        {
            if (Vertices != null && index >= 0 && index <= Vertices.Length - 1) {
                int newLength = Vertices.Length - 1;
                Vector3[] newVertices = new Vector3[newLength];

                for (int i = 0; i < index; i++) {
                    newVertices[i] = Vertices[i];
                }
                for (int i = index + 1; i < Vertices.Length; i++) {
                    newVertices[i - 1] = Vertices[i];
                }

                Vertices = newVertices;
            }
        }

        public Vector3 PopVertex()
        {
            Vector3 v = Vector3.zero;

            if (Vertices != null && Vertices.Length > 1) {
                v = Vertices[Vertices.Length - 1];

                Vector3[] newVertices = new Vector3[Vertices.Length - 2];
                for (int i = 0; i < newVertices.Length; i++) {
                    newVertices[i] = Vertices[i];
                }
                Vertices = newVertices;

                if (UseQuaternions && Quaternions != null) {
                    Quaternion[] newQuaternion = null;
                    newQuaternion = new Quaternion[Vertices.Length + 1];
                    for (int i = 0; i < newQuaternion.Length; i++) {
                        newQuaternion[i] = Quaternions[i];
                    }
                    Quaternions = newQuaternion;
                }
                else {
                    Vector3[] newEuler = null;
                    newEuler = new Vector3[Vertices.Length + 1];
                    for (int i = 0; i < newEuler.Length; i++) {
                        newEuler[i] = EulerAngles[i];
                    }
                    EulerAngles = newEuler;
                }
            }
            return v;
        }

        /// <summary>
        /// Interpolates along the polygon to insert a new point at the specified percentage along the
        /// line.
        /// </summary>
        public int InsertVertexAtPercent(float percent)
        {
            Vector3[] newVertices = new Vector3[Vertices.Length + 1];
            Length = CalculateLength();

            int index = 0;
            int x = 0;
            int i = 0;

            // If the percent is out of range, loop it
            if (Length == 0) Length = CalculateLength();

            if (IsClosed) {
                percent = percent - Mathf.FloorToInt(percent);
            }
            else {
                // Clamp the value
                percent = Mathf.Clamp(percent, 0f, 1f);
            }
            float pos = percent * Length;

            if (percent == 0f) {
                index = 0;
                newVertices[index] = Vertices[0];
                x++;
                for (i = 0; i < Vertices.Length; i++) {
                    newVertices[x] = Vertices[i];
                }
            }
            else
            if (percent == 1f) {
                index = Vertices.Length;
                newVertices[index] = Vertices[Vertices.Length - 1];
                for (i = 0; i < Vertices.Length; i++) {
                    newVertices[x] = Vertices[i];
                }
            }
            else {
                bool placed = false;
                float posNow = 0f;
                float posPrev = 0f;
                newVertices[0] = Vertices[0];
                x = 1;
                for (i = 1; i < Vertices.Length; i++) {
                    if (!placed) {
                        posPrev = posNow;
                        posNow += MathUtil.Distance(Vertices[i - 1], Vertices[i]);

                        if (pos > posPrev && pos < posNow && posNow > posPrev) {
                            // insert the point when it is between the last and current vertices
                            float interp = (pos - posPrev) / (posNow - posPrev);
                            newVertices[x] = MathUtil.Interpolate(Vertices[i - 1], Vertices[i], interp);
                            placed = true;
                            index = x;
                            x++;
                        }
                    }
                    newVertices[x] = Vertices[i];
                    x++;
                }
            }

            Vertices = newVertices;
            return index;
        }

        /// <summary>
        /// Returns the index of the nearest vertex interpolating along the polygon by percent.
        /// </summary>
        public int GetIndexAtPercent(float percent)
        {
            if (Vertices == null || Vertices.Length == 0) return 0;
            if (Vertices.Length == 1) {
                return 0;
            }
            int index = 0;
            if (Length == 0) Length = CalculateLength();

            if (IsClosed) {
                percent = percent - Mathf.FloorToInt(percent);
            }
            else {
                // Clamp the value
                percent = Mathf.Clamp(percent, 0f, 1f);
            }
            if (percent == 0f) {
                return 0;
            }
            else
            if (percent == 1f) {
                return Vertices.Length - 1;
            }
            else {
                //Vector3 point = Vector3.zero;
                int i = 0;

                // If the percent is out of range, loop it
                while (percent < 0f) {
                    percent += 1f;
                }
                while (percent > 1f) {
                    percent -= 1f;
                }
                float pos = percent * Length;

                bool placed = false;
                float posNow = 0f;
                float posPrev = 0f;
                for (i = 1; i < Vertices.Length; i++) {
                    if (!placed) {
                        posPrev = posNow;
                        posNow += MathUtil.Distance(Vertices[i - 1], Vertices[i]);

                        if (pos > posPrev && pos < posNow) {
                            // insert the point when it is between the last and current vertices
                            index = i;
                            placed = true;
                            break;
                        }
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Returns the nearest vertex point value interpolating along the polygon by percent.
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public Vector3 GetPointAtPercent(float percent)
        {
            Vector3 point = Vector3.zero;
            Vector3 rotation = Vector3.zero;

            GetPointAtPercent(percent, out point, out rotation);
            return point;
        }

        /// <summary>
        /// Returns the position and rotation at the specified percent, interpolating between vertices.
        /// </summary>
        public void GetPointAtPercent(float percent, out Vector3 point, out Vector3 euler)
        {
            Quaternion quaternion = Quaternion.identity;
            GetPointAtPercent(percent, out point, out euler, out quaternion);
        }

        public void GetPointAtPercent(float percent, out Vector3 point, out Quaternion quaternion)
        {
            Vector3 euler = Vector3.zero;
            GetPointAtPercent(percent, out point, out euler, out quaternion);
        }

        public void GetPointAtPercent(float percent, out Vector3 point, out Vector3 euler, out Quaternion quaternion)
        {
            point = Vector3.zero;
            euler = Vector3.zero;
            quaternion = Quaternion.identity;

            if (Vertices == null || Vertices.Length == 0) {
                Debug.LogWarning("Poly has no vertices");
                return;
            }
            point = Vertices[0];

            bool hasQuaternions = false;
            hasQuaternions = Quaternions != null && Quaternions.Length == Vertices.Length;
            if (hasQuaternions) {
                quaternion = Quaternions[0];
                euler = quaternion.eulerAngles;
            }

            bool hasEuler = false;
            if (!hasQuaternions) {
                hasEuler = EulerAngles != null && EulerAngles.Length == Vertices.Length;
                if (hasEuler) {
                    euler = EulerAngles[0];
                    quaternion = Quaternion.Euler(euler);
                }
            }

            if (Vertices.Length == 1) {
                Debug.LogWarning("Poly has only 1 vertex");
                return;
            }
            if (Length == 0) Length = CalculateLength();
            if (Distances == null || Distances.Length != Vertices.Length + 1) {
                StoreDistances = true;
                PrepareForInterpolation();
            }
            if (IsClosed) {
                percent = percent - Mathf.FloorToInt(percent);
            }
            else
            if (!IsExtended) {
                // Clamp the value
                percent = Mathf.Clamp(percent, 0f, 1f);
            }
            if (IsExtended && (percent < 0f || percent > 1f)) {
                if (percent > 1f) {
                    int a = Vertices.Length - 1;
                    percent -= 1f;
                    if (hasQuaternions) {
                        quaternion = Quaternions[a];
                        euler = quaternion.eulerAngles;
                    }
                    else
                    if (hasEuler) {
                        euler = EulerAngles[a];
                        quaternion = Quaternion.Euler(euler);
                    }
                }
                float segmentPercent = 1f / Vertices.Length;
                point = MathUtil.Subtract(Vertices[1], Vertices[0]);
                point = MathUtil.Multiply(point, segmentPercent * percent);
                point = MathUtil.Add(point, Vertices[0]);
            }
            else
            if (percent == 0f) {
            }
            else
            if (percent == 1f) {
                if (IsClosed) {
                }
                else {
                    point = Vertices[Vertices.Length - 1];
                    if (hasQuaternions) {
                        quaternion = Quaternions[Vertices.Length - 1];
                        euler = quaternion.eulerAngles;
                    }
                    else
                    if (hasEuler) {
                        euler = EulerAngles[Vertices.Length - 1];
                        quaternion = Quaternion.Euler(euler);
                    }
                }
            }
            else {
                point = Vector3.zero;
                int i = 0;
                float posNow = 0f;
                float posPrev = 0f;
                int last = Vertices.Length - 1;
                int total = last;
                if (IsClosed) total++;

                float target = percent * Length;

                for (i = 0; i < total; i++) {
                    int a = i;
                    int b = a + 1;

                    if (IsClosed) {
                        while (b > last) b -= Vertices.Length;
                        while (b < 0) b += Vertices.Length;

                        while (a > last) a -= Vertices.Length;
                        while (a < 0) a += Vertices.Length;
                    }
                    else {
                        if (b > last) b = last;
                        else
                        if (b < 0) b = 0;

                        if (a > last) a = last;
                        else
                        if (a < 0) a = 0;
                    }

                    posPrev = posNow;
                    posNow += Distances[i + 1];

                    if (target > posPrev && target <= posNow) {
                        // insert the point when it is between the last and current vertices
                        float n = posNow - posPrev;
                        if (n == 0) {
                            point = Vertices[a];
                            if (hasQuaternions) {
                                quaternion = Quaternions[a];
                                euler = quaternion.eulerAngles;
                            }
                            else
                            if (hasEuler) {
                                euler = EulerAngles[a];
                                quaternion = Quaternion.Euler(euler);
                            }
                        }
                        else {
                            float interp = (target - posPrev) / n;
                            if (Interpolation == Interpolations.Linear) {
                                point = MathUtil.Interpolate(Vertices[a], Vertices[b], interp);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector3 atan = Vector3.zero;
                                Vector3 btan = Vector3.zero;

                                if (a > 1) {
                                    Vector3 v = Vertices[a] - Vertices[a - 1];
                                    atan = MathUtil.EaseInOutQuad(Vertices[a], Vertices[a] + v, 0.25f);
                                }
                                else {
                                    atan = MathUtil.EaseInOutQuad(Vertices[a], Vertices[b], 0.25f);
                                }
                                atan = MathUtil.EaseInOutQuad(Vertices[a], Vertices[b], 0.25f);

                                if (b < last - 1) {
                                    Vector3 v = Vertices[b] - Vertices[b + 1];
                                    btan = MathUtil.EaseInOutQuad(Vertices[b], Vertices[b] + v, 0.25f);
                                }
                                else {
                                    btan = MathUtil.EaseInOutQuad(Vertices[a], Vertices[b], 0.75f);
                                }

                                BezierCurve3D bz = new BezierCurve3D(Vertices[a], atan, btan, Vertices[b]);
                                point = bz.GetPointAtTime(interp);
                            }
                            if (hasQuaternions) {
                                if (InterpolateRotation) {
                                    quaternion = Quaternion.Lerp(Quaternions[a], Quaternions[b], interp);
                                }
                                else {
                                    quaternion = Quaternions[a];
                                }
                                euler = quaternion.eulerAngles;
                            }
                            else
                            if (hasEuler) {
                                if (InterpolateRotation) {
                                    euler = MathUtil.Interpolate(EulerAngles[a], EulerAngles[b], interp);
                                }
                                else {
                                    if (interp < 0.9f) {
                                        euler = EulerAngles[a];
                                    }
                                    else {
                                        euler = EulerAngles[b];
                                    }
                                }
                                quaternion = Quaternion.Euler(euler);
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate linear interpolation based on vertex indices rather than percentage of distance. Use
        /// this to interpolate baked data to preserve velocity of original data.
        /// </summary>
        public Vector3 Interpolate(float interp)
        {
            Vector3 pos = Vector3.zero;

            if (Vertices != null && Vertices.Length > 0) {
                if (interp <= 0f) {
                    pos = Vertices[0];
                }
                else
                if (interp >= 1f) {
                    pos = Vertices[Vertices.Length - 1];
                }
                else {
                    float i = interp * (float)(Vertices.Length - 1);
                    int a = Mathf.FloorToInt(i);
                    int b = Mathf.CeilToInt(i);
                    if (a >= Vertices.Length) {
                        /// out of range
                        pos = Vertices[a];
                    }
                    else
                    if (a == b || b >= Vertices.Length) {
                        pos = Vertices[a];
                    }
                    else {
                        float d = i - (float)a;
                        pos = MathUtil.Interpolate(Vertices[a], Vertices[b], d);
                    }
                }
            }
            return pos;
        }

        public Vector3 GetNearestPoint(Vector3 point)
        {
            if (Vertices == null || Vertices.Length == 0) return Vector3.zero;

            float d = 0f;
            float dist = MathUtil.Distance(point, Vertices[0]);
            int a = 0;

            for (int i = 1; i < Vertices.Length; i++) {
                d = MathUtil.Distance(point, Vertices[i]);
                if (d < dist) {
                    dist = d;
                    a = i;
                }
            }

            return Vertices[a];
        }

        public int GetNearestPoint(Vector3 point, out Vector3 position, out Quaternion rotation, out float completion)
        {
            if (Vertices == null || Vertices.Length < 2) {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                completion = 0;
                return 0;
            }

            float d = 0f;
            float dist = MathUtil.Distance(point, Vertices[0]);
            int a = 0;
            int b = 1;

            for (int i = 1; i < Vertices.Length; i++) {
                d = MathUtil.Distance(point, Vertices[i]);
                if (d < dist) {
                    dist = d;
                    a = i;
                }
            }
            if (a == Vertices.Length - 1) {
                if (!IsClosed) {
                    b = a;
                }
                else {
                    Vector3 pNorm = (Vertices[a - 1] - Vertices[a]).normalized;
                    Vector3 nNorm = (Vertices[0] - Vertices[a]).normalized;

                    Vector3 pointRel = point - Vertices[a];
                    float pDist = MathUtil.Distance(pointRel, pNorm);
                    float nDist = MathUtil.Distance(pointRel, nNorm);
                    if (pDist < nDist) {
                        b = a;
                        a--;
                    }
                    else {
                        b = 0;
                    }
                }
            }
            else
            if (a == 0) {
                if (!IsClosed) {
                    b = a;
                }
                else {
                    Vector3 pNorm = (Vertices[Vertices.Length - 1] - Vertices[a]).normalized;
                    Vector3 nNorm = (Vertices[a + 1] - Vertices[a]).normalized;

                    Vector3 pointRel = point - Vertices[a];
                    float pDist = MathUtil.Distance(pointRel, pNorm);
                    float nDist = MathUtil.Distance(pointRel, nNorm);
                    if (pDist < nDist) {
                        b = Vertices.Length - 1;
                    }
                    else {
                        b = 1;
                    }
                }
            }
            else {
                Vector3 pNorm = (Vertices[a - 1] - Vertices[a]).normalized;
                Vector3 nNorm = (Vertices[a + 1] - Vertices[a]).normalized;

                Vector3 pointRel = point - Vertices[a];
                float pDist = MathUtil.Distance(pointRel, pNorm);
                float nDist = MathUtil.Distance(pointRel, nNorm);
                if (pDist < nDist) {
                    b = a;
                    a = b - 1;
                }
                else {
                    b = a + 1;
                }
            }
            if (a == b) {
                b = a + 1;
                if (b >= Vertices.Length) {
                    b = a;
                    a -= 1;
                }
            }

            float interp;
            position = MathUtil.NearestPointOnLine(Vertices[a], Vertices[b], point, true, out interp);

            if (InterpolateRotation) {
                rotation = Quaternion.Lerp(Quaternions[a], Quaternions[b], interp);
            }
            else {
                rotation = Quaternions[a];
            }

            completion = MathUtil.Interpolate((float)a / (float)(Vertices.Length - 1), (float)b / (float)(Vertices.Length - 1), interp);
            return a;
        }

        public int GetNextPoint(int pointIndex, out Vector3 position, out Quaternion rotation, out float completion)
        {
            int a = pointIndex + 1;

            if (a >= Vertices.Length) {
                a = Vertices.Length - 1;
            }
            position = Vertices[a];
            rotation = Quaternions[a];

            completion = (float)a / (float)Vertices.Length;

            return a;
        }

        /// <summary>
        /// This modifies a polygon by removing points from index a to index b, creating a new shortened
        /// list of vertices. Visually this is like slicing a chunk of the polygon off.
        /// </summary>
        public Vector2 Slice(int a, int b)
        {
            bool invert = b < a;

            // Delete the points between a and b
            int dif = (b - a) - 1;
            if (invert) {
                dif = (b + (Vertices.Length - a)) - 1;
            }
            if (dif > 0) {
                Vector3[] newVertices = new Vector3[Vertices.Length - dif];
                int x = 0;
                if (invert) {
                    // Make the last vertex the first
                    newVertices[x] = Vertices[a];
                    x++;
                }
                for (int i = 0; i < Vertices.Length; i++) {
                    if ((invert && (i >= b && i < a)) || (!invert && (i <= a || i >= b))) {
                        newVertices[x] = Vertices[i];
                        x++;
                    }
                }
                // TODO: If Rotations isn't null, process it the same way
                Vertices = newVertices;
            }
            if (IsClosed) {
                Close();
            }

            // Adjust the indices for the deleted Vertices
            if (!invert) {
                b = b - dif;
            }
            else {
                a = 0;
                b = 1;
            }

            return new Vector2(a, b);
        }

        /// <summary>
        /// This performs a slice while also inserting the new specified vertices at the cut points.
        /// </summary>
        public Vector2 Slice(int a, Vector3 av, int b, Vector3 bv)
        {
            Vector2 indices;
            if (a > b) {
                // This is cutting the start and end
                InsertVertex(b, bv);
                a++; // Offset after inserting the first vertex
                InsertVertex(a, av);
                indices = Slice(a, b);
            }
            else {
                InsertVertex(a, av);
                b++; // Offset after inserting the first vertex
                InsertVertex(b, bv);
                indices = Slice(a, b);
            }
            return indices;
        }

        /// <summary>
        /// Works like Slice, but rather than specifying vertex indices, you can use interpolation percents
        /// to cut. New vertices are inserted at the cut poitns to maintain the shape of the rest of the
        /// polygon.
        /// </summary>
        public Vector2 SlicePercent(float from, float to)
        {
            int a = InsertVertexAtPercent(from);
            int b = InsertVertexAtPercent(to);

            while (from < 0f) {
                from += 1f;
            }
            while (from > 1f) {
                from -= 1f;
            }
            while (to < 0f) {
                to += 1f;
            }
            while (to > 1f) {
                to -= 1f;
            }

            if (from > to) {
                // a comes after b, but was created before b was
                a++;
            }

            return Slice(a, b);
        }

        /// <summary>
        /// Generates a flat filled polygon mesh.
        /// </summary>
        public Mesh BuildMesh(float width, int mode)
        {
            Vector3[] vertices = new Vector3[Vertices.Length * 2];
            Vector2[] uvs = new Vector2[Vertices.Length * 2];

            int triangleCount = (Vertices.Length) * 2 * 3;
            int[] triangles = new int[triangleCount];

            float uvx = 0f;
            float xlen = 0f;
            float uvy = 1f;

            float segLength = CalculateLength();

            float lineWidth = width;
            Vector3 a = Vertices[0];
            Vector3 b = Vector3.zero;
            Vector3 c = Vector3.zero;

            Vector2 a1 = Vector2.zero;
            Vector2 a2 = Vector2.zero;
            Vector2 a3 = Vector2.zero;
            Vector2 a4 = Vector2.zero;

            Vector2 b1 = Vector2.zero;
            Vector2 b2 = Vector2.zero;
            Vector2 b3 = Vector2.zero;
            Vector2 b4 = Vector2.zero;

            int v = 0;
            int t = 0;
            float angleA;
            float angleB;
            int start = 1;

            if (IsClosed) {
                start = 0;
                a = Vertices[Vertices.Length - 2];
            }

            for (int i = start; i < Vertices.Length; i++) {
                b = Vertices[i];

                // Create the bounding vertices of the 1st outline
                angleA = MathUtil.Angle(a, b);
                float apx = Mathf.Sin(angleA) * lineWidth;
                float apy = Mathf.Cos(angleA) * lineWidth;

                if (mode == 1) {
                    a1 = new Vector2(a.x - apx, a.y + apy);
                    a2 = new Vector2(a.x, a.y);
                    a3 = new Vector2(b.x - apx, b.y + apy);
                    a4 = new Vector2(b.x, b.y);
                }
                else
                if (mode == 2) {
                    a1 = new Vector2(a.x, a.y);
                    a2 = new Vector2(a.x + apx, a.y - apy);
                    a3 = new Vector2(b.x, b.y);
                    a4 = new Vector2(b.x + apx, b.y - apy);
                }
                else {
                    a1 = new Vector2(a.x - apx, a.y + apy);
                    a2 = new Vector2(a.x + apx, a.y - apy);
                    a3 = new Vector2(b.x - apx, b.y + apy);
                    a4 = new Vector2(b.x + apx, b.y - apy);
                }

                bool third = false;
                if (i + 1 < Vertices.Length) {
                    // Create the bounding vertices of the 2nd outline
                    c = Vertices[i + 1];
                    third = true;
                }
                else
                if (IsClosed) {
                    c = Vertices[1];
                    third = true;
                }
                if (third) {
                    angleB = MathUtil.Angle(b, c);
                    float bpx = Mathf.Sin(angleB) * lineWidth;
                    float bpy = Mathf.Cos(angleB) * lineWidth;

                    if (mode == 1) {
                        b1 = new Vector2(b.x - bpx, b.y + bpy);
                        b2 = new Vector2(b.x, b.y);
                        b3 = new Vector2(c.x - bpx, c.y + bpy);
                        b4 = new Vector2(c.x, c.y);
                    }
                    else
                    if (mode == 2) {
                        b1 = new Vector2(b.x, b.y);
                        b2 = new Vector2(b.x + bpx, b.y - bpy);
                        b3 = new Vector2(c.x, c.y);
                        b4 = new Vector2(c.x + bpx, c.y - bpy);
                    }
                    else {
                        b1 = new Vector2(b.x - bpx, b.y + bpy);
                        b2 = new Vector2(b.x + bpx, b.y - bpy);
                        b3 = new Vector2(c.x - bpx, c.y + bpy);
                        b4 = new Vector2(c.x + bpx, c.y - bpy);
                    }

                    // Calculate the intersection points of the boxes
                    a3 = MathUtil.Intersect(a1, a3, b1, b3);
                    a4 = MathUtil.Intersect(a2, a4, b2, b4);
                }

                // Build the vertices and uvs arrays
                if (i == 0 || segLength == 0) {
                    xlen = 0f;
                    uvx = 0f;

                    uvs[v] = new Vector2(uvx, uvy);
                    vertices[v] = a3;
                    v++;

                    uvs[v] = new Vector2(uvx, 0f);
                    vertices[v] = a4;
                    v++;
                }
                else {
                    xlen += MathUtil.Distance(a, b);
                    uvx = xlen / segLength;

                    if (i == 1 && !IsClosed) {
                        uvs[v] = new Vector2(uvx, uvy);
                        vertices[v] = a1;
                        v++;

                        uvs[v] = new Vector2(uvx, 0f);
                        vertices[v] = a2;
                        v++;
                    }
                    uvs[v] = new Vector2(uvx, uvy);
                    vertices[v] = a3;
                    v++;

                    uvs[v] = new Vector2(uvx, 0f);
                    vertices[v] = a4;
                    v++;
                }

                // Build the triangles array
                if (i > 0) {
                    int to = ((i - 1) * 2);
                    triangles[t] = 0 + to; t++;
                    triangles[t] = 2 + to; t++;
                    triangles[t] = 1 + to; t++;
                    triangles[t] = 1 + to; t++;
                    triangles[t] = 2 + to; t++;
                    triangles[t] = 3 + to; t++;
                }

                a = b;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.normals = new Vector3[vertices.Length];

            return mesh;
        }

        /// <summary>
        /// Generates UV coordinates for each vertex, using a basic flat mapping.
        /// </summary>
        /// <returns></returns>
        public Vector2[] CalculateUVs()
        {
            Vector2[] uvs = new Vector2[Vertices.Length];

            Vector2 min = Vertices[0];
            Vector2 max = Vertices[0];

            for (int i = 1; i < Vertices.Length; i++) {
                if (min.x > Vertices[i].x) min.x = Vertices[i].x;
                if (min.y > Vertices[i].y) min.y = Vertices[i].y;
                if (max.x < Vertices[i].x) max.x = Vertices[i].x;
                if (max.y < Vertices[i].y) max.y = Vertices[i].y;
            }

            Vector2 size = MathUtil.Subtract(max, min);

            for (int i = 0; i < Vertices.Length; i++) {
                Vector3 min3 = min;
                Vector2 v1 = MathUtil.Subtract(Vertices[i], min3);
                uvs[i] = MathUtil.Divide(v1, size);
            }

            return uvs;
        }

        /// <summary>
        /// Triangulates the shape and returns and indexed list ready for use with a Unity mesh object.
        /// </summary>
        public int[] GetTriangles()
        {
            return GetTriangleIndices(Triangulate());
        }

        /// <summary>
        /// Performs ear-clipping triangulation on the polygon. If the polygon is self-intersecting or
        /// poorly formed, null will return.  Otherwise a list of triangles is returned.
        /// </summary>
        public Triangle[] Triangulate()
        {
            int vNum = Vertices.Length;
            if (vNum < 3) {
                Debug.LogWarning("Polygon.Triangulate: The polygon does not have enough vertices to form a surface.");
                return null;
            }

            int i;
            Triangle toAdd;
            Triangle[] buffer = new Triangle[vNum];
            int bufferSize = 0;
            float[] xrem = new float[vNum];
            float[] yrem = new float[vNum];

            ArrayList triangleIndices = new ArrayList();

            for (i = 0; i < vNum; ++i) {
                xrem[i] = Vertices[i].x;
                yrem[i] = Vertices[i].y;
            }

            int ti = 0;
            string tlist = "";
            while (vNum > 3) {
                //Find an ear
                int earIndex = -1;
                for (i = 0; i < vNum; ++i) {
                    if (TriangulateEarTest(i, xrem, yrem)) {
                        earIndex = i;
                        break;
                    }
                }
                if (earIndex == -1) {
                    //Debug.LogWarning("Malformed geometery. Unable to triangulate.");
                    return null;
                }

                --vNum;
                float[] newx = new float[vNum];
                float[] newy = new float[vNum];
                int currDest = 0;
                for (i = 0; i < vNum; ++i) {
                    if (currDest == earIndex) ++currDest;
                    newx[i] = xrem[currDest];
                    newy[i] = yrem[currDest];
                    ++currDest;
                }

                // Add the clipped triangle to the list
                int under = (earIndex == 0) ? (xrem.Length - 1) : (earIndex - 1);
                int over = (earIndex == xrem.Length - 1) ? 0 : (earIndex + 1);

                triangleIndices.Add(earIndex);
                triangleIndices.Add(over);
                triangleIndices.Add(under);
                tlist += ti + "(" + earIndex + "," + over + "," + under + ") ";

                toAdd = new Triangle(xrem[earIndex], yrem[earIndex], xrem[over], yrem[over], xrem[under], yrem[under]);
                buffer[bufferSize] = toAdd;
                ++bufferSize;

                xrem = newx;
                yrem = newy;
            }
            triangleIndices.Add(1);
            triangleIndices.Add(2);
            triangleIndices.Add(0);
            tlist += ti + "(1,2,0) ";
            toAdd = new Triangle(xrem[1], yrem[1], xrem[2], yrem[2], xrem[0], yrem[0]);
            buffer[bufferSize] = toAdd;
            ++bufferSize;

            Triangle[] triangles = new Triangle[bufferSize];
            for (i = 0; i < bufferSize; i++) {
                triangles[i] = buffer[i];
            }
            return triangles;
        }

        /// <summary>
        /// Tests whether the vertex is an ear.
        /// </summary>
        public static bool TriangulateEarTest(int i, float[] xv, float[] yv)
        {
            float dx0 = 0;
            float dy0 = 0;
            float dx1 = 0;
            float dy1 = 0;
            if (i >= xv.Length || i < 0 || xv.Length < 3) {
                return false;
            }
            int upper = i + 1;
            int lower = i - 1;
            if (i == 0) {
                dx0 = xv[0] - xv[xv.Length - 1]; dy0 = yv[0] - yv[yv.Length - 1];
                dx1 = xv[1] - xv[0]; dy1 = yv[1] - yv[0];
                lower = xv.Length - 1;
            }
            else
            if (i == xv.Length - 1) {
                dx0 = xv[i] - xv[i - 1]; dy0 = yv[i] - yv[i - 1];
                dx1 = xv[0] - xv[i]; dy1 = yv[0] - yv[i];
                upper = 0;
            }
            else {
                dx0 = xv[i] - xv[i - 1]; dy0 = yv[i] - yv[i - 1];
                dx1 = xv[i + 1] - xv[i]; dy1 = yv[i + 1] - yv[i];
            }
            float cross = dx0 * dy1 - dx1 * dy0;
            if (cross > 0f) return false;
            Triangle myTri = new Triangle(xv[i], yv[i], xv[upper], yv[upper], xv[lower], yv[lower]);
            for (int j = 0; j < xv.Length; ++j) {
                if (j == i || j == lower || j == upper) continue;
                if (myTri.IsInside(xv[j], yv[j])) return false;
            }
            return true;
        }

        /// <summary>
        /// Converts a list of triangle coordinates into polygon indices, which is the way that Unity
        /// requires triangles to be structured.
        /// </summary>
        public int[] GetTriangleIndices(Triangle[] trianglesList)
        {
            int[] triangles = new int[trianglesList.Length * 3];
            int i = 0;
            foreach (Triangle t in trianglesList) {
                triangles[i] = FindVertex(new Vector3(t.x[2], t.y[2], 0f));
                i++;
                triangles[i] = FindVertex(new Vector3(t.x[1], t.y[1], 0f));
                i++;
                triangles[i] = FindVertex(new Vector3(t.x[0], t.y[0], 0f));
                i++;
            }
            return triangles;
        }

        /// <summary>
        /// Builds a polygon in the shape of a circle
        /// </summary>
        public static Polygon CreateCircle(Vector3 center, float radius, int steps)
        {
            if (steps < 3) steps = 3;

            Polygon poly = new Polygon();
            float step = -1f;

            poly.Vertices = new Vector3[steps];

            for (int i = 0; i < steps; i++) {
                float r = (step * i) / (1f * steps) * 2f * Mathf.PI;
                float x = Mathf.Cos(r) * radius;
                float y = Mathf.Sin(r) * radius;
                poly.Vertices[i].x = center.x + x;
                poly.Vertices[i].y = center.y + y;
                poly.Vertices[i].z = center.z;
            }
            poly.Close();

            return poly;
        }
    }

}//AxonGenesis
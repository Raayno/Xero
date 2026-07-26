// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;

namespace AxonGenesis
{
    /// <summary>
    /// EdgeRect defines a special rect type that has the values l, r, t, b (left, right, top, bottom),
    /// which is better suited for some processes than the standard x, y, w, h rect model.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "EdgeRect")]
    public struct EdgeRect
    {
        public float l;
        public float r;
        public float t;
        public float b;

        public EdgeRect(float _l, float _r, float _t, float _b)
        {
            l = _l;
            r = _r;
            t = _t;
            b = _b;
        }

        public override bool Equals(object obj)
        {
            if (obj is EdgeRect) {
                var other = (EdgeRect)obj;
                return l == other.l && r == other.r && t == other.t && b == other.b;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // Combine hash codes of individual fields for a unique hash
            return l.GetHashCode() ^ r.GetHashCode() ^ t.GetHashCode() ^ b.GetHashCode();
        }

        public static bool operator ==(EdgeRect lhs, EdgeRect rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(EdgeRect lhs, EdgeRect rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

}//AxonGenesis
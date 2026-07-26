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
    /// A global utility for comparing and manipulating Rects
    /// </summary>
    public static class RectUtil
    {

        /// <summary>
        /// Returns true if the rects overlap each other at all.
        /// </summary>
        public static bool Overlaps(Rect a, Rect b)
        {
            bool overlaps = (a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y);
            return overlaps;
        }

        /// <summary>
        /// Scale a rect by a certain amount.
        /// </summary>
        public static Rect Scale(Rect r, float scale)
        {
            r.x = r.x * scale;
            r.y = r.y * scale;
            r.width = r.width * scale;
            r.height = r.height * scale;
            return r;
        }

        /// <summary>
        /// Move the position of a rect by the specified offset.
        /// </summary>
        public static Rect Offset(Rect r, Vector2 offset)
        {
            r.x += offset.x;
            r.y += offset.y;
            return r;
        }

        /// <summary>
        /// Returns the offset of the position relative to the rect x,y position
        /// </summary>
        public static Vector2 GetOffset(Rect r, Vector2 pos)
        {
            return pos - new Vector2(r.x, r.y);
        }

        /// <summary>
        /// Returns the offset of the position relative to the rect x,y position
        /// </summary>
        public static Vector2 GetOffset(Rect r, Vector2 pos, bool inverted)
        {
            if (inverted) {
                return new Vector2(r.xMax, r.yMax) - pos;
            }
            return pos - new Vector2(r.x, r.y);
        }

        /// <summary>
        /// Returns true if the rect contains the given point. Optionally add a pad value to effectively
        /// expand the rect area equally in all directions.
        /// </summary>
        public static bool Contains(Rect rect, Vector2 pos, float pad)
        {
            float pad2 = pad * 2;
            Rect r = new Rect(rect.x - pad, rect.y - pad, rect.width + pad2, rect.height + pad2);
            return r.Contains(pos);
        }

        /// <summary>
        /// Ensures the rect has at least 1 width and height, and will correct the rect if it is inverted
        /// (ie. negative width or height)
        /// </summary>
        public static void Correct(ref Rect rect)
        {
            if (rect.width == 0) rect.width = 1f;
            else
            if (rect.width < 0) {
                float t = rect.x;
                rect.x = rect.width + rect.x;
                rect.width = t - rect.x;
            }
            if (rect.height == 0) rect.height = 1f;
            else
            if (rect.height < 0) {
                float t = rect.y;
                rect.y = rect.height + rect.y;
                rect.height = t - rect.y;
            }
        }

    }

}//AxonGenesis
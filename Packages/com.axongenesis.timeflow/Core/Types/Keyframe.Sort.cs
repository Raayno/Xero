// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;

namespace AxonGenesis
{

    public static class KeyframeSort
    {
        private static _SortKeyframesTimeAsc byTimeAsc = null;
        public static _SortKeyframesTimeAsc ByTimeAsc {
            get {
                if (byTimeAsc == null) {
                    byTimeAsc = new _SortKeyframesTimeAsc();
                }
                return byTimeAsc;
            }
        }

        private static _SortKeyframesTimeDesc byTimeDesc = null;
        public static _SortKeyframesTimeDesc ByTimeDesc {
            get {
                if (byTimeDesc == null) {
                    byTimeDesc = new _SortKeyframesTimeDesc();
                }
                return byTimeDesc;
            }
        }

        private static _SortKeyframesSizeAsc bySizeAsc = null;
        public static _SortKeyframesSizeAsc BySizeAsc {
            get {
                if (bySizeAsc == null) {
                    bySizeAsc = new _SortKeyframesSizeAsc();
                }
                return bySizeAsc;
            }
        }
        private static _SortKeyframesSizeDesc bySizeDesc = null;
        public static _SortKeyframesSizeDesc BySizeDesc {
            get {
                if (bySizeDesc == null) {
                    bySizeDesc = new _SortKeyframesSizeDesc();
                }
                return bySizeDesc;
            }
        }
    }

    public class _SortKeyframesTimeAsc : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int c = 0;

            if (a.KeyTime < b.KeyTime) {
                c = -1;
            }
            else
            if (a.KeyTime > b.KeyTime) {
                c = 1;
            }

            return c;
        }
    }

    public class _SortKeyframesTimeDesc : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int c = 0;

            if (a.KeyTime > b.KeyTime) {
                c = -1;
            }
            else
            if (a.KeyTime < b.KeyTime) {
                c = 1;
            }

            return c;
        }
    }

    public class _SortKeyframesSizeAsc : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int c = 0;
#if UNITY_EDITOR

            if (a.GUIRect.width < b.GUIRect.width) {
                c = -1;
            }
            else
            if (a.GUIRect.width > b.GUIRect.width) {
                c = 1;
            }
#endif
            return c;
        }
    }

    public class _SortKeyframesSizeDesc : IComparer<Keyframe>
    {
        public int Compare(Keyframe a, Keyframe b)
        {
            int c = 0;
#if UNITY_EDITOR

            if (a.GUIRect.width > b.GUIRect.width) {
                c = -1;
            }
            else
            if (a.GUIRect.width < b.GUIRect.width) {
                c = 1;
            }
#endif

            return c;
        }
    }


}
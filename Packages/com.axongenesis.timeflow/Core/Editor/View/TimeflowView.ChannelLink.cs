// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        private const int _channelLinkViewRightPad = 100;

        #region STATIC

        // Handled statically so that linking states affect all timeflow instances
        public static bool ShowAllLinks;
        public static bool IsLinking;
        public static bool IsLinkValid;
        public static TimeflowChannel LinkReceiver;
        public static TimeflowChannel LastHit;
        private static Vector3[] curve;
        private static float curveLastTime;
        private static float curveMarchTime;

        private static void StartLinking(TimeflowChannel receiver)
        {
            LinkReceiver = receiver;
            IsLinking = LinkReceiver != null;
        }

        private static void SelectChannelLink(TimeflowChannel channel)
        {
            if (channel != LinkReceiver && LinkReceiver.IsLinkable(channel)) {
                Undo.IncrementCurrentGroup();
                int grp = Undo.GetCurrentGroup();
                UndoUtil.Undo(channel.Behavior, "Add Channel Link", true);
                UndoUtil.Undo(LinkReceiver.Behavior, "Add Channel Link", true);
                LinkReceiver.RemoveLink(); // First remove existing link if any
                LinkReceiver.Link = new TimeflowChannelLink(LinkReceiver, channel);

                Undo.CollapseUndoOperations(grp);
            }
            StopLinking();
        }

        public static void StopLinking()
        {
            LinkReceiver = null;
            IsLinking = false;
        }

        private static void DrawCurve(Vector3 start, Vector3 end, Color color, float alpha = 1f)
        {
            if (start == Vector3.zero || end == Vector3.zero) return;

            int half = 16;
            int count = 33;
            if (curve == null) curve = new Vector3[count];

            color.a *= alpha;

            if (curveMarchTime < 0f || curveMarchTime > 1f) curveMarchTime = 0f;
            int curveMarchIndex = Mathf.RoundToInt((float)count * (1f - curveMarchTime));
            if (curveMarchIndex > count) curveMarchIndex = 0;
            int curveMarchEnd = curveMarchIndex + 6;
            if (curveMarchEnd > count - 7) {
                curveMarchIndex = 0;
                curveMarchEnd = 6;
            }

            Vector3 mid = MathUtil.Average(start, end);
            float factor = MathUtil.GetInterpolation(20f, 200f, MathUtil.Distance(start, end));
            float offset = factor * 20f;
            //mid.x += offset;
            Vector2 node = Vector2.zero;

            for (int i = 0; i < half; i++) {
                float interp = (float)i / (float)half;
                Vector3 a = MathUtil.Interpolate(start, mid, interp);
                a.x += MathUtil.EaseOutCircle(0f, offset, interp);
                if (i >= curveMarchIndex && i <= curveMarchEnd) {
                    node = a;
                }
                curve[i] = a;
            }
            for (int i = half; i < count; i++) {
                float interp = (float)(i - half) / (float)half;
                Vector3 a = MathUtil.Interpolate(mid, end, interp);
                a.x += MathUtil.EaseInCircle(offset, 0f, interp);
                if (i >= curveMarchIndex && i <= curveMarchEnd) {
                    node = a;
                }

                curve[i] = a;
            }

            curveMarchTime += Time.time - curveLastTime;
            curveLastTime = Time.time;

            Handles.color = color;
            Handles.DrawAAPolyLine(4f, curve);

            Vector2 al = node;
            al.x -= 2f;
            Vector2 bl = node;
            bl.x += 2f;
            Handles.DrawLine(al, bl);
        }

        #endregion

        #region CHANNEL LINK

        public void GUIChannelLink()
        {
            GUIRect channelLinkArea = Layout.Hierarchy;
            channelLinkArea.width += _channelLinkViewRightPad;

            GUILayout.BeginArea(channelLinkArea);
            if (IsLinking) {
                if (LinkReceiver == null) {
                    StopLinking();
                }
                else {
                    DrawCurve(LinkReceiver.GUILinkRect.center, MousePosition,
                        IsLinkValid ? AxonColor.ChannelLinkValid : AxonColor.ChannelLinkInvalid);
                }
            }
            else {
                if (Display.Objects != null) {
                    Vector3[] line = new Vector3[2];
                    bool showLinks = IsControl || IsAlt || ShowAllLinks;
                    foreach (TimeflowObject obj in Display.Objects) {
                        if (obj.IsDisplayed && obj.AllChannelsForDisplay != null && !obj.IsCollapsed) {
                            foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                                if (!ch.IsHidden && ch != obj.Track && ch.CanLink && !ch.Object.IsCollapsed) {
                                    if (ch.IsLinked && ch.Link.Channel != null && (showLinks || ch.IsSelected || ch.Link.Channel.IsSelected) && ch.GUILinkRect.center != Vector2.zero) {
                                        Vector3 dst = ch.Link.Channel.GUILinkRect.center;
                                        Vector3 src = ch.GUILinkRect.center;
                                        if (!ch.Link.Channel.IsDisplayed || !ch.Link.Channel.Object.IsDisplayed || dst == Vector3.zero) {
                                            dst = src;
                                            dst.x += 20f;
                                        }
                                        float alpha = 1f;
                                        if (LastHit != null) {
                                            alpha = LastHit == ch ? 1f : 0.25f;
                                        }
                                        DrawCurve(src, dst, ch.Link.GUIColor, alpha);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        public TimeflowChannel ChannelLinkHit(bool apply)
        {
            IsLinkValid = false;

            LastHit = null;
            if (Display.Objects != null) {
                Vector2 p = Input.GetMousePosition(Layout.Hierarchy);
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && obj.AllChannels != null) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && ch != obj.Track && ch.GUILinkRect.Contains(p) && ch.CanLink) {
                                bool linkable = ch.CanLink;
                                if (IsLinking) {
                                    if (LinkReceiver != null) {
                                        linkable = LinkReceiver.IsLinkable(ch);
                                    }
                                }

                                if (linkable) {
                                    if (IsLinking) {
                                        if (LinkReceiver == null || IsControl) {
                                            StopLinking();
                                        }
                                        else
                                        if (ch != null && LinkReceiver.IsLinkable(ch)) {
                                            IsLinkValid = true;
                                            LastHit = ch;
                                        }
                                    }
                                    else
                                    if (!IsControl) {
                                        IsLinkValid = true;
                                        LastHit = ch;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (apply) {
                if (IsLinking) {
                    if (LastHit != null) {
                        SelectChannelLink(LastHit);
                    }
                    else {
                        StopLinking();
                    }
                }
                else {
                    if (LastHit != null) {
                        StartLinking(LastHit);
                    }
                }
            }

            ShowAllLinks = LastHit != null;

            return LastHit;
        }

        #endregion

    }

}//AxonGenesis

#endif

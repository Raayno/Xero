// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        private const int _gridSpacingMin = 25;
        private const int _gridTimeLabelTopPad = -1;
        private const int _gridTimeLabelWidth = 60;
        private const int _gridTimeLabelHeight = 20;
        private const int _gridLabelOverlapWidth = 20;
        private const int _gridLabelOverlapTimecodeWidth = 60;

        [SerializeField]
        public bool GridEnabled = true;

        [SerializeField, FormerlySerializedAs("GridTimeDisplay")]
        private TimeDisplayModes _GridTimeDisplay = TimeDisplayModes.Seconds;

        public TimeDisplayModes GridTimeDisplay {
            get {
                if (TimeflowPreferences.Current.UnifiedTimeDisplay) return TimeDisplay;
                return _GridTimeDisplay;
            }
            set {
                _GridTimeDisplay = value;
                if (TimeflowPreferences.Current.UnifiedTimeDisplay) TimeDisplay = value;
            }
        }

        public void GUIGrid()
        {
            if (GridEnabled) {
                int leftEdge = Layout.SeparatorH3.Left + Layout.SeparatorH3.Width;
                int rightEdge = WindowWidth - Layout.VScrollbar.Width - 20;

                float leftEdgeTime = TimeOfPosition(ScrollOffset.x, true, false);

                int xMin = PositionOfTime(leftEdgeTime, true);
                int xMax = rightEdge;
                int yMin = Layout.TimeAreaOuter.Top + 12;
                int yMax = WindowHeight - Layout.Footer.Height;

                if (!TimeflowPreferences.Current.DisplayScrollbarOnTop) {
                    yMax -= Layout.ScrollbarMain.Height;
                }

                //GUI.BeginClip(Layout.TimeAreaInner);

                Vector2 a = new Vector2(xMin, yMin);
                Vector2 b = new Vector2(xMax, yMax);
                GUIRect timeLabelRect = new GUIRect(xMin, Layout.TimeAreaOuter.Top + _gridTimeLabelTopPad, _gridTimeLabelWidth, _gridTimeLabelHeight);

                float viewRange = rightEdge - leftEdge;
                int maxGridLines = Mathf.Abs(Mathf.FloorToInt((float)viewRange / (float)_gridSpacingMin));
                if (maxGridLines == 0) return;

                float startTime = TimeOfPosition(leftEdge, false, false);
                float endTime = TimeOfPosition(rightEdge, false, false);

                if (Timeflow.IsTimeScopeEnabled) {
                    startTime -= Timeflow.TimeScopeStart;
                    endTime -= Timeflow.TimeScopeEnd;
                }

                float timeRange = endTime - startTime;
                float snapCount = timeRange / Snap; // how many snap points (ie grid lines) fit in the time range

                float snapMultiplier = 1f;
                float targetCount = snapCount;
                while (targetCount > maxGridLines) {
                    snapMultiplier *= 2f;
                    targetCount *= 0.5f;
                }

                int subdivisions;
                if (UseMusicalTiming && GridTimeDisplay == TimeDisplayModes.Measures) {
                    subdivisions = Timeflow.BeatNoteSize;
                }
                else {
                    subdivisions = 4;
                }
                float snapInc = Snap * snapMultiplier;
                _snapDisplayed = snapInc;

                int subdivisionInt = Mathf.RoundToInt(subdivisions);
                int midpoint = Mathf.FloorToInt(subdivisions / 2);
                float startTimeSnapped = SnapTime(startTime, true);
                int labelWidthOverlap = _gridLabelOverlapWidth;
                if (GridTimeDisplay == TimeDisplayModes.Timecode) {
                    labelWidthOverlap = _gridLabelOverlapWidth;
                }

                if (subdivisions < 1) subdivisions = 1;

                float minorInc = snapInc / (float)subdivisions;
                float lineTimeLast = 0;
                int lineCount = -1;
                bool first = true;
                int timePosition = xMin;
                string lastLabel = null;

                Handles.BeginGUI();
                while (timePosition < xMax) {
                    float lineTime = SnapTime(startTimeSnapped + (snapInc * lineCount), true, false);
                    timePosition = PositionOfTime(lineTime, false, true);
                    a.x = b.x = timePosition;

                    if (timePosition > leftEdge && timePosition <= rightEdge) {
                        Handles.color = AxonColor.GridLineMajor;
                        Handles.DrawLine(a, b);

                        float distanceFromLastLabel = timePosition - (timeLabelRect.x + timeLabelRect.width);

                        /// To avoid clutter, time numbers overlapping marker labels are skipped
                        if (first || distanceFromLastLabel > labelWidthOverlap) {
                            timeLabelRect.x = timePosition;
                            string label = GUIGridGetLabel(lineTime);
                            if (label != lastLabel) { // prevent repeating the same label
                                timeLabelRect.width = (int)AxonGUI.CalculateWidth(label);
                                timeLabelRect.x -= timeLabelRect.width / 2;

                                if (timeLabelRect.x < Layout.H3) {
                                    timeLabelRect.x = Layout.H3;
                                }

                                //if (first) Debug.Log($"timeLabelRect:{timeLabelRect}");
                                //GUI.color = IsOverlappingMarker(timeLabelRect) ? AxonColor.DimField : AxonColor.Default;
                                GUI.Box(timeLabelRect, label, AxonUI.TimeRulerLabelStyle);
                                lastLabel = label;
                            }
                        }
                        if (first) first = false;
                    }
                    float minorTime = lineTimeLast + minorInc;

                    if (TimeflowPreferences.Current.DrawMinorGridLines) {
                        // Draw minor grid lines
                        _snapDisplayed = snapInc / (float)subdivisions;
                        for (int i = 1; i < subdivisionInt; i++) {
                            int minorTimePosition = PositionOfTime(SnapTime(minorTime, true, false), false, true);
                            if (minorTimePosition >= leftEdge && minorTimePosition <= rightEdge) {
                                Handles.color = i == midpoint ? AxonColor.GridLineMinor : AxonColor.GridLineSubMinor;
                                a.x = b.x = minorTimePosition;
                                Handles.DrawLine(a, b);
                            }
                            minorTime += minorInc;
                        }
                    }

                    lineTimeLast = lineTime;
                    lineCount++;

                    if (lineCount > 1000) {
                        //Debug.LogError("Grid line count exceeded 1000");
                        break;
                    }
                }
                Handles.EndGUI();
                //GUI.EndClip();
            }
        }

        private bool IsOverlappingMarker(GUIRect timeLabelRect)
        {
            bool isOverlapping = false;
            if (Timeflow.ShowMarkers && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                    if (marker.WorldRect.Overlaps(timeLabelRect)) {
                        isOverlapping = true;
                        break;
                    }
                }
            }
            return isOverlapping;
        }

        private string GUIGridGetLabel(float time)
        {
            string label = "";
            if (GridTimeDisplay == TimeDisplayModes.Seconds) {
                label = "" + (Mathf.Round(time * 100f) / 100f);
            }
            else
            if (GridTimeDisplay == TimeDisplayModes.Frames) {
                label = "" + Mathf.Round(time * Timeflow.FPS);
            }
            else
            if (GridTimeDisplay == TimeDisplayModes.Timecode) {
                label = StringUtil.SecondsToTimecode(time, true, UseFractionalTime, Timeflow.FPS);
            }
            else
            if (GridTimeDisplay == TimeDisplayModes.Measures) {
                label = StringUtil.SecondsToMeasures(time, Timeflow.BPM, Timeflow.BeatsPerBar, Timeflow.BeatNoteSize);
            }

            return label;
        }
    }

}//AxonGenesis

#endif

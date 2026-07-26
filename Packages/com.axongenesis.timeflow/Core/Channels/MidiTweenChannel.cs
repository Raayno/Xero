// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This class is based on data channel in order to make use of its interpolation methods, however this
    /// is not strictly a data-only channel so overrides certain attributes.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MidiTweenChannel")]
    sealed public class MidiTweenChannel : DataChannel
    {
        private const int _maxMidiNoteDraw = 5000;

        public enum NoteModes
        {
            Single,
            Range,
            Any
        }

        public NoteModes NoteMode = NoteModes.Single;
        public int NoteMin;
        public int NoteMax = 256;
        public float MinVelocity;

        public GameObject TargetObject;

        [NonSerialized]
        public MidiTween MidiTweenParent;

        [NonSerialized]
        public bool IsPlayingNote = false;

        [NonSerialized]
        public int LastNotePlayed = 0;

        [NonSerialized]
        public bool WasPlayingNote = false;

        public MidiTweenChannel(MidiTween parent, MidiTweenChannel copy) : base(parent)
        {
            MidiTweenParent = parent;
            Behavior = parent;
            if (copy != null) {
                NoteMode = copy.NoteMode;
                NoteMin = copy.NoteMin;
                NoteMax = copy.NoteMax;
                MinVelocity = copy.MinVelocity;
                ToProperty = new Property(parent, copy.ToProperty);
                ToProperty.SwitchGameObject(parent.gameObject);
            }
            else {
                ToProperty = new Property();
            }
            PerformSetup(parent);

#if UNITY_EDITOR
            GUICanDraw = copy.GUICanDraw;
#endif
        }

        public MidiTweenChannel(MidiTween parent) : base(parent)
        {
            MidiTweenParent = parent;
            DataParent = parent;

#if UNITY_EDITOR
            GUICanDraw = false;
#endif
        }

        public override void OnSetup(TimeflowBehavior parent)
        {
            if (ToProperty == null) ToProperty = new Property();
            PerformSetup(parent);
            // skip the base behavior that forces data only
        }

        public override void SetupKeyframes()
        {
            base.SetupKeyframes();
            if (ToProperty != null && ToProperty.Comp != null) {
                TargetObject = ToProperty.Comp.gameObject;
            }
        }

#if UNITY_EDITOR

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUIHierarchyControls()
        {
            if (IsHidden || !IsSelectable) return;
            GUI.color = AxonColor.Default;
            GUIChannelLink();
            GUIExpandRegion();
        }

        public override void GUIKeyframes()
        {
            if (!GUICanDraw && !IsGraphLocked) {
            }
            else
            if (MidiTweenParent != null && Timeflow.Active != null && MidiTweenParent.Track != null && MidiTweenParent.Midi != null) {

                float startTime = Timeflow.Active.View.ViewStartTime;
                float endTime = Timeflow.Active.View.ViewEndTime;
                float viewTotalTime = endTime - startTime;
                if (viewTotalTime <= 0) return;

                List <MidiNote> notes = MidiTweenParent.Track.Notes;
                if (notes != null) {
                    GUI.color = GUIColor;
                    if (!IsSelected && Timeflow.View.IsGraphSolo) GUI.color = ColorUtil.SetAlpha(GUI.color, 0.5f);

                    float startPos = ((0f - startTime) / viewTotalTime) * GUIRect.width;
                    float endPos = Timeflow.Active.View.PositionOfTime(endTime, true);
                    float width = endPos - startPos;
                    float height = GUIRect.height - 4f;

                    int x = 0;
                    float y = GUIRect.y + 2f;
                    int count = notes.Count;

                    float offset = TimeOffsetWorld + MidiTweenParent.Midi.StartTime;

                    ///TODO: Show looped midi notes if loop is enabled and time range crosses loop point.
                    ///MidiTweenParent.Midi.EnableLoop

                    foreach (MidiNote n in notes) {
                        float noteStart = n.StartTime + offset;
                        float noteEnd = n.EndTime + offset;

                        if (noteStart < startTime) {
                        }
                        else
                        if (noteEnd > endTime) {
                            /// Remaining notes are outside of display
                            break;
                        }
                        else
                        if (n.Note >= NoteMin && n.Note <= NoteMax && MathUtil.Overlaps(startTime, endTime, noteStart, noteEnd)) {
                            float a = Timeflow.Active.View.PositionOfTime(noteStart, true);
                            float b = Timeflow.Active.View.PositionOfTime(noteEnd, true);
                            Rect r = new Rect(a, y, b - a, height);
                            GUI.Box(r, "", AxonUI.MidiNoteStyle);
                            if (x > _maxMidiNoteDraw) break; // limit to prevent massive UI drawing delays
                            x++;
                        }
                    }
                    GUI.color = AxonColor.Default;
                }

            }
        }

        public override void GUIGraphPass2()
        {
            GUIKeyframes();
        }

#endif

    }

}//AxonGenesis
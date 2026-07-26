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
    /// Stores data for a specific track in a MidiFile.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MidiTrack")]
    public class MidiTrack : SerializableObject
    {
        public MidiFile Parent;
        public ulong ChunkSize;
        public ushort Type;
        public byte[] Data;
        public ushort Number;
        public string Name = "Track";
        public string Instrument = "";

        public List<MidiEvent> Events;
        public List<MidiNote> Notes;
        public int MaxNotes = 256;

#if UNITY_EDITOR
        public bool EditorShow;
        public GUIRect GUIRect = new GUIRect(0, 0, 0, 0);
#endif

        public MidiTrack(MidiFile parent)
        {
            Parent = parent;
            Events = new List<MidiEvent>();
        }

        public MidiEvent AddEvent()
        {
            MidiEvent evt = new MidiEvent();
            AddEvent(evt);
            return evt;
        }

        public void AddEvent(MidiEvent evt)
        {
            if (Events == null) {
                Events = new List<MidiEvent>();
            }
            Events.Add(evt);
        }

        public MidiEvent LastEvent()
        {
            MidiEvent e = null;
            if (Events != null && Events.Count > 0) {
                e = Events[Events.Count - 1];
            }
            return e;
        }

        public int TotalNotes {
            get {
                if (Notes == null) return 0;
                else return Notes.Count;
            }
        }

        public float Duration {
            get {
                if (Notes == null || Notes.Count == 0) return 0;
                else return Notes[Notes.Count - 1].EndTime;
            }
        }

        public Vector2 NotesRange()
        {
            Vector2 range = new Vector2(0, 127f);
            if (Notes != null) {
                range.x = 127f;
                range.y = 0f;
                foreach (MidiNote n in Notes) {
                    if (n.Note < range.x) range.x = n.Note;
                    if (n.Note > range.y) range.y = n.Note;
                }
                if (range.x > range.y) {
                    float t = range.x;
                    range.x = range.y;
                    range.y = t;
                }
            }
            return range;
        }

        public List<int> NotesList()
        {
            List<int> notes = new List<int>();
            if (Notes != null) {
                foreach (MidiNote n in Notes) {
                    if (!notes.Contains(n.Note)) notes.Add(n.Note);
                }
                notes.Sort();
            }
            return notes;
        }

        public bool IsNoteOnAtTime(float time, int noteMin, int noteMax, float maxDuration)
        {
            bool noteOn = false;
            if (Notes != null && Notes.Count > 0) {
                foreach (MidiNote n in Notes) {
                    float end = n.EndTime - n.StartTime;
                    if (end > maxDuration) end = maxDuration;
                    end += n.StartTime;
                    if (n.StartTime <= time && end > time && n.Note >= noteMin && n.Note <= noteMax) {
                        noteOn = true;
                        break;
                    }
                }
            }
            return noteOn;
        }

        /// <summary>
        /// Returns all notes overlapping the specified time. The release is the additional time in seconds
        /// to fade the note out.
        /// </summary>
        public List<MidiNote> NotesAtTime(float time, float release, int fromNote, int toNote)
        {
            List<MidiNote> notes = null;
            if (Notes != null && Notes.Count > 0) {
                // widen search time so notes between frames don't get skipped
                release = Mathf.Max(release, Parent.Timeflow.FrameDuration);
                foreach (MidiNote n in Notes) {
                    if (n.Note >= fromNote && n.Note <= toNote && n.StartTime <= time && (n.EndTime + release) > time) {
                        if (notes == null) notes = new List<MidiNote>();
                        notes.Add(n);
                    }
                }
            }
            return notes;
        }

        /// <summary>
        /// Returns one note nearest occuring on or before the specified time. The release is the
        /// additional time in seconds to fade the note out.
        /// </summary>
        public MidiNote NoteAtTime(float time, float release, int fromNote, int toNote)
        {
            MidiNote note = null;
            if (Notes != null && Notes.Count > 0) {
                // widen search time so notes between frames don't get skipped
                release = Mathf.Max(release, Parent.Timeflow.FrameDuration);
                foreach (MidiNote n in Notes) {
                    if (n.Note >= fromNote && n.Note <= toNote && n.StartTime <= time && (n.EndTime + release) > time) {
                        if (note == null) {
                            note = n;
                        }
                        else
                        if (n.StartTime > note.StartTime) {
                            note = n;
                        }
                    }
                }
            }
            return note;
        }

        /// <summary>
        /// Returns all notes overlapping the specified time. The release is the additional time in seconds
        /// to fade the note out.
        /// </summary>
        public List<MidiNote> NotesAtTime(float time, float release, List<int> onlyNotes)
        {
            List<MidiNote> notes = null;
            if (Notes != null && Notes.Count > 0) {
                // widen search time so notes between frames don't get skipped
                release = Mathf.Max(release, Parent.Timeflow.FrameDuration);
                foreach (MidiNote n in Notes) {
                    if (onlyNotes.Contains(n.Note) && n.StartTime <= time && (n.EndTime + release) > time) {
                        if (notes == null) notes = new List<MidiNote>();
                        notes.Add(n);
                    }
                }
            }
            return notes;
        }

        /// <summary>
        /// Returns one note nearest occuring on or before the specified time. The release is the
        /// additional time in seconds to fade the note out.
        /// </summary>
        public MidiNote NoteAtTime(float time, float release, List<int> onlyNotes)
        {
            MidiNote note = null;
            if (Notes != null && Notes.Count > 0) {
                release = Mathf.Max(release, Parent.Timeflow.FrameDuration);
                foreach (MidiNote n in Notes) {
                    if (onlyNotes.Contains(n.Note) && n.StartTime <= time && (n.EndTime + release) > time) {
                        if (note == null) {
                            note = n;
                        }
                        else
                        if (n.StartTime > note.StartTime) {
                            note = n;
                        }
                    }
                }
            }
            return note;
        }

        public void SetupNotes()
        {
            if (Events != null && Events.Count > 0) {
                Notes = new List<MidiNote>();
                if (MaxNotes > 0) Notes.Capacity = MaxNotes;

                List<MidiNote> openNotes = new List<MidiNote>();

                MidiNote note = null;

                float startTime = 0f;
                float endTime = 0f;
                float velocity = 0f;
                int noteValue = 0;

                int noteCount = 0;

                int i = 0;
                int noteIndex = 0;

                foreach (MidiEvent e in Events) {
                    if (e.Type == MidiEvent.EventTypes.NoteOn) {
                        startTime = e.Time;
                        noteValue = e.Param1;
                        velocity = (float)e.Param2 / 127f;

                        // See if a note matching this is already in the list
                        note = null;
                        foreach (MidiNote n in openNotes) {
                            if (n.Note == noteValue) {
                                note = n;
                                break;
                            }
                        }
                        if (note == null) {
                            note = new MidiNote(noteIndex, noteValue, startTime, velocity);
                            openNotes.Add(note);
                            noteIndex++;
                        }
                        else {
                            note.SetEndTime(startTime);
                            openNotes.Remove(note);
                            Notes.Add(note);
                            noteCount++;
                        }
                    }
                    else
                    if (e.Type == MidiEvent.EventTypes.NoteOff) {
                        endTime = e.Time;
                        noteValue = e.Param1;

                        // Find the corresponding note
                        note = null;
                        foreach (MidiNote n in openNotes) {
                            if (n.Note == noteValue) {
                                note = n;
                                break;
                            }
                        }

                        if (note == null) {
                            Debug.LogWarning("NoteOff could not be matched to a NoteOn event:" + i + " note:" + noteValue + " time:" + endTime);
                            velocity = (float)e.Param2 / 127f;
                            note = new MidiNote(i, noteValue, 0f, velocity);
                            note.SetEndTime(endTime);
                            openNotes.Add(note);
                        }
                        else {
                            note.SetEndTime(endTime);
                            openNotes.Remove(note);
                            Notes.Add(note);
                            noteCount++;
                        }
                    }
                    i++;
                }
            }
            else {
                Notes = null;
            }
        }

        public void PrepareNotesToPlay()
        {
            foreach (MidiNote n in Notes) {
                n.MessageSent = false;
            }
        }
    }

}//AxonGenesis
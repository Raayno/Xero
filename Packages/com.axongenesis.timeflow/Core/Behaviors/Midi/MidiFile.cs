// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This reads binary midi files exported from any DAW. In order to work with Unity, the file must be
    /// renamed with the extension .bytes otherwise .midi or .mid files will not be recognized. This is a
    /// simplified implementation of midi only to extract note information for timing to animation. It
    /// detects the BPM and the notes in each of the tracks present. Once the midi file has been read, this
    /// class stores the data locally for fast interpolation during playback. 
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/MIDI File")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-file")]
    sealed public class MidiFile : TimeflowBehavior
    {
        public static MidiFile Instance { get; private set; }

        #region PUBLIC

        public TextAsset File;

        public ulong ChunkSize;         // dword
        public ushort Format;           // word
        public ushort TrackCount;

        public int TPQN = 15360;            // Ticks Per Quarter Note
        public int MPQN = 500000;           // Microseconds Per Quarter Note
        public float BPM = 120f;            // Beats Per Minute
        public float TargetBPM = 120f;      // Beats Per Minute
        public bool MatchSceneBPM = true;

        public double SecondsPerTick;   // seconds = SecondsPerTick * Ticks
        public int AbsoluteTime;
        public int TimeSignature;
        public float StartTime;

        [FormerlySerializedAs("TimeScale")]
        public float MidiTimeScale = 1f;        // Values other than 1 can be used to scale time to a different bpm
        public bool EnableLoop = true;

        public float MinAttack;
        public float MinRelease;
        public float Intensity = 1f;

        public float Duration;

        public bool EditorShowTiming = true;
        public bool EditorShowNotes = true;
        public bool EditorShowTracks = true;
        public MidiTrack[] Tracks;

        public bool IsLoaded;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Stream stream;

        [NonSerialized]
        private MidiReader reader;

        [NonSerialized]
        private byte runningCommand;

        [NonSerialized]
        private int runningChannel;

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();

            Instance = this;

#if UNITY_EDITOR
            /// This is a hack to clear a progress bar if it gets stuck onscreen due to an error that
            /// interrupts the script. This can happen if the midi file has unknown data or is the
            /// incorrect format. If the progress bar gets stuck on screen, it can be cleared by making any
            /// minor script change to force Unity to recompile scripts, which it does automatically and
            /// the progress bar is cleared when this scripts wakes up again.
            EditorUtility.ClearProgressBar();
#endif
        }

        protected override void OnDestruct()
        {
            base.OnDestruct();
            Instance = null;
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        public float TicksToSeconds(int ticks)
        {
            return (float)((double)ticks * SecondsPerTick);
        }

        public float LoopTime(float time)
        {
            time *= MidiTimeScale;
            if (EnableLoop) {
                if (Duration > 0) {
                    while (time > Duration) {
                        time -= Duration;
                    }
                }
                if (time < 0f) time = 0f;

            }
            time -= StartTime;
            return time;
        }

        public bool IsNoteOnAtTime(float time, int track, int noteMin, int noteMax, float maxDuration)
        {
            bool noteOn = false;

            time = LoopTime(time);
            if (time >= 0f && Tracks != null) {
                if (track == -1) {
                    for (int i = 0; i < Tracks.Length; i++) {
                        if (Tracks[i].IsNoteOnAtTime(time, noteMin, noteMax, maxDuration)) {
                            noteOn = true;
                            break;
                        }
                    }
                }
                else
                if (track < Tracks.Length) {
                    noteOn = Tracks[track].IsNoteOnAtTime(time, noteMin, noteMax, maxDuration);
                }
            }
            return noteOn;
        }

        public float GetDuration()
        {
            Duration = 0f;

            if (Tracks != null) {
                foreach (MidiTrack track in Tracks) {
                    if (track.Notes != null && track.Notes.Count > 0) {
                        foreach (MidiNote note in track.Notes) {
                            if (Duration < note.EndTime) {
                                Duration = note.EndTime;
                            }
                        }
                    }
                }
            }

            return Duration;
        }

        public void Parse()
        {
            bool proceed = true;

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();

            if (File == null) {
                Debug.LogWarning("Please assign a midi asset to import. Midi files must be in binary format and named with the file extension .bytes");
                return;
            }

            /// This is a quick preflight check to make sure the user is ready for a long wait if trying to
            /// import a large file.
            var fileInfo = new FileInfo(AssetDatabase.GetAssetPath(File));
            if (fileInfo != null) {
                if (fileInfo.Length > 100000) { // approximately 100k (which is about 10 times larger than normal)
                    if (!EditorUtility.DisplayDialog("Large MIDI File", "The midi file selected appears to be quite large (" + fileInfo.Length + " bytes). You may want to save your scene first and proceed with caution. It is advisable to clean up the file first to remove any extraneous data and split into separate files if needed.", "Continue Loading", "Cancel")) {
                        proceed = false;
                        Debug.LogWarning("Loading midi file aborted");
                        return;
                    }
                }
                //if (DebugEnabled) Debug.Log("Midi file size:" + fileInfo.Length);
            }
#endif
            if (proceed) {
                IsLoaded = true;
                AbsoluteTime = 0;
                Duration = 0f;

                //if (DebugEnabled) Debug.Log("MidiFile.Parse:" + File.name + " length:" + File.bytes.Length);
                using (stream = new MemoryStream(File.bytes)) {
                    using (reader = new MidiReader(stream)) {
                        string hr = Encoding.UTF8.GetString(reader.ReadBytes(4));
                        if (hr != "MThd") {
                            Debug.LogError("Midi.Parse: Invalid header format:" + hr);
                            IsLoaded = false;
                            return;
                        }

                        ChunkSize = reader.ReadUInt32();
                        Format = reader.ReadUInt16();
                        TrackCount = reader.ReadUInt16();
                        TPQN = (int)reader.ReadInt16();

                        if (TPQN <= 0) {
                            int fps = (TPQN >> 8) & 0x00ff;
                            int res = TPQN & 0x00ff;
                            switch (fps) {
                                case 232: fps = 24; break;
                                case 231: fps = 25; break;
                                case 227: fps = 29; break;
                                case 226: fps = 30; break;
                                default: fps = 255 - fps + 1; break;
                            }
                            TPQN = (short)(fps * res);
                            //if (DebugEnabled) Debug.Log("fps:" + fps + " res:" + res + " TimeDivision:" + TPQN);
                        }

                        //if (DebugEnabled) Debug.Log("ChunkSize:" + ChunkSize + " Format:" + Format + " TrackCount:" + TrackCount + " TimeDivision:" + TPQN);
                        UpdateBPM();

                        if (TrackCount <= 0) {
                            Debug.LogError($"Error parsing MIDI file. Invalid track count:{TrackCount}");
                            return;
                        }

                        Tracks = new MidiTrack[TrackCount];

                        for (int t = 0; t < TrackCount; t++) {
#if UNITY_EDITOR
                            if (EditorUtility.DisplayCancelableProgressBar("Reading Midi", "Processing Track " + t, (float)t / (float)TrackCount)) {
                                break;
                            }
#endif
                            AbsoluteTime = 0;
                            Tracks[t] = new MidiTrack(this);

                            hr = Encoding.UTF8.GetString(reader.ReadBytes(4));
                            if (hr != "MTrk") {
                                Debug.LogError("Midi.Parse: Invalid track format:" + hr);
                                IsLoaded = false;
                                return;
                            }

                            Tracks[t].ChunkSize = reader.ReadUInt32();
                            //if (DebugEnabled) Debug.Log("Track:" + t + " ChunkSize:" + Tracks[t].ChunkSize);
                            long p = reader.BaseStream.Position;

                            bool isDone = false;
                            while (!isDone) {
#if UNITY_EDITOR
                                if (EditorUtility.DisplayCancelableProgressBar("Reading Midi", "Processing Track " + t, (float)t / (float)TrackCount)) {
                                    break;
                                }
#endif
                                isDone = ParseNextEvent(Tracks[t]);
                            }

                            Tracks[t].SetupNotes();

                            // Check how many bytes were read when parsing the event and adjust if necessary
                            long r = (long)Tracks[t].ChunkSize - (reader.BaseStream.Position - p);
                            if (r > 0) {
                                //if (DebugEnabled) Debug.Log("Remaining bytes in chunk:" + r);
                                byte[] extra = reader.ReadBytes((int)r);
                                if (extra != null) {
                                    // To avoid compiler warnings that extra is not used :/
                                }
                            }
                        }
                    }
                }
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif

            GetDuration();
        }

        public void UpdateBPM()
        {
            BPM = Mathf.Round(6000000000f / (float)MPQN) / 100f; // Set rounding to .00 
            if (BPM <= 0) BPM = 1;

            float secondsPQN = ((float)MPQN / 1000000f);
            if (TPQN <= 0) TPQN = 1;
            SecondsPerTick = secondsPQN / (float)TPQN;

            if (Timeflow != null) {
                MidiTimeScale = (Timeflow.BPM / TargetBPM) * (TargetBPM / BPM);
            }
            if (Mathf.Abs(1f - MidiTimeScale) < 0.01f) MidiTimeScale = 1f; // Fix rounding errors

            //if (DebugEnabled) Debug.Log("TPQN:" + TPQN + " BPM:" + BPM + " Tempo:" + MPQN + " TimeScale:" + TimeScale);
        }

        public bool ParseNextEvent(MidiTrack track)
        {
            bool isDone = false;

            if (reader.IsEOF()) {
                return true;
            }
            MidiEvent thisEvent = track.AddEvent();

            byte b;
            thisEvent.DeltaTime = reader.ReadVariable();
            AbsoluteTime += thisEvent.DeltaTime;
            thisEvent.Time = TicksToSeconds(AbsoluteTime);
            //if (DebugEnabled) Debug.Log("ParseNextEvent:" + thisEvent.Time + " Delta:" + thisEvent.DeltaTime);

            thisEvent.Channel = 1;
            b = reader.ReadByte();

            if ((b & 0x80) == 0) {
                // Continuing running command
                if (runningCommand == 0x00) {
                    Debug.LogError("NO PRIOR COMMAND");
                    thisEvent.Type = MidiEvent.EventTypes.None;
                }
                thisEvent.Type = (MidiEvent.EventTypes)runningCommand;
                thisEvent.Channel = runningChannel;
                reader.BaseStream.Position--;
            }
            else {
                if ((b & 0xF0) == 0xF0) {
                    thisEvent.Type = (MidiEvent.EventTypes)b;
                }
                else {
                    thisEvent.Type = (MidiEvent.EventTypes)(b & 0xF0);
                    thisEvent.Channel = (b & 0x0F) + 1;
                }
                runningCommand = b;
                runningChannel = thisEvent.Channel;
            }

            if (thisEvent.Type == MidiEvent.EventTypes.None) {
                Debug.LogWarning("Unsupported Event:" + b.ToString("X2") + ":" + b);
            }
            //if (DebugEnabled) Debug.Log("Event:" + thisEvent.Type + " :" + thisEvent.Time + " channel:" + thisEvent.Channel);

            if (thisEvent.Type == MidiEvent.EventTypes.None ||
               thisEvent.Type == MidiEvent.EventTypes.Unknown1 ||
               thisEvent.Type == MidiEvent.EventTypes.NoteOn ||
               thisEvent.Type == MidiEvent.EventTypes.NoteOff ||
               thisEvent.Type == MidiEvent.EventTypes.NoteAftertouch ||
               thisEvent.Type == MidiEvent.EventTypes.Controller ||
               thisEvent.Type == MidiEvent.EventTypes.PitchBend
            ) {
                thisEvent.Param1 = reader.ReadByte();
                thisEvent.Param2 = reader.ReadByte();
            }
            else
            if (thisEvent.Type == MidiEvent.EventTypes.ProgramChange ||
                thisEvent.Type == MidiEvent.EventTypes.ChannelAfterTouch
            ) {
                thisEvent.Param1 = reader.ReadByte();
            }
            else
            if (thisEvent.Type == MidiEvent.EventTypes.MetaEvent) {
                thisEvent.Param1 = reader.ReadByte();
                thisEvent.Param2 = reader.ReadVariable();

                MidiEvent.MetaEventTypes meta = (MidiEvent.MetaEventTypes)thisEvent.Param1;
                //if (DebugEnabled) Debug.Log("MetaEvent:" + meta + ":" + thisEvent.Param2);

                if (meta == MidiEvent.MetaEventTypes.TrackNumber) {
                    track.Number = reader.ReadUInt16();
                    //if (DebugEnabled) Debug.Log("TrackNumber:" + track.Number);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.TextEvent) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("TextEvent:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.Copyright) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("Copyright:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.TrackName) {
                    track.Name = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("TrackName:" + track.Name);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.TrackInstrument) {
                    track.Instrument = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("TrackInstrument:" + track.Instrument + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.Lyrics) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("Lyrics:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.Marker) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("Marker:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.CuePoint) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("CuePoint:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.DeviceName) {
                    string text = Encoding.ASCII.GetString(reader.ReadBytes(thisEvent.Param2));
                    //if (DebugEnabled) Debug.Log("DeviceName:" + text + ":" + thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.MidiChannel) {
                    reader.ReadByte();
                }
                else
                if (meta == MidiEvent.MetaEventTypes.MidiPort) {
                    reader.ReadByte();
                }
                else
                if (meta == MidiEvent.MetaEventTypes.EndTrack) {
                    isDone = true;
                }
                else
                if (meta == MidiEvent.MetaEventTypes.SetTempo) {
                    MPQN = (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte();
                    UpdateBPM();
                }
                else
                if (meta == MidiEvent.MetaEventTypes.SmpteOffset) {
                    reader.ReadBytes(thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.TimeSignature) {
                    reader.ReadBytes(thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.KeySignature) {
                    reader.ReadBytes(thisEvent.Param2);
                }
                else
                if (meta == MidiEvent.MetaEventTypes.SequencerSpecific) {
                    //if (DebugEnabled) Debug.Log("SequencerSpecific: len:" + thisEvent.Param2);
                    reader.ReadBytes(thisEvent.Param2);
                }
                else {
                    reader.ReadBytes(thisEvent.Param2);
                }
            }
            else
            if (thisEvent.Type == MidiEvent.EventTypes.SysEx) {
                while (true) {
                    b = reader.ReadByte();
                    if (b == 0xF7) break;
                    if (reader.IsEOF()) {
                        isDone = true;
                        break;
                    }
                }
            }


            return isDone;
        }

#if UNITY_EDITOR

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContextMenuProperty info = new TimeflowContextMenuProperty();
            info.Obj = TimeflowContext.Obj;
            MidiFile file;
            if (!TimeflowContext.Obj.TryGetComponent<MidiFile>(out file)) {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Midi/File Source"), false, GUIMenu_Add, info);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Midi/File Source"), true, null);
            }
        }

        public static void GUIMenu_Add(object info)
        {
            TimeflowContextMenuProperty prop = (TimeflowContextMenuProperty)info;
            if (prop != null) {
                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        obj.BehaviorsEnabled = true;

                        MidiFile comp = Undo.AddComponent<MidiFile>(obj.gameObject);
                        if (comp != null) {
                            comp.SetupChannels(true);
                        }
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

#endif

    }

}//AxonGenesis

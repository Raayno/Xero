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
    /// A class for storing event data read from a MIDI file
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MidiEvent")]
    public class MidiEvent : SerializableObject
    {
        public enum EventTypes : byte
        {
            None = 0x00,
            Unknown1 = 0x64,

            // Two Param
            NoteOff = 0x80,
            NoteOn = 0x90,
            NoteAftertouch = 0xA0,
            Controller = 0xB0,
            PitchBend = 0xE0,

            // Single Param
            ProgramChange = 0xC0,
            ChannelAfterTouch = 0xD0,

            // Variable Length Data
            SysEx = 0xF0,
            MetaEvent = 0xFF
        }

        public enum MetaEventTypes : byte
        {
            TrackNumber = 0x00,
            TextEvent = 0x01,
            Copyright = 0x02,
            TrackName = 0x03,
            TrackInstrument = 0x04,
            Lyrics = 0x05,
            Marker = 0x06,
            CuePoint = 0x07,
            ProgramName = 0x08,
            DeviceName = 0x09,
            MidiChannel = 0x20,
            MidiPort = 0x21,
            EndTrack = 0x2F,
            SetTempo = 0x51,
            SmpteOffset = 0x54,
            TimeSignature = 0x58,
            KeySignature = 0x59,
            SequencerSpecific = 0x7F
        }

        public float Time;
        public int DeltaTime;
        public int Channel;
        public EventTypes Type = EventTypes.None;
        public int Param1;
        public int Param2;
    }

}//AxonGenesis
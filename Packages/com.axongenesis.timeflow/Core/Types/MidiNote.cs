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
    /// Stores the properties for a specific midi note providing key timing and velocity information.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MidiNote")]
    public class MidiNote : SerializableObject
    {
        public int Index; // unique id counting order played in midi track
        public int Note; // 0-127
        public float Velocity;
        public float StartTime;
        public float EndTime;
        public float Duration;

        [NonSerialized]
        public int PlayIndex = -1; // for scripts to keep track of which notes they've played

        [NonSerialized]
        public MidiTweenChannel MapToChannel = null;

        private bool _MessageSent;

        public bool MessageSent {
            get {
                return _MessageSent;
            }
            set {
                if (_MessageSent != value) {
                    _MessageSent = value;
                }
            }
        }

        public MidiNote(int index, int note, float startTime, float velocity)
        {
            Index = index;
            Note = note;
            Velocity = velocity;
            StartTime = startTime;
        }

        public void SetEndTime(float endTime)
        {
            EndTime = endTime;
            Duration = endTime - StartTime;
        }

        public int AbsNote(bool collapseOctaves)
        {
            int n = Note;
            if (collapseOctaves) {
                while (n > 12) n -= 12;
            }
            return n;
        }
    }

}//AxonGenesis
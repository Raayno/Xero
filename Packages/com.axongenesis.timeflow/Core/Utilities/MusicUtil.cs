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
    /// A utility class defining global methods for calculating musical timing
    /// </summary>
    public static class MusicUtil
    {

        public enum Notes
        {
            Bar,
            HalfNote,
            QuarterNote,
            EighthNote,
            TwelvethNote,
            SixteenthNote,
            TwentyFourthNote,
            ThirtySecondNote,
            SixtyFourthNote
        }

        public static float Duration(float bpm, MusicUtil.Notes note, float count)
        {
            if (bpm <= 0) return 0;
            float seconds = 0f;
            float quarterNote = (60f / bpm);

            if (note == MusicUtil.Notes.Bar) {
                seconds = quarterNote * 4f;
            }
            else
            if (note == MusicUtil.Notes.HalfNote) {
                seconds = quarterNote * 2f;
            }
            else
            if (note == MusicUtil.Notes.QuarterNote) {
                seconds = quarterNote;
            }
            else
            if (note == MusicUtil.Notes.EighthNote) {
                seconds = quarterNote / 2f;
            }
            else
            if (note == MusicUtil.Notes.TwelvethNote) {
                seconds = quarterNote / 3f;
            }
            else
            if (note == MusicUtil.Notes.SixteenthNote) {
                seconds = quarterNote / 4f;
            }
            else
            if (note == MusicUtil.Notes.TwentyFourthNote) {
                seconds = quarterNote / 6f;
            }
            else
            if (note == MusicUtil.Notes.ThirtySecondNote) {
                seconds = quarterNote / 8f;
            }
            else
            if (note == MusicUtil.Notes.SixtyFourthNote) {
                seconds = quarterNote / 16f;
            }
            else {
                Debug.LogWarning("Unhandled note length:" + note);
            }
            seconds *= count;

            return seconds;
        }

        public static int CountNotes(float bpm, MusicUtil.Notes note, float time)
        {
            float dur = Duration(bpm, note, 1f);
            return dur <= 0 ? 0 : Mathf.FloorToInt(time / dur);
        }

        //TODO convert to const
        public static float[] NoteFrequencies;
        public static string[] NoteNames;

        private static void SetupNoteFrequencies()
        {
            if (NoteFrequencies == null) {
                NoteFrequencies = new float[13];
                NoteFrequencies[0] = 440f;
                NoteFrequencies[1] = 466.16f;
                NoteFrequencies[2] = 493.88f;
                NoteFrequencies[3] = 523.25f;
                NoteFrequencies[4] = 554.37f;
                NoteFrequencies[5] = 587.33f;
                NoteFrequencies[6] = 622.25f;
                NoteFrequencies[7] = 659.25f;
                NoteFrequencies[8] = 698.46f;
                NoteFrequencies[9] = 739.99f;
                NoteFrequencies[10] = 783.99f;
                NoteFrequencies[11] = 830.61f;
                NoteFrequencies[12] = 880f;
            }
            if (NoteNames == null) {
                NoteNames = new string[13];
                NoteNames[0] = "A";
                NoteNames[1] = "A#";
                NoteNames[2] = "B";
                NoteNames[3] = "C";
                NoteNames[4] = "C#";
                NoteNames[5] = "D";
                NoteNames[6] = "D#";
                NoteNames[7] = "E";
                NoteNames[8] = "F";
                NoteNames[9] = "F#";
                NoteNames[10] = "G";
                NoteNames[11] = "G#";
                NoteNames[12] = "A";
            }
        }

        public static string GetNoteName(float frequency)
        {
            frequency = Mathf.Abs(frequency);
            string name = "x";
            if (frequency == 0f) return "[" + frequency + "]";
            SetupNoteFrequencies();

            int octave = 0;
            float min = NoteFrequencies[0];
            float max = NoteFrequencies[12];
            while (frequency > max) {
                frequency = frequency / 2f;
                octave--;
                if (octave < -8) break;
            }
            while (frequency < min) {
                frequency = frequency * 2f;
                octave++;
                if (octave > 8) break;
            }

            for (int i = 0; i < 12; i++) {
                int j = i + 1;
                if (frequency >= NoteFrequencies[i] && frequency <= NoteFrequencies[j]) {
                    float a = Mathf.Abs(frequency - NoteFrequencies[i]);
                    float b = Mathf.Abs(frequency - NoteFrequencies[j]);
                    if (a < b) {
                        name = NoteNames[i];
                        break;
                    }
                    else {
                        name = NoteNames[j];
                        break;
                    }
                }
            }
            return name + " " + octave;
        }

        public static string FrequencyAndNote(float frequency)
        {
            string name = "(" + ((float)Mathf.RoundToInt(frequency * 10f) / 10f) + ") " + GetNoteName(frequency);

            return name;
        }
    }

}//AxonGenesis
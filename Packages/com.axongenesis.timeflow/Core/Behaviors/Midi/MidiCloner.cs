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
    [ExecuteInEditMode]
    [AddComponentMenu("Timeflow/MIDI Cloner")]
    public class MidiCloner : AxonGenesisBehavior
    {
        public MidiFile Midi = null;
        public int TrackNum = 1;

        public enum NoteModes
        {
            All,
            Single,
            Range
        }
        public NoteModes NoteMode = NoteModes.All;

        public int NoteMin = 0;
        public int NoteMax = 255;
        public float DurationMultiplier = 1f;
        public bool CollapseOctaves = false;

        public bool UseTimeRange = false;
        public float TimeRangeMin = 0;
        public float TimeRangeMax = 100;

        public GameObject Prefab = null;
        public bool LimitObjectCount = false;
        public int MaxObjectCount = 100;


        public enum MapModes
        {
            None,
            Time,
            Note,
            Velocity,
            Duration
        }
        public MapModes MapPositionX = MapModes.None;
        public MapModes MapPositionY = MapModes.None;
        public MapModes MapPositionZ = MapModes.Time;

        public MapModes MapRotationX = MapModes.None;
        public MapModes MapRotationY = MapModes.None;
        public MapModes MapRotationZ = MapModes.None;

        public MapModes MapScaleX = MapModes.None;
        public MapModes MapScaleY = MapModes.None;
        public MapModes MapScaleZ = MapModes.None;

        public bool EnableMapPosition = true;
        public bool EnableMapRotation = false;
        public bool EnableMapScale = false;

        public float MapPositionXAmount = 1f;
        public float MapPositionYAmount = 1f;
        public float MapPositionZAmount = 1f;

        public float MapRotationXAmount = 1f;
        public float MapRotationYAmount = 1f;
        public float MapRotationZAmount = 1f;

        public float MapScaleXAmount = 1f;
        public float MapScaleYAmount = 1f;
        public float MapScaleZAmount = 1f;

        public int RandomSeed = 1;

        public bool RandomizePosition = false;
        public Vector3 PositionRandom = Vector3.zero;

        public bool RandomizeRotation = false;
        public Vector3 RotationRandom = Vector3.zero;

        public bool RandomizeScale = false;
        public Vector3 ScaleRandom = Vector3.zero;

        public bool AutoRebuild = true;
        public bool AutoUpdate = false;
        public List<MidiClonerObject> Objects = null;


        public string NotesList = "";

#if UNITY_EDITOR

        public bool EditorShowMidi = true;
        public bool EditorShowPrefab = true;
        public bool EditorShowMapping = true;
        public bool EditorShowRandomize = true;

        public bool RandomizePositionMinMax = false;
        public bool RandomizeRotationMinMax = false;
        public bool RandomizeScaleMinMax = false;

#endif

        public void Clear()
        {
            Objects = new List<MidiClonerObject>();
            ObjectUtil.DestroyChildrenImmediate(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Midi == null) {
                Midi = MidiFile.Instance;
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            UpdateLayout();
        }

        private float CalculatePlacement(MidiNote note, MapModes mode, float factor, float defaultValue = 0f)
        {
            float value = defaultValue;
            if (mode == MapModes.Time) {
                value = note.StartTime * factor;
            }
            else
            if (mode == MapModes.Note) {
                value = note.AbsNote(CollapseOctaves) * factor;
            }
            else
            if (mode == MapModes.Velocity) {
                value = note.Velocity * factor;
            }
            else
            if (mode == MapModes.Duration) {
                value = note.Duration * factor;
            }

            return value;
        }

        public void DetectNoteRanges()
        {
            if (Midi == null) {
                Debug.LogWarning("Please add a MIDI File Source to the scene");
                return;
            }
            NotesList = null;
            List<int> notes = Midi.Tracks[TrackNum].NotesList();
            foreach (int n in notes) {
                if (NotesList == null) {
                    NotesList = "";
                }
                else {
                    NotesList += ", ";
                }
                NotesList += n;
            }
            //if (DebugEnabled) Debug.Log("Midi Notes on Track(" + TrackNum + "):" + NotesList);

            Vector2 range = Midi.Tracks[TrackNum].NotesRange();
            if (NoteMode == MidiCloner.NoteModes.All) {
                NoteMin = 0;
                NoteMax = 127;
            }
            else {
                NoteMin = (int)range.x;
                NoteMax = (int)range.y;
            }
        }

        public void Rebuild()
        {
            //if (DebugEnabled) Debug.Log($"{name}.MidiCloner.Build");
            if (Prefab == null || Midi == null) return;
            Clear();

            if (Midi.Tracks == null || TrackNum >= Midi.Tracks.Length) {
                Debug.LogWarning("The midi data does not contain the track index:" + TrackNum);
                return;
            }

            foreach (MidiNote note in Midi.Tracks[TrackNum].Notes) {
                bool buildNote = true;

                if (NoteMode == NoteModes.Single) {
                    buildNote = note.Note == NoteMin;
                }
                else
                if (NoteMode == NoteModes.Range) {
                    buildNote = note.Note >= NoteMin && note.Note <= NoteMax;
                }

                if (buildNote && UseTimeRange) {
                    buildNote = note.StartTime >= TimeRangeMin && note.StartTime <= TimeRangeMax;
                }
                if (buildNote && LimitObjectCount) {
                    if (Objects.Count >= MaxObjectCount) {
                        break;
                    }
                }

                if (!buildNote) continue;


                GameObject clone = GameObject.Instantiate(Prefab);
                clone.SetActive(true);
                clone.transform.parent = transform;
                ObjectUtil.ResetTransform(clone);

                MidiClonerObject obj = new MidiClonerObject(clone, note);
                Objects.Add(obj);
                UpdateObject(obj);

                if (LimitObjectCount && Objects.Count >= MaxObjectCount) break;
            }
        }

        public void UpdateLayout()
        {
            if (Prefab == null || Midi == null) return;
            if (Objects == null || Objects.Count == 0) {
                Rebuild();
                return;
            }
            //if (DebugEnabled) Debug.Log($"{name}.MidiCloner.UpdateLayout");
            if (Midi.Tracks == null || TrackNum >= Midi.Tracks.Length) {
                Debug.LogWarning("The midi data does not contain the track index:" + TrackNum);
                return;
            }

            UnityEngine.Random.InitState(RandomSeed);

            foreach (MidiClonerObject obj in Objects) {
                UpdateObject(obj);
            }
        }

        public void UpdateObject(MidiClonerObject obj)
        {
            Vector3 pos = obj.transform.localPosition;
            Vector3 rot = obj.transform.localEulerAngles;
            Vector3 scale = obj.transform.localScale;

            if (EnableMapPosition) {
                pos.x = CalculatePlacement(obj.Note, MapPositionX, MapPositionXAmount);
                pos.y = CalculatePlacement(obj.Note, MapPositionY, MapPositionYAmount);
                pos.z = CalculatePlacement(obj.Note, MapPositionZ, MapPositionZAmount);
            }

            if (EnableMapRotation) {
                rot.x = CalculatePlacement(obj.Note, MapRotationX, MapRotationXAmount);
                rot.y = CalculatePlacement(obj.Note, MapRotationY, MapRotationYAmount);
                rot.z = CalculatePlacement(obj.Note, MapRotationZ, MapRotationZAmount);
            }

            if (EnableMapScale) {
                scale.x = CalculatePlacement(obj.Note, MapScaleX, MapScaleXAmount, 1f);
                scale.y = CalculatePlacement(obj.Note, MapScaleY, MapScaleYAmount, 1f);
                scale.z = CalculatePlacement(obj.Note, MapScaleZ, MapScaleZAmount, 1f);
            }

            if (RandomizePosition) {
                pos.x += UnityEngine.Random.Range(-PositionRandom.x, PositionRandom.x);
                pos.y += UnityEngine.Random.Range(-PositionRandom.y, PositionRandom.y);
                pos.z += UnityEngine.Random.Range(-PositionRandom.z, PositionRandom.z);
            }

            if (RandomizeRotation) {
                rot.x += UnityEngine.Random.Range(-RotationRandom.x, RotationRandom.x);
                rot.y += UnityEngine.Random.Range(-RotationRandom.y, RotationRandom.y);
                rot.z += UnityEngine.Random.Range(-RotationRandom.z, RotationRandom.z);
            }

            if (RandomizeScale) {
                scale.x += UnityEngine.Random.Range(-ScaleRandom.x, ScaleRandom.x);
                scale.y += UnityEngine.Random.Range(-ScaleRandom.y, ScaleRandom.y);
                scale.z += UnityEngine.Random.Range(-ScaleRandom.z, ScaleRandom.z);
            }

            obj.transform.localPosition = pos;
            obj.transform.localEulerAngles = rot;
            obj.transform.localScale = scale;
        }

        private void Update()
        {
            if (AutoUpdate) {
                UpdateLayout();
            }
        }
    }

    [Serializable]
    public class MidiClonerObject : SerializableObject
    {
        public GameObject Object;
        public MidiNote Note;

        public Transform transform => Object.transform;

        public MidiClonerObject() { }

        public MidiClonerObject(GameObject obj, MidiNote note)
        {
            Object = obj;
            Note = note;
        }

    }

}//AxonGenesis
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiFile))]
    public class MidiFileEditor : AxonGenesisEditor<MidiFile, MidiFileEdit> { }

    sealed public class MidiFileEdit : AxonGenesisBehaviorEdit<MidiFile>
    {
#if TIMEFLOW_PRO
        public const string kAddMidiFile = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎹 MIDI File";
#else
        public const string kAddMidiFile = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "MIDI File";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: MIDI File";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddMidiFile, false, 180)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddMidiFile, false, 180)]
        public static void AddMidiFile()
        {
            ObjectUtil.GetOrAddComponent<MidiFile>(TimeflowMenu.GetSelectedOrNewGameObject("MIDI File"));
        }

        public int DisplayStart;
        public int DisplayLimit = 25;

        public MidiFileEdit() { }
        public MidiFileEdit(MidiFile _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-file";
        }

        public override void GUIMenu()
        {
            AxonGUI.SetTooltip("Select a binary midi asset to load note and timing data. Important! The file must end with the extension .bytes to be properly read.");
            if (AxonGUI.ButtonInline("Load MIDI File")) {
                target.Parse();
            }

            AxonGUI.UndoName = "Set MIDI File";
            TextAsset file = AxonGUI.FieldObjectInline(target, target.File, typeof(TextAsset), false, GUILayout.Width(200)) as TextAsset;
            if (file != target.File) {
                target.File = file;
                if (target.File != null) target.Parse();
            }

        }

        public override void OnInspectorGUI()
        {
            if (target.File == null || !target.IsLoaded) {
                AxonGUI.HelpBox("Assign a binary midi asset and press 'Load MIDI File' to read note and timing data. Important! The file must end with the extension .bytes to be properly read.", MessageType.Info);
            }
            else {
                TimingGUI();
                NotesGUI();
                TracksGUI();
            }
        }

        public void TimingGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTiming = AxonGUI.Foldout(target.EditorShowTiming, "Timing");
            if (target.EditorShowTiming) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.HelpBox("The following parameters are automatically read from the midi file and some cannot be modified manually", MessageType.Info);

                EditorGUI.BeginDisabledGroup(true);
                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Ticks Per Quarternote";
                AxonGUI.SetTooltip("This value defines the time signature in the midi file. In conjunction with milliseconds per quarternote it determine beats per minute.");
                target.TPQN = AxonGUI.FieldInt(target, "Ticks Per Quarternote", target.TPQN);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Ms Per Quarternote";
                AxonGUI.SetTooltip("This is provided by the midi file and defines the time in milliseconds per quarter note, used to derrive beats per minute.");
                target.MPQN = AxonGUI.FieldInt(target, "Ms Per Quarternote", target.MPQN);
                AxonGUI.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                AxonGUI.BeginHorizontalBox();
                EditorGUI.BeginDisabledGroup(true);
                AxonGUI.UndoName = "Set BPM";
                AxonGUI.SetTooltip("This defines the timing (beats per minute) of the midi file. This value is auto-detected but may be forced to another timing.");
                target.BPM = AxonGUI.FieldFloat(target, "BPM", target.BPM);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(target.MatchSceneBPM);
                AxonGUI.UndoName = "Set Target BPM";
                target.TargetBPM = AxonGUI.FieldFloatInline(target, "Target BPM", target.TargetBPM);
                EditorGUI.EndDisabledGroup();

                AxonGUI.UndoName = "Set Match Scene BPM";
                AxonGUI.SetTooltip("Automatically matches the current BPM set in the current Timeflow. It is recommended that all midi and Timeflow instances use the same BPM.");
                target.MatchSceneBPM = AxonGUI.FieldToggleInline(target, "Match Scene BPM", target.MatchSceneBPM);
                if (target.MatchSceneBPM && Timeflow.Active != null) {
                    target.TargetBPM = Timeflow.Active.BPM;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Time Scale";
                AxonGUI.SetTooltip("Time Scale is auto-calculated when the midi BPM is different from the current Timeflow BPM, in order to match the timing of the scene.");
                target.MidiTimeScale = AxonGUI.FieldFloat(target, "Time Scale", target.MidiTimeScale);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Start Time";
                AxonGUI.SetTooltip("Specifies the time in seconds for the midi file to start playing, relative to the current Timeflow instance.");
                target.StartTime = AxonGUI.FieldFloat(target, "Start Time", target.StartTime);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Duration";
                AxonGUI.SetTooltip("The total length in seconds of the midi file. This value is automatically calculated when reading the file, but can be manually changed as needed.");
                target.Duration = AxonGUI.FieldFloat(target, "Duration", target.Duration);

                AxonGUI.UndoName = "Set Loop";
                AxonGUI.SetTooltip("Enable loop to repeat the midi duration indefinitely.");
                target.EnableLoop = AxonGUI.FieldToggleInline(target, "Loop", target.EnableLoop);
                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void NotesGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowNotes = AxonGUI.Foldout(target.EditorShowNotes, "Note Adjustments");
            if (target.EditorShowNotes) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Note Min Attack";
                AxonGUI.SetTooltip("This sets the minimum starting attack time (in seconds) for all notes, affecting all MidiTween instances. Default value is 0.");
                target.MinAttack = AxonGUI.FieldFloat(target, "Min Attack", target.MinAttack);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Note Min Release";
                AxonGUI.SetTooltip("This sets the minimum release time (in seconds) for all notes, affecting all MidiTween instances. Default value is 0.");
                target.MinRelease = AxonGUI.FieldFloat(target, "Min Release", target.MinRelease);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Note Intensity";
                AxonGUI.SetTooltip("This multiplies the velocity of all notes played, to globally increase or decrease note intensity. Default value is 1.");
                target.Intensity = AxonGUI.FieldFloat(target, "Intensity", target.Intensity);
                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void TracksGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTracks = AxonGUI.Foldout(target.EditorShowTracks, "Tracks");
            if (target.EditorShowTracks) {
                AxonGUI.BeginBoxPadded();

                if (target.Tracks != null && target.Tracks.Length > 0) {
                    for (int i = 0; i < target.Tracks.Length; i++) {
                        MidiTrack t = target.Tracks[i];
                        t.EditorShow = AxonGUI.Foldout(t.EditorShow, (i + 1) + ": " + t.Name + " (" + t.Notes.Count + " notes)");
                        if (t.EditorShow) {
                            AxonGUI.BeginBoxPadded();
                            AxonGUI.UndoName = "Set Track Name";
                            t.Name = AxonGUI.FieldText(target, "Track Name", t.Name);

                            AxonGUI.Space();
                            if (t.Notes == null || t.Notes.Count == 0) {
                                AxonGUI.Label(" ", "No Notes");
                            }
                            else {
                                AxonGUI.BeginBox();
                                AxonGUI.Heading("Notes");
                                if (t.Notes.Count > 10) {
                                    AxonGUI.UndoName = "Set Display Count";
                                    DisplayLimit = AxonGUI.FieldSliderInt(target, "Display Count", DisplayLimit, 0, t.Notes.Count);

                                    AxonGUI.UndoName = "Set Starting At";
                                    DisplayStart = AxonGUI.FieldSliderInt(target, "Starting At", DisplayStart, 0, t.Notes.Count);
                                }
                                else {
                                    DisplayLimit = 10;
                                    DisplayStart = 0;
                                }

                                AxonGUI.Space();

                                int count = DisplayLimit + DisplayStart;
                                if (count > t.Notes.Count) count = t.Notes.Count;
                                for (int n = DisplayStart; n < count; n++) {
                                    MidiNote note = t.Notes[n];
                                    AxonGUI.BeginHorizontal();
                                    AxonGUI.LabelInline("" + (n + 1));

                                    AxonGUI.UndoName = "Set Note Start Time";
                                    note.StartTime = AxonGUI.FieldFloatInline(target, "Start", note.StartTime);

                                    AxonGUI.UndoName = "Set Note End Time";
                                    note.EndTime = AxonGUI.FieldFloatInline(target, "End", note.EndTime);

                                    AxonGUI.UndoName = "Set Note Duration";
                                    note.Duration = AxonGUI.FieldFloatInline(target, "Duration", note.Duration);

                                    AxonGUI.UndoName = "Set Note";
                                    note.Note = AxonGUI.FieldIntInline(target, "Note", note.Note, GUILayout.Width(60));

                                    AxonGUI.UndoName = "Set Note Velocity";
                                    note.Velocity = AxonGUI.FieldFloatInline(target, "Velocity", note.Velocity);

                                    AxonGUI.EndHorizontal();
                                }
                                AxonGUI.EndBox();
                            }
                            AxonGUI.EndBoxPadded();
                        }
                    }
                }
                else {
                    AxonGUI.Label("No Tracks", "");
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }
    }


}//AxonGenesis

#endif
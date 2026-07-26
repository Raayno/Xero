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
    [CustomEditor(typeof(MidiCloner))]
    public class MidiClonerEditor : AxonGenesisEditor<MidiCloner, MidiClonerUI> { }

    public class MidiClonerUI : AxonGenesisBehaviorEdit<MidiCloner>
    {
#if TIMEFLOW_PRO
        public const string kAddMidiCloner = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎡 MIDI Cloner";
#else
        public const string kAddMidiCloner = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "MIDI Cloner";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: MIDI Cloner";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddMidiCloner, false, 183)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddMidiCloner, false, 183)]
        public static void AddMidiCloner()
        {
            ObjectUtil.GetOrAddComponent<MidiCloner>(TimeflowMenu.GetSelectedOrNewGameObject("MIDI Cloner"));
        }

        public MidiClonerUI() { }

        public MidiClonerUI(MidiCloner _target)
        {
            target = _target;
        }

        public override void GUIMenu()
        {
            AxonGUI.SetTooltip("Forces all objects to update.");
            if (AxonGUI.ButtonInline("Refresh")) {
                target.Refresh();
            }

            AxonGUI.SetTooltip("");
            if (AxonGUI.ButtonInline("Rebuild")) {
                target.Rebuild();
            }

            AxonGUI.UndoName = "Set Auto Rebuild";
            AxonGUI.SetTooltip("If enabled, regenerates prefab objects whenever a change is made to the inspector settings.");
            target.AutoRebuild = AxonGUI.FieldToggleInline(target, "Auto Rebuild", target.AutoRebuild);

            AxonGUI.UndoName = "Set Auto Update";
            AxonGUI.SetTooltip("If enabled, the existing object transforms are udpated every frame. Use this for dynamic updating and animations.");
            target.AutoUpdate = AxonGUI.FieldToggleInline(target, "Auto Update", target.AutoUpdate);
        }

        public override void OnInspectorGUI()
        {
            MidiGUI();
            PrefabGUI();
            MappingGUI();
            RandomizeGUI();

            if (GUI.changed) {
                target.UpdateLayout();
            }
        }

        public void MidiGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowMidi = AxonGUI.Foldout(target.EditorShowMidi, "Midi Input");
            if (target.EditorShowMidi) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginChangeCheck();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set MIDI File";
                AxonGUI.SetTooltip("Assign a MidiFile instance to provide midi data. If one has not yet been created, use the menu command Tools/Timeflow/Add/MIDI File");
                MidiFile file = AxonGUI.FieldObject(target, "MIDI File", target.Midi, typeof(MidiFile), true) as MidiFile;
                if (file != target.Midi) {
                    target.Midi = file;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                if (target.Midi != null && target.Midi.Tracks != null && target.Midi.Tracks.Length > 0) {
                    string[] tracks = new string[target.Midi.Tracks.Length];
                    for (int i = 0; i < tracks.Length; i++) {
                        tracks[i] = target.Midi.Tracks[i].Name;
                        if (tracks[i] == "" || tracks[i] == null) {
                            tracks[i] = "Track " + i;
                        }
                    }
                    AxonGUI.UndoName = "Set Track Number";
                    AxonGUI.SetTooltip("Select which track of midi data to read notes from. Only 1 track may be referenced for each MidiCloner instance.");
                    target.TrackNum = AxonGUI.FieldPopup(target, "Track", target.TrackNum, tracks);
                }
                else {
                    AxonGUI.SetTooltip("Missing MidiFile or the file contains no readable tracks. Please check the MidiFile setup.");
                    AxonGUI.UndoName = "Set Track Number";
                    target.TrackNum = AxonGUI.FieldInt(target, "Track Number", target.TrackNum);
                }
                AxonGUI.EndHorizontal();

                if (target.NotesList != null && target.NotesList.Length >= 120) {
                    AxonGUI.SetTooltip("This displays the midi notes by index number played on the selected track. Press 'Get Notes' to list all of the notes played on the track.");
                    AxonGUI.LabelInline("Notes", target.NotesList);
                }

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Determines how midi notes are mapped.\n\n " +
                    "SINGLE OBJECT\n\n" +
                    "All:\nAll midi notes on the track are played, mapping to 1 object property.\n\n" +
                    "Single:\nPlays one specific midi note, mapping to 1 object property.\n\n" +
                    "Range:\nPlays all midi notes within a range, mapping to 1 object property.\n\n");
                MidiCloner.NoteModes notes = (MidiCloner.NoteModes)AxonGUI.FieldEnumPopup(target, "Notes", target.NoteMode);
                if (notes != target.NoteMode) {
                    target.NoteMode = notes;
                }

                if (target.NoteMode != MidiCloner.NoteModes.All) {
                    if (target.NoteMode == MidiCloner.NoteModes.Range) {
                        AxonGUI.SetTooltip("Sets the lowest and highest midi notes to use as input. All notes equal to or between min and max are played.");
                        AxonGUI.UndoName = "Set Note Min";
                        target.NoteMin = AxonGUI.FieldIntInline(target, "Note Min", target.NoteMin);

                        AxonGUI.UndoName = "Set Note Max";
                        target.NoteMax = AxonGUI.FieldIntInline(target, "Max", target.NoteMax);
                    }
                    else
                    if (target.NoteMode == MidiCloner.NoteModes.Single) {
                        AxonGUI.UndoName = "Set Note";
                        AxonGUI.SetTooltip("Plays only the specified note, ignoring all others. This might be used, for example, on a drum track to isolate just the kick. Use 'Get Notes' above to determine which notes on the selected track are played.");
                        target.NoteMin = target.NoteMax = AxonGUI.FieldIntInline(target, "Note", target.NoteMin);
                    }

                    AxonGUI.UndoName = "Set Collapse Octaves";
                    AxonGUI.SetTooltip("If enabled, all notes map to the 12 notes of the scale, regardless of the octave they are played in.");
                    target.CollapseOctaves = AxonGUI.FieldToggleInline(target, "Collapse Octaves", target.CollapseOctaves);
                }
                AxonGUI.EndHorizontal();

                if (target.NoteMin < 0) target.NoteMin = 0;
                if (target.NoteMax < 0) target.NoteMax = 0;
                if (target.NoteMin > 127) target.NoteMin = 127;
                if (target.NoteMax > 127) target.NoteMax = 127;

                AxonGUI.BeginBox();
                if (target.Midi != null) {
                    AxonGUI.SetTooltip("This displays the midi notes by index number played on the selected track.");
                    string notesList = "None";
                    if (target.NotesList != null && target.NotesList.Length < 120) {
                        notesList = target.NotesList;
                    }
                    AxonGUI.FieldTextArea(null, "Notes Played", notesList + "\n", GUILayout.ExpandHeight(true));
                    AxonGUI.BeginHorizontal();
                    AxonGUI.Space();
                    AxonGUI.SetTooltip("Automatically detects the notes played in the midi track and assigns the note indices to the object properties listed below. If using mutiple targets, each object will be assigned a different note index.");
                    if (AxonGUI.ButtonInline("Detect Note Range")) {
                        target.DetectNoteRanges();
                    }
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();


                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Time Range";
                AxonGUI.SetTooltip("Increases or decreases the duration of all notes, affecting their length.");
                target.UseTimeRange = AxonGUI.FieldToggle(target, "Time Range", target.UseTimeRange);
                if (target.UseTimeRange) {
                    float max = target.Midi != null ? target.Midi.GetDuration() : 100;

                    AxonGUI.UndoName = "Set Time Range Min";
                    target.TimeRangeMin = AxonGUI.FieldFloatInline(target, "Min", target.TimeRangeMin);
                    AxonGUI.FieldSliderMinMaxInline(target, "", ref target.TimeRangeMin, ref target.TimeRangeMax, 0, max);

                    AxonGUI.UndoName = "Set Time Range Max";
                    target.TimeRangeMax = AxonGUI.FieldFloatInline(target, "Max", target.TimeRangeMax);
                }
                AxonGUI.EndHorizontal();

                if (AxonGUI.EndChangeCheck() && target.AutoRebuild) {
                    target.Rebuild();
                }

                AxonGUI.EndBoxPadded();

            }
            AxonGUI.EndBox();
        }

        public void PrefabGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowPrefab = AxonGUI.Foldout(target.EditorShowPrefab, "Prefab Object");
            if (target.EditorShowPrefab) {
                AxonGUI.BeginBoxPadded();
                AxonGUI.BeginChangeCheck();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Prefab";
                AxonGUI.SetTooltip("Assign the prefab to clone");
                target.Prefab = AxonGUI.FieldObject(target, "Prefab", target.Prefab, typeof(GameObject), true) as GameObject;
                AxonGUI.UndoName = "Set Prefab Limit Count";
                target.LimitObjectCount = AxonGUI.FieldToggleInline(target, "Limit Count", target.LimitObjectCount);
                if (target.LimitObjectCount) {
                    AxonGUI.UndoName = "Set Prefab Limit Max";
                    target.MaxObjectCount = AxonGUI.FieldIntInline(target, "Max", target.MaxObjectCount);
                }
                AxonGUI.EndHorizontal();

                if (AxonGUI.EndChangeCheck() && target.AutoRebuild) {
                    target.Rebuild();
                }

                AxonGUI.EndBoxPadded();

            }
            AxonGUI.EndBox();
        }

        public void MappingGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowMapping = AxonGUI.Foldout(target.EditorShowMapping, "Mapping");
            if (target.EditorShowMapping) {
                AxonGUI.BeginBoxPadded();

                var mapWidth = GUILayout.Width(100);
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Enable Position";
                target.EnableMapPosition = AxonGUI.FieldToggle(target, "Enable Position", target.EnableMapPosition);

                if (target.EnableMapPosition) {
                    AxonGUI.UndoName = "Set Enable Position X";
                    target.MapPositionX = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "X", target.MapPositionX, mapWidth);
                    if (target.MapPositionX != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Position X *";
                        target.MapPositionXAmount = AxonGUI.FieldFloatInline(target, "*", target.MapPositionXAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Position Y";
                    target.MapPositionY = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Y", target.MapPositionY, mapWidth);
                    if (target.MapPositionY != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Position Y *";
                        target.MapPositionYAmount = AxonGUI.FieldFloatInline(target, "*", target.MapPositionYAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Position Z";
                    target.MapPositionZ = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Z", target.MapPositionZ, mapWidth);
                    if (target.MapPositionZ != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Position Z *";
                        target.MapPositionZAmount = AxonGUI.FieldFloatInline(target, "*", target.MapPositionZAmount);
                    }
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Enable Rotation";
                target.EnableMapRotation = AxonGUI.FieldToggle(target, "Enable Rotation", target.EnableMapRotation);

                if (target.EnableMapRotation) {
                    AxonGUI.UndoName = "Set Enable Rotation X";
                    target.MapRotationX = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "X", target.MapRotationX, mapWidth);
                    if (target.MapRotationX != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Rotation X *";
                        target.MapRotationXAmount = AxonGUI.FieldFloatInline(target, "*", target.MapRotationXAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Rotation Y";
                    target.MapRotationY = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Y", target.MapRotationY, mapWidth);
                    if (target.MapRotationY != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Rotation Y *";
                        target.MapRotationYAmount = AxonGUI.FieldFloatInline(target, "*", target.MapRotationYAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Rotation Z";
                    target.MapRotationZ = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Z", target.MapRotationZ, mapWidth);
                    if (target.MapRotationZ != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Rotation Z *";
                        target.MapRotationZAmount = AxonGUI.FieldFloatInline(target, "*", target.MapRotationZAmount);
                    }
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Enable Scale";
                target.EnableMapScale = AxonGUI.FieldToggle(target, "Enable Scale", target.EnableMapScale);

                if (target.EnableMapScale) {
                    AxonGUI.UndoName = "Set Enable Scale X";
                    target.MapScaleX = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "X", target.MapScaleX, mapWidth);
                    if (target.MapScaleX != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Scale X *";
                        target.MapScaleXAmount = AxonGUI.FieldFloatInline(target, "*", target.MapScaleXAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Scale Y";
                    target.MapScaleY = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Y", target.MapScaleY, mapWidth);
                    if (target.MapScaleY != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Scale Y *";
                        target.MapScaleYAmount = AxonGUI.FieldFloatInline(target, "*", target.MapScaleYAmount);
                    }
                    AxonGUI.UndoName = "Set Enable Scale Z";
                    target.MapScaleZ = (MidiCloner.MapModes)AxonGUI.FieldEnumPopupInline(target, "Z", target.MapScaleZ, mapWidth);
                    if (target.MapScaleZ != MidiCloner.MapModes.None) {
                        AxonGUI.UndoName = "Set Enable Scale Z *";
                        target.MapScaleZAmount = AxonGUI.FieldFloatInline(target, "*", target.MapScaleZAmount);
                    }
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.EndBoxPadded();

            }
            AxonGUI.EndBox();
        }

        public void RandomizeGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowRandomize = AxonGUI.Foldout(target.EditorShowRandomize, "Randomize");
            if (target.EditorShowRandomize) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();

                AxonGUI.UndoName = "Set Randomize Position";
                if (target.RandomizePosition) {
                    AxonGUI.FieldVector3MinMax(target, "Position", ref target.PositionRandom, ref target.PositionRandom, ref target.RandomizePositionMinMax,
                        target.RandomizePositionMinMax ? "Min Value" : "Target Value",
                        target.RandomizePositionMinMax ? "Max Value" : "Add/Subtract Amount");
                }
                else {
                    target.RandomizePosition = AxonGUI.FieldToggle(target, "Position", target.RandomizePosition);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Randomize Rotation";
                if (target.RandomizeRotation) {
                    AxonGUI.FieldVector3MinMax(target, "Rotation", ref target.RotationRandom, ref target.RotationRandom, ref target.RandomizeRotationMinMax,
                        target.RandomizeRotationMinMax ? "Min Value" : "Target Value",
                        target.RandomizeRotationMinMax ? "Max Value" : "Add/Subtract Amount");
                }
                else {
                    target.RandomizeRotation = AxonGUI.FieldToggle(target, "Rotation", target.RandomizeRotation);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Randomize Scale";
                if (target.RandomizeScale) {
                    AxonGUI.FieldVector3MinMax(target, "Scale", ref target.ScaleRandom, ref target.ScaleRandom, ref target.RandomizeScaleMinMax,
                        target.RandomizeScaleMinMax ? "Min Value" : "Target Value",
                        target.RandomizeScaleMinMax ? "Max Value" : "Add/Subtract Amount");
                }
                else {
                    target.RandomizeScale = AxonGUI.FieldToggle(target, "Scale", target.RandomizeScale);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.UndoName = "Set Random Seed";
                target.RandomSeed = AxonGUI.FieldInt(target, "Seed", target.RandomSeed);

                AxonGUI.EndBoxPadded();

            }
            AxonGUI.EndBox();
        }
    }

}//AxonGenesis

#endif
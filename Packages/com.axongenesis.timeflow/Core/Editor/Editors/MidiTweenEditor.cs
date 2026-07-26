// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiTween))]
    public class MidiTweenEditor : AxonGenesisEditor<MidiTween, MidiTweenUI> { }

    sealed public class MidiTweenUI : AxonGenesisBehaviorEdit<MidiTween>
    {
#if TIMEFLOW_PRO
        public const string kAddMidiTween = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🥁 MIDI Tween";
#else
        public const string kAddMidiTween = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "MIDI Tween";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: MIDI Tween";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddMidiTween, false, 182)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddMidiTween, false, 182)]
        public static void AddMidiTween()
        {
            Undo.AddComponent<MidiTween>(TimeflowMenu.GetSelectedOrNewGameObject("MIDI Tween"));
        }

        private bool isRenaming;
        private string baseName = "Note";

        public bool showExportGUI = true;
        public TimeflowBehaviorSharedEdit behaviorUI;

        public MidiTweenUI() { }

        public MidiTweenUI(MidiTween _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-tween";
        }

        public override void GUISetup()
        {
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            base.GUISetup();
        }

        public override void GUIMenu()
        {
            int c = target.MidiChannelCount;
            if (c == 1) {
                AxonGUI.PropertySelect(target, typeof(MidiTween), target.gameObject, target.MidiChannels[0].ToProperty, Property.PropertyFilters.NumericOnly, null, true, true, false, false);
            }
            else
            if (c > 1) {
                AxonGUI.LabelInline("Multiple Targets (" + c + ")");
            }
            else {
                AxonGUI.Warning("No target property has been assigned");
            }
#if AXON_EXPERIMENTAL
            target.AEFrameRate = AxonGUI.FieldFloatInline(target.AEFrameRate, GUILayout.Width(60));
            if (GUILayout.Button("Copy to AE", GUILayout.Width(100))) {
                target.CopyToAE();
            }
#endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            MidiGUI();
            PropertyGUI();
            ProcessingGUI();
            if (!target.SendOnly) ValuesGUI();

            if (EditorGUI.EndChangeCheck()) {
                target.SetupMidi();
            }
            AdvancedGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.Refresh();
            }
        }

        public void MidiGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowMidi = AxonGUI.Foldout(target.EditorShowMidi, "Midi Input");
            if (target.EditorShowMidi) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set MIDI File";
                AxonGUI.SetTooltip("Assign a MidiFile instance to provide midi data. If one has not yet been created, use the menu command AxonGenesis/Add/MIDI File");
                MidiFile file = AxonGUI.FieldObject(target, "MIDI File", target.Midi, typeof(MidiFile), true) as MidiFile;
                if (file != target.Midi) {
                    target.Midi = file;
                    target.SetupNoteRanges();
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Track Number";
                if (target.Midi != null && target.Midi.Tracks != null && target.Midi.Tracks.Length > 0) {
                    string[] tracks = new string[target.Midi.Tracks.Length];
                    for (int i = 0; i < tracks.Length; i++) {
                        tracks[i] = target.Midi.Tracks[i].Name;
                        if (tracks[i] == "" || tracks[i] == null) {
                            tracks[i] = "Track " + i;
                        }
                    }
                    AxonGUI.SetTooltip("Select which track of midi data to read notes from. Only 1 track may be referenced for each MidiTween instance.");
                    target.TrackNum = AxonGUI.FieldPopup(target, "Track", target.TrackNum, tracks);
                }
                else {
                    AxonGUI.SetTooltip("Missing MidiFile or the file contains no readable tracks. Please check the MidiFile setup.");
                    target.TrackNum = AxonGUI.FieldInt(target, "Track Number", target.TrackNum);
                }
                AxonGUI.EndHorizontal();

                if (target.NotesList != null && target.NotesList.Length >= 120) {
                    AxonGUI.SetTooltip("This displays the midi notes by index number played on the selected track. Press 'Get Notes' to list all of the notes played on the track.");
                    AxonGUI.LabelInline("Notes", target.NotesList);
                }

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Note Mode";
                AxonGUI.SetTooltip("Determines how midi notes are mapped.\n\n " +
                    "SINGLE OBJECT\n\n" +
                    "All:\nAll midi notes on the track are played, mapping to 1 object property.\n\n" +
                    "Single:\nPlays one specific midi note, mapping to 1 object property.\n\n" +
                    "Range:\nPlays all midi notes within a range, mapping to 1 object property.\n\n" +
                    "MULTIPLE OBJECTS\n\n" +
                    "Multiple Targets:\nPlays all midi notes, mapping each note to a different object.\n\n" +
                    "Sequence Targets:\nPlays all midi notes, mapping each note hit to the subsequent object in a list, looping in order.");
                MidiTween.NoteModes notes = (MidiTween.NoteModes)AxonGUI.FieldEnumPopup(target, "Notes", target.NoteMode);
                if (notes != target.NoteMode) {
                    target.NoteMode = notes;
                    target.SetupNoteRanges();
                }

                if (target.NoteMode == MidiTween.NoteModes.All || target.NoteMode == MidiTween.NoteModes.Range || target.NoteMode == MidiTween.NoteModes.Single) {
                    AxonGUI.UndoName = "Set Note Polyphonic";
                    AxonGUI.SetTooltip("This allows multiple midi notes to be played simultaneously, applying cumulatively. Alternatively if disabled, only 1 note is played at a time interrupting any other notes playing. In musical terms this is equivalent to legato (on) and staccato (off).");
                    target.Polyphonic = AxonGUI.FieldToggleInline(target, "Polyphonic", target.Polyphonic);
                }
                AxonGUI.EndHorizontal();

                if (target.NoteMode != MidiTween.NoteModes.MultipleTargets) {
                    AxonGUI.BeginChangeCheck();
                    if (target.NoteMode != MidiTween.NoteModes.All) {
                        AxonGUI.BeginHorizontal();
                        if (target.NoteMode == MidiTween.NoteModes.Range || target.NoteMode == MidiTween.NoteModes.SequenceTargets) {
                            AxonGUI.SetTooltip("Sets the lowest and highest midi notes to use as input. All notes equal to or between min and max are played.");
                            AxonGUI.UndoName = "Set Note Min";
                            target.NoteMin = AxonGUI.FieldInt(target, "Note Min", target.NoteMin);

                            AxonGUI.UndoName = "Set Note Max";
                            target.NoteMax = AxonGUI.FieldIntInline(target, "Max", target.NoteMax);
                        }
                        else
                        if (target.NoteMode == MidiTween.NoteModes.Single) {
                            AxonGUI.UndoName = "Set Note";
                            AxonGUI.SetTooltip("Plays only the specified note, ignoring all others. This might be used, for example, on a drum track to isolate just the kick. Use 'Get Notes' above to determine which notes on the selected track are played.");
                            target.NoteMin = target.NoteMax = AxonGUI.FieldInt(target, "Note", target.NoteMin);
                        }
                        AxonGUI.EndHorizontal();
                    }

                    if (target.NoteMin < 0) target.NoteMin = 0;
                    if (target.NoteMax < 0) target.NoteMax = 0;
                    if (target.NoteMin > 127) target.NoteMin = 127;
                    if (target.NoteMax > 127) target.NoteMax = 127;
                    if (target.MidiChannels[0].MinVelocity < 0) target.MidiChannels[0].MinVelocity = 0;
                    if (target.MidiChannels[0].MinVelocity > 1f) target.MidiChannels[0].MinVelocity = 1f;


                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Note Min Velocity";
                    AxonGUI.SetTooltip("Specifies the minimum velocity (how hard the key must be pressed) for the note to be detected. Use a value of 0 to react to all midi notes regardless of velocity.");
                    target.MidiChannels[0].MinVelocity = AxonGUI.FieldSlider(target, "Min Velocity", target.MidiChannels[0].MinVelocity, 0, 1f);
                    AxonGUI.EndHorizontal();

                    if (AxonGUI.EndChangeCheck()) {
                        target.SetupChannels(true);
                        EditorGUIUtility.ExitGUI();
                    }
                }

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
                        target.SetupNoteRanges();
                    }
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();


                AxonGUI.EndBoxPadded();

            }
            AxonGUI.EndBox();
        }

        public void PropertyGUI()
        {
            string foldoutName = "Target Property";
            if (target.NoteMode == MidiTween.NoteModes.MultipleTargets) {
                foldoutName = "Multiple Targets";
            }
            else
            if (target.NoteMode == MidiTween.NoteModes.SequenceTargets) {
                foldoutName = "Sequence Targets";
            }

            AxonGUI.BeginBox();
            target.EditorShowProperty = AxonGUI.Foldout(target.EditorShowProperty, foldoutName);
            if (target.EditorShowProperty) {
                AxonGUI.BeginBoxPadded();

                if (target.NoteMode == MidiTween.NoteModes.SequenceTargets) {
                    AxonGUI.BeginHorizontal();

                    AxonGUI.UndoName = "Set Sequence Mode";
                    AxonGUI.SetTooltip("The sequence determines the order in which to traverse the list of objects. Each midi note affects the subsequent object in the list, repeating the order when the end is reached.");
                    target.SequenceMode = (MidiTween.SequenceModes)AxonGUI.FieldEnumPopupInline(target, "Sequence Mode", target.SequenceMode);

                    if (target.SequenceMode == MidiTween.SequenceModes.Skip || target.SequenceMode == MidiTween.SequenceModes.SkipReverse) {
                        AxonGUI.SetTooltip("Instead of playing the objects sequentially with each note hit, this skips the specified number of objects each note hit, looping back around to the start when it reaches the end.");
                        AxonGUI.UndoName = "Set Skip";
                        target.SequenceSkip = AxonGUI.FieldIntInline(target, "Skip", target.SequenceSkip);
                    }
                    else
                    if (target.SequenceMode == MidiTween.SequenceModes.Random) {
                        AxonGUI.UndoName = "Set Random Seed";
                        AxonGUI.SetTooltip("Set the random seed for alternate random patterns.");
                        target.SequenceRandomSeed = AxonGUI.FieldIntInline(target, "Random Seed", target.SequenceRandomSeed);
                    }
                    AxonGUI.EndHorizontal();
                }


                if (target.NoteMode == MidiTween.NoteModes.MultipleTargets || target.NoteMode == MidiTween.NoteModes.SequenceTargets) {
                    ObjectsGUI();
                }
                else {
                    AxonGUI.BeginHorizontal();
                    if (target.MidiChannels[0].ToProperty == null) target.MidiChannels[0].ToProperty = new Property();
                    if (target.MidiChannels[0].ToProperty.Comp == null) target.MidiChannels[0].ToProperty.Comp = target.transform;
                    if (target.SendOnly) {
                        AxonGUI.UndoName = "Set Target Object";
                        AxonGUI.SetTooltip("Specify a component to receive a 'OnMidiNote' message on each midi note detected.");
                        target.MidiChannels[0].ToProperty.Comp = (Component)AxonGUI.FieldObject(target, "Target Object", target.MidiChannels[0].ToProperty.Comp, typeof(Component), true);
                    }
                    else {
                        AxonGUI.SetTooltip("Select the target property to apply motion to based on the midi notes played.");
                        AxonGUI.PropertySelect(target, typeof(MidiTween), target.gameObject, target.MidiChannels[0].ToProperty, Property.PropertyFilters.NumericOnly, "Apply To", true, false);
                    }
                    AxonGUI.EndHorizontal();
                }

                if (!target.SendOnly) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Activate Object";
                    AxonGUI.SetTooltip("If enabled, each target object is enabled and disabled with each note played. Important: this must only be used to control other game objects and not this current game object, since it needs to remain active for the script to function.");
                    target.ActivateObject = AxonGUI.FieldToggle(target, "Activate Object" + (target.NoteMode == MidiTween.NoteModes.MultipleTargets || target.NoteMode == MidiTween.NoteModes.SequenceTargets ? "s" : ""), target.ActivateObject);
                    if (target.ActivateObject) {
                        AxonGUI.UndoName = "Set Delay Hide";
                        AxonGUI.SetTooltip("Extra time in seconds to keep the object visible after each note hit.");
                        target.DelayHide = AxonGUI.FieldFloat(target, "Delay Hide", target.DelayHide);
                    }
                    AxonGUI.EndHorizontal();
                    if (target.ActivateObject && target.MidiChannels[0].ToProperty.Comp.gameObject == target.gameObject) {
                        AxonGUI.HelpBox("Please assign a target object other than this game object. MidiTween must remain on an active object to operate.", MessageType.Error);
                    }
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void ProcessingGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowProcessing = AxonGUI.Foldout(target.EditorShowProcessing, "Note Processing (ADSR)");
            if (target.EditorShowProcessing) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Processing Mode";
                AxonGUI.SetTooltip("Interpolate: each note interpolates between on and off values\n" +
                    "Increment: each note hit increments the value by a specified amount for progressive behaviors.");
                target.ProcessingMode = (MidiTween.ProcessingModes)AxonGUI.FieldEnumPopup(target, "Processing Mode", target.ProcessingMode);
                if (target.ProcessingMode == MidiTween.ProcessingModes.Increment) {
                    AxonGUI.UndoName = "Set Processing Max Steps";
                    AxonGUI.SetTooltip("This determines how many notes are played before resetting back to the start value. To never reset and continue incrementing, set the value to 0.");
                    target.OutputIncrementSteps = AxonGUI.FieldIntInline(target, "Max Steps", target.OutputIncrementSteps);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Velocity Mode";
                AxonGUI.SetTooltip("Determines how the midi note velocity is used (ie how loud or softly a note is played).\n\n " +
                    "Ignore:\nDon't use the note velocity and instead play all notes at the same intensity.\n\n " +
                    "Shorten Attack:\nUse this for a more instantaneous response when the midi note is played louder, or slower response for softly played notes.\n\n " +
                    "Scale Value:\nVelocity is used as a percentage to scale the output value. Use this to vary the intensity of notes based on how loudly or softly they are played.");
                target.VelocityMode = (MidiTween.VelocityModes)AxonGUI.FieldEnumPopup(target, "Velocity", target.VelocityMode);
                if (target.VelocityMode == MidiTween.VelocityModes.ScaleValue) {
                    AxonGUI.SetTooltip("The min and max range allow you to control how much the velocity affects the output scale.");
                    AxonGUI.UndoName = "Set Velocity Min";
                    target.VelocityMin = AxonGUI.FieldFloatInline(target, "Min", target.VelocityMin);

                    AxonGUI.UndoName = "Set Velocity Max";
                    target.VelocityMax = AxonGUI.FieldFloatInline(target, "Max", target.VelocityMax);
                }
                AxonGUI.EndHorizontal();

                if (target.ProcessingMode == MidiTween.ProcessingModes.Increment) {
                    target.Attack = 0f;
                    target.Decay = 0f;
                    target.Sustain = 1f;
                    target.SustainMax = 0f;
                }
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Note Attack";
                AxonGUI.SetTooltip("Sets the time in seconds it takes for the note to go from 0 to full intensity.");
                target.Attack = AxonGUI.FieldFloat(target, "Attack", target.Attack, GUILayout.Width(180));

                AxonGUI.UndoName = "Set Note Attack Ease";
                target.AttackEase = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.AttackEase, GUILayout.Width(100));

                AxonGUI.UndoName = "Set Note Anticipate";
                AxonGUI.SetTooltip("When enabled, the note attack will be applied in anticipation (ahead of the note hit time). Use this for tighter synchronization.");
                target.AnticipateAttack = AxonGUI.FieldToggleInline(target, "Anticipate", target.AnticipateAttack);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Note Decay";
                AxonGUI.SetTooltip("Sets the time in seconds after the attack to ramp down to the sustain value.");
                target.Decay = AxonGUI.FieldFloat(target, "Decay", target.Decay, GUILayout.Width(180));
                target.DecayEase = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.DecayEase, GUILayout.Width(100));
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Note Sustain";
                AxonGUI.SetTooltip("Sets the value held when sustaining a note, when the note is held on indefinitely. ");
                target.Sustain = AxonGUI.FieldSlider(target, "Sustain", target.Sustain, 0f, 1f);

                AxonGUI.UndoName = "Set Note Max Duration";
                AxonGUI.SetTooltip("Sets how long in seconds a note may be held on. If set to 0, no limit is enforced and a note may be on indefinitely.");
                target.SustainMax = AxonGUI.FieldFloatInline(target, "Max Duration", target.SustainMax, GUILayout.Width(120));
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Note Release";
                AxonGUI.SetTooltip("Sets the time in seconds it takes for the note to be fully off after it is released. Use this to fade or ramp down note hits gradually, or set the value to 0 to turn off each note instantly.");
                target.Release = AxonGUI.FieldFloat(target, "Release", target.Release, GUILayout.Width(180));
                target.ReleaseEase = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.ReleaseEase, GUILayout.Width(100));
                AxonGUI.EndHorizontal();

                if (target.Attack < 0) target.Attack = 0;
                if (target.Decay < 0) target.Decay = 0;
                if (target.Sustain < 0) target.Sustain = 0;
                if (target.Release < 0) target.Release = 0;
                if (target.Sustain > 1f) target.Sustain = 1f;

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void ValuesGUI()
        {
            if (target.SendOnly) return;
            AxonGUI.BeginBox();
            target.EditorShowValues = AxonGUI.Foldout(target.EditorShowValues, "Output Values");
            if (target.EditorShowValues) {
                AxonGUI.BeginBoxPadded();

                if (target.IsColor) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Color Off";
                    AxonGUI.SetTooltip("The color when the note is off, in resting state.");
                    target.ValueOffColor = AxonGUI.FieldColor(target, "Color Off", target.ValueOffColor, true);

                    AxonGUI.UndoName = "Set Color Off *";
                    AxonGUI.SetTooltip("An amount to scale the color value by. This can be used to adjust the intensity without changing the color.");
                    target.ValueOffMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOffMultiply, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    if (target.ProcessingMode == MidiTween.ProcessingModes.Interpolate) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Color On";
                        AxonGUI.SetTooltip("This is the color when the note is fully on.");
                        target.ValueOnColor = AxonGUI.FieldColor(target, "Color On", target.ValueOnColor, true);
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Color Increment";
                        AxonGUI.SetTooltip("How much color to increment by when each note is played.");
                        target.ValueOnColor = AxonGUI.FieldColor(target, "Color Increment", target.ValueOnColor, true);
                    }

                    AxonGUI.UndoName = "Set Color On *";
                    AxonGUI.SetTooltip("An amount to scale the color value by. This can be used to adjust the intensity without changing the color.");
                    target.ValueOnMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOnMultiply, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Reverse Values";
                    AxonGUI.SetTooltip("Enable to swap the start and end values to go in the opposite direction.");
                    target.ReverseValues = AxonGUI.FieldToggle(target, "Reverse", target.ReverseValues);
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.IsVector) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Vector Off";
                    AxonGUI.SetTooltip("The color when the note is off, in resting state.");
                    if (target.ToProperty.IsVector2) {
                        target.ValueOff = AxonGUI.FieldVector2(target, "Vector Off", target.ValueOff);
                    }
                    else
                    if (target.ToProperty.IsVector3) {
                        target.ValueOff = AxonGUI.FieldVector3(target, "Vector Off", target.ValueOff);
                    }
                    else {
                        target.ValueOff = AxonGUI.FieldVector4(target, "Vector Off", target.ValueOff);
                    }

                    AxonGUI.UndoName = "Set Vector Off *";
                    AxonGUI.SetTooltip("An amount to scale the color value by. This can be used to adjust the intensity without changing the color.");
                    target.ValueOffMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOffMultiply, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    if (target.ProcessingMode == MidiTween.ProcessingModes.Interpolate) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Vector On";
                        AxonGUI.SetTooltip("This is the color when the note is fully on.");
                        if (target.ToProperty.IsVector2) {
                            target.ValueOn = AxonGUI.FieldVector2(target, "Vector On", target.ValueOn);
                        }
                        else
                        if (target.ToProperty.IsVector3) {
                            target.ValueOn = AxonGUI.FieldVector3(target, "Vector On", target.ValueOn);
                        }
                        else {
                            target.ValueOn = AxonGUI.FieldVector4(target, "Vector On", target.ValueOn);
                        }
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Vector Increment";
                        AxonGUI.SetTooltip("How much Vector to increment by when each note is played.");
                        if (target.ToProperty.IsVector2) {
                            target.ValueOn = AxonGUI.FieldVector2(target, "Vector Increment", target.ValueOn);
                        }
                        else
                        if (target.ToProperty.IsVector3) {
                            target.ValueOn = AxonGUI.FieldVector3(target, "Vector Increment", target.ValueOn);
                        }
                        else {
                            target.ValueOn = AxonGUI.FieldVector4(target, "Vector Increment", target.ValueOn);
                        }
                    }

                    AxonGUI.UndoName = "Set Vector On *";
                    AxonGUI.SetTooltip("An amount to scale the Vector value by. This can be used to adjust the intensity without changing the Vector.");
                    target.ValueOnMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOnMultiply, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Reverse Values";
                    AxonGUI.SetTooltip("Enable to swap the start and end values to go in the opposite direction.");
                    target.ReverseValues = AxonGUI.FieldToggle(target, "Reverse", target.ReverseValues);
                    AxonGUI.EndHorizontal();
                }
                else {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Value Off";
                    AxonGUI.SetTooltip("This sets the resting value, when the note is off. The target property remains set to this value until a note is played.");
                    target.ValueOffFloat = AxonGUI.FieldFloat(target, "Value Off", target.ValueOffFloat);

                    AxonGUI.UndoName = "Set Value Off *";
                    AxonGUI.SetTooltip("An amount to scale the value by. This can be used to adjust the overall intensity without changing the min max values.");
                    target.ValueOffMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOffMultiply);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    if (target.ProcessingMode == MidiTween.ProcessingModes.Interpolate) {
                        AxonGUI.UndoName = "Set Value On";
                        AxonGUI.SetTooltip("This is the value when the note is fully on. Interpolation is controlled by the attack, decay, sustain, and release.");
                        target.ValueOnFloat = AxonGUI.FieldFloat(target, "Value On", target.ValueOnFloat);
                    }
                    else {
                        AxonGUI.UndoName = "Set Value Increment";
                        AxonGUI.SetTooltip("This sets how much to increment the value each time the note is on. The total amount is still controlled by the attack, decay, sustain, and release.");
                        target.ValueOnFloat = AxonGUI.FieldFloat(target, "Value Increment", target.ValueOnFloat);
                    }
                    AxonGUI.UndoName = "Set Value On *";
                    AxonGUI.SetTooltip("An amount to scale the value by. This can be used to adjust the overall intensity without changing the min max values.");
                    target.ValueOnMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOnMultiply);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Reverse Values";
                    AxonGUI.SetTooltip("Enable to swap the start and end values to go in the opposite direction.");
                    target.ReverseValues = AxonGUI.FieldToggle(target, "Reverse", target.ReverseValues);
                    AxonGUI.EndHorizontal();
                }

                if (target.NoteMode != MidiTween.NoteModes.Single) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Note Offset";
                    AxonGUI.SetTooltip("This offsets the output value by a certain amount based on the which note is played. " +
                        "For example, this can be used to represent each note on a scale with an increasing intensity. " +
                        "When using Polyphonic mode, the offset is based on the last note played.");
                    target.NoteOffset = AxonGUI.FieldToggle(target, "Note Offset", target.NoteOffset);
                    if (target.NoteOffset) {
                        AxonGUI.UndoName = "Set Note Offset Relative";
                        AxonGUI.SetTooltip("Relative mode calculates the offset starting with the lowest note as the base (no offset). Each note played applies an increment based on how many steps away the note is from the lowest note. If this option is disabled, each raw note value is multiplied by the increment value.");
                        target.NoteRelative = AxonGUI.FieldToggleInline(target, "Relative", target.NoteRelative);

                        AxonGUI.UndoName = "Set Note Offset Increment";
                        AxonGUI.SetTooltip("This specifies how much to offset each half step (correlating to each note step in midi). This value is in the units of the target property selected. When assigning a float value, the increment is applied additively for each note, whereas when assigning a color the increment is multiplied by the note index.");
                        target.NoteIncrement = AxonGUI.FieldFloatInline(target, "Increment", target.NoteIncrement);
                    }
                    AxonGUI.EndHorizontal();
                }


                AxonGUI.UndoName = "Set Amount";
                AxonGUI.SetTooltip("This determines how much of the value is applied. This can be used to diminish or turn off the reaction to midi input.");
                target.Amount = AxonGUI.FieldSlider(target, "Amount", target.Amount, 0f, 1f);

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Override Enabled";
                AxonGUI.SetTooltip("Enable to partially or fully to override the output values. This can be used to modify or overwrite the values calculated from midi input.");
                target.EnableOverride = AxonGUI.FieldToggle(target, "Override", target.EnableOverride);
                if (target.EnableOverride) {
                    if (target.IsColor) {
                        target.OverrideValue = AxonGUI.FieldColorInline(target, target.OverrideValue, false);
                    }
                    else
                    if (target.IsFloat) {
                        target.OverrideValue.x = AxonGUI.FieldFloatInline(target, target.OverrideValue.x);
                    }
                    else {
                        if (target.ToProperty.IsVector2) {
                            target.OverrideValue = AxonGUI.FieldVector2Inline(target, target.OverrideValue);
                        }
                        else
                        if (target.ToProperty.IsVector3) {
                            target.OverrideValue = AxonGUI.FieldVector3Inline(target, target.OverrideValue);
                        }
                        else {
                            target.OverrideValue = AxonGUI.FieldVector4Inline(target, target.OverrideValue);
                        }
                    }
                }
                AxonGUI.EndHorizontal();

                if (target.EnableOverride) {
                    AxonGUI.UndoName = "Set Override Blend";
                    AxonGUI.SetTooltip("Use this to blend the override value with the output values calculated from the midi. This slider could be animated or controlled by another script to override the output for a particular section or via trigger in a runtime environment.");
                    target.OverrideBlend = AxonGUI.FieldSlider(target, "Override Blend", target.OverrideBlend, 0f, 1f);
                    AxonGUI.Space();
                }

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("This displays the final calculated value, which is applied to the target object(s)");
                if (target.IsColor) {
                    AxonGUI.FieldColor(target, "Final Value", target.OutputValue, true);
                }
                else
                if (target.IsFloat) {
                    AxonGUI.FieldFloat(target, "Final Value", target.OverrideValue.x);
                }
                else {
                    if (target.ToProperty.IsVector2) {
                        AxonGUI.FieldVector2(target, "Final Value", target.OutputValue);
                    }
                    else
                    if (target.ToProperty.IsVector3) {
                        AxonGUI.FieldVector3(target, "Final Value", target.OutputValue);
                    }
                    else {
                        AxonGUI.FieldVector4(target, "Final Value", target.OutputValue);
                    }
                }

                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void ObjectsGUI()
        {
            AxonGUI.BeginBoxPadded();

            if (target.MidiChannels == null) target.MidiChannels = new List<MidiTweenChannel>();

            EditorGUI.indentLevel++;

            AxonGUI.BeginHorizontal();

            if (AxonGUI.ButtonInline("Gather Children")) {
                target.GatherChildren(true);
            }
            if (AxonGUI.ButtonInline("Add Selected")) {
                target.AddSelected();
            }
            if (isRenaming) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonInline("Cancel")) {
                    isRenaming = false;
                }
                baseName = AxonGUI.FieldTextInline(target, "Name", baseName);
                if (AxonGUI.ButtonInline("Apply")) {
                    int x = 0;
                    foreach (Transform child in target.transform) {
                        UndoUtil.Undo(child, "Rename Children");
                        child.name = baseName + StringUtil.PadNumber2(x + 1);
                        x++;
                    }
                    isRenaming = false;
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }
            else {
                if (AxonGUI.ButtonInline("Rename")) {
                    isRenaming = true;
                }
            }
            if (AxonGUI.ButtonInline("Clear All")) {
                target.ClearChannels();
            }
            if (AxonGUI.ButtonInline("Add Channel")) {
                target.AddChannel();
            }
            AxonGUI.EndHorizontal();
            AxonGUI.Space();

            int i = 0;
            int remove = -1;
            //int insert = -1;
            foreach (MidiTweenChannel obj in target.MidiChannels) {
                if (obj.ToProperty == null) obj.ToProperty = new Property();
                if (obj.ToProperty.Comp == null) obj.ToProperty.Comp = target.transform;
                if (obj.Object == null) obj.Object = target.ParentObject;

                AxonGUI.BeginBoxPadded();
                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Channel")) {
                    remove = i;
                }
                if (target.SendOnly) {
                    AxonGUI.SetTooltip("The component to receive the 'OnMidiNote' message. Component class must implement method OnMidiNote(float time).");
                    obj.ToProperty.Comp = (Component)AxonGUI.FieldObjectInline(target, obj.ToProperty.Comp, typeof(Component), true);
                }
                else {
                    AxonGUI.PropertySelect(obj.Behavior, typeof(MidiTween), obj.Object.gameObject, obj.ToProperty, Property.PropertyFilters.NumericOnly, "Property", true, true);
                }

                if (target.NoteMode != MidiTween.NoteModes.SequenceTargets) {
                    AxonGUI.EndHorizontal();
                    AxonGUI.BeginHorizontalIndent();
                    obj.NoteMode = (MidiTweenChannel.NoteModes)AxonGUI.FieldEnumPopupInline(target, "Notes", obj.NoteMode);
                    if (obj.NoteMode == MidiTweenChannel.NoteModes.Range) {
                        obj.NoteMin = AxonGUI.FieldIntInline(target, "Note Min", obj.NoteMin);
                        obj.NoteMax = AxonGUI.FieldIntInline(target, "Note Max", obj.NoteMax);
                    }
                    else
                    if (obj.NoteMode == MidiTweenChannel.NoteModes.Single) {
                        obj.NoteMin = obj.NoteMax = AxonGUI.FieldIntInline(target, "Note", obj.NoteMin);
                    }
                    else {
                        obj.NoteMin = 0;
                        obj.NoteMax = 256;
                    }
                    AxonGUI.SetTooltip("Any midi notes with a lower velocity than specified (ie played too softly) are ignored.");
                    obj.MinVelocity = AxonGUI.FieldSliderInline(obj.Behavior, "Min Velocity", obj.MinVelocity, 0f, 1f);

                    if (obj.NoteMin < 0) obj.NoteMin = 0;
                    if (obj.NoteMax < 0) obj.NoteMax = 0;
                    if (obj.NoteMin > 127) obj.NoteMin = 127;
                    if (obj.NoteMax > 127) obj.NoteMax = 127;
                    if (obj.MinVelocity < 0) obj.MinVelocity = 0;
                    if (obj.MinVelocity > 1f) obj.MinVelocity = 1f;
                }

                AxonGUI.EndHorizontal();
                AxonGUI.EndBoxPadded();
                i++;
            }
            if (remove > -1) {
                UndoUtil.Undo(target, "Remove Object");
                target.RemoveChannel(target.MidiChannels[remove]);
            }
            EditorGUI.indentLevel--;

            AxonGUI.Space();

            AxonGUI.EndBoxPadded();
        }

        public void AdvancedGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowAdvanced = AxonGUI.Foldout(target.EditorShowAdvanced, "Advanced Settings");
            if (target.EditorShowAdvanced) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Assign an audio clip to play in response to midi notes. ");
                target.Audio = (AudioSource)AxonGUI.FieldObject(target, "Play Audio", target.Audio, typeof(AudioSource), true);
                if (target.Audio != null) {
                    AxonGUI.SetTooltip("One Shot: play the whole audio clip each time a note is hit.\n " +
                        "Sync Track: treats the audio as a track layer, using midi notes to control the audio on-off keeping the audio synced.\n " +
                        "Resume: play the audio clip while a midi note is on, resuming from where it last left off.");
                    target.AudioMode = (MidiTween.AudioModes)AxonGUI.FieldEnumPopupInline(target, "Playback", target.AudioMode, GUILayout.Width(140));
                    AxonGUI.LabelInline(target.IsNoteOn ? "ON" : "OFF", "");
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                target.SetShaderProperty = AxonGUI.FieldToggle(target, "Set Shader Value", target.SetShaderProperty);
                if (target.SetShaderProperty) {
                    target.ShaderPropertyName = AxonGUI.FieldTextInline(target, "Name", target.ShaderPropertyName);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("When Send Message is enabled, each midi note sends a message 'OnMidiNote' to the object. Scripts can implement OnMidiNote(object note) to drive custom behavior. Send Message only works in runtime.");
                target.Send = AxonGUI.FieldToggle(target, "Send Message", target.Send);
                if (target.Send) {
                    AxonGUI.SetTooltip("Enable this to only send messages from this MidiTween and not apply interpolations.");
                    target.SendOnly = AxonGUI.FieldToggleInline(target, "Send Only", target.SendOnly);

                    AxonGUI.SetTooltip("Enable this to send the message to all child objects. Beware that excessive messaging can degrade performance.");
                    target.SendBroadcast = AxonGUI.FieldToggleInline(target, "Broadcast", target.SendBroadcast);

                    AxonGUI.SetTooltip("If enabled, messages are only sent during runtime and ignored in edit mode.");
                    target.SendRuntimeOnly = AxonGUI.FieldToggleInline(target, "Runtime Only", target.SendRuntimeOnly);
                }
                else target.SendOnly = false;
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("The allows another script to take control over this instance of MidiTween, overriding the output value. This is used by MidiReceiver to map live midi input to MidiTween.");
                target.EnableRemoteControl = AxonGUI.FieldToggle(target, "Remote Control", target.EnableRemoteControl);
                if (target.EnableRemoteControl) {
                    target.EnableRemotePassThru = AxonGUI.FieldToggleInline(target, "Pass Thru", target.EnableRemotePassThru);
                    target.RemoteValue = AxonGUI.FieldFloatInline(target, "Value", target.RemoteValue);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        public void ExportGUI()
        {
            showExportGUI = AxonGUI.Foldout(showExportGUI, "Export Data to Clipboard");
            if (showExportGUI) {
                if (GUILayout.Button("Copy Data", GUILayout.Width(100))) {
                    target.CopyData(false);
                    Debug.Log("Data copied to clipboard");//--KEEP
                }
                if (GUILayout.Button("Copy Time", GUILayout.Width(100))) {
                    target.CopyData(true);
                    Debug.Log("Time data copied to clipboard");//--KEEP
                }
            }
        }


#if AXON_EXPERIMENTAL
        public override void GUIDropDown(ref GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Copy to Clipboard for After Effects"), false, CopyToAE, (object)target);
        }

        public static void CopyToAE(object obj)
        {
            MidiTween m = (MidiTween)obj;
            if (m != null) {
                m.CopyToAE();
            }
        }
#endif

    }

}//AxonGenesis

#endif
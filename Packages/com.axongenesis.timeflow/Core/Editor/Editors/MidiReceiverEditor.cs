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
using UnityEngine.UI;

namespace AxonGenesis
{
    [CustomEditor(typeof(MidiReceiver))]
    public class MidiReceiverEditor : AxonGenesisEditor<MidiReceiver, MidiReceiverEdit> { }

    sealed public class MidiReceiverEdit : AxonGenesisBehaviorEdit<MidiReceiver>
    {
#if TIMEFLOW_PRO
        public const string kAddMidiReceiver = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "📡 MIDI Receiver";
#else
        public const string kAddMidiReceiver = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "MIDI Receiver";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: MIDI Receiver";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddMidiReceiver, false, 181)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddMidiReceiver, false, 181)]
        public static void AddMidiReceiver()
        {
            ObjectUtil.GetOrAddComponent<MidiReceiver>(TimeflowMenu.GetSelectedOrNewGameObject("MIDI Receiver"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;
        public bool showExportGUI = true;

        SerializedProperty NoteOnEvent;
        SerializedProperty NoteOffEvent;
        SerializedProperty KnobChangedEvent;

        public MidiReceiverEdit() { }
        public MidiReceiverEdit(MidiReceiver _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-receiver";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            if (target.ToChannel.ToProperty == null) target.ToChannel.ToProperty = new Property();
            if (target.ToChannel.ToProperty.Comp == null) target.ToChannel.ToProperty.Comp = target;

            NoteOnEvent = editor.serializedObject.FindProperty("NoteOnEvent");
            NoteOffEvent = editor.serializedObject.FindProperty("NoteOffEvent");
            KnobChangedEvent = editor.serializedObject.FindProperty("KnobChangedEvent");
        }

        public override void GUIMenu()
        {
            GUI.color = target.IsNoteOn ? AxonColor.MidiNoteOn : AxonColor.Default;
            if (target.IsMapping) {
                GUI.color = Color.green;
            }

            if (Application.isPlaying) {
                if (AxonGUI.ButtonInline("Exit play mode")) {
                    EditorApplication.ExitPlaymode();
                }
#if MINIS || MIDIJACK
                AxonGUI.LabelInline("Input: " + MidiReceiver.LastType + ":" + (MidiReceiver.LastType == MidiReceiver.InputTypes.Note ? MidiReceiver.LastNote : MidiReceiver.LastKnob) +
                " (" + MidiReceiver.LastVelocity + ")", " " + MidiReceiver.LastChannel);
#endif
            }
            else {
                AxonGUI.SetTooltip("Midi signals can only be detected at runtime when MidiJack is installed.");
                if (!Application.isPlaying) {
                    if (AxonGUI.ButtonInline("Enter play mode for input")) {
                        EditorApplication.EnterPlaymode();
                    }
                }
                GUI.color = AxonColor.Default;
            }
            if (target.IsMapping) {
                AxonGUI.SetTooltip("Press done when you are finished mapping to return to normal midi input.");
                if (AxonGUI.ButtonInline("Done")) {
                    target.StopMapping();
                }
            }
            else {
                AxonGUI.SetTooltip("Save the current mapping to PlayerPrefs. The current game object name '" + target.gameObject.name + "' is used as the key.");
                if (AxonGUI.ButtonInline("Save")) {
                    target.SaveConfig();
                }
                AxonGUI.SetTooltip("Load the note mapping from PlayerPrefs. The current game object name '" + target.gameObject.name + "' is used as the key.");
                if (AxonGUI.ButtonInline("Load")) {
                    target.LoadConfig();
                }

                AxonGUI.UndoName = "Set Auto Load and Save";
                AxonGUI.SetTooltip("Automatically saves and loads configuration from PlayerPrefs. In play mode, configuration is loaded OnAwake and saved OnDestruct.");
                target.AutoLoadAndSave = AxonGUI.FieldToggleInline(target, "Auto", target.AutoLoadAndSave);

                AxonGUI.SetTooltip("Press this button during play mode to enter listening mode. The first midi key pressed (on keyboard or other device) is captured and mapped to this receiver.");
                if (AxonGUI.ButtonInline("Map")) {
                    target.StartMapping();
                }
            }
            GUI.color = AxonColor.Default;
        }

        [UnityEditor.MenuItem("CONTEXT/MidiReceiver/Configure MidiJack")]
        public static void ConfigureMidiJack()
        {
            if (EditorUtility.DisplayDialog("MidiJack Setup", "To activate MidiJack with Timeflow, the Scripting Define Symbols (in Player Settings) need to be updated by adding the symbol MIDIJACK. Click Continue to automatically set it up and recompile scripts. ", "Continue")) {
                EditorScriptingDefineUtils.AddScriptingDefineSymbol("MIDIJACK");
            }
        }

        [UnityEditor.MenuItem("CONTEXT/MidiReceiver/Configure Minis")]
        public static void ConfigureMinis()
        {
            if (EditorUtility.DisplayDialog("Minis Setup", "To activate Minis with Timeflow, the Scripting Define Symbols (in Player Settings) need to be updated by adding the symbol MINIS. Click Continue to automatically set it up and recompile scripts. ", "Continue")) {
                EditorScriptingDefineUtils.AddScriptingDefineSymbol("MINIS");
            }
        }

        public override void OnInspectorGUI()
        {
#if MINIS || MIDIJACK
#else
            GUI.backgroundColor = AxonColor.Warning;
            AxonGUI.BeginBox();
            GUI.backgroundColor = Color.white;
            AxonGUI.HelpBox("For MIDI device support, one of the free 3rd party plugins MidiJack or Minis by Keijiro is required. MidiJack is an older system no longer being updated, while Minis is the newer system using Unity's Input System. Select 1 of these options depending on your target platform.", MessageType.Warning);
            AxonGUI.BeginHorizontal();
            AxonGUI.Space();

            if (EditorScriptingDefineUtils.NamespaceExists("Minis")) {
                if (AxonGUI.ButtonInline("Setup Minis")) {
                    ConfigureMinis();
                }
            }
            else {
                if (AxonGUI.ButtonInline("Install Minis")) {
                    Application.OpenURL("https://github.com/keijiro/Minis");
                }
            }
            if (EditorScriptingDefineUtils.NamespaceExists("MidiJack")) {
                if (AxonGUI.ButtonInline("Setup MidiJack")) {
                    ConfigureMidiJack();
                }
            }
            else {
                if (AxonGUI.ButtonInline("Import MidiJack")) {
                    EditorUtility.DisplayDialog("Import MidiJack", "Please locate Timeflow in the Package Manager and import MidiJack from the Samples", "Ok");
                }
            }
            if (AxonGUI.ButtonInline("About MidiJack")) {
                Application.OpenURL("https://github.com/keijiro/MidiJack");
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();
            AxonGUI.Space();
#endif

            ConfigGUI();
            ProcessingGUI();
            MappingGUI();
            EventsGUI();

            behaviorUI.MainGUI();

            editor.serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                if (target.AutoLoadAndSave) {
                    target.SaveConfig();
                }
                target.Setup();
            }
        }

        private void ConfigGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowConfig = AxonGUI.Foldout(target.EditorShowConfig, "Configuration");
            if (target.EditorShowConfig) {
                AxonGUI.BeginBoxPadded();

#if AXON_EXPERIMENTAL
                // doesn't work properly
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Instead of a midi device a standard keyboard key can be used. This does not require MidiJack to work.");
                AxonGUI.UndoName = "Set Use Keyboard Input";
                target.UseKeyboardInput = AxonGUI.Toggle("Use Keyboard Input", target.UseKeyboardInput);
                if (target.UseKeyboardInput) {
                    AxonGUI.UndoName = "Set Key Input Code";
                    target.KeyInputCode = (KeyCode)AxonGUI.FieldEnumPopupInline(target.KeyInputCode);
                }
                AxonGUI.EndHorizontal();
#endif
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Channel";
#if MINIS
                AxonGUI.SetTooltip("Select the midi channel you want to receive input from.");
                target.Channel = AxonGUI.FieldInt(target, "Channel", target.Channel);
#elif MIDIJACK
                AxonGUI.SetTooltip("Select the midi channel you want to receive input from.");
                target.Channel = (MidiJack.MidiChannel)AxonGUI.FieldEnumPopup(target, "Channel", target.Channel);
#else
                AxonGUI.Label("Channel", GUILayout.Width(AxonGUI.LabelWidth));
#endif
                AxonGUI.UndoName = "Set Input Type";
                AxonGUI.SetTooltip("Specifies the type of control input to receive.");
                target.InputType = (MidiReceiver.InputTypes)AxonGUI.FieldEnumPopupInline(target, "Type", target.InputType);
                if (target.InputType == MidiReceiver.InputTypes.Knob) {
                    AxonGUI.UndoName = "Set Knob Number";
                    AxonGUI.SetTooltip("Designates the index number of a midi knob control to receive data from.");
                    target.KnobNumber = AxonGUI.FieldIntInline(target, "Knob Number", target.KnobNumber);

                    AxonGUI.UndoName = "Set Knob Multiplier";
                    AxonGUI.SetTooltip("Use the multiplier to amplify or reduce the strength of the signal from the midi device. This can help correct knobs which register very low or high value ranges.");
                    target.KnobMultiplier = AxonGUI.FieldFloatInline(target, "Multiplier", target.KnobMultiplier);
                }
                else {
                    AxonGUI.UndoName = "Set Note Mode";
                    AxonGUI.SetTooltip("Select an option to play all notes, a range of notes, or a single note. All other midi signals are ignored.");
                    target.NoteMode = (MidiReceiver.NoteModes)AxonGUI.FieldEnumPopupInline(target, target.NoteMode);
                    if (target.NoteMode == MidiReceiver.NoteModes.Range) {
                        AxonGUI.UndoName = "Set Note Range Min";
                        target.MinNote = AxonGUI.FieldIntInline(target, "Min", target.MinNote);

                        AxonGUI.UndoName = "Set Note Range Max";
                        target.MaxNote = AxonGUI.FieldIntInline(target, "Max", target.MaxNote);
                    }
                    else
                    if (target.NoteMode == MidiReceiver.NoteModes.Single) {
                        AxonGUI.UndoName = "Set Note";
                        target.MinNote = AxonGUI.FieldIntInline(target, "Note", target.MinNote);
                    }
                    AxonGUI.UndoName = "Set All Octaves";
                    target.AllOctaves = AxonGUI.FieldToggleInline(target, "All Octaves", target.AllOctaves);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Program Button";
                AxonGUI.SetTooltip("(Optional) Assign a UI Button to set mapping in play mode. The button press event should call the method StartMapping if you want it to learn when pressed. In the editor you can begin mapping by simply pressing the Map button above.");
                target.ProgramButton = (Button)AxonGUI.FieldObject(target, "Program Button", target.ProgramButton, typeof(Button), true);
                if (target.ProgramButton != null) {
                    AxonGUI.UndoName = "Set Update Label";
                    AxonGUI.SetTooltip("If enabled, the button text label updates to show the current mapped midi note number and turns green any time a note is detected (in play mode only)");
                    target.UpdateButtonLabel = AxonGUI.FieldToggleInline(target, "Update Label", target.UpdateButtonLabel);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set PlayerPrefs Name";
                AxonGUI.SetTooltip("Sets the basename for values saved in PlayerPrefs. See the code MidiReceiver.SaveConfig() for more info.");
                target.ConfigName = AxonGUI.FieldText(target, "PlayerPrefs Name", target.ConfigName);
                if (AxonGUI.ButtonInline("Erase Configuration")) {
                    target.EraseConfig();
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set QWERTY Input";
                AxonGUI.SetTooltip("If enabled, keyboard keystrokes can be mapped as virtual midi input. Key input only has on and off value so there is no velocity to control note amplitude and all notes are played at 100%.");
                target.UseKeyboardInput = AxonGUI.FieldToggle(target, "QWERTY Input", target.UseKeyboardInput);

                AxonGUI.UndoName = "Set Input Key Code";
                AxonGUI.SetTooltip("Assign the key manually, or enter Map mode and press any key on the keyboard to record the mapping.");
                target.InputKeyCode = (KeyCode)AxonGUI.FieldEnumPopupInline(target, target.InputKeyCode);

                AxonGUI.UndoName = "Set Any Key";
                AxonGUI.SetTooltip("If enabled, any key on the keyboard pressed will act as input.");
                target.UseAnyKey = AxonGUI.FieldToggleInline(target, "Any Key", target.UseAnyKey);
                AxonGUI.EndHorizontal();

                if (target.IsMapping) {
                    GUI.backgroundColor = AxonColor.Warning;
                    AxonGUI.HelpBox("Waiting for midi event to learn...", MessageType.Info);
                    GUI.backgroundColor = Color.white;
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void MappingGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowMapping = AxonGUI.Foldout(target.EditorShowMapping, "Mapping");
            if (target.EditorShowMapping) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Map To";
                AxonGUI.SetTooltip("Select how midi data should be routed.");
                target.MapMode = (MidiReceiver.MapModes)AxonGUI.FieldEnumPopup(target, "Map To", target.MapMode);
                AxonGUI.EndHorizontal();

                if (target.MapMode == MidiReceiver.MapModes.Property) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Select a target property to apply interpolated values to. ");
                    AxonGUI.PropertySelect(target, typeof(MidiReceiver), target.gameObject, target.ToChannel.ToProperty, Property.PropertyFilters.NumericOnly, "Apply To", true, false);
                    AxonGUI.EndHorizontal();

                    if (target.ToChannel.ToProperty.IsColor) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Color Off";
                        AxonGUI.SetTooltip("The color when the note is off, in resting state.");
                        target.ValueOffColor = AxonGUI.FieldColor(target, "Color Off", target.ValueOffColor, true);

                        AxonGUI.UndoName = "Set Color Off *";
                        AxonGUI.SetTooltip("An amount to scale the color value by. This is a way to adjust the intensity without changing the color.");
                        target.ValueOffMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOffMultiply, GUILayout.Width(80));
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Color On";
                        AxonGUI.SetTooltip("This is the color when the note is fully on.");
                        target.ValueOnColor = AxonGUI.FieldColor(target, "Color On", target.ValueOnColor, true);

                        AxonGUI.UndoName = "Set Color On *";
                        AxonGUI.SetTooltip("An amount to scale the color value by. This can be used to adjust the intensity without changing the color.");
                        target.ValueOnMultiply = AxonGUI.FieldFloatInline(target, "*", target.ValueOnMultiply, GUILayout.Width(80));
                        AxonGUI.EndHorizontal();
                    }
                    else
                    if (target.ToChannel.ToProperty.IsVector && target.ToChannel.ToProperty.IsCombinedValue) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Vector Min";
                        AxonGUI.SetTooltip("The value when the note is off, in resting state.");
                        target.ValueOff = AxonGUI.FieldVector4(target, "Vector Min", target.ValueOff);
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Vector Max";
                        AxonGUI.SetTooltip("This is the value when the note is fully on.");
                        target.ValueOn = AxonGUI.FieldVector4(target, "Vector Max", target.ValueOn);
                        AxonGUI.EndHorizontal();
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Value Min";
                        AxonGUI.SetTooltip("The value when the note is off, in resting state.");
                        target.ValueOffFloat = AxonGUI.FieldFloat(target, "Value Min", target.ValueOffFloat);
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Value Max";
                        AxonGUI.SetTooltip("This is the value when the note is fully on.");
                        target.ValueOnFloat = AxonGUI.FieldFloat(target, "Value Max", target.ValueOnFloat);
                        AxonGUI.EndHorizontal();

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Value Scale";
                        AxonGUI.SetTooltip("Multiplies the final value to control overall intensity.");
                        target.ValueOffMultiply = AxonGUI.FieldFloat(target, "Value Scale", target.ValueOffMultiply);
                        AxonGUI.EndHorizontal();
                    }

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Reverse";
                    AxonGUI.SetTooltip("Enable Invert to swap the start and end values to interpolate in the opposite direction.");
                    target.ReverseValues = AxonGUI.FieldToggle(target, "Reverse", target.ReverseValues);
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.MapMode == MidiReceiver.MapModes.Tween) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Tween";
                    AxonGUI.SetTooltip("Designate an instance of Tween to receive the midi data. The Tween component is automatically set to Remote Control mode to receive input.");
                    target.Tween = (Tween)AxonGUI.FieldObject(target, "Tween", target.Tween, typeof(Tween), true);

                    AxonGUI.UndoName = "Set Tween Enabled";
                    AxonGUI.SetTooltip("This toggles the Remote Control mode, effectively turning on and off the live midi input. This way the Tween can have its own default behavior while midi is off.");
                    target.TweenEnabled = AxonGUI.FieldToggleInline(target, "Enabled", target.TweenEnabled);
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.MapMode == MidiReceiver.MapModes.MidiTween) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set MIDI Tween";
                    AxonGUI.SetTooltip("Map input from this midi receiver to an instance of MidiTween, which is automatically set to Remote Control mode.");
                    target.MidiTween = (MidiTween)AxonGUI.FieldObject(target, "MIDI Tween", target.MidiTween, typeof(MidiTween), true);

                    AxonGUI.UndoName = "Set MIDI Tween Enabled";
                    AxonGUI.SetTooltip("Toggles the midi receiver input on and off, returning the MidiTween to its default beahvior when off.");
                    target.MidiTweenEnabled = AxonGUI.FieldToggleInline(target, "Enabled", target.MidiTweenEnabled);
                    AxonGUI.EndHorizontal();
                }

                if (target.MapMode != MidiReceiver.MapModes.TriggerOnly) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Amount";
                    AxonGUI.SetTooltip("Mutliplies the final output value and can be used to fade or turn off midi input.");
                    target.Amount = AxonGUI.FieldSlider(target, "Amount", target.Amount, 0f, 1f);
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("The final calculated interpolation value");
                    if (target.ToChannel.ToProperty != null) {
                        if (target.ToChannel.ToProperty.IsColor) {
                            target.OutputValue = AxonGUI.FieldColor(target, "Final Value", (Color)target.OutputValue, true);
                        }
                        else
                        if (target.ToChannel.ToProperty.IsVector && target.ToChannel.ToProperty.IsCombinedValue) {
                            if (target.ToChannel.ToProperty.IsVector2) {
                                target.OutputValue = AxonGUI.FieldVector2(target, "Final Value", target.OutputValue);
                            }
                            else
                            if (target.ToChannel.ToProperty.IsVector3) {
                                target.OutputValue = AxonGUI.FieldVector3(target, "Final Value", target.OutputValue);
                            }
                            else {
                                target.OutputValue = AxonGUI.FieldVector4(target, "Final Value", target.OutputValue);
                            }
                        }
                        else {
                            target.OutputValueFloat = AxonGUI.FieldFloat(target, "Final Value", target.OutputValueFloat);
                        }
                    }
                    AxonGUI.EndHorizontal();
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

                if (target.InputType == MidiReceiver.InputTypes.Knob) {
                    AxonGUI.HelpBox("When using a knob as midi input, the knob value is used directly to interpolate between the off and on value.", MessageType.Info);
                }
                else {
                    AxonGUI.BeginBox();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Note Velocity";
                    AxonGUI.SetTooltip("Determines how the midi note velocity is used (ie how loud or softly a note is played).\n\n " +
                        "Ignore:\nDon't use the note velocity and instead play all notes at the same intensity.\n\n " +
                        "Shorten Attack:\nUse this for a more instantaneous response when the midi note is played louder.\n\n " +
                        "Scale Value:\nVelocity is used as a percentage to scale the output value. Use this to vary the intensity of notes based on how loudly or softly they are played.");
                    target.VelocityMode = (MidiReceiver.VelocityModes)AxonGUI.FieldEnumPopup(target, "Velocity", target.VelocityMode);
                    if (target.VelocityMode == MidiReceiver.VelocityModes.ScaleValue) {
                        AxonGUI.SetTooltip("The min and max range allow you to control how much the velocity affects the output scale.");
                        AxonGUI.UndoName = "Set Note Velocity Min";
                        target.VelocityMin = AxonGUI.FieldFloatInline(target, "Min", target.VelocityMin);

                        AxonGUI.UndoName = "Set Note Velocity Max";
                        target.VelocityMax = AxonGUI.FieldFloatInline(target, "Max", target.VelocityMax);
                    }
                    AxonGUI.UndoName = "Set Note Polyphonic";
                    AxonGUI.SetTooltip("When enabled, midi notes play cumulatively for smoother interpolation. Otherwise, each note played restarts interpolation from the min value, interrupting any previously played notes.");
                    target.Polyphonic = AxonGUI.FieldToggleInline(target, "Polyphonic", target.Polyphonic);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Note Attack";
                    AxonGUI.SetTooltip("Sets the time in seconds it takes for the note to go from 0 to full intensity.");
                    EditorGUI.BeginDisabledGroup(target.Instant);
                    if (!target.Instant) {
                        target.Attack = AxonGUI.FieldFloat(target, "Attack", target.Attack, GUILayout.Width(180));
                        target.AttackEase = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.AttackEase, GUILayout.Width(100));
                    }
                    else {
                        AxonGUI.FieldFloat(target, "Attack", 0f, GUILayout.Width(180));
                    }
                    EditorGUI.EndDisabledGroup();
                    AxonGUI.UndoName = "Set Note Instant";
                    AxonGUI.SetTooltip("When enabled, the note attack is applied in anticipation (ahead of the note hit time). Use this for tighter synchronization.");
                    target.Instant = AxonGUI.FieldToggleInline(target, "Instant", target.Instant);

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
                    target.Sustain = AxonGUI.FieldFloat(target, "Sustain", target.Sustain, GUILayout.Width(180));

                    AxonGUI.UndoName = "Set Note Max Duration";
                    AxonGUI.SetTooltip("Sets how long in seconds a note may be held on for. If set to 0, no limit is enforced and a note may be on indefinitely.");
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

                    AxonGUI.EndBox();
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void EventsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowEvents = AxonGUI.Foldout(target.EditorShowEvents, "Events");
            if (target.EditorShowEvents) {
                AxonGUI.BeginBoxPadded();

                if (target.InputType == MidiReceiver.InputTypes.Knob) {
                    EditorGUILayout.PropertyField(KnobChangedEvent, new GUIContent("Knob Changed Event"));
                }
                else {
                    EditorGUILayout.PropertyField(NoteOnEvent, new GUIContent("Note On Event"));
                    EditorGUILayout.PropertyField(NoteOffEvent, new GUIContent("Note Off Event"));
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

    }

}//AxonGenesis

#endif

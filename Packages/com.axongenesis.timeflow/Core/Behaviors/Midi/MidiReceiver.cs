// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if MINIS
using Minis;
#elif MIDIJACK
using MidiJack;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This takes live midi signal input and maps it to a specific behavior. This can be used to drive
    /// behaviors of Tween, MidiTween, or any property value. This requires an external midi device and the
    /// 3rd party Unity extension MidiJack to operate. MIDIJACK must be added in the Project Settings >
    /// Player > Scripting Define Symbols
    /// </summary>

    [ExecuteInEditMode]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/MIDI Receiver")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-receiver")]
    sealed public class MidiReceiver : TimeflowBehavior
    {
        #region STATIC

        public static InputTypes LastType { get; private set; }
        public static int LastNote { get; private set; }
        public static int LastKnob { get; private set; }
        public static float LastVelocity { get; private set; }

#if MINIS
        public static int? LastChannel = 0;
#elif MIDIJACK
        public static MidiChannel LastChannel = MidiChannel.All;
#endif
        #endregion

        #region PUBLIC
#if MINIS
        public int Channel = 0;
#elif MIDIJACK
        public MidiChannel Channel = MidiChannel.All;
#endif

        public enum InputTypes
        {
            None,
            Note,
            Knob
        }
        public InputTypes InputType = InputTypes.Note;
        public int KnobNumber = 0;
        public float KnobMultiplier = 1f;

        public enum NoteModes
        {
            Any,
            Single,
            Range
        }
        public NoteModes NoteMode = NoteModes.Single;
        public int MinNote = -1;
        public int MaxNote = -1;
        public bool AllOctaves = false;

        public MidiReceiverChannel ToChannel;

        public enum MapModes
        {
            Property,
            Tween,
            MidiTween,
            TriggerOnly
        }
        public MapModes MapMode = MapModes.Property;

        public Tween Tween;
        public bool TweenEnabled = true;

        public MidiTween MidiTween;
        public bool MidiTweenEnabled = true;

        public float Attack = 0.1f;
        public float Decay = 0.1f;
        public float Sustain = 1f;
        public float SustainMax;
        public float Release = 0.2f;
        public bool Instant = true;
        public bool Polyphonic = true;

        public enum VelocityModes
        {
            Ignore,
            ShortenAttack,
            ScaleValue,
            LimitSustain
        }
        public VelocityModes VelocityMode = VelocityModes.Ignore;
        public float VelocityMin;
        public float VelocityMax = 1f;

        public MathUtil.InterpolationModes AttackEase = MathUtil.InterpolationModes.Linear;
        public MathUtil.InterpolationModes DecayEase = MathUtil.InterpolationModes.Linear;
        public MathUtil.InterpolationModes ReleaseEase = MathUtil.InterpolationModes.Linear;


        public Vector4 ValueOff = new Vector4(0, 0, 0, 1);
        public Vector4 ValueOn = Vector4.one;
        public float ValueOffMultiply = 1f;
        public float ValueOnMultiply = 1f;
        public bool ReverseValues;

        public float Amount = 1f;

        public UnityEvent<float> NoteOnEvent;
        public UnityEvent<float> NoteOffEvent;
        public UnityEvent<float> KnobChangedEvent;

        public string ConfigName = "Receiver1";
        public bool AutoLoadAndSave = true;

        public bool UseKeyboardInput = false;
        public KeyCode InputKeyCode = KeyCode.K;
        public bool UseAnyKey = false;

        public Button ProgramButton;
        public bool UpdateButtonLabel = true;

        #endregion

        #region PUBLIC NONSERIALIZED

        [NonSerialized]
        public bool IsNoteOn;

        [NonSerialized]
        public float NoteValue;

        [NonSerialized]
        public bool IsMapping;

        [NonSerialized]
        public float Velocity;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private bool valueChanged;

        [NonSerialized]
        private Text programButtonLabel;

        [NonSerialized]
        private Image programButtonImage;

        [NonSerialized]
        private float noteStart;

        [NonSerialized]
        private float noteStartValue;

        [NonSerialized]
        private float noteEnded = -1f;

        [NonSerialized]
        private float noteEndValue;

        [NonSerialized]
        private float outputValueFloat;

        [NonSerialized]
        private Vector4 outputValueVector;

        [NonSerialized]
        private bool isKeydown = false;

        [NonSerialized]
        private bool isKeyboardInput = false;

        [NonSerialized]
        private int minNoteAdjusted = 0;

        [NonSerialized]
        private int maxNoteAdjusted = 0;

        #endregion

        #region ACCESSORS

        public bool HasValueChanged {
            get {
                return valueChanged;
            }
        }

        public float OutputValueFloat {
            get {
                return outputValueFloat;
            }
            set {
                if (outputValueFloat != value) {
                    outputValueFloat = value;
                    ToChannel.CurrentValue = value;
                    valueChanged = true;
                }
            }
        }

        public Vector4 OutputValue {
            get {
                return outputValueVector;
            }
            set {
                if (outputValueVector != value) {
                    outputValueVector = value;
                    ToChannel.CurrentVector = value;
                    valueChanged = true;
                }
            }
        }

        public float ValueOffFloat {
            get {
                return ValueOff.x;
            }
            set {
                ValueOff.x = value;
            }
        }

        public float ValueOnFloat {
            get {
                return ValueOn.x;
            }
            set {
                ValueOn.x = value;
            }
        }

        public Color ValueOffColor {
            get {
                return (Color)ValueOff;
            }
            set {
                ValueOff = (Vector4)value;
            }
        }

        public Color ValueOnColor {
            get {
                return (Color)ValueOn;
            }
            set {
                ValueOn = (Vector4)value;
            }
        }

        #endregion

        #region SETUP

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsAwake) return;
            if (Application.isPlaying) {
#if MIDIJACK
                MidiMaster.noteOnDelegate += NoteOn;
                MidiMaster.noteOffDelegate += NoteOff;
                MidiMaster.knobDelegate += Knob;
#endif
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Application.isPlaying) {
#if MIDIJACK
                MidiMaster.noteOnDelegate -= NoteOn;
                MidiMaster.noteOffDelegate -= NoteOff;
                MidiMaster.knobDelegate -= Knob;
#endif
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            ConfigName = gameObject.name;
            if (AutoLoadAndSave) {
                LoadConfig();
            }
            Setup();

            if (ProgramButton != null) {
                GameObject firstChild = ObjectUtil.GetFirstChild(ProgramButton.gameObject);
                if (firstChild != null) {
                    if (firstChild.TryGetComponent<Text>(out programButtonLabel)) {
                        programButtonLabel.text = "LISTENING";
                    }
                    ProgramButton.TryGetComponent<Image>(out programButtonImage);
                }
                ProgramButton.onClick.RemoveListener(ToggleMapping);
                ProgramButton.onClick.AddListener(ToggleMapping);
            }
            UpdateProgramButton();
        }

        /// <summary>
        /// If AutoLoadAndSave is enabled, the midi settings are automatically saved.
        /// </summary>
        protected override void OnDestruct()
        {
            if (AutoLoadAndSave) {
                SaveConfig();
            }
            if (ToChannel != null) {
                RemoveChannel(ToChannel);
            }
            if (ProgramButton != null) {
                ProgramButton.onClick.RemoveListener(ToggleMapping);
            }
            base.OnDestruct();
        }

        public void Setup()
        {
            if (Tween == null) TryGetComponent<Tween>(out Tween);
            if (Tween != null) {
                Tween.EnableRemoteControl = TweenEnabled;
            }

            if (MidiTween == null) TryGetComponent<MidiTween>(out MidiTween);
            if (MidiTween != null) {
                MidiTween.EnableRemoteControl = MidiTweenEnabled;
            }

            OutputValue = Vector4.zero; // also applies value
            Velocity = 0;
            UpdateMinMax();
            ApplyValue();
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (ToChannel == null) {
                ToChannel = new MidiReceiverChannel();
                AddChannel(ToChannel);
            }
            if (ToChannel.ToProperty != null) ToChannel.ToProperty.Prepare();
            ToChannel.OnSetup(this);
            ToChannel.MidiReceiverParent = this;

            if (Channels == null) Channels = new List<TimeflowChannel>();
            if (!HasChannel(ToChannel)) Channels.Add(ToChannel);
        }

        #endregion

        #region UPDATE

        public override void OnPlay()
        {
            Setup();
        }

        public override void UpdateTime()
        {
            /// Only update during UpdateTimeChannel
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            /// bypass base implementation to only use the update method above. There is only 1 channel
            _Update();
        }

        private void _CheckKeyboardInput()
        {
            if (IsMapping) {
                if (UseKeyboardInput) {
                    if (InputUtil.IsAnyKey()) {
                        KeyCode code = InputUtil.DetectKeyDown();
                        if (code != KeyCode.None) {
                            //if (DebugEnabled) Debug.Log($"{name}.MidiReceiver.KeyPressed:{code}");

                            InputKeyCode = code;
                            InputType = InputTypes.Note;
                            NoteMode = NoteModes.Single;
                            StopMapping();
                        }
                    }
                }
            }
            else
            if ((UseAnyKey && InputUtil.IsAnyKey()) || InputUtil.GetKeyDown(InputKeyCode)) {
                IsNoteOn = true;
                NoteValue = 1f;
                Velocity = 1f;
                noteStart = CurrentTime;
                isKeydown = true;
                isKeyboardInput = true;
                Trigger();
            }
            else
            if (isKeydown && ((UseAnyKey && InputUtil.IsAnyKeyReleased()) || InputUtil.GetKeyUp(InputKeyCode))) {
                isKeydown = false;
                IsNoteOn = false;
                NoteValue = 0f;
                Velocity = 0f;
                noteEnded = CurrentTime;
                TriggerOff();
            }
        }

        private void _Update()
        {
            if (!CanUpdate) return;

            if (UseKeyboardInput) _CheckKeyboardInput();

            // Only process ADSR for midi input
            if (!isKeyboardInput && InputType == InputTypes.Note && noteStart > 0) {
                float attack = Instant ? 0 : Attack;
                if (VelocityMode == VelocityModes.ShortenAttack) {
                    attack *= (1f - Velocity);
                }
                float sustain = Sustain;
                if (VelocityMode == VelocityModes.LimitSustain) {
                    sustain = Velocity;
                }
                float time = ToChannel.CurrentTime - noteStart;
                float duration = noteEnded < 0 ? -1 : noteEnded - noteStart;
                if (duration > 0 && SustainMax > 0 && duration > SustainMax) duration = SustainMax;

                if (IsNoteOn) {
                    float v = MathUtil.ADSR(time, attack, Decay, sustain, Release, duration, AttackEase, DecayEase, ReleaseEase);
                    NoteValue = v;
                    if (Polyphonic && noteStartValue > 0f) {
                        NoteValue = MathUtil.Interpolate(noteStartValue, sustain, NoteValue);
                    }
                }
                else {
                    if (Release > 0) {
                        float r = ToChannel.CurrentTime - noteEnded;
                        if (r >= Release || Release == 0) {
                            NoteValue = 0f;
                        }
                        else {
                            NoteValue = MathUtil.InterpolateMode(noteEndValue, 0f, r / Release, ReleaseEase);
                        }
                    }
                    else {
                        NoteValue = 0f;
                    }
                    if (NoteValue == 0) {
                        Velocity = 0f;
                        noteStart = 0f;
                        IsNoteOn = false;
                    }
                }
            }

            if (InputType != InputTypes.None) {
                ApplyValue();
            }
            UpdateProgramButton();

            base.UpdateTime();
        }

        /// <summary>
        /// Passes the value to a Property, Tween, or MidiTween
        /// </summary>
        public void ApplyValue()
        {
            valueChanged = false;
            float value = NoteValue;
            if (InputType == InputTypes.Note && VelocityMode == VelocityModes.ScaleValue) {
                value *= MathUtil.GetInterpolation(VelocityMin, VelocityMax, Velocity);
            }
            value *= Amount;

            if (ReverseValues) value = 1f - value;

            if (MapMode == MapModes.Property) {
                if (ToChannel != null && ToChannel.ToProperty != null) {
                    if (ToChannel.ToProperty.IsColor) {
                        OutputValue = MathUtil.Interpolate(ValueOffColor * ValueOffMultiply, ValueOnColor * ValueOnMultiply, value);
                        ToChannel.ToProperty.ColorValue = OutputValue;
                    }
                    else
                    if (ToChannel.ToProperty.IsVector && ToChannel.ToProperty.IsCombinedValue) {
                        OutputValue = MathUtil.Interpolate(ValueOff, ValueOn, value);
                        ToChannel.ToProperty.ColorValue = OutputValue;
                    }
                    else {
                        OutputValueFloat = MathUtil.Interpolate(ValueOffFloat, ValueOnFloat, value) * ValueOffMultiply;
                        ToChannel.ToProperty.FloatValue = OutputValueFloat;
                    }
                }
            }
            else
            if (MapMode == MapModes.Tween) {
                if (Tween != null) {
                    Tween.EnableRemoteControl = TweenEnabled;
                    if (TweenEnabled) {
                        Tween.RemoteValue = value;
                        OutputValue = Tween.OutputValue;
                    }
                }
            }
            else
            if (MapMode == MapModes.MidiTween) {
                if (MidiTween != null) {
                    MidiTween.EnableRemoteControl = MidiTweenEnabled;
                    if (MidiTweenEnabled) {
                        MidiTween.RemoteValue = value;
                        OutputValue = MidiTween.OutputValue;
                    }
                }
            }
            else
            if (MapMode == MapModes.TriggerOnly) {
                Trigger();
            }
        }

        #endregion

        #region MAPPING

        public void ToggleMapping()
        {
            if (IsMapping) {
                StopMapping();
            }
            else {
                StartMapping();
            }
        }

        /// <summary>
        /// Listens for the first midi not pressed and records that as input to trigger this instance.
        /// </summary>
        public void StartMapping()
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.StartMapping");
            if (Application.isPlaying) {
                if (IsMapping) {
                    StopMapping();
                    return;
                }
                IsMapping = true;
                MinNote = -1;
                MaxNote = -1;
                UpdateProgramButton();
            }
        }

        /// <summary>
        /// Ends listening mode and returns to normal input detection, with the new mapping updated.
        /// </summary>
        public void StopMapping()
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.StopMapping");
            if (Application.isPlaying) {
                IsMapping = false;
                UpdateMinMax();
                UpdateProgramButton();
            }
        }

        private void UpdateMinMax()
        {
            minNoteAdjusted = ApplyOctave(MinNote);
            maxNoteAdjusted = ApplyOctave(MaxNote);
        }

        private void UpdateProgramButton()
        {
            if (programButtonLabel != null) {
                if (IsMapping) {
                    programButtonLabel.text = "LISTENING";
                }
                else {
                    int num = InputType == InputTypes.Note ? MinNote : KnobNumber;
                    string text = $"{InputType}:{num}";
                    if (UseKeyboardInput) {
                        if (UseAnyKey) {
                            text += " Key:Any";
                        }
                        else {
                            text += $" Key:{InputKeyCode}]";
                        }
                    }
                    programButtonLabel.text = text;
                }
                programButtonLabel.color = IsNoteOn ? Color.green : Color.black;
            }
        }

        #endregion

        #region NOTES & TRIGGERING

        public void Trigger()
        {
            //if (DebugEnabled) Debug.Log($"{name}.Trigger");
            if (NoteOnEvent != null) NoteOnEvent.Invoke(NoteValue);
        }

        public void TriggerOff()
        {
            //if (DebugEnabled) Debug.Log($"{name}.TriggerOff");
            if (NoteOffEvent != null) NoteOffEvent.Invoke(NoteValue);
        }

        private bool IsNoteTriggered(int note)
        {
            note = ApplyOctave(note);

            bool triggered = false;
            if (NoteMode == NoteModes.Any) {
                triggered = true;
            }
            else
            if (NoteMode == NoteModes.Single) {
                if (note == minNoteAdjusted) {
                    triggered = true;
                }
            }
            else
            if (NoteMode == NoteModes.Range) {
                if (note >= minNoteAdjusted && note <= maxNoteAdjusted) {
                    triggered = true;
                }
            }
            return triggered;
        }

        private int ApplyOctave(int note)
        {
            if (AllOctaves) {
                while (note > 12) note -= 12;
            }
            if (note < 0) note = 0;
            return note;
        }

#if MINIS
        private void Start()
        {
            InputSystem.onDeviceChange += (device, change) => {
                if (change != InputDeviceChange.Added) return;

                var midiDevice = device as Minis.MidiDevice;
                if (midiDevice == null) return;

                midiDevice.onWillNoteOn += (note, velocity) => {
                    int? channel = (note.device as Minis.MidiDevice)?.channel;
                    //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.NoteOn: " + channel + "," + note.noteNumber + "," + velocity);

                    LastType = InputTypes.Note;
                    LastChannel = channel;
                    LastNote = note.noteNumber;
                    LastVelocity = velocity;

                    if (IsMapping) {
                        InputType = InputTypes.Note;
                        NoteMode = NoteModes.Single;
                        MinNote = note.noteNumber;
                        MaxNote = note.noteNumber;
                        StopMapping();
                    }
                    else
                    if (InputType == InputTypes.Note) {
                        if (IsNoteTriggered(note.noteNumber)) {
                            Velocity = velocity;
                            IsNoteOn = true;
                            noteStart = CurrentTime;
                            noteStartValue = NoteValue;
                            noteEnded = -1f; // unknown end time until user releases midi note
                            isKeyboardInput = false;

                            Trigger();
                        }
                    }

                    //if (DebugEnabled) Debug.Log(string.Format(
                        "Note On #{0} ({1}) vel:{2:0.00} ch:{3} dev:'{4}'",
                        note.noteNumber,
                        note.shortDisplayName,
                        velocity,
                        (note.device as Minis.MidiDevice)?.channel,
                        note.device.description.product
                    ));
                };

                midiDevice.onWillNoteOff += (note) => {
                    int? channel = (note.device as Minis.MidiDevice)?.channel;
                    //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.NoteOff: " + channel + "," + note.noteNumber);
                    if (InputType == InputTypes.Note) {
                        if (IsNoteTriggered(note.noteNumber)) {
                            IsNoteOn = false;
                            noteEnded = CurrentTime;
                            noteEndValue = NoteValue;
                            TriggerOff();
                        }
                    }
                    //if (DebugEnabled) Debug.Log(string.Format(
                        "Note Off #{0} ({1}) ch:{2} dev:'{3}'",
                        note.noteNumber,
                        note.shortDisplayName,
                        (note.device as Minis.MidiDevice)?.channel,
                        note.device.description.product
                    ));
                };

                midiDevice.onWillControlChange += (control, value) => {
                    //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.Knob: " + control.controlNumber + "," + value);
                    LastType = InputTypes.Knob;
                    LastKnob = control.controlNumber;
                    LastVelocity = value;

                    if (IsMapping) {
                        InputType = InputTypes.Knob;
                        KnobNumber = control.controlNumber;
                        StopMapping();
                    }
                    else
                    if (InputType == InputTypes.Knob) {
                        if (control.controlNumber == KnobNumber) {
                            NoteValue = value;
                            if (KnobChangedEvent != null) KnobChangedEvent.Invoke(NoteValue);
                        }
                    }
                };
            };
        }
#elif MIDIJACK

        /// <summary>
        /// This is registered as a delegate with MidiJack to get notification of midi notes.
        /// </summary>
        public void NoteOn(MidiChannel channel, int note, float velocity)
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.NoteOn: " + channel + "," + note + "," + velocity);
            LastType = InputTypes.Note;
            LastChannel = channel;
            LastNote = note;
            LastVelocity = velocity;

            if (IsMapping) {
                InputType = InputTypes.Note;
                NoteMode = NoteModes.Single;
                MinNote = note;
                MaxNote = note;
                StopMapping();
            }
            else
            if (InputType == InputTypes.Note) {
                if (IsNoteTriggered(note)) {
                    Velocity = velocity;
                    IsNoteOn = true;
                    noteStart = CurrentTime;
                    noteStartValue = NoteValue;
                    noteEnded = -1f; // unknown end time until user releases midi note
                    isKeyboardInput = false;

                    Trigger();
                }
            }
        }

        public void NoteOff(MidiChannel channel, int note)
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.NoteOff: " + channel + "," + note);
            if (InputType == InputTypes.Note) {
                if (IsNoteTriggered(note)) {
                    IsNoteOn = false;
                    noteEnded = CurrentTime;
                    noteEndValue = NoteValue;

                    TriggerOff();
                }
            }
        }

        public void Knob(MidiChannel channel, int knobNumber, float knobValue)
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.Knob: " + knobNumber + "," + knobValue);
            LastType = InputTypes.Knob;
            LastChannel = channel;
            LastKnob = knobNumber;
            LastVelocity = knobValue * KnobMultiplier;

            if (IsMapping) {
                InputType = InputTypes.Knob;
                KnobNumber = knobNumber;
                StopMapping();
            }
            else
            if (InputType == InputTypes.Knob) {
                if (knobNumber == KnobNumber) {
                    NoteValue = LastVelocity;
                    if (KnobChangedEvent != null) KnobChangedEvent.Invoke(NoteValue);
                }
            }
        }
#endif

        #endregion

        #region CONFIGURATION

        /// <summary>
        /// Saves the midi trigger settings in PlayerPrefs.
        /// </summary>
        public void SaveConfig()
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.SaveConfig: " + ConfigName);
            PlayerPrefs.SetInt(ConfigName + "Type", InputType == InputTypes.Knob ? 1 : 0);
            PlayerPrefs.SetInt(ConfigName + "Mode", NoteMode == NoteModes.Any ? 0 : NoteMode == NoteModes.Single ? 1 : 2);
            PlayerPrefs.SetInt(ConfigName + "Knob", KnobNumber);
            PlayerPrefs.SetInt(ConfigName + "MinNote", MinNote);
            PlayerPrefs.SetInt(ConfigName + "MaxNote", MaxNote);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Saves the midi trigger settings in PlayerPrefs.
        /// </summary>
        public void EraseConfig()
        {
            //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.SaveConfig: " + ConfigName);
            PlayerPrefs.DeleteKey(ConfigName + "Type");
            PlayerPrefs.DeleteKey(ConfigName + "Mode");
            PlayerPrefs.DeleteKey(ConfigName + "Knob");
            PlayerPrefs.DeleteKey(ConfigName + "MinNote");
            PlayerPrefs.DeleteKey(ConfigName + "MaxNote");
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads midi trigger settings from PlayerPrefs.
        /// </summary>
        public void LoadConfig()
        {
            if (PlayerPrefs.HasKey(ConfigName + "Type")) {
                //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.LoadConfig: " + ConfigName);
                int inputType = PlayerPrefs.GetInt(ConfigName + "Type");
                InputType = inputType == 0 ? InputTypes.Note : InputTypes.Knob;

                int noteMode = PlayerPrefs.GetInt(ConfigName + "Mode");
                NoteMode = noteMode == 0 ? NoteModes.Any : noteMode == 1 ? NoteModes.Single : NoteModes.Range;
                KnobNumber = PlayerPrefs.GetInt(ConfigName + "Knob");
                MinNote = PlayerPrefs.GetInt(ConfigName + "MinNote");
                MaxNote = PlayerPrefs.GetInt(ConfigName + "MaxNote");
                UpdateMinMax();
            }
            else {
                //if (DebugEnabled) Debug.Log(name + ".MidiReceiver.LoadConfig: " + ConfigName + " has not been saved yet.");
            }
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR

        public bool EditorShowConfig = true;
        public bool EditorShowProcessing = true;
        public bool EditorShowMapping = true;
        public bool EditorShowEvents;

        public override Texture2D Icon => AxonUI.Icons.MidiReceiver;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            AxonGUI.PropertySelectMenu(TimeflowContext.Menu, typeof(MidiReceiver), TimeflowContext.Owner, TimeflowContext.Obj.gameObject, null, Property.PropertyFilters.NumericOnly, "Add Midi/Receiver/", true, GUIMenu_Add);
        }

        public static void GUIMenu_Add(object info)
        {
            PropertyMenuItem prop = (PropertyMenuItem)info;
            if (prop != null) {
                GameObject gobj = prop.FromProperty.AssignToObject;
                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        obj.BehaviorsEnabled = true;

                        MidiReceiver comp = Undo.AddComponent<MidiReceiver>(obj.gameObject);
                        if (comp != null) {
                            comp.SetupChannels(false);
                            comp.ToChannel.ToProperty = new Property(comp, prop.FromProperty);
                            comp.ToChannel.ToProperty.ResetName(false);
                            comp.SetupChannels(true);
                        }
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }


#endif
        #endregion
    }

}//AxonGenesis
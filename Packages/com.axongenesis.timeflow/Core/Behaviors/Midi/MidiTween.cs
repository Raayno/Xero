// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/MIDI Tween")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/midi/midi-tween")]
    sealed public partial class MidiTween : TimeflowDataBehavior
    {

        #region PUBLIC

        public MidiFile Midi;
        public int TrackNum;

        public enum NoteModes
        {
            All,
            Range,
            Single,
            MultipleTargets,
            SequenceTargets
        }
        public NoteModes NoteMode = NoteModes.All;
        public string NotesList = "";
        public string CurrentNotes = "";

        public bool NoteOffset;
        public bool NoteRelative = true;
        public float NoteIncrement = 1f;

        public enum SequenceModes
        {
            Forward,
            Reverse,
            Random,
            Skip,
            SkipReverse
        }
        public SequenceModes SequenceMode = SequenceModes.Forward;
        public int SequenceSkip = 1;
        public int SequenceRandomSeed = 1;

        [FormerlySerializedAs("SendTrigger")]
        public bool Send;
        public bool SendOnly;
        public bool SendRuntimeOnly;
        public bool SendBroadcast;

        [FormerlySerializedAs("Objects")]
        public List<MidiTweenChannel> MidiChannels = new List<MidiTweenChannel>();

        public enum CurveModes
        {
            Linear,
            Quadratic,
            Exponential,
            Circle
        }

        public float Attack = 0.1f;
        public float Decay = 0.1f;
        public float Sustain = 1f;
        public float SustainMax;
        public float Release = 0.2f;

        [FormerlySerializedAs("SyncAttack")]
        public bool AnticipateAttack = true;

        [FormerlySerializedAs("ToggleVisibility")]
        public bool ActivateObject;

        public float DelayHide = 1f;

        [FormerlySerializedAs("OverlapNotes")]
        public bool Polyphonic;

        public MathUtil.InterpolationModes AttackEase = MathUtil.InterpolationModes.Linear;
        public MathUtil.InterpolationModes DecayEase = MathUtil.InterpolationModes.Linear;
        public MathUtil.InterpolationModes ReleaseEase = MathUtil.InterpolationModes.Linear;

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

        public enum ProcessingModes
        {
            Interpolate,
            Increment
        }
        public ProcessingModes ProcessingMode = ProcessingModes.Interpolate;
        public int OutputIncrementSteps = 4;

        public Vector4 ValueOff = new Vector4(0, 0, 0, 1); // for default alpha channel
        public Vector4 ValueOn = Vector4.one;

        public float ValueOffMultiply = 1f;
        public float ValueOnMultiply = 1f;
        public bool ReverseValues;

        public float Amount = 1f;
        public int NoteMin;
        public int NoteMax = 127;
        public int LowestNote;

        public bool EnableOverride;
        public float OverrideBlend = 1f;
        public Vector4 OverrideValue = Vector4.zero;

        public enum AudioModes
        {
            OneShot,
            SyncTrack,
            Resume
        }
        public AudioModes AudioMode = AudioModes.OneShot;
        public AudioSource Audio;

        public bool SetShaderProperty;
        public string ShaderPropertyName = "_Amplitude";

        public bool EnableRemoteControl;
        public bool EnableRemotePassThru = true;

        #endregion

        #region PUBLIC NON-SERIALIZED

        /// <summary>
        /// Set a value from 0 to 1 to interpolate between start and end values
        /// </summary>
        [NonSerialized]
        public float RemoteValue = 0f;

        [NonSerialized]
        public MidiTrack Track;

        [NonSerialized]
        public bool IsNoteOn;

        [NonSerialized]
        public Vector4 OutputValue = Vector4.zero;

        #endregion

        #region EDITOR VARS

        #endregion

        #region PRIVATE

        [NonSerialized]
        private int currentPlayIndex;

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private int sequenceIndex;

        [NonSerialized]
        private MidiNote lastNote;

        [NonSerialized]
        private List<MidiNote> currentNotes;

        [NonSerialized]
        private bool isAudioPlaying;

        [NonSerialized]
        private float incAmount;

        [NonSerialized]
        private int incrementStep;

        [NonSerialized]
        private Vector4 valueStep = Vector4.zero;

        [NonSerialized]
        private Vector4 valueStepEnd = Vector4.zero;

        [NonSerialized]
        private int shaderPropertyID;

        #endregion

        #region DELEGATES

        public delegate void OnMidiNoteEventHandler(MidiNote note);
        public event OnMidiNoteEventHandler OnMidiNoteEvent;

        #endregion

        #region ACCESSORS

        public override DataChannel Channel {
            get {
                if (MidiChannels == null) SetupChannels(true);
                return MidiChannels[0];
            }
            set {
                _Channel = null; // Don't use the base channel property
            }
        }

        public override Property ToProperty {
            get {
                if (Channel.ToProperty == null) {
                    Channel.ToProperty = new Property();
                }
                return Channel.ToProperty;
            }
        }

        public bool IsColor {
            get {
                return ToProperty.IsColor;
            }
        }

        public bool IsVector {
            get {
                return ToProperty.IsVector && !ToProperty.IsSingleAttribute;
            }
        }

        public bool IsFloat {
            get {
                return ToProperty.IsSingleAttribute;
            }
        }

        public override bool CanUpdate {
            get {
                return base.CanUpdate && MidiChannels != null && MidiChannels.Count > 0 && Track != null && Midi != null && Track.TotalNotes > 0;
            }
            protected set {
                _canUpdate = value;
            }
        }

        public int MidiChannelCount {
            get {
                int count = 1;
                if (MidiChannels != null && MidiChannels.Count > 0) {
                    count = MidiChannels.Count;
                }
                if (NoteMode == NoteModes.All || NoteMode == NoteModes.Range || NoteMode == NoteModes.Single) {
                    if (count > 1) count = 1;
                }
                return count;
            }
        }

        /// <summary>
        /// To save on serialized data, vector on and off are used to store color and float values too
        /// </summary>
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

        private float StartValue {
            get {
                return (ReverseValues ? ValueOnFloat : ValueOffFloat) * ValueOnMultiply;
            }
        }

        private float EndValue {
            get {
                return (ReverseValues ? ValueOffFloat : ValueOnFloat) * ValueOffMultiply;
            }
        }

        private Color StartColor {
            get {
                return (ReverseValues ? ValueOnColor * ValueOnMultiply : ValueOffColor * ValueOffMultiply);
            }
        }

        private Color EndColor {
            get {
                return (ReverseValues ? ValueOffColor * ValueOffMultiply : ValueOnColor * ValueOnMultiply);
            }
        }

        private Vector4 StartVector {
            get {
                return (ReverseValues ? ValueOn * ValueOnMultiply : ValueOff * ValueOffMultiply);
            }
        }

        private Vector4 EndVector {
            get {
                return (ReverseValues ? ValueOff * ValueOffMultiply : ValueOn * ValueOnMultiply);
            }
        }

        #endregion

        #region SETUP

        protected override void OnStart()
        {
            base.OnStart();
            SetupMidi();
        }

        public void SetupMidi()
        {
            if (MidiChannels == null || MidiChannels.Count == 0) SetupChannels(true);

            if (Midi == null) {
                if (MidiFile.Instance != null) {
                    Midi = MidiFile.Instance;
                }
                else {
                    Midi = UnityEngine.Object.FindFirstObjectByType<MidiFile>();
                }
            }
            LowestNote = 0;
            if (Midi != null && Midi.Tracks != null && TrackNum < Midi.Tracks.Length) {
                Track = Midi.Tracks[TrackNum];
                Track.PrepareNotesToPlay();

                List<int> notes = Midi.Tracks[TrackNum].NotesList();
                if (notes != null && notes.Count > 0) {
                    LowestNote = notes[0];
                }
            }
            if (NoteMode == NoteModes.All) {
                NoteMin = 0;
                NoteMax = 127;
            }
            TimeflowObject tobj = Timeflow.SetupTimeflowObject(gameObject);

            sequenceIndex = 0;
            currentPlayIndex = 1;
            UnityEngine.Random.InitState(SequenceRandomSeed);

            if (Audio == null) TryGetComponent<AudioSource>(out Audio);

            if (MidiChannels != null && MidiChannels.Count > 0) {
                // Reset the target value for all objects
                foreach (MidiTweenChannel obj in MidiChannels) {
                    InterpolateNote(0f, null, obj, true);
                }
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (MidiChannels == null) MidiChannels = new List<MidiTweenChannel>();
            if (MidiChannels.Count == 0) {
                MidiChannels.Add(new MidiTweenChannel(this));
            }

            // Setup the base behavior channels list
            //if (DebugEnabled) Debug.Log(name + ":MidiTween.SetupChannels");
            Channels = new List<TimeflowChannel>();

            foreach (MidiTweenChannel channel in MidiChannels) {
                channel.MidiTweenParent = this;
                channel.OnSetup(this);
                Channels.Add(channel);
            }

            if (NoteMode == NoteModes.All) {
                MidiChannels[0].NoteMin = 0;
                MidiChannels[0].NoteMax = 256;
            }
            else
            if (NoteMode == NoteModes.Single) {
                MidiChannels[0].NoteMin = NoteMin;
                MidiChannels[0].NoteMax = NoteMin;
            }
            else
            if (NoteMode == NoteModes.Range) {
                MidiChannels[0].NoteMin = NoteMin;
                MidiChannels[0].NoteMax = NoteMax;
            }
        }

        public void SetupNoteRanges()
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
            if (NoteMode == MidiTween.NoteModes.MultipleTargets) {
                for (int x = 0; x < notes.Count; x++) {
                    if (x < MidiChannels.Count) {
                        MidiChannels[x].NoteMin = MidiChannels[x].NoteMax = notes[x];
                    }
                }
            }

            if (NoteMode == MidiTween.NoteModes.All) {
                NoteMin = 0;
                NoteMax = 127;
            }
            else {
                NoteMin = (int)range.x;
                NoteMax = (int)range.y;
            }
        }

        public void AddChannel()
        {
            MidiTweenChannel channel = null;
            if (MidiChannels != null && MidiChannels.Count > 1) {
                channel = new MidiTweenChannel(this, MidiChannels[0]);
            }
            else {
                channel = new MidiTweenChannel(this);
            }
            AddChannel(channel);
        }

        public void AddChannel(MidiTweenChannel channel)
        {
            if (MidiChannels == null) MidiChannels = new List<MidiTweenChannel>();
            MidiChannels.Add(channel);

            base.AddChannel(channel);
        }

        public void RemoveChannel(MidiTweenChannel channel)
        {
            if (MidiChannels != null && MidiChannels.Contains(channel)) {
                MidiChannels.Remove(channel);
            }
            base.RemoveChannel(channel);
        }

        public void ClearChannels()
        {
            if (MidiChannels != null && MidiChannels.Count > 1) {
                MidiTweenChannel mainChannel = MidiChannels[0];

                /// Create a copy of the list to avoid modification errors
                List<MidiTweenChannel> toRemove = new List<MidiTweenChannel>();
                foreach (MidiTweenChannel ch in MidiChannels) {
                    if (ch != mainChannel) {
                        toRemove.Add(ch);
                    }
                }
                foreach (MidiTweenChannel ch in toRemove) {
                    RemoveChannel(ch);
                }

                MidiChannels = new List<MidiTweenChannel>();
                MidiChannels.Add(mainChannel);
                SetupChannels(true);
                SetupMidi();
            }
        }

        public void GatherChildren(bool forceRebuild)
        {
            AddTransforms(ObjectUtil.GetChildren(gameObject), forceRebuild);
        }

        public void AddTransforms(List<Transform> transforms, bool forceRebuild)
        {
            //if (DebugEnabled) Debug.Log($"{name}.GatherChildren:{forceRebuild}");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Gather Children", true);
#endif
            MidiTweenChannel prevChannel = MidiChannels.Count > 0 ? MidiChannels[0] : null;
            if (MidiChannels == null || forceRebuild) MidiChannels = new List<MidiTweenChannel>();

            int i = 1;
            foreach (Transform child in transforms) {
                MidiTweenChannel channel = null;

                if (!forceRebuild && MidiChannels.Count > 0) {
                    // Keep existing channels related to each target game object
                    foreach (MidiTweenChannel ch in MidiChannels) {
                        if (ch.TargetObject == child.gameObject) {
                            channel = ch;
                            break;
                        }
                    }
                }

                if (channel == null) {
                    string channelName = "MidiTween";
                    channel = new MidiTweenChannel(this, prevChannel);
                    if (prevChannel == null || prevChannel.ToProperty == null || prevChannel.ToProperty.Comp == null) {
                        channel.ToProperty.Comp = child;
                        channel.ToProperty.Attribute = -1;
                        prevChannel = channel;
                    }
                    else {
                        channel.ToProperty.Comp = child.GetComponent(prevChannel.ToProperty.Comp.GetType());
                        channel.ToProperty.Name = prevChannel.ToProperty.Name;
                        channel.ToProperty.Attribute = prevChannel.ToProperty.Attribute;
                        channelName = channel.ToProperty.GetNameAndAttribute(channelName, true, true, false) + " [" + i + "]";
                    }
                    channel.Name = channelName;
                    AddChannel(channel);
                }
                channel.TargetObject = child.gameObject;

                i++;
            }
            SetupNoteRanges();
            SetupChannels(true);
            SetupMidi();
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            MidiTween t = (MidiTween)src;
            if (t != null) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.Copy:" + src.name);
                base.Copy(src, false); // base takes care of majority of properties

                if (includeChannels) {
                    MidiChannels = null;
                    if (t.MidiChannels != null && t.MidiChannels.Count > 0) {
                        MidiChannels = new List<MidiTweenChannel>();
                        foreach (MidiTweenChannel ch in t.MidiChannels) {
                            MidiChannels.Add(new MidiTweenChannel(this, ch));
                        }
                    }
                    SetupChannels(true);
                }
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ":MidiTween.CopyChannel");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            if (MidiChannels == null) MidiChannels = new List<MidiTweenChannel>();
            MidiTweenChannel t = (MidiTweenChannel)src;
            if (t != null) {
                MidiChannels.Add(new MidiTweenChannel(this, t));
            }
            return Channel;
        }

        #endregion

        #region UPDATE

        public override void OnPlay()
        {
            SetupMidi();
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (Track != null) {
                //if (DebugEnabled) Debug.Log(name + ".OnRewind: PrepareNotesToPlay");
                Track.PrepareNotesToPlay();
            }
            incAmount = 0f;
            incrementStep = 0;
            valueStep = valueStepEnd = ValueOff;
            //if (DebugEnabled) Debug.Log(name + ".OnRewind:" + incrementStep + " ValueStep:" + valueStep);
        }

        public override void UpdateTime()
        {
            if (!CanUpdate) return;

            CurrentNotes = "";
            if (Track.TotalNotes == 0) {
                // Do nothing. No notes in track
            }
            else {
                if (lastTime > CurrentTime) {
                    currentPlayIndex++;
                }

                if (NoteMode == NoteModes.SequenceTargets) {
                    UpdateSequence(Midi.LoopTime(CurrentTime));
                }

                UpdateAudio();
                lastTime = CurrentTime;
            }
            base.UpdateTime();
        }

        /// <summary>
        /// This update is called for each channel by the containing TimeflowObject after UpdateTime has
        /// been called above. This separation is needed to support channel ordering managed by the object.
        /// </summary>
        /// <param name="channel"></param>
        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (!CanUpdate) return;

            if (NoteMode == NoteModes.SequenceTargets) {
                /// Sequence mode requires a more top-down approach, so channels cannot be individually
                /// processed in this case. It should also be noted that this mode does not support time
                /// offset with channel link (though same-time channel link is fine).
            }
            else {
                float currentTime = Midi.LoopTime(channel.CurrentTime);

                float v = 0f;
                if (Polyphonic) {
                    v = InterpolatePolyphonic(currentTime, (MidiTweenChannel)channel, true);
                }
                else {
                    v = InterpolateValue(currentTime, (MidiTweenChannel)channel, true);
                }
                IsNoteOn = v != 0f;
            }
        }

        private void UpdateAudio()
        {
            if (Audio != null) {
                //if (DebugEnabled) Debug.Log("UpdateAudio: IsNoteOn:" + IsNoteOn + " isAudioPlaying:" + isAudioPlaying + " mode:" + AudioMode);
                if (IsNoteOn) {
                    if (!isAudioPlaying) {
                        isAudioPlaying = true;

                        if (AudioMode == AudioModes.OneShot) {
                            Audio.time = 0f;
                            Audio.Play();
                        }
                        else
                        if (AudioMode == AudioModes.SyncTrack) {
                            if (CurrentTime < Audio.clip.length) {
                                Audio.time = CurrentTime;
                                Audio.Play();
                            }
                        }
                        else {
                            Audio.Play();
                        }
                    }
                }
                else {
                    if (isAudioPlaying) {
                        isAudioPlaying = false;

                        if (AudioMode == AudioModes.Resume) {
                            Audio.Pause();
                        }
                        else
                        if (AudioMode == AudioModes.SyncTrack) {
                            Audio.Stop();
                        }
                    }
                }
            }
        }

        private void UpdateSequence(float currentTime)
        {
            if (EnableRemoteControl && !EnableRemotePassThru) return;

            // Get all notes currently being played
            float time = currentTime;
            float attack = Mathf.Max(Attack, Midi.MinAttack);
            float release = Mathf.Max(Release, Midi.MinRelease);
            float duration = attack + release + (ActivateObject ? DelayHide : 0f);

            if (AnticipateAttack) time += attack;
            List<MidiNote> notes = Track.NotesAtTime(time, duration, NoteMin, NoteMax);

            bool notesPlayed = notes != null && notes.Count > 0;
            if (currentNotes != null && currentNotes.Count > 0) {
                foreach (MidiNote lastNote in currentNotes) {
                    bool isNotePlaying = false;
                    if (notesPlayed) {
                        foreach (MidiNote newNote in notes) {
                            if (newNote.Note == lastNote.Note) {
                                newNote.MapToChannel = lastNote.MapToChannel;
                                isNotePlaying = true;
                                break;
                            }
                        }
                    }
                    if (!isNotePlaying && lastNote.MapToChannel != null) {
                        // note stopped playing - process object and return it to it's original state
                        InterpolateNote(time, lastNote, lastNote.MapToChannel, true);
                        lastNote.MapToChannel = null;
                        if (!SendOnly && ActivateObject && gameObject != lastNote.MapToChannel.ToProperty.Comp.gameObject) {
                            lastNote.MapToChannel.ToProperty.Comp.gameObject.SetActive(false);
                        }
                    }
                }
            }

#if UNITY_EDITOR
            CurrentNotes = "";
#endif
            // Reset the target value for all objects
            foreach (MidiTweenChannel obj in MidiChannels) {
                obj.WasPlayingNote = obj.IsPlayingNote;
                obj.IsPlayingNote = false;
            }

            if (notesPlayed) {

                foreach (MidiNote note in notes) {
#if UNITY_EDITOR
                    if (Selection.activeGameObject == gameObject) {
                        if (!string.IsNullOrEmpty(CurrentNotes)) {
                            CurrentNotes += ", ";
                        }
                        CurrentNotes += "" + note.Note + ":" + note.StartTime + ":" + note.Velocity;
                    }
#endif
                    IncrementSequence(note);
                    note.MapToChannel = MidiChannels[sequenceIndex];
                    MidiChannels[sequenceIndex].IsPlayingNote = true;
                    InterpolateNote(time, note, note.MapToChannel, true);
                }
            }

            // Make sure that each note that was played previously completely returns to Off value
            foreach (MidiTweenChannel obj in MidiChannels) {
                if (!obj.IsPlayingNote && obj.WasPlayingNote) {
                    InterpolateNote(time, null, obj, true);
                }
            }

            currentNotes = notes;
        }

        public void IncrementSequence(MidiNote note)
        {
            sequenceIndex = note.Index;
            int count = Track.TotalNotes;
            if (SequenceMode == SequenceModes.Reverse) {
                sequenceIndex = count - sequenceIndex;
            }
            else
            if (SequenceMode == SequenceModes.Random) {
                sequenceIndex = (int)UnityEngine.Random.Range(0, MidiChannels.Count - 1);
            }
            else
            if (SequenceMode == SequenceModes.Skip) {
                sequenceIndex = (sequenceIndex * SequenceSkip) - (SequenceSkip - 1);
            }
            else
            if (SequenceMode == SequenceModes.SkipReverse) {
                sequenceIndex = (sequenceIndex * -SequenceSkip) - ((-SequenceSkip) - 1);
            }

            if (MidiChannels.Count > 0) {
                while (sequenceIndex >= MidiChannels.Count) {
                    sequenceIndex -= MidiChannels.Count;
                }
                while (sequenceIndex < 0) {
                    sequenceIndex += MidiChannels.Count;
                }
            }

            //if (DebugEnabled) Debug.Log(name + ".MidiTween.IncrementSequence:" + SequenceMode + " i:" + sequenceIndex + " note(" + note.Note + "):" + note.Index);
        }

        public float InterpolateValue(float currentTime, MidiTweenChannel channel, bool apply)
        {
            float value = 0f;
            if (channel == null || (EnableRemoteControl && !EnableRemotePassThru) || Track == null) return value;

            float attack = Mathf.Max(Attack, Midi.MinAttack);
            float release = Mathf.Max(Release, Midi.MinRelease);

            if (AnticipateAttack) currentTime += attack;
            if (currentTime > 0) {
                float duration = attack + release + (ActivateObject ? DelayHide : 0f);
                value = InterpolateNote(currentTime, Track.NoteAtTime(currentTime, duration, channel.NoteMin, channel.NoteMax), channel, apply);
            }
            else
            if (apply && ActivateObject) {
                if (gameObject != channel.ToProperty.Comp.gameObject) {
                    if (channel.ToProperty.Comp.gameObject.activeSelf) channel.ToProperty.Comp.gameObject.SetActive(false);
                }
            }
            return value;
        }

        public float InterpolatePolyphonic(float currentTime, MidiTweenChannel channel, bool apply)
        {
            if (channel == null || (EnableRemoteControl && !EnableRemotePassThru)) return 0;
            //if (DebugEnabled) Debug.Log("MidiTween[" + name + "].InterpolatePolyphonic:" + channel.ToProperty.Comp.name + " sequenceIndex:" + sequenceIndex);

            float amount = 0f;
            float attack = Mathf.Max(Attack, Midi.MinAttack);
            float release = Mathf.Max(Release, Midi.MinRelease);

            if (AnticipateAttack) currentTime += attack;
            if (currentTime > 0) {
#if UNITY_EDITOR
                CurrentNotes = "";
#endif
                float duration = attack + release + (ActivateObject ? DelayHide : 0f);
                List<MidiNote> notes = Track.NotesAtTime(currentTime, duration, channel.NoteMin, channel.NoteMax);

                float valueEnd = EndValue;
                bool notePlayed = false;
                float noteStart = 0f;
                if (notes != null && notes.Count > 0) {
                    float lastValue = 0f;
                    bool isFirst = true;
                    //int i = 0;
                    float avg = 0;
                    foreach (MidiNote note in notes) {
                        if (note != null) {
                            if (!notePlayed) {
                                if (apply) {
                                    /// Check that the same note hasn't already been played to avoid
                                    /// multiply processing the same note
                                    notePlayed = note.PlayIndex != currentPlayIndex;
                                    note.PlayIndex = currentPlayIndex;
                                }
                                else {
                                    // this allows random time samplings to be done without affecting regular update playback 
                                    notePlayed = true;
                                }
                                noteStart = currentTime - note.StartTime;
                            }
                            amount = InterpolateNote(currentTime, note, channel, apply);
                        }
                        else
                        if (!SendOnly) {
                            if (apply && ActivateObject) {
                                if (gameObject != channel.ToProperty.Comp.gameObject) {
                                    if (channel.ToProperty.Comp.gameObject.activeSelf) channel.ToProperty.Comp.gameObject.SetActive(false);
                                }
                            }
                            if (isFirst) {
                                isFirst = false;
                            }
                            else {
                                amount = Mathf.Max(amount, lastValue);
                            }
                            lastValue = amount;

                            //if (DebugEnabled) Debug.Log("note[" + i + "]:" + note.Note + " v:" + note.Velocity + " value:" + amount);
                        }
                        else {
                            amount = 0f;
                        }
                        lastNote = note;
                        avg += amount;
                    }
                }
                else
                if (!SendOnly) {
                    if (apply && ActivateObject) {
                        if (gameObject != channel.ToProperty.Comp.gameObject) {
                            if (channel.ToProperty.Comp.gameObject.activeSelf) channel.ToProperty.Comp.gameObject.SetActive(false);
                        }
                    }
                }
                if (notePlayed) SetChannelValue(channel, amount, lastNote, apply);
            }
            return amount;
        }

        public float InterpolateNote(float currentTime, MidiNote note, MidiTweenChannel channel, bool apply)
        {
            if (EnableRemoteControl && !EnableRemotePassThru) return 0;
            float amount = 0f;
            if (channel == null) {
                Debug.LogWarning(name + ".MidiTween.Interpolate: Missing a channel reference");
                return amount;
            }

            if (note == null) {
                if (ProcessingMode == ProcessingModes.Increment) {
                    amount = 1f;
                    incAmount = 0f;
                    //if (DebugEnabled) Debug.Log("note is NULL incAmount:" + incAmount);
                }
                SetChannelValue(channel, amount, null, apply);
            }
            else
            if (note.Velocity >= channel.MinVelocity) {
                note.PlayIndex = currentPlayIndex;

#if UNITY_EDITOR
                if (apply && Selection.activeGameObject == gameObject) {
                    if (!string.IsNullOrEmpty(CurrentNotes)) {
                        CurrentNotes += ", ";
                    }
                    CurrentNotes += "" + note.Note + ":" + note.StartTime + ":" + note.Velocity;
                }
#endif
                if (LocalDeltaTime == 0 || currentTime == 0) {
                    // Rewind or skip in timeline
                    if (ProcessingMode == ProcessingModes.Increment) {
                        incAmount = 0f;
                        incrementStep = 0;
                        valueStep = valueStepEnd = ValueOff;
                        note.MessageSent = false;
                        //if (DebugEnabled) Debug.Log(name + ".Reset Increment:" + incrementStep + " ValueStep:" + valueStep);
                    }
                }
                else
                if (channel.LastNotePlayed != note.Index) {
                    channel.LastNotePlayed = note.Index;

                    if (apply) {
                        // Only send the note once - reset on rewind
                        note.MessageSent = true;
                        if (OnMidiNoteEvent != null) {
                            OnMidiNoteEvent.Invoke(note);
                        }
                        if (Send && channel.ToProperty.Comp != null && (!SendRuntimeOnly || Application.isPlaying)) {
                            if (SendBroadcast) {
                                channel.ToProperty.Comp.gameObject.BroadcastMessage("OnMidiNote", note, SendMessageOptions.DontRequireReceiver);
                            }
                            else {
                                channel.ToProperty.Comp.gameObject.SendMessage("OnMidiNote", note, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }

                    if (ProcessingMode == ProcessingModes.Increment) {
                        incAmount = 0f;
                        incrementStep++;
                        if (OutputIncrementSteps > 0 && incrementStep > OutputIncrementSteps) {
                            incrementStep = 0;
                            valueStep = ValueOff;
                        }
                        else {
                            valueStep = valueStepEnd; // Last set value
                        }
                        //if (DebugEnabled) Debug.Log(name + ".IncrementStep:" + incrementStep + " ValueStep:" + valueStep);
                    }
                }
                if (!SendOnly) {
                    if (apply && ActivateObject) {
                        if (gameObject != channel.ToProperty.Comp.gameObject) {
                            if (!channel.ToProperty.Comp.gameObject.activeSelf) channel.ToProperty.Comp.gameObject.SetActive(true);
                        }
                    }

                    float attack = Mathf.Max(Attack, Midi.MinAttack);
                    float release = Mathf.Max(Release, Midi.MinRelease);
                    float a = attack;
                    if (VelocityMode == VelocityModes.ShortenAttack) {
                        a *= (1f - note.Velocity);
                        a = Mathf.Max(a, Midi.MinAttack);
                    }

                    float start = currentTime - note.StartTime;
                    float sustain = Sustain;
                    if (VelocityMode == VelocityModes.LimitSustain) {
                        sustain = note.Velocity;
                    }
                    amount = MathUtil.ADSR(start, a, Decay, sustain, release, 0, AttackEase, DecayEase, ReleaseEase);

                    if (apply && ProcessingMode == ProcessingModes.Increment) {
                        /// The value only increases when using increment mode and playing forward. Since
                        /// it would require excessive calculation to determine the increment at any
                        /// arbitrary time, it is a limitation that this mode does not work with channel
                        /// links using time offsets.
                        incAmount = Mathf.Max(incAmount, (1f - amount));
                        //if (DebugEnabled) Debug.Log("note:" + note.Note + " incAmount:" + incAmount + " amount:" + amount);
                        amount = incAmount;
                    }

                    if (VelocityMode == VelocityModes.ScaleValue) {
                        amount *= MathUtil.Interpolate(VelocityMin, VelocityMax, note.Velocity);
                    }
                    amount *= Midi.Intensity;

                    SetChannelValue(channel, amount, note, apply);
                }
                lastNote = note;

            }
            return amount;
        }

        public void SetChannelValue(MidiTweenChannel channel, float amount, MidiNote note, bool apply)
        {
            if (EnableRemoteControl) {
                if (EnableRemotePassThru) {
                    amount = Mathf.Max(amount, RemoteValue);
                }
                else {
                    amount = RemoteValue;
                }
            }

            channel.ToProperty.ReadValue();
            if (channel.ToProperty.IsColor) {
                OutputValue = EndColor;
                if (NoteOffset && note != null) {
                    if (NoteRelative) {
                        OutputValue = MathUtil.Multiply(OutputValue, NoteIncrement * (note.Note - LowestNote));
                    }
                    else {
                        OutputValue = MathUtil.Multiply(OutputValue, NoteIncrement * note.Note);
                    }
                }

                if (ProcessingMode == ProcessingModes.Interpolate) {
                    OutputValue = MathUtil.Interpolate(StartColor, EndColor, amount * Amount);
                }
                else {
                    // Increment instead of interpolate
                    valueStepEnd = valueStep + OutputValue;
                    OutputValue = MathUtil.Interpolate(valueStep, valueStepEnd, amount * Amount);
                    //if (DebugEnabled) Debug.Log("Value:" + OutputValue + " step:" + valueStep + " end:" + valueStepEnd + " amount:" + amount);
                }
                if (EnableOverride) {
                    OutputValue = MathUtil.Interpolate(OutputValue, OverrideValue, OverrideBlend);
                }
                if (apply) {
                    channel.ToProperty.ColorValue = OutputValue;

                    if (SetShaderProperty) {
                        if (shaderPropertyID == 0) shaderPropertyID = Shader.PropertyToID(ShaderPropertyName);
                        Shader.SetGlobalColor(ShaderPropertyName, OutputValue);
                    }
                    //if (DebugEnabled) Debug.Log(channel.ToProperty.PathName() + ".SetChannelValue:" + OutputValue + " value:" + amount + " amount:" + Amount);
                }
            }
            else
            if (channel.ToProperty.IsVector) {
                OutputValue = EndVector;
                if (NoteOffset && note != null) {
                    if (NoteRelative) {
                        OutputValue = MathUtil.Multiply(OutputValue, NoteIncrement * (note.Note - LowestNote));
                    }
                    else {
                        OutputValue = MathUtil.Multiply(OutputValue, NoteIncrement * note.Note);
                    }
                }

                if (ProcessingMode == ProcessingModes.Interpolate) {
                    OutputValue = MathUtil.Interpolate(StartVector, EndVector, amount * Amount);
                }
                else {
                    // Increment instead of interpolate
                    valueStepEnd = valueStep + OutputValue;
                    OutputValue = MathUtil.Interpolate(valueStep, valueStepEnd, amount * Amount);
                    //if (DebugEnabled) Debug.Log("Value:" + OutputValue + " step:" + valueStep + " end:" + valueStepEnd + " amount:" + amount);
                }
                if (EnableOverride) {
                    OutputValue = MathUtil.Interpolate(OutputValue, OverrideValue, OverrideBlend);
                }
                if (apply) {
                    channel.ToProperty.Vector4Value = OutputValue;

                    if (SetShaderProperty) {
                        if (shaderPropertyID == 0) shaderPropertyID = Shader.PropertyToID(ShaderPropertyName);
                        Shader.SetGlobalVector(ShaderPropertyName, OutputValue);
                    }
                    //if (DebugEnabled) Debug.Log(channel.ToProperty.PathName() + ".SetChannelValue:" + OutputValue + " value:" + amount + " amount:" + Amount);
                }
            }
            else {
                float valueEnd = EndValue;
                if (NoteOffset && note != null) {
                    if (NoteRelative) {
                        valueEnd += NoteIncrement * (note.Note - LowestNote);
                    }
                    else {
                        valueEnd += NoteIncrement * note.Note;
                    }
                }

                if (ProcessingMode == ProcessingModes.Interpolate) {
                    OutputValue.x = MathUtil.Interpolate(StartValue, valueEnd, amount * Amount);
                }
                else {
                    // Increment instead of interpolate
                    valueStepEnd.x = valueStep.x + valueEnd;
                    OutputValue.x = MathUtil.Interpolate(valueStep.x, valueStepEnd.x, amount * Amount);
                    //if (DebugEnabled) Debug.Log("Value:" + OutputValue.x + " step:" + valueStep + " end:" + valueStepEnd + " amount:" + amount);
                }
                if (EnableOverride) {
                    OutputValue.x = MathUtil.Interpolate(OutputValue.x, OverrideValue.x, OverrideBlend);
                }
                if (apply) {
                    if (channel.ToProperty.IsSingleAttribute) {
                        channel.ToProperty.FloatValue = OutputValue.x;
                    }
                    else
                    if (channel.ToProperty.IsColor) {
                        channel.ToProperty.ColorValue = (Color)OutputValue;
                    }
                    else {
                        channel.ToProperty.Vector4Value = OutputValue;
                    }
                    if (SetShaderProperty) {
                        if (shaderPropertyID == 0) shaderPropertyID = Shader.PropertyToID(ShaderPropertyName);
                        if (channel.ToProperty.IsSingleAttribute) {
                            Shader.SetGlobalFloat(ShaderPropertyName, OutputValue.x);
                        }
                        else
                        if (channel.ToProperty.IsColor) {
                            Shader.SetGlobalColor(ShaderPropertyName, (Color)OutputValue);
                        }
                        else {
                            Shader.SetGlobalVector(ShaderPropertyName, OutputValue);
                        }
                    }
                }
            }
        }

        #endregion

        #region DATA UPDATE

        private void _InterpolateChannel(MidiTweenChannel channel, float time, bool apply)
        {
            if (Polyphonic) {
                InterpolatePolyphonic(time, channel, apply);
            }
            else {
                InterpolateValue(time, channel, apply);
            }
        }

        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            if (ProcessingMode != ProcessingModes.Increment && NoteMode != NoteModes.SequenceTargets) {
                /// Increment mode cannot be reliably calculated at arbitrary times, so return the current
                /// value only
                _InterpolateChannel((MidiTweenChannel)channel, time, apply);
            }
            //if (DebugEnabled) Debug.Log(channel.Name + ".InterpolateValue:" + OutputValue.x + " time:" + time + " apply:" + apply);
            return OutputValue.x;
        }

        public override Vector2 InterpolateVector2(TimeflowChannel channel, float time, bool apply)
        {
            //if (DebugEnabled) Debug.Log(channel.Name + ".InterpolateVector2:" + time + " apply:" + apply);
            if (ProcessingMode != ProcessingModes.Increment && NoteMode != NoteModes.SequenceTargets) {
                _InterpolateChannel((MidiTweenChannel)channel, time, apply);
            }
            return OutputValue;
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            //if (DebugEnabled) Debug.Log(channel.Name + ".InterpolateVector3:" + time + " apply:" + apply);
            if (ProcessingMode != ProcessingModes.Increment && NoteMode != NoteModes.SequenceTargets) {
                _InterpolateChannel((MidiTweenChannel)channel, time, apply);
            }
            return OutputValue;
        }

        public override Vector4 InterpolateVector4(TimeflowChannel channel, float time, bool apply)
        {
            //if (DebugEnabled) Debug.Log(channel.Name + ".InterpolateVector4:" + time + " apply:" + apply);
            if (ProcessingMode != ProcessingModes.Increment && NoteMode != NoteModes.SequenceTargets) {
                _InterpolateChannel((MidiTweenChannel)channel, time, apply);
            }
            return OutputValue;
        }

        public override Color InterpolateColor(TimeflowChannel channel, float time, bool apply)
        {
            //if (DebugEnabled) Debug.Log(channel.Name + ".InterpolateColor:" + time + " apply:" + apply);
            if (ProcessingMode != ProcessingModes.Increment && NoteMode != NoteModes.SequenceTargets) {
                _InterpolateChannel((MidiTweenChannel)channel, time, apply);
            }
            return (Color)OutputValue;
        }

        #endregion

#if UNITY_EDITOR

        public float ExportFrameRate = 30f;

        public bool EditorShowMidi = true;
        public bool EditorShowNotes = true;
        public bool EditorShowProperty;
        public bool EditorShowProcessing = true;
        public bool EditorShowValues = true;
        public bool EditorShowAdvanced;

        public override Texture2D Icon => AxonUI.Icons.MidiTween;

        public override void Refresh()
        {
            base.Refresh();
            SetupMidi();
            UpdateTime();
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":MidiTween.RemoveChannelWithUndo");

            if (MidiChannelCount > 1) {
                MidiTweenChannel mc = (MidiTweenChannel)channel;
                if (mc != null) {
                    if (MidiChannels.Contains(mc)) MidiChannels.Remove(mc);
                }
            }
            // base method handles destroying object
            base.RemoveChannelWithUndo(channel);
        }

        public void AddSelected()
        {
            AddTransforms(new List<Transform>(Selection.transforms), false);
        }

        #region TOOLS

#if AXON_EXPERIMENTAL

        public float AEFrameRate = 30f;
        public void CopyToAE()
        {
            //			Track = Midi.Tracks[TrackNum];
            //
            //			string output = "Adobe After Effects 8.0 Keyframe Data\n";
            //			output += "\tUnits Per Second	"+AEFrameRate+"\n";
            //			output += "\tSource Width	100\n";
            //			output += "\tSource Height	100\n";
            //			output += "\tSource Pixel Aspect Ratio	1\n";
            //			output += "\tComp Pixel Aspect Ratio	1\n";
            //			output += "\n";
            //				
            //			output += "Transform	Opacity\n";
            //			output += "\tFrame	percent	\n";
            //			//output += "\t0	100	100	100\n";
            //			
            //			float t = 0;
            //			int f = 0;
            //			float frame = 1f / AEFrameRate;
            //			float lastValue = 0f;
            //			float lastTime = 0f;
            //			float prevFrame = 0f;
            //			bool isFirst = true;
            //			//Debug.Log("frame:"+frame);
            //			while(t < Timeline.TotalTime) {
            //				float v = InterpolateValue(t);
            //				if(KeyEveryFrame || v != lastValue || isFirst) {
            //					if(!KeyEveryFrame && !isFirst && prevFrame != lastTime) {
            //					// Insert a keyframe before this one so the value holds
            //						output += "\t"+prevFrame+"\t"+lastValue+"\n";
            //					}
            //					output += "\t"+f+"\t"+v+"\n";
            //					lastValue = v;
            //					lastTime = t;
            //				}
            //				t += frame;
            //				if(isFirst) {
            //					isFirst = false;
            //					lastValue = v;
            //					lastTime = t;
            //				}
            //				prevFrame = t;
            //				f++;
            //			}
            //			
            //			output += "\n";
            //			output += "End of Keyframe Data";
            //			Debug.Log(output);
            //			
            //			EditorGUIUtility.systemCopyBuffer = output;
            //			
            //			CurrentNotes = "";

        }
#endif

        public void CopyData(bool copyTime)
        {
        }

        #endregion

        #region GUI

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            AxonGUI.PropertySelectMenu(TimeflowContext.Menu, typeof(MidiTween), TimeflowContext.Owner, TimeflowContext.Obj.gameObject, null, Property.PropertyFilters.NumericOnly, "Add Midi/Tween/", true, GUIMenu_Add);
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

                        MidiTween comp = Undo.AddComponent<MidiTween>(obj.gameObject);
                        if (comp != null) {
                            comp.SetupChannels(false);
                            comp.MidiChannels[0].ToProperty = new Property(comp, prop.FromProperty);
                            comp.SetupChannels(true);
                        }
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

        #endregion

#endif

    }

}//AxonGenesis

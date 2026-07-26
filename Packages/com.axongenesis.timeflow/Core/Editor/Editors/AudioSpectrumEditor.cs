// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;

#if TMPRO_3_OR_NEWER
using TMPro;
#endif

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioSpectrum))]
    public class AudioSpectrumEditor : AxonGenesisEditor<AudioSpectrum, AudioSpectrumEdit>
    {
    }
    sealed public class AudioSpectrumEdit : AxonGenesisBehaviorEdit<AudioSpectrum>
    {
#if TIMEFLOW_PRO
        public const string kAddAudioSpectrum = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "📶 Audio Spectrum";
#else
        public const string kAddAudioSpectrum = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Audio Spectrum";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Audio Spectrum";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAudioSpectrum, false, 163)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAudioSpectrum, false, 163)]
        public static void AddAudioSpectrum()
        {
            GameObject target = Selection.activeGameObject;

            if (target == null && AudioTrack.Instance != null) target = AudioTrack.Instance.gameObject;

            AudioSource src;
            if (target == null || !target.TryGetComponent<AudioSource>(out src)) {
                EditorUtil.ShowDialog("No Audio Source Found", "Please select a game object with an AudioSource component to use as input for AudioSpectrum.");
                return;
            }

            AudioSpectrum sampler = Undo.AddComponent<AudioSpectrum>(target);
            if (sampler != null) {
                sampler.Source = src;
            }

            SelectionUtil.Select(target);
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        private bool hasAudioSamples = false;

        public AudioSpectrumEdit() { }

        public AudioSpectrumEdit(AudioSpectrum _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-sampler";
            hasAudioSamples = AudioSample.HasAnyInstances();
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void Refresh()
        {
            base.Refresh();
            target.SetupAudio();
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Audio Input";
            target.AudioInput = (AudioSpectrum.AudioInputs)AxonGUI.FieldEnumPopupInline(target, target.AudioInput, GUILayout.Width(120));
            if (target.AudioInput == AudioSpectrum.AudioInputs.AudioSource) {
                AxonGUI.SetTooltip("An Audio Source component is required whether using a device or not. When using a device, the audio clip should be null.");
                AxonGUI.UndoName = "Set Audio Source";
                target.Source = (AudioSource)AxonGUI.FieldObjectInline(target, target.Source, typeof(AudioSource), true);
            }
            else
            if (target.AudioInput == AudioSpectrum.AudioInputs.AudioListener) {
                AxonGUI.SetTooltip("If enabled, this uses the scenes AudioListener as the source. This allows sampling to include all audio heard in the scene.");
            }
            else
            if (target.AudioInput == AudioSpectrum.AudioInputs.Device) {
                DevicesGUI();
            }
            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup();
            if (target.AudioInput == AudioSpectrum.AudioInputs.Device) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
#if TMPRO_3_OR_NEWER
                AxonGUI.UndoName = "Set Display Device Name";
                AxonGUI.SetTooltip("This is an optional setting to display the selected device name in the GUI using a TextMeshPro object.");
                target.DeviceNameText = (TextMeshPro)AxonGUI.FieldObject(target, "Display Device Name", target.DeviceNameText, typeof(TextMeshPro), true);
#else
                AxonGUI.HelpBox("Please install the TextMeshPro Essentials package to display the Device Name", MessageType.Info);
#endif

                AxonGUI.UndoName = "Set Timeout";
                AxonGUI.SetTooltip("Latency adds time in seconds to lag behind the audio source. In most cases 0 latency is desired.");
                target.DeviceTimeout = AxonGUI.FieldFloatInline(target, "Timeout", target.DeviceTimeout);
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Audio Channel";
            AxonGUI.SetTooltip("Select which stereo channel to use. Note that sampling both (in stereo) does incur more processing, so only select 1 channel if sufficient.");
            target.AudioChannel = (AudioSpectrum.AudioChannels)AxonGUI.FieldEnumPopup(target, "Channel", target.AudioChannel);

            AxonGUI.BeginDisabledGroup(target.AutoSampleRate);
            AxonGUI.UndoName = "Set Sample Rate";
            AxonGUI.SetTooltip("This defines the audio sampling rate, typically 44100 or 48000. This setting is automatically adjusted according to the audio device settings.");
            target.SampleRate = AxonGUI.FieldIntInline(target, "Sample Rate", target.SampleRate);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Auto Sample Rate";
            bool auto = AxonGUI.FieldToggleInline(target, "Auto", target.AutoSampleRate);
            if (auto != target.AutoSampleRate) {
                target.AutoSampleRate = auto;
                target.SampleRate = AudioSettings.outputSampleRate;
            }

            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Spectrum Window";
            AxonGUI.SetTooltip("Sets the spectrum analysis type used. Each type may produce different results in sampling and effeciency.");
            target.SpectrumWindow = (FFTWindow)AxonGUI.FieldEnumPopup(target, "Spectrum Window", target.SpectrumWindow);

            AxonGUI.UndoName = "Set Spectrum Resolution";
            AxonGUI.SetTooltip("This sets the size of the spectrum to be analyzed and affects overall sampling quality. A power of 2 number is required.");
            if (target.SpectrumResolution < 64) target.SpectrumResolution = 64;
            if (target.SpectrumResolution > 4096) target.SpectrumResolution = 4096;
            int res = AxonGUI.FieldIntInline(target, "Resolution", target.SpectrumResolution, true);
            if (target.SpectrumResolution != res) {
                target.SpectrumResolution = res;
                target.Refresh();
                EditorGUIUtility.ExitGUI();
            }

            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            EditorGUI.BeginChangeCheck();
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Volume Mode";
            AxonGUI.SetTooltip("Enable to output volume in a separate channel");
            target.EnableVolume = AxonGUI.FieldToggle(target, "Volume (" + (target.RMSRaw ? "RMS" : "dB") + ")", target.EnableVolume);
            if (target.EnableVolume) {
                if (!target.RMSRaw) {
                    AxonGUI.SetTooltip("Provides an RMS reference value to measure volume levels by. The default value is 0.1 for 0dB.");
                    AxonGUI.UndoName = "Set Volume RMS Reference";
                    target.RMSReference = AxonGUI.FieldFloatInline(target, "RMS / Reference", target.RMSReference);
                }
                AxonGUI.SetTooltip("Outputs volume as RMS value unfiltered.");
                AxonGUI.UndoName = "Set RMS Raw Value";
                target.RMSRaw = AxonGUI.FieldToggleInline(target, "Raw", target.RMSRaw);

                if (target.VolumeChannel != null) {
                    if (target.AudioChannel == AudioSpectrum.AudioChannels.Stereo) {
                        AxonGUI.FieldVector2Inline(target, target.VolumeChannel.CurrentVector);
                    }
                    else {
                        AxonGUI.FieldFloatInline(target, target.VolumeChannel.CurrentValue);
                    }
                }
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Frequency";
            AxonGUI.SetTooltip("Enable pitch detection output as a separate channel as frequency in hertz.");
            target.EnableFrequency = AxonGUI.FieldToggle(target, "Frequency", target.EnableFrequency);
            if (target.EnableFrequency) {
                AxonGUI.UndoName = "Set Min Threshold";
                AxonGUI.SetTooltip("Determines the sensitivity for detecting frequency. Note that frequency values can be unpredictable and inaccurate at times.");
                target.FrequencyThreshold = AxonGUI.FieldFloatInline(target, "Min Threshold", target.FrequencyThreshold);

                if (target.FrequencyChannel != null) {
                    if (target.AudioChannel == AudioSpectrum.AudioChannels.Stereo) {
                        AxonGUI.FieldVector2Inline(target, target.FrequencyChannel.CurrentVector);
                    }
                    else {
                        AxonGUI.FieldFloatInline(target, target.FrequencyChannel.CurrentValue);
                    }
                }

                if (target.SpectrumResolution < 1024) {
                    AxonGUI.Warning("Frequency calculation works best with a spectrum resolution of 1024 or greater. Lower resolutions may produce inaccurate pitch and note detection.");
                }
            }
            AxonGUI.EndHorizontal(false);
            AxonGUI.EndBox();

            if (EditorGUI.EndChangeCheck()) {
                target.SetupChannels(true);
                Timeflow.Active.Refresh(true);
                EditorGUIUtility.ExitGUI();
            }

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Graph in Game View";
            AxonGUI.SetTooltip("If enabled, a basic audio graph is drawn in the game view. This is an editor feature only, useful for seeing audio input visually.");
            target.ShowGraph = AxonGUI.FieldToggle(target, "Graph in Game View", target.ShowGraph);
            if (target.ShowGraph) {
                AxonGUI.UndoName = "Set Graph Scale";
                AxonGUI.SetTooltip("Sets the size of the graph displayed, to adjust for quiet or loud audio.");
                target.GraphScale = AxonGUI.FieldFloatInline(target, "Scale", target.GraphScale);

            }
            AxonGUI.EndHorizontal();

            if (target.ShowGraph) {
                AxonGUI.UndoName = "Set Frequency Range";
                AxonGUI.FieldSliderMinMax(target, "Frequency Range", ref target.GraphRange.x, ref target.GraphRange.y, 0f, 24000f);
                target.GraphRange = AxonGUI.FieldVector2(target, "Range Min-Max", target.GraphRange);

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Graph Threshold";
                AxonGUI.SetTooltip("Sets the minimum value required to register a spike. Use this to filter out noise.");
                target.GraphThreshold = AxonGUI.FieldSlider(target, "Threshold", target.GraphThreshold, 0f, target.GraphThresholdMax);

                AxonGUI.UndoName = "Set Graph Max";
                target.GraphThresholdMax = AxonGUI.FieldFloatInline(target, "Max", target.GraphThresholdMax, GUILayout.Width(80));
                AxonGUI.EndHorizontal(false);
            }

            AxonGUI.EndBox();

            if (!hasAudioSamples) {
                AxonGUI.HelpBox("AudioSpectrum does not produce any behaviors on its own. Use AudioSample components to sample specific areas of the spectrum.", MessageType.Info);
            }
        }

        public void DevicesGUI()
        {
#if TIMEFLOW_DISABLE_MICROPHONE
            AxonGUI.HelpBox("Audio input devices are disabled in this build. To re-enable devices, remove the scripting define symbol TIMEFLOW_DISABLE_MICROPHONE from the player settings.", MessageType.Warning);
            if (AxonGUI.ButtonInline("Enable Timeflow Pro")) {
                EditorUtil.OpenPackageManager();
            }
            target.DeviceName = "No Device";
            target.FallbackToFirstDevice = false;
#else
            if (string.IsNullOrEmpty(target.DeviceName)) target.DeviceName = "Please select a device";
            if (AxonGUI.ButtonInline(target.DeviceName)) {
                if (Microphone.devices == null || Microphone.devices.Length == 0) {
                    EditorUtil.ShowDialog("No Audio Input Device", "No suitable audio devices for input could be detected. Please make sure that a microphone or other audio input device is connected.");
                }
                else {
                    GenericMenu menu = new GenericMenu();
                    foreach (string device in Microphone.devices) {
                        bool isSelected = device.Equals(target.DeviceName);
                        DeviceMenuItem item = new DeviceMenuItem();
                        item.Spectrum = target;
                        item.DeviceName = device;
                        menu.AddItem(new GUIContent(device), isSelected, SelectDevice, item);
                    }
                    menu.ShowAsContext();
                }
            }
            AxonGUI.UndoName = "Set Audio Fallback Device";
            AxonGUI.SetTooltip("If enabled, the first audio device found becomes the input if the specific named device isn't located. This can be useful on mobile devices where the microphone name may vary.");
            target.FallbackToFirstDevice = AxonGUI.FieldToggleInline(target, "Fallback", target.FallbackToFirstDevice);
            if (target.HasDevice) {
                GUI.color = AxonColor.Active;
                AxonGUI.ButtonInline("Connected");
            }
            else {
                AxonGUI.Warning("Device not found!");
            }
            AxonGUI.Info("To completely disable microphone access, add the scripting define symbol TIMEFLOW_DISABLE_MICROPHONE to the player settings.");
            GUI.color = Color.white;
#endif
        }

        public static void SelectDevice(object value)
        {
            DeviceMenuItem item = (DeviceMenuItem)value;
            if (item != null) {
                item.Spectrum.DeviceName = item.DeviceName;
                item.Spectrum.FindDevice();
            }
        }
    }

    public class DeviceMenuItem
    {
        public AudioSpectrum Spectrum;
        public string DeviceName;
    }

}//AxonGenesis

#endif
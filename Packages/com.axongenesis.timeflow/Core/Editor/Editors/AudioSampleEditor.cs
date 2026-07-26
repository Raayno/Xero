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
    [CustomEditor(typeof(AudioSample))]
    public class AudioSampleEditor : AxonGenesisEditor<AudioSample, AudioSampleEdit>
    {
    }
    sealed public class AudioSampleEdit : AxonGenesisBehaviorEdit<AudioSample>
    {
#if TIMEFLOW_PRO
        public const string kAddAudioSample = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎵 Audio Sample";
#else
        public const string kAddAudioSample = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Audio Sample";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Audio Sample";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAudioSample, false, 162)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAudioSample, false, 162)]
        public static void AddAudioSample()
        {
            ObjectUtil.GetOrAddComponent<AudioSample>(TimeflowMenu.GetSelectedOrNewGameObject("Audio Sample"));
        }

        public static bool IsPreviewMode;
        public static bool WasGraphShowing;
        public static float GraphThreshold = 0f;

        public TimeflowBehaviorSharedEdit behaviorUI;


        public AudioSampleEdit() { }

        public AudioSampleEdit(AudioSample _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-sample";
            UpdateSpectrumGraph();
        }

        private void UpdateSpectrumGraph()
        {
            if (IsPreviewMode && target.Spectrum != null) {
                target.Spectrum.GraphRange.x = target.StartFrequency;
                target.Spectrum.GraphRange.y = target.EndFrequency;
                target.Spectrum.GraphThreshold = target.AmplitudeThreshold;
                target.Spectrum.GraphThresholdMax = target.AmplitudeThresholdMax;
            }
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            if (AxonGUI.ButtonInline("Sample Now")) {
                target.SampleExplicit();
            }

            AxonGUI.LabelInline("Amplitude: " + target.Amplitude, "");
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Audio Spectrum";
            target.Spectrum = (AudioSpectrum)AxonGUI.FieldObject(target, "Spectrum", target.Spectrum, typeof(AudioSpectrum), true);

            AxonGUI.UndoName = "Set Audio Sample Sum Mode";
            target.SumMode = (AudioSample.SumModes)AxonGUI.FieldEnumPopupInline(target, "Mode", target.SumMode, GUILayout.Width(120));
            if (target.Spectrum != null && !target.Spectrum.IsRegistered(target)) {
                if (AxonGUI.ButtonInline("Register")) {
                    target.Spectrum.Register(target);
                }
            }
            AxonGUI.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Frequency Range";
            AxonGUI.FieldSliderMinMax(target, "Frequency Range", ref target.StartFrequency, ref target.EndFrequency, 0f, 24000f);
            AxonGUI.EndHorizontal(false);

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Frequence Range";
            Vector2 v2 = AxonGUI.FieldVector2(target, "Range Min-Max", new Vector2(target.StartFrequency, target.EndFrequency));
            if (GUI.changed) {
                target.StartFrequency = v2.x;
                target.EndFrequency = v2.y;
            }
            GUI.color = IsPreviewMode ? AxonColor.EditingOverride : Color.white;
            if (AxonGUI.ButtonInline("Show Graph")) {
                IsPreviewMode = !IsPreviewMode;
                if (target.Spectrum != null) {
                    if (!IsPreviewMode) {
                        target.Spectrum.ShowGraph = WasGraphShowing;
                        target.Spectrum.GraphRange.x = 0;
                        target.Spectrum.GraphRange.y = 24000f;
                        target.AmplitudeThreshold = target.Spectrum.GraphThreshold;
                        target.AmplitudeThresholdMax = target.Spectrum.GraphThresholdMax;
                    }
                    else {
                        /// store the UI state so it can be set back after previewing
                        WasGraphShowing = target.Spectrum.ShowGraph;
                        target.Spectrum.ShowGraph = true;
                    }
                }
            }
            GUI.color = Color.white;
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            if (target.AmplitudeThreshold < 0f) target.AmplitudeThreshold = 0f;
            
            AxonGUI.SetTooltip("Sets the minimum value required to register a spike. Use this to filter out noise.");
            AxonGUI.UndoName = "Set Threshold";
            target.AmplitudeThreshold = AxonGUI.FieldSlider(target, "Threshold", target.AmplitudeThreshold, 0f, target.AmplitudeThresholdMax);

            AxonGUI.UndoName = "Set Max";
            target.AmplitudeThresholdMax = AxonGUI.FieldFloatInline(target, "Max", target.AmplitudeThresholdMax, GUILayout.Width(80));

            AxonGUI.EndHorizontal();

            if (target.EndFrequency < target.StartFrequency) {
                target.EndFrequency = target.StartFrequency;
            }

            if (EditorGUI.EndChangeCheck()) {
                UpdateSpectrumGraph();
            }

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Decay Rate";
            AxonGUI.SetTooltip("Makes sounds spikes last longer over time. Decay is length in seconds.");
            target.DecayRate = AxonGUI.FieldFloat(target, "Decay Rate", target.DecayRate);
            if (target.DecayRate < 0f) target.DecayRate = 0f;

            AxonGUI.UndoName = "Set Multiply Amplitude";
            AxonGUI.SetTooltip("Multiplies the final amplitude for increased effect");
            target.Multiply = AxonGUI.FieldFloatInline(target, "Multiply", target.Multiply);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply to Scale";
            target.ApplyToScale = AxonGUI.FieldToggle(target, "Apply to Scale", target.ApplyToScale);
            if (target.ApplyToScale) {
                AxonGUI.UndoName = "Set Scale Uniform";
                target.ScaleUniform = AxonGUI.FieldToggleInline(target, "Uniform", target.ScaleUniform);
                if (target.ScaleUniform) {
                    AxonGUI.UndoName = "Set Base Scale";
                    target.BaseScale.x = target.BaseScale.y = target.BaseScale.z = AxonGUI.FieldFloatInline(target, "Base Scale", target.BaseScale.x);

                    AxonGUI.UndoName = "Set Add Scale";
                    target.Scale.x = target.Scale.y = target.Scale.z = AxonGUI.FieldFloatInline(target, "Add Scale", target.Scale.x);
                }
            }
            AxonGUI.EndHorizontal();

            if (target.ApplyToScale) {
                if (!target.ScaleUniform) {
                    AxonGUI.UndoName = "Set Base Scale";
                    target.BaseScale = AxonGUI.FieldVector3(target, "Base Scale", target.BaseScale);

                    AxonGUI.UndoName = "Set Add Scale";
                    target.Scale = AxonGUI.FieldVector3(target, "Add Scale", target.Scale);
                }
            }
            AxonGUI.EndBox();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Shader Property";
            target.SetShaderProperty = AxonGUI.FieldToggle(target, "Set Shader Property", target.SetShaderProperty);
            if (target.SetShaderProperty) {
            AxonGUI.UndoName = "Set Shader Property Name";
                target.ShaderPropertyName = AxonGUI.FieldTextInline(target, "Name", target.ShaderPropertyName);
            }
            AxonGUI.EndHorizontal();

            if (target.IsBaking) {
                GUI.color = AxonColor.BrandRed;
            }

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Use Baked Data";
            target.UseBakedData = AxonGUI.FieldToggle(target, "Use Baked Data", target.UseBakedData);
            if (target.UseBakedData) {
                if (target.IsBaking) {
                    if (AxonGUI.ButtonInline("Stop Baking")) {
                        target.StopBaking();
                    }
                }
                else {
                    if (AxonGUI.ButtonInline("Start Baking")) {
                        target.StartBaking(true);
                    }
                }

                if (AxonGUI.ButtonInline("Restart")) {
                    target.ClearBakedData();
                    target.StartBaking(true);
                }
                if (AxonGUI.ButtonInline("Start All")) {
                    AudioSample.SetIsBakingAll(true, true, false);
                }
                if (AxonGUI.ButtonInline("Stop All")) {
                    AudioSample.SetIsBakingAll(false, true, false);
                }
                if (AxonGUI.ButtonInline("Turn Off All")) {
                    AudioSample.SetIsBakingAll(false, false, false);
                }
                if (AxonGUI.ButtonInline("Clear All")) {
                    AudioSample.SetIsBakingAll(false, false, true);
                }
            }
            else {
                target.IsBaking = false;
            }
            AxonGUI.EndHorizontal();

            GUI.color = AxonColor.Default;
            behaviorUI.MainGUI();

            if (target.IsBaking) {
                AxonGUI.HelpBox("Baking is enabled. Playback the timeline from the start continously in edit mode to capture audio data. Click Stop Baking when done.", MessageType.Info);
            }
        }
    }

}//AxonGenesis

#endif
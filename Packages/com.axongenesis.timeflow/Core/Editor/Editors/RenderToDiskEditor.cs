// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(RenderToDisk))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-to-disk")]
    public class RenderToDiskEditor : AxonGenesisEditor<RenderToDisk, RenderToDiskEdit> { }

    sealed public class RenderToDiskEdit : AxonGenesisBehaviorEdit<RenderToDisk>
    {
#if TIMEFLOW_PRO
        public const string kAddRenderToDisk = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎞️ Render to Disk";
#else
        public const string kAddRenderToDisk = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Render to Disk";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Render to Disk";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddRenderToDisk, false, 220)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddRenderToDisk, false, 220)]
        public static void AddRenderToDisk()
        {
            GameObject obj = new GameObject("RenderToDisk");
            UndoUtil.UndoCreate(obj, "Add Render to Disk");
            if (Timeflow.Active != null) {
                obj.transform.SetParent(Timeflow.Active.gameObject.transform);
            }
            ObjectUtil.ResetTransform(obj);
            RenderToDisk render = ObjectUtil.AddComponent<RenderToDisk>(obj);
            if (Timeflow.Active != null) {
                // Match default time settings to active Timeflow
                render.FrameRate = Timeflow.Active.FPS;
                render.Range.StartTime.TimeType = TimeValue.TimeTypes.Start;
                render.Range.EndTime.TimeType = TimeValue.TimeTypes.End;
            }

            SelectionUtil.Select(obj);
        }

        private bool showWarnings = true;
        private bool hasAudioSamples = false;
        private AudioSample[] audioSamples;
        private bool updateSize = true;
        private bool HasGUIChanged => updateSize && GUI.changed && !Application.isPlaying; // prevents interrupting render

        SerializedProperty OnEnabled;
        SerializedProperty OnRenderStarted;
        SerializedProperty OnRenderAborted;
        SerializedProperty OnRenderFinished;

        public override void OnEnable()
        {
            target.CheckPaths();
            target.UpdateCaptureSize();
            audioSamples = AudioSample.GetAllAudioSampleInstances();
            hasAudioSamples = audioSamples != null && audioSamples.Length > 0;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            OnEnabled = editor.serializedObject.FindProperty("OnEnabled");
            OnRenderStarted = editor.serializedObject.FindProperty("OnRenderStarted");
            OnRenderAborted = editor.serializedObject.FindProperty("OnRenderAborted");
            OnRenderFinished = editor.serializedObject.FindProperty("OnRenderFinished");
        }

        public override void OnInspectorGUI()
        {
            if (target.enabled) {
                updateSize = true;

                WarningsGUI();
                InfoGUI();
                CameraGUI();
                OutputGUI();
                TimeRangesGUI();

                OptionsGUI();
                EventsGUI();

                if (HasGUIChanged) {
                    target.Prepare();
                }
            }
            if (HasGUIChanged) {
                EditorUtil.SetDirty(target);
            }
            editor.serializedObject.ApplyModifiedProperties();
        }

        public override void GUIMenu()
        {
            if (RenderToDisk.IsEncodingImage) return;
            AxonGUI.BeginDisabledGroup(RenderToDisk.IsEncodingImage);
            AxonGUI.BeginHorizontalBox();
            if (RenderToDisk.IsRendering) {
                if (AxonGUI.ButtonInline("Pause")) {
                    target.Pause();
                }
                if (AxonGUI.ButtonInline("Abort and Exit")) {
                    target.Abort();
                }
                if (AxonGUI.ButtonInline("Abort After Current Render")) {
                    target.AbortAfterCurrentRender();
                }
            }
            else {
                if (target.HasBeenStarted) {
                    if (AxonGUI.ButtonInline("Resume")) {
                        target.Resume();
                    }
                    if (AxonGUI.ButtonInline("Abort and Exit")) {
                        target.Abort();
                    }
                    if (AxonGUI.ButtonInline("Restart")) {
                        target.StartRendering();
                    }
                }
                else
                if (AxonGUI.ButtonInline((EditorApplication.isPlaying || target.AutoStart) ? "Start Render" : "Enter Play Mode")) {
                    if (!EditorApplication.isPlaying) {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        EditorApplication.isPlaying = true;
                    }
                    else {
                        target.StartRendering();
                    }
                }
            }
            if (!Application.isPlaying) {
                AxonGUI.UndoName = "Set Auto Start";
                AxonGUI.SetTooltip("When enabled, rendering starts immediately upon entering play mode. Turn this off to play " +
                    "normally and only beging rendering when Play Mode starts.");
                target.AutoStart = AxonGUI.FieldToggleInline(target, "Auto Start", target.AutoStart);
            }
            if (target.IsStillFrame) {
                AxonGUI.BeginDisabledGroup(!Application.isPlaying);
                if (AxonGUI.ButtonInline("Render Frame")) {
                    target.RenderStill();
                }
                AxonGUI.EndDisabledGroup();
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndDisabledGroup();
        }

        private void WarningsGUI()
        {
            if (showWarnings) {
                string warning = null;
                GameObject warningGameObject = null;
                if (hasAudioSamples) {
                    foreach (AudioSample sample in audioSamples) {
                        if (!sample.UseBakedData) {
                            warning = "Since audio sampling cannot be done while rendering, make sure all instances of AudioSample are using baked data, otherwise the rendered output will not match the runtime behavior.";
                            warningGameObject = sample.gameObject;
                            break;
                        }
                    }
                }
                if (warning != null) {
                    GUI.color = AxonColor.Error;
                    AxonGUI.BeginBox();
                    GUI.color = AxonColor.Default;

                    AxonGUI.HelpBox(warning, MessageType.Warning);
                    AxonGUI.BeginHorizontal();
                    if (warningGameObject != null && AxonGUI.Button("Select Object")) {
                        SelectionUtil.Select(warningGameObject);
                    }
                    if (AxonGUI.Button("Dismiss Warnings")) {
                        showWarnings = false;
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();
                }
            }
        }

        private void InfoGUI()
        {
            AxonGUI.BeginBoxPadded();

            if (Application.isPlaying) {
                ProgressBarGUI();
            }
            else {
                OutputSizeGUI();
            }
            OutputInfoGUI();

            AxonGUI.UndoName = "Set Render Next";
            AxonGUI.SetTooltip("If the scene has multiple RenderToDisk setups you wish to render, you can chain them together in a sequence. " +
                "It is recommended to deactivate all RenderToDisk objects except for the first one.");
            target.RenderNext = (RenderToDisk)AxonGUI.FieldObject(target, "Render Next", target.RenderNext, typeof(RenderToDisk), true);

            AxonGUI.EndBoxPadded();
        }

        private void ProgressBarGUI()
        {
            if (target.IsStillFrame) {
                AxonGUI.BeginHorizontal();
                GUI.color = AxonColor.DimField;
                AxonGUI.Label("Press the Render Frame button to render a single frame.");
                if (!string.IsNullOrEmpty(target.RenderName)) {
                    AxonGUI.LabelInline(target.RenderName);
                }
                GUI.color = Color.white;
                AxonGUI.EndHorizontal();
            }
            else {
                float x = 40f;
                float y = 50f;
                float w = EditorGUIUtility.currentViewWidth - 90;
                float h = 10;

                if (target.RenderProgress > 1) target.RenderProgress = 1;
                bool isVideoEncoding = (int)target.RenderProgress == 1 && !VideoQueue.IsFinished;

                EditorGUI.DrawRect(new Rect(x - 1, y - 1, w + 2, h + 2), Color.black);
                if (isVideoEncoding) {
                    EditorGUI.DrawRect(new Rect(x, y, w, h), AxonColor.RenderEncoding);
                }
                else
                if (target.RenderProgress < 0) {
                    // Pending or performing preroll
                    EditorGUI.DrawRect(new Rect(x, y, target.RenderProgress * w * -1f, h), AxonColor.RenderPending);
                }
                else {
                    EditorGUI.DrawRect(new Rect(x, y, target.RenderProgress * w, h), AxonColor.RenderProgress);
                }
                AxonGUI.Space();
                AxonGUI.Space();

                AxonGUI.BeginHorizontal();
                GUI.color = AxonColor.DimField;
                if (isVideoEncoding) {
                    AxonGUI.Label(target.RenderName, $"Encoding Video... Remaining Items:{VideoQueue.RemainingItems}");
                    if (VideoQueue.RemainingItems > 0) {
                        AxonGUI.HelpBox("Don't exit play mode until there are 0 remaining items, otherwise the video encodings queued will not get processed", MessageType.Warning);
                    }
                    if (AxonGUI.ButtonInline("Stop")) {
                        VideoQueue.StopCurrent();
                    }
                    if (AxonGUI.ButtonInline("Stop All")) {
                        VideoQueue.StopAll();
                    }
                }
                else {
                    AxonGUI.Label(target.RenderStatus);
                    if (!string.IsNullOrEmpty(target.RenderName)) {
                        AxonGUI.LabelInline(target.RenderName);
                    }
                }
                GUI.color = Color.white;
                AxonGUI.EndHorizontal();

                AxonGUI.Space();
            }
            OutputSizeGUI();
        }

        private void OutputInfoGUI()
        {
            if (target.Range == null) return;

            string ext = target.FileExtension;
            string preview = StringUtil.PadNumber(1, target.Range.FrameNumberPadding) + ext;

            if (target.IsSingleRange || target.IsStillFrame) {
                preview = target.OutputPath + target.Range.NamePrefix + target.Range.Name + "_" + preview;

                AxonGUI.BeginBox();
                AxonGUI.Label("Output", preview);
                if (target.Range.EnableVideoEncoding && target.Range.VideoItem != null) {
                    AxonGUI.Label("Video", target.Range.VideoItem.OutputFilepath);
                }
                AxonGUI.EndBox();
            }
            else {
                int i = 1;
                foreach (RenderToDiskRange range in target.Ranges) {
                    if (range.EnableRender) {
                        if (RenderToDisk.IsRendering) {
                            GUI.color = target.Range == range ? Color.green : Color.grey;
                        }
                        AxonGUI.BeginBox();
                        string rangePreview = range.OutputPath + PathUtil.Separator + range.NamePrefix + range.Name + "_" + preview;
                        AxonGUI.Label($"{i}: Output", rangePreview);
                        if (range.EnableVideoEncoding && range.VideoItem != null) {
                            AxonGUI.Label("    Video", range.VideoItem.OutputFilepath);
                        }
                        AxonGUI.EndBox();
                        i++;
                    }
                }
                GUI.color = Color.white;
            }
        }

        private void SetCaptureSize(int x, int y)
        {
            target.OverrideCaptureSize = new Vector2(x, y);
            target.UpdateCaptureSize();
            updateSize = false;
        }

        private void AddCaptureSize(GenericMenu menu, string label, int x, int y)
        {
            menu.AddItem(new GUIContent(label), false, () => { SetCaptureSize(x, y); });
        }

        private void OutputSizeGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            if (!target.SetCaptureSize) {
                AxonGUI.Label("Capture Size", $"{target.CaptureSize.x} x {target.CaptureSize.y}");
                AxonGUI.SetTooltip("If enabled, the Game View size is set upon rendering. Otherwise if disabled, the output size automatically uses the current Game View size.");
                target.SetCaptureSize = AxonGUI.FieldToggleInline(target, "Set", target.SetCaptureSize);
            }
            else {
                AxonGUI.UndoName = "Set Capture Size";
                target.OverrideCaptureSize = AxonGUI.FieldVector2(target, "Capture Size", target.OverrideCaptureSize, true);
                target.SetCaptureSize = AxonGUI.FieldToggleInline(target, "Set", target.SetCaptureSize);
                if (AxonGUI.ButtonInline("...")) {
                    GenericMenu menu = new GenericMenu();
                    AddCaptureSize(menu, "Thumbnail (320 x 240)", 320, 240);
                    AddCaptureSize(menu, "SD (640 x 480)", 640, 480);
                    AddCaptureSize(menu, "HD (1280 x 720)", 1280, 720);
                    AddCaptureSize(menu, "FHD (1920 x 1080)", 1920, 1080);
                    AddCaptureSize(menu, "QHD (2560 x 1440)", 2560, 1440);
                    AddCaptureSize(menu, "UHD (3840 x 2160)", 3840, 2160);
                    AddCaptureSize(menu, "6KHD (6144 x 3456)", 6144, 3456);
                    AddCaptureSize(menu, "8KHD (7680 x 4320)", 7680, 4320);
                    AddCaptureSize(menu, "Square 0.5K (512 x 512)", 512, 512);
                    AddCaptureSize(menu, "Square 1K (1024 x 1024)", 1024, 1024);
                    AddCaptureSize(menu, "Square 2K (2048 x 2048)", 2048, 2048);
                    AddCaptureSize(menu, "Square 3K (3092 x 3092)", 3092, 3092);
                    AddCaptureSize(menu, "Square 4K (4096 x 4096)", 4096, 4096);
                    AddCaptureSize(menu, "Square 8K (8192 x 8192)", 8192, 8192);
                    AddCaptureSize(menu, "Square 16K (16384 x 16384)", 16384, 16384);
                    menu.ShowAsContext();
                    updateSize = false;
                }
                if (AxonGUI.ButtonInline("Get")) {
                    target.CaptureSize = target.OverrideCaptureSize = GameViewUtil.GetSize();
                    updateSize = false;
                }
                if (AxonGUI.ButtonInline("Apply to Game View")) {
                    target.UpdateCaptureSize();
                    updateSize = false;
                }
                if (updateSize && HasGUIChanged) {
                    target.UpdateCaptureSize();
                }
            }

            AxonGUI.Info("Set the pixel resolution you want to output. The output size is determined by the Game View. " +
                "During rendering, do not make changes to the Game View resolution or scale, or it may affect the render. " +
                "Even if the resolution is larger than can be displayed in the view, it will still render the fullsize.");
            if (!target.IsStereo && target.IsVR360 && ((int)(target.CaptureSize.x / target.CaptureSize.y) != 2)) {
                AxonGUI.Warning("Rendering to mono VR360 requires a resolution with a 2:1 aspect ratio. Please set the " +
                    "capture size matching the output texture resolution.");
            }
            else
            if ((target.IsFulldome || target.IsVR180 || (target.IsStereo && target.IsVR360)) && (int)target.CaptureSize.x != (int)target.CaptureSize.y) {
                AxonGUI.Warning("This rendering setup requires a square 1:1 aspect ratio. Please set the capture size to " +
                    "match the render texture resolution. For VR360, set the size to match the final output texture.");
            }
            AxonGUI.EndHorizontal();

            AxonGUI.UndoName = "Set Edit Mode Resolution";
            AxonGUI.SetTooltip("This affects the game view size while in edit mode which helps working in edit mode when rendering to high resolutions.");
            target.EditorPreviewSize = (RenderToDisk.EditorPreviewSizes)AxonGUI.FieldEnumPopup(target, "Edit Mode Resolution", target.EditorPreviewSize);

            AxonGUI.Label("Framerate", $"{target.FrameRate}fps");


            if (target.FinalSize != target.CaptureSize) {
                AxonGUI.Label("Output Size", $"{target.FinalSize.x} x {target.FinalSize.y}");
            }
        }

        private void OutputGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOutput = AxonGUI.Foldout(target.EditorShowOutput, "Output");
            if (target.EditorShowOutput) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.UndoName = "Set Preview Only";
                AxonGUI.SetTooltip("Use this to preview output without generating any image files. This is normally used for testing purposes.");
                target.PreviewOnly = AxonGUI.FieldToggle(target, "Preview Only", target.PreviewOnly);

                if (!target.IsStillFrame) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Time Scale";
                    AxonGUI.SetTooltip("Applies to time globally as Time.timeScale");
                    target.TimeScale = AxonGUI.FieldFloat(target, "Time Scale", target.TimeScale);

                    AxonGUI.UndoName = "Set Time Step";
                    if (target.Framestep <= 0) target.Framestep = 1;
                    AxonGUI.SetTooltip("Output every X frames, skipping frames inbetween. Use this option to render");
                    target.Framestep = AxonGUI.FieldIntInline(target, "Step", target.Framestep);

                    if (!target.UseTimeflowFrameRate) {
                        AxonGUI.UndoName = "Set Framerate";
                        AxonGUI.SetTooltip("Sets the output frame rate (frames per second). Note that this can be different from the " +
                            "Timeflow FPS setting to force rendering to a different rate.");
                        target.FrameRate = AxonGUI.FieldFloatInline(target, "Framerate", target.FrameRate, true);
                    }
                    else {
                        target.FrameRate = Timeflow.Active == null ? 30 : Timeflow.Active.FPS;
                    }

                    if (Timeflow.Active != null) {
                        AxonGUI.UndoName = "Set Auto FPS";
                        target.UseTimeflowFrameRate = AxonGUI.FieldToggleInline(target, "Auto (" + Timeflow.Active.FPS + "fps)", target.UseTimeflowFrameRate);
                        if (Timeflow.Active.FPS != target.FrameRate) {
                            AxonGUI.Warning("The rendering framerate (" + target.FrameRate + " FPS) does not match Timeflow (" + Timeflow.Active.FPS + " FPS). This can be ignored if intentional.");
                        }
                    }

                    AxonGUI.EndHorizontal();

#if AXON_EXPERIMENTAL
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Subframes render inbetween frames rendered to disk. This can be useful in special situations to create smoother interoplations between rendered frames.");
                    AxonGUI.UndoName = "Set Generate Subframes";
                    target.Subframes = AxonGUI.IntField(target, "Generate Subframes", target.Subframes);
                    AxonGUI.UndoName = "Set Subframe Scale";
                    target.SubframeScale = AxonGUI.FieldFloatInline(target, "Scale Time", target.SubframeScale);
                    AxonGUI.EndHorizontal();
#endif
                }
                if (target.PreviewOnly) {
                    AxonGUI.HelpBox("No files will be saved to disk in preview mode. Generated output is displayed onscreen only.", MessageType.Warning);
                }
                else {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set File Format";
                    AxonGUI.SetTooltip("JPEG format is preferred for fast high quality output. Use PNG format if an alpha channel is needed.");
                    target.FileFormat = (RenderToDisk.FileFormats)AxonGUI.FieldEnumPopup(target, "File Format", target.FileFormat);
                    if (target.FileFormat == RenderToDisk.FileFormats.JPEG) {
                        target.OutputAlpha = false;
                        AxonGUI.UndoName = "Set Quality";
                        target.JPEGQuality = AxonGUI.FieldSliderInline(target, "Quality", target.JPEGQuality, 0, 100f);
                    }
                    else
                    if (target.FileFormat == RenderToDisk.FileFormats.EXR) {
                        AxonGUI.UndoName = "Set EXR Options";
                        target.EXRFlags = (Texture2D.EXRFlags)AxonGUI.FieldEnumPopupInline(target, "Options", target.EXRFlags, true);
                    }

                    if (target.FileFormat != RenderToDisk.FileFormats.JPEG) {
                        AxonGUI.UndoName = "Set HDR";
                        AxonGUI.SetTooltip("If enabled, the internal render texture format uses float space to preserver high dynamic range. " +
                            "This option requires more memory and computing power. Use this option if you wish to render to EXR float.");
                        target.TexFormatHDR = AxonGUI.FieldToggleInline(target, "HDR", target.TexFormatHDR);

                        AxonGUI.UndoName = "Set Output Alpha";
                        AxonGUI.SetTooltip("If enabled, outputs a transparent png file. If no alpha is generated in the output, try using " +
                            "Screen Capture mode. Alpha rendering also depends on the render pipeline settings and scene setup.");
                        target.OutputAlpha = AxonGUI.FieldToggleInline(target, "Output Alpha", target.OutputAlpha);

                        AxonGUI.Info("Please note that alpha channel support and quality depends on Unity's render pipeline settings and is not " +
                            "something controlled by Timeflow. Also, you must turn off post processing for alpha to render properly!");
                    }
                    AxonGUI.EndHorizontal();
                    OutputSaveGUI();
                }

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void OutputSaveGUI()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Specify a directory path to save to. This should be outside the Assets folder, otherwise Unity will import the rendered images. Note that the speed of the hard drive significantly impacts render time, so save files to your fastest drive.");
            if (target.IsStereo && target.IsStereoSaveSeparately) {
                AxonGUI.UndoName = "Set Save Left to Directory";
                target.SaveToDirectory = AxonGUI.FieldText(target, "Save Left", target.SaveToDirectory, true);

                AxonGUI.UndoName = "Set Save Right to Directory";
                target.SaveToDirectoryRight = AxonGUI.FieldText(target, "Save Right", target.SaveToDirectoryRight, true);
            }
            else {
                AxonGUI.UndoName = "Set Save to Directory";
                target.SaveToDirectory = AxonGUI.FieldText(target, "Save to Directory", target.SaveToDirectory, true);
            }
            AxonGUI.Info("You may also start a path with on of the following variables:\n" +
                "$PROJECT - Path of the current Unity project, at the same level of Assets.\n" +
                "$CUSTOM - Use a custom path, defined in the Timeflow Preferences window.\n" +
                "$ASSETS - Path of the Assets directory (not recommended for image sequences).\n");
            if (AxonGUI.ButtonInline("Select Path")) {
                target.SaveToDirectory = EditorUtility.OpenFolderPanel("Save to Directory", "", "");
                target.CheckPaths();
                EditorGUIUtility.ExitGUI();
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Filename Prefix";
            AxonGUI.SetTooltip("Optionally prepends a fixed name to all files rendered.");
            target.FilenamePrefix = AxonGUI.FieldText(target, "Filename Prefix", target.FilenamePrefix, true);

            AxonGUI.UndoName = "Set Use Scene Name";
            target.FilenamePrefixUseSceneName = AxonGUI.FieldToggleInline(target, "Use Scene Name", target.FilenamePrefixUseSceneName);

            AxonGUI.UndoName = "Set Append Name";
            AxonGUI.SetTooltip("Adds a secondary part to the prefix, helpful for defining a category or other label.");
            target.FilenamePrefix2 = AxonGUI.FieldTextInline(target, "Append", target.FilenamePrefix2, true);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();

            AxonGUI.UndoName = "Set Open When Finished";
            target.OpenDirectoryWhenFinished = AxonGUI.FieldToggle(target, "Open When Finished", target.OpenDirectoryWhenFinished);
            if (AxonGUI.ButtonInline("Open Now")) {
                EditorUtility.RevealInFinder(target.OutputPath);
            }
            AxonGUI.SetTooltip("Updates all ranges with paths based on the primary Save to Directory, replacing any previously customized paths.");
            if (AxonGUI.ButtonInline("Refresh All Paths")) {
                target.ResetAllPaths();
            }
            AxonGUI.EndHorizontal();

            if (string.IsNullOrEmpty(target.SaveToDirectory)) {
                AxonGUI.HelpBox("Please specify the directory path to save images to. This should be a full system path, starting with drive letter. For example:\n" +
                    @" E:\Renders\", MessageType.Warning);
            }
        }

        private void UseCaptureGUI()
        {
            AxonGUI.UndoName = "Set Use Active Camera";
            AxonGUI.SetTooltip("Uses Camera.current (or Camera.main as fallback) for rendering. If enabled, this field cannot be animated or set directly.");
            target.UseActiveCamera = AxonGUI.FieldToggleInline(target, "Use Active", target.UseActiveCamera);

            AxonGUI.UndoName = "Set Use Screen Capture";
            AxonGUI.SetTooltip("When enabled, renders the game view using CaptureScreenshotAsTexture and is preferred for general rendering. " +
                "This automatically renders whichever camera is currently active. If disabled, the directly specified camera is captured using " +
                "a render texture. These modes may produce slightly different results, especially when rendering an alpha channel.");
            target.UseScreenCapture = AxonGUI.FieldToggleInline(target, "Screen Capture", target.UseScreenCapture);
        }

        private void CameraGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowCamera = AxonGUI.Foldout(target.EditorShowCamera, "Camera");
            if (target.EditorShowCamera) {
                AxonGUI.BeginBoxPadded();

                bool useCubemap = target.IsVR180 || target.IsVR360 || target.IsFulldome;
                bool canUseCapture = !useCubemap && !target.IsStereo;
                if (!canUseCapture) {
                    target.UseActiveCamera = false;
                    target.UseScreenCapture = false;
                }

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Output Format";
                RenderToDisk.OutputFormats f = (RenderToDisk.OutputFormats)AxonGUI.FieldEnumPopup(target, "Output Format", target.OutputFormat);
                if (target.OutputFormat != f) {
                    target.OutputFormat = f;
                    target.UseScreenCapture = canUseCapture; // default on if allowed
                }
                AxonGUI.UndoName = "Set Stereo Mode";
                AxonGUI.SetTooltip("Enables stereoscopic rendering using separate cameras for left and right eyes.");
                target.IsStereo = AxonGUI.FieldToggleInline(target, "Stereo", target.IsStereo);
                if (target.IsStereo) {
                    AxonGUI.UndoName = "Set Interocular Distance";
                    AxonGUI.SetTooltip("Set the interocular distance between left and right eyes. This is usually between 50 and 75mm (0.05 - 0.075).");
                    target.StereoSeparation = AxonGUI.FieldFloatInline(target, "Interocular Distance", target.StereoSeparation);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                if (!target.IsStereo) {
                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(target.UseActiveCamera);
                    AxonGUI.UndoName = "Set Camera";
                    target.MainCamera = (Camera)AxonGUI.FieldObject(target, "Camera", target.MainCamera, typeof(Camera), true);
                    EditorGUI.EndDisabledGroup();
                    if (canUseCapture) UseCaptureGUI();
                    if (useCubemap) {
                        AxonGUI.UndoName = "Set Camera Cubemap";
                        target.CubemapLeft = (RenderTexture)AxonGUI.FieldObjectInline(target, "Cubemap", target.CubemapLeft, typeof(RenderTexture), true, false, GUILayout.Width(140));
                    }
                    AxonGUI.EndHorizontal();
                    if (useCubemap) {
                        AxonGUI.UndoName = "Set Camera Cubemap Faces";
                        target.CubemapFace = (RenderToDisk.CubemapFaces)AxonGUI.FieldEnumPopup(target, "Cubemap Faces", (RenderToDisk.CubemapFaces)target.CubemapFace, true);
                    }
                    AxonGUI.EndBox();
                }
                else {
                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.BeginDisabledGroup(target.IsVR180 || target.IsVR360);
                    AxonGUI.UndoName = "Set Stereoscopic Format";
                    target.StereoscopicFormat = (RenderToDisk.StereoscopicFormats)AxonGUI.FieldEnumPopup(target, "Layout", target.StereoscopicFormat);
                    AxonGUI.EndDisabledGroup();
                    if (canUseCapture) UseCaptureGUI();
                    AxonGUI.EndHorizontal();

                    if (useCubemap) {
                        AxonGUI.UndoName = "Set Cubemap Faces";
                        target.CubemapFace = (RenderToDisk.CubemapFaces)AxonGUI.FieldEnumPopup(target, "Cubemap Faces", (RenderToDisk.CubemapFaces)target.CubemapFace, true);
                    }

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Left Camera";
                    target.MainCamera = (Camera)AxonGUI.FieldObject(target, "Left Camera", target.MainCamera, typeof(Camera), true);
                    if (target.MainCamera == null) AxonGUI.Warning("Please assign the left eye camera.");
                    if (useCubemap) {
                        AxonGUI.UndoName = "Set Left Cubemap";
                        target.CubemapLeft = (RenderTexture)AxonGUI.FieldObjectInline(target, "Cubemap", target.CubemapLeft, typeof(RenderTexture), true, false, GUILayout.Width(140));
                        if (target.CubemapLeft == null) {
                            AxonGUI.Warning("Please assign a cubemap render texture matching the capture resolution.");
                        }
                    }
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Right Camera";
                    target.RightCamera = (Camera)AxonGUI.FieldObject(target, "Right Camera", target.RightCamera, typeof(Camera), true);
                    if (target.RightCamera == null) AxonGUI.Warning("Please assign the right eye camera.");
                    if (useCubemap) {
                        AxonGUI.UndoName = "Set Right Cubemap";
                        target.CubemapRight = (RenderTexture)AxonGUI.FieldObjectInline(target, "Cubemap", target.CubemapRight, typeof(RenderTexture), true, false, GUILayout.Width(140));
                        if (target.CubemapRight == null) {
                            AxonGUI.Warning("Please assign a cubemap render texture matching the capture resolution.");
                        }
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();
                }

                if (target.IsVR180) {
                    target.UseScreenCapture = false;
                    target.DomeOrientation = RenderToDisk.DomeOrientations.Fisheye;
                    target.StereoscopicFormat = RenderToDisk.StereoscopicFormats.LeftRight;

                    FulldomeOptionsGUI();
                }
                else
                if (target.IsVR360) {
                    target.StereoscopicFormat = RenderToDisk.StereoscopicFormats.TopBottom;
                }
                else
                if (target.IsFulldome) {
                    AxonGUI.BeginBox();
                    AxonGUI.UndoName = "Set Dome Orientation";
                    target.DomeOrientation = (RenderToDisk.DomeOrientations)AxonGUI.FieldEnumPopup(target, "Orientation", target.DomeOrientation);
                    AxonGUI.EndBox();

                    FulldomeOptionsGUI();
                }

                AxonGUI.BeginBox();
                AxonGUI.UndoName = "Set Output Texture";
                AxonGUI.SetTooltip("Optionally assign a render texture matching the output resolution if you wish to perform additional processing. " +
                    "If not, this field may be left empty and a temporary render texture will be generated automatically. However, an output texture " +
                    "is required when using an overlay camera to capture a final image with post processing or other effects applied.");
                target.OutputTexture = (RenderTexture)AxonGUI.FieldObject(target, "Output Texture", target.OutputTexture, typeof(RenderTexture), true, false, GUILayout.Width(AxonGUI.LabelWidth + 70));
                if (target.CaptureOverlayCamera && target.OutputTexture == null) {
                    AxonGUI.Warning("This configuration requires assigning render texture asset matching the final output resolution.");
                }
                AxonGUI.EndBox();

                AxonGUI.EndBoxPadded();

                OverlayGUI();
            }
            AxonGUI.EndBox();
        }

        private void FulldomeOptionsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Dome Horizon";
            target.DomeHorizon = AxonGUI.FieldSlider(target, "Horizon", target.DomeHorizon, 90f, 360f);

            AxonGUI.UndoName = "Set Dome Tilt";
            target.DomeTilt = AxonGUI.FieldSlider(target, "Tilt", target.DomeTilt, 0f, 360f);

            AxonGUI.UndoName = "Set Dome Masked";
            target.DomeMasked = AxonGUI.FieldToggle(target, "Masked", target.DomeMasked);
            if (target.DomeMasked) {
                AxonGUI.UndoName = "Set Dome Mask Roundness";
                target.MaskRoundness = AxonGUI.FieldSlider(target, "Mask Roundness", target.MaskRoundness, 0f, 1f);

                AxonGUI.UndoName = "Set Dome Mask Softness";
                target.MaskSoftness = AxonGUI.FieldSlider(target, "Mask Softness", target.MaskSoftness, 0f, 1f);
            }
            AxonGUI.EndBox();

            if (target.IsStereo) {
                AxonGUI.BeginBox();
                AxonGUI.UndoName = "Set Post Process Stereo";
                target.UsePostStereoCameras = AxonGUI.FieldToggle(target, "Post Process Stereo", target.UsePostStereoCameras);
                if (target.UsePostStereoCameras) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Left Image";
                    target.ImageLeft = (RenderTexture)AxonGUI.FieldObject(target, "Left Image", target.ImageLeft, typeof(RenderTexture), true, false);
                    AxonGUI.UndoName = "Set Left Post Camera";
                    target.LeftCameraPost = (Camera)AxonGUI.FieldObjectInline(target, "Left Post Camera", target.LeftCameraPost, typeof(Camera), true);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Right Image";
                    target.ImageRight = (RenderTexture)AxonGUI.FieldObject(target, "Right Image", target.ImageRight, typeof(RenderTexture), true, false);
                    AxonGUI.UndoName = "Set Right Post Camera";
                    target.RightCameraPost = (Camera)AxonGUI.FieldObjectInline(target, "Right Post Camera", target.RightCameraPost, typeof(Camera), true);
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();
            }
        }

        private void TimeRangesGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTiming = AxonGUI.Foldout(target.EditorShowTiming, "Time Range");
            AxonGUI.FlexibleSpace();
            AxonGUI.UndoName = "Set Render Mode";
            AxonGUI.SetTooltip("Render a single frame range or multiple ranges. When rendering multiple ranges, each named sequence is saved in a separate subdirectory.");
            target.RenderMode = (RenderToDisk.RenderModes)AxonGUI.FieldEnumPopupInline(target, target.RenderMode, GUILayout.Width(AxonGUI.LabelWidth + 120));
            AxonGUI.EndHorizontal();

            if (target.EditorShowTiming) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                if (target.IsMultipleRanges) {
                    AxonGUI.SetTooltip("Removes all time ranges from the list below.");
                    if (AxonGUI.ButtonInline("Clear All") || target.Ranges == null || target.Ranges.Count == 0) {
                        target.ResetRanges();
                    }
                    if (AxonGUI.ButtonInline("Add Range")) {
                        RenderToDiskRange newRange = new RenderToDiskRange();
                        RenderToDiskRange copyRange = null;
                        if (target.Ranges.Count > 0) {
                            copyRange = target.Ranges[target.Ranges.Count - 1];
                        }
                        newRange.InitRange(true, copyRange);
                        target.Ranges.Add(newRange);
                    }
                    AxonGUI.SetTooltip("This automatically sets up new ranges based on the markers in the Timeflow view.");
                    if (AxonGUI.ButtonInline("Generate Ranges from Markers")) {
                        target.GenerateMarkerRanges();
                        EditorGUIUtility.ExitGUI();
                    }
                }
                AxonGUI.EndHorizontal();

                if (target.IsMultipleRanges) {
                    RangesGUI();
                }
                else
                if (target.IsSingleRange) {
                    RangeGUI(target.Range);
                }
                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void OverlayGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOverlay = AxonGUI.Foldout(target.EditorShowOverlay, "Overlay");
            if (target.EditorShowOverlay) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Overlay Camera";
                AxonGUI.SetTooltip("An additional camera may be used to either render as an overlay, or as a secondary pass to capture " +
                    "effects such as post processing.");
                target.OverlayCamera = (Camera)AxonGUI.FieldObject(target, "Overlay Camera", target.OverlayCamera, typeof(Camera), true);
                if (target.OverlayCamera == null) {
                    target.CaptureOverlayCamera = false;
                }
                else {
                    AxonGUI.UndoName = "Set Capture as Final";
                    AxonGUI.SetTooltip("If enabled, the final output from the Overlay Camera. Use this if the Overlay Camera is being used to apply post processing after the main render.");
                    target.CaptureOverlayCamera = AxonGUI.FieldToggleInline(target, "Capture as Final", target.CaptureOverlayCamera);
                }
                AxonGUI.EndHorizontal();


                AxonGUI.ShowPropertyObjectField = true;
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Current Time";
                AxonGUI.SetTooltip("Select a property field to display the current time.");
                target.HasCurrentTimeProperty = AxonGUI.FieldToggle(target, "Current Time", target.HasCurrentTimeProperty);
                if (target.HasCurrentTimeProperty) {
                    AxonGUI.PropertySelect(target, typeof(RenderToDisk), target.gameObject, target.CurrentTimeProperty);
                    AxonGUI.UndoName = "Set Current Timecode";
                    AxonGUI.SetTooltip("If enabled, the time is formated as timecode (00:00:00), otherwise it is displayed in decimal seconds (1.00)");
                    target.CurrentTimeAsTimecode = AxonGUI.FieldToggleInline(target, "Timecode", target.CurrentTimeAsTimecode);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Time Remaining";
                target.HasTimeRemainingProperty = AxonGUI.FieldToggle(target, "Time Remaining", target.HasTimeRemainingProperty);
                if (target.HasTimeRemainingProperty) {
                    AxonGUI.PropertySelect(target, typeof(RenderToDisk), target.gameObject, target.TimeRemainingProperty);
                    AxonGUI.UndoName = "Set Time Remaining Timecode";

                    AxonGUI.SetTooltip("If enabled, the time is formated as timecode (00:00:00), otherwise it is displayed in decimal seconds (1.00)");
                    target.TimeRemainingAsTimecode = AxonGUI.FieldToggleInline(target, "Timecode", target.TimeRemainingAsTimecode);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Frame Numbers";
                target.HasFrameNumbersProperty = AxonGUI.FieldToggle(target, "Frame Numbers", target.HasFrameNumbersProperty);
                if (target.HasFrameNumbersProperty) {
                    AxonGUI.PropertySelect(target, typeof(RenderToDisk), target.gameObject, target.FrameNumbersProperty);
                    AxonGUI.UndoName = "Set Frame Number Padded";
                    AxonGUI.SetTooltip("If enabled, the frame number is padded with zeros matching the output file settings.");
                    target.FrameNumbersPadded = AxonGUI.FieldToggleInline(target, "Padded", target.FrameNumbersPadded);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.ShowPropertyObjectField = false;

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void OptionsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOptions = AxonGUI.Foldout(target.EditorShowOptions, "Options");
            if (target.EditorShowOptions) {
                AxonGUI.BeginBoxPadded();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Hide On Render";
                AxonGUI.SetTooltip("Optionally assign a game object to hide on rendering. This may be useful to hide UI or other objects that shouldn't be rendered into the final image.");
                target.HideOnRender = (GameObject)AxonGUI.FieldObject(target, "Hide On Render", target.HideOnRender, typeof(GameObject), true);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Force Global LOD 0";
                AxonGUI.SetTooltip("Sets the global LOD level. Use this to force all game objects to render LOD 0 for highest quality output.");
                target.ForceLOD0 = AxonGUI.FieldToggle(target, "Force Global LOD 0", target.ForceLOD0);
                if (AxonGUI.ButtonInline("Apply Now")) {
                    ObjectUtil.SetGlobalLODLevel(0);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Log Every (frames)";
                AxonGUI.SetTooltip("Sets the frequency in which to write log messages to the console, which is helpful for monitoring render progress.");
                target.LogEvery = AxonGUI.FieldInt(target, "Log Every (frames)", target.LogEvery);

                AxonGUI.UndoName = "Set Suppress Video Logging";
                AxonGUI.SetTooltip("If enabled, log output from video encoding will not be displayed in the console, only reporting when encodings have finished.");
                target.SuppressVideoLog = AxonGUI.FieldToggleInline(target, "Suppress Video Logging", target.SuppressVideoLog);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Debug Pause";
                AxonGUI.SetTooltip("Use this to pause the render on a particular frame. This may be useful when troubleshooting render issues.");
                target.EnableDebugBreak = AxonGUI.FieldToggle(target, "Debug Pause", target.EnableDebugBreak);
                if (target.EnableDebugBreak) {
                    AxonGUI.UndoName = "Set Debug Pause On Frame";
                    target.DebugBreakOnFrame = AxonGUI.FieldIntInline(target, "On Frame", target.DebugBreakOnFrame);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void EventsGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.SetTooltip("Peform setup and setdown operations when rendering.");
            target.EditorShowEvents = AxonGUI.Foldout(target.EditorShowEvents, "Events");
            if (target.EditorShowEvents) {
                AxonGUI.BeginBoxPadded();

                EditorGUILayout.PropertyField(OnEnabled, new GUIContent("On Enabled"));
                EditorGUILayout.PropertyField(OnRenderStarted, new GUIContent("On Render Started"));
                EditorGUILayout.PropertyField(OnRenderAborted, new GUIContent("On Render Aborted"));
                EditorGUILayout.PropertyField(OnRenderFinished, new GUIContent("On Render Finished"));

                AxonGUI.EndBoxPadded();
            }
            AxonGUI.EndBox();
        }

        private void RangesGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.Space();
            AxonGUI.Indent++;

            if (target.Ranges == null) {
                target.Ranges = new List<RenderToDiskRange>();
                RenderToDiskRange newRange = new RenderToDiskRange();
                newRange.InitRange(true);
                target.Ranges.Add(newRange);
            }

            int moveUp = -1;
            int moveDown = -1;
            int insert = -1;
            int remove = -1;

            for (int x = 0; x < target.Ranges.Count; x++) {
                AxonGUI.BeginVertical("box");
                AxonGUI.BeginHorizontal();

                target.Ranges[x].EditorShowSettings = AxonGUI.FoldoutInline(target.Ranges[x].EditorShowSettings, null);

                if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Range", true)) {
                    insert = x;
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Range", true)) {
                    remove = x;
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                    moveUp = x;
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                    moveDown = x;
                }

                RangeGUI(target.Ranges[x]);

                AxonGUI.EndVertical();
            }

            if (remove > -1) {
                UndoUtil.Undo(target, "Remove Range", true);
                target.Ranges.RemoveAt(remove);
            }
            if (moveUp > 0) {
                UndoUtil.Undo(target, "Reorder Range", true);
                RenderToDiskRange a = target.Ranges[moveUp];
                RenderToDiskRange b = target.Ranges[moveUp - 1];
                target.Ranges[moveUp] = b;
                target.Ranges[moveUp - 1] = a;
            }
            if (moveDown >= 0 && moveDown < target.Ranges.Count - 1) {
                UndoUtil.Undo(target, "Reorder Range", true);
                RenderToDiskRange a = target.Ranges[moveDown];
                RenderToDiskRange b = target.Ranges[moveDown + 1];
                target.Ranges[moveDown] = b;
                target.Ranges[moveDown + 1] = a;
            }
            if (insert != -1) {
                UndoUtil.Undo(target, "Add Range", true);
                RenderToDiskRange newRange = new RenderToDiskRange();
                newRange.InitRange(true);
                target.Ranges.Insert(insert + 1, newRange);
            }
            AxonGUI.Indent--;
            AxonGUI.Space();
            AxonGUI.EndBox();
        }

        private void RangeGUI(RenderToDiskRange range)
        {
            if (range == null) return;
            bool fold = !range.EditorShowSettings;
            if (target.IsMultipleRanges) {
                AxonGUI.UndoName = "Set Enable Rendering";
                range.EnableRender = AxonGUI.FieldToggleInline(target, range.EnableRender);
            }
            else {
                fold = false;
                range.EnableRender = true;
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
            }

            AxonGUI.UndoName = "Set Filename";
            AxonGUI.SetTooltip("Enter the name for the time range, used for file naming. Please note that a new directory will be created for each range based on its file name.");
            if (target.IsMultipleRanges) {
                range.Name = AxonGUI.FieldTextInline(target, null, range.Name, true);
            }
            else {
                range.Name = AxonGUI.FieldText(target, "Filename", range.Name, true, GUILayout.MinWidth(300));
            }

            if (range.EnableRender) {
                AxonGUI.UndoName = "Set Frame Number Padding";
                AxonGUI.SetTooltip("Pad numbers with leading zeros for uniform naming. Set the number of places, for example 4 would number: 0001");
                range.FrameNumberPadding = AxonGUI.FieldIntInline(target, "# Padding", range.FrameNumberPadding);
                string preview = StringUtil.PadNumber(1, range.FrameNumberPadding) + target.FileExtension;
                preview = range.NamePrefix + range.Name + "_" + preview;
                AxonGUI.LabelInline(preview);
            }
            AxonGUI.EndHorizontal();

            if (range.EnableRender && !fold) {
                if (target.IsMultipleRanges) {
                    AxonGUI.BeginBox();
                }

                AxonGUI.BeginHorizontal();
                AxonGUI.BeginDisabledGroup(range.AutoOutputPath);
                AxonGUI.UndoName = "Set Output Path";
                range.OutputPath = AxonGUI.FieldText(target, "Output Path", range.OutputPath, true);
                AxonGUI.EndDisabledGroup();
                AxonGUI.SetTooltip("Turn off auto to set a custom output path.");
                range.AutoOutputPath = AxonGUI.FieldToggleInline(target, "Auto", range.AutoOutputPath);
                AxonGUI.EndHorizontal();

                if (range.StartTime == null) {
                    range.InitRange(true);
                }
                range.StartTime.Mode = TimeValue.Modes.Time;
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Start Time";
                AxonGUI.FieldTimeValue(target, "Start Time", range.StartTime);
                if (AxonGUI.ButtonInline("Get Time")) {
                    range.StartTime.TimeType = TimeValue.TimeTypes.Seconds;
                    range.StartTime.Time = Timeflow.Active.CurrentTime;
                }
                if (AxonGUI.ButtonInline("Goto Time")) {
                    Timeflow.Active.CurrentTime = range.StartTime.Time;
                }
                if (AxonGUI.ButtonInline("...")) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Full Duration"), false, SetRangeFullDuration, range);
                    menu.AddItem(new GUIContent("Use Work Area"), false, GetRangeWorkArea, range);
                    menu.AddItem(new GUIContent("Set Work Area"), false, SetRangeWorkArea, range);
                    menu.ShowAsContext();
                }
                AxonGUI.EndHorizontal();

                range.EndTime.Mode = TimeValue.Modes.Time;
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set End Time";
                AxonGUI.FieldTimeValue(target, "End Time", range.EndTime);
                if (AxonGUI.ButtonInline("Get Time")) {
                    range.EndTime.TimeType = TimeValue.TimeTypes.Seconds;
                    range.EndTime.Time = Timeflow.Active.CurrentTime;
                }
                if (AxonGUI.ButtonInline("Goto Time")) {
                    Timeflow.Active.CurrentTime = range.EndTime.Time;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                float duration = range.GetDuration();
                int frames = Mathf.RoundToInt(duration * target.FrameRate);
                float seconds = Mathf.Round(duration * 100f) / 100f;
                AxonGUI.Label("Duration", $"{seconds} Seconds ({frames} Frames)");
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Frame Handles";
                AxonGUI.SetTooltip("Renders additional frames before and after the start and end as extra padding for editing later.");
                range.FrameHandles = AxonGUI.FieldInt(target, "Frame Handles", range.FrameHandles);
                if (range.FrameHandles < 0) {
                    range.FrameHandles = 0;
                }
                AxonGUI.UndoName = "Set Perfect Loop";
                AxonGUI.SetTooltip("Enable this option when rendering a sequence whose starting and ending frames match. " +
                    "This subtracts the last frame so that the resuling sequence perfectly loops without a duplicate frame at the end.");
                range.PerfectLoop = AxonGUI.FieldToggleInline(target, "Pefect Loop", range.PerfectLoop);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Renumber Frames";
                AxonGUI.SetTooltip("Frame numbers use the Timeflow frame number unless renumbering is enabled.");
                range.RenumberFrames = AxonGUI.FieldToggle(target, "Renumber Frames", range.RenumberFrames);
                if (range.RenumberFrames) {
                    range.RenumberStartingAt = AxonGUI.FieldIntInline(target, "Starting At", range.RenumberStartingAt);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set PreRoll Frames";
                AxonGUI.SetTooltip("Prerolling is necessary when there are effects, physics, or other elements that need time to simulate. Specify a number of frames to adquequately warm up the scene for rendering.");
                range.PreRoll = AxonGUI.FieldToggle(target, "PreRoll Frames", range.PreRoll);
                if (range.PreRoll) {
                    AxonGUI.UndoName = "Set PreRoll Frames Starting At";
                    range.PreRollStartFrame = AxonGUI.FieldIntInline(target, "Starting At", range.PreRollStartFrame);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                if (!target.PreviewOnly) {
                    RangeVideoGUI(range);
                }

                if (HasGUIChanged) {
                    target.PrepareRange(range);
                }
            }
        }

        private void RangeVideoGUI(RenderToDiskRange range)
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Export Video";
            range.EnableVideoEncoding = AxonGUI.FieldToggle(target, "Export Video", range.EnableVideoEncoding);
            AxonGUI.Info("Video encoding is peformed -after- rendering frames using ffmepg. Each encoding is queued when the frames are " +
                "finished rendering and executed in a windowless shell. When rendering multiple outputs, each video encoding is queued to " +
                "be processed one at a time. When finished, results are displayed in the console log.");
            AxonGUI.ButtonDocs("Video Encoding Documentation", "https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/video-encoding");

            if (target.IsMultipleRanges) {
                AxonGUI.SetTooltip("Press this button to copy the video settings to all ranges.");
                if (AxonGUI.ButtonInline("Apply Settings to All Ranges")) {
                    UndoUtil.Undo(target, "Apply Settings to All Ranges", true);

                    foreach (RenderToDiskRange r in target.Ranges) {
                        if (r != range) {
                            r.EnableVideoEncoding = range.EnableVideoEncoding;
                            r.Encoding = range.Encoding;
                            r.VideoFilename = null; // forces refresh to range name
                            r.VideoHasAudio = range.VideoHasAudio;
                            r.AudioFilepath = range.AudioFilepath;
                            r.AutoAudioStartTime = range.AutoAudioStartTime;
                            r.AudioStartTime = range.AudioStartTime;
                            r.ShowProcessWindow = range.ShowProcessWindow;
                            target.PrepareRange(r);
                            r.SetupVideoEncoding();
                        }
                    }
                }
            }
            AxonGUI.EndHorizontal();

            if (range.EnableVideoEncoding) {
                if (target.IsStereoSaveSeparately) {
                    AxonGUI.HelpBox("Video encoding is not available when saving stereo eyes separately.");
                    AxonGUI.EndBox();
                    return;
                }
                if (target.IsStillFrame) {
                    AxonGUI.HelpBox("Video encoding is not available when saving still images.");
                    AxonGUI.EndBox();
                    return;
                }
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                if (range.EnableVideoEncoding) {
                    AxonGUI.BeginDisabledGroup(range.AutoVideoFilepath);
                    if (range.AutoVideoFilepath) range.VideoFilename = range.Name;
                    AxonGUI.UndoName = "Set Video Filename";
                    range.VideoFilename = AxonGUI.FieldText(target, "Video Filename", range.VideoFilename, true);
                    AxonGUI.EndDisabledGroup();

                    AxonGUI.UndoName = "Set File Overwrite";
                    AxonGUI.SetTooltip("If enabled, the video will replace any previously existing file. Otherwise if off, this encoding will be skipped if the file already exists.");
                    range.OverwriteVideo = AxonGUI.FieldToggleInline(target, "Overwrite", range.OverwriteVideo);

                    AxonGUI.UndoName = "Set Automatic Range Name";
                    AxonGUI.SetTooltip("If enabled, the video file has the same name as the range name. You may turn this off to customize the name.");
                    range.AutoVideoFilepath = AxonGUI.FieldToggleInline(target, "Auto", range.AutoVideoFilepath);
                }
                AxonGUI.EndHorizontal();

                if (range.EnableVideoEncoding) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.BeginDisabledGroup(range.AutoVideoFilepath);
                    AxonGUI.UndoName = "Set Video Output Path";
                    range.VideoOutputPath = AxonGUI.FieldText(target, "Output Path", range.VideoOutputPath, true);
                    if (AxonGUI.ButtonInline("Select Path")) {
                        range.VideoOutputPath = EditorUtility.OpenFolderPanel("Save to Directory", "", "");
                        target.CheckPaths();
                        EditorGUIUtility.ExitGUI();
                    }
                    AxonGUI.EndDisabledGroup();
                    AxonGUI.Info("For optimal speed, select a path on the fastest hard drive available. Disk writing times are the slowest part of the rendering process, so using a fast solid state drive is recommended.");
                    AxonGUI.EndHorizontal();


                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Include Audio";
                    range.VideoHasAudio = AxonGUI.FieldToggle(target, "Include Audio", range.VideoHasAudio);
                    if (range.VideoHasAudio) {
                        AxonGUI.UndoName = "Set Audio Filepath";
                        range.AudioFilepath = AxonGUI.FieldTextInline(target, null, range.AudioFilepath, true);
                        if (AxonGUI.ButtonInline("Select Path")) {
                            range.AudioFilepath = EditorUtility.OpenFilePanel("Select Audio File", "", "");
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                    AxonGUI.EndHorizontal();

                    if (range.VideoHasAudio) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Audio Start Time";
                        AxonGUI.SetTooltip("Specifies the start time in seconds in the audio file to sync with the timeline start frame. " +
                            "Leave at a value of 0 to align with the beginning of the Timeflow timeline.");
                        range.AudioStartTime = AxonGUI.FieldFloat(target, "Audio Start Time", range.AudioStartTime);

                        AxonGUI.UndoName = "Set Auto Adust Audio to Output Range";
                        AxonGUI.SetTooltip("If auto is enabled, the audio is automatically synced to the output based on the frame range being rendered.");
                        range.AutoAudioStartTime = AxonGUI.FieldToggleInline(target, "Auto Adjust to Range", range.AutoAudioStartTime);
                        AxonGUI.EndHorizontal();
                    }

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Video Encoding";
                    AxonGUI.SetTooltip("Please assign a Video Encoding asset to configure video settings with ffmpeg. You may also create a new one from the context menu " +
                        "in the Project view: Timeflow/New Video Encoding");
                    range.Encoding = (VideoEncoding)AxonGUI.FieldObject(target, "Video Encoding", range.Encoding, typeof(VideoEncoding), false);
                    if (range.Encoding == null) {
                        string path = AssetDatabase.GUIDToAssetPath("1fa5046bc1cc57a4680057fbeca1eb9f"); //Settings/VideoEncoding/H264_HighQuality.asset
                        range.Encoding = AssetDatabase.LoadAssetAtPath(path, typeof(VideoEncoding)) as VideoEncoding;
                        if (range.Encoding == null) {
                            AxonGUI.Warning("Please assign a Video Encoding asset to configure encoding.");
                        }
                    }

                    AxonGUI.UndoName = "Set Show Window";
                    AxonGUI.SetTooltip("If enabled, the shell process window will be displayed. This option is helpful if you wish to spawn and monitor encodings without " +
                        "having to wait for them to finish in the editor. Once you exit play mode, any encodings not yet finished will continue in their shell window but " +
                        "the log output will not longer be displayed in Unity's console window.");
                    range.ShowProcessWindow = AxonGUI.FieldToggleInline(target, "Show Window", range.ShowProcessWindow);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Metadata";
                    AxonGUI.SetTooltip("If enabled, metadata is automatically added for sterescopic and VR rendering modes.");
                    range.Metadata = AxonGUI.FieldText(target, "Metadata", range.Metadata, true);

                    AxonGUI.UndoName = "Set Metadata Auto";
                    range.AutoMetadata = AxonGUI.FieldToggleInline(target, "Auto", range.AutoMetadata);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    if (range.VideoItem == null) range.SetupVideoEncoding();
                    if (range.VideoItem != null) {
                        AxonGUI.SetTooltip("This displays the ffmepg command used for encoding. The field is read only. To make changes, adjust the settings below.");
                        string cmd = "ffmpeg " + range.VideoItem.Command;
                        AxonGUI.FieldText(target, "Command Line", cmd, true);
                        if (AxonGUI.ButtonInline("Copy")) {
                            EditorGUIUtility.systemCopyBuffer = cmd;
                        }
                        if (AxonGUI.ButtonInline("Refresh")) {
                            target.PrepareRange(range);
                            range.SetupVideoEncoding();
                        }
                    }
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();

            }
            AxonGUI.EndBox();
        }

        public static void SetRangeFullDuration(object obj)
        {
            RenderToDiskRange range = (RenderToDiskRange)obj;
            range.StartTime.TimeType = TimeValue.TimeTypes.Start;
            range.EndTime.TimeType = TimeValue.TimeTypes.End;
        }

        public static void GetRangeWorkArea(object obj)
        {
            RenderToDiskRange range = (RenderToDiskRange)obj;
            range.StartTime.TimeType = TimeValue.TimeTypes.WorkAreaStart;
            range.EndTime.TimeType = TimeValue.TimeTypes.WorkAreaEnd;
        }

        public static void SetRangeWorkArea(object obj)
        {
            RenderToDiskRange range = (RenderToDiskRange)obj;
            Timeflow.Active.SetWorkArea(range.StartTime.Time, range.EndTime.Time, true);
        }
    }

}//AxonGenesis

#endif

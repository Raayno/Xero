// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Renders to sequenced image files. Note that this component is not a TimeflowBehavior so that it
    /// operates outside of and can control Timeflow. This game object does not need to be added to
    /// Timeflow nor does it need a TimeflowObject. However, there is no harm in adding RenderToDisk to
    /// Timeflow and it does permit the MainCamera to be animated (to switch between cameras while
    /// rendering explicitly). RenderToDisk may also be used entirely without Timeflow present.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Timeflow/Rendering/Render to Disk")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-to-disk")]
    sealed public class RenderToDisk : AxonGenesisBehavior
    {
        public static bool IsRendering { get; private set; }

        #region ENUMS

        public enum OutputFormats
        {
            Standard,
            Fulldome,
            VR360,
            VR180
        }

        public enum FileFormats
        {
            PNG,
            JPEG,
            EXR,
            TGA
        }

        public enum RenderModes
        {
            SingleRange,
            MultipleRanges,
            StillFrames
        }

        public enum StereoscopicFormats
        {
            LeftRight,
            TopBottom,
            SaveSeparately
        }

        public enum DomeOrientations
        {
            Fisheye = 0,
            Fulldome = 1,
        }

        public enum DomeResolutions
        {
            Domemaster1k = 1024,
            Domemaster2k = 2048,
            Domemaster3k = 3072,
            Domemaster4k = 4096,
        }

        [Flags]
        public enum CubemapFaces
        {
            None = 0,
            PositiveX = 1 << 0,
            NegativeX = 1 << 1,
            PositiveY = 1 << 2,
            NegativeY = 1 << 3,
            PositiveZ = 1 << 4,
            NegativeZ = 1 << 5
        }

        public enum EditorPreviewSizes
        {
            Eighth,
            Quarter,
            Half,
            Full
        }

        #endregion

#if UNITY_EDITOR
        public static RenderToDisk Instance { get; private set; }

        public static bool EnableEditorRendering = false; // disabled because it doesn't work in Unity 2019

        #region PUBLIC SERIALIZED

        #region OUTPUT
        public OutputFormats OutputFormat = OutputFormats.Standard;

        public FileFormats FileFormat = FileFormats.JPEG;
        public float JPEGQuality = 100f;
        public bool OutputAlpha = true;

        public Texture2D.EXRFlags EXRFlags = Texture2D.EXRFlags.CompressRLE;

        public bool PreviewOnly;
        public EditorPreviewSizes EditorPreviewSize = EditorPreviewSizes.Full;
        public bool OpenDirectoryWhenFinished = true;
        public string SaveToDirectory = "";
        public string SaveToDirectoryRight = "";
        public string SaveToDirectoryStills = "";
        public string FilenamePrefix = "";
        public string FilenamePrefix2 = "";
        public bool FilenamePrefixUseSceneName = false;
        public int Framestep = 1;

        public int Subframes;
        public float SubframeScale = 1f;
        public float TimeScale = 1f;

        public bool UseScreenCapture = true;

        [FormerlySerializedAs("IsVRStereo")]
        public bool IsStereo;

        [FormerlySerializedAs("VRStereoSeparation")]
        public float StereoSeparation = 0.064f;

        [FormerlySerializedAs("SetOutputSize")]
        public bool SetCaptureSize;

        [FormerlySerializedAs("OutputSizeOverride")]
        public Vector2 OverrideCaptureSize = new Vector2(1920, 1080);

        public StereoscopicFormats StereoscopicFormat = StereoscopicFormats.TopBottom;
        #endregion

        #region CAMERA
        public bool UseActiveCamera = true;
        public Camera _MainCamera; // Left
        public Camera RightCamera;

        public bool UsePostStereoCameras = false;
        public Camera LeftCameraPost; // Left
        public Camera RightCameraPost;

        public TextureFormat TexFormat = TextureFormat.ARGB32;
        public RenderTextureFormat RenderTexFormat = RenderTextureFormat.ARGB32;
        public bool TexFormatHDR = false;

        public RenderTexture CubemapLeft;
        public RenderTexture CubemapRight;
        public RenderTexture ImageLeft;
        public RenderTexture ImageRight;
        public RenderTexture OutputTexture;

        public DomeOrientations DomeOrientation = DomeOrientations.Fulldome;

        public CubemapFaces CubemapFace = (CubemapFaces)63; // all sides

        public float DomeHorizon = 180f;
        public float DomeTilt;
        public bool DomeMasked;
        public float MaskRoundness = 1f;
        public float MaskSoftness = 0;
        #endregion

        #region TIMING
        public RenderModes RenderMode = RenderModes.SingleRange;
        public bool AutoStart = true;
        public KeyCode KeyPress = KeyCode.F1;

        public float FrameRate = 60f;
        public bool UseTimeflowFrameRate = true;

        public List<RenderToDiskRange> Ranges = null;
        #endregion

        #region OVERLAY
        public Camera OverlayCamera;
        public bool CaptureOverlayCamera;

        public bool HasCurrentTimeProperty = false;
        public Property CurrentTimeProperty = null;
        public bool CurrentTimeAsTimecode = true;

        public bool HasTimeRemainingProperty = false;
        public Property TimeRemainingProperty = null;
        public bool TimeRemainingAsTimecode = true;

        public bool HasFrameNumbersProperty = false;
        public Property FrameNumbersProperty = null;
        public bool FrameNumbersPadded = true;

        #endregion


        #region EXTRAS
        public bool ForceLOD0 = true;
        public GameObject HideOnRender;
        public bool EnableDebugBreak;
        public int DebugBreakOnFrame = 100;
        public int LogEvery = 10;
        public bool SuppressVideoLog = false;

        public RenderToDisk RenderNext = null;
        #endregion

        #region CALLBACKS

        public bool EditorShowEvents = false;
        public UnityEvent OnEnabled = null;
        public UnityEvent OnRenderStarted = null;
        public UnityEvent OnRenderAborted = null;
        public UnityEvent OnRenderFinished = null;

        #endregion

        public float TimeRemaining;

        public bool EditorShowCamera = true;
        public bool EditorShowOutput = true;
        public bool EditorShowTiming = true;
        public bool EditorShowOverlay;
        public bool EditorShowOptions;

        #endregion

        #region PUBLIC NON-SERIALZIED 

        [NonSerialized]
        public float RenderProgress;

        [NonSerialized]
        public string RenderStatus = "Not rendering";

        [NonSerialized]
        public string RenderName = "";

        [NonSerialized]
        public bool HasBeenStarted;

        #endregion

        #region PRIVATE NON-SERIALZIED 

        [NonSerialized]
        private bool canOutput = true;

        [NonSerialized]
        private int frameNumber = 1;

        [NonSerialized]
        private int frameNumberOutput;

        [NonSerialized]
        private Texture2D imageBuffer;

        [NonSerialized]
        private int frameLogCount;

        [NonSerialized]
        private DateTime startTime = DateTime.Now;

        [NonSerialized]
        private int currentRange = 0;

        [NonSerialized]
        private int subframe;

        [NonSerialized]
        private bool isSubframe;

        [NonSerialized]
        private float subframeTime;

        [NonSerialized]
        private bool hasStarted;

        [NonSerialized]
        private bool pauseRequested;

        [NonSerialized]
        private Material _fisheyeMaterial;

        private string filenamePrefix = "";

        private bool deallocImageBuffer = false;

        #endregion

        #region ACCESSORS

        public Timeflow Timeflow => Timeflow.Active;

        public int RenderStartFrame {
            get {
                int f = Range.StartTime.Frame - Range.FrameHandles;
                if (f < 0) f = 0;
                return f;
            }
        }

        public int RenderEndFrame => Range.EndTime.Frame + Range.FrameHandles - (Range.PerfectLoop ? 1 : 0);

        public Camera MainCamera {
            get {
                if (UseActiveCamera) {
                    if (Camera.current != null) return Camera.current;
                    else
                    if (Camera.main != null) return Camera.main;
                }
                return _MainCamera;
            }
            set {
                _MainCamera = value;
            }
        }

        public Vector2 CaptureSize { get; set; }

        public Vector2 FinalSize {
            get {
                Vector2 s = CaptureSize;
                if (IsStereo && !IsVR360) {
                    if (IsStereoTopBottom) {
                        s.y *= 2f;
                    }
                    else
                    if (IsStereoLeftRight) {
                        s.x *= 2f;
                    }
                }
                return s;
            }
        }

        public string FileExtension {
            get {
                if (FileFormat == FileFormats.PNG) {
                    return ".png";
                }
                else
                if (FileFormat == FileFormats.EXR) {
                    return ".exr";
                }
                else
                if (FileFormat == FileFormats.TGA) {
                    return ".tga";
                }
                else {
                    return ".jpg";
                }
            }
        }

        public string OutputPath { get; private set; }

        public bool IsStillFrame => RenderMode == RenderModes.StillFrames;

        public bool IsSingleRange => RenderMode == RenderModes.SingleRange;

        public bool IsMultipleRanges => RenderMode == RenderModes.MultipleRanges;

        public bool IsStandard => OutputFormat == OutputFormats.Standard;

        public bool IsFulldome => OutputFormat == OutputFormats.Fulldome;

        public bool IsVR360 => OutputFormat == OutputFormats.VR360;

        public bool IsVR180 => OutputFormat == OutputFormats.VR180;

        public bool IsStereoSaveSeparately => StereoscopicFormat == StereoscopicFormats.SaveSeparately;

        public bool IsStereoLeftRight => IsStereo && StereoscopicFormat == StereoscopicFormats.LeftRight;

        public bool IsStereoTopBottom => IsStereo && StereoscopicFormat == StereoscopicFormats.TopBottom;

        private bool IsFisheye => DomeOrientation == DomeOrientations.Fisheye;

        public RenderToDiskRange Range {
            get {
                if (Ranges == null || Ranges.Count == 0) {
                    Ranges = new List<RenderToDiskRange> {
                        new RenderToDiskRange()
                    };
                }
                else
                if (Ranges[0] == null) {
                    Ranges[0] = new RenderToDiskRange();
                }
                if (currentRange < 0) currentRange = 0;
                else
                if (currentRange >= Ranges.Count) currentRange = Ranges.Count - 1;
                return Ranges[currentRange];
            }
        }

        public int NthFrame { get; private set; }

        public int CubemapFaceMask => (int)CubemapFace;

        private Material FisheyeMaterial {
            get {
                if (_fisheyeMaterial == null) {
                    Shader shader = Shader.Find("Axon Genesis/CubemapToDome");
                    if (shader != null) _fisheyeMaterial = new Material(shader);
                }
                return _fisheyeMaterial;
            }
        }

        #endregion

        #region INIT

        protected override void OnEnable()
        {
            base.OnEnable();
            Instance = this;
            if (OnEnabled != null) OnEnabled.Invoke();
            Prepare();
        }

        protected override void OnDestruct()
        {
            Instance = null;
            base.OnDestruct();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (!Application.isPlaying) return;
            if (AutoStart) {
                // Select the game object so that the inspector shows progress automatically
                SelectionUtil.Select(gameObject);
                if (!IsStillFrame && AutoStart) {
                    StartRendering();
                }
            }
        }

        #endregion

        #region SETUP

        public void Prepare()
        {
            if (Application.isPlaying && !IsStillFrame && Timeflow != null) {
                Timeflow.LoopEnabled = false; // disable loop so it doesn't interfer with frame ranges
            }

            if (Ranges == null || Ranges.Count == 0) {
                ResetRanges();
            }

            CheckPaths();
            UpdateCaptureSize();

            Time.captureFramerate = 0;
            //imageBuffer = null;
            NthFrame = Framestep;
            frameNumber = 0;
            frameNumberOutput = frameNumber;

            if (FrameRate <= 0) FrameRate = 1;
            if (Subframes < 0) Subframes = 0;
            if (Subframes > 0) {
                subframeTime = SubframeScale * ((1f / FrameRate) / (Subframes + 1f));
            }
            else subframeTime = 0;

            if (TexFormatHDR) {
                TexFormat = TextureFormat.RGBAFloat;
                RenderTexFormat = RenderTextureFormat.ARGBFloat;
            }
            else
            if (OutputAlpha && FileFormat != FileFormats.JPEG) {
                TexFormat = TextureFormat.ARGB32;
                RenderTexFormat = RenderTextureFormat.ARGB32;
            }
            else {
                TexFormat = TextureFormat.RGB24;
                RenderTexFormat = RenderTextureFormat.Default;
            }
            //if (DebugEnabled) Debug.Log($"TexFormatHDR:{TexFormatHDR} format:{TexFormat}");

            if ((IsVR360 || (IsVR180 && IsStereo)) && (CubemapLeft == null || (IsStereo && CubemapRight == null))) {
                Debug.LogWarning("RenderToDisk: Please assign the cubemap render textures");
                canOutput = false;
            }
            else
            if (IsStereo && RightCamera == null) {
                Debug.LogWarning("RenderToDisk: Please assign the Right Camera");
                canOutput = false;
            }
            else
            if (string.IsNullOrEmpty(SaveToDirectory)) {
                canOutput = false;
                Debug.LogWarning("RenderToDisk: Please specify an output path");
            }
        }

        private void AllocateBuffer()
        {
            if (!Application.isPlaying) return;
            if (imageBuffer == null && !UseScreenCapture) {
                imageBuffer = new Texture2D((int)FinalSize.x, (int)FinalSize.y, TexFormat, false);
                imageBuffer.filterMode = FilterMode.Bilinear;
                //if (DebugEnabled) Debug.Log("RenderToDisk.AllocateBuffer:" + imageBuffer.width + " x " + imageBuffer.height + " TexFormat:" + TexFormat);
            }
        }

        private void DeallocateBuffer()
        {
            if (!Application.isPlaying) return;
            if (imageBuffer != null) {
                UnityEngine.Object.Destroy(imageBuffer);
                imageBuffer = null;
                //if (DebugEnabled) Debug.Log("RenderToDisk.RenderFrame: DeallocateBuffer");
            }
        }

        #endregion

        #region PATHS

        public void CheckPaths()
        {
            if (Range == null) return;
            if (string.IsNullOrEmpty(SaveToDirectory)) {
                SaveToDirectory = "";
            }
            SaveToDirectory = PathUtil.Clean(SaveToDirectory);

            if (string.IsNullOrEmpty(SaveToDirectoryStills)) {
                SaveToDirectoryStills = SaveToDirectory;
            }
            else {
                SaveToDirectoryStills = PathUtil.Clean(SaveToDirectoryStills);
            }
            string assets = PathUtil.Separator + "Assets" + PathUtil.Separator;
            if (SaveToDirectory.Contains(assets) || (!string.IsNullOrEmpty(Range.VideoOutputPath) && Range.VideoOutputPath.Contains(assets))) {
                AxonGUI.Warning("Rendering to the Assets folder will result in the image files being imported into the project. To avoid this, it is recommended to render to a path outside of the Assets directory.");
            }

            if (IsStillFrame && !string.IsNullOrEmpty(SaveToDirectoryStills)) {
                OutputPath = SaveToDirectoryStills;
            }
            else {
                OutputPath = SaveToDirectory;
            }
            OutputPath = PathUtil.Wildcards(OutputPath);

            if (FilenamePrefixUseSceneName) {
                FilenamePrefix = SceneManager.GetActiveScene().name;
            }
            filenamePrefix = FilenamePrefix + FilenamePrefix2;

            if (Ranges != null && Ranges.Count > 0) {
                foreach (RenderToDiskRange range in Ranges) {
                    if (range.Name == null) range.Name = "";
                    else range.Name = PathUtil.Clean(range.Name, false);

                    if (!string.IsNullOrEmpty(range.VideoFilename)) range.VideoFilename = PathUtil.Clean(range.VideoFilename, false);
                    else range.VideoFilename = "Output";

                    if (string.IsNullOrEmpty(range.VideoOutputPath) || range.AutoVideoFilepath) {
                        range.NamePrefix = filenamePrefix;
                        range.OutputPath = PathUtil.Clean(OutputPath + filenamePrefix + range.Name);
                        range.VideoOutputPath = OutputPath;
                        range.SetupVideoEncoding();
                    }
                    if (!string.IsNullOrEmpty(range.AudioFilepath)) range.AudioFilepath = PathUtil.Clean(range.AudioFilepath);
                }
            }
        }

        private void PrepareOutputPath()
        {
            CheckPaths();

            if (Application.isPlaying) {
                if (!Directory.Exists(OutputPath)) {
                    Directory.CreateDirectory(OutputPath);
                }
            }
        }

        public string GetOutputFilepath(bool isRightEye = false)
        {
            string filepath = null;
            string eye = "";
            string dir = PathUtil.Clean(OutputPath + filenamePrefix + Range.Name);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            if (IsStereo && IsStereoSaveSeparately) {
                eye = isRightEye ? "_R" : "_L";
                dir = isRightEye ? SaveToDirectoryRight : dir;
            }
            string path = dir + filenamePrefix + Range.Name + eye + "_";

            if (IsStillFrame) {
                filepath = path + FileExtension;
                // Skip existing files on disk
                int f = 0;
                while (File.Exists(filepath)) {
                    f++;
                    filepath = path + StringUtil.PadNumber(f, Range.FrameNumberPadding) + FileExtension;
                }
            }
            else {
                filepath = path + StringUtil.PadNumber(frameNumberOutput, Range.FrameNumberPadding) + FileExtension;
            }
            return filepath;
        }

        public void ResetAllPaths()
        {
            PrepareOutputPath();
            foreach (RenderToDiskRange range in Ranges) {
                range.AutoOutputPath = true;
                range.NamePrefix = filenamePrefix;
                range.OutputPath = PathUtil.Clean(OutputPath + filenamePrefix + range.Name);
                range.VideoOutputPath = OutputPath;
                range.SetupVideoEncoding();
            }
        }

        #endregion

        #region RANGES

        public void ResetRanges()
        {
            Ranges = new List<RenderToDiskRange>();
            Ranges.Add(new RenderToDiskRange());
        }

        public void PrepareRange(RenderToDiskRange range)
        {
            PrepareOutputPath();

            range.GetDuration();

            range.FrameRate = FrameRate;
            if (range.AutoOutputPath) {
                range.OutputPath = PathUtil.Clean(OutputPath + filenamePrefix + range.Name);
            }
            range.FileExtension = FileExtension;

            if (range.AutoMetadata) {
                if (IsStereoLeftRight) {
                    range.Metadata = "-metadata:s:v stereo_mode=left_right";
                }
                else
                if (IsStereoTopBottom) {
                    range.Metadata = "-metadata:s:v stereo_mode=top_bottom";
                }
                else {
                    range.Metadata = "";
                }
            }
        }

        private void UpdateRange()
        {
            //if (DebugEnabled) Debug.Log($"UpdateRange:{Range.Name}");
            PrepareRange(Range);

            frameNumber = Range.PreRoll ? Range.PreRollStartFrame : RenderStartFrame;
            frameNumberOutput = Range.RenumberFrameStart;

            if (Timeflow != null) Timeflow.SetTime((float)frameNumber / (float)FrameRate);

            PrepareOutputPath();

            //if (DebugEnabled) Debug.Log("RenderToDisk.UpdateRange:" + " frameNumber:" + frameNumber + " start:" + RenderStartFrame + " end:" + RenderEndFrame);
        }

        private void RangeFinished()
        {
            if (PreviewOnly) return;

            //if (DebugEnabled) Debug.Log($"RangeFinished:{Range.Name}");

            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
            float timeElapsed = (float)elapsed.TotalSeconds;
            RenderQueue.RenderRangeFinished(SceneManager.GetActiveScene().name + ":" + name, timeElapsed);

            if (RenderQueue.LogEnabled) {
                string renderStats = $"{SceneManager.GetActiveScene().name}: {Range.Name}\n";
                renderStats += $"Frames:{Range.EndTime.Frame - Range.StartTime.Frame}\n";
                renderStats += $"Render Time:{StringUtil.SecondsToTimecode(timeElapsed)}\n";
                File.WriteAllText(OutputPath + filenamePrefix + Range.Name + "_Stats.txt", renderStats);
            }

            // Reset time for next render range if any
            startTime = DateTime.Now;

            if (OpenDirectoryWhenFinished) {
                EditorUtility.RevealInFinder(Range.OutputPath);
            }
            if (Range.EnableVideoEncoding) {
                Range.QueueVideoEncoding();
            }
        }

        #endregion

        #region CONTROL

        public void StartRendering()
        {
            if (!Application.isPlaying || !enabled || !canOutput) return;
            if (IsStillFrame) return;
            if (Ranges == null || Ranges.Count == 0) {
                Debug.LogError("No render frame ranges have been defined");
                return;
            }
            //if (DebugEnabled) Debug.Log($"RenderToDisk.StartRendering:{Ranges.Count}");

            Application.runInBackground = true;
            VideoQueue.SuppressLog = SuppressVideoLog;

            Time.timeScale = 0; // pause all rendering until ready
            Time.captureFramerate = (int)FrameRate;

            StopAllCoroutines();
            UpdateCaptureSize();

            HasBeenStarted = true;
            currentRange = 0;
            while (!Ranges[currentRange].EnableRender) {
                currentRange++;
                if (currentRange > Ranges.Count - 1) {
                    Debug.LogWarning("No ranges have been enabled to render!");
                    return;
                }
            }
            UpdateRange();

            if (Timeflow != null) Timeflow.CurrentTimeExact = (float)RenderStartFrame / (float)FrameRate;
            frameNumber = Range.PreRoll ? Range.PreRollStartFrame : RenderStartFrame;
            frameNumberOutput = Range.RenumberFrameStart;

            if (Subframes < 0) Subframes = 0;
            if (Subframes > 0) {
                subframeTime = SubframeScale * ((1f / FrameRate) / (Subframes + 1f));
            }
            else subframeTime = 0;

            if (UseTimeflowFrameRate && Timeflow != null) {
                FrameRate = Timeflow.FPS;
            }
            if (FrameRate <= 0) FrameRate = 1;

            startTime = DateTime.Now;
            TimeRemaining = Range.EndTime.Frame - Range.StartTime.Frame; // assume 1 second per frame initially

            //if (DebugEnabled) Debug.Log("RenderToDisk.Time.timeScale:" + Time.timeScale + " fps:" + Time.captureFramerate);

            if (HideOnRender != null) {
                HideOnRender.SetActive(false);
            }
            if (Timeflow != null && Timeflow.HideObjectOnPlay != null) {
                Timeflow.HideObjectOnPlay.SetActive(false);
            }
            if (ForceLOD0) {
                ObjectUtil.SetGlobalLODLevel(0);
            }

            PrepareOutputPath();
            UpdateOverlay();
            StartCoroutine(StartRecordingDelayed());
        }

        private IEnumerator StartRecordingDelayed()
        {
            IsRendering = false;

            // Allow extra time to give unity time to update
            yield return new WaitForSecondsRealtime(0.5f);

            UpdateCaptureSize();

            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = TimeScale;


            if (Timeflow != null) {
                /// Take control over playback when rendering ranges
                Timeflow.SetTime((float)RenderStartFrame / (float)FrameRate);
                Timeflow.Stop();

                if (Range.PreRoll) {
                    //if (DebugEnabled) Debug.Log("RenderToDisk.Preroll:" + Range.PreRollStartFrame);
                    Timeflow.SetTime((float)Range.PreRollStartFrame / (float)FrameRate);
                }
                else {
                    //if (DebugEnabled) Debug.Log("RenderToDisk.StartFrame:" + RenderStartFrame);
                    Timeflow.SetTime((float)RenderStartFrame / (float)FrameRate);
                }
                //if (DebugEnabled) Debug.Log("RenderToDisk.StartRendering: Time:" + Timeflow.CurrentTime);
                Timeflow.Play(false);
            }
            if (OnRenderStarted != null) OnRenderStarted.Invoke();
            IsRendering = true;
            yield break;
        }

        private void RenderFinished()
        {
            //if (DebugEnabled) Debug.Log($"RenderFinished!");
            RenderProgress = 1f;
            IsRendering = false;

            if (IsStillFrame) {
                if (Timeflow != null) Timeflow.Resume();
            }
            else {
                if (Timeflow != null) Timeflow.Stop();
            }

            if (OnRenderFinished != null) OnRenderFinished.Invoke();

            if (!IsStillFrame) {
                StopAllCoroutines();
                gameObject.SetActive(false);

                RenderQueue.RenderFinished(RenderNext);
            }

        }

        public void Abort()
        {
            Debug.LogWarning("Rendering Aborted!");
            StopAllCoroutines();
            IsRendering = false;
            Time.captureFramerate = 0;

            if (Range.VideoItem != null) {
                Range.VideoItem.Aborted = true;
            }

            if (OnRenderAborted != null) OnRenderAborted.Invoke();

            if (!IsStillFrame) {
                // Continue playing in still frame mode
                EditorApplication.ExitPlaymode();
            }
        }

        public void AbortAfterCurrentRender()
        {
            RenderQueue.AbortAfterCurrentRender = true;
        }

        public void Pause()
        {
            pauseRequested = true;
        }

        private void PauseImmediate()
        {
            pauseRequested = false;
            IsRendering = false;
            //Time.timeScale = 0f; // fully pauses updates
            Time.timeScale = 1f; // Allow normal operation
            Time.captureFramerate = 0;
        }

        public void Resume()
        {
            IsRendering = true;
            Time.timeScale = TimeScale;
            Time.captureFramerate = (int)FrameRate;
        }

        public void RenderStill()
        {
            IsRendering = true;
        }

        #endregion

        #region UPDATE

        public void UpdateCaptureSize()
        {
            if (SetCaptureSize && enabled && gameObject.activeInHierarchy) {
                Vector2 gameViewSize = OverrideCaptureSize;
                if (!Application.isPlaying && EditorPreviewSize != EditorPreviewSizes.Full) {
                    if (EditorPreviewSize == EditorPreviewSizes.Half) {
                        gameViewSize.x = gameViewSize.x / 2f;
                        gameViewSize.y = gameViewSize.y / 2f;
                    }
                    else
                    if (EditorPreviewSize == EditorPreviewSizes.Quarter) {
                        gameViewSize.x = gameViewSize.x / 4f;
                        gameViewSize.y = gameViewSize.y / 4f;
                    }
                    else
                    if (EditorPreviewSize == EditorPreviewSizes.Eighth) {
                        gameViewSize.x = gameViewSize.x / 8f;
                        gameViewSize.y = gameViewSize.y / 8f;
                    }
                }
                // Force the game view to zoom out fully to show the full image
                GameViewUtil.SetSizeAndScale(gameViewSize, 0);
            }
            CaptureSize = GameViewUtil.GetSize();
        }

        private void Update()
        {
            if (!Application.isPlaying || !enabled || !canOutput) return;
            if (pauseRequested) {
                PauseImmediate();
                return;
            }

            if (EnableDebugBreak) {
                if (frameNumber == DebugBreakOnFrame) {
                    Debug.Log("===== DEBUG PAUSE FRAME:" + frameNumber + " ====== Unpause to continue...");//--KEEP
                    Debug.Break();
                }
            }
            if (IsRendering) {
                UpdateCaptureSize();
                if (Timeflow != null) Timeflow.IsPlaying = false;
                if (IsStillFrame) {
                    isSubframe = false;
                    if (Timeflow != null) {
                        Range.EndTime.Frame = Range.StartTime.Frame = frameNumber = Timeflow.CurrentFrame;
                    }
                    else {
                        Range.EndTime.Frame = Range.StartTime.Frame = frameNumber = (int)(FrameRate * Time.time);
                    }
                }
                else {
                    if (Range.PreRoll && frameNumber < Range.PreRollStartFrame) frameNumber = Range.PreRollStartFrame;
                    float t = (float)(frameNumber) * (1f / FrameRate);
                    if (subframe < Subframes && Subframes > 1) {
                        t += subframe * subframeTime;
                        isSubframe = true;
                        subframe++;
                    }
                    else {
                        t = (float)(frameNumber + 1f) * (1f / FrameRate);
                        isSubframe = false;
                        subframe = 0;
                    }
                    if (Timeflow != null) Timeflow.CurrentTimeExact = t;
                }
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || !enabled || !canOutput) return;
            if (IsRendering && !isSubframe) {
                //if (DebugEnabled) Debug.Log("RenderToDisk.OnLateUpdate: frameNumber:" + frameNumber + " step:" + Framestep);
                if (CanContinue()) {
                    StartCoroutine(RenderFrame());
                    NthFrame--;
                    if (NthFrame <= 0) NthFrame = Framestep;
                }
                else {
                    RenderFinished();
                }
                UpdateRenderStatus();
            }
        }

        private void UpdateRenderStatus()
        {
            if (IsStillFrame) {
                RenderStatus = "Render Still Frame " + frameNumber;
                RenderName = RenderName + "_" + frameNumber;
            }
            else {
                if (frameNumber < RenderStartFrame && RenderStartFrame > Range.PreRollStartFrame) {
                    RenderProgress = -1f * (frameNumber - Range.PreRollStartFrame) / (RenderStartFrame - Range.PreRollStartFrame);
                    RenderStatus = "Preroll Frame " + frameNumber + " starting at " + RenderStartFrame;
                }
                else
                if (RenderEndFrame > RenderStartFrame) {
                    RenderProgress = (frameNumber - (float)RenderStartFrame) / (float)(RenderEndFrame - RenderStartFrame);
                    RenderStatus = "Frame " + frameNumber + " from " + RenderStartFrame + " to " + RenderEndFrame;
                }

                if (IsMultipleRanges) {
                    if (Ranges != null && Ranges.Count > 0) {
                        RenderName = Range.NamePrefix + Range.Name + " (" + (currentRange + 1) + " of " + Ranges.Count + ")";
                    }
                }
            }
        }

        private void UpdateOverlay()
        {
            if (HasCurrentTimeProperty && CurrentTimeProperty != null) {
                float currentTime = (float)(frameNumber) / FrameRate;
                if (CurrentTimeProperty.IsString) {
                    CurrentTimeProperty.StringValue = $"{(CurrentTimeAsTimecode ? StringUtil.SecondsToTimecode(currentTime) : currentTime.ToString())}";
                }
                else {
                    CurrentTimeProperty.FloatValue = currentTime;
                }
            }
            if (HasFrameNumbersProperty && FrameNumbersProperty != null) {
                if (FrameNumbersProperty.IsString) {
                    FrameNumbersProperty.StringValue = FrameNumbersPadded ? StringUtil.PadNumber(frameNumberOutput, Range.FrameNumberPadding) : frameNumberOutput.ToString();
                }
                else {
                    FrameNumbersProperty.FloatValue = frameNumberOutput;
                }
            }
            if (HasTimeRemainingProperty && TimeRemainingProperty != null) {
                if (TimeRemainingProperty.IsString) {
                    TimeRemainingProperty.StringValue = $"{(TimeRemainingAsTimecode ? StringUtil.SecondsToTimecode(TimeRemaining / FrameRate) : TimeRemaining.ToString())}";
                }
                else {
                    TimeRemainingProperty.FloatValue = TimeRemaining;
                }
            }
        }

        private bool CanContinue()
        {
            if (frameNumber <= RenderEndFrame) return true;
            if (IsStillFrame) {
                RenderFinished();
                return false;
            }
            RangeFinished();

            if (RenderQueue.AbortAfterCurrentRender) {
                return false;
            }

            if (!IsMultipleRanges) {
                return false;
            }

            int nextRange = currentRange + 1;
            if (nextRange > Ranges.Count - 1) {
                RenderFinished();
                return false;
            }
            while (!Ranges[nextRange].EnableRender) {
                nextRange++;
                if (nextRange > Ranges.Count - 1) {
                    RenderFinished();
                    return false;
                }
            }
            currentRange = nextRange;

            UpdateRange();

            if (Timeflow != null) Timeflow.CurrentTimeExact = (float)(frameNumber) * (1f / FrameRate);
            return true;
        }

        #endregion

        #region RENDERING

        private IEnumerator RenderFrame()
        {
            if (!IsRendering || !Application.isPlaying) yield break;
            //if (DebugEnabled) Debug.Log("RenderFrame: Camera:" + (MainCamera == null ? "NULL" : MainCamera.name));
            if (MainCamera == null) {
                Debug.LogError("Main camera is NULL. Aborting render.");
                Abort();
                yield break;
            }
            yield return new WaitForEndOfFrame();

            //if (DebugEnabled) Debug.Log("RenderToDisk.RenderFrame:" + frameNumber + " StartFrame:" + RenderStartFrame + " EndFrame:" + RenderEndFrame);
            if (Range.PreRoll && frameNumber < RenderStartFrame) {
                frameNumber++;
                //if (DebugEnabled) Debug.Log("PreRolling:" + frameNumber);
            }
            else
            if (frameNumber >= RenderStartFrame) {
                if (PreviewOnly || NthFrame != Framestep) {
                    Debug.Log("Simulate output frame: " + frameNumber);//--KEEP
                }
                else {
                    AllocateBuffer();

                    if (!Range.RenumberFrames) {
                        frameNumberOutput = frameNumber;
                    }
                    //if (DebugEnabled) Debug.Log("Rendering Frame:" + frameNumber + " frameNumberOutput:" + frameNumberOutput);

                    if (IsVR180 || IsFulldome) {
                        yield return RenderFrameFisheye();
                    }
                    else
                    if (IsVR360) {
                        yield return RenderFrame360();
                    }
                    else
                    if (UseScreenCapture) {
                        yield return RenderFrameScreenCapture();
                    }
                    else {
                        yield return RenderStereoCameras(MainCamera, RightCamera);
                    }

                    SaveFile(true);

                    frameNumberOutput++;
                }

                int framesRendered = frameNumber - RenderStartFrame;
                if (framesRendered > 1) {
                    int framesRemaining = RenderEndFrame - frameNumber;

                    TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                    float timeElapsed = (float)elapsed.TotalSeconds;

                    float timePerFrame = timeElapsed / (float)framesRendered;
                    TimeRemaining = framesRemaining;

                    if (LogEvery > 0) {
                        if (frameLogCount < LogEvery) {
                            frameLogCount++;
                        }
                        else {
                            frameLogCount = 0;
                            string remainingTime = StringUtil.SecondsToTimecode(framesRemaining * timePerFrame, true);
                            Debug.Log($"Rendered Frames:{framesRendered} Remaining Time:{remainingTime}");//--KEEP
                        }
                    }
                }

                frameNumber++;
                subframe = 0;
                isSubframe = Subframes > 0;
                //if (DebugEnabled) Debug.Log("RenderToDisk.RenderFrame: NextFrame:" + frameNumber);
            }
            else {
                subframe = 0;
                frameNumber = RenderStartFrame;
                isSubframe = false;
                if (Timeflow != null) Timeflow.CurrentTimeExact = (float)(frameNumber) * (1f / FrameRate);
                //if (DebugEnabled) Debug.Log("Queue to start:" + frameNumber + " frame: " + frameNumber);
            }

            UpdateOverlay();

            if (!hasStarted) {
                hasStarted = true;
                StartRendering();
            }
            //if (DebugEnabled) Debug.Log("RenderToDisk.RenderFrame: Complete");
        }

        private bool RenderCubemap(Camera camera, RenderTexture cubeMap, Camera.MonoOrStereoscopicEye eye)
        {
            bool success = camera.RenderToCubemap(cubeMap, CubemapFaceMask, eye);
            if (!success) {
                Debug.LogError($"Failed to render cubemap for camera '{camera.name}'. Aborting render.");
                Abort();
            }
            return success;
        }

        private void SetupFisheyeMaterial(Transform cameraTransform)
        {
            FisheyeMaterial.SetInt("_IsFulldome", IsFisheye ? 0 : 1);
            FisheyeMaterial.SetFloat("_Horizon", DomeHorizon);
            FisheyeMaterial.SetFloat("_DomeTilt", DomeTilt);
            FisheyeMaterial.SetInt("_Masked", (DomeMasked ? 1 : 0));
            FisheyeMaterial.SetFloat("_MaskRoundness", MaskRoundness);
            FisheyeMaterial.SetFloat("_MaskSoftness", MaskSoftness);
            FisheyeMaterial.SetVector("_Rotation", cameraTransform.rotation.eulerAngles);
        }

        private IEnumerator RenderFrameFisheye()
        {
            if (!IsStereo) {
                yield return RenderFisheyeCameras(true);
            }
            else {
                UseActiveCamera = false;
                MainCamera.stereoSeparation = StereoSeparation;

                // Shift the left eye to keep the right eye (dominant) centered
                MainCamera.transform.localPosition = new Vector3(-StereoSeparation, 0, 0);
                RightCamera.transform.localPosition = new Vector3(0, 0, 0);

                yield return RenderFisheyeCameras(!UsePostStereoCameras);

                if (UsePostStereoCameras) {
                    yield return RenderStereoCameras(LeftCameraPost, RightCameraPost);
                }
            }
            yield break;
        }

        private IEnumerator RenderFisheyeCameras(bool getFinalImage)
        {
            if (!RenderCubemap(MainCamera, CubemapLeft, IsStereo ? Camera.MonoOrStereoscopicEye.Left : Camera.MonoOrStereoscopicEye.Mono)) {
                yield break;
            }
            SetupFisheyeMaterial(MainCamera.transform);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture image = IsStereo ? ImageLeft : OutputTexture;
            RenderTexture.active = image != null ? image : MainCamera.targetTexture;

            Graphics.Blit(CubemapLeft, RenderTexture.active, FisheyeMaterial);

            if (getFinalImage) ImageToBuffer(true);

            RenderTexture.active = currentRT;

            if (getFinalImage) ImageToBuffer(true);

            if (IsStereo) {
                if (!RenderCubemap(RightCamera, CubemapRight, Camera.MonoOrStereoscopicEye.Right)) {
                    yield break;
                }
                SetupFisheyeMaterial(RightCamera.transform);

                currentRT = RenderTexture.active;
                RenderTexture.active = ImageRight != null ? ImageRight : RightCamera.targetTexture;

                Graphics.Blit(CubemapRight, RenderTexture.active, FisheyeMaterial);

                if (getFinalImage) ImageToBuffer(false);

                RenderTexture.active = currentRT;
            }

            if (getFinalImage && CaptureOverlayCamera) {
                RenderOverlayCamera();
            }
            yield break;
        }

        private IEnumerator RenderFrameScreenCapture()
        {
            if (IsStereo) {
                //if (DebugEnabled) Debug.Log("CaptureScreenshotAsTexture");
                imageBuffer = ScreenCapture.CaptureScreenshotAsTexture(ScreenCapture.StereoScreenCaptureMode.LeftEye);
                if (IsStereoSaveSeparately) {
                    SaveFile(false);
                    yield return new WaitForEndOfFrame();

                    //if (DebugEnabled) Debug.Log("CaptureScreenshotAsTexture");
                    imageBuffer = ScreenCapture.CaptureScreenshotAsTexture(ScreenCapture.StereoScreenCaptureMode.RightEye);
                }
            }
            else {
                //if (DebugEnabled) Debug.Log("CaptureScreenshotAsTexture");
                imageBuffer = ScreenCapture.CaptureScreenshotAsTexture();
            }
            deallocImageBuffer = true;
            yield break;
        }

        private IEnumerator RenderFrame360()
        {
            if (!IsStereo) {
                RenderCubemap(MainCamera, CubemapLeft, Camera.MonoOrStereoscopicEye.Mono);
                CubemapLeft.ConvertToEquirect(OutputTexture, Camera.MonoOrStereoscopicEye.Mono);
            }
            else {
                MainCamera.stereoSeparation = StereoSeparation;
                // Shift the left eye to keep the right eye (dominant) centered
                MainCamera.transform.localPosition = new Vector3(-StereoSeparation, 0, 0);

                RenderCubemap(MainCamera, CubemapLeft, Camera.MonoOrStereoscopicEye.Left);
                RenderCubemap(RightCamera, CubemapRight, Camera.MonoOrStereoscopicEye.Right);
                CubemapLeft.ConvertToEquirect(OutputTexture, Camera.MonoOrStereoscopicEye.Left);
                CubemapRight.ConvertToEquirect(OutputTexture, Camera.MonoOrStereoscopicEye.Right);
            }

            if (CaptureOverlayCamera) {
                RenderOverlayCamera();
            }
            else {
                Debug.LogError("This rendering mode is obsolete and should not be used. Instead, please use the " +
                    "overlay camera method to avoid gamma issues which slow down rendering. See the example scene " +
                    "RenderingVR360StereoPostProcessing. Rendering aborted.");

                Abort();
            }
            yield break;
        }

        private IEnumerator RenderStandardCamera(Camera camera, bool isMain)
        {
            RenderTexture currentRT = RenderTexture.active;
            //RenderTexture rt = OutputTexture;
            //bool dispose = false;
            //if (rt == null) {
            //    dispose = true;
            //    rt = RenderTexture.GetTemporary((int)CaptureSize.x, (int)CaptureSize.y, 0, RenderTexFormat, RenderTextureReadWrite.Default, AntiAliasing);
            //}

            RenderTexture.active = camera.activeTexture;
            //RenderTexture.active = rt;
            //camera.targetTexture = rt;
            //Debug.Log($"RenderStandardCamera:{camera.name}");
            camera.Render();

            yield return new WaitForEndOfFrame();
            ImageToBuffer(isMain);
            //camera.targetTexture = null;

            RenderTexture.active = currentRT;
            //if (dispose) RenderTexture.ReleaseTemporary(rt);
            yield break;
        }

        private void RenderOverlayCamera()
        {
            RenderTexture.active = OverlayCamera.targetTexture;
            OverlayCamera.Render();
            imageBuffer.ReadPixels(new Rect(0, 0, (int)FinalSize.x, (int)FinalSize.y), 0, 0, false);
        }

        private IEnumerator RenderStereoCameras(Camera leftCamera, Camera rightCamera)
        {
            RenderTexture currentRT = RenderTexture.active;
            yield return RenderStandardCamera(leftCamera, true);

            if (IsStereo && rightCamera != null) {
                if (IsStereoSaveSeparately) {
                    SaveFile(false);
                }
                yield return RenderStandardCamera(rightCamera, false);

                if (IsStereoSaveSeparately) {
                    SaveFile(true);
                }
            }

            if (CaptureOverlayCamera) {
                RenderTexture.active = OverlayCamera.targetTexture;
                OverlayCamera.Render();
                imageBuffer.ReadPixels(new Rect(0, 0, (int)FinalSize.x, (int)FinalSize.y), 0, 0, false);
            }
            RenderTexture.active = currentRT;
        }

        private void ImageToBuffer(bool isMainLeft)
        {
            if (!CaptureOverlayCamera && OverlayCamera != null) {
                OverlayCamera.Render();
            }

            // Coordinate system is 0,0 bottom left
            if (!IsStereo || isMainLeft) {
                imageBuffer.ReadPixels(new Rect(0, 0, (int)CaptureSize.x, (int)CaptureSize.y), 0, 0, false);
            }
            else
            if (IsStereoLeftRight) {
                imageBuffer.ReadPixels(new Rect(0, 0, (int)CaptureSize.x, (int)CaptureSize.y), (int)CaptureSize.x, 0, false);
            }
            else
            if (IsStereoTopBottom) {
                imageBuffer.ReadPixels(new Rect(0, 0, (int)CaptureSize.x, (int)CaptureSize.y), 0, (int)CaptureSize.y, false);
            }
        }

        #endregion

        #region SAVE FILE

        public static bool IsEncodingImage = false;

        private byte[] EncodeBufferToImageFile()
        {
            byte[] bytes = null;
            if (FileFormat == FileFormats.PNG) {
                bytes = imageBuffer.EncodeToPNG();
            }
            else
            if (FileFormat == FileFormats.EXR) {
                bytes = ImageConversion.EncodeToEXR(imageBuffer, EXRFlags);
            }
            else
            if (FileFormat == FileFormats.TGA) {
                bytes = ImageConversion.EncodeToTGA(imageBuffer);
            }
            else {
                bytes = ImageConversion.EncodeToJPG(imageBuffer, (int)JPEGQuality);
            }

            if (deallocImageBuffer) {
                deallocImageBuffer = false;
                DeallocateBuffer();
            }
            return bytes;
        }

        private void SaveFile(bool isRightEye = false)
        {
            byte[] bytes = EncodeBufferToImageFile();

            if (bytes == null || bytes.Length == 0) {
                Debug.LogError($"RenderToDisk.RenderFrame: Failed encoding texture to {FileFormat}. Aborting render.");
                Abort();
                return;
            }

            string filepath = GetOutputFilepath(isRightEye);

            //if (DebugEnabled) Debug.Log("SaveFile:" + filepath);
            FileStream fs = File.Create(filepath);
            if (fs != null) {
                fs.Write(bytes, 0, bytes.Length);
            }
            else {
                Debug.LogError($"RenderToDisk.RenderFrame: Failed saving file: {filepath}. Aborting render.");
                Abort();
            }
            fs.Close();
        }

        #endregion

        #region TOOLS

        public void GenerateMarkerRanges()
        {
            if (Timeflow == null) {
                Debug.LogError("An active instance of Timeflow must be present to set the time ranges.");
                return;
            }
            if (Timeflow.MarkerList == null || Timeflow.MarkerList.Count == 0) {
                EditorUtility.DisplayDialog("No Markers to Add", "No markers have been added to the active Timeflow instance. Please create markers first before using this feature.", "Ok");
                return;
            }
            int r = EditorUtility.DisplayDialogComplex("Generate Marker Ranges",
                "Do you want to replace all existing frame ranges, or append them to the existing list?",
                "Replace", "Cancel", "Append");
            if (r == 1) return;// Cancel

            UndoUtil.Undo(this, "Generate Marker Ranges", true);

            if (r == 0) {
                Ranges = new List<RenderToDiskRange>();
            }
            else {
                /// Appending ranges, only recreate list if null
                if (Ranges == null) Ranges = new List<RenderToDiskRange>();
            }

            /// Make sure the markers are listed in ascending time order
            Timeflow.MarkerList.Sort(new SortTimeflowMarkers());


            if (Timeflow.MarkerList[0].Time > Timeflow.StartTime) {
                /// The first marker is after the beginning so we need first inserts the start range
                RenderToDiskRange range = new RenderToDiskRange();
                range.Name = "Start";
                range.StartTime.TimeType = TimeValue.TimeTypes.Marker;
                range.EndTime.TimeType = TimeValue.TimeTypes.Marker;
                range.StartTime.Marker = 0;
                range.EndTime.Marker = Timeflow.MarkerList[0].ID;
                //range.StartFrame = 0; // always starts at frame 0
                //range.EndFrame = Timeflow.GetFrameFromSeconds(Timeflow.MarkerList[0].Time);
                Ranges.Add(range);
            }
            for (int i = 0; i < Timeflow.MarkerList.Count - 1; i++) {
                TimeflowMarker from = Timeflow.MarkerList[i];
                TimeflowMarker to = Timeflow.MarkerList[i + 1];
                RenderToDiskRange range = new RenderToDiskRange();
                range.Name = from.Name;

                range.StartTime.TimeType = TimeValue.TimeTypes.Marker;
                range.EndTime.TimeType = TimeValue.TimeTypes.Marker;
                range.StartTime.Marker = from.ID;
                range.EndTime.Marker = to.ID;

                //range.StartFrame = Timeflow.GetFrameFromSeconds(from.Time);
                //range.EndFrame = Timeflow.GetFrameFromSeconds(to.Time);
                Ranges.Add(range);
            }

            TimeflowMarker end = Timeflow.MarkerList[Timeflow.MarkerList.Count - 1];
            if (end.Time < Timeflow.EndTime) {
                /// Insert the final end range if the last marker is before the Timeflow end
                RenderToDiskRange range = new RenderToDiskRange();
                range.Name = end.Name;
                range.StartTime.TimeType = TimeValue.TimeTypes.Marker;
                range.EndTime.TimeType = TimeValue.TimeTypes.Marker;
                range.StartTime.Marker = end.ID;
                range.EndTime.Marker = -1;

                //range.StartFrame = Timeflow.GetFrameFromSeconds(end.Time);
                //range.EndFrame = Timeflow.GetFrameFromSeconds(Timeflow.EndTime);
                Ranges.Add(range);
            }
        }

        #endregion
#endif
    }


}//AxonGenesis

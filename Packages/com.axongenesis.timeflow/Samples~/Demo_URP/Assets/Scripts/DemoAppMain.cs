// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.


using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

#if URP_10_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is the main controller for the Timeflow Demo App. It handles async scene loading and unloading
    /// and general UI functions. This is only intended to be an example showing how Timeflow can be
    /// integrated into an interactive app.
    /// </summary>
    public class DemoAppMain : AxonGenesisBehavior
    {
        public static DemoAppMain Instance { get; private set; }

        public static void LoadAppMain()
        {
            if (Instance != null || !Application.isPlaying) {
                return;
            }

            if (SceneManager.sceneCount > 0) {
                string sceneName = SceneUtility.GetScenePathByBuildIndex(0);
                if (sceneName.Contains("DemoAppMain")) {
                    Debug.Log("Loading AppMain...");//--KEEP
                    SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
                }
                else {
                    Debug.Log($"Scene[0]:{sceneName}");//--KEEP
                    Debug.LogWarning("The demo app scenes have not been setup correctly in the Build Settings. " +
                        "Please refer to the Timeflow Demo App documentation for setup instructions.");
                }
            }
        }

        public Timeflow IntroTimeflow;
        public Canvas MainCanvas;

        public GameObject Navigation;

        public GameObject PlayControls;
        public Button PlayButton;
        public Button PauseButton;
        public Button BackButton;
        public Button NextButton;
        public Button AudioOnButton;
        public Button AudioOffButton;
        public Slider TimeSlider;

        public enum QualityModes
        {
            Low,
            Medium,
            High
        }
        public QualityModes QualityMode = QualityModes.Low;

        [NonSerialized]
        public bool IsAudioOn = true;

        public float AudioFadeTime = 1f;

        public Light DirectionalLight;

        [NonSerialized]
        private bool isStartup = true;

        [NonSerialized]
        private bool isLoading;

        #region ACCESSORS

        public Timeflow Timeflow {
            get {
                return Timeflow.Active;
            }
        }

        public bool IsPlaying {
            get {
                return Timeflow != null && Timeflow.IsPlaying;
            }
            set {
                if (!value && Timeflow.IsPlaying) {
                    Timeflow.Stop();
                }
                else
                if (value && !Timeflow.IsPlaying) {
                    Timeflow.Resume();
                }
            }
        }

        public int Scene { get; private set; }

        public bool IsFinished { get; private set; }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log("DemoAppMain.OnAwake");
            base.OnAwake();
            Instance = this;

            if (DemoAppScene.Instance == null) {
                isStartup = true;
            }
            else
            if (IntroTimeflow != null) {
                DestroyImmediate(IntroTimeflow.gameObject);
            }

            if (DirectionalLight != null) {
                // This is a hack for WebGL which otherwise strips these property definitions resulting in
                // an ArgumentException error from System.Reflection, since it cannot locate property.
                DirectionalLight.intensity = 1f;
                DirectionalLight.color = Color.white;
            }

            //if (DebugEnabled) Debug.Log("DontDestroyOnLoad:" + gameObject.name);
            DontDestroyOnLoad(gameObject);
            UpdateNavigation();
        }

        protected override void OnStart()
        {
            base.OnStart();
            SetupAudio();

            SetQuality(PlayerPrefs.GetInt("TimeflowDemoQuality", (int)QualityMode));
        }

        public void SetupAudio()
        {
            if (IsAudioOn) {
                // Start audio volume at 0 and fade on
                bool isAudioStarting = AudioFadeTime > 0f && !isStartup;
                AudioListener.volume = isAudioStarting ? 0f : 1f;
            }
            else {
                AudioListener.volume = 0f;
            }
            if (AudioOnButton != null) AudioOnButton.gameObject.SetActive(IsAudioOn);
            if (AudioOffButton != null) AudioOffButton.gameObject.SetActive(!IsAudioOn);
        }

        public void AudioOn()
        {
            AudioFadeTime = 0f;
            isStartup = false;
            IsAudioOn = true;
            SetupAudio();
        }

        public void AudioOff()
        {
            AudioFadeTime = 0f;
            isStartup = false;
            IsAudioOn = false;
            SetupAudio();
        }

        #endregion

        #region QUALITY

        /// <summary>
        /// Cycle through the quality levels. This assumes the quality settings have been configured in
        /// Project Settings > Quality with the 3 levels, Low, Medium, and High. The Timeflow examples
        /// include render pipeline assets for URP, however you may use your own settings if you prefer.
        /// </summary>
        public void ToggleQuality()
        {
            switch (QualityMode) {
                case QualityModes.Low:
                    SetQuality(QualityModes.Medium);
                    break;
                case QualityModes.Medium:
                    SetQuality(QualityModes.High);
                    break;
                default:
                    SetQuality(QualityModes.Low);
                    break;
            }
        }

        public void SetQuality(QualityModes mode)
        {
            if (QualityMode == mode) return;

            QualityMode = mode;
            int modeIndex = (int)mode;

            if (mode == QualityModes.Low) {
                Application.targetFrameRate = 30;
            }
            else
            if (mode == QualityModes.Medium) {
                Application.targetFrameRate = 60;
            }
            else
            if (mode == QualityModes.High) {
                Application.targetFrameRate = 90;
            }
            //Debug.Log("QualityMode:" + mode + " targetFrameRate:" + Application.targetFrameRate);

            QualitySettings.SetQualityLevel(modeIndex, true);

            /// Stores the setting to be used next time the app is launched
            PlayerPrefs.SetInt("TimeflowDemoQuality", modeIndex);

            UpdateQualitySettings();
        }

        /// <summary>
        /// Applies the selected quality settings as an int value.
        /// </summary>
        /// <param name="modeIndex">A value from 0 to 2 mapping to Low, Medium, and High</param>
        public void SetQuality(int modeIndex)
        {
            QualityModes mode = modeIndex switch {
                1 => QualityModes.Medium,
                2 => QualityModes.High,
                _ => QualityModes.Low
            };
            SetQuality(mode);
        }

        /// <summary>
        /// Quality settings are applied to the camera and post processing volumes in addition to the
        /// global render quality setting being set.
        /// </summary>
        public void UpdateQualitySettings()
        {

#if URP_10_OR_NEWER
            /// Please note that this demo app is only setup to work with URP presently
            Volume postProcessing = ObjectUtil.FindComponent<Volume>();
            UniversalAdditionalCameraData cameraData = null;

            Bloom bloom = null;
            MotionBlur motionBlur = null;
            Vignette vignette = null;
            ChromaticAberration chromatic = null;

            if (postProcessing != null) {
                postProcessing.gameObject.SetActive(true);
                postProcessing.profile.TryGet(out bloom);
                postProcessing.profile.TryGet(out motionBlur);
                postProcessing.profile.TryGet(out vignette);
                postProcessing.profile.TryGet(out chromatic);
            }

            if (Camera.main != null) {
                Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out cameraData);
            }
            switch (QualityMode) {
                case QualityModes.Low: {
                        if (cameraData != null) {
                            cameraData.antialiasing = AntialiasingMode.None;
                            cameraData.renderPostProcessing = false;
                        }

                        break;
                    }
                case QualityModes.Medium: {
                        if (cameraData != null) {
                            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                            cameraData.renderPostProcessing = true;

                            if (bloom != null) bloom.active = true;
                            if (motionBlur != null) motionBlur.active = false;
                            if (vignette != null) vignette.active = false;
                            if (chromatic != null) chromatic.active = false;
                        }

                        break;
                    }
                case QualityModes.High: {
                        if (cameraData != null) {
                            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                            cameraData.renderPostProcessing = true;

                            if (bloom != null) bloom.active = true;
                            if (motionBlur != null) motionBlur.active = true;
                            if (vignette != null) vignette.active = true;
                            if (chromatic != null) chromatic.active = true;
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }

        #endregion

        #region LOADING

        public static void VerifyBuildScenes()
        {
#if UNITY_EDITOR
            bool hasIssues = false;
            int i = 0;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes) {
                if (!buildScene.enabled) hasIssues = true; // all scenes must be enabled
                if (i == 0) {
                    if (!buildScene.path.EndsWith("DemoAppMain.unity")) {
                        hasIssues = true;
                    }
                }
                else {
                    if (!buildScene.path.Contains("App" + StringUtil.PadNumber2(i))) {
                        hasIssues = true;
                    }
                }
                if (hasIssues) break;
                i++;
                if (i > 16) break;
            }

            if (!hasIssues) return;

            string message = "The Timeflow Demo App requires that the demo scenes be added in specific order to the Build Settings. " +
                             "The current Scenes in Build are either out of order or missing scene references. Please check the Readme in DemoAppMain " +
                             "for further setup instructions.";
            Debug.LogError(message);
            EditorApplication.ExitPlaymode();

            int r = EditorUtility.DisplayDialogComplex("Build Scene Configuration", message, "Ok", "Cancel", "Goto Documentation");
            if (r == 2) {
                Application.OpenURL("https://axongenesis.gitbook.io/timeflow/reference/examples/demo-app");
            }

#endif
        }

        public void LoadHome()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) {
                VerifyBuildScenes();
            }
#endif
            LoadScene(1);
        }

        public void LoadPrevious()
        {
            if (Scene <= 1) return;

            //if (DebugEnabled) Debug.Log("Main.LoadPrevious:" + (Scene - 1));
            StartCoroutine(_Load(Scene - 1));
        }

        public void LoadNext()
        {
            int next = Scene + 1;
            if (next > 16) next = 1;
            //if (DebugEnabled) Debug.Log("Main.LoadNext:" + next);

            StartCoroutine(_Load(next));
        }

        public void LoadScene(int sceneIndex)
        {
            //if (DebugEnabled) Debug.Log("Main.LoadScene:" + sceneIndex + " count:" + SceneManager.sceneCountInBuildSettings);
            StartCoroutine(_Load(sceneIndex));
        }

        public IEnumerator _Load(int sceneIndex)
        {
            if (isLoading) {
                // Wait until current load operation has finished before loading another one
                Debug.LogWarning("Waiting for current scene load to finish");
                yield break;
            }
            isLoading = true;
            if (isStartup) {
                IsFinished = false;
                isStartup = false;
                Scene = -1;
            }

            Navigation.SetActive(false);

            if (sceneIndex >= SceneManager.sceneCountInBuildSettings - 1) {
                sceneIndex = SceneManager.sceneCountInBuildSettings - 1;
                //if (DebugEnabled) Debug.Log("Main.Load: END: " + sceneIndex);
                IsFinished = true;
            }
            else {
                IsFinished = false;
                if (sceneIndex <= 0) {
                    sceneIndex = 0;
                    //if (DebugEnabled) Debug.Log("Main.Load: START: " + sceneIndex);
                }
            }

            if (Scene != sceneIndex) {
                // Scene 0 is the main scene an always remains loaded
                if (Scene > 0) {
                    //if (DebugEnabled) Debug.Log("Main.Load: Unloading:" + Scene);
                }
                Scene = sceneIndex;

                if (IsAudioOn && AudioFadeTime > 0f) {
                    /// Loop until audio volume has faded out before loading the scene
                    while (AudioListener.volume > 0f && Time.deltaTime > 0) {
                        AudioListener.volume -= Time.deltaTime;
                        if (AudioListener.volume <= 0f) {
                            AudioListener.volume = 0f;
                            break;
                        }
                        yield return null;
                    }
                }

                if (Scene > 0) {
                    //if (DebugEnabled) Debug.Log("Main.Load: Loading:" + Scene);
                    yield return SceneManager.LoadSceneAsync(Scene, LoadSceneMode.Single);
                }

                if (IntroTimeflow != null) {
                    DestroyImmediate(IntroTimeflow.gameObject);
                }

                /// Wait until the scene controller has fully loaded and started
                while (!DemoAppScene.HasStarted) {
                    yield return null;
                }

                /// Make sure quality settings are applied in the newly loaded scene
                UpdateQualitySettings();

                if (Timeflow != null) {
                    if (TimeSlider != null) {
                        TimeSlider.SetValueWithoutNotify(0);
                    }
                }
                UpdateNavigation();

                /// Audio listener changes from scene to scene
                SetupAudio();

                if (IsAudioOn && AudioFadeTime > 0f) {
                    /// Loop until audio volume has faded out before loading the scene
                    while (AudioListener.volume < 1f && Time.deltaTime > 0) {
                        AudioListener.volume += Time.deltaTime;
                        if (AudioListener.volume >= 1f) {
                            AudioListener.volume = 1f;
                            break;
                        }
                        yield return null;
                    }
                }
            }
            isLoading = false;
        }

        #endregion

        #region UI METHODS

        public void UpdateNavigation()
        {
            if (Scene > 1) {
                /// Only show navigation for scenes after the home screen
                Navigation.SetActive(true);
                PlayControls.SetActive(true);
                UpdatePlayControls();

                //if (Timeflow != null && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 1) {
                NextButton.gameObject.SetActive(true);
                //}
                //else {
                //    NextButton.gameObject.SetActive(false);
                //}
            }
            else {
                Navigation.SetActive(false);
                PlayControls.SetActive(Scene == 0);
                UpdatePlayControls();
            }
        }

        public void UpdatePlayControls()
        {
            PauseButton.gameObject.SetActive(!IsPlaying);
            PlayButton.gameObject.SetActive(IsPlaying);
            if (IsPlaying) {
                PlayButton.Select();
            }
            else {
                PauseButton.Select();
            }
        }

        public void ToggleNavigation()
        {
            Navigation.SetActive(!Navigation.activeSelf);
            //if (DebugEnabled) Debug.Log("DemoAppMain.ToggleNavigation:" + Navigation.activeSelf);
            UpdateNavigation();
        }

        public void TogglePlay()
        {
            if (DemoAppScene.Instance != null) {
                DemoAppScene.Instance.TogglePlay();
            }
            else
            if (Timeflow != null) {
                Timeflow.TogglePlay();
            }
            UpdateNavigation();
        }

        public void ReplayIntro()
        {
            Timeflow.Rewind();
            if (!Timeflow.IsPlaying) Timeflow.Play();
        }

        public void RewindScene()
        {
            if (DemoAppScene.Instance != null) {
                DemoAppScene.Instance.Rewind();
            }
            else
            if (Timeflow != null) {
                Timeflow.Rewind();
            }
        }

        public void AdvanceScene()
        {
            if (DemoAppScene.Instance != null) {
                DemoAppScene.Instance.Advance();
            }
        }

        /// <summary>
        /// Called when the user manually drags the slider handle which pauses playback. The player must
        /// press the play button to resume playing.
        /// </summary>
        /// <param name="val">This value is required by the callback but does not represent the actual
        ///     value of the slider and is ignored</param>
        public void OnSliderValueChanged(float val)
        {
            if (Timeflow.IsPlaying) Timeflow.Stop();
            if (DemoAppScene.Instance != null) {
                DemoAppScene.CurrentTime = TimeSlider.normalizedValue * DemoAppScene.Duration;
            }
            else
            if (Timeflow != null) {
                Timeflow.SetTime(TimeSlider.normalizedValue * Timeflow.Duration);
            }

            UpdateNavigation();
        }

        public void Quit()
        {
            //if (DebugEnabled) Debug.Log("Main.Quit");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        private void Update()
        {
            if (TimeSlider == null) return;

            if (DemoAppScene.Instance != null && DemoAppScene.Duration > 0) {
                TimeSlider.SetValueWithoutNotify(DemoAppScene.CurrentTime / DemoAppScene.Duration);
            }
            else
            if (Timeflow != null && Timeflow.Duration > 0) {
                TimeSlider.SetValueWithoutNotify(Timeflow.CurrentTime / Timeflow.Duration);
            }
        }
    }

}//AxonGenesis

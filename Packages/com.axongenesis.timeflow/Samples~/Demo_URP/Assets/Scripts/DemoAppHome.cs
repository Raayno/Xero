// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections;
using UnityEngine;

#if TMPRO_3_OR_NEWER
using TMPro;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is an example scene controller for the Timeflow demo.
    /// </summary>
    [ExecuteInEditMode]
    public class DemoAppHome : DemoAppScene
    {
        public static DemoAppHome Home;
        public static bool FirstRun = true;

        public Blend CameraBlend;
        public float TransitionTime = 2f;
        public float LoadDelay = 1f;

        public AudioTrack MenuAudio;
        public AudioTrack TestMusicAudio;
        public GameObject TestAudioButton;
        public GameObject ScreenAudioReactive;

#if TMPRO_3_OR_NEWER
        public TextMeshProUGUI QualityText;
#endif

        [NonSerialized]
        public bool IsZoomed = true;

        [NonSerialized]
        public int _ZoomIndex = -1;

        [NonSerialized]
        private bool isLocalStart;

        public int ZoomIndex {
            get {
                return _ZoomIndex;
            }
            set {
                if (_ZoomIndex != value) {
                    _ZoomIndex = value;
                    //if (DebugEnabled) Debug.Log("ZoomIndex:" + value);
                }
            }
        }

        protected override void OnAwake()
        {
            Home = this;

            /// Note whether the scene is starting up from DemoAppMain or played directly
            isLocalStart = DemoAppMain.Instance == null;

            base.OnAwake();
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            UpdateQuality();

            if (isLocalStart) {
                /// Skip intro when playing scene directly in editor
                ZoomHome();
            }
            else {
                ZoomIntro();
            }
        }

        #region BUTTON ACTIONS

        public void GotoAssetStore()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/247895");
        }

        public void GotoWebsite()
        {
            Application.OpenURL("https://axongenesis.gitbook.io/timeflow");
        }

        public void ToggleTestAudio()
        {
            if (TestMusicAudio == null || MenuAudio == null) return;

            float opacity = 1f;
            Color buttonColor = Color.white;
            if (TestMusicAudio.Mute) {
                TestMusicAudio.Mute = false;
                MenuAudio.Mute = true;

                /// Make sure audio is enabled otherwise audio reactive features won't work
                DemoAppMain.Instance.AudioOn();
            }
            else {
                TestMusicAudio.Mute = true;
                MenuAudio.Mute = false;
                opacity = 0.25f;
            }

            if (ScreenAudioReactive == null) return;

            Material mat = ObjectUtil.GetMaterial(ScreenAudioReactive);
            if (mat != null) {
                mat.SetFloat("_Opacity", opacity);
            }
        }

        public void ToggleQuality()
        {
            if (DemoAppMain.Instance == null) return;
            DemoAppMain.Instance.ToggleQuality();
            UpdateQuality();
        }

        public void UpdateQuality()
        {
#if TMPRO_3_OR_NEWER
            if (QualityText == null || DemoAppMain.Instance == null) return;

            switch (DemoAppMain.Instance.QualityMode)
            {
                case DemoAppMain.QualityModes.Low:
                    QualityText.text = "Low";
                    QualityText.color = Color.cyan;
                    break;
                case DemoAppMain.QualityModes.Medium:
                    QualityText.text = "Med";
                    QualityText.color = new Color(1f, 0.7f, 0f);
                    break;
                case DemoAppMain.QualityModes.High:
                    QualityText.text = "High";
                    QualityText.color = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }

        #endregion

        #region LOAD SCENES

        public void LoadCurrent()
        {
            //if (DebugEnabled) Debug.Log("DemoAppHome.LoadCurrent:" + ZoomIndex);
            if (ZoomIndex == 1) {
                Application.OpenURL("https://axongenesis.com/timeflow");
            }
            else
            if (ZoomIndex > 1 && ZoomIndex < 17) {
                DemoAppMain.Instance.LoadScene(ZoomIndex);
            }
        }

        public void LoadIndex(int index)
        {
            //if (DebugEnabled) Debug.Log("DemoAppHome.LoadIndex:" + index);
            if (LoadDelay <= 0f) {
                DemoAppMain.Instance.LoadScene(index);
            }
            else {
                StartCoroutine(_LoadIndex(index));
            }
        }

        private IEnumerator _LoadIndex(int index)
        {
            yield return new WaitForSeconds(LoadDelay);
            DemoAppMain.Instance.LoadScene(index);
        }

        public void LoadDemo()
        {
            LoadIndex(GetIndex("Demo"));
        }

        public void LoadKeyframer()
        {
            LoadIndex(GetIndex("Keyframer"));
        }

        public void LoadAnimation()
        {
            LoadIndex(GetIndex("Animation"));
        }

        public void LoadTween()
        {
            LoadIndex(GetIndex("Tween"));
        }

        public void LoadBlend()
        {
            LoadIndex(GetIndex("Blend"));
        }

        public void LoadMotionPath()
        {
            LoadIndex(GetIndex("MotionPath"));
        }

        public void LoadFlyby()
        {
            LoadIndex(GetIndex("Flyby"));
        }

        public void LoadAutoBank()
        {
            LoadIndex(GetIndex("AutoBank"));
        }

        public void LoadAutoRotate()
        {
            LoadIndex(GetIndex("Rotate"));
        }

        public void LoadAudioReactive()
        {
            LoadIndex(GetIndex("AudioReactive"));
        }

        public void LoadMidiReactive()
        {
            LoadIndex(GetIndex("MidiReactive"));
        }

        public void LoadPlaceOnSurface()
        {
            LoadIndex(GetIndex("PlaceOnSurface"));
        }

        public void LoadFollow()
        {
            LoadIndex(GetIndex("Follow"));
        }

        public void LoadNoise()
        {
            LoadIndex(GetIndex("Noise"));
        }

        public void LoadPlaceOnPath()
        {
            LoadIndex(GetIndex("PlaceOnPath"));
        }

        #endregion

        #region ZOOM SECTIONS

        private int GetIndex(string section)
        {
            int index = 0;
            switch (section)
            {
                case "About":
                    index = 1;
                    break;
                case "Demo":
                    index = 2;
                    break;
                case "Keyframer":
                    index = 3;
                    break;
                case "Animation":
                    index = 4;
                    break;
                case "Tween":
                    index = 5;
                    break;
                case "Blend":
                    index = 6;
                    break;
                case "MotionPath":
                    index = 7;
                    break;
                case "Flyby":
                    index = 8;
                    break;
                case "AutoBank":
                    index = 9;
                    break;
                case "AutoRotate":
                    index = 10;
                    break;
                case "AudioReactive":
                    index = 11;
                    break;
                case "MidiReactive":
                    index = 12;
                    break;
                case "PlaceOnSurface":
                    index = 13;
                    break;
                case "Follow":
                    index = 14;
                    break;
                case "Noise":
                    index = 15;
                    break;
                case "PlaceOnPath":
                    index = 16;
                    break;
                default:
                    Debug.LogWarning("Could not find index for section '" + section + "'");
                    break;
            }
            return index;
        }

        private string GetSection(int index)
        {
            string section = null;

            switch (index)
            {
                case 0:
                    section = "Home";
                    break;
                case 1:
                    section = "About";
                    break;
                case 2:
                    section = "Demo";
                    break;
                case 3:
                    section = "Keyframer";
                    break;
                case 4:
                    section = "Animation";
                    break;
                case 5:
                    section = "Tween";
                    break;
                case 6:
                    section = "Blend";
                    break;
                case 7:
                    section = "MotionPath";
                    break;
                case 8:
                    section = "Flyby";
                    break;
                case 9:
                    section = "AutoBank";
                    break;
                case 10:
                    section = "AutoRotate";
                    break;
                case 11:
                    section = "AudioReactive";
                    break;
                case 12:
                    section = "MidiReactive";
                    break;
                case 13:
                    section = "PlaceOnSurface";
                    break;
                case 14:
                    section = "Follow";
                    break;
                case 15:
                    section = "Noise";
                    break;
                case 16:
                    section = "PlaceOnPath";
                    break;
                default:
                    Debug.LogWarning("Index " + index + " out of range");
                    break;
            }
            return section;
        }

        private IEnumerator _HidePanels(float wait)
        {
            if (wait > 0f) yield return new WaitForSeconds(wait);

            Tween.TriggerAllOff(true); // Make sure all panels are closed
        }


        public void ZoomIntro()
        {
            //if (DebugEnabled) Debug.Log("DemoAppHome.ZoomIntro FirstRun:"+ FirstRun);
            if (FirstRun) {
                FirstRun = false;
                CameraBlend.TransitionTo("Startup", 0f);
                CameraBlend.TransitionTo("Intro", 1f);
            }
            else {
                CameraBlend.TransitionTo("Home", TransitionTime);
            }
            IsZoomed = true;
            ZoomIndex = 0;
        }

        public void ZoomHome()
        {
            //if (DebugEnabled) Debug.Log("DemoAppHome.ZoomHome");
            CameraBlend.TransitionTo("Home", TransitionTime);
            FirstRun = false;
            IsZoomed = false;
            ZoomIndex = 0;

            // Ensures panels and dim area are fully hidden incase of playback lag on slow devices
            StartCoroutine(_HidePanels(TransitionTime));
        }

        public void ZoomPrev()
        {
            int index = ZoomIndex - 1;
            if (index < 0) index = 0;
            ZoomTo(index);
        }

        public void ZoomNext()
        {
            int index = ZoomIndex + 1;
            if (index > 16) index = 0;
            ZoomTo(index);
        }

        public void ZoomTo(int index)
        {
            if (index <= 0 || index > 16) {
                ZoomHome();
            }
            else {
                ZoomTo(GetSection(index));
            }
        }

        public void ZoomTo(string section)
        {
            int index = GetIndex(section);
            if (IsZoomed && index == ZoomIndex) {
                //if (DebugEnabled) Debug.Log("DemoAppHome.ZoomHome:" + index + " :" + section);
                ZoomHome();
            }
            else {
                ZoomIndex = index;
                IsZoomed = ZoomIndex > 0;
                CameraBlend.TransitionTo(section, TransitionTime);
                //if (DebugEnabled) Debug.Log("DemoAppHome.ZoomTo:" + ZoomIndex + " :" + section);
            }
        }

        public void ZoomAbout()
        {
            ZoomTo("About");
        }

        public void ZoomWelcome()
        {
            ZoomTo("Welcome");
        }

        public void ZoomDemo()
        {
            ZoomTo("Demo");
        }

        public void ZoomKeyframer()
        {
            ZoomTo("Keyframer");
        }

        public void ZoomAnimation()
        {
            ZoomTo("Animation");
        }

        public void ZoomTween()
        {
            ZoomTo("Tween");
        }

        public void ZoomBlend()
        {
            ZoomTo("Blend");
        }

        public void ZoomMotionPath()
        {
            ZoomTo("MotionPath");
        }

        public void ZoomFlyby()
        {
            ZoomTo("Flyby");
        }

        public void ZoomAutoBank()
        {
            ZoomTo("AutoBank");
        }

        public void ZoomAutoRotate()
        {
            ZoomTo("AutoRotate");
        }

        public void ZoomAudioReactive()
        {
            ZoomTo("AudioReactive");
        }

        public void ZoomMidiReactive()
        {
            ZoomTo("MidiReactive");
        }

        public void ZoomPlaceOnSurface()
        {
            ZoomTo("PlaceOnSurface");
        }

        public void ZoomFollow()
        {
            ZoomTo("Follow");
        }

        public void ZoomNoise()
        {
            ZoomTo("Noise");
        }

        public void ZoomPlaceOnPath()
        {
            ZoomTo("PlaceOnPath");
        }

        #endregion
    }

}//AxonGenesis

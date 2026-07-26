// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is an example scene controller for the Timeflow Demo App.
    /// </summary>
    [ExecuteInEditMode]
    public class DemoAppScene : AxonGenesisBehavior
    {
        public static DemoAppScene Instance { get; private set; }
        public static bool HasStarted;
        public static float Duration = 1f;
        public static TimeflowMarker ActiveMarker;

        /// <summary>
        /// Interaces with the current instance of Timeflow to get and set the current time.
        /// </summary>
        public static float CurrentTime {
            get {
                if (Instance != null && Instance.Timeflow != null) {
                    if (Instance.Timeflow.MarkerList != null && Instance.Timeflow.MarkerList.Count > 1 && ActiveMarker != null) {
                        /// return the time relative to the current marker section
                        return Instance.Timeflow.CurrentTime - ActiveMarker.Time;
                    }
                    else {
                        /// return the world time of the current timeflow instance
                        return Instance.Timeflow.CurrentTime;
                    }
                }
                return 0f;
            }
            set {
                if (Instance != null && Instance.Timeflow != null) {
                    if (ActiveMarker != null) {
                        /// sets the time relative to the current marker section
                        Instance.Timeflow.SetTime(ActiveMarker.Time + value);
                    }
                    else {
                        /// sets the world time of the timeflow instance
                        Instance.Timeflow.SetTime(value);
                    }
                }
            }
        }

        public Timeflow Timeflow;
        public GameObject WebGLNotice;

        protected override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
            ActiveMarker = null;
            //if (DebugEnabled) Debug.Log(name + ".DemoAppScene.OnAwake");

            if (Timeflow == null) {
                TryGetComponent<Timeflow>(out Timeflow);
                if (Timeflow == null) Debug.LogError("Timeflow is NULL");
            }

            if (WebGLNotice != null) {
#if UNITY_WEBGL_API
                WebGLNotice.SetActive(true);
#else
                WebGLNotice.SetActive(false);
#endif
            }

            if (Application.isPlaying && DemoAppMain.Instance == null) {
                DemoAppMain.LoadAppMain();
            }
        }

        protected override void OnDestruct()
        {
            base.OnDestruct();
            HasStarted = false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            HasStarted = true;
            GetDuration();

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 1) {
                /// Set the work area to the first marker
                Timeflow.WorkAreaEnabled = true;
                Timeflow.Markers.GotoMarker(0);
            }
            else {
                Timeflow.WorkAreaEnabled = false;
            }

            /// Make sure to start at beginning
            Timeflow.Rewind();

            if (Application.isPlaying) {
                Timeflow.Play();
            }
        }

        public bool IsPlaying {
            get {
                if (Timeflow != null) {
                    return Timeflow.IsPlaying;
                }
                return false;
            }
            set {
                if (Timeflow != null) {
                    if (!value && Timeflow.IsPlaying) {
                        Timeflow.Stop();
                    }
                    else
                    if (value && !Timeflow.IsPlaying) {
                        Timeflow.Resume();
                    }
                }
            }
        }

        public void Play()
        {
            if (!IsPlaying) {
                IsPlaying = true;
            }
        }

        public void TogglePlay()
        {
            if (IsPlaying) {
                IsPlaying = false;
            }
            else {
                IsPlaying = true;
            }
        }

        /// <summary>
        /// Calculates the duration of the current marker section if any is active, otherwise the full
        /// length of the timeline.
        /// </summary>
        public float GetDuration()
        {
            Duration = 0f;
            if (Timeflow != null) {
                if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                    float t = Timeflow.CurrentTime + 0.1f; // nudge time forward
                    TimeflowMarker currentMarker = Timeflow.Markers.GetPrevMarker(t, true);
                    TimeflowMarker nextMarker = Timeflow.Markers.GetNextMarker(Timeflow.CurrentTime);

                    if (currentMarker != null && nextMarker != null) {
                        Timeflow.WorkAreaDisableOnStart = false;
                        Timeflow.WorkAreaEnabled = true;
                        Duration = nextMarker.Time - currentMarker.Time;
                    }
                    else
                    if (currentMarker != null) {
                        Duration = Timeflow.EndTime - currentMarker.Time;
                    }
                }
                else {
                    /// Don't use the work area if no markers are set
                    Timeflow.WorkAreaEnabled = false;
                }
                if (Duration <= 0) {
                    Duration = Timeflow.Duration;
                }

                /// Automatically enable looping for all scenes
                Timeflow.LoopEnabled = true;
            }
            return Duration;
        }

        /// <summary>
        /// Rewinds to the beginning of the current marker section or the beginning of the scene. For
        /// scenes that use marker regions to demonstrate different features, this steps back through each
        /// section.
        /// </summary>
        public void Rewind()
        {
            if (Timeflow != null) {
                bool handled = false;
                /// If the scene has markers, use the back arrow to rewind the current or go back to the
                /// previous marker if the current section is already rewound
                if (Timeflow.MarkerList != null) {
                    TimeflowMarker marker = Timeflow.Markers.GetPrevMarker(Timeflow.CurrentTime, true);
                    if (marker != null) {
                        float dif = Timeflow.CurrentTime - marker.Time;
                        if (dif < 2f) {
                            /// The current section just started playing, so assume the player wants to go
                            /// backfurther. Check for an earlier marker
                            TimeflowMarker previousMarker = Timeflow.Markers.GetPrevMarker(marker.Time, false);
                            if (previousMarker != null) {
                                ActiveMarker = previousMarker;
                                Timeflow.Markers.GotoMarker(previousMarker);
                                handled = true;
                            }
                        }
                        if (!handled) {
                            ActiveMarker = marker;
                            Timeflow.Markers.GotoMarker(marker);
                            handled = true;
                        }
                    }
                }
                if (!handled) {
                    ActiveMarker = null;
                    Timeflow.Rewind();
                    if (!Timeflow.IsPlaying) Timeflow.Play();
                }
                GetDuration();
            }
        }

        /// <summary>
        /// For scenes which use markers to define multiple examples, this advances to the next one. 
        /// </summary>
        public void Advance()
        {
            if (Timeflow == null) return;
            TimeflowMarker marker = Timeflow.Markers.GetNextMarker(Timeflow.CurrentTime);
            if (marker != null) {
                ActiveMarker = marker;
                Timeflow.Markers.GotoMarker(marker);
                GetDuration();
            }
            else {
                NextScene();
            }
        }

        public void NextScene()
        {
            DemoAppMain.Instance.LoadNext();
        }

        public void ExitApp()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();

#else
            Application.Quit();
#endif
        }

    }

}//AxonGenesis

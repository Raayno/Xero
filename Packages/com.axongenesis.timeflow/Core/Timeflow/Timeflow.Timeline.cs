// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        public PlayableDirector Director;

        [TimeflowIgnore]
        [SerializeField]
        [FormerlySerializedAs("AttachToDirector")]
        [FormerlySerializedAs("DirectorSyncEnabled")]
        private bool _DirectorSyncEnabled;

        [TimeflowIgnore]
        public bool DirectorIsPlaying;

        [TimeflowIgnore]
        public float DirectorTime;

        [TimeflowIgnore]
        public float DirectorTimeStart;

        [TimeflowIgnore]
        public float DirectorTimeEnd;

        [TimeflowIgnore]
        public bool IsDirectorInControl = false;

#if UNITY_EDITOR

        #region DELEGATES

        public delegate void OnDirectorUpdateDelegate();

        /// <summary>
        /// Used for Timeline integration to hook window refresh in editor only
        /// </summary>
        public event OnDirectorUpdateDelegate OnEditorDirectorUpdate;

        #endregion
#endif

        #region TIMELINE

        public bool DirectorSyncEnabled {
            get {
                // Nested Timeflows cannot sync with a director
                if (HasTimeflowParent) return false;
                return _DirectorSyncEnabled;
            }
            set {
                if (_DirectorSyncEnabled != value) {
                    _DirectorSyncEnabled = value;
                }
            }
        }

        public void SetupDirector()
        {
            if (Director == null) {
                /// This assumes there is 1 Timeline director in the scene. If you are using multiple
                /// directors, then each may be assigned to Timeflow explicitly in the Inspector. This is
                /// only assigned on initial setup.
                Director = UnityEngine.Object.FindFirstObjectByType<PlayableDirector>();
                if (Director != null) {
                    Transform parent = Director.transform.parent;

                    // Search for the top-most director in the hierarchy
                    while (parent != null) {
                        PlayableDirector parentDirector = parent.GetComponent<PlayableDirector>();
                        if (parentDirector != null) {
                            Director = parentDirector;
                        }
                        parent = parent.parent;
                    }
                    DirectorSyncEnabled = true;
                }
            }
            if (DirectorSyncEnabled) {
                LinkDirector();
            }
            else {
                UnlinkDirector();
            }
        }

        private void LinkDirector()
        {
            UnlinkDirector(); // prevent duplicate actions

            if (Director != null) {
                Director.played += OnPlayableDirectorPlayed;
                Director.paused += OnPlayableDirectorPaused;
                Director.stopped += OnPlayableDirectorPaused;
            }
        }

        private void UnlinkDirector()
        {
            if (Director != null) {
                Director.played -= OnPlayableDirectorPlayed;
                Director.paused -= OnPlayableDirectorPaused;
                Director.stopped -= OnPlayableDirectorPaused;
            }
        }

        public void OnPlayableDirectorPlayed(PlayableDirector director)
        {
            if (Director == director && !DirectorIsPlaying) {
                //if (DebugEnabled) Debug.Log($"{name}.OnPlayableDirectorPlayed");
                IsDirectorInControl = true;
                DirectorIsPlaying = true;
                CurrentTime = DirectorTime + DirectorTimeStart;
                Play(false);
            }
        }

        public void OnPlayableDirectorPaused(PlayableDirector director)
        {
            if (Director == director && DirectorIsPlaying) {
                //if (DebugEnabled) Debug.Log($"{name}.OnPlayableDirectorPaused");
                IsDirectorInControl = false;
                DirectorIsPlaying = false;
                Stop();
            }
        }

        public void PlayDirector()
        {
            if (DirectorSyncEnabled && !DirectorIsPlaying && Director != null) {
                //if (DebugEnabled) Debug.Log($"{name}.PlayDirector");
                DirectorIsPlaying = true;
                IsDirectorInControl = false;
                UpdateDirector();
                Director.Play();
            }
        }

        public void StopDirector()
        {
            if (DirectorSyncEnabled && DirectorIsPlaying && Director != null) {
                //if(DebugEnabled) Debug.Log($"{name}.StopDirector");
                DirectorIsPlaying = false;
                UpdateDirector();
                Director.Pause();
            }
        }

        /// <summary>
        /// This is only used when integrated with a Timeline director.
        /// </summary>
        public void UpdateDirector()
        {
            if (!DirectorSyncEnabled || Director == null || IsDirectorInControl || HasTimeflowParent) return;

            Director.time = CurrentTime + DirectorTimeStart;
            Director.Evaluate();

#if UNITY_EDITOR
            /// A delegate is used so that the Timeline integration can be optional and this script
            /// doesn't need to know about Timeline
            if (OnEditorDirectorUpdate != null) OnEditorDirectorUpdate();
#endif
        }

        #endregion
    }

}//AxonGenesis


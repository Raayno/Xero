// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AxonGenesis
{
    /// <summary>
    /// This is a specialized class for managing RenderToDisk across multiple scenes. The scenes listed in
    /// Build Settings will be rendered in that order. Each scene must have 1 instance of RenderToDisk
    /// enabled and prepared for rendering. To start the render queue, open the RenderQueue scene (make
    /// sure RenderQueue scene is NOT in the build list!) and enter play mode to begin rendering. If you
    /// don't have the RenderQueue scene, simply create a new empty scene with a single game object and add
    /// the RenderQueue component. 
    /// </summary>
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    [AddComponentMenu("Timeflow/Rendering/Render Queue")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-queue")]
    sealed public class RenderQueue : MonoBehaviour
    {
        public static RenderQueue Instance { get; private set; }
        public static bool LogEnabled = false;
        public static bool ForceFrameRenumbering = false;
        public static int ForceFrameRenumberingStart = 0;
        public static bool AbortAfterCurrentRender = false;

        public static RenderQueue Instantiate()
        {
            if (Instance == null) {
                GameObject obj = new GameObject("RenderQueue");
                DontDestroyOnLoad(obj);
                Instance = ObjectUtil.AddComponent<RenderQueue>(obj);

                // Prevent scene loading when instance is created procedurally since it is only
                // used to manage multiple RenderToDisk instances in thes ame scene.
                Instance.CanLoadScenes = false;
            }
            return Instance;
        }

        public static void RenderRangeFinished(string renderName, float timeElapsed)
        {
            if (Instance == null) {
                Instantiate();
            }
            Instance.LogRenderTime(renderName, timeElapsed);
        }

        public static void RenderFinished(RenderToDisk renderNext = null)
        {
            if (Instance == null) {
                Instantiate();
            }
            Instance.LoadNext(renderNext);
        }

        public static void ExitWhenFinished()
        {
            if (!VideoQueue.IsFinished) {
                VideoQueue.ExitWhenFinished();
            }
            else {
                EditorApplication.isPlaying = false;
            }
        }

        public bool DebugEnabled = false;

        public bool LogRenderTimes = true;
        public bool ForceFrameRenumber = false;
        public int ForceFrameRenumberStart = 0;

        [NonSerialized]
        public bool CanLoadScenes = true;

        [NonSerialized]
        private bool isStarting = true;

        [NonSerialized]
        private bool isLoading;

        [NonSerialized]
        private bool isFinished;

        [NonSerialized]
        private bool hasFinished = false;

        [NonSerialized]
        private int sceneIndex;

        [NonSerialized]
        private string renderTimes;

        private void Awake()
        {
            Instance = this;
            isFinished = false;
            hasFinished = false;
            renderTimes = "";
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (CanLoadScenes) {
                StartCoroutine(_LoadNext());
            }
        }

        private void Update()
        {
            if (isFinished) {
                LogEnabled = LogRenderTimes;
                ForceFrameRenumbering = ForceFrameRenumber;
                ForceFrameRenumberingStart = ForceFrameRenumberStart;

                if (!hasFinished) {
                    hasFinished = true;
                    if (LogRenderTimes) {
                        Debug.Log("Render Queue Finished:\n" + renderTimes);//--KEEP
                    }
                    ExitWhenFinished();
                }
            }
        }

        public void LogRenderTime(string renderName, float timeInSeconds)
        {
            if (LogRenderTimes) {
                string renderTime = $"{renderName}: Render Time: {StringUtil.SecondsToTimecode(timeInSeconds)}\n";
                Debug.Log(renderTime);//--KEEP
                renderTimes += renderTime;
            }
        }

        public void LoadNext(RenderToDisk renderNext = null)
        {
            StartCoroutine(_LoadNext(renderNext));
        }

        private IEnumerator _LoadNext(RenderToDisk renderNext = null)
        {
            if (AbortAfterCurrentRender) {
                AbortAfterCurrentRender = false;
                isFinished = true;
                yield break;
            }
            if (renderNext != null) {
                isFinished = false;
                // Load the next RenderToDisk instance
                // Wait a second for the previous RenderToDisk instance to fully disable
                yield return new WaitForSecondsRealtime(1f);

                renderNext.gameObject.SetActive(true);
                yield break;
            }
            else
            if (!CanLoadScenes) {
                isFinished = true;
                yield break;
            }
            else {
                if (isLoading) {
                    yield break;
                }
                isLoading = true;
                if (isStarting) {
                    isFinished = false;
                    isStarting = false;
                    sceneIndex = 0;
                }
                else {
                    sceneIndex++;
                }
                //if (DebugEnabled) Debug.Log("RenderQueue.LoadNext:" + sceneIndex + " count:" + SceneManager.sceneCountInBuildSettings);
                if (sceneIndex >= SceneManager.sceneCountInBuildSettings - 1) {
                    //if (DebugEnabled) Debug.Log("RenderQueue.LoadNext: FINISHED");
                    isFinished = true;
                    yield break;
                }


                //if (DebugEnabled) Debug.Log("RenderQueue.LoadNext: Loading:" + sceneIndex);
                yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
                isLoading = false;
            }
        }

    }

}//AxonGenesis
#endif

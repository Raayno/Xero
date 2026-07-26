// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class VideoQueue : MonoBehaviour
    {
        private static VideoQueue _Instance = null;

        public static VideoQueue Instance => _Instance;

        public static VideoQueue Instantiate()
        {
            if (!IsSupported) {
                Debug.LogWarning("Video encoding is not supported on the current Unity editor platform");
                return null;
            }
            if (_Instance == null) {
                GameObject obj = new GameObject("VideoQueue");
                _Instance = obj.AddComponent<VideoQueue>();
            }
            return _Instance;
        }

        private static Queue<VideoQueueItem> Items = null;

        private static Process process = null;

        public static bool IsSupported {
            get {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                return true;
#else
                return false;
#endif
            }
        }

        public static bool SuppressLog { get; set; }

        public static bool IsProcessing { get; private set; }

        public static bool IsFinished => !IsProcessing && (Items == null || Items.Count == 0);

        public static int RemainingItems => Items == null ? 0 : Items.Count;

        public static void StopCurrent()
        {
            if(process != null) process.Kill();
            IsProcessing = false;
            Debug.Log($"Stopped current video encoding process.");//--KEEP
        }

        public static void StopAll()
        {
            StopCurrent();
            Items = null;
            IsProcessing = false;
            Debug.Log($"Stopped current video encoding process.");//--KEEP
        }

        public static void ExitWhenFinished()
        {
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance._ExitWhenFinished());
        }

        public static string App {
            get {
                if (!string.IsNullOrEmpty(TimeflowPreferences.Current.FFMPEGPath)) {
                    return TimeflowPreferences.Current.FFMPEGPath;
                }

#if UNITY_EDITOR_WIN
                return "ffmpeg.exe";
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                return "/usr/local/bin/ffmpeg";
#endif
            }
        }

        public static void AddItem(VideoQueueItem item)
        {
            if (item == null) return;
            Debug.Log($"Video Queued: {item.OutputName}");//--KEEP
            if (Items == null) Items = new Queue<VideoQueueItem>();
            item.Aborted = false;
            Items.Enqueue(item);
        }

        private void Awake()
        {
            if (Application.isPlaying) {
                DontDestroyOnLoad(this);
            }
        }

        void Update()
        {
            if (!IsProcessing && !IsFinished) {
                VideoQueueItem item = Items.Dequeue();
                while (item.Aborted && Items.Count > 0) {
                    item = Items.Dequeue();
                }
                if (!item.Aborted) {
                    Execute(item);
                }
            }
        }

        private void Execute(VideoQueueItem item)
        {
            IsProcessing = true;
            Debug.Log($"Starting Process: {App}");//--KEEP
            Debug.Log($"Begin Encoding: {item.OutputName} Remaining Items:{Items.Count}");//--KEEP
            Debug.Log($"{item.Command}");//--KEEP
            item.Prepare();

            ProcessStartInfo start = new ProcessStartInfo(App);

            start.Arguments = item.Command;
            start.CreateNoWindow = !item.ShowProcessWindow;
            start.ErrorDialog = true;
            start.UseShellExecute = false;
            start.WorkingDirectory = "./";
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.Encoding.UTF8;

            process = new Process {
                StartInfo = start,
                EnableRaisingEvents = true
            };

            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                if (!SuppressLog) Debug.Log($"ffmpeg: {e.Data}");//--KEEP
            };
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                if (!SuppressLog) Debug.Log($"ffmpeg: {e.Data}");//--KEEP
            };
            process.Exited += delegate (object sender, System.EventArgs e) {
                IsProcessing = false;
                if (!SuppressLog) Debug.Log($"ffmpeg: {e}");//--KEEP
                Debug.Log($"Finished: {item.OutputName} Remaining Items:{Items.Count}");//--KEEP
            };

            try {
                process.Start();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                int result = EditorUtility.DisplayDialogComplex("Failed to encode video. FFMPEG not found!", "Please check that FFMPEG is installed on your system and that " +
                    "it has been registered in the PATH environment variables. Alternatively, you may specify a full system file path " +
                    "to the ffmpeg executable in the Timeflow Preferences.", "Ok", "Get FFMPEG", "");
                if (result == 1) {
                    Application.OpenURL("http://ffmpeg.org");
                }
                if (RenderToDisk.Instance != null) {
                    RenderToDisk.Instance.Abort();
                }
                return;
            }

            process.BeginErrorReadLine();
        }

        private IEnumerator _ExitWhenFinished()
        {
            Debug.Log("Please stay in play mode. Will exit when video encodings are finished...");//--KEEP
            while (!VideoQueue.IsFinished) {
                yield return null;
            }
            Debug.Log($"All videos finished!");//--KEEP

            // Allow a little extra time in case anything gets queued before quitting
            yield return new WaitForSecondsRealtime(2f);

            EditorApplication.isPlaying = false;
            yield break;
        }

    }
}//AxonGenesis
#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

namespace AxonGenesis
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class SampleInstaller
    {
        private const string TimeflowCoreBehaviorsGUID = "3750521cba76e534d983311ff84b0467";
        private const string PathInAssets = "Samples/Timeflow Animation System Samples"; // Where samples will be copied

        public static string InstallPath(string samplesName)
        {
            return Path.Combine(Application.dataPath, PathInAssets, samplesName);
        }

        public static void Install(string samplesName)
        {
            string samplesPath = AssetDatabase.GUIDToAssetPath(TimeflowCoreBehaviorsGUID).Replace("Core/Behaviors", "Samples~");

            if (string.IsNullOrEmpty(samplesPath)) {
                Debug.LogError($"Could not find Timeflow samples: {samplesPath}");
                return;
            }

            string sourceSamplesPath = Path.Combine(samplesPath, samplesName);

            if (!Directory.Exists(sourceSamplesPath)) {
                Debug.LogError($"[SampleInstaller] Samples directory not found in package at: {sourceSamplesPath}");
                return;
            }

            string targetSamplesPath = Path.Combine(Application.dataPath, PathInAssets);
            if (!Directory.Exists(targetSamplesPath)) {
                Directory.CreateDirectory(targetSamplesPath);
            }

            targetSamplesPath += "/" + Timeflow.Version;
            if (!Directory.Exists(targetSamplesPath)) {
                Directory.CreateDirectory(targetSamplesPath);
            }

            targetSamplesPath += "/" + samplesName;
            if (!Directory.Exists(targetSamplesPath)) {
                Directory.CreateDirectory(targetSamplesPath);
            }

            try {
                CopyDirectory(sourceSamplesPath, targetSamplesPath);
                Debug.Log($"{samplesName} successfully installed to: {PathInAssets}");//--KEEP
                AssetDatabase.Refresh(); // Refresh AssetDatabase to pick up new files
            }
            catch (System.Exception e) {
                Debug.LogError($"Error installing samples: {e.Message}");
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories)) {
                string dir = dirPath.Replace(sourceDir, destinationDir);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(sourceDir, destinationDir), true);
            }
        }
    }

}

#endif

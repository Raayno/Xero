// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public class MidiAssetPostprocessor : AssetPostprocessor
    {
        static string[] extensions = new string[] { "mid", "midi" };


        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            if (TimeflowPreferences.Current == null) return;
            if (!TimeflowPreferences.Current.EnableMidiFileRenaming) return;
            foreach (string assetPath in importedAssets) {
                bool convert = false;
                string path = assetPath.ToLower();
                foreach (string ext in extensions) {
                    if (path.EndsWith("." + ext)) {
                        convert = true;
                        break;
                    }
                }

                if (convert) {
                    if (!File.Exists(assetPath)) {
                        continue;
                    }

                    string destFile = Path.GetDirectoryName(assetPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(assetPath) + ".bytes";
                    Debug.Log("Converted " + path + " to .bytes:" + destFile);//--KEEP
                    File.Move(assetPath, destFile);
                    AssetDatabase.ImportAsset(destFile);
                }
            }
        }
    }
}//AxonGenesis

#endif
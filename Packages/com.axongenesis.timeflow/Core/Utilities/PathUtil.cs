// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace AxonGenesis
{
    public class PathUtil : MonoBehaviour
    {
#if UNITY_EDITOR_WIN
        public static readonly string Separator = @"\";
#else
        public static readonly string Separator = @"/";
#endif

        public static string AppPath => Application.dataPath;

        public static string CustomPath => TimeflowPreferences.Current.CustomRenderPath;

        public static string ProjectPath => System.IO.Directory.GetParent(Application.dataPath).ToString();

        public static string Clean(string path, bool isDirectory = true)
        {
#if UNITY_EDITOR_WIN
            path = path.Replace(@"/", Separator);
#else
            path = path.Replace(@"\", Separator);
#endif
            path = path.Replace("\"", Separator);
            path = path.Replace("'", Separator);

            if (isDirectory && !path.EndsWith(Separator)) {
                path += Separator;
            }

            string doubleSep = Separator + Separator;
            if (path.Contains(doubleSep)) {
                path.Replace(doubleSep, Separator);
            }
            return path;
        }

        public static string Wildcards(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.Contains("$ASSETS")) {
                path = path.Replace("$ASSETS", AppPath);
            }
            else
            if (path.Contains("$CUSTOM") && !string.IsNullOrEmpty(CustomPath)) {
                path = path.Replace("$CUSTOM", CustomPath);
            }
            else
            if (path.Contains("$PROJECT")) {
                path = path.Replace("$PROJECT", ProjectPath);
            }
            path = Clean(path);
            return path;
        }

        public static List<string> GetDirectories(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                Debug.LogWarning("Path is null or empty");
                return null;
            }
            if(!System.IO.Directory.Exists(path)) {
                Debug.LogWarning($"Path does not exist: {path}");
                return null;
            }

            string[] directories = System.IO.Directory.GetDirectories(path);
            List<string> folderNames = new List<string>();

            foreach (string directory in directories) {
                //Debug.Log($"directory:{directory}");
                folderNames.Add(System.IO.Path.GetFileName(directory));
            }

            return folderNames;
        }
    }
}
#endif
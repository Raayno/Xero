// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Context menu for saving mesh data. This is a helpful utility for saving generative mesh data as an
    /// asset.
    /// </summary>
    public static class MeshUtilMenu
    {
        [UnityEditor.MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
        public static void SaveMeshInPlace(MenuCommand menuCommand)
        {
            MeshFilter mf = menuCommand.context as MeshFilter;
            if (mf != null) {
                Mesh m = mf.sharedMesh;

                string path = EditorUtility.SaveFilePanel("Save Mesh", "Assets/", m.name, "asset");
                if (string.IsNullOrEmpty(path)) return;
                path = FileUtil.GetProjectRelativePath(path);

                SaveMesh(m, path, false, true);
            }
        }

        [UnityEditor.MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
        public static void SaveMeshNewInstanceItem(MenuCommand menuCommand)
        {
            MeshFilter mf = menuCommand.context as MeshFilter;
            if (mf != null) {
                Mesh m = mf.sharedMesh;

                string path = EditorUtility.SaveFilePanel("Save Mesh As New Instance", "Assets/", m.name, "asset");
                if (string.IsNullOrEmpty(path)) return;

                path = FileUtil.GetProjectRelativePath(path);
                SaveMesh(m, path, true, true);
            }
        }

        public static Mesh SaveMesh(Mesh mesh, string path, bool makeNewInstance, bool optimizeMesh)
        {
            Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

            if (optimizeMesh)
                MeshUtility.Optimize(meshToSave);

            meshToSave = EditorUtil.CreateOrReplaceAsset<Mesh>(meshToSave, path);

            Debug.Log("Saved Mesh:" + path);//--KEEP

            return meshToSave;
        }

    }

}//AxonGenesis

#endif
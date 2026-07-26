// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Utilities for working with meshes.
    /// </summary>
    public static class MeshUtil
    {
        public static List<MeshFilter> mMeshes = null;

        public static void GatherMeshesRecursive(GameObject obj)
        {
            if (obj.TryGetComponent<MeshFilter>(out var mf)) mMeshes.Add(mf);
            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    GatherMeshesRecursive(child.gameObject);
                }
            }
        }


        public static void SaveMeshAsset(Mesh mesh, string path)
        {
            if (!Application.isPlaying) {
                Debug.Log("SaveMeshAsset:" + path);//--KEEP
                AssetDatabase.CreateAsset((Mesh)UnityEngine.Object.Instantiate(mesh), path);
                AssetDatabase.SaveAssets();

                MakeReadable(path);
            }
        }

        public static void CopyMeshAsset(MeshFilter mr, string path)
        {
            if (!Application.isPlaying) {
                Debug.Log("CopyMeshAsset:" + path);//--KEEP
                AssetDatabase.CreateAsset((Mesh)UnityEngine.Object.Instantiate(mr.sharedMesh), path);
                AssetDatabase.SaveAssets();

                MakeReadable(path);
                mr.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
            }
            else {
                mr.sharedMesh = (Mesh)UnityEngine.Object.Instantiate(mr.sharedMesh);
            }
        }

        public static void CopyMeshAsset(SkinnedMeshRenderer mr, string path)
        {
            if (!Application.isPlaying) {
                Debug.Log("CopyMeshAsset:" + path);//--KEEP
                AssetDatabase.CreateAsset((Mesh)UnityEngine.Object.Instantiate(mr.sharedMesh), path);
                AssetDatabase.SaveAssets();

                MakeReadable(path);
                mr.sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
            }
            else {
                mr.sharedMesh = (Mesh)UnityEngine.Object.Instantiate(mr.sharedMesh);
            }
        }

        public static Mesh FreezeMesh(GameObject targetObject)
        {
            Debug.Log("FreezeMesh:" + targetObject.name);//--KEEP
            GameObject copy = GameObject.Instantiate(targetObject);
            UndoUtil.UndoCreate(copy, "Freeze Mesh");
            Mesh mesh = null;
            MeshFilter mf = copy.GetComponent<MeshFilter>();
            SkinnedMeshRenderer smr = copy.GetComponent<SkinnedMeshRenderer>();
            if (mf != null || smr != null) {
                if (mf != null) {
                    CopyMeshAsset(mf, "Assets/" + copy.name + "_mesh_freeze.asset");
                    mesh = mf.sharedMesh;
                }
                else {
                    Mesh baked = new Mesh();
                    smr.BakeMesh(baked);
                    SaveMeshAsset(baked, "Assets/" + copy.name + "_mesh_freeze.asset");
                    mesh = baked;

                    mf = copy.AddComponent<MeshFilter>();
                    mf.sharedMesh = baked;

                    MeshRenderer mr = copy.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = smr.sharedMaterial;

                    GameObject.DestroyImmediate(smr);
                }

                // Capture vertices in world space before transform reset
                Vector3[] vertices = new Vector3[mesh.vertexCount];
                int i = 0;
                foreach (Vector3 v in mesh.vertices) {
                    vertices[i] = targetObject.transform.TransformPoint(v);
                    i++;
                }

                // Reset transform to 0
                copy.transform.SetParent(null);
                copy.transform.position = Vector3.zero;
                copy.transform.rotation = Quaternion.identity;
                copy.transform.localScale = Vector3.one;

                mf.sharedMesh.vertices = vertices;
                mf.sharedMesh.RecalculateBounds();
            }
            else {
                Debug.LogError("Invalid mesh. Select a game object with a MeshFilter or SkinnedMeshRenderer component.");
            }
            return mesh;
        }
        public static void GetMeshPolycount(GameObject targetObject)
        {
            if (targetObject == null) return;
            List<MeshFilter> meshFilters = ObjectUtil.GetComponentsRecursive<MeshFilter>(targetObject);
            if (meshFilters == null || meshFilters.Count == 0) {
                Debug.LogWarning("There are no meshes in the selected game objects");//--KEEP
                return;
            }

            string log = "";
            int totalTriangles = 0;
            int totalVertices = 0;
            int totalSubmeshes = 0;
            foreach (MeshFilter filter in meshFilters) {
                if(filter == null || filter.sharedMesh == null) continue;
                int triangles = filter.sharedMesh.triangles.Length / 3;
                totalTriangles += triangles;
                totalVertices += filter.sharedMesh.vertexCount;
                totalSubmeshes += filter.sharedMesh.subMeshCount;
                log += $"{filter.name}: triangles:{triangles} vertices:{filter.sharedMesh.vertexCount} subMeshes:{filter.sharedMesh.subMeshCount}\n";
            }

            Debug.Log($"{targetObject.name}: total triangles:{totalTriangles} vertices:{totalVertices} subMeshes:{totalSubmeshes}\n" + log);//--KEEP
        }

        public static void MakeReadable(string meshAssetPath)
        {
            string fileText = File.ReadAllText(meshAssetPath);
            fileText = fileText.Replace("m_IsReadable: 0", "m_IsReadable: 1");
            File.WriteAllText(meshAssetPath, fileText);
            AssetDatabase.Refresh();
        }

        public static void CombineMeshes(GameObject[] objects)
        {
            CombineMeshes(objects, null, false);
        }

        public static void CombineMeshes(GameObject[] objects, GameObject targetObject, bool destroyObjects)
        {
            Debug.Log("CombineMeshes");//--KEEP
            if (objects == null || objects.Length == 0) {
                Debug.LogWarning("No objects selected for CombineMeshes");
                return;
            }
            if (targetObject == null) {
                targetObject = new GameObject(objects[0].name + " Combined");
                UndoUtil.UndoCreate(targetObject, "Combine Meshes");
            }
            if (objects != null && objects.Length > 0 && targetObject != null) {
                MeshRenderer mr = null;
                Material mat = null;
                mMeshes = new List<MeshFilter>();

                UndoUtil.Undo(targetObject, "Combine Meshes");

                Transform parent = targetObject.transform.parent;
                Vector3 pos = targetObject.transform.localPosition;
                Quaternion rot = targetObject.transform.localRotation;
                Vector3 scale = targetObject.transform.localScale;

                targetObject.transform.parent = null;
                targetObject.transform.localPosition = Vector3.zero;
                targetObject.transform.localRotation = Quaternion.identity;
                targetObject.transform.localScale = Vector3.one;

                foreach (GameObject obj in objects) {
                    if (obj != null) {
                        GatherMeshesRecursive(obj);
                    }
                }

                CombineInstance[] combine = new CombineInstance[mMeshes.Count];
                for (int i = 0; i < mMeshes.Count; i++) {
                    combine[i].mesh = mMeshes[i].sharedMesh;
                    combine[i].transform = mMeshes[i].transform.localToWorldMatrix;

                    UndoUtil.Undo(mMeshes[i].gameObject, "Combine Meshes");
                    mMeshes[i].gameObject.SetActive(false);
                    if (mat == null) {
                        mr = mMeshes[i].GetComponent<MeshRenderer>();
                        if (mr != null && mr.sharedMaterial != null) {
                            mat = mr.sharedMaterial;
                        }
                    }
                }

                MeshFilter mf = targetObject.GetComponent<MeshFilter>();
                if (mf == null) mf = targetObject.AddComponent<MeshFilter>();
                if (mf != null) {
                    mf.sharedMesh = new Mesh();
                    mf.sharedMesh.CombineMeshes(combine);
                    targetObject.SetActive(true);
                }

                mr = targetObject.GetComponent<MeshRenderer>();
                if (mr == null) mr = targetObject.AddComponent<MeshRenderer>();
                if (mr != null) {
                    mr.enabled = true;
                    mr.sharedMaterial = mat;
                }

                targetObject.transform.parent = parent;
                targetObject.transform.localPosition = pos;
                targetObject.transform.localRotation = rot;
                targetObject.transform.localScale = scale;

                if (destroyObjects) {
                    foreach (GameObject obj in objects) {
                        UndoUtil.UndoDestroy(obj);
                    }
                }
            }
        }
#if AXON_EXPERIMENTAL
#endif

    }
}//AxonGenesis
#endif

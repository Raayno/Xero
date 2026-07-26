// Copyright 2025 Axon Genesis. All rights reserved.
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
    [CreateAssetMenu(fileName = "Curve Asset", menuName = "Timeflow/Curve Asset", order = 1)]
    public class CurveAsset : ScriptableObject
    {
        [SerializeField]
        public Curve Curve = new Curve();

#if UNITY_EDITOR
        public static void SaveAsset(Curve curve, string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<CurveAsset>(path);
            if (asset == null) {
                asset = ScriptableObject.CreateInstance<CurveAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.Curve = curve;
            AssetDatabase.SaveAssets();

            SelectionUtil.Select(asset);
        }
#endif
    }
}

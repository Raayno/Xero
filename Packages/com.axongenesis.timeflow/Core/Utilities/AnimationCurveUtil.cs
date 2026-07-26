// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.


#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public static class AnimationCurveUtil
    {
        public static void SetKeyframe(AnimationCurve curve, UnityEngine.Keyframe key)
        {
            if (curve == null) return;
            curve.AddKey(key);
        }

        // Method to add or set a keyframe at a specific time with a specific value
        public static UnityEngine.Keyframe? SetKeyframe(AnimationCurve curve, float time, float value)
        {
            if (curve == null) return null;

            // Check if a key already exists at the given time
            int keyIndex = FindKeyIndexAtTime(curve, time);

            if (keyIndex != -1) {
                // Update existing key
                curve.MoveKey(keyIndex, new UnityEngine.Keyframe(time, value));
            }
            else {
                // Add a new keyframe if no key exists at the given time
                keyIndex = curve.AddKey(new UnityEngine.Keyframe(time, value));
            }
            return curve[keyIndex];
        }

        // Method to get the value of the curve at a specific time
        public static float GetValueAtTime(AnimationCurve curve, float time)
        {
            if (curve == null) return 0f;

            return curve.Evaluate(time);
        }

        // Method to retrieve a keyframe at a specific index
        public static UnityEngine.Keyframe? GetKeyframe(AnimationCurve curve, int index)
        {
            if (curve != null && index >= 0 && index < curve.length) {
                return curve[index];
            }
            Debug.LogWarning("Index out of bounds for keyframe retrieval.");
            return null;
        }

        // Method to find an existing key index at a specific time (returns -1 if none found)
        public static int FindKeyIndexAtTime(AnimationCurve curve, float time)
        {
            if (curve == null) return -1;

            for (int i = 0; i < curve.length; i++) {
                if (Mathf.Approximately(curve[i].time, time)) {
                    return i;
                }
            }
            return -1;
        }

        // Method to remove a keyframe at a specific time
        public static void RemoveKeyframe(AnimationCurve curve, float time)
        {
            if (curve == null) return;

            int keyIndex = FindKeyIndexAtTime(curve, time);

            if (keyIndex != -1) {
                curve.RemoveKey(keyIndex);
            }
            else {
                Debug.LogWarning("No keyframe found at the specified time to remove.");
            }
        }

        // Method to clear all keyframes from the curve
        public static void ClearAllKeyframes(AnimationCurve curve)
        {
            if (curve == null) return;

            curve.keys = new UnityEngine.Keyframe[0];
        }

        public static void SaveCurveAsAsset(AnimationCurve curve, string path)
        {
            var curveContainer = AssetDatabase.LoadAssetAtPath<AnimationCurveContainer>(path);
            if (curveContainer == null) {
                curveContainer = ScriptableObject.CreateInstance<AnimationCurveContainer>();
                AssetDatabase.CreateAsset(curveContainer, path);
            }
            curveContainer.Curve = curve;
            AssetDatabase.SaveAssets();

            SelectionUtil.Select(curveContainer);
        }
    }
}
#endif
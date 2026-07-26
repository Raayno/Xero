// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    public partial class Keyframe : SerializableObject
    {
        //private static bool DebugEnabled = false;
        public static bool OverrideLocks = false;

        private static List<Keyframe> ExposedKeyframes;

        public static void RegisterExposedKeyframe(Keyframe k)
        {
            if (ExposedKeyframes == null) ExposedKeyframes = new List<Keyframe>();
            if (!ExposedKeyframes.Contains(k)) {
                ExposedKeyframes.Add(k);
            }
        }

        public static void UnregisterExposedKeyframe(Keyframe k)
        {
            if (ExposedKeyframes == null && k != null) return;
            if (ExposedKeyframes.Contains(k)) {
                ExposedKeyframes.Remove(k);
            }
        }

        /// <summary>
        /// Finds the first keyframe with matching ID.
        /// </summary>
        /// <param name="id">The ID to locate, matching the keyframe ExposedID</param>
        /// <returns>The Keyframe object or null if none found</returns>
        public static Keyframe GetExposedKeyframe(int id)
        {
            Keyframe key = null;
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        key = k;
                        break;
                    }
                }
            }
            return key;
        }

        /// <summary>
        /// Returns the float value of a keyframe matching the provided ID.
        /// </summary>
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The input value is returned if no matching keyframe is found</param>
        /// <returns>The keyframe value</returns>
        public static float GetExposedKeyframe(int id, float value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        value = k.KeyValue;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Returns the Color value of a keyframe matching the provided ID.
        /// </summary>
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The input value is returned if no matching keyframe is found</param>
        /// <returns>The keyframe value</returns>
        public static Color GetExposedKeyframe(int id, Color value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        value = k.KeyColor;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Returns the Vector4 value of a keyframe matching the provided ID.
        /// </summary>
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The input value is returned if no matching keyframe is found</param>
        /// <returns>The keyframe value</returns>
        public static Vector4 GetExposedKeyframe(int id, Vector4 value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        value = k.KeyVector;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// This sets a keyframe value by a specified global name. This could be used to dynamically and
        /// procedurally change keyframe values, which can be useful to create variations.
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The new value to assign to the keyframe</param>
        /// </summary>
        public static void SetExposedKeyframe(int id, float value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        bool tmp = k.LockValue;
                        k.LockValue = false; // bypass the lock to set directly
                        k.KeyValue = value;
                        k.LockValue = tmp;
                    }
                }
            }
        }

        /// <summary>
        /// This sets a keyframe Color value by a specified global name. This could be used to dynamically
        /// and procedurally change keyframe values, which can be useful to create variations.
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The new value to assign to the keyframe</param>
        /// </summary>
        public static void SetExposedKeyframe(int id, Color value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        bool tmp = k.LockValue;
                        k.LockValue = false; // bypass the lock to set directly
                        k.KeyColor = value;
                        k.LockValue = tmp;
                    }
                }
            }
        }

        /// <summary>
        /// This sets a keyframe Vector4 value by a specified global name. This could be used to
        /// dynamically and procedurally change keyframe values, which can be useful to create variations.
        /// <param name="id">The ExposedID of the keyframe</param>
        /// <param name="value">The new value to assign to the keyframe</param>
        /// </summary>
        public static void SetExposedKeyframe(int id, Vector4 value)
        {
            if (ExposedKeyframes != null) {
                for (int i = 0; i < ExposedKeyframes.Count; i++) {
                    Keyframe k = ExposedKeyframes[i];
                    if (k != null && k.ExposedID == id) {
                        bool tmp = k.LockValue;
                        k.LockValue = false; // bypass the lock to set directly
                        k.KeyVector = value;
                        k.LockValue = tmp;
                    }
                }
            }
        }
    }
}
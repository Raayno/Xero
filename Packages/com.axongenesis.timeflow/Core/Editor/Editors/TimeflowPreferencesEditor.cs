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

    [CustomEditor(typeof(TimeflowPreferences))]
    public class TimeflowPreferencesEditor : Editor
    {
        private TimeflowPreferences preferences;
        private SerializedObject settings;

        private void OnEnable()
        {
            preferences = (TimeflowPreferences)target;
            settings = new SerializedObject(preferences);
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup(140);
            TimeflowPreferencesIMGUIRegister.Settings(settings, preferences);

            if (GUI.changed) {
                EditorUtility.SetDirty(preferences);
            }
        }
    }
}
#endif
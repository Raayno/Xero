#if UNITY_EDITOR  
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(AdvancedPreset))]
    public class AdvancedPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Editing is disabled here. Please use the Advanced Presets window for editing.", MessageType.Info);

            if (AxonGUI.Button("Edit in Advanced Presets Window", GUI.skin.button)) {
                AdvancedPresetsWindow.EditPreset(target as AdvancedPreset);
            }

        }
    }
}
#endif

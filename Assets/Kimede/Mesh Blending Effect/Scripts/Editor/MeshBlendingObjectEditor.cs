using UnityEngine;
using UnityEditor;

namespace Kimede
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshBlendingObject))]
    public class MeshBlendingObjectEditor : Editor
    {
        private SerializedProperty blendingRadius;
        private SerializedProperty showDebugInfo;

        private void OnEnable()
        {
            blendingRadius = serializedObject.FindProperty("blendingRadius");
            showDebugInfo = serializedObject.FindProperty("showDebugInfo");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MeshBlendingObject script = (MeshBlendingObject)target;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Per-Object Mesh Blending Radius", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This component allows you to customize the blending radius for this specific object. " +
                "The radius is encoded in the ObjectID texture and read by the fullscreen shader.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Blending Radius
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Blending Radius Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(blendingRadius, new GUIContent("Blend Radius",
                "Size of the blend area around this object (0.01 - 2.0)"));

            // Visual feedback of current value
            EditorGUILayout.Space(5);
            float normalizedValue = Mathf.InverseLerp(0.01f, 2.0f, script.blendingRadius);
            EditorGUILayout.LabelField($"Normalized Value: {normalizedValue:F3} (stored in texture alpha)");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

    

            // Debug
            EditorGUILayout.PropertyField(showDebugInfo, new GUIContent("Show Debug Info"));

            if (showDebugInfo.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Debug info will be logged to Console when values update.",
                    MessageType.None);
            }

            EditorGUILayout.Space(10);

            // Quick Actions
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set to Min (0.01)"))
            {
                blendingRadius.floatValue = 0.01f;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Set to Default (0.1)"))
            {
                blendingRadius.floatValue = 0.1f;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Set to Max (2.0)"))
            {
                blendingRadius.floatValue = 2.0f;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

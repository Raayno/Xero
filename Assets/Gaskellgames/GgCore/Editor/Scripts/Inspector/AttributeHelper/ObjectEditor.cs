#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using NaughtyAttributes.Editor;

namespace Gaskellgames.EditorOnly
{
    /// <summary>
    /// Code updated by Gaskellgames
    /// Original code from 'EditorCools': https://github.com/datsfain/EditorCools
    /// </summary>
    
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    internal class ObjectEditor : NaughtyInspector
    {
        private ButtonDrawer buttonDrawer;

        protected override void OnEnable()
        {
            base.OnEnable();
            buttonDrawer = new ButtonDrawer(target);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (buttonDrawer != null && targets != null && 0 < targets.Length)
            {
                EditorGUILayout.Space();
                buttonDrawer.DrawButtons(targets);
            }
        }
        
    } // class end
}
#endif
#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    public abstract class ComponentPresetEditorBase<T, UI> : Editor
    where T : ComponentPreset
    where UI : ComponentPresetEditBase<T>, new()
    {
        public UI GUI;

        public virtual void OnEnable()
        {
            GUISetup();
            if (GUI != null) {
                GUI.OnEnable();
            }
        }

        public virtual void OnDisable()
        {
            GUISetup();
            if (GUI != null) {
                GUI.OnDisable();
            }
        }
        protected void GUISetup()
        {
            if (GUI == null && target != null) {
                GUI = new UI();
                GUI.SetTarget<T>((T)target, this);
            }
        }

        public override void OnInspectorGUI()
        {
            GUISetup();
            
            GUI.MainGUI();

            if (UnityEngine.GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

#endif

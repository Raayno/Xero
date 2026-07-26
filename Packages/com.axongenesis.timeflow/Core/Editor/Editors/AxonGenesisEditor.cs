// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    /// <summary>
    /// This defines the base class for an inspector editor window using AxonGenesisBehavior. This acts as
    /// a wrapper to send UI messages to a class derrived from AxonGenesisBehaviorEdit, which provides an improved
    /// polymorphic interface for building inspector UIs. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="UI"></typeparam>
    public class AxonGenesisEditor<T, UI> : Editor
        where T : AxonGenesisBehavior
        where UI : AxonGenesisBehaviorEdit<T>, new()
    {
        public UI GUI;

        protected bool IsProjectView = false;

        protected void GUISetup()
        {
            if (GUI == null && target != null) {
                GUI = new UI();
                GUI.SetTarget<T>((T)target);
                GUI.editor = this;
                IsProjectView = EditorUtility.IsPersistent(target);
            }
        }

        public override void OnInspectorGUI()
        {
            GUISetup();
            if (IsProjectView) {
                AxonGUI.HelpBox("Direct editing not allowed from the project view. Please open this prefab or add to a scene to edit");
                return;
            }

            string typeName = typeof(T).ToString().Replace("AxonGenesis.", "");
            if (!GUI.GUIHeader(typeName)) return;

            if (GUI.target != null && GUI.target.Enabled) {
                GUI.GUIBegin();
                if (IsEditingMultiple()) {
                    GUI.OnMultiEditGUI();
                }
                else {
                    GUI.OnInspectorGUI();
                }
                GUI.GUIEnd();
            }
            else {
                AxonGUI.HelpBox("An unknown error has occurred. The target for this editor is null.", MessageType.Error);
            }
            if (UnityEngine.GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }

        public void OnSceneGUI()
        {
            if (IsProjectView) return;
            GUISetup();
            GUI.OnSceneGUI();
        }

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

        public bool IsEditingMultiple()
        {
            if (targets != null && targets.Length > 1) return true;
            return false;
        }
    }

}//AxonGenesis
#endif
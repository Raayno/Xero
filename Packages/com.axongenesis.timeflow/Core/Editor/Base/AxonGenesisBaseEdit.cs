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
    /// Base class for AxonGenesis editor UIs. This provides a polymorphic interface for more easily
    /// working with class inheritance and sharing UI displays with derrived classes. This is placed in the
    /// runtime assembly on purpose for script dependencies, however is stripped from builds.
    /// </summary>
    public class AxonGenesisBaseEdit
    {
        /// <summary>
        /// Flag to indicate when being displayed in the TimeflowInsepctor window so that the UI can be
        /// hidden in the main Unity Insepctor to avoid double-drawing the UI. The user can choose not to
        /// use the TimeflowInspector if they choose
        /// </summary>
        public static bool IsTimeflowInspector = false;

        public Editor editor = null;

        public virtual void SetTarget(AxonGenesisBehavior target) { }

        public virtual void OnEnable() {}

        public virtual void OnDisable() { }

        public virtual void GUISetup()
        {
            AxonGUI.Setup();
        }

        public virtual bool GUIHeader(string typeName)
        {
            return false;
        }

        public virtual void GUIBegin()
        {
            AxonGUI.BeginVertical();
        }

        public virtual void GUIEnd()
        {
            AxonGUI.EndVertical();
        }

        public virtual void OnInspectorGUI() { }

        public virtual void GUIMenu() { }

        public virtual void GUIMenuIcons() { }

        public virtual void GUIMenuOptions() { }

        public virtual void OnMultiEditGUI()
        {
            AxonGUI.HelpBox("Multi-object editing has not been implemented for this behavior", MessageType.Info);
        }

        public virtual void OnSceneGUI() { }

        public virtual void Defaults() { }

        public virtual void Refresh() { }

    }

}//AxonGenesis

#endif
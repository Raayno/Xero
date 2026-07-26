// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public interface IBehaviorEditor
    {
#if UNITY_EDITOR
        bool IsSelected { get; set; }
        bool ShowSelected { get; }
        Color GUIColor { get; set; }
        void ResetName();
        void EditorUpdate();
        void DrawGizmos();
        void OnDrawGizmos();
        void OnSavePrefab();
#endif
    }
}
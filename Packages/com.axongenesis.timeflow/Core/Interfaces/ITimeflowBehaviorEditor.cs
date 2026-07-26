// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public interface ITimeflowBehaviorEditor
    {
#if UNITY_EDITOR
        Color GUIColor { get; set; }
        bool IsSelected { get; set; }
        void OnNewInstance();
        void OnKeyChange();
        void OnNewGUIColor();
        void OnDragStart();
        void OnDragCancel();
        void OnDragUpdate();
        float DragTimeOffset(float offset, bool canSnap);
        void DeleteAllChannels();
        TimeflowChannel DuplicateChannel(TimeflowChannel channel, GameObject dstObject = null, bool deleteOriginal = false);
        void InsertTime(float start, float end, bool isLocalTime, bool isGlobal);
        void DuplicateTime(float start, float end, bool isLocalTime, bool isGlobal);
        void DeleteTime(float start, float end, bool isLocalTime, bool isGlobal);
        void ClearTime(float start, float end, bool isLocalTime, bool isGlobal, TimeflowView.SelectionModes mode = TimeflowView.SelectionModes.Any);
        void ScaleTime(float scale);
        void OnHierarchyChange();
        void ViewInspector();
        void GUIGraph(Rect rect);
        void GUIGraphFit(bool init, bool selectedOnly);
        void DrawGizmos();
#endif
    }
}
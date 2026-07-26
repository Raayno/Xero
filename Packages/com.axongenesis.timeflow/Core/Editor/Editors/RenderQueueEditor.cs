// Copyright 2023 AxonGenesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(RenderQueue))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-queue")]
    public class RenderQueueEditor : Editor
    {
#if TIMEFLOW_PRO
        public const string kAddRenderQueue = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "⏱️ Render Queue";
#else
        public const string kAddRenderQueue = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Render Queue";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Render Queue";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddRenderQueue, false, 221)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddRenderQueue, false, 221)]
        public static void AddRenderToDisk()
        {
            GameObject obj = new GameObject("RenderQueue");
            UndoUtil.UndoCreate(obj, "Add Render Queue");
            ObjectUtil.ResetTransform(obj);
            RenderQueue r = ObjectUtil.AddComponent<RenderQueue>(obj);

            SelectionUtil.Select(obj);

            EditorUtil.ShowDialog("Please see the documentation to configure RenderQueue",
                "Make sure that the scene is empty except for the RenderQueue object. " +
                "Add each scene you want to render to the Build Settings scenes list, " +
                "then play the RenderQueue scene to start rendering.");
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup();
            AxonGUI.HelpBox("Add the scenes you wish to render into the Build Settings scene list.\nEach scene must be configured and ready to render with a RenderToDisk instance.\n\nOpen an existing RenderQueue scene or create a new empty scene and apply this RenderQueue script to an empty game object.\n\nUpon entering playmode, each scene in the build is rendered in the order listed and exits playmode upon completion.", MessageType.Info);

            RenderQueue r = (RenderQueue)target;
            AxonGUI.UndoName = "Set Log Render Times";
            AxonGUI.SetTooltip("If enabled, the total rendering time for each render is recorded and output to the console upon render completion.");
            r.LogRenderTimes = AxonGUI.FieldToggle(r, "Log Render Times", r.LogRenderTimes);

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Frame Renumbering";
            AxonGUI.SetTooltip("Override all render range outputs to renumber frames starting at the specified number.");
            r.ForceFrameRenumber = AxonGUI.FieldToggle(r, "Frame Renumbering", r.ForceFrameRenumber);
            if (r.ForceFrameRenumber) {
                AxonGUI.UndoName = "Set Frame Renumbering Starting At";
                r.ForceFrameRenumberStart = AxonGUI.FieldIntInline(r, "Starting At", r.ForceFrameRenumberStart);
            }
            AxonGUI.EndHorizontal();
        }

    }

}//AxonGenesis

#endif
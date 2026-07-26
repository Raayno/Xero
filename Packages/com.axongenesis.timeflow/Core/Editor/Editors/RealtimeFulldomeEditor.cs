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
    [CustomEditor(typeof(RealtimeFulldome))]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-to-disk")]
    public class RealtimeFulldomeEditor : AxonGenesisEditor<RealtimeFulldome, RealtimeFulldomeEdit> { }

    sealed public class RealtimeFulldomeEdit : AxonGenesisBehaviorEdit<RealtimeFulldome>
    {
#if TIMEFLOW_PRO
        public const string kAddRealtimeFulldomeCamera = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🌍 Realtime Fulldome Camera";
#else
        public const string kAddRealtimeFulldomeCamera = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Realtime Fulldome Camera";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Realtime Fulldome Camera";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddRealtimeFulldomeCamera, false, 222)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddRealtimeFulldomeCamera, false, 222)]
        public static void AddRealtimeFulldome()
        {
            ObjectUtil.GetOrAddComponent<RealtimeFulldome>(TimeflowMenu.GetSelectedOrNewGameObject("Realtime Fulldome"));
        }

        public override void OnEnable()
        {
            base.OnEnable();
            target.UpdateGameView();
        }

        public override void OnInspectorGUI()
        {
            CameraGUI();

            if (GUI.changed) {
                target.UpdateGameView();
                EditorUtil.SetDirty(target);
            }
        }

        public override void GUIMenu()
        {
            AxonGUI.UndoName = "Set Auto Game View Size";
            AxonGUI.SetTooltip("Enable this setting to ensure that the game view size matches the size of the cubemap assigned.");
            target.AutoGameViewSize = AxonGUI.FieldToggleInline(target, "Auto Game View Size", target.AutoGameViewSize);
        }

        private void CameraGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Camera";
            target.Camera = (Camera)AxonGUI.FieldObject(target, "Camera", target.Camera, typeof(Camera), true, false);

            AxonGUI.UndoName = "Set Camera Cubemap";
            target.Cubemap = (RenderTexture)AxonGUI.FieldObjectInline(target, "Cubemap", target.Cubemap, typeof(RenderTexture), true, false, GUILayout.Width(160));
            AxonGUI.EndHorizontal();

            if (target.Cubemap == null) {
                AxonGUI.HelpBox("Please assign a cubemap render texture. Floating point color format is recommended (R32G32B32A32_SFLOAT), otherwise use ARGB32", MessageType.Warning);
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Dome Orientation";
            target.DomeOrientation = (RenderToDisk.DomeOrientations)AxonGUI.FieldEnumPopup(target, "Orientation", target.DomeOrientation);

            AxonGUI.UndoName = "Set Cubemap Faces";
            target.CubemapFace = (RenderToDisk.CubemapFaces)AxonGUI.FieldEnumPopup(target, "Cubemap Faces", (RenderToDisk.CubemapFaces)target.CubemapFace, true);

            AxonGUI.UndoName = "Set Horizon";
            target.DomeHorizon = AxonGUI.FieldSlider(target, "Horizon", target.DomeHorizon, 90f, 360f);

            AxonGUI.UndoName = "Set Tilt";
            target.DomeTilt = AxonGUI.FieldSlider(target, "Tilt", target.DomeTilt, 0f, 360f);

            AxonGUI.UndoName = "Set Masked";
            target.DomeMasked = AxonGUI.FieldToggle(target, "Masked", target.DomeMasked);
            if (target.DomeMasked) {
                AxonGUI.UndoName = "Set Mask Roundness";
                target.MaskRoundness = AxonGUI.FieldSlider(target, "Mask Roundness", target.MaskRoundness, 0f, 1f);
                AxonGUI.UndoName = "Set Mask Softness";
                target.MaskSoftness = AxonGUI.FieldSlider(target, "Mask Softness", target.MaskSoftness, 0f, 1f);
            }
            AxonGUI.EndBox();

            if (GUI.changed) {
                target.Setup();
            }
        }

    }

}//AxonGenesis

#endif
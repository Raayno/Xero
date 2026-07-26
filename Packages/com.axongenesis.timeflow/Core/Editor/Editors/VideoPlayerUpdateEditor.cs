// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Video;

namespace AxonGenesis
{
    [CustomEditor(typeof(VideoPlayerUpdate))]
    public class VideoPlayerUpdateEditor : AxonGenesisEditor<VideoPlayerUpdate, VideoPlayerUpdateEdit> { }

    sealed public class VideoPlayerUpdateEdit : AxonGenesisBehaviorEdit<VideoPlayerUpdate>
    {
#if TIMEFLOW_PRO
        public const string kAddVideoPlayerUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "📺 Video Player Update";
#else
        public const string kAddVideoPlayerUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Video Player Update";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Video Player Update";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddVideoPlayerUpdate, false, 202)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddVideoPlayerUpdate, false, 202)]
        public static void AddVideoPlayerUpdate()
        {
            ObjectUtil.GetOrAddComponent<VideoPlayerUpdate>(TimeflowMenu.GetSelectedOrNewGameObject("Video Player Update"));
        }

        public bool isBuilding = false;

        public VideoPlayerUpdateEdit() { }

        public VideoPlayerUpdateEdit(VideoPlayerUpdate _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/video-player-update";
        }

        public override void GUIMenu()
        {
            AxonGUI.Info("Please enter play mode for full video playback capability. Also note that due to API limitations, precice frame-by-frame accuracy cannot be guaranteed.");
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginDisabledGroup(true);
            AxonGUI.UndoName = "Set Video Player";
            AxonGUI.SetTooltip("The VideoPlayer component is required and automatically assigned");
            target.VideoPlayer = (VideoPlayer)AxonGUI.FieldObject(target, "Video Player", target.VideoPlayer, typeof(VideoPlayer), true);
            AxonGUI.EndDisabledGroup();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Start Time In Video";
            AxonGUI.SetTooltip("Sets the time in seconds in the video where playback starts. Leave this at 0 to play the video from its beginning.");
            target.StartAtTimeInVideo = AxonGUI.FieldFloat(target, "Start at Time in Video", target.StartAtTimeInVideo);
            if (target.StartAtTimeInVideo < 0) target.StartAtTimeInVideo = 0f;

            if (target.VideoPlayer != null) {
                AxonGUI.UndoName = "Set Loop Enabled";
                AxonGUI.SetTooltip("Loops the video endlessly. If disabled, the video only plays from its start to length.");
                target.VideoPlayer.isLooping = AxonGUI.FieldToggleInline(target, "Loop", target.VideoPlayer.isLooping);
            }
            AxonGUI.EndHorizontal();

            if (GUI.changed) {
                target.EditorUpdate();
            }
        }
    }

}//AxonGenesis 

#endif
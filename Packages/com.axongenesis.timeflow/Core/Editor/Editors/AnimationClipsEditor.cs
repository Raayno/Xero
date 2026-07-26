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

namespace AxonGenesis
{
    [CustomEditor(typeof(AnimationClips))]
    public class AnimationClipsEditor : AxonGenesisEditor<AnimationClips, AnimationClipsEdit> { }

    sealed public class AnimationClipsEdit : AxonGenesisBehaviorEdit<AnimationClips>
    {
#if TIMEFLOW_PRO
        public const string kAddAnimationClips = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "👯 Animation Clips";
#else
        public const string kAddAnimationClips = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Animation Clips";
#endif

        public const string kShortcut = "Timeflow/Add Behavior: Animpation Clips";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAnimationClips, false, 105)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAnimationClips, false, 105)]
        public static void AddAnimationClips()
        {
            ObjectUtil.GetOrAddComponent<AnimationClips>(TimeflowMenu.GetSelectedOrNewGameObject("Animation Clips"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public AnimationClipsEdit() { }
        public AnimationClipsEdit(AnimationClips _target)
        {
            target = _target;
            behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/animation-clips";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (target != null) AnimatorInfo.Init(target.gameObject, false);
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.HelpBox("Please leave the Animator component in place and disabled to allow this component to update the animation behavior.", MessageType.Info);

            behaviorUI.ChannelsGUI(false);
            behaviorUI.MainGUI();

            if (GUI.changed) {
                AnimatorInfo.Init(target.gameObject, true);
                EditorUtility.SetDirty(target);
            }
        }
    }

}//AxonGenesis

#endif
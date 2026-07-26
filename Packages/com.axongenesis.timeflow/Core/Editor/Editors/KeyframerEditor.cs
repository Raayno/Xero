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
    [CustomEditor(typeof(Keyframer))]
    public class KeyframerEditor : AxonGenesisEditor<Keyframer, KeyframerEdit> { }

    public class KeyframerSharedEdit
    {
        public Editor editor;
        public Keyframer target;
        public TimeflowBehaviorSharedEdit behaviorUI;

        public bool IsVisible = true;

        public KeyframerSharedEdit()
        {
        }

        public KeyframerSharedEdit(Keyframer _target, Editor ed)
        {
            target = (Keyframer)_target;
            editor = ed;
            behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public void GUIMenu()
        {
            if (target == null) return;
        }

        public void MainGUI()
        {
            if (target == null) return;

            behaviorUI.ChannelsGUI(true);
            behaviorUI.MainGUI();

            if (GUI.changed) {
                EditorUtil.SetDirty(target);
            }
        }

        public void OnSceneGUI()
        {
            if (target != null) {
                target.OnDrawGizmos();
            }
        }
    }

    sealed public class KeyframerEdit : AxonGenesisBehaviorEdit<Keyframer>
    {
#if TIMEFLOW_PRO
        public const string kAddKeyframer = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "◈ Keyframer";
#else
        public const string kAddKeyframer = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Keyframer";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Keyframer";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddKeyframer, false, 100)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddKeyframer, false, 100)]
        public static void AddKeyframer()
        {
            ObjectUtil.GetOrAddComponent<Keyframer>(TimeflowMenu.GetSelectedOrNewGameObject("Keyframer"));
        }

        public KeyframerSharedEdit ui;

        public KeyframerEdit() { }

        public KeyframerEdit(Keyframer _target)
        {
            target = _target;
            ui = new KeyframerSharedEdit(_target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/keyframer";
        }

        public override void GUISetup()
        {
            base.GUISetup();

            if (ui == null) ui = new KeyframerSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            ui.GUIMenu();
        }

        public override void OnInspectorGUI()
        {
            if (target.Enabled) {
                ui.MainGUI();
            }
        }

        public override void OnSceneGUI()
        {
            if (ui != null) ui.OnSceneGUI();
        }

    }

}//AxonGenesis

#endif
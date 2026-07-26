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
    [CustomEditor(typeof(ParticleSystemUpdate))]
    public class ParticleSystemUpdateEditor : AxonGenesisEditor<ParticleSystemUpdate, ParticleSystemUpdateEdit> { }

    sealed public class ParticleSystemUpdateEdit : AxonGenesisBehaviorEdit<ParticleSystemUpdate>
    {
#if TIMEFLOW_PRO
        public const string kAddParticleSystemUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "✨ Particle System Update";
#else
        public const string kAddParticleSystemUpdate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Particle System Update";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Particle System Update";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddParticleSystemUpdate, false, 200)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddParticleSystemUpdate, false, 200)]
        public static void AddParticleSystemUpdate()
        {
            ObjectUtil.GetOrAddComponent<ParticleSystemUpdate>(TimeflowMenu.GetSelectedOrNewGameObject("Particle System Update"));
        }

        public bool isBuilding = false;

        public ParticleSystemUpdateEdit() { }

        public ParticleSystemUpdateEdit(ParticleSystemUpdate _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/particle-system-update";
        }

        public override void GUIMenu()
        {
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.UndoName = "Set Particle System";
            AxonGUI.SetTooltip("Assign the particle system to update with Timeflow.");
            target.Particles = (ParticleSystem)AxonGUI.FieldObject(target, "Particle System", target.Particles, typeof(ParticleSystem), true);

            AxonGUI.UndoName = "Set Clear On Stop";
            AxonGUI.SetTooltip("If enabled, particles are cleared from view when Timeflow playback is stopped.");
            target.ClearOnStop = AxonGUI.FieldToggle(target, "Clear On Stop", target.ClearOnStop);

            AxonGUI.UndoName = "Set Clear On Rewind";
            AxonGUI.SetTooltip("If enabled, particles are cleared upon rewinding or looping time. Otherwise if off, particles display continously. Turn off for seemless looping.");
            target.ClearOnRewind = AxonGUI.FieldToggle(target, "Clear On Rewind", target.ClearOnRewind);

            if (GUI.changed) {
                target.EditorUpdate();
            }
        }
    }

}//AxonGenesis 

#endif
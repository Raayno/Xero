// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a utility that forces a ParticleSytem to update with Timeflow in edit mode.  At runtime the
    /// particle system is allowed to update normally. This script is not required to use particles with
    /// Timeflow, but is a helpful assistant to ensure proper updating.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [RequireComponent(typeof(ParticleSystem))]
    [AddComponentMenu("Timeflow/Particle System Update")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/particle-system-update")]
    sealed public class ParticleSystemUpdate : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC

        public ParticleSystem Particles;
        public bool ClearOnRewind = true;
        public bool ClearOnStop = true;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private bool isPaused;

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            Particles = GetComponent<ParticleSystem>();
        }

        public override void OnPlay()
        {
            base.OnPlay();
            isPaused = false;
            if (Enabled && Particles != null) Particles.Play();
            //if (DebugEnabled) Debug.Log(name + ".ParticleSystemUpdate.OnPlay");
        }

        public override void OnStop()
        {
            base.OnStop();
            if (Enabled && ClearOnStop && Particles != null && !Application.isPlaying) {
                //if (DebugEnabled) Debug.Log(name + ".ParticleSystemUpdate.OnStop");
                Particles.Stop();
                Particles.Clear(true);
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (Enabled && ClearOnRewind && Particles != null) {
                Particles.Clear(true);
            }
        }

        public void Pause()
        {
            if (Enabled && Particles != null) {
                Particles.Pause();
                isPaused = true;
            }
        }

        public override void UpdateTime()
        {
            if (!CanUpdate) return;
            base.UpdateTime();
            if (!Application.isPlaying && Particles != null && !isPaused) {
                float d = CurrentTime - lastTime;
                if (d > 0f) {
                    //if (DebugEnabled) Debug.Log(name + ".ParticleSystemUpdate.UpdateTime:" + d);
                    Particles.Simulate(d, true, false);
                }
                lastTime = CurrentTime;
            }
        }

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.ParticleSystemUpdate;

        /// <summary>
        /// Prevents component reference from being listed in property lists, since there's nothing to
        /// animate here.
        /// </summary>
        public override bool ArePropertiesHidden {
            get {
                return true;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Particle System Update"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<ParticleSystemUpdate>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }

}//AxonGenesis

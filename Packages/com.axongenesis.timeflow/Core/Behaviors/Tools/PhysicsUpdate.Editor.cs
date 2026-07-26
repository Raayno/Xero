// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public partial class PhysicsUpdate : TimeflowBehavior
    {
        public bool EditorShowStates = true;
        public bool EditorShowStateValues = true;

        public override Texture2D Icon => AxonUI.Icons.PhysicsUpdate;

        public override void UpdateTime()
        {
            if (Application.isPlaying || !Timeflow.IsPlaying) return;
            base.UpdateTime();
            Physics.Simulate(Time.fixedDeltaTime);
        }

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

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Physics Update"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<PhysicsUpdate>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

    }
}//AxonGenesis
#endif
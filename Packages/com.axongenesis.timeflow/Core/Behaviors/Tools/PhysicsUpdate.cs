// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{

    [ExecuteInEditMode]
    public partial class PhysicsUpdate : TimeflowBehavior
    {
        public static PhysicsUpdate Instance { get; private set; }

        public bool SaveStatesOnAwake = false;
        public bool RestoreStatesOnEnable = true;
        public bool RestoreStatesOnRewind = true;
        public float RestoreTimeThreshold = 1f;

        public bool HasInitialStates => InitialStates != null && InitialStates.Count > 0;


        [SerializeField]
        public List<RigidbodyState> InitialStates = null;

        protected override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
            if (SaveStatesOnAwake) {
                SaveInitialStates();
            }
            else
            if (InitialStates == null || InitialStates.Count == 0) {
                SaveInitialStates();
            }
        }

        protected override void OnDestruct()
        {
            Instance = null;
            base.OnDestruct();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (RestoreStatesOnEnable) RestoreInitialStates();
#if UNITY_EDITOR
#if UNITY_2022_1_OR_NEWER
            Physics.simulationMode = SimulationMode.Script;
#else
            Physics.autoSimulation = false;
#endif
#endif
        }

        public void SaveInitialStates()
        {
            List<RigidbodyState> copy = new List<RigidbodyState>();
            if (InitialStates == null) {
                InitialStates = new List<RigidbodyState>();
            }
            else {
                foreach (var state in InitialStates) {
                    copy.Add(state);
                }
                InitialStates.Clear();
            }
            if (DebugEnabled) Debug.Log($"{name}.SaveInitialStates");

#if UNITY_2023_1_OR_NEWER
            foreach (var rb in Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None)) {
#else
            foreach (var rb in FindObjectsOfType<Rigidbody>()) {
#endif
                InitialStates.Add(new RigidbodyState {
                    Rigidbody = rb,
                    Position = rb.transform.position,
                    Rotation = rb.transform.rotation,
#if UNITY_6000_0_OR_NEWER
                    Velocity = rb.linearVelocity,
#else
                    Velocity = rb.velocity,
#endif
                    AngularVelocity = rb.angularVelocity
                });

                var previousState = copy.Find(s => s.Rigidbody == rb);
                if (previousState != null) {
                    InitialStates[InitialStates.Count - 1].Enabled = previousState.Enabled;
                    InitialStates[InitialStates.Count - 1].Foldout = previousState.Foldout;
                }
            }

            foreach(var state in InitialStates) {
                if (state.Rigidbody == null) {
                    Debug.Log($"RigidbodyState in {name} has a null Rigidbody reference.");
                }
            }
            if (DebugEnabled) Debug.Log($"{name}.PhysicsUpdate: Saved initial rigidbody states for {InitialStates.Count} transforms");
        }

        public void RestoreInitialStates()
        {
            if (Instance != this) {
                Debug.LogWarning($"Multiple instances of {nameof(PhysicsUpdate)} detected. There should only be one per scene. Destroying the new instance on {name}.");
                EditorUtil.ShowDialog($"Multiple instances of {nameof(PhysicsUpdate)} detected.", $"There should only instance of PhysicsUpdate per scene. Destroying the old instance {Instance.name}.");
                DestroyImmediate(Instance);
            }

            if (InitialStates == null || InitialStates.Count == 0) return;
            if (DebugEnabled) Debug.Log($"{name}.RestoreInitialStates");
            int index = -1;
            int count = 0;
            foreach (var state in InitialStates) {
                index++;
                var rb = state.Rigidbody;
                if (rb == null) {
                    Debug.Log($"RigidbodyState[{index}] in {name} has a null Rigidbody reference.");
                    continue;
                }

                if (!state.Enabled) continue;
                rb.transform.position = state.Position;
                rb.transform.rotation = state.Rotation;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = state.Velocity;
#else
                rb.velocity = state.Velocity;
#endif
                rb.angularVelocity = state.AngularVelocity;
                count++;
            }
            if (DebugEnabled) Debug.Log($"{name}.PhysicsUpdate: Restored initial rigidbody states for {count} of {InitialStates.Count} transforms");
        }

        public void ClearInitialStates()
        {
            if (InitialStates != null) {
                InitialStates.Clear();
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (DebugEnabled) Debug.Log($"{name}.OnRewind");

            if (RestoreStatesOnRewind && RestoreTimeThreshold == 0 || Mathf.Abs(CurrentTime) < RestoreTimeThreshold) {
                RestoreInitialStates();
            }
        }

    }
}//AxonGenesis

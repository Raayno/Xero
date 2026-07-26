// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// A simple controller to demonstrate how Timeflow can be used with player input. This doesn't present
    /// an ideal character setup by any means, but it does demonstrate show how animation can switch
    /// between player input and Timeflow control. As an alternative to starting and stopping Timeflow
    /// playback, one could also enable/disable channels, or use tracks and work areas to choreograph
    /// sequences for cut scenes or other purposes.
    /// </summary>
    public class DemoTestCharacter : MonoBehaviour
    {

        public KeyCode IdleKey = KeyCode.S;
        public KeyCode WalkKey = KeyCode.W;
        public KeyCode Dance1Key = KeyCode.A;
        public KeyCode Dance2Key = KeyCode.D;
        public KeyCode TPoseKey = KeyCode.T;
        public KeyCode PlayTimeflowKey = KeyCode.Space;

        [NonSerialized]
        private Animator animator;

        [NonSerialized]
        private TimeflowObject obj;


        private void Start()
        {
            TryGetComponent<Animator>(out animator);

            /// Method #1 This script simply enables/disables the TimeflowObject to effectively turn the
            /// animation on and off. When the TimeflowObject is disabled, it is no longer processed by
            /// Timeflow and will allow player input to pass through to the animator parameters.
            TryGetComponent<TimeflowObject>(out obj);

            if (obj != null) {
                /// Method #2 This shows how to get a specific channel from a TimeflowObject, which can
                /// then be individually enabled/disabled, instead of applying to the whole TimeflowObject.
                /// Use this approach to be more selective and to still allow other animation channels to
                /// remain playing.
                TimeflowChannel walkChannel = obj.GetChannel("Walk");
                if (walkChannel != null) walkChannel.IsEnabled = true;

                /// Method #3 Or alternatively you could get enable/disable a specific behavior instead.
                /// This effects all animation channels belonging to the behavior component, while leaving
                /// others playing. Use this method if you are using other component types such as Tween
                /// that you don't want to affect.
                if (TryGetComponent<Keyframer>(out var kefyramer)) kefyramer.Enabled = true;

                /// Method #4 Instead of scripting, you can also use choregraphed sequences in Timeflow
                /// that occur at specific times, using track sections or triggered and queued using 
                /// Work Area and Markers.
            }
        }

        private void Update()
        {
            if (animator != null) {
                if (Input.anyKeyDown) {
                    bool walk = false;
                    bool dance1 = false;
                    bool dance2 = false;
                    bool tpose = false;

                    if (Input.GetKeyDown(PlayTimeflowKey)) {
                        /// toggle the active state of the object in Timeflow
                        if (obj != null) {
                            obj.Enabled = !obj.Enabled;
                        }
                    }
                    else {
                        /// Get player input keys pressed if any
                        bool playerInput = false;
                        if (Input.GetKeyDown(IdleKey)) {
                            // all off
                            playerInput = true;
                        }
                        else
                        if (Input.GetKeyDown(WalkKey)) {
                            walk = true;
                            playerInput = true;
                        }
                        else
                        if (Input.GetKeyDown(Dance1Key)) {
                            dance1 = true;
                            playerInput = true;
                        }
                        else
                        if (Input.GetKeyDown(Dance2Key)) {
                            dance2 = true;
                            playerInput = true;
                        }
                        else
                        if (Input.GetKeyDown(TPoseKey)) {
                            tpose = true;
                            playerInput = true;
                        }
                        if (playerInput) {
                            /// stop playback of the animation to allow player input
                            if (obj != null) {
                                obj.Enabled = false;
                            }
                        }
                    }

                    /// Apply input to animator - defaults to idle if any other key was pressed
                    animator.SetFloat("Walk", walk ? 1 : 0);
                    animator.SetBool("Dance1", dance1);
                    animator.SetBool("Dance2", dance2);
                    animator.SetBool("T-Pose", tpose);
                }
            }
        }
    }

}//AxonGenesis

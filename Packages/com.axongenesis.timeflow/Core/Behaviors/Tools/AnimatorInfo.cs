// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a utility class that gathers and stores information about Animator states to be used at
    /// runtime. Since the scripting API for Animator is lacking the ability to get certain information
    /// about states (ex. length) it must be read and stored in edit mode.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/animator-info")]
    sealed public class AnimatorInfo : MonoBehaviour
    {
        [HideInInspector]
        public List<AnimatorData> Data;

        [HideInInspector]
        public string DefaultStateName = "";

        [HideInInspector]
        public int DefaultStateHash;

        /// <summary>
        /// This collects information for all of the animation states on the object. This should only be
        /// invoked in the editor.
        /// </summary>
        public static void Init(GameObject obj, bool refresh)
        {
            Animator anim;
            if (obj.TryGetComponent<Animator>(out anim)) {
                AnimatorInfo info;
                if (!obj.TryGetComponent<AnimatorInfo>(out info)) {
                    ObjectUtil.AddComponent<AnimatorInfo>(obj);
                }
                else
                if (refresh) {
                    info.Gather();
                }
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) {
                Gather();
            }
        }

        /// <summary>
        /// This collects information for all of the animation states on the object. This should only be
        /// invoked in the editor. Once data has been collected, it is stored and available at runtime.
        /// </summary>
        public void Gather()
        {
            if (!Application.isPlaying) {
                Data = new List<AnimatorData>();

#if UNITY_EDITOR
                TryGetComponent<TimeflowObject>(out _object);
                TryGetComponent<AnimationClips>(out _animationClips);
                TryGetComponent<AnimationSequencer>(out _sequencer);

                // Animation clips are updated independently of Animator
                updateAnimator = _object != null && _animationClips == null && _sequencer == null;

                TryGetComponent<Animator>(out _animator);
                if (_animator) {
                    _animator.Update(0);
                }
#endif
            }
        }


        /// <summary>
        /// Returns the name of a specific animation track. The use of a hash improves performance over
        /// using strings.
        /// </summary>
        /// <param name="hash">The hash code provided by Unity as uniqueNameHash.</param>
        /// <returns></returns>
        public string GetName(int hash)
        {
            string name = "";
            if (Data != null) {
                foreach (AnimatorData d in Data) {
                    if (d.Hash == hash) {
                        name = d.Name;
                        break;
                    }
                }
            }
            return name;
        }


        /// <summary>
        /// Returns the length of a specific animation track given its name.
        /// </summary>
        /// <param name="name">The name of the animation track.</param>
        public float GetLength(string name)
        {
            float length = 0f;
            if (Data != null) {
                foreach (AnimatorData d in Data) {
                    if (d.Name == name) {
                        length = d.Length;
                        break;
                    }
                }
            }
            return length;
        }

        /// <summary>
        /// Returns the length of a specific animation track given its unique hash.
        /// </summary>
        /// <param name="hash">The unique hash int provided by Unity for the animation track.</param>
        public float GetLength(int hash)
        {
            float length = 0f;
            if (Data != null) {
                foreach (AnimatorData d in Data) {
                    if (d.Hash == hash) {
                        length = d.Length;
                        break;
                    }
                }
            }
            return length;
        }

        /// <summary>
        /// Returns AnimationClip data for the named animation track.
        /// </summary>
        /// <param name="name">The name of the animation track.</param>
        public AnimationClip GetAnimationClip(string name)
        {
            return GetAnimationClip(Animator.StringToHash(name));
        }


        /// <summary>
        /// Returns AnimationClip data for the animation track identified by the unique hash.
        /// </summary>
        /// <param name="hash">The unique hash int provided by Unity for the animation track.</param>
        public AnimationClip GetAnimationClip(int hash)
        {
            AnimationClip clip = null;
            if (Data != null) {
                foreach (AnimatorData d in Data) {
                    if (d.Hash == hash) {
                        clip = d.Clip;
                        break;
                    }
                }
            }
            return clip;
        }

        /// <summary>
        /// Looks for an alternate name of a track. Returns the alt track name if found, otherwise returns
        /// the original name.
        /// </summary>
        public string GetAltTrackName(string name, string append)
        {
            string altName = name;
            string findName = name + append;
            if (Data != null) {
                foreach (AnimatorData d in Data) {
                    if (d.Name == findName) {
                        altName = findName;
                        break;
                    }
                }
            }
            return altName;
        }


#if UNITY_EDITOR

        [NonSerialized]
        private Animator _animator = null;

        [NonSerialized]
        private AnimationClips _animationClips = null;

        [NonSerialized]
        private AnimationSequencer _sequencer = null;

        [NonSerialized]
        private TimeflowObject _object = null;

        [NonSerialized]
        private bool updateAnimator = false;

        private void Update()
        {
            // Since Animator doesn't update automatically in edit mode, force it if Timeflow is playing
            if (!Application.isPlaying && updateAnimator && _object.Timeflow.IsPlaying) {
                if (_animator) _animator.Update(Time.deltaTime);
            }
        }

#endif
    }

}//AxonGenesis
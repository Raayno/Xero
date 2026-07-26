// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Defines a set of utility functions for working with legacy animations and Animator components.
    /// </summary>
    public static class AnimationUtil
    {
        [SerializeField]
        private static bool _DebugEnabled;

        public static bool DebugEnabled {
            get {
                return _DebugEnabled && TimeflowPreferences.DebugEnabled;
            }
            set {
                _DebugEnabled = value;
            }
        }

        /// <summary>
        /// Determines whether the Animator component is being used, rather than the legacy Animation
        /// component
        /// </summary>
        public static bool HasAnimation(GameObject obj)
        {
            bool hasAnimation = false;
            if (obj != null) {
                if (obj.TryGetComponent<Animation>(out var animation)) {
                    hasAnimation = true;
                }
                else
                if (obj.TryGetComponent<Animator>(out var animator)) {
                    hasAnimation = true;
                }
            }
            //if (DebugEnabled) Debug.Log("AnimationUtil.HasAnimation(" + obj.name + "):" + hasAnimation);
            return hasAnimation;
        }

        /// <summary>
        /// Returns a list of animation clips on the specified object
        /// </summary>
        public static List<AnimationClip> GetAnimationClips(GameObject obj)
        {
            List<AnimationClip> clips = null;

            if (obj.TryGetComponent<Animator>(out var animator)) {
                if (animator.runtimeAnimatorController == null) {
                    //Debug.Log(obj.name + ": does not have an Animator Controller assigned.");
                }
                else {
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
                        if (clips == null) clips = new List<AnimationClip>();
                        clips.Add(clip);
                    }
                }
            }
            else {
                if (obj.TryGetComponent<Animation>(out var anim)) {
                    if (clips == null) clips = new List<AnimationClip>();
                    clips.Add(anim.clip);
                }
            }
            return clips;
        }

        public static void SamplePose(Animator animator, AnimationClip clip, float normalizedTime)
        {
            if (animator == null || clip == null)
                return;

            // SampleAnimation sets transforms of all animated bones relative to the GameObject
            clip.SampleAnimation(animator.gameObject, clip.length * normalizedTime);

            // Immediately update Animator with current transforms
            animator.Update(0f);
        }

        public static bool TryGetAnimatorControllerPlayable(Animator animator, out AnimatorControllerPlayable controllerPlayable)
        {
            controllerPlayable = default;

            var graph = animator.playableGraph;
            if (!graph.IsValid())
                return false;

            int outputCount = graph.GetOutputCount();
            for (int i = 0; i < outputCount; i++) {
                var output = graph.GetOutput(i);

                // Only care about animation outputs
                if (output.GetPlayableOutputType() != typeof(AnimationPlayableOutput))
                    continue;

                var animOutput = (AnimationPlayableOutput)output;

                // Only outputs targeting our animator
                if (animOutput.GetTarget() != animator)
                    continue;

                // Get the root playable driving this output
                Playable rootPlayable = animOutput.GetSourcePlayable();
                if (!rootPlayable.IsValid())
                    continue;

                // Recursively search the playable tree for the AnimatorControllerPlayable
                if (FindControllerPlayableRecursive(rootPlayable, out controllerPlayable))
                    return true;
            }

            return false;
        }

        private static bool FindControllerPlayableRecursive(Playable playable, out AnimatorControllerPlayable controller)
        {
            controller = default;

            if (!playable.IsValid())
                return false;

            if (playable.IsPlayableOfType<AnimatorControllerPlayable>()) {
                controller = (AnimatorControllerPlayable)playable;
                return true;
            }

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++) {
                var child = playable.GetInput(i);
                if (FindControllerPlayableRecursive(child, out controller))
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Tries to open/focus the Animation window and load the given clip into it,
        /// handling multiple Unity versions / internal API changes.
        /// </summary>
        public static bool OpenAnimationClipInEditor(AnimationClip clip)
        {
            //Debug.Log($"<color=cyan>[AnimationUtil]</color> OpenAnimationClipInEditor: {clip.name}");

            // Try to open it in the Animation Window using reflection
            Type animWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            EditorWindow animWindow = EditorWindow.GetWindow(animWindowType);

            var method = animWindowType.GetMethod("OpenAnimationClip", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                method.Invoke(animWindow, new object[] { clip });
                return true;
            }

            // 1) Open/focus animation window (menu path may differ in very old versions, but works 2019+)
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");

            // 2) Get AnimationWindow (internal)
            var editorAsm = typeof(EditorWindow).Assembly;
            if (animWindowType == null) {
                Debug.LogError("Could not find UnityEditor.AnimationWindow type.");
                return false;
            }

            var window = EditorWindow.GetWindow(animWindowType);
            if (window == null) {
                Debug.LogError("Could not open Animation Window.");
                return false;
            }

            // 3) Get the internal AnimEditor and state
            var animEditorField = animWindowType.GetField("m_AnimEditor", BindingFlags.Instance | BindingFlags.NonPublic);
            if (animEditorField == null) {
                Debug.LogError("AnimationWindow.m_AnimEditor field not found.");
                return false;
            }

            var animEditor = animEditorField.GetValue(window);
            if (animEditor == null) {
                Debug.LogError("AnimationWindow.m_AnimEditor is null.");
                return false;
            }

            var animEditorType = animEditor.GetType();
            var stateProp = animEditorType.GetProperty("state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (stateProp == null) {
                Debug.LogError("AnimEditor.state property not found.");
                return false;
            }

            var state = stateProp.GetValue(animEditor);
            if (state == null) {
                Debug.LogError("AnimEditor.state is null.");
                return false;
            }

            var stateType = state.GetType();

            // 4) Try multiple known signatures (Unity versions differ)
            //    We’ll try in this order:
            //    - SetAnimationClip(AnimationClip, UnityEngine.Object)              // some 2019–2021
            //    - SetAnimationClip(AnimationClip, bool)                            // some versions
            //    - SetAnimationClip(AnimationClip)                                  // some versions
            //    - SetCurrentAnimationClip(AnimationClip)                           // rare
            //    - Properties: activeAnimationClip / currentAnimationClip / previewAnimationClip
            //    Each attempt logs; if one works, we return true.

            bool TryInvoke(string methodName, params object[] args)
            {
                var m = stateType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args.Select(a => a?.GetType() ?? typeof(object)).ToArray(), null);
                if (m == null) return false;
                try {
                    m.Invoke(state, args);
                    //Debug.Log($"AnimationWindowState.{methodName} invoked with ({string.Join(", ", args.Select(a => a?.GetType().Name ?? "null"))}).");
                    return true;
                }
                catch (TargetParameterCountException) { } // signature mismatch
                catch (Exception e) {
                    Debug.LogWarning($"Invoking {methodName} failed: {e.GetType().Name} - {e.Message}");
                }
                return false;
            }

            bool TryMethodOverloads_SetAnimationClip()
            {
                // (AnimationClip, UnityEngine.Object)
                var unityObjectType = typeof(UnityEngine.Object);
                var m = stateType.GetMethod("SetAnimationClip", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (m != null) {
                    var pars = m.GetParameters().Select(p => p.ParameterType).ToArray();
                    // Try common overloads.
                    if (pars.Length == 2 && pars[0] == typeof(AnimationClip) && unityObjectType.IsAssignableFrom(pars[1])) {
                        return TryInvoke("SetAnimationClip", clip, null);
                    }
                }

                // Try (AnimationClip, bool)
                var m2 = stateType.GetMethod("SetAnimationClip", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(AnimationClip), typeof(bool) }, null);
                if (m2 != null && TryInvoke("SetAnimationClip", clip, true)) return true;

                // Try (AnimationClip)
                var m3 = stateType.GetMethod("SetAnimationClip", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(AnimationClip) }, null);
                if (m3 != null && TryInvoke("SetAnimationClip", clip)) return true;

                return false;
            }

            bool TryAlternativeMethods()
            {
                if (TryInvoke("SetCurrentAnimationClip", clip)) return true;
                return false;
            }

            bool TrySetProperty(string propName)
            {
                var p = stateType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p == null || !p.CanWrite) return false;
                try {
                    p.SetValue(state, clip);
                    return true;
                }
                catch (Exception e) {
                    Debug.LogWarning($"Setting property {propName} failed: {e.Message}");
                    return false;
                }
            }

            // Attempt sequence
            if (TryMethodOverloads_SetAnimationClip()) {
                return true;
            }
            if (TryAlternativeMethods()) {
                return true;
            }
            if (TrySetProperty("currentAnimationClip")) {
                return true;
            }
            if (TrySetProperty("previewAnimationClip")) {
                return true;
            }
            if (TrySetProperty("activeAnimationClip")) {
                return true;
            }

            // As a last nudge: focus & select the asset so the window may pick it up
            Selection.activeObject = clip;
            window.Repaint();
            Debug.LogWarning("No compatible internal API found; selected the clip and repainted the window instead.");

            // For troubleshooting: dump candidate members to the console.
            DumpStateMembers(stateType);

            return false;
        }

        private static void DumpStateMembers(Type stateType)
        {
            var methods = stateType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.Name.Contains("Clip", StringComparison.OrdinalIgnoreCase) ||
                            m.Name.Contains("Selection", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Debug.Log($"[AnimationWindowState] methods mentioning 'Clip' or 'Selection':\n" + //KEEP
                      string.Join("\n", methods.Select(m =>
                          $" - {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})")));
            var props = stateType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.Name.Contains("Clip", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Debug.Log($"[AnimationWindowState] properties mentioning 'Clip':\n" + //KEEP
                      string.Join("\n", props.Select(p => $" - {p.Name} (canWrite={p.CanWrite})")));
        }

#endif

#if AXON_DEVELOPMENT

        /// <summary>
        /// Determines whether the Animator component is being used, rather than the legacy Animation
        /// component
        /// </summary>
        public static bool IsUsingAnimator(GameObject obj)
        {
            bool isAnimator = false;
            if (obj != null) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    isAnimator = true;
                }
            }
            //if (DebugEnabled) Debug.Log("AnimationUtil.IsUsingAnimator(" + obj.name + "):" + isAnimator);
            return isAnimator;
        }

        /// <summary>
        /// Finds and returns the name of the animation track matching the nameHash value
        /// </summary>
        public static string GetNameFromHash(GameObject obj, int nameHash)
        {
            string name = "";
            if (obj != null) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        name = info.GetName(nameHash);
                    }
                }
            }
            //if (DebugEnabled) Debug.Log("AnimationUtil.GetNameFromHash(" + obj.name + ":" + nameHash + "):" + name);
            return name;
        }

        /// <summary>
        /// Determines whether any animation track is playing on the object
        /// </summary>
        public static bool IsPlaying(GameObject obj)
        {
            bool isPlaying = false;
            if (obj != null) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    for (int x = 0; x < anim.layerCount; x++) {
                        AnimatorClipInfo[] infos = anim.GetCurrentAnimatorClipInfo(x);
                        if (infos.Length > 0) {
                            isPlaying = true;
                            break;
                        }
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null) {
                    isPlaying = obj.GetComponent<Animation>().isPlaying;
                }
            }
            return isPlaying;
        }

        /// <summary>
        /// Determines whether the specified animation track is currently playing on the object
        /// </summary>
        public static bool IsPlaying(GameObject obj, string track)
        {
            bool isPlaying = false;
            if (obj && !string.IsNullOrEmpty(track)) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    for (int x = 0; x < anim.layerCount; x++) {
                        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(x);
                        if (info.IsName(track)) {
                            isPlaying = true;
                            break;
                        }
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[track] != null) {
                    isPlaying = obj.GetComponent<Animation>().IsPlaying(track);
                }
            }
            return isPlaying;
        }

        /// <summary>
        /// Returns the length in seconds of the animation.
        /// </summary>
        public static float GetLength(GameObject obj, string track)
        {
            float length = 0f;
            if (obj && !string.IsNullOrEmpty(track)) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    bool found = false;
                    for (int x = 0; x < anim.layerCount; x++) {
                        AnimatorClipInfo[] infos = anim.GetCurrentAnimatorClipInfo(x);
                        for (int i = 0; i < infos.Length; i++) {
                            if (infos[i].clip.name == track) {
                                length = infos[i].clip.length;
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[track] != null) {
                    length = obj.GetComponent<Animation>()[track].length;
                }
            }
            return length;
        }

        /// <summary>
        /// Returns the current time of the animation (in seconds relative to the animation start).
        /// </summary>
        public static float GetTime(GameObject obj, string track)
        {
            float time = 0f;
            if (obj && !string.IsNullOrEmpty(track)) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    for (int x = 0; x < anim.layerCount; x++) {
                        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(x);
                        if (info.IsName(track)) {
                            time = info.normalizedTime * info.length;
                            break;
                        }
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[track] != null) {
                    time = obj.GetComponent<Animation>()[track].time;
                }
            }
            return time;
        }

        /// <summary>
        /// Returns the speed of the specified animation track
        /// </summary>
        public static float GetSpeed(GameObject obj, string track)
        {
            float speed = 0f;
            if (obj && !string.IsNullOrEmpty(track)) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    speed = anim.speed;
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[track] != null) {
                    speed = obj.GetComponent<Animation>()[track].speed;
                }
            }
            return speed;
        }

        public static float Play(GameObject obj, string track, float time)
        {
            float length = 0f;
            if (obj && !string.IsNullOrEmpty(track)) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Play(" + obj.name + ":" + track + "):" + time);

                Animator animator = obj.GetComponent<Animator>();
                Animation animation = obj.GetComponent<Animation>();
                if (animator != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        length = info.GetLength(track);
                    }
                    float nTime = 0f;
                    if (length != 0f) nTime = time / length;

                    animator.Play(track, -1, nTime);
                }
                else
                if (animation != null && animation[track] != null) {
                    animation.Play(track);
                    animation[track].time = time;
                    length = animation[track].clip.length;
                    //if (DebugEnabled) Debug.Log("AnimationUtil.Play(" + obj.name + ":" + track + "):" + time + ":" + length);
                }
                else {
                    Debug.LogWarning("AnimationUtil.Play(" + obj.name + ":" + track + "): Missing Animation or Animator component");
                }
            }
            return length - time;
        }

        /// <summary>
        /// Sets the speed of the specified animation track
        /// </summary>
        public static void SetSpeed(GameObject obj, string track, float speed)
        {
            if (obj && !string.IsNullOrEmpty(track)) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.SetSpeed(" + obj.name + ":" + track + "):" + speed);

                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    anim.speed = speed;
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[track] != null) {
                    obj.GetComponent<Animation>()[track].speed = speed;
                }
            }
        }

        /// <summary>
        /// Returns the name of the default animation track
        /// </summary>
        public static string GetDefaultTrackName(GameObject obj)
        {
            string name = "";
            if (obj != null) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        name = info.DefaultStateName;
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>().clip != null) {
                    name = obj.GetComponent<Animation>().clip.name;
                }
            }
            //if(DebugEnabled) Debug.Log("AnimationUtil.GetDefaultTrackName("+obj.name+"):"+name);
            return name;
        }

        /// <summary>
        /// This looks for an animation track named with the appended string. This is used to dynamically
        /// switch animations for specific devices.
        /// </summary>
        public static string GetAltTrackName(GameObject obj, string track, string suffix)
        {
            string name = track;
            if (obj && !string.IsNullOrEmpty(track)) {
                string findName = track + suffix;
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        name = info.GetAltTrackName(track, suffix);
                    }
                }
                else
                if (obj.GetComponent<Animation>() != null && obj.GetComponent<Animation>()[findName] != null) {
                    name = findName;
                }
            }
            //if(DebugEnabled) Debug.Log("AnimationUtil.GetAltTrackName("+obj.name+":"+track+":"+suffix+"):"+name);
            return name;
        }

        /// <summary>
        /// Plays an animation on an object, doing all the checks to be sure the animation can be played.
        /// </summary>
        public static float Play(GameObject obj, string track)
        {
            return Play(obj, track, 0f);
        }

        /// <summary>
        /// Gets the animation clip matching the provided track name
        /// </summary>
        public static AnimationClip GetAnimationClip(GameObject obj, string track)
        {
            //if (DebugEnabled) Debug.Log("AnimationUtil.GetAnimationClip:" + track);
            AnimationClip clip = null;
            string name = track;

            if (obj && !string.IsNullOrEmpty(track)) {
                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        clip = info.GetAnimationClip(track);
                        if (clip != null) name = clip.name;
                    }
                }
                else {
                    List<AnimationClip> clips = GetAnimationClips(obj);
                    if (clips == null) {
                        //if (DebugEnabled) Debug.Log("AnimationUtil.GetAnimationClip: " + obj.name + ": no animation clips");
                    }
                    else {
                        foreach (AnimationClip c in clips) {
                            if (c != null && c.name == track) {
                                clip = c;
                                break;
                            }
                        }
                    }
                }
            }
            //if (DebugEnabled) Debug.Log("AnimationUtil.GetAnimationClip: " + obj.name + ":" + name);
            return clip;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Plays an animation on an object using the track name hash value.
        /// </summary>
        public static float Play(GameObject obj, int trackHash, float time)
        {
            float length = 0f;
            if (obj) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Play(" + obj.name + ":" + trackHash + "):" + time);

                Animator anim = obj.GetComponent<Animator>();
                if (anim != null) {
                    AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                    if (info != null) {
                        length = info.GetLength(trackHash);
                    }
                    float nTime = 0f;
                    if (length != 0f) nTime = time / length;

                    anim.Play(trackHash, -1, nTime);
                }
            }
            return length - time;
        }

        /// <summary>
        /// Pauses an animation. A temporary value is stored to keep track of the pause time to be resumed.
        /// </summary>
        public static float Pause(GameObject obj, string track)
        {
            float time = 0f;
            float speed = 1f;
            if (obj != null) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Pause(" + obj.name + ":" + track + ")");

                time = GetTime(obj, track);
                speed = GetSpeed(obj, track);

                if (obj.GetComponent<Animation>() != null) {
                    obj.GetComponent<Animation>().Stop(track);
                }
                else {
                    // Animator can't really be paused, so set the speed to 0
                    SetSpeed(obj, track, 0f);
                }
            }
            PlayerPrefs.SetFloat(track + "ResumeTime", time);
            PlayerPrefs.SetFloat(track + "ResumeSpeed", speed);

            return time;
        }

        /// <summary>
        /// Resumes a previously paused animation.
        /// </summary>
        public static float Resume(GameObject obj, string track)
        {
            float time = PlayerPrefs.GetFloat(track + "ResumeTime", 0f);
            float speed = PlayerPrefs.GetFloat(track + "ResumeSpeed", 1f);

            if (obj != null) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Resume(" + obj.name + ":" + track + "):" + time);
                time -= Play(obj, track, time);
                SetSpeed(obj, track, speed);
            }
            return time;
        }

        /// <summary>
        /// Stops all animation on the object
        /// </summary>
        public static void Stop(GameObject obj)
        {
            if (obj != null) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Stop(" + obj.name + ")");
                if (obj.GetComponent<Animation>() != null) {
                    obj.GetComponent<Animation>().Stop();
                }
                else {
                    // Animator doesn't have a Stop function, so instead we'll return to the default state
                    Animator anim = obj.GetComponent<Animator>();
                    if (anim != null) {
                        AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                        if (info != null) {
                            anim.Play(info.DefaultStateHash, -1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stops all animation on the object
        /// </summary>
        public static void Stop(GameObject obj, string track)
        {
            if (obj != null) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.Stop(" + obj.name + ")");
                if (obj.GetComponent<Animation>() != null) {
                    obj.GetComponent<Animation>().Stop(track);
                }
                else {
                    // Animator doesn't have a Stop function, so instead we'll return to the default state
                    Animator anim = obj.GetComponent<Animator>();
                    if (anim != null) {
                        AnimatorInfo info = obj.GetComponent<AnimatorInfo>();
                        if (info != null) {
                            anim.Play(info.DefaultStateHash, -1);
                        }
                    }
                }
            }
        }

        public static void ResetAnimationClipPosition(AnimationClip clip, string path, string channel)
        {
            if (clip == null) {
                Debug.LogWarning("AnimationUtil.ResetAnimationClipPosition: clip is null");
                return;
            }
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (EditorCurveBinding binding in bindings) {
                //Debug.Log("binding [" + binding.path + "] " + binding.propertyName);
                string propX = channel + ".x";
                string propZ = channel + ".z";
                if (path == binding.path && (binding.propertyName == propX || binding.propertyName == propZ)) {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve.keys != null && curve.keys.Length != 0) {
                        //Debug.Log("UPDATED:" + path + ":" + binding.propertyName);
                        AnimationCurve newCurve = new AnimationCurve();
                        bool isFirst = true;
                        float offset = 0f;
                        for (int k = 0; k < curve.keys.Length; k++) {
                            if (isFirst) {
                                isFirst = false;
                                offset = -curve.keys[k].value;
                                //Debug.Log("offset:" + offset);
                            }
                            UnityEngine.Keyframe key = curve.keys[k];
                            key.value = curve.keys[k].value + offset;
                            newCurve.AddKey(key);
                            //Debug.Log(k + " value:" + key.value + " kv:" + curve.keys[k].value);
                        }

                        AnimationUtility.SetEditorCurve(clip, binding, newCurve);
                        EditorUtil.SetDirty(clip);
                    }
                }
            }
        }

#endif

        /// <summary>
        /// This is used in the editor to move an object's transform to the coordinates at the specified
        /// time of the animation clip.
        /// </summary>
        public static void GotoTransformAtTime(Transform xform, string clipName, float time)
        {
            GotoTransformAtTime(xform, GetAnimationClip(xform.gameObject, clipName), time);
        }
        public static void GotoTransformAtTime(Transform xform, AnimationClip clip, float time)
        {
#if UNITY_EDITOR
            if (clip == null) {
                //if (DebugEnabled) Debug.Log("AnimationUtil.GotoTransformAtTime(" + xform.name + ") clip is null");
            }
            else {
                //if (DebugEnabled) Debug.Log("AnimationUtil.GotoTransformAtTime(" + xform.name + ") clip:" + clip.name + " time:" + time);
                float keyA = 0;
                float keyB = 0;

                float keyAtime = 0.0f;
                float keyBtime = 0.0f;

                EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                //AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip, true);
                //foreach(AnimationClipCurveData curve in curves) {
                foreach (EditorCurveBinding binding in bindings) {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

                    //Debug.Log("Animation:"+clip.name+" prop:"+curve.propertyName+" type:"+curve.type);
                    if (curve.keys != null && curve.keys.Length != 0) {
                        bool hasMin = false;
                        bool hasMax = false;

                        foreach (UnityEngine.Keyframe key in curve.keys) {
                            if (key.time <= time) {
                                //Debug.Log("KeyA:"+key.time+"="+key.value);
                                keyA = key.value;
                                keyAtime = key.time;
                                hasMin = true;
                            }
                            else {
                                //Debug.Log("KeyB:"+key.time+"="+key.value);
                                keyB = key.value;
                                keyBtime = key.time;
                                hasMax = true;
                                break;
                            }
                        }

                        float dif = (keyBtime - keyAtime);
                        if (dif != 0.0f) {
                            dif = (time - keyAtime) / dif;
                        }
                        float value = 0;
                        if (hasMin && hasMax) {
                            value = Interpolate(keyA, keyB, dif);
                        }
                        else
                            if (hasMin) value = keyA;
                        else
                            if (hasMax) value = keyB;

                        if (float.IsNaN(value)) value = 0.0f;

                        //Debug.Log("Interp:"+keyA+" to "+keyB+" by:"+dif+" ="+value);
                        //Debug.Log(curve.propertyName+":"+value);

                        //if(curve.type == typeof(Material)) {
                        //	Material m = curve.target as Material;
                        //	m.SetFloat(curve.propertyName, value);
                        //}
                        //else
                        if (binding.propertyName.IndexOf("LocalPosition") != -1) {
                            Vector3 pos = xform.localPosition;
                            if (binding.propertyName.Equals("m_LocalPosition.x")) {
                                pos.x = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalPosition.y")) {
                                pos.y = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalPosition.z")) {
                                pos.z = value;
                            }
                            //Debug.Log("localPosition:"+pos);
                            xform.localPosition = pos;
                        }
                        else
                        if (binding.propertyName.IndexOf("Scale") != -1) {
                            Vector3 scale = xform.localScale;
                            if (binding.propertyName.Equals("m_LocalScale.x")) {
                                scale.x = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalScale.y")) {
                                scale.y = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalScale.z")) {
                                scale.z = value;
                            }
                            //Debug.Log("localScale:"+scale);
                            xform.localScale = scale;
                        }
                        else
                        if (binding.propertyName.IndexOf("Rotation") != -1) {
                            Quaternion rot = xform.localRotation;
                            if (binding.propertyName.Equals("m_LocalRotation.x")) {
                                rot.x = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalRotation.y")) {
                                rot.y = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalRotation.z")) {
                                rot.z = value;
                            }
                            else
                            if (binding.propertyName.Equals("m_LocalRotation.w")) {
                                rot.w = value;
                            }
                            //Debug.Log("localRotation:"+rot);
                            xform.localRotation = rot;//Quaternion.Euler(rot.x, rot.y, rot.z);
                        }
                    }
                }
            }
#endif
        }

        public static float Interpolate(float a, float b, float amount)
        {
            if (amount == 0.0f) return a;
            else return ((1.0f - amount) * a) + (b * amount);
        }

        public static Vector3 Interpolate(Vector3 a, Vector3 b, float amount)
        {
            Vector3 c = Vector3.zero;
            if (amount == 0.0f) return a;
            c.x = ((1.0f - amount) * a.x) + (b.x * amount);
            c.y = ((1.0f - amount) * a.y) + (b.y * amount);
            c.z = ((1.0f - amount) * a.z) + (b.z * amount);
            return c;
        }

        public static Vector4 Interpolate(Vector4 a, Vector4 b, float amount)
        {
            Vector4 c = Vector4.zero;
            if (amount == 0.0f) return a;
            c.x = ((1.0f - amount) * a.x) + (b.x * amount);
            c.y = ((1.0f - amount) * a.y) + (b.y * amount);
            c.z = ((1.0f - amount) * a.z) + (b.z * amount);
            c.w = ((1.0f - amount) * a.w) + (b.w * amount);
            return c;
        }

#endif
    }

}//AxonGenesis
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// A simple example showing how to use the ITimeflowPlayback interface to add
    /// Timeflow playback events to any script. This can also be used on 
    /// ScriptableObject types and does not have to be a MonoBehaviour.
    /// ---
    /// IMPORTANT: For your script to work in edit mode, be sure to set ExecuteInEditMode
    /// </summary>
    [ExecuteInEditMode]
    public class TimeflowPlayback : TimeflowPlaybackBase
    {
        /// <summary>
        /// Registers an object to receive playback events. If a game object is specified, it will
        /// be used to try to find the Timeflow parent in the hierarchy above. 
        /// </summary>
        /// <param name="playback">Reference to the object that imlements ITimeflowPlayback</param>
        /// <param name="obj">The game object, or may be null</param>
        public static void Register(ITimeflowPlayback playback, GameObject obj = null)
        {
            if (playback.TimeflowParent == null && obj != null) {
                playback.TimeflowParent = ObjectUtil.GetComponentInSelfOrAncestors<Timeflow>(obj);
            }
            if (playback.TimeflowParent == null) playback.TimeflowParent = Timeflow.Active;
            if (playback.TimeflowParent == null) return;
            playback.TimeflowParent.RegisterPlaybackListener(playback);
        }

        /// <summary>
        /// Any script which calls Register must also call Unregster (preferably OnDisable) otherwise
        /// null reference errors may occur if objects are destroyed while Timeflow is playing.
        /// </summary>
        /// <param name="playback"></param>
        public static void Unregister(ITimeflowPlayback playback)
        {
            if (playback == null || playback.TimeflowParent == null) return;
            playback.TimeflowParent.UnregisterPlaybackListener(playback);
        }

        [Tooltip("Assign the Timeflow instance or leave unassigned to automatically work with the first active Timeflow instance in the scene.")]
        [SerializeField]
        private Timeflow _Timeflow = null;

        public override Timeflow TimeflowParent {
            get {
                return _Timeflow;
            }
            set {
                if (_Timeflow == null) _Timeflow = Timeflow.Active;
                _Timeflow = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // This object must be registered to receive event callbacks
            //if (DebugEnabled) Debug.Log($"ITimeflowPlayback.Register:{name}");
            TimeflowPlayback.Register(this, gameObject);
        }

        protected override void OnDisable()
        {
            // Always be sure to unregister objects or null reference errors may occur
            //if (DebugEnabled) Debug.Log($"ITimeflowPlayback.Unregister:{name}");
            TimeflowPlayback.Unregister(this);           
            base.OnDisable();
        }

        public override void OnPlay()
        {
            //if (DebugEnabled) Debug.Log("OnPlay");
        }

        public override void OnLoop()
        {
            //if (DebugEnabled) Debug.Log("OnLoop");
        }

        public override void OnRewind()
        {
            //if (DebugEnabled) Debug.Log("OnRewind");
        }

        public override void OnStop()
        {
            //if (DebugEnabled) Debug.Log("OnStop");
        }

        public override void OnUpdate()
        {
            //if (DebugEnabled) Debug.Log($"OnUpdate:{TimeflowParent.CurrentTime}");
        }
    }
}

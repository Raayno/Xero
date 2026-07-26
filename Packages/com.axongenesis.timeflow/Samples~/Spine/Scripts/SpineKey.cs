// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;

namespace AxonGenesis
{
    /// <summary>
    /// This extends Keyframe with additional information for performing blends.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "SpineKey")]
    public class SpineKey : CustomKey
    {
        private static SpineKey _Default;
        public static readonly Color DefaultTintColor = new Color(1f, 1f, 1f, 0.25f);

        public static SpineKey Default {
            get {
                if (_Default == null) _Default = new SpineKey();
                return _Default;
            }
        }

        #region PUBLIC

        [SerializeField]
        public bool Loop = false;

        [SerializeField]
        public bool FlipX = false;

        [SerializeField]
        public bool FlipY = false;

        [SerializeField]
        public bool AllTracks = false;

        [SerializeField]
        public bool IsEmpty = false;

        [SerializeField]
        public float Duration = 0f;

        [SerializeField]
        public float MixDuration = 0f;

        [SerializeField]
        public Color TintColor = DefaultTintColor;

        [SerializeField]
        public UnityEvent Event;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public SpineAnimator Spine;

        #endregion

        public SpineKey() { }
        
        public SpineKey(SpineKey key)
        {
            Copy(key);
        }

        public string AnimationName => Key == null ? null : Key.KeyString;

        public override void OnValueChanged()
        {
            if(Key == null) return;
            //Debug.Log($"{Key.KeyString} {Key.KeyTime} OnValueChanged");
            if (Key.KeyString == "Empty") {
                Key.KeyString = null;
            }
            IsEmpty = string.IsNullOrEmpty(Key.KeyString);

            Spine.GetKeyDuration(this);
        }

        public static SpineKey CreateCopy(SpineKey from)
        {
            SpineKey copy = new SpineKey();
            copy.Copy(from);
            return copy;
        }

        public override void Copy(CustomKey from)
        {
            SpineKey orig = (SpineKey)from;
            if (orig != null) {
                Key = orig.Key;
                Spine = orig.Spine;
                Loop = orig.Loop;
                FlipX = orig.FlipX;
                FlipY = orig.FlipY;
                AllTracks = orig.AllTracks;
                Event = orig.Event;
                Spine = orig.Spine;
                MixDuration = orig.MixDuration;
                Duration = orig.Duration;
                TintColor = orig.TintColor;
            }
            OnValueChanged();
        }

        public void PerformTrigger()
        {
            if (Event != null) Event.Invoke();
        }
    }

}//AxonGenesis
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using Spine;
using Spine.Unity;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This component sets a Spine bone position controlled by Timeflow
    /// </summary>
    [ExecuteInEditMode]
    public class SpineBonePosition : MonoBehaviour, ITimeflowPlayback
    {
        [Header("Assign either a SkeletonAnimation or SkeletonGraphic")]
        [Tooltip("Assign the SkeletonAnimation. Use this for a regular Spine object.")]
        public SkeletonAnimation SkeletonAnimation = null;

        [Tooltip("Select the bone you want to control the position of.")]
        [Spine.Unity.SpineBone(dataField: "SkeletonAnimation", fallbackToTextField: true)]
        public string BoneName = null;

        [Tooltip("Specifies the SkeletonGraphic component. Use this for canvas rendered elements.")]
        public SkeletonGraphic SkeletonGraphic = null;

        [Tooltip("Select the bone you want to control the position of. This field is only applicable when using a graphic component")]
        [Spine.Unity.SpineBone(dataField: "SkeletonGraphic", fallbackToTextField: true)]
        public string BoneNameGraphic = null;

        public enum Modes
        {
            Off,
            Transform,
            Position
        }
        [Tooltip("Determines how the bone position is set. Either using the specified transform's position, or an explicitly set position value. Or set to Off to disable.")]
        public Modes Mode = Modes.Transform;

        [Tooltip("Specifies the transform object to use as input when Mode is set to Transform. Use this for the bone to follow another game object.")]
        public Transform Transform = null;

        [Tooltip("Sets the position value directly when Mode is set to Position. Use this mode to drive the position using an animation channel, channel link, or other custom method.")]
        public Vector3 Position = Vector3.zero;

        [Tooltip("If enabled, the transform or position is handled as world coordinates. Otherwise local space coordinates are used.")]
        public bool UseWorldCoordinates = false;

        [Tooltip("If enabled, updates are only made when a change is detected in position. Otherwise if off, the bone position is updated every frame regardless.")]
        public bool OnChangeOnly = true;

        private Bone bone = null;
        private Vector3 lastPosition = Vector3.zero;

        public bool HasSkeletonAnimation {get; private set;}

        public bool HasSkeletonGraphic {get; private set;}

        public bool HasBone {get; private set;}

        public Timeflow TimeflowParent { get; set; }

        private void OnValidate()
        {
            Setup();
        }

        private void OnEnable()
        {
            TimeflowPlayback.Register(this, gameObject);
            Setup();
        }

        private void OnDisable()
        {
            TimeflowPlayback.Unregister(this);
        }

        private void Setup()
        {
            if (Transform == null) Transform = transform; // Use this game object by default

            HasSkeletonAnimation = SkeletonAnimation != null;
            HasSkeletonGraphic = SkeletonGraphic != null;

            //if (SkeletonAnimation == null) SpineObject.TryGetComponent<ISkeletonAnimation>(out SkeletonAnimation);
            if (HasSkeletonAnimation) bone = SkeletonAnimation.Skeleton.FindBone(BoneName);
            else
            if (HasSkeletonGraphic) bone = SkeletonGraphic.Skeleton.FindBone(BoneName);

            HasBone = bone != null && (HasSkeletonAnimation || HasSkeletonGraphic);
        }

        public void OnPlay()
        {
            //Debug.Log($"{name}.OnPlay");
            Setup();
        }

        public void OnStop()
        {
            //Debug.Log($"{name}.OnStop");
        }

        public void OnRewind()
        {
            //Debug.Log($"{name}.OnRewind");
        }

        public void OnLoop()
        {
            //Debug.Log($"{name}.OnLoop");
        }

        public void OnUpdate()
        {
            if (!HasBone || Mode == Modes.Off) return;

            Vector3 pos;

            if (Mode == Modes.Transform) {
                pos = UseWorldCoordinates ? Transform.transform.position : Transform.transform.localPosition;
            }
            else {
                pos = Position;
            }
            if (OnChangeOnly && lastPosition == pos) return;

            //Debug.Log($"{name}.OnUpdate.Position:{pos}");
            bone.SetLocalPosition(UseWorldCoordinates ? SkeletonAnimation.transform.InverseTransformPoint(pos) : pos);
            bone.UpdateAppliedTransform();
            bone.Update(Skeleton.Physics.Update);

            if (OnChangeOnly) lastPosition = pos;
        }
    }
}
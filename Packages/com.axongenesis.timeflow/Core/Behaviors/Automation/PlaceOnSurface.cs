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
    /// Apply this to a game object to make it adhere to the terrain, maintaining a specific height. This
    /// will work with all terrains in the scene, or revert to world space if no terrain is found. Note
    /// that this does not work with physics so should not be used with Rigidbody.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [DisallowMultipleComponent]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Place On Surface")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/place-on-surface")]
    sealed public class PlaceOnSurface : TimeflowDataBehavior
    {
        #region STATIC

        public static void ProcessAll()
        {
            PlaceOnSurface[] instances = UnityEngine.Object.FindObjectsByType(typeof(PlaceOnSurface), FindObjectsInactive.Include, FindObjectsSortMode.None) as PlaceOnSurface[];
            if (instances != null) {
                foreach (PlaceOnSurface item in instances) {
                    item.Process(item.CurrentTime, true);
                }
            }
        }

        #endregion

        #region PUBLIC

        public bool EnablePosition = true;
        public float SmoothTime;
        public float SmoothTimeMax = 1f;

        public bool EnableRotation = true;
        public bool FaceSurfaceHit;
        public float RotationSmoothTime;

        public Transform ApplyToTransform;

        public enum PlacementModes
        {
            SampleTerrainHeight,
            Raycast
        }
        public PlacementModes PlacementMode = PlacementModes.SampleTerrainHeight;

        public bool UseRigidbody;
        public float RaycastOffset;
        public Vector3 RaycastDirection = Vector3.down;
        public bool UseTransformForDirection;
        public Transform TransformDirection;
        public bool UseTerrainHeight = true;
        public float RaycastDistance = 1000f;
        public LayerMask RaycastLayerMask = (1 << 0);

        public Vector3 Orientation = Vector3.zero;

        public TimeflowChannel RotationChannel;

        public bool EnablePositionX = true;
        public bool EnablePositionY = true;
        public bool EnablePositionZ = true;

        public bool LimitPosition;
        public bool LimitPositionX;
        public bool LimitPositionY;
        public bool LimitPositionZ;
        public Vector3 PostionMin = Vector3.zero;
        public Vector3 PositionMax = Vector3.zero;

        public float Height;

        [NonSerialized]
        public RigidbodyHelper body;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Vector3 lastPos = Vector3.zero;

        [NonSerialized]
        private Vector3 lastEuler = Vector3.zero;

        [NonSerialized]
        private Rotator applyToRotator;

        [NonSerialized]
        private Vector3 raycastOrigin = Vector3.zero;

        [NonSerialized]
        private Vector3 raycastPoint = Vector3.zero;

        [NonSerialized]
        private Vector3 raycastNormal = Vector3.zero;

        #endregion

        #region ACCESSORS

        public RigidbodyHelper Body {
            get {
                if (body == null) {
                    if (ApplyToTransform == null) ApplyToTransform = transform;
                    body = new RigidbodyHelper(ApplyToTransform.gameObject);
                }
                return body;
            }
        }

        #endregion

        #region SETUP

        public override void Refresh()
        {
            base.Refresh();
            Setup();
            Process(Channel.CurrentTime, true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Setup();
#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject) {
                /// Make sure move and rotate tools can be accessed.
                Tools.hidden = false;
            }
#endif
        }

        public void Setup()
        {
            if (ApplyToTransform == null) ApplyToTransform = transform;
            applyToRotator = Rotator.Setup(ApplyToTransform);
            lastPos = transform.position;
            lastEuler = applyToRotator == null ? Vector3.zero : applyToRotator.Euler;

            if (EnableRotation) {
                applyToRotator.UsePhysics = UseRigidbody;
            }
            SetupPhysics();
        }

        public void SetupPhysics()
        {
            if (UseRigidbody && !Body.HasBody) {
                Body.AddRigidbody(ApplyToTransform.gameObject);
                Body.useGravity = false;
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            //if (DebugEnabled) Debug.Log(name + ".SetupChannels");
            Channel.PropertyType = Property.PropertyTypes.Vector3;
            Channel.Attribute = -1;
            Channel.IsCombinedValue = true;
            Channel.ToProperty.CanBeAssigned = false;
            Channel.ToProperty.EnableHandlers = false;
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            Channel.ToProperty.IsCombinedValue = true;
            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Placed Position";
            }

            if (EnableRotation) {
                if (RotationChannel == null) {
                    RotationChannel = new TimeflowChannel();
                }
                RotationChannel.IsEnabled = true;
                if (RotationChannel.ToProperty == null) {
                    RotationChannel.ToProperty = new Property();
                }
                RotationChannel.SupportsKeyframes = false;
                RotationChannel.PropertyType = Property.PropertyTypes.Vector3;
                RotationChannel.ToProperty.IsEnabled = true;
                RotationChannel.ToProperty.CanBeAssigned = false;
                RotationChannel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
                RotationChannel.ToProperty.IsDataOnly = true;
                RotationChannel.ToProperty.IsCombinedValue = true;
                RotationChannel.OnSetup(this);

                if (string.IsNullOrEmpty(RotationChannel.Name) || string.IsNullOrEmpty(RotationChannel.ToProperty.Name)) {
                    RotationChannel.Name = RotationChannel.ToProperty.Name = "Placed Rotation";
                }

                Channels.Add(RotationChannel);
            }
            else {
                if (RotationChannel != null && RotationChannel.IsEnabled) {
                    RemoveChannelWithUndo(RotationChannel);
                }
                RotationChannel = null;
            }
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// This handles assigning a position value while maintaining original values for any disabled
        /// axis. Only enabled axis values are assigned.
        /// </summary>
        /// <param name="from">The original position</param>
        /// <param name="to">The desired target position</param>
        private Vector3 TranslatePosition(Vector3 from, Vector3 to)
        {
            if (EnablePositionX) {
                from.x = to.x;
            }
            if (EnablePositionY) {
                from.y = to.y;
            }
            if (EnablePositionZ) {
                from.z = to.z;
            }
            if (LimitPosition) {
                Vector3 p = from;
                if (EnablePositionX && LimitPositionX) {
                    from.x = MathUtil.MinMax(from.x, PostionMin.x, PositionMax.x);
                }
                if (EnablePositionY && LimitPositionY) {
                    from.y = MathUtil.MinMax(from.y, PostionMin.y, PositionMax.y);
                }
                if (EnablePositionZ && LimitPositionZ) {
                    from.z = MathUtil.MinMax(from.z, PostionMin.z, PositionMax.z);
                }
            }
            return from;
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            if (!CanUpdate) return ApplyToTransform.position;
            //if (DebugEnabled) Debug.Log("PlaceOnSurface.OnUpdate:" + time + " apply:" + apply);
            return Process(time, apply);
        }

        /// <summary>
        /// Performs the placement operation. Note that the current transform is used for calculations. If
        /// the result is not being applied or is applied to a separate transform, then the current
        /// transform is reset to its original position, so that it is not modified by the placement. This
        /// setup is useful to maintain separate control over the placing transform versus the one being
        /// placed. 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public Vector3 Process(float time, bool apply)
        {
            /// target might be same or different than this.transform
            Vector3 targetPos = lastPos;
            Vector3 targeEuler = lastEuler;
            Vector3 targetPosLocal = ApplyToTransform.localPosition;

            Vector3 pos = targetPos;
            Vector3 euler = targeEuler;

            /// Store original transform values to restore at end
            Vector3 origPos = transform.position;
            Quaternion origRot = transform.rotation;

            if (!Body.Is2D && PlacementMode == PlacementModes.SampleTerrainHeight) {
                pos = origPos;
                float hit = ObjectUtil.GetTerrainHeight(pos) + Height;
                pos.y = hit;
                pos = TranslatePosition(targetPos, pos);
                euler = Orientation;
            }
            else {
                float terrainHeight = UseTerrainHeight ? ObjectUtil.GetTerrainHeight(pos) : 0f;
                float offsetLength = RaycastOffset + terrainHeight;

                Vector3 rayDirection = RaycastDirection;
                if (UseTransformForDirection && TransformDirection != null) {
                    rayDirection = TransformDirection.position - transform.position;
                    rayDirection.Normalize();
                }

                if (applyToRotator != null) {
                    /// Reset rotation before raycasting
                    applyToRotator.Euler = Vector3.zero;
                }
                raycastOrigin = transform.TransformPoint(rayDirection * -offsetLength);
                if (Body.Is2D) {
                    RaycastHit2D raycast2D = ObjectUtil.PlaceObjectOnRaycast2D(transform, UseRigidbody ? Body.Rigidbody2D : null, rayDirection, offsetLength, Height, EnablePosition, EnableRotation, RaycastDistance, RaycastLayerMask);

                    raycastPoint = raycast2D.point;
                    raycastNormal = raycast2D.normal;
                }
                else {
                    RaycastHit raycast = ObjectUtil.PlaceObjectOnRaycast(transform, UseRigidbody ? Body.Rigidbody : null, rayDirection, offsetLength, Height, EnablePosition, EnableRotation, RaycastDistance, RaycastLayerMask);

                    raycastPoint = raycast.point;
                    raycastNormal = raycast.normal;
                }

                if (FaceSurfaceHit) {
                    transform.LookAt(raycastPoint, Vector3.up);
                }

                pos = TranslatePosition(targetPos, transform.position);
                euler = transform.eulerAngles + Orientation;

                //if (DebugEnabled) Debug.Log("PlaceOnSurface.Process:" + time + " pos:" + pos + " xpos:" + transform.position);
            }

            if (apply) {
                if (LocalDeltaTime != 0f) {
                    if (SmoothTime > 0f) {
                        pos = MathUtil.Interpolate(lastPos, pos, LocalDeltaTime / SmoothTime);
                    }
                    if (EnableRotation && RotationSmoothTime > 0f) {
                        Vector3 target = MathUtil.RotationTarget(targeEuler, euler);
                        euler = MathUtil.Interpolate(targeEuler, target, LocalDeltaTime / RotationSmoothTime);
                    }
                }
                lastPos = pos;
                lastEuler = euler;
            }

            //if (DebugEnabled) Debug.Log("PlaceOnSurface.ApplyPosition:" + EnablePosition + " apply:" + apply + " euler:" + euler);
            if (apply) {
                if (EnablePosition) {
                    if (Application.isPlaying && UseRigidbody && Body.HasBody) {
                        Body.MovePosition(pos);
                    }
                    else {
                        ApplyToTransform.position = pos;
                    }
                    Channel.CurrentVector = Channel.ToProperty.Vector3Value = pos;

                    if (!EnablePositionX || !EnablePositionY || !EnablePositionZ) {
                        /// Preserve the original local position to get rid of rounding errors introduced
                        /// by using global position
                        Vector3 loc = ApplyToTransform.localPosition;
                        if (!EnablePositionX) {
                            loc.x = targetPosLocal.x;
                        }
                        if (!EnablePositionY) {
                            loc.y = targetPosLocal.y;
                        }
                        if (!EnablePositionZ) {
                            loc.z = targetPosLocal.z;
                        }
                        ApplyToTransform.localPosition = loc;
                    }
                }

                if (EnableRotation && applyToRotator != null) {
                    applyToRotator.Euler = euler; // Rotator handles Rigidbody already
                    if (RotationChannel != null) {
                        RotationChannel.CurrentVector = RotationChannel.ToProperty.Vector3Value = applyToRotator.Euler;
                    }
                }
            }

            /// Reset the current transform if not applying or if targetting a separate object
            if (!apply || ApplyToTransform != transform) {
                transform.position = origPos;
                transform.rotation = origRot;
            }


#if UNITY_EDITOR
            if (apply) rayUpdate = true;
#endif
            return pos;
        }

        #endregion

#if UNITY_EDITOR

        public bool EnableGizmos = true;
        public bool DebugDrawRay;
        public Color DebugRayColor = Color.red;
        public float DebugRayDuration = 5f;

        private bool rayUpdate;

        public override Texture2D Icon => AxonUI.Icons.PlaceOnSurface;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Place On Surface"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    PlaceOnSurface comp = Undo.AddComponent<PlaceOnSurface>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public override void DrawGizmos()
        {
            if (Selection.activeGameObject != gameObject) return;
            if (PlacementMode == PlacementModes.Raycast) {

                if (EnableGizmos && Enabled) {
                    float terrainHeight = UseTerrainHeight ? ObjectUtil.GetTerrainHeight(transform.position) : 0f;
                    Handles.color = GUIColor;

                    /// Reset rotation before raycasting
                    Vector3 euler = applyToRotator.Euler;
                    applyToRotator.Euler = Vector3.zero;

                    Vector3 rayDirection = RaycastDirection;
                    if (UseTransformForDirection && TransformDirection != null) {
                        rayDirection = TransformDirection.position - transform.position;
                        rayDirection.Normalize();
                    }

                    Vector3 direction = transform.TransformDirection(rayDirection);
                    Vector3 target = raycastOrigin + MathUtil.Multiply(direction, RaycastDistance);
                    Handles.DrawLine(raycastOrigin, target, 3f);

                    Handles.color = Color.yellow;
                    Handles.DrawSolidDisc(raycastPoint, raycastNormal, 0.1f);

                    Handles.color = Color.white;

                    applyToRotator.Euler = euler;
                }
                if (rayUpdate && DebugDrawRay) {
                    if (raycastPoint != Vector3.zero) {
                        Debug.DrawRay(raycastOrigin, raycastPoint - raycastOrigin, DebugRayColor, DebugRayDuration);
                    }
                    rayUpdate = false; // ensures ray is only drawn when updated
                }
            }
            base.DrawGizmos();
        }

#endif
    }

}//AxonGenesis
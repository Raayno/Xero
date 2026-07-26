// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(PlaceOnSurface))]
    public class PlaceOnSurfaceEditor : AxonGenesisEditor<PlaceOnSurface, PlaceOnSurfaceEdit> { }

    sealed public class PlaceOnSurfaceEdit : AxonGenesisBehaviorEdit<PlaceOnSurface>
    {
#if TIMEFLOW_PRO
        public const string kAddPlaceOnSurface = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "⛳ Place On Surface";
#else
        public const string kAddPlaceOnSurface = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Place On Surface";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Place On Surface";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddPlaceOnSurface, false, 142)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddPlaceOnSurface, false, 142)]
        public static void AddPlaceOnSurface()
        {
            ObjectUtil.GetOrAddComponent<PlaceOnSurface>(TimeflowMenu.GetSelectedOrNewGameObject("Place On Surface"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public PlaceOnSurfaceEdit() { }

        public PlaceOnSurfaceEdit(PlaceOnSurface _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/place-on-surface";
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();

            if (target.Body.Is2D) {
                target.PlacementMode = PlaceOnSurface.PlacementModes.Raycast;
                EditorGUI.BeginDisabledGroup(true);
            }

            AxonGUI.UndoName = "Set Placement Mode";
            AxonGUI.SetTooltip("Placement can be done either by sampling the height of the terrain (faster), or by performing a raycast to detect collisions with objects on the specified layer mask. Raycasts have greater flexibility but come at a performance cost, so use it wisely.");
            target.PlacementMode = (PlaceOnSurface.PlacementModes)AxonGUI.FieldEnumPopupInline(target, target.PlacementMode);
            if (target.Body.Is2D) {
                EditorGUI.EndDisabledGroup();
                AxonGUI.Info("Only raycast mode is available when using 2D physics.");
            }

            if (AxonGUI.ButtonInline("Refresh All")) {
                PlaceOnSurface.ProcessAll();
            }
            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            if (target.PlacementMode == PlaceOnSurface.PlacementModes.SampleTerrainHeight) {
                target.EnableRotation = false;
            }
            RaycastGUI();
            RigidbodyGUI();
            PositionGUI();
            RotationGUI();
            GizmosGUI();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                if (target.UpdateFrequency != TimeflowBehavior.UpdateFrequencies.Explicit) {
                    target.Refresh();
                }
                EditorUtil.SetDirty(target);
            }
        }

        public void RaycastGUI()
        {
            if (target.PlacementMode == PlaceOnSurface.PlacementModes.Raycast) {
                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                if (target.PlacementMode != PlaceOnSurface.PlacementModes.SampleTerrainHeight) {
                    AxonGUI.UndoName = "Set Layer Mask";
                    AxonGUI.SetTooltip("Only objects and terrains on the specified layer mask are processed.");
                    target.RaycastLayerMask = AxonGUI.FieldLayerMask(target, "Layer Mask", target.RaycastLayerMask);


                    if (ObjectUtil.IsOnLayer(target.gameObject.layer, target.RaycastLayerMask)) {
                        AxonGUI.Warning("Make sure the current object is on a different layer than the placement layer mask, otherwise raycasts will hit the object resulting in self-referencing runaway behavior. If your object flies off into space during placement, this is likely the cause.");
                    }
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Sets the vector angle for the raycast. Typically this is set to 0,-1,0 so that the ray is projected downward on the Y axis.");

                if (target.UseTransformForDirection) {
                    AxonGUI.UndoName = "Set Raycast Direction";
                    target.TransformDirection = (Transform)AxonGUI.FieldObject(target, "Raycast Direction", target.TransformDirection, typeof(Transform), true);
                }
                else {
                    AxonGUI.UndoName = "Set Raycast Direction";
                    target.RaycastDirection = AxonGUI.FieldVector3(target, "Raycast Direction", target.RaycastDirection.normalized);
                }
                AxonGUI.UndoName = "Set Use Transform";
                target.UseTransformForDirection = AxonGUI.FieldToggleInline(target, "Use Transform", target.UseTransformForDirection);

                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Raycast Offset";
                AxonGUI.SetTooltip("An additional offset to the raycast starting position. It is often necessary to increase the offset to allow extra space for objects.");
                target.RaycastOffset = AxonGUI.FieldFloat(target, "Raycast Offset", target.RaycastOffset);

                AxonGUI.UndoName = "Set Terrain Height";
                AxonGUI.SetTooltip("If enabled, the raycast starting point is set to the height of the terrain. This ensures the raycast always starts above the terrain so it has a chance to hit it.");
                target.UseTerrainHeight = AxonGUI.FieldToggleInline(target, "+ Terrain Height", target.UseTerrainHeight);
                AxonGUI.EndHorizontal();


                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Raycast Distance";
                AxonGUI.SetTooltip("This sets the length of the raycast. Make sure this value is set large enough to reach all the objects desired.");
                target.RaycastDistance = AxonGUI.FieldFloat(target, "Raycast Distance", target.RaycastDistance);

                AxonGUI.EndHorizontal();

                AxonGUI.EndBox();
            }
        }

        public void PositionGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Position";
            AxonGUI.SetTooltip("Use Smooth mode to reduce jitter, though too much may cause floating or passing through objects.");
            target.EnablePosition = AxonGUI.FieldToggle(target, "Position", target.EnablePosition);

            AxonGUI.SetTooltip("The object position is only applied to the axes enabled.");
            AxonGUI.UndoName = "Set Position X";
            target.EnablePositionX = AxonGUI.FieldToggleInline(target, "X", target.EnablePositionX);

            AxonGUI.UndoName = "Set Position Y";
            target.EnablePositionY = AxonGUI.FieldToggleInline(target, "Y", target.EnablePositionY);

            AxonGUI.UndoName = "Set Position Z";
            target.EnablePositionZ = AxonGUI.FieldToggleInline(target, "Z", target.EnablePositionZ);
            AxonGUI.EndHorizontal();

            if (target.EnablePosition) {
                AxonGUI.BeginHorizontal();
                if (target.SmoothTime < 0f) target.SmoothTime = 0f;
                AxonGUI.UndoName = "Set Position Smooth Time";
                target.SmoothTime = AxonGUI.FieldSlider(target, "Smooth Time", target.SmoothTime, 0f, target.SmoothTimeMax);
                AxonGUI.UndoName = "Set Position Smooth Time Max";
                target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Position Height Offset";
                AxonGUI.SetTooltip("This offsets the height of the final placement. This can be used to account for an object's pivot point if it is not at the base of the mesh.");
                target.Height = AxonGUI.FieldFloat(target, "Height Offset", target.Height);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Position World Limits";
                target.LimitPosition = AxonGUI.FieldToggle(target, "World Limits", target.LimitPosition);
                if (target.LimitPosition) {
                    if (!target.EnablePositionX) target.LimitPositionX = false;
                    EditorGUI.BeginDisabledGroup(!target.EnablePositionX);
                    AxonGUI.UndoName = "Set Position World Limit X";
                    target.LimitPositionX = AxonGUI.FieldToggleInline(target, "X", target.LimitPositionX);
                    EditorGUI.EndDisabledGroup();

                    if (!target.EnablePositionY) target.LimitPositionY = false;
                    EditorGUI.BeginDisabledGroup(!target.EnablePositionY);
                    AxonGUI.UndoName = "Set Position World Limit Y";
                    target.LimitPositionY = AxonGUI.FieldToggleInline(target, "Y", target.LimitPositionY);
                    EditorGUI.EndDisabledGroup();

                    if (!target.EnablePositionZ) target.LimitPositionZ = false;
                    EditorGUI.BeginDisabledGroup(!target.EnablePositionZ);
                    AxonGUI.UndoName = "Set Position World Limit Z";
                    target.LimitPositionZ = AxonGUI.FieldToggleInline(target, "Z", target.LimitPositionZ);
                    EditorGUI.EndDisabledGroup();

                    AxonGUI.EndHorizontal();

                    AxonGUI.UndoName = "Set Position Min";
                    target.PostionMin = AxonGUI.FieldVector3(target, "Min", target.PostionMin);

                    AxonGUI.UndoName = "Set Position Max";
                    target.PositionMax = AxonGUI.FieldVector3(target, "Max", target.PositionMax);
                }
                else {
                    AxonGUI.EndHorizontal();
                }
            }
            AxonGUI.EndBox();
        }

        public void RotationGUI()
        {
            if (target.PlacementMode == PlaceOnSurface.PlacementModes.Raycast) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation";
                AxonGUI.SetTooltip("Apply smoothing to reduce jitter, though too much smoothing may cause intersection with the terrain and objects.");
                target.EnableRotation = AxonGUI.FieldToggle(target, "Rotation", target.EnableRotation);
                if (target.EnableRotation) {
                    AxonGUI.UndoName = "Set Rotation Face Surface Hit";
                    AxonGUI.SetTooltip("If enabled, the object rotation looks at the raycast hit position. Otherwise if off, the object rotates to match the surface normal.");
                    target.FaceSurfaceHit = AxonGUI.FieldToggleInline(target, "Face Surface Hit", target.FaceSurfaceHit);
                }
                AxonGUI.EndHorizontal();

                if (target.EnableRotation) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Rotation Smooth Time";
                    target.RotationSmoothTime = AxonGUI.FieldSlider(target, "Smooth Time", target.RotationSmoothTime, 0f, target.SmoothTimeMax);

                    AxonGUI.UndoName = "Set Rotation Smooth Time Max";
                    target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Orientation";
                    AxonGUI.SetTooltip("Rotation offset to orient the object after placement.");
                    target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);
                    AxonGUI.EndHorizontal();
                }

                AxonGUI.EndBox();
            }
        }

        public void RigidbodyGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply To";
            AxonGUI.SetTooltip("Assign the transform to apply the placement to. This defaults to be the same object.");
            target.ApplyToTransform = (Transform)AxonGUI.FieldObject(target, "Apply to", target.ApplyToTransform, typeof(Transform), true);

            AxonGUI.UndoName = "Set Use Rigidbody";
            AxonGUI.SetTooltip("When enabled, position and rotation are applied via the Rigidbody to allow for physics actions.");
            bool useBody = AxonGUI.FieldToggleInline(target, "Use Rigidbody", target.UseRigidbody);
            if (target.UseRigidbody != useBody) {
                target.UseRigidbody = useBody;
                if (useBody) target.SetupPhysics();
            }
            AxonGUI.EndHorizontal();

            if (target.UseRigidbody && target.Body.HasBody) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.HelpBox("This setup requires a rigidbody component and collider.", UnityEditor.MessageType.Warning);
                if (AxonGUI.ButtonInline("Fix")) {
                    target.SetupPhysics();
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        public void GizmosGUI()
        {
            if (target.PlacementMode == PlaceOnSurface.PlacementModes.Raycast) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Draw Gizmos";
                AxonGUI.SetTooltip("Draws a line showing the direction, orientation, and length rays are cast along.");
                target.EnableGizmos = AxonGUI.FieldToggle(target, "Gizmos", target.EnableGizmos);
                if (target.EnableGizmos) {
                    AxonGUI.UndoName = "Set Gizmos Color";
                    target.GUIColor = AxonGUI.FieldColorInline(target, target.GUIColor, false);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("For each raycast calculated, draws a line in the Scene view and remains visible for the duration set.");
                AxonGUI.UndoName = "Set Draw Debug Ray";
                target.DebugDrawRay = AxonGUI.FieldToggle(target, "Debug Ray", target.DebugDrawRay);
                if (target.DebugDrawRay) {
                    AxonGUI.UndoName = "Set Debug Ray Color";
                    target.DebugRayColor = AxonGUI.FieldColorInline(target, target.DebugRayColor, false);
                    AxonGUI.SetTooltip("Sets the duration for rays to be displayed before they dissappear.");

                    AxonGUI.UndoName = "Set Debug Ray Duration";
                    target.DebugRayDuration = AxonGUI.FieldFloatInline(target, "Duration", target.DebugRayDuration);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }
        }

        public override void OnSceneGUI()
        {
            if (target == null) return;
            target.DrawGizmos();
        }

    }

}//AxonGenesis

#endif
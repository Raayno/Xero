// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

namespace AxonGenesis
{
    /// <summary>
    /// Manages the main menu options displayed under Tool/AxonGenesis for Timeflow based operations. The
    /// Tools menu is mandatory as per the asset store submission guidelines
    /// </summary>
    public static partial class TimeflowMenu
    {
        public const string Sep = "/";
        public const string Tab = "\t";
        public const string Tab2 = "     "; // Tab causes invalid menu path error

#if TIMEFLOW_PRO

        public const string kTimeflow = "🎬 Timeflow";
        public const string kAddBehavior = "➕ Add Behavior";
        public const string kAnimation = "🕺 Animation";
        public const string kComposition = "📥 Composition";
        public const string kDisplay = "🖥️ Display";
        public const string kJumpTo = "⤵️ Jump to";
        public const string kHierarchy = "▼ Hierarchy";
        public const string kMesh = "🌐 Mesh";
        public const string kSelect = "👆 Select";
        public const string kTracks = "🟰 Tracks";
        public const string kTransform = "✳️ Transform";
        public const string kVisibility = "👁️ Visibility";
        public const string kEditor = "⚙️ Editor";

        public const string MenuPath = kTimeflow + Sep;
        public const string MenuPath2 = "GameObject" + Sep + kTimeflow + Sep;

        public const string kAutoKeyframing = kAnimation + Sep + "🕐 Toggle Auto-Keyframing";
        public const string kAddKeyframe = kAnimation + Sep + "➕ Add Keyframe";

        public const string kAddNewTimeflow = kComposition + Sep + "🆕 Add New Timeflow";
        public const string kPrecompose = kComposition + Sep + "📥 Precompose or Add Precomp";
        public const string kDecompose = kComposition + Sep + "📤 Decompose";
        public const string kEnterEditMode = kComposition + Sep + "🖊️ Enter Edit Mode";
        public const string kExitEditMode = kComposition + Sep + "🚪 Exit Edit Mode";
        public const string kSaveSelectedPrefabs = kComposition + Sep + "💾 Save Selected Prefabs";

        public const string kDisplayNothing = kDisplay + Sep + "🚫 Nothing";
        public const string kDisplayEverything = kDisplay + Sep + "🌐 Everything";
        public const string kDisplaySelectedOnly = kDisplay + Sep + "🔲 Selected Only";
        public const string kDisplayAddSelectedToView = kDisplay + Sep + "➕ Add Selected to View";
        public const string kDisplayActiveSelectionGrouped = kDisplay + Sep + "👥 Selected Group";
        public const string kDisplaySoloSelectedAppend = kDisplay + Sep + "🎯➕ Solo Selected (Append)";
        public const string kDisplaySelectedObject = kDisplay + Sep + "🔘 Selected Object";
        public const string kDisplaySoloSelected = kDisplay + Sep + "🎯 Solo Selected";
        public const string kDisplayToggleHidden = kDisplay + Sep + "🙈 Toggle Hidden";
        public const string kDisplayPrevious = kDisplay + Sep + "⏮️ Previous Saved Display";
        public const string kDisplayNext = kDisplay + Sep + "⏭️ Next Saved Display";

        public const string kJumpToFullDuration = kJumpTo + Sep + "⏱️ Full Duration";
        public const string kJumpToMarker1 = kJumpTo + Sep + "📍 Marker 1";
        public const string kJumpToMarker2 = kJumpTo + Sep + "📍 Marker 2";
        public const string kJumpToMarker3 = kJumpTo + Sep + "📍 Marker 3";
        public const string kJumpToMarker4 = kJumpTo + Sep + "📍 Marker 4";
        public const string kJumpToMarker5 = kJumpTo + Sep + "📍 Marker 5";
        public const string kJumpToMarker6 = kJumpTo + Sep + "📍 Marker 6";
        public const string kJumpToMarker7 = kJumpTo + Sep + "📍 Marker 7";
        public const string kJumpToMarker8 = kJumpTo + Sep + "📍 Marker 8";
        public const string kJumpToMarker9 = kJumpTo + Sep + "📍 Marker 9";

        public const string kDestroyAllTimeflowBehaviors = kHierarchy + Sep + "💥 Destroy All Timeflow Behaviors (Selected Objects)";
        public const string kDeleteChildren = kHierarchy + Sep + "🗑️ Delete Children";
        public const string kSortChildren = kHierarchy + Sep + "🔼 Sort Children";
        public const string kSortChildrenReverse = kHierarchy + Sep + "🔽 Sort Children Reverse";
        public const string kHideChildrenInHierarchy = kHierarchy + Sep + "🙈 Hide Children in Hierarchy";
        public const string kShowChildrenInHierarchy = kHierarchy + Sep + "👀 Show Children in Hierarchy";
        public const string kGroup = kHierarchy + Sep + "🗂️ Group";
        public const string kUngroup = kHierarchy + Sep + "📤 Ungroup";
        public const string kUnparent = kHierarchy + Sep + "⛓️‍💥 Unparent";
        public const string kFlatten = kHierarchy + Sep + "📄 Flatten";
        public const string kRemoveNumbering = kHierarchy + Sep + "📛 Remove Numbering";

        public const string kGetRendererSize = kMesh + Sep + "📏 Get Renderer Size";
        public const string kGetBoundingBox = kMesh + Sep + "📦 Get Bounding Box";
        public const string kGetPolycount = kMesh + Sep + "🔢 Get Polycount";
        public const string kFreezeMesh = kMesh + Sep + "❄️ Freeze Mesh";
        public const string kCombineMeshes = kMesh + Sep + "🧩 Combine Meshes";

        public const string kDeselectAll = kSelect + Sep + "🚫 Deselect All";
        public const string kSelectChildren = kSelect + Sep + "👶 Children";
        public const string kSelectDescendants = kSelect + Sep + "🌳 Descendants";
        public const string kSelectParents = kSelect + Sep + "👪 Parent";
        public const string kSelectAncestors = kSelect + Sep + "🧬 Ancestors";
        public const string kSelectRenderersRecursive = kSelect + Sep + "🎨 Renderers Recursive";
        public const string kSelectMainCamera = kSelect + Sep + "🎥 Main Camera";

        public const string kQuickSelectObject1 = kSelect + Sep + "🔵 Object 1";
        public const string kQuickSelectObject2 = kSelect + Sep + "🔵 Object 2";
        public const string kQuickSelectObject3 = kSelect + Sep + "🔵 Object 3";
        public const string kQuickSelectObject4 = kSelect + Sep + "🔵 Object 4";
        public const string kQuickSelectObject5 = kSelect + Sep + "🔵 Object 5";
        public const string kQuickSelectObject6 = kSelect + Sep + "🔵 Object 6";
        public const string kQuickSelectObject7 = kSelect + Sep + "🔵 Object 7";
        public const string kQuickSelectObject8 = kSelect + Sep + "🔵 Object 8";
        public const string kQuickSelectObject9 = kSelect + Sep + "🔵 Object 9";
        public const string kQuickSelectObject10 = kSelect + Sep + "🔵 Object 10";
        public const string kQuickSelectObject11 = kSelect + Sep + "🔵 Object 11";
        public const string kQuickSelectObject12 = kSelect + Sep + "🔵 Object 12";

        public const string kQuickSelectAssignObject1 = kSelect + Sep + "📝 Assign Object 1";
        public const string kQuickSelectAssignObject2 = kSelect + Sep + "📝 Assign Object 2";
        public const string kQuickSelectAssignObject3 = kSelect + Sep + "📝 Assign Object 3";
        public const string kQuickSelectAssignObject4 = kSelect + Sep + "📝 Assign Object 4";
        public const string kQuickSelectAssignObject5 = kSelect + Sep + "📝 Assign Object 5";
        public const string kQuickSelectAssignObject6 = kSelect + Sep + "📝 Assign Object 6";
        public const string kQuickSelectAssignObject7 = kSelect + Sep + "📝 Assign Object 7";
        public const string kQuickSelectAssignObject8 = kSelect + Sep + "📝 Assign Object 8";
        public const string kQuickSelectAssignObject9 = kSelect + Sep + "📝 Assign Object 9";
        public const string kQuickSelectAssignObject10 = kSelect + Sep + "📝 Assign Object 10";
        public const string kQuickSelectAssignObject11 = kSelect + Sep + "📝 Assign Object 11";
        public const string kQuickSelectAssignObject12 = kSelect + Sep + "📝 Assign Object 12";

        public const string kResetTracksSelected = kTracks + Sep + "🔄 Reset Tracks (Selected Objects)";
        public const string kResetAllTracks = kTracks + Sep + "🗑️ Reset All Tracks In Scene";
        public const string kJoinAdjacentTracks = kTracks + Sep + "🔗 Join Adjacent Tracks (Selected Objects)";

        public const string kTransformReset = kTransform + Sep + "🔄 Reset Transform";
        public const string kTransformCopy = kTransform + Sep + "📋 Copy Transform";
        public const string kTransformPaste = kTransform + Sep + "📥 Paste Transform";
        public const string kTransformPasteResetScale = kTransform + Sep + "📥 Paste Transform Reset Scale";
        public const string kTransformPastePositionOnly = kTransform + Sep + "📍 Paste Position Only";

        public const string kVisibilityActivate = kVisibility + Sep + "✅ Activate";
        public const string kVisibilityDeactivate = kVisibility + Sep + "🚫 Deactivate";
        public const string kVisibilityActivateRecursive = kVisibility + Sep + "✅🔁 Activate Recursively";
        public const string kVisibilityDeactivateRecursive = kVisibility + Sep + "🚫🔁 Deactivate Recursively";
        public const string kVisibilityEnableRenderersRecursive = kVisibility + Sep + "🟢🔁Enable Renderers Recursively";
        public const string kVisibilityDisableRenderersRecursive = kVisibility + Sep + "🟢🔁Disable Renderers Recursively";

        public const string kEditorDebugMarkLine = kEditor + Sep + "🐞 Debug Mark Line in Console";
        public const string kEditorDebugBoard = kEditor + Sep + "🐛 Debug Board";
        public const string kEditorDisableDebugAll = kEditor + Sep + "🚫 Disable Debug For All Objects";
        public const string kEditorListDependencies = kEditor + Sep + "📋 List Dependencies";
#else
        public const string kTimeflow = "Timeflow";
        public const string kAddBehavior = "Add Behavior";
        public const string kAnimation = "Animation";
        public const string kComposition = "Composition";
        public const string kDisplay = "Composition";
        public const string kJumpTo = "Jump to";
        public const string kHierarchy = "Hierarchy";
        public const string kMesh = "Mesh";
        public const string kSelect = "Select";
        public const string kTracks = "Tracks";
        public const string kTransform = "Transform";
        public const string kVisibility = "Visibility";
        public const string kEditor = "Editor";

        public const string MenuPath = "Tools" + Sep + kTimeflow + Sep;
        public const string MenuPath2 = "GameObject" + Sep + kTimeflow + Sep;

        public const string kAutoKeyframing = kAnimation + Sep + "Toggle Auto-Keyframing";
        public const string kAddKeyframe = kAnimation + Sep + "Add Keyframe";

        public const string kAddNewTimeflow = kComposition + Sep + "Add New Timeflow";
        public const string kPrecompose = kComposition + Sep + "Precompose or Add Precomp";
        public const string kDecompose = kComposition + Sep + "Decompose";
        public const string kEnterEditMode = kComposition + Sep + "Enter Edit Mode";
        public const string kExitEditMode = kComposition + Sep + "Exit Edit Mode";
        public const string kSaveSelectedPrefabs = kComposition + Sep + "Save Selected Prefabs";

        public const string kDisplayNothing = kDisplay + Sep + "Nothing";
        public const string kDisplayEverything = kDisplay + Sep + "Everything";
        public const string kDisplaySelectedOnly = kDisplay + Sep + "Selected Only";
        public const string kDisplayAddSelectedToView = kDisplay + Sep + "Add Selected to View";
        public const string kDisplayActiveSelectionGrouped = kDisplay + Sep + "Selected Group";
        public const string kDisplaySoloSelectedAppend = kDisplay + Sep + "Solo Selected (Append)";
        public const string kDisplaySelectedObject = kDisplay + Sep + "Selected Object";
        public const string kDisplaySoloSelected = kDisplay + Sep + "Solo Selected";
        public const string kDisplayToggleHidden = kDisplay + Sep + "Toggle Hidden";
        public const string kDisplayPrevious = kDisplay + Sep + "Previous Saved Display";
        public const string kDisplayNext = kDisplay + Sep + "Next Saved Display";

        public const string kJumpToFullDuration = kJumpTo + Sep + "Full Duration";
        public const string kJumpToMarker1 = kJumpTo + Sep + "Marker 1";
        public const string kJumpToMarker2 = kJumpTo + Sep + "Marker 2";
        public const string kJumpToMarker3 = kJumpTo + Sep + "Marker 3";
        public const string kJumpToMarker4 = kJumpTo + Sep + "Marker 4";
        public const string kJumpToMarker5 = kJumpTo + Sep + "Marker 5";
        public const string kJumpToMarker6 = kJumpTo + Sep + "Marker 6";
        public const string kJumpToMarker7 = kJumpTo + Sep + "Marker 7";
        public const string kJumpToMarker8 = kJumpTo + Sep + "Marker 8";
        public const string kJumpToMarker9 = kJumpTo + Sep + "Marker 9";

        public const string kDestroyAllTimeflowBehaviors = kHierarchy + Sep + "Destroy All Timeflow Behaviors (Selected Objects)";
        public const string kDeleteChildren = kHierarchy + Sep + "Delete Children";
        public const string kSortChildren = kHierarchy + Sep + "Sort Children";
        public const string kSortChildrenReverse = kHierarchy + Sep + "Sort Children Reverse";
        public const string kHideChildrenInHierarchy = kHierarchy + Sep + "Hide Children in Hierarchy";
        public const string kShowChildrenInHierarchy = kHierarchy + Sep + "Show Children in Hierarchy";
        public const string kGroup = kHierarchy + Sep + "Group";
        public const string kUngroup = kHierarchy + Sep + "Ungroup";
        public const string kUnparent = kHierarchy + Sep + "Unparent";
        public const string kFlatten = kHierarchy + Sep + "Flatten";
        public const string kRemoveNumbering = kHierarchy + Sep + "Remove Numbering";

        public const string kGetRendererSize = kMesh + Sep + "Get Renderer Size";
        public const string kGetBoundingBox = kMesh + Sep + "Get Bounding Box";
        public const string kGetPolycount = kMesh + Sep + "Get Polycount";
        public const string kFreezeMesh = kMesh + Sep + "Freeze Mesh";
        public const string kCombineMeshes = kMesh + Sep + "Combine Meshes";

        public const string kDeselectAll = kSelect + Sep + "Deselect All";
        public const string kSelectChildren = kSelect + Sep + "Children";
        public const string kSelectDescendants = kSelect + Sep + "Descendants";
        public const string kSelectParents = kSelect + Sep + "Parent";
        public const string kSelectAncestors = kSelect + Sep + "Ancestors";
        public const string kSelectRenderersRecursive = kSelect + Sep + "Renderers Recursive";
        public const string kSelectMainCamera = kSelect + Sep + "Main Camera";

        public const string kQuickSelectObject1 = kSelect + Sep + "Object 1";
        public const string kQuickSelectObject2 = kSelect + Sep + "Object 2";
        public const string kQuickSelectObject3 = kSelect + Sep + "Object 3";
        public const string kQuickSelectObject4 = kSelect + Sep + "Object 4";
        public const string kQuickSelectObject5 = kSelect + Sep + "Object 5";
        public const string kQuickSelectObject6 = kSelect + Sep + "Object 6";
        public const string kQuickSelectObject7 = kSelect + Sep + "Object 7";
        public const string kQuickSelectObject8 = kSelect + Sep + "Object 8";
        public const string kQuickSelectObject9 = kSelect + Sep + "Object 9";
        public const string kQuickSelectObject10 = kSelect + Sep + "Object 10";
        public const string kQuickSelectObject11 = kSelect + Sep + "Object 11";
        public const string kQuickSelectObject12 = kSelect + Sep + "Object 12";

        public const string kQuickSelectAssignObject1 = kSelect + Sep + "Assign Object 1";
        public const string kQuickSelectAssignObject2 = kSelect + Sep + "Assign Object 2";
        public const string kQuickSelectAssignObject3 = kSelect + Sep + "Assign Object 3";
        public const string kQuickSelectAssignObject4 = kSelect + Sep + "Assign Object 4";
        public const string kQuickSelectAssignObject5 = kSelect + Sep + "Assign Object 5";
        public const string kQuickSelectAssignObject6 = kSelect + Sep + "Assign Object 6";
        public const string kQuickSelectAssignObject7 = kSelect + Sep + "Assign Object 7";
        public const string kQuickSelectAssignObject8 = kSelect + Sep + "Assign Object 8";
        public const string kQuickSelectAssignObject9 = kSelect + Sep + "Assign Object 9";
        public const string kQuickSelectAssignObject10 = kSelect + Sep + "Assign Object 10";
        public const string kQuickSelectAssignObject11 = kSelect + Sep + "Assign Object 11";
        public const string kQuickSelectAssignObject12 = kSelect + Sep + "Assign Object 12";

        public const string kResetTracksSelected = kTracks + Sep + "Reset Tracks (Selected Objects)";
        public const string kResetAllTracks = kTracks + Sep + "Reset All Tracks In Scene";
        public const string kJoinAdjacentTracks = kTracks + Sep + "Join Adjacent Tracks (Selected Objects)";

        public const string kTransformReset = kTransform + Sep + "Reset Transform";
        public const string kTransformCopy = kTransform + Sep + "Copy Transform";
        public const string kTransformPaste = kTransform + Sep + "Paste Transform";
        public const string kTransformPasteResetScale = kTransform + Sep + "Paste Transform Reset Scale";
        public const string kTransformPastePositionOnly = kTransform + Sep + "Paste Position Only";

        public const string kVisibilityActivate = kVisibility + Sep + "Activate";
        public const string kVisibilityDeactivate = kVisibility + Sep + "Deactivate";
        public const string kVisibilityActivateRecursive = kVisibility + Sep + "Activate Recursively";
        public const string kVisibilityDeactivateRecursive = kVisibility + Sep + "Deactivate Recursively";
        public const string kVisibilityEnableRenderersRecursive = kVisibility + Sep + "Enable Renderers Recursively";
        public const string kVisibilityDisableRenderersRecursive = kVisibility + Sep + "Disable Renderers Recursively";

        public const string kEditorDebugMarkLine = kEditor + Sep + "Debug Mark Line in Console";
        public const string kEditorDebugBoard = kEditor + Sep + "Debug Board";
        public const string kEditorDisableDebugAll = kEditor + Sep + "Disable Debug For All Objects";
        public const string kEditorListDependencies = kEditor + Sep + "List Dependencies";

#endif
    }

}//AxonGenesis

#endif

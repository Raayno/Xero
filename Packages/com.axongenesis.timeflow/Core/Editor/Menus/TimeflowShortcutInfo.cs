// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using System.Reflection;

namespace AxonGenesis
{
    public static class TimeflowShortcutInfo
    {
        public static List<TimeflowShortcut> GetAllShortcuts()
        {
            var fields = typeof(TimeflowShortcutInfo).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var shortcuts = new List<TimeflowShortcut>();
            foreach (var field in fields) {
                // Check for static readonly fields of type TimeflowShortcut
                if (field.IsInitOnly && field.FieldType == typeof(TimeflowShortcut)) // IsInitOnly == true for readonly
                {
                    shortcuts.Add((TimeflowShortcut)field.GetValue(null)); // null because static
                }
            }

            shortcuts.Sort((a, b) => string.Compare(a.Category, b.Category));
            return shortcuts;
        }

        #region TIMEFLOW

        public const string Path_OpenTimeflowWindow = "Timeflow/Open Timeflow Window";
        public const string Path_OpenAdvancedPresets = "Timeflow/Open Advanced Presets";
        public const string Path_QuickMenu = "Timeflow/Quick Menu";
        public const string Path_AddNewTimeflow = "Timeflow/Add New Timeflow";
        public const string Path_SaveSceneBackup = "Timeflow/Save Scene Backup";
        public const string Path_OpenDocumentation = "Timeflow/Open Documentation";
        public const string Path_ToggleTimeflowWindowMinimized = "Timeflow/Toggle Timeflow Minimized";
        public const string Path_TimeflowPro = "Timeflow/Toggle Timeflow Pro";

        public static readonly TimeflowShortcut OpenTimeflowWindow = new TimeflowShortcut("Timeflow", "Open Timeflow Window", Path_OpenTimeflowWindow);
        public static readonly TimeflowShortcut OpenAdvancedPresets = new TimeflowShortcut("Timeflow", "Open Advanced Presets", Path_OpenAdvancedPresets);
        public static readonly TimeflowShortcut AddNewTimeflow = new TimeflowShortcut("Timeflow", "Add New Timeflow", Path_AddNewTimeflow);
        public static readonly TimeflowShortcut SaveSceneBackup = new TimeflowShortcut("Timeflow", "Save Scene Backup", Path_SaveSceneBackup);
        public static readonly TimeflowShortcut QuickMenu = new TimeflowShortcut("Timeflow", "Quick Menu", Path_QuickMenu);
        public static readonly TimeflowShortcut OpenDocumentation = new TimeflowShortcut("Timeflow", "Open Documentation", Path_OpenDocumentation);
        public static readonly TimeflowShortcut ToggleTimeflowWindowMinimized = new TimeflowShortcut("Timeflow", "Toggle Timeflow Minimized", Path_ToggleTimeflowWindowMinimized);
        public static readonly TimeflowShortcut TimeflowPro = new TimeflowShortcut("Timeflow", "Toggle Timeflow Pro", Path_TimeflowPro);

        #endregion

        #region PLAYBACK  

        public const string Path_TogglePlay = "Timeflow/Playback: Toggle Play";
        public const string Path_TogglePlayReverse = "Timeflow/Playback: Toggle Play Reverse";
        public const string Path_ToggleContinuousPlay = "Timeflow/Playback: Toggle Continuous Play";
        public const string Path_LoopSelected = "Timeflow/View: Loop Selected";
        public const string Path_ToggleLoop = "Timeflow/View: Toggle Loop";

        public static readonly TimeflowShortcut TogglePlay = new TimeflowShortcut("Playback", "Toggle Play", Path_TogglePlay);
        public static readonly TimeflowShortcut TogglePlayReverse = new TimeflowShortcut("Playback", "Toggle Play Reverse", Path_TogglePlayReverse);
        public static readonly TimeflowShortcut ToggleContinuousPlay = new TimeflowShortcut("Playback", "Toggle Continuous Play", Path_ToggleContinuousPlay);
        public static readonly TimeflowShortcut LoopSelected = new TimeflowShortcut("Playback", "Loop Selected", Path_LoopSelected, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ToggleLoop = new TimeflowShortcut("Playback", "Toggle Loop", Path_ToggleLoop, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region MARKERS  

        public const string Path_ToggleMarkers = "Timeflow/View: Show Markers";
        public const string Path_AddMarkerAtCurrentTime = "Timeflow/View: Add Marker at Current Time";

        public static readonly TimeflowShortcut ShowMarkers = new TimeflowShortcut("Markers", "Show Markers", Path_ToggleMarkers, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut AddMarker = new TimeflowShortcut("Markers", "Add Marker", Path_AddMarkerAtCurrentTime, TimeflowShortcut.ShortcutTypes.View);

        public const string Path_JumpToMarker1 = "Timeflow/Jump to Marker 1";
        public const string Path_JumpToMarker2 = "Timeflow/Jump to Marker 2";
        public const string Path_JumpToMarker3 = "Timeflow/Jump to Marker 3";
        public const string Path_JumpToMarker4 = "Timeflow/Jump to Marker 4";
        public const string Path_JumpToMarker5 = "Timeflow/Jump to Marker 5";
        public const string Path_JumpToMarker6 = "Timeflow/Jump to Marker 6";
        public const string Path_JumpToMarker7 = "Timeflow/Jump to Marker 7";
        public const string Path_JumpToMarker8 = "Timeflow/Jump to Marker 8";
        public const string Path_JumpToMarker9 = "Timeflow/Jump to Marker 9";
        public const string Path_JumpToFullDuration = "Timeflow/Jump to Full Duration";

        public static readonly TimeflowShortcut JumpToMarker1 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker1);
        public static readonly TimeflowShortcut JumpToMarker2 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker2);
        public static readonly TimeflowShortcut JumpToMarker3 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker3);
        public static readonly TimeflowShortcut JumpToMarker4 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker4);
        public static readonly TimeflowShortcut JumpToMarker5 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker5);
        public static readonly TimeflowShortcut JumpToMarker6 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker6);
        public static readonly TimeflowShortcut JumpToMarker7 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker7);
        public static readonly TimeflowShortcut JumpToMarker8 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker8);
        public static readonly TimeflowShortcut JumpToMarker9 = new TimeflowShortcut("Markers", "Jump to Marker 1-9", Path_JumpToMarker9);
        public static readonly TimeflowShortcut JumpToFullDuration = new TimeflowShortcut("Markers", "Full Duration", Path_JumpToFullDuration, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region FIELDS

        public const string Path_AddBoolField = "Timeflow/Add Field: Bool";
        public const string Path_AddColorField = "Timeflow/Add Field: Color";
        public const string Path_AddComponentField = "Timeflow/Add Field: Component";
        public const string Path_AddFloatField = "Timeflow/Add Field: Float";
        public const string Path_AddGameObjectField = "Timeflow/Add Field: GameObject";
        public const string Path_AddRectField = "Timeflow/Add Field: Rect";
        public const string Path_AddStringField = "Timeflow/Add Field: String";
        public const string Path_AddVector2Field = "Timeflow/Add Field: Vector2";
        public const string Path_AddVector3Field = "Timeflow/Add Field: Vector3";
        public const string Path_AddVector4Field = "Timeflow/Add Field: Vector4";

        public static readonly TimeflowShortcut AddBoolField = new TimeflowShortcut("Fields", "Add Bool Field", Path_AddBoolField);
        public static readonly TimeflowShortcut AddColorField = new TimeflowShortcut("Fields", "Add Color Field", Path_AddColorField);
        public static readonly TimeflowShortcut AddComponentField = new TimeflowShortcut("Fields", "Add Component Field", Path_AddComponentField);
        public static readonly TimeflowShortcut AddFloatField = new TimeflowShortcut("Fields", "Add Float Field", Path_AddFloatField);
        public static readonly TimeflowShortcut AddGameObjectField = new TimeflowShortcut("Fields", "Add GameObject Field", Path_AddGameObjectField);
        public static readonly TimeflowShortcut AddRectField = new TimeflowShortcut("Fields", "Add Rect Field", Path_AddRectField);
        public static readonly TimeflowShortcut AddStringField = new TimeflowShortcut("Fields", "Add String Field", Path_AddStringField);
        public static readonly TimeflowShortcut AddVector2Field = new TimeflowShortcut("Fields", "Add Vector2 Field", Path_AddVector2Field);
        public static readonly TimeflowShortcut AddVector3Field = new TimeflowShortcut("Fields", "Add Vector3 Field", Path_AddVector3Field);
        public static readonly TimeflowShortcut AddVector4Field = new TimeflowShortcut("Fields", "Add Vector4 Field", Path_AddVector4Field);

        #endregion

        #region QUICK SELECT

        public const string Path_QuickSelectObject1 = "Timeflow/Quick Select Object 1";
        public const string Path_QuickSelectObject2 = "Timeflow/Quick Select Object 2";
        public const string Path_QuickSelectObject3 = "Timeflow/Quick Select Object 3";
        public const string Path_QuickSelectObject4 = "Timeflow/Quick Select Object 4";
        public const string Path_QuickSelectObject5 = "Timeflow/Quick Select Object 5";
        public const string Path_QuickSelectObject6 = "Timeflow/Quick Select Object 6";
        public const string Path_QuickSelectObject7 = "Timeflow/Quick Select Object 7";
        public const string Path_QuickSelectObject8 = "Timeflow/Quick Select Object 8";
        public const string Path_QuickSelectObject9 = "Timeflow/Quick Select Object 9";
        public const string Path_QuickSelectObject10 = "Timeflow/Quick Select Object 10";
        public const string Path_QuickSelectObject11 = "Timeflow/Quick Select Object 11";
        public const string Path_QuickSelectObject12 = "Timeflow/Quick Select Object 12";

        public static readonly TimeflowShortcut QuickSelectObject1 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject1);
        public static readonly TimeflowShortcut QuickSelectObject2 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject2);
        public static readonly TimeflowShortcut QuickSelectObject3 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject3);
        public static readonly TimeflowShortcut QuickSelectObject4 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject4);
        public static readonly TimeflowShortcut QuickSelectObject5 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject5);
        public static readonly TimeflowShortcut QuickSelectObject6 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject6);
        public static readonly TimeflowShortcut QuickSelectObject7 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject7);
        public static readonly TimeflowShortcut QuickSelectObject8 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject8);
        public static readonly TimeflowShortcut QuickSelectObject9 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject9);
        public static readonly TimeflowShortcut QuickSelectObject10 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject10);
        public static readonly TimeflowShortcut QuickSelectObject11 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject11);
        public static readonly TimeflowShortcut QuickSelectObject12 = new TimeflowShortcut("Quick Select", "Select Object 1-12", Path_QuickSelectObject12);

        public const string Path_QuickSelectAssignObject1 = "Timeflow/Quick Select Object Assign 1";
        public const string Path_QuickSelectAssignObject2 = "Timeflow/Quick Select Object Assign 2";
        public const string Path_QuickSelectAssignObject3 = "Timeflow/Quick Select Object Assign 3";
        public const string Path_QuickSelectAssignObject4 = "Timeflow/Quick Select Object Assign 4";
        public const string Path_QuickSelectAssignObject5 = "Timeflow/Quick Select Object Assign 5";
        public const string Path_QuickSelectAssignObject6 = "Timeflow/Quick Select Object Assign 6";
        public const string Path_QuickSelectAssignObject7 = "Timeflow/Quick Select Object Assign 7";
        public const string Path_QuickSelectAssignObject8 = "Timeflow/Quick Select Object Assign 8";
        public const string Path_QuickSelectAssignObject9 = "Timeflow/Quick Select Object Assign 9";
        public const string Path_QuickSelectAssignObject10 = "Timeflow/Quick Select Object Assign 10";
        public const string Path_QuickSelectAssignObject11 = "Timeflow/Quick Select Object Assign 11";
        public const string Path_QuickSelectAssignObject12 = "Timeflow/Quick Select Object Assign 12";

        public static readonly TimeflowShortcut QuickSelectAssignObject1 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject1);
        public static readonly TimeflowShortcut QuickSelectAssignObject2 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject2);
        public static readonly TimeflowShortcut QuickSelectAssignObject3 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject3);
        public static readonly TimeflowShortcut QuickSelectAssignObject4 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject4);
        public static readonly TimeflowShortcut QuickSelectAssignObject5 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject5);
        public static readonly TimeflowShortcut QuickSelectAssignObject6 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject6);
        public static readonly TimeflowShortcut QuickSelectAssignObject7 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject7);
        public static readonly TimeflowShortcut QuickSelectAssignObject8 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject8);
        public static readonly TimeflowShortcut QuickSelectAssignObject9 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject9);
        public static readonly TimeflowShortcut QuickSelectAssignObject10 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject10);
        public static readonly TimeflowShortcut QuickSelectAssignObject11 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject11);
        public static readonly TimeflowShortcut QuickSelectAssignObject12 = new TimeflowShortcut("Quick Select", "Assign Object 1-12", Path_QuickSelectAssignObject12);

        #endregion

        #region SELECTION

        public const string Path_DeselectAll = "Timeflow/View: Deselect All";
        public const string Path_SelectAll = "Timeflow/View: Select All";
        public const string Path_GrowSelection = "Timeflow/View: Grow Selection";
        public const string Path_DiscreetSelection = "Timeflow/View: Discreet Selection";
        public const string Path_SelectNextPrevious = "Timeflow/Selection: Select Next/Previous";
        public const string Path_DuplicateSelected = "Timeflow/Selection: Duplicate";
        public const string Path_SelectMainCamera = "Timeflow/Game Object: Select Main Camera";

        public static readonly TimeflowShortcut DeselectAll = new TimeflowShortcut("Selection", "Deselect All", Path_DeselectAll);
        public static readonly TimeflowShortcut SelectAll = new TimeflowShortcut("Selection", "Select All", Path_SelectAll, TimeflowShortcutBindings.SelectAll);
        public static readonly TimeflowShortcut GrowSelection = new TimeflowShortcut("Selection", "Grow Selection", Path_GrowSelection, "Shift", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut DiscreetSelection = new TimeflowShortcut("Selection", "Discreet Selection", Path_DiscreetSelection, "Control", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut SelectNextPrevious = new TimeflowShortcut("Selection", "Select Next/Previous", Path_SelectNextPrevious, "Up/Down Arrow", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut DuplicateSelected = new TimeflowShortcut("Selection", "Duplicate Selected", Path_DuplicateSelected, "Control + D", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut MainCamera = new TimeflowShortcut("Selection", "Select Main Camera", Path_SelectMainCamera);

        #endregion

        #region PAN & SCROLL

        public const string Path_ZoomTime = "Timeflow/View: Zoom Time";
        public const string Path_PanTime = "Timeflow/View: Pan Time";
        public const string Path_PanView = "Timeflow/View: Pan View";
        public const string Path_ScrollZoomIn = "Timeflow/View: Scroll Zoom In";
        public const string Path_ScrollZoomInAlternate = "Timeflow/View: Scroll Zoom In (Alternate)";
        public const string Path_ScrollZoomOut = "Timeflow/View: Scroll Zoom Out";
        public const string Path_ScrollZoomOutAlternate = "Timeflow/View: Scroll Zoom Out (Alternate)";
        public const string Path_ScrollZoomToggle = "Timeflow/View: Scroll Zoom Toggle";
        public const string Path_ZoomGraphVertically = "Timeflow/View: Zoom Graph Vertically";

        public static readonly TimeflowShortcut ZoomTime = new TimeflowShortcut("Scroll & Pan", "Zoom Time", Path_ZoomTime, "Scroll", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut PanTime = new TimeflowShortcut("Scroll & Pan", "Pan Time", Path_PanTime, "Shift + Scroll", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut PanView = new TimeflowShortcut("Scroll & Pan", "Pan View", Path_PanView, "Alt + Drag", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ScrollZoomIn = new TimeflowShortcut("Scroll & Pan", "Scroll Zoom In", Path_ScrollZoomIn, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ScrollZoomInAlternate = new TimeflowShortcut("Scroll & Pan", "Scroll Zoom In", Path_ScrollZoomInAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ScrollZoomOut = new TimeflowShortcut("Scroll & Pan", "Scroll Zoom Out", Path_ScrollZoomOut, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ScrollZoomOutAlternate = new TimeflowShortcut("Scroll & Pan", "Scroll Zoom Out", Path_ScrollZoomOutAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ScrollZoomToggle = new TimeflowShortcut("Scroll & Pan", "Scroll Zoom Toggle", Path_ScrollZoomToggle, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ZoomGraphVertically = new TimeflowShortcut("Scroll & Pan", "Zoom Graph Vertically", Path_ZoomGraphVertically, "Control + Alt + Scroll", TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region WORK AREA

        public const string Path_ToggleWorkArea = "Timeflow/View: Toggle Work Area";
        public const string Path_SetWorkAreaStart = "Timeflow/View: Set Work Area Start";
        public const string Path_SetWorkAreaEnd = "Timeflow/View: Set Work Area End";
        public const string Path_SetWorkAreaToSelected = "Timeflow/View: Set Work Area to Selected";
        public const string Path_SetWorkAreaStartKeepDuration = "Timeflow/View: Set Work Area Start (Keep Duration)";
        public const string Path_SetWorkAreaEndKeepDuration = "Timeflow/View: Set Work Area End (Keep Duration)";

        public static readonly TimeflowShortcut ToggleWorkArea = new TimeflowShortcut("Work Area", "Toggle", Path_ToggleWorkArea, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetWorkAreaStart = new TimeflowShortcut("Work Area", "Set Start", Path_SetWorkAreaStart, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetWorkAreaEnd = new TimeflowShortcut("Work Area", "Set End", Path_SetWorkAreaEnd, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetWorkAreaToSelected = new TimeflowShortcut("Work Area", "Set to Selected", Path_SetWorkAreaToSelected, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetWorkAreaStartKeepDuration = new TimeflowShortcut("Work Area", "Set Start (Keep Duration)", Path_SetWorkAreaStartKeepDuration, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetWorkAreaEndKeepDuration = new TimeflowShortcut("Work Area", "Set End (Keep Duration)", Path_SetWorkAreaEndKeepDuration, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region COPY & PASTE 

        public const string Path_CopySelection = "Timeflow/View: Copy Keyframes";
        public const string Path_CutSelection = "Timeflow/View: Cut Keyframes";
        public const string Path_PasteAtCurrentTime = "Timeflow/View: Paste Keyframes (At Current Time)";
        public const string Path_PastePreserveTime = "Timeflow/View: Paste Keyframes (Preserve Time)";
        public const string Path_DuplicateSelection = "Timeflow/View: Duplicate Selected";
        public const string Path_PasteTangentsOnly = "Timeflow/View: Paste Keyframe Tangents Only";

        public static readonly TimeflowShortcut CopySelection = new TimeflowShortcut("Copy & Paste", "Copy Selection", Path_CopySelection, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut CutSelection = new TimeflowShortcut("Copy & Paste", "Cut Selection", Path_CutSelection, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut PasteAtCurrentTime = new TimeflowShortcut("Copy & Paste", "Paste (At Current Time)", Path_PasteAtCurrentTime, TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut PastePreserveTime = new TimeflowShortcut("Copy & Paste", "Paste (Preserve Time)", Path_PastePreserveTime, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut DuplicateSelectionDrag = new TimeflowShortcut("Copy & Paste", "Duplicate Drag", Path_DuplicateSelection, "Control", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut PasteTangentsOnly = new TimeflowShortcut("Copy & Paste", "Paste Tangents Only", Path_PasteTangentsOnly, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region DISPLAY

        public const string Path_DisplayNothing = "Timeflow/Display: Nothing";
        public const string Path_DisplayEverything = "Timeflow/Display: Everything";
        public const string Path_DisplaySelectedOnly = "Timeflow/Display: Selected Only";
        public const string Path_DisplaySelectedObject = "Timeflow/Display: Selected Object";
        public const string Path_AddSelectedToView = "Timeflow/Display: Add Selected to View";
        public const string Path_ToggleHidden = "Timeflow/Display: Toggle Hidden";
        public const string Path_ActiveSelectionGrouped = "Timeflow/Display: Active Selection (Grouped)";
        public const string Path_DisplayNext = "Timeflow/Display: Next";
        public const string Path_DisplayPrevious = "Timeflow/Display: Previous";
        public const string Path_SoloSelected = "Timeflow/Display: Solo Selected";
        public const string Path_SoloSelectedAppend = "Timeflow/Display: Solo Selected (Append)";

        public static readonly TimeflowShortcut DisplayNothing = new TimeflowShortcut("Display Controls", "Display Nothing", Path_DisplayNothing);
        public static readonly TimeflowShortcut DisplayEverything = new TimeflowShortcut("Display Controls", "Display Everything", Path_DisplayEverything);
        public static readonly TimeflowShortcut DisplaySelectedOnly = new TimeflowShortcut("Display Controls", "Display Selected Only", Path_DisplaySelectedOnly);
        public static readonly TimeflowShortcut DisplaySelectedObject = new TimeflowShortcut("Display Controls", "Display Selected Object", Path_DisplaySelectedObject);
        public static readonly TimeflowShortcut AddSelectedToView = new TimeflowShortcut("Display Controls", "Add Selected to View", Path_AddSelectedToView);
        public static readonly TimeflowShortcut ToggleHidden = new TimeflowShortcut("Display Controls", "Toggle Hidden", Path_ToggleHidden);
        public static readonly TimeflowShortcut ActiveSelectionGrouped = new TimeflowShortcut("Display Controls", "Active Selection (Grouped)", Path_ActiveSelectionGrouped);
        public static readonly TimeflowShortcut DisplayNext = new TimeflowShortcut("Display Controls", "Display Next", Path_DisplayNext);
        public static readonly TimeflowShortcut DisplayPrevious = new TimeflowShortcut("Display Controls", "Display Previous", Path_DisplayPrevious);
        public static readonly TimeflowShortcut SoloSelected = new TimeflowShortcut("Display Controls", "Solo Selected", Path_SoloSelected);
        public static readonly TimeflowShortcut SoloSelectedAppend = new TimeflowShortcut("Display Controls", "Solo Selected (Append)", Path_SoloSelectedAppend);

        #endregion

        #region HIERARCHY

        public const string Path_DestroyAllTimeflowBehaviors = "Timeflow/Hierarchy: Destroy All Timeflow Behaviors";
        public const string Path_DeleteChildren = "Timeflow/Hierarchy: Delete Children";
        public const string Path_SortChildren = "Timeflow/Hierarchy: Sort Children";
        public const string Path_SortChildrenReverse = "Timeflow/Hierarchy: Sort Children Reverse";
        public const string Path_HideChildrenInHierarchy = "Timeflow/Hierarchy: Hide Children in Hierarchy";
        public const string Path_ShowChildrenInHierarchy = "Timeflow/Hierarchy: Show Children in Hierarchy";
        public const string Path_Group = "Timeflow/Hierarchy: Group";
        public const string Path_Ungroup = "Timeflow/Hierarchy: Ungroup";
        public const string Path_Unparent = "Timeflow/Hierarchy: Unparent";
        public const string Path_Flatten = "Timeflow/Hierarchy: Flatten";
        public const string Path_RemoveNumbering = "Timeflow/Hierarchy: Remove Numbering";

        public static readonly TimeflowShortcut DestroyAllTimeflowBehaviors = new TimeflowShortcut("Hierarchy", "Destroy All Timeflow Behaviors", Path_DestroyAllTimeflowBehaviors);
        public static readonly TimeflowShortcut DeleteChildren = new TimeflowShortcut("Hierarchy", "Delete Children", Path_DeleteChildren);
        public static readonly TimeflowShortcut SortChildren = new TimeflowShortcut("Hierarchy", "Sort Children", Path_SortChildren);
        public static readonly TimeflowShortcut SortChildrenReverse = new TimeflowShortcut("Hierarchy", "Sort Children Reverse", Path_SortChildrenReverse);
        public static readonly TimeflowShortcut HideChildrenInHierarchy = new TimeflowShortcut("Hierarchy", "Hide Children in Hierarchy", Path_HideChildrenInHierarchy);
        public static readonly TimeflowShortcut ShowChildrenInHierarchy = new TimeflowShortcut("Hierarchy", "Show Children in Hierarchy", Path_ShowChildrenInHierarchy);
        public static readonly TimeflowShortcut Group = new TimeflowShortcut("Hierarchy", "Group", Path_Group);
        public static readonly TimeflowShortcut Ungroup = new TimeflowShortcut("Hierarchy", "Ungroup", Path_Ungroup);
        public static readonly TimeflowShortcut Unparent = new TimeflowShortcut("Hierarchy", "Unparent", Path_Unparent);
        public static readonly TimeflowShortcut Flatten = new TimeflowShortcut("Hierarchy", "Flatten", Path_Flatten);
        public static readonly TimeflowShortcut RemoveNumbering = new TimeflowShortcut("Hierarchy", "Remove Numbering", Path_RemoveNumbering);

        #endregion

        #region GAME OBJECT

        public const string Path_Activate = "Timeflow/Game Object: Activate";
        public const string Path_Deactivate = "Timeflow/Game Object: Deactivate";
        public const string Path_ActivateRecursive = "Timeflow/Game Object: Activate Recursively";
        public const string Path_DeactivateRecursive = "Timeflow/Game Object: Deactivate Recursively";
        public const string Path_EnableRenderersRecursive = "Timeflow/Game Object: Enable Renderers Recursively";
        public const string Path_DisableRenderersRecursive = "Timeflow/Game Object: Disable Renderers Recursively";
        public const string Path_RenameSelectedObject = "Timeflow/View: Rename Selected Object or Channel";

        public static readonly TimeflowShortcut Activate = new TimeflowShortcut("Game Object", "Activate", Path_Activate);
        public static readonly TimeflowShortcut Deactivate = new TimeflowShortcut("Game Object", "Deactivate", Path_Deactivate);
        public static readonly TimeflowShortcut ActivateRecursive = new TimeflowShortcut("Game Object", "Activate Recursively", Path_ActivateRecursive);
        public static readonly TimeflowShortcut DeactivateRecursive = new TimeflowShortcut("Game Object", "Deactivate Recursively", Path_DeactivateRecursive);
        public static readonly TimeflowShortcut EnableRenderersRecursive = new TimeflowShortcut("Game Object", "Enable Renderers Recursively", Path_EnableRenderersRecursive);
        public static readonly TimeflowShortcut DisableRenderersRecursively = new TimeflowShortcut("Game Object", "Disable Renderers Recursively", Path_DisableRenderersRecursive);
        public static readonly TimeflowShortcut RenameSelectedObject = new TimeflowShortcut("Game Object", "Rename Object or Channel", Path_RenameSelectedObject);

        public const string Path_GetRendererSize = "Timeflow/Game Object: Get Renderer Size";
        public const string Path_GetBoundingBox = "Timeflow/Game Object: Get Bounding Box";
        public const string Path_GetPolycount = "Timeflow/Game Object: Get Polycount";
        public const string Path_FreezeMesh = "Timeflow/Game Object: Freeze Mesh";
        public const string Path_CombineMeshes = "Timeflow/Game Object: Combine Meshes";
        public const string Path_SelectChildren = "Timeflow/Game Object: Select Children";
        public const string Path_SelectDescendants = "Timeflow/Game Object: Select Descendants";
        public const string Path_SelectParents = "Timeflow/Game Object: Select Parents";
        public const string Path_SelectAncestors = "Timeflow/Game Object: Select Ancestors";
        public const string Path_SelectRenderersRecursive = "Timeflow/Game Object: Select Renderers Recursive";

        public static readonly TimeflowShortcut GetRendererSize = new TimeflowShortcut("Game Object", "Get Renderer Size", Path_GetRendererSize);
        public static readonly TimeflowShortcut GetBoundingBox = new TimeflowShortcut("Game Object", "Get Bounding Box", Path_GetBoundingBox);
        public static readonly TimeflowShortcut GetPolycount = new TimeflowShortcut("Game Object", "Get Polycount", Path_GetPolycount);
        public static readonly TimeflowShortcut FreezeMesh = new TimeflowShortcut("Game Object", "Freeze Mesh", Path_FreezeMesh);
        public static readonly TimeflowShortcut CombineMeshes = new TimeflowShortcut("Game Object", "Combine Meshes", Path_CombineMeshes);
        public static readonly TimeflowShortcut SelectChildren = new TimeflowShortcut("Game Object", "Select Children", Path_SelectChildren);
        public static readonly TimeflowShortcut SelectDescendants = new TimeflowShortcut("Game Object", "Select Descendants", Path_SelectDescendants);
        public static readonly TimeflowShortcut SelectParents = new TimeflowShortcut("Game Object", "Select Parents", Path_SelectParents);
        public static readonly TimeflowShortcut SelectAncestors = new TimeflowShortcut("Game Object", "Select Ancestors", Path_SelectAncestors);
        public static readonly TimeflowShortcut SelectRenderersRecursive = new TimeflowShortcut("Game Object", "Select Renderers Recursive", Path_SelectRenderersRecursive);

        #endregion

        #region PREFABS & PRECOMPOSE

        public const string Path_Precompose = "Timeflow/Precompose";
        public const string Path_Decompose = "Timeflow/Decompose";
        public const string Path_SaveSelectedPrefabs = "Timeflow/Game Object: Save Selected Prefabs";
        public const string Path_EnterPrefabPrecompEditMode = "Timeflow/Game Object: Enter Prefab-Precomp Edit Mode";
        public const string Path_ExitPrefabPrecompEditMode = "Timeflow/Game Object: Exit Prefab-Precomp Edit Mode";

        public static readonly TimeflowShortcut PrecomposeAddPrecomp = new TimeflowShortcut("Prefabs & Precomps", "Precompose / Add Precomp", Path_Precompose);
        public static readonly TimeflowShortcut SaveSelectedPrefabs = new TimeflowShortcut("Prefabs & Precomps", "Save Selected Prefabs", Path_SaveSelectedPrefabs);
        public static readonly TimeflowShortcut EnterEditMode = new TimeflowShortcut("Prefabs & Precomps", "Enter Edit Mode", Path_EnterPrefabPrecompEditMode);
        public static readonly TimeflowShortcut ExitEditMode = new TimeflowShortcut("Prefabs & Precomps", "Exit Edit Mode", Path_ExitPrefabPrecompEditMode);

        #endregion

        #region ADD BEHAVIOR

        public const string Path_AddBehaviorBlend = "Timeflow/Add Behavior: Blend";
        public const string Path_AddBehaviorFlyby = "Timeflow/Add Behavior: Flyby";
        public const string Path_AddBehaviorMotionPath = "Timeflow/Add Behavior: Motion Path";
        public const string Path_AddBehaviorTween = "Timeflow/Add Behavior: Tween";

        public static readonly TimeflowShortcut AddBehaviorBlend = new TimeflowShortcut("Add Behavior", "Blend", Path_AddBehaviorBlend);
        public static readonly TimeflowShortcut AddBehaviorFlyby = new TimeflowShortcut("Add Behavior", "Flyby", Path_AddBehaviorFlyby);
        public static readonly TimeflowShortcut AddBehaviorMotionPath = new TimeflowShortcut("Add Behavior", "Motion Path", Path_AddBehaviorMotionPath);
        public static readonly TimeflowShortcut AddBehaviorTween = new TimeflowShortcut("Add Behavior", "Tween", Path_AddBehaviorTween);

        #endregion

        #region TRANSFORM

        public const string Path_TransformReset = "Timeflow/Transform: Reset";
        public const string Path_TransformCopy = "Timeflow/Transform: Copy";
        public const string Path_TransformPaste = "Timeflow/Transform: Paste";
        public const string Path_TransformPasteResetScale = "Timeflow/Transform: Paste Transform Reset Scale";
        public const string Path_TransformPastePositionOnly = "Timeflow/Transform: Paste Position Only";

        public static readonly TimeflowShortcut TransformReset = new TimeflowShortcut("Transform", "Reset", Path_TransformReset, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TransformCopy = new TimeflowShortcut("Transform", "Copy", Path_TransformCopy, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TransformPaste = new TimeflowShortcut("Transform", "Paste", Path_TransformPaste, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TransformPasteResetScale = new TimeflowShortcut("Transform", "Paste Transform Reset Scale", Path_TransformPasteResetScale, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TransformPastePositionOnly = new TimeflowShortcut("Transform", "Paste Position Only", Path_TransformPastePositionOnly, TimeflowShortcut.ShortcutTypes.View);


        #endregion

        #region DRAG & ADJUST

        public const string Path_CancelDrag = "Timeflow/View: Cancel Drag";
        public const string Path_ConstrainDrag = "Timeflow/View: Constrain Drag";
        public const string Path_DuplicateSelectionDrag = "Timeflow/View: Drag Duplicate";
        public const string Path_MicroKeyframeAdjustment = "Timeflow/View: Micro Keyframe Adjust";
        public const string Path_SnapToGrid = "Timeflow/View: Snap To Grid";

        public static readonly TimeflowShortcut CancelDrag = new TimeflowShortcut("Drag Modifiers", "Cancel Drag", Path_CancelDrag, "Escape", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut ConstrainDrag = new TimeflowShortcut("Drag Modifiers", "Constrain Drag", Path_ConstrainDrag, "Shift + Drag", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut DuplicateSelection = new TimeflowShortcut("Drag Modifiers", "Duplicate Selection", Path_DuplicateSelectionDrag, "Control + Drag", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut MicroKeyframeAdjustment = new TimeflowShortcut("Drag Modifiers", "Micro Keyframe Adjustment", Path_MicroKeyframeAdjustment, "Control + Alt + Shift + Drag", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SnapToGrid = new TimeflowShortcut("Drag Modifiers", "Snap to Grid", Path_SnapToGrid, "Alt + Drag Selection", TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region GO TO

        public const string Path_GoToStart = "Timeflow/Go to Start";
        public const string Path_GoToEnd = "Timeflow/Go to End";
        public const string Path_GoToStartGlobal = "Timeflow/Go to Start (Global)";
        public const string Path_GoToEndGlobal = "Timeflow/Go to End (Global)";
        public const string Path_GoToStartOfSelection = "Timeflow/View: Go to Start of Selection";
        public const string Path_GoToEndOfSelection = "Timeflow/View: Go to End of Selection";
        public const string Path_ToggleLocalTimeScope = "Timeflow/View: Toggle Local Time Scope";

        public static readonly TimeflowShortcut GoToStart = new TimeflowShortcut("Go to Time", "Start", Path_GoToStart, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToEnd = new TimeflowShortcut("Go to Time", "End", Path_GoToEnd, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToStartGlobal = new TimeflowShortcut("Go to Time", "Start (Global)", Path_GoToStartGlobal, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToEndGlobal = new TimeflowShortcut("Go to Time", "End (Global)", Path_GoToEndGlobal, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToStartOfSelection = new TimeflowShortcut("Go to Time", "Start of Selection", Path_GoToStartOfSelection, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToEndOfSelection = new TimeflowShortcut("Go to Time", "End of Selection", Path_GoToEndOfSelection, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TimeScope = new TimeflowShortcut("Go to Time", "Time Scope", Path_ToggleLocalTimeScope, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region CHANNELS

        public const string Path_DecreaseSelectedChannelHeights = "Timeflow/Decrease Selected Channel Heights";
        public const string Path_IncreaseSelectedChannelHeights = "Timeflow/Increase Selected Channel Heights";
        public const string Path_ToggleLockSelectedChannelHeights = "Timeflow/Toggle Lock Selected Channel Heights";
        public const string Path_RemoveChannelLink = "Timeflow/Remove Channel Link";
        public const string Path_ToggleChannelLink = "Timeflow/Toggle Channel Link";
        public const string Path_SelectChannels = "Timeflow/Game Object: Select Channels";

        public static readonly TimeflowShortcut DecreaseHeight = new TimeflowShortcut("Channels", "Decrease Height", Path_DecreaseSelectedChannelHeights, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut IncreaseHeight = new TimeflowShortcut("Channels", "Increase Height", Path_IncreaseSelectedChannelHeights, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ToggleHeightLock = new TimeflowShortcut("Channels", "Toggle Height Lock", Path_ToggleLockSelectedChannelHeights, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut RemoveChannelLink = new TimeflowShortcut("Channels", "Remove Channel Link", Path_RemoveChannelLink, "Control + Click", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut ToggleChannelLink = new TimeflowShortcut("Channels", "Toggle Channel Link", Path_ToggleChannelLink, "Alt + Click", TimeflowShortcut.ShortcutTypes.Builtin);
        public static readonly TimeflowShortcut SelectChannels = new TimeflowShortcut("Channels", "Select Channels", Path_SelectChannels, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region TRACK & GRAPH VIEW

        public const string Path_ToggleGraphTrackMode = "Timeflow/View: Toggle Graph-Track Mode";
        public const string Path_Fit = "Timeflow/View: Fit";
        public const string Path_FitGraphAuto = "Timeflow/View: Fit Graph (Auto)";
        public const string Path_FitTimeOnly = "Timeflow/View: Fit Time Only";
        public const string Path_SetTimeScope = "Timeflow/Set Time Scope";
        public const string Path_ToggleGraphLock = "Timeflow/View: Lock Graph";

        public static readonly TimeflowShortcut GraphTrack = new TimeflowShortcut("Track & Graph View", "Graph/Track", Path_ToggleGraphTrackMode, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut Fit = new TimeflowShortcut("Track & Graph View", "Fit", Path_Fit, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut FitGraphAuto = new TimeflowShortcut("Track & Graph View", "Fit Graph (Auto)", Path_FitGraphAuto, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut FitTimeOnly = new TimeflowShortcut("Track & Graph View", "Fit Time Only", Path_FitTimeOnly, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetTimeScope = new TimeflowShortcut("Track & Graph View", "Set Time Scope", Path_SetTimeScope, "Double-Click Selection", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut LockGraph = new TimeflowShortcut("Track & Graph View", "Lock Graph", Path_ToggleGraphLock, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region GRID & SNAP

        public const string Path_ToggleGrid = "Timeflow/View: Toggle Grid";
        public const string Path_DecreaseGrid = "Timeflow/View: Decrease Grid";
        public const string Path_DecreaseGridAlternate = "Timeflow/View: Decrease Grid (Alternate)";
        public const string Path_IncreaseGrid = "Timeflow/View: Increase Grid";
        public const string Path_IncreaseGridAlternate = "Timeflow/View: Increase Grid (Alternate)";
        public const string Path_SnapTimesOfSelectedKeyframesQuantize = "Timeflow/View: Snap Times of Selected Keyframes (Quantize)";
        public const string Path_SnapValuesOfSelectedKeyframesQuantize = "Timeflow/View: Snap Values of Selected Keyframes (Quantize)";
        public const string Path_SnapTime = "Timeflow/View: Snap Time";
        public const string Path_SnapValue = "Timeflow/View: Snap Value";

        public static readonly TimeflowShortcut ToggleGrid = new TimeflowShortcut("Grid & Snap", "Toggle Grid", Path_ToggleGrid, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut DecreaseGrid = new TimeflowShortcut("Grid & Snap", "Decrease Grid", Path_DecreaseGrid, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut DecreaseGridAlternate = new TimeflowShortcut("Grid & Snap", "Decrease Grid", Path_DecreaseGridAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut IncreaseGrid = new TimeflowShortcut("Grid & Snap", "Increase Grid", Path_IncreaseGrid, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut IncreaseGridAlternate = new TimeflowShortcut("Grid & Snap", "Increase Grid", Path_IncreaseGridAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SnapTimesQuantize = new TimeflowShortcut("Grid & Snap", "Snap Times (Quantize)", Path_SnapTimesOfSelectedKeyframesQuantize, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SnapValuesQuantize = new TimeflowShortcut("Grid & Snap", "Snap Values (Quantize)", Path_SnapValuesOfSelectedKeyframesQuantize, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ToggleSnapTime = new TimeflowShortcut("Grid & Snap", "Toggle Snap Time", Path_SnapTime, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ToggleSnapValue = new TimeflowShortcut("Grid & Snap", "Toggle Snap Value", Path_SnapValue, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region GO TO NEXT/PREV

        public const string Path_GoToPreviousFrame = "Timeflow/Go to Previous Frame";
        public const string Path_GoToNextFrame = "Timeflow/Go to Next Frame";
        public const string Path_GoToPreviousKeyframe = "Timeflow/Go to Previous Keyframe";
        public const string Path_GoToNextKeyframe = "Timeflow/Go to Next Keyframe";
        public const string Path_GoToPreviousSnapTime = "Timeflow/Go to Previous Snap Time";
        public const string Path_GoToNextSnapTime = "Timeflow/Go to Next Snap Time";
        public const string Path_GoToPreviousCustomStep = "Timeflow/Go to Previous Custom Step";
        public const string Path_GoToNextCustomStep = "Timeflow/Go to Next Custom Step";
        public const string Path_GoToNextMarker = "Timeflow/Go to Next Marker";
        public const string Path_GoToPreviousMarker = "Timeflow/Go to Previous Marker";

        public static readonly TimeflowShortcut GoToPreviousFrame = new TimeflowShortcut("Go to ...", "Previous Frame", Path_GoToPreviousFrame, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToNextFrame = new TimeflowShortcut("Go to ...", "Next Frame", Path_GoToNextFrame, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToPreviousKeyframe = new TimeflowShortcut("Go to ...", "Previous Keyframe", Path_GoToPreviousKeyframe, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToNextKeyframe = new TimeflowShortcut("Go to ...", "Next Keyframe", Path_GoToNextKeyframe, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToPreviousSnapTime = new TimeflowShortcut("Go to ...", "Previous Snap Time", Path_GoToPreviousSnapTime, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToNextSnapTime = new TimeflowShortcut("Go to ...", "Next Snap Time", Path_GoToNextSnapTime, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToPreviousCustomStep = new TimeflowShortcut("Go to ...", "Previous Snap Time", Path_GoToPreviousCustomStep, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToNextCustomStep = new TimeflowShortcut("Go to ...", "Next Snap Time", Path_GoToNextCustomStep, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToNextMarker = new TimeflowShortcut("Go to ...", "Next Marker", Path_GoToNextMarker, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut GoToPreviousMarker = new TimeflowShortcut("Go to ...", "Previous Marker", Path_GoToPreviousMarker, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region TRACKS

        public const string Path_ResetTracksSelected = "Timeflow/Tracks: Reset Tracks (Selected Objects)";
        public const string Path_ResetAllTracks = "Timeflow/Tracks: Reset All Tracks In Scene";
        public const string Path_JoinAdjacentTracks = "Timeflow/Tracks: Join Adjacent Tracks (Selected Objects)";

        public static readonly TimeflowShortcut ResetTracksSelected = new TimeflowShortcut("Tracks", "Reset Tracks (Selected Objects)", Path_ResetTracksSelected, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ResetAllTracks = new TimeflowShortcut("Tracks", "Reset All Tracks In Scene", Path_ResetAllTracks, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut JoinAdjacentTracks = new TimeflowShortcut("Tracks", "Join Adjacent Tracks (Selected Objects)", Path_JoinAdjacentTracks, TimeflowShortcut.ShortcutTypes.View);

        public const string Path_SetStartOfSelectedTracks = "Timeflow/View: Set Start of Selected Tracks";
        public const string Path_SetEndOfSelectedTracks = "Timeflow/View: Set End of Selected Tracks";
        public const string Path_SetSelectedTracksToWorkArea = "Timeflow/View: Set Selected Tracks to Work Area";
        public const string Path_JoinSelectedTracks = "Timeflow/View: Join Selected Tracks";
        public const string Path_SplitSelectedTracksAtCurrentTime = "Timeflow/View: Split Selected Tracks at Current Time";
        public const string Path_SplitSelectedTracksByWorkArea = "Timeflow/View: Split Selected Tracks by Work Area";

        public static readonly TimeflowShortcut SetStart = new TimeflowShortcut("Track Editing", "Set Start", Path_SetStartOfSelectedTracks, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetEnd = new TimeflowShortcut("Track Editing", "Set End", Path_SetEndOfSelectedTracks, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SetToWorkArea = new TimeflowShortcut("Track Editing", "Set to Work Area", Path_SetSelectedTracksToWorkArea, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut Join = new TimeflowShortcut("Track Editing", "Join", Path_JoinSelectedTracks, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SplitAtCurrentTime = new TimeflowShortcut("Track Editing", "Split at Current Time", Path_SplitSelectedTracksAtCurrentTime, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SplitByWorkArea = new TimeflowShortcut("Track Editing", "Split by Work Area", Path_SplitSelectedTracksByWorkArea, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region TRACK COLORS

        public const string Path_TrackColorsOpenPalette = "Timeflow/Track Colors/Open Color Palette";
        public const string Path_TrackColorsAssignSequential = "Timeflow/Track Colors/Assign Sequential Track Colors";
        public const string Path_TrackColorsAssignRandom = "Timeflow/Track Colors/Assign Random Track Colors";
        public const string Path_TrackColorsAssignAuto = "Timeflow/Track Colors/Assign Auto Track Colors";

        public static readonly TimeflowShortcut TrackColorsOpenPalette = new TimeflowShortcut("Track Colors", "Open Color Palette", Path_TrackColorsOpenPalette, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TrackColorsAssignSequential = new TimeflowShortcut("Track Colors", "Sequential Track Colors", Path_TrackColorsAssignSequential, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TrackColorsAssignRandom = new TimeflowShortcut("Track Colors", "Random Track Colors", Path_TrackColorsAssignRandom, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TrackColorsAssignAuto = new TimeflowShortcut("Track Colors", "Auto Track Colors", Path_TrackColorsAssignAuto, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region TRACKS & KEYFRAMES

        public const string Path_AutoKeyframing = "Timeflow/Auto Keyframing";
        public const string Path_AddKeyframe = "Timeflow/View: Add Keyframe";
        public const string Path_KeysOnlyTool = "Timeflow/View: Keys-Only Tool";
        public const string Path_TangentsTool = "Timeflow/View: Tangents Tool";

        public static readonly TimeflowShortcut AutoKeyframing = new TimeflowShortcut("Keyframing", "Auto Keyframing", Path_AutoKeyframing);
        public static readonly TimeflowShortcut AddKeyframe = new TimeflowShortcut("Keyframing", "Add Keyframe", Path_AddKeyframe, TimeflowShortcut.ShortcutTypes.Global);
        public static readonly TimeflowShortcut KeysOnlyTool = new TimeflowShortcut("Keyframing", "Keys-Only Tool", Path_KeysOnlyTool, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut TangentsTool = new TimeflowShortcut("Keyframing", "Tangents Tool", Path_TangentsTool, TimeflowShortcut.ShortcutTypes.View);

        public const string Path_InsertKeyframe = "Timeflow/View: Insert Keyframe";
        public const string Path_SelectionToggleEnabled = "Timeflow/View: Selection Toggle Enabled";
        public const string Path_SelectionToggleEnabledAlternate = "Timeflow/View: Selection Toggle Enabled (Alternate)";
        public const string Path_SelectionToggleLocked = "Timeflow/View: Selection Toggle Locked";
        public const string Path_SelectionToggleLockedAlternate = "Timeflow/View: Selection Toggle Locked (Alternate)";
        public const string Path_ToggleKeyframeBoundingBox = "Timeflow/View: Toggle Keyframe Bounding Box";

        public static readonly TimeflowShortcut Insert = new TimeflowShortcut("Tracks & Keyframes", "Insert", Path_InsertKeyframe, "Control + Click", TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SelectionToggleEnabled = new TimeflowShortcut("Tracks & Keyframes", "Toggle Enabled", Path_SelectionToggleEnabled, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SelectionToggleEnabledAlternate = new TimeflowShortcut("Tracks & Keyframes", "Toggle Enabled", Path_SelectionToggleEnabledAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SelectionToggleLocked = new TimeflowShortcut("Tracks & Keyframes", "Toggle Locked", Path_SelectionToggleLocked, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut SelectionToggleLockedAlternate = new TimeflowShortcut("Tracks & Keyframes", "Toggle Locked", Path_SelectionToggleLockedAlternate, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut BoundingBox = new TimeflowShortcut("Tracks & Keyframes", "Bounding Box", Path_ToggleKeyframeBoundingBox, TimeflowShortcut.ShortcutTypes.View);

        #endregion

        #region EDITOR

        public const string Path_EditorDebugMarkLine = "Timeflow/Editor: Debug Mark Line in Console";
        public const string Path_EditorDisableDebugAll = "Timeflow/Editor: Disable Debug For All Objects";
        public const string Path_EditorListDependencies = "Timeflow/Editor: List Dependencies";
        public const string Path_ExportShortcuts = "Timeflow/Editor: Export Shortcuts";
        public const string Path_ImportShortcuts = "Timeflow/Editor: Import Shortcuts";
        public const string Path_ResetShortcuts = "Timeflow/Editor: Reset Shortcuts to Default";
        public const string Path_OpenShortcutsManager = "Timeflow/Editor: Open Shortcuts Manager";
        public const string Path_DisableTimeflow = "Timeflow/Editor: Disable Timeflow Pro";
        public const string Path_EnableTimeflow = "Timeflow/Editor: Enable Timeflow Pro";
        public const string Path_OpenPreferences = "Timeflow/Editor: Open Preferences";

        public static readonly TimeflowShortcut EditorDebugMarkLine = new TimeflowShortcut("Editor Actions", "Debug Mark Line in Console", Path_EditorDebugMarkLine, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut EditorDisableDebugAll = new TimeflowShortcut("Editor Actions", "Disable Debug For All Objects", Path_EditorDisableDebugAll, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut EditorListDependencies = new TimeflowShortcut("Editor Actions", "List Dependencies", Path_EditorListDependencies, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ExportShortcuts = new TimeflowShortcut("Editor Actions", "Export Shortcuts", Path_ExportShortcuts, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ImportShortcuts = new TimeflowShortcut("Editor Actions", "Import Shortcuts", Path_ImportShortcuts, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut ResetShortcuts = new TimeflowShortcut("Editor Actions", "Reset Shortcuts to Default", Path_ResetShortcuts, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut OpenShortcutsManager = new TimeflowShortcut("Editor Actions", "Open Shortcuts Manager", Path_OpenShortcutsManager, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut DisableTimeflow = new TimeflowShortcut("Editor Actions", "Disable Timeflow Pro", Path_DisableTimeflow, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut EnableTimeflow = new TimeflowShortcut("Editor Actions", "Enable Timeflow Pro", Path_EnableTimeflow, TimeflowShortcut.ShortcutTypes.View);
        public static readonly TimeflowShortcut OpenPreferences = new TimeflowShortcut("Editor Actions", "Open Preferences", Path_OpenPreferences, TimeflowShortcut.ShortcutTypes.View);

        #endregion


    }
}


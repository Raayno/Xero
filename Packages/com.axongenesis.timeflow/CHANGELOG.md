---
description: Changelog for updates made to Timeflow
---

# 🔢 Version History

{% hint style="success" %}
If you are a Timeflow user, please consider writing a review in the Unity Asset Store. Your feedback matters and reviews support the continued development of the product!

[https://u3d.as/31KB](https://u3d.as/31KB)
{% endhint %}

{% hint style="info" %}
**Beta Access**

For the most recent updates please join the Timeflow Discord channel. Patches are available to all, but for beta access you must become a verified user. Instructions are provided within.

[https://discord.com/invite/sJgtnAF4Tq](https://discord.com/invite/sJgtnAF4Tq)
{% endhint %}

{% hint style="warning" %}
_**UPDATE NOTICE:**_ \
_**It is recommended to remove the previous version of Timeflow before installing an update to ensure a clean install, as file names and structures may have changed.**_
{% endhint %}

## 1.9.3 - 2025-12-14

### Fixes
* Fixed null reference error in PropertiesOfMaterial

## 1.9.2 - 2025-12-13

### Features
* All fields with +/- randomization now have a toggle to switch to min-max value ranges instead.

### Fixes
* Fixed bug with Audio Spectrum repeatedly deleting channels during setup.
* The preference Undo Time Changes now allows undo for all explicit changes to the playhead time.
* Fixed null reference in editor for FindPresetsForComponent.
* Fixed warning Animator is not playing an AnimatorController.
* Animation Sequencer now defaults to LateUpdate to fix update ordering.
* Fixed bug with Property Link ignoring selected input attribute (XYZW).

## 1.9.1 - 2025-12-01

### Features
* Added keyboard shortcut Alt+Scroll to adjust heights of the current or selected channels.
* Animation Sequencer keyframes can now be replaced by drag-and-dropping animation clips holding Alt.
* Added global preference "Force Shared Materials" to animate shared materials rather than instances.
* Added component Material Property Options to force shared materials on specific game objects only.

### Fixes
* Fixed preview when drag-and-dropping animation clips into the Timeflow view.
* When a new Animation Sequencer is added it now defaults to LateUpdate to ensure correct execution order.
* Fixed crash when drag-and-dropping animation clips from project view.
* Removed extraneous sample content.
* Adding new Animation Sequencer track matches existing channel heights.
* Fixed bug with Animation Sequencer not rebuilding when Avatar Mask is assigned.
* Fixed bug with animated material properties refreshing instances at runtime.
* Fixed Motion Path presets.
* Added Auto Select option in Motion Path editor to allow node game object selection.

## 1.9.0 - 2025-11-17

### Features
* New Animation Sequencer behavior to sequence, blend, and layer animation clips.
* New Physics Update behavior to simulate physics in edit mode.
* New Time Display behavior for an in-game display of time with various formatting options.
* Enhanced Search & Filters to find objects and channels by name and type.
* Improved Graph Lock behavior and added Solo Mode to work with selected channels only.
* Improvements to Timeflow Controller API and editor UI.

### Fixes
* Fixed bug with Loop Time Offset not always showing the first loop section in the Timeflow view.
* Improved keyframe value display in the Track view by preventing overlapping values.
* Fixed TIMEFLOW_DISABLE_MICROPHONE to fully remove all code references to Microphone.
* Fixed bug with inserted keyframes having incorrect position in the graph view.
* Fixed bug dragging and dropping prefab instance into Timeflow view.
* Fixed bug with dragging and re-ordering objects in the Timeflow view.

## 1.8.3 - 2025-09-04

### Features
* Added Loop Time Offset to loop entire objects and precomps.

### Fixes
* Fixed bug with objects being automatically reselected.
* Nested prefabs can now be opened directly from within the Timeflow view.
* Fixed cultural number parsing bug with time fields in Seconds mode. 
* Fixed issues with selecting and editing the primary time display. 
* Improved caching of the Timeflow display to avoid collection modification errors.
* Improved behavior of snapping time to keyframes and tracks.
* Fixed Enforce Start Time behavior when Time Scope is enabled.

## 1.8.2 - 2025-08-20

### Features
* Added preference to shorten labels in Transform Override editor.

### Fixes
* Fixed alignment of controls and layout in Transform Override editor.
* Fixed bug with animated post processing volume properties at runtime.
* Improved editor performance when viewing large object lists by automatically reducing GUI draw calls. 

## 1.8.1 - 2025-08-15

### Fixes
* Fixed layout of Transform Override editor.
* Added scripting define symbol TIMEFLOW_DISABLE_MICROPHONE to fully disable microphone use.

## 1.8.0 - 2025-08-12

### Features
* There is now a Timeflow Pro preference to enable enhanced menus.
 * Added icons to menus and shortcut hints.
 * The same commands can now all be accessed from the Quick Menu, Game Objects menu, and main Timeflow menu.
* New Advanced Presets system for managing and applying complex animation and behavior presets.
 * Added Advanced Presets Window with customizable display settings and palettes.
 * Samples includes a collection of standard presets for common animation setups.
 * Prefab-based definitions with per-component configuration and hierarchy support.
 * Apply presets by replacing or merging components, or instantiating new objects with optional prefab linkage.
 * Access presets from the Timeflow view with a popup menu for quick access.
 * Create your own preset collections for animation and object setups to expedite your workflow.
 * Extensible preset processing for custom script integrations.
 * Works with any component type and is not limited to Timeflow.
* Code optimizations to improve runtime performance and reduce garbage collection overhead.
 * Use the scripting define symbol AXON_LEGACY_PROPERTIES to revert to the previous property mapping method.
* Keyframes and tracks can now be assigned individual colors, overriding the color inherited from the channel.
* The Timeflow instances drop down menu now indents nested precomps to show hierarchical relationships.
* Improved timebar layout and spacing of elements.
* Added preference to display horizontal scrollbars either above or below the timeline area. 
* Added a horizontal scrollbar to expand or condense the object hierarchy.
* Improved scrollbar behavior making it easier to set in and out points.
* Added preference to set the opacity of the diagonal bar pattern on selected tracks.
* Added preferences to customize the default name for Timeflow instances and Precomps.
* Implemented Time Scale setting to control playback speed of individual channels, behaviors, and Timeflow instances.
* Added Unpack Prefab command to Timeflow context menu for convenience.
* Added Clamp setting to Tween to constrain movement between min and max, or allow overshoot if disabled.
* The work area start and end points can now be set numerically in the info panel in the Timeflow view.
* Added preference Always Number Names to append numbering to objects starting with _01

### Fixes
* Fixed alignment of marker vertical time line. 
* Fixed bug with Tween graph incorrectly displaying time offset.
* Fixed bug with precomps causing playback to pause when using Activate track mode and Auto Play enabled.
* Fixed warnings related to missing blendshapes during initialization.
* The Flyby behavior has been moved in the menus and is now under the Animation category.
* Fixed bug with rotation not being captured by auto-keyframing.

## 1.7.11 - 2025-04-05

### Fixes
* Fixed bug with hierarchy navigation using up/down arrows in the Timeflow view.
* Fixed bulk assignment of track colors to include precomp children within the view.
* Fixed bug with Timeflow start, end, and duration fields being disabled in the inspector.
* Fixed bug with audio skipping when time format is set to Frames mode and TransformOverride is enabled.

## 1.7.10 - 2025-03-16

### Features
* Code optimizations to improve runtime performance and reduce garbage collection overhead.
* Post processing property animation with auto-keyframing is now fully supported for all render pipelines.
* Track sections are now specially highlighted when their object is selected.
* Time fields now accept a comma as a dot for entering decimal time values (1,234 => 1.234) to support azerty keyboards.

### Fixes
* Fixed a bug with selection in the Timeflow view not selecting objects in the scene.
* Fixed bug with Auto Full Length track setting turning back on automatically for precomps.

## 1.7.9 - 2025-03-02

### Features
* Added Quick Menu (Shift + T) to invoke the Timeflow menu at the cursor position anywhere onscreen.

### Fixes
* Fixed slight misalignment of marker positions in time.
* Fixed bug with track shadows remaining fixed at 10 seconds. 
* Fixed null references when bulk activating/deactivating TimeflowObjects.
* Fixed bug with auto-keyframing not recording vector fields.
* Improved behavior of precomposing and decomposing.
* Fixed bug with Auto Rotate sometimes producing no rotation.

## 1.7.8 - 2025-02-18

### Features
* Look At behavior now how has configurable rotation limits.
* A Custom Grid Time Increment can now be set in the Timeflow settings.
* Added keyboard shortcuts Control + Shift + Page Up/Down to step forward or back by the Custom Time Step set in the Timeflow preferences.

### Fixes
* Updated display menu names 'Selected Group' and 'Selected Object'.
* Increased margin and cleaned up the starting edge of the timeline.
* Added 'Look At Target' to context menu 'Add Automation'.
* Fixed bug in Motion Path with Orientation not applying to Interpolate Rotation Mode.
* Fixed bug in Blend with rotation not working when switching between world objects.
* Fixed issues auto-keyframing rotations using Transform Override inspector. 
* Fixed bug with panning view both horizontally and vertically in graph view using Alt + Drag.
* Which Timeflow instance is active in the view is now saved and recalled when opening the scene.
* Fixed bug with Auto Full Length tracks not working with precomps.
* Fixed bug with auto-keyframing creating multiple channels for the same property.

## 1.7.7 - 2025-02-03

### Features
* Added preference 'Draw Minor Grid Lines' to control whether the time snap grid is displayed with subdivisions.

### Fixes
* Fixed shortcuts Alt + Home and Alt + End to go to the starting or end time, ignoring the work area.
* Fixed minor UI issues with SpineAnimator component.
* Fixed bugs with looping during play mode.
* Fixed bugs with work area and grid settings not getting saved in the scene.
* Fixed bugs with channel link and time offsets.
* Fixed bug with Tween not including the channel's time offset.

## 1.7.6 - 2025-01-27

### Features
* The keyboard shortcut K to add a keyframe can now be used globally, without the Timeflow view in focus.
* The menus Tools/Timeflow and GameObject/Timeflow now show the same commands with improved organization. 
* The 'Add Behavior' menu commands now create a new game object if no object is selected.
* Auto-Keyframing:
  * Now works with transforms in the Scene view and the built-in transform editor.
  * Works with multiple object selection, keyframing channels on each object.

### Fixes
* Flyby now uses current time and position for initial keyframe.
* Removed unnecessary dialog message when switching the Track Color Palette mode.
* Channels added by auto-keyframing now inherit the object's track color by default.
* Fixed bug with object deselection when a new auto-keyframe channel is created.
* Fixed bug setting transform values numerically when auto-keyframing is enabled.
* Fixed bug with adjusting keyframe values in the Info Panel not updating the animation.
* Fixed errors disabling Transform Override.
* Fixed issues copy-pasting keyframes to replace existing keyframes.
* Fixed bug with track shadows not updating after setting Timeflow end time.
* Fixed issues with auto-keyframing rotations.
* Removed obsolete menu item: Backup Timeflow Scripts.
* Fixed GameObject menu commands repeating for each selected object.

## 1.7.5 - 2025-01-20

### Features
* Click the Timeflow end marker to numerically enter the end time, or drag it to set interactively.
* Added preference 'Minimize Floating View To Bottom' to control minimize behavior for undocked Timeflow view.
* Add shortcut Control + Alt + Shift + W to toggle the Timeflow window minimized.
* Adding a new Timeflow or precomp now ensures a unique name using incremental numbering.
* Adding selected objects (Alt + `) to the display now creates a Timeflow instance if none exists in the scene.
* Added 'Decompose' to Game Object menu with ability to assign customized shortcut.
* Menu commands in Tools/Timeflow are now mirrored with GameObject/Timeflow for easer use.
* Up/Down Arrow keys can now be used to select channels in the Timeflow object panel, holding Shift for multiselect.

### Fixes
* Fixed stack overflow error with Add New Precomp.
* Fixed issues with opening and editing prefabs.
* Fixed duplicate naming when adding a new Timeflow to the scene.
* Fixed bug adding new Timeflow showing objects belonging to another Timeflow.
* Precompose now either precomposes selected objects, or adds a new child precomp if a Timeflow instance is selected.
* Updated and improved Timeflow layout: Assets/Plugins/Timeflow/Settings/TimeflowLayout.wlt
* Fixed null reference error when viewing a TrackColorPalette asset in the inspector. 
* Render To Disk: Fixed start and restart buttons, and ability to play and move playhead when the render is paused.
* Fixed bug with animation not updating when scrubbing playhead position in play mode.
* Fixed error 'Restored Transform child parent pointer from NULL' when undoing a Group operation.
* Fixed bug with time snap sometimes not working when dragging tracks and keyframes.

## 1.7.4 - 2025-01-13

### Features
* Added shortcut Alt + Shift + P to precompose selected objects, or add a new precomp if nothing is selected.
* Added shortcut Control + Alt + Shift + N to add a new Timeflow instance to the scene.

### Fixes
* Fixed null reference errors and shortcuts Control + Shift + I/O to copy and paste object transform values.
* Fixed Transform Override Rotation fields not updating with copy, paste, or reset.
* Re-added missing shortcut Control + Shift + U to Ungroup objects.
* Fixed shortcut C in Graph View to cycle through select modes: Keys Only, Tangents Only, or All
* Fixed menu command Set as First/Last Sibling not updating the Timeflow view sorting order.
* Fixed bug dragging object order in Timeflow view.
* Fixed missing keyboard shortcut Alt + Shift + B to add a new Blend component.
* Updated Keyboard Shortcuts documentation with comprehensive list of all built-in and customizable commands. 
* Fixed shortcut Alt + Click to disable/enable Channel Link.
* Fixed shortcut Control + Spacebar to play the active Timeflow continuously without interruption.
* Improved behavior of shortcut Control + L to loop the current selection.
* Fixed shortcut Shift + Page Up/Down to go to the prev/next keyframe.
* Local Time Scale is now ignored if the start an end times are the same as the global start and end.
* Improved shortcut Alt + I to set the time offset for channels with Drag Time Offset enabled, instead of modifying the track time.
* Fixed bug with tracks disappearing when undoing an edit operation.
* To resolve binding conflict, the shortcut Control + Shift + Alt + U is now used to Ungroup objects.
* Improved behavior when adding objects to the Timeflow view, or when using active selection modes, to correctly switch to the parent Timeflow view for the objects selected. 
* Fixed bug with Channel Link Subtract mode being inverted on vectors.
* Fixed error trying to set texture properties on materials without a texture.

## 1.7.3 - 2025-01-07

### Features
* Unparented TimeflowObjects may now assign which Timeflow they belong to, without changing the scene hierarchy.
* Added preference 'Auto Scroll to Selection' to automatically scroll the Timeflow view vertically to the selected object.
* Animated enum values are now displayed as drop-down menus instead of int fields in both the Values Column and Info Panel.
* Channel Links now have the option to 'Use World Time' to link channels in time globally, or in local time with time offsets.

### Fixes
* Fixed bugs with Timeflow hierarchies and orphaned objects not updating.
* Tween: 
 * Fixed bug with Duration Mode not accepting value change when new Tween is created.
 * Fixed timing and graph drawing when using markers.
 * Fixed bug with time offset not being respected by start and end marker times.
* Flyby: Fixed null reference error when selecting Rotation Channel.
* Suppressed warning when attempting to set keyframes on channels that don't support keyframes.
* Fixed 'Set Channel Height' context menu options being disabled for selected objects.
* Fixed sort order when using up/down arrow keys to navigate objects, or tab when renaming.
* Fixed bug setting solo mode for individual or selected channels.
* Animation curves for int and enum types now interpolate using integral steps matching the property type.
* Fixed null reference when adding a new precomp and no object is selected.
* Fixed bug with hierarchy sorting change when adding an object to the Timeflow view.
* Fixed bug with selection and drag-and-dropping objects to create channel links.
* Fixed drawing of linked channels curves in the graph view.
* Fixed time offset issues with channel links.
* Fixed bug with advancing to previous or next keyframe sometimes not working.
* Fixed auto track length calculation when in local time scope mode.
* Improved behavior of soloing selecting objects.
* Fixed channel values and time offsets not displaying correctly for soloed items.
* Channel link set to Ovewrite no longer disables keyframes on the same channel, so they can be edited.
* Fixed circular loop crash when Timeflow is child of TimeflowObject.
* Fixed bug with dragging keyframes in graph view while in local time scope mode.

## 1.7.2 - 2024-12-19

### Features
* Added menu command 'Add New Timeflow', in the GameObject menu and the Timeflow context menu.
* Added Timeflow context menu command 'Add New Precomp' to add a new Timeflow precomp.
* Added keyboard shortcuts Control + Keypad Enter to enter editing mode for a prefab or precomp, and Control + Keypad - to return to the parent Timeflow.
* Precompose now inserts a parent group if the selected object has animation channels, or multiple objects are selected.
* The Timeflow editor Tools menu now has revised options for Add Timeflow
 * Add New Precomp - Inserts a new Timeflow instance as a child
 * Add Timeflow Sibling - Adds a new Timeflow instance on the same hierarchical level
* Added keyboard shortcuts for selected game objects:
 * Select Children: Ctrl Shift - 
 * Select Descendants: Ctrl Alt Shift - 
 * Select Parent: Ctrl Shift = 
 * Select Ancestors: Ctrl Alt Shift =
 

### Fixes
* Fixed bug with auto keyframing not adding channels and deselecting the active object.
* Improved auto keyframing detection.
* Fixed bug with info panel locking keyframe value field when only time is locked.
* Fixed bug with dragging keyframes in the graph view with either locked time or value.
* Improved Property Link editor and fixed 'Add Selected' game objects.
* Fixed bug with channel names not updating when assigning a property. 
 * The name remains unchanged only if the channel was manually renamed.
 * Use the menu command Reset Channel Names to revert to the assigned property name.
* Fixed issue with component icons not working for all behaviors.
* Fixed jump to Full Duration to show the entire timeline, not constrained to the work area.
* Fixed bug with object rename field sometimes being uneditable.
* Improved Property Link setup options and fixed editor UI issues.
* Fixed bug with prefab edit arrow showing for children of prefabs.
 * Prefab edit mode is only for prefab root objects.
* Fixed playback error: InvalidOperationException: Collection was modified; enumeration operation may not execute.
* Fixed bug with Tween curve incorrectly drawing time offset.
* Fixed bug with undoing Precompose causing stack overflow.
* Fixed issues with Timeflow instances becoming disabled as a result of errors.
* Fixed bug with Track Color Palette Assignment Mode not getting saved and reverting unexpectedly.
* Fixed issued with 'Prefab edit mode' blocking display list menu.

## 1.7.1 - 2024-12-12

### Fixes
* Fixed null reference error when adding new Timeflow to a scene
* Fixed serialization issue with Sync Timeline Director, causing scenes using Unity Timeline to not be synced with Timeflow.
* Improved warnings when TimeflowObject (prefab) is added to a scene without Timeflow.

## 1.7.0 - 2024-12-11

### New Features

* Precomposing is now supported, allowing multiple instances of Timeflow to be contained one within another. 
 * Timeflow precomps work just like any other object and can be sequenced in its parent view, or it can be opened in its own local view with markers, work area, and other managed settings.
 * Precomps can optionally be made into prefabs, which can then be used in scenes standalone, or sequenced in a parent Timeflow.
* Each Timeflow instance can now set Global Time Scale. If enabled, this sets the value of Time Scale in the Project Settings.
* Audio Tracks now have a Local Time Scale setting to control the audio playback speed to counteract or combine with the global time scale.
* Track Color Palette settings now have separate sorting for colors and types. 
 * Order by color to arrange the palette display.
 * Order by type to set which components have priority over others. 
 * Added a dropdown menu to apply basic sorting.
 * Added Assignment Mode options to the Track Color Menu (gear icon)
* Improvements and fixes made to dragging and reorganizing objects and channels with full hierarchy support.
* Added preference to Allow Hierarchy Sorting. When enabled, changes made to the sorting order of objects in the Timeflow view are applied to the Hierarchy view as well.
* Improvements to track behaviors
 * When Auto Full Length is enabled, tracks now automatically match the length of their parent.
  * Nested Timeflow instances (precomps) automatically set their track length to match the start and end time set in the settings, which can also be adjusted by modifying the track end points.
 * Added preference Track Auto Full Length Lock, to set whether tracks are locked by default when set to automatic length mode. This defaults off to facilitate time offset dragging.
 * Auto Full Length tracks now display a small letter A to the left of the track to indicate auto mode is enabled.
 * Time Offset and Drag Time Offset can now be set in the info panel for selected tracks, making it easier to access.

### Fixes

* Fixed null reference error when TimeflowObject is orphaned and no Timeflow exists.
* Fixed AmbiguousMatchException error when opening the context menu for certain components.
* Fixed issues with displaying deep object hierarchies.  
* Fixed bugs with drag-and-dropping objects and channels in the Timeflow object panel.
* When renaming an object or channel, it now selects all text in the field by default.
* When dragging an object or channel, hold Control to duplicate the item.
* Duplicate objects and channels now auto-count (1), (2), etc
* Fixed alphabetical sort for objects in the Timeflow view.
* Fixed errors and improved handling of default settings assets being removed and restored.
* Fixed bug with Automatic Yield color assignment mode.
* Fixed bug with toggling Solo Mode not including selected channels.
* Fixed bug with track shading so it applies correctly on both sides of the track.
* Fixed error in the Inspector when opening the section Timeflow->Object->Events
* Fixed issues with looping behavior. If looping is disabled, playback now stops when it reaches the end of the work area if enabled, or otherwise stops at the end of the timeline unless Play Past End is enabled.
* Fixed Timeflow time setting Fit to Displayed Objects, which sets the start and end time to encompass the tracks and keyframes currently displayed in the view.
* Fixes and improvements to TimeflowController and TimeflowPlayback

## 1.6.5 - 2024-11-25

### New Features

* Improved undo history with more descriptive names and value changes.
* Added new buttons Mirror Time and Mirror Values to the alignment toolbar.
* Added more channel height options:
 * Channel Height Presets can now be defined in the Timeflow Preferences.
 * In the Timeflow view, right click on selected objects and channels to find additional options under Set Channel Height
 * Use the keyboard shortcuts [ Left Bracket and ] Right Bracket to step through channel height presets for selected objects and channels.
 * Use the shortcut Control + ] Right Bracket to toggle the channel height lock.
 * When a channel's height is locked, it is displayed with a slightly darker border line and the height cannot be changed until unlocked.
 * Added preference Channel Height Drag Sensitivity to control how much channel heights respond to dragging the divider line.
 * The channel height value and lock setting are now exposed in the inspector in the channel details.
* Added  Default Random Color and Default Color settings to Track Color Palette to specify a specific or random color for new tracks.
* Added new keyboard shortcuts to apply track colors to selected objects and/or channels. Or if nothing is selected, it applies to all items displayed in the Timeflow view:
 * Alt + T : Assign track colors automatically by type (using the palette settings)
 * Alt + D : Assign track colors randomly
 * Alt + E : Assign track colors sequentially
* Enhanced modifiers for switches (lock, visibility, enabled, display filter, and collapsed state)
 * Hold the Shift key when clicking a switch/foldout to set it recursively (on all children and sub-children) 
 * Hold the Control key when clicking the enabled switch as a shortcut to remove all Timeflow Behaviors.
 * Added the option Activate Timeflow to control clips in Timeline, to display the associated Timeflow view (when multiple Timeflow instances are in use).

### Fixes

* In the graph view, keyframes set to Hold interpolation mode now display the left Bezier tangent to control the curve leading into the hold (previously it was hidden). 
* Fixed Bezier tangents when using the commands Mirror Time and Mirror Values on selected keyframes.
* Fixed bug with setting Solo Mode and display filters on selected objects and channels.
* Fixed shortcut key binding conflict with Timeflow/Game Object: Ungroup 
* Fixed bugs and improved assigning track colors.
* Fixed bug with Auto Keyframing setting keys unexpectedly.
* Fixed UI issues with editors for AudioReactive, AudioSpectrum, and AudioSample.
* Fixed invalid cast error when assigning channel links to Tween
* Fixed bugs with setting keyframes on time offset tracks.
* Fixed drawing bug in graph view with Blend keyframes set to Hold.
* Fixed size of progress bar in Render to Disk editor.

## 1.6.4 - 2024-11-13

### Fixes

* Fixed compile error with TimeflowMarkers.cs when making a build.
* Fixed layout issues with Timeflow toolbar icons.
* Fixed bug with Spine animation playback skipping. 
* Fixed broken references in Spine animation package.

## 1.6.3 - 2024-11-12

### New Features

* Local Time Scope: Isolate tracks and keyframes for editing and playback in local time relative to the selection. This focuses the entire timeline view on the selected objects until the mode is exited.
 * This feature mimics the behavior of nested Timeflows (or precomps) without any extra setup or overhead.
  * When prefab objects are opened, they are automatically displayed in Local Time Scope.
 * Users can quickly switch between global time and Local Time Scope for any selected tracks, keyframes, markers, or prefabs using the keyboard shortcut S, or clicking the icon in the menu bar, or double-click on any selected track to display the selection in an isolated timeline. 
  * Hold the Alt key to also isolate the selected objects in Solo Mode.
 * Double-click in any empty area in the track view to exit Local Time Scope and return to the global time view. 
  * Hold Alt to also turn off Solo Mode.
* Prefabs can now be opened and edited directly in the Timeflow view.
* Added preference 'Prefab Save Path' to specify any directory within Assets to save prefabs to, using the command 'Save Selected Prefabs' (Control + Alt + Shift + S).

### Fixes

* Fixed and improved behavior of opening and editing prefabs in Timeflow. 
* Fixed bug with setting keyframe in Timeflow view causing new track segment to be created.
* Fixed warnings creating default preferences after installing Timeflow.
* Fixed 'Getting Started...' menu item - the Readme.pdf is now included with the default assets installed in Assets/Plugins/Timeflow
* Fixed missing references in HDRP and URP VFX samples and added missing scene PlaceOnPath_UnitySpline 
* Fixed bug with fit time (F) and punch zoom (keyboard shortcut Semicolon ; ) 

## 1.6.2 - 2024-11-03

### New Features

* Auto keyframing now detects material property changes. 
* Added 'Auto Fit Graph' setting. Use the keyboard shortcut Control + F, or hold Control while clicking the Fit View button to toggle auto mode on or off. While auto fit is active, any time a channel is selected to view in the graph, it automatically adjusts the vertical size to fit the channel curve.
* Graph View now remembers the vertical zoom and scroll for each channel, so when a channel is selected the graph automatically resizes to it, reducing the need to fit the view every time. 
* Added new preference setting 'Display Scrollbar On Left' - when enable, the vertical scrollbar is displayed on the left side of the timeline area, otherwise it is displayed on the right side by default.

### Fixes

* Improved auto keyframing detection to only listen for direct changes on selected game objects.
* Dragging and panning in the Graph View is now separate from the object hierarchy panel scroll.
* Improved behavior of locking objects and channels by keeping the selection until deselected by the user. 
* Channels are implicitly locked when their parent objects are locked. When a channel is not directly locked itself, a half-opacity lock icon is displayed.

## 1.6.1 - 2024-10-26

### New Features

* Auto keyframing has been extended to detect most property changes on components. This automatically adds the object to the Timeflow view, creates an animation channel for the property, and sets an initial keyframe at the current time.
 * The preference 'New Channel Attribute Mode' has been added determining whether new auto-keyframed channels are created using combined values (XYZ), or separate attribute channels (X, Y, Z) for each value modified. The default is Auto mode which treats Color and Rect fields as combined, and vectors and other types as separate.
 * Hold the Alt key when changing a property to force combined attributes, or hold the Control key to force separate attributes.
 * Note: Auto keyframing cannot detect changes to materials or assets, however Timeflow can animate most properties when mapped explicitly from the property select menu or using a script to map to custom properties.
* PlaceOnPath now supports a PathProvider to implement custom path interpolation. A sample has been added to demonstrate using Unity Splines - see the scene 'PlaceOnPath_UnitySpline'.
* TimeflowObject now exposes events in the editor for Track On, Track Off, and Track Visibility Changed - to execute custom behaviors when tracks in Timeflow start and end.
* Added Vertical Tangents button to the graph view to align selected keyframe tangents vertically.
* Bezier curve tangents in the graph view now support time and value snapping, either toggled using the snap buttons or by holding the Alt key (after drag start).
 * Hold the Shift key while adjusting tangents to constrain them flat or vertical, depending on the direction of drag.
* Added support for UnityEngine.Object types to animate any object property. 
 * Texture properties can now be animated. See the sample scene 'TextureAnimation'
* Added the option 'Always Show Values' in the channel context menu. When enabled, keyframe value labels are always shown for that channel regardless of whether 'Show Keyframe Values' is enabled or not in the Timeflow view.
* Added Go to Start/End and Set Start/End commands to the tracks context menu.

### Fixes

* Fixed bug clicking to select objects in the Timeflow view when the switches panel is closed.
* Fixed bugs with auto keyframing resulting in extra unmapped channels being created.
* Improved behavior and fixed bugs with copy-paste-reset features in the Transform Editor Override.
* Fixed bug with Tween curves not drawing correctly in relation to tracks in the Timeflow view.
* Fixed bug with selecting and deleting multiple channels.
* Fixed bug with the info panel displaying track info when keyframes are selected in the the graph view.
* Improved the Work Area locking behavior and updated its settings in the Timeflow editor.
* Fixed bug with the Prev/Next Frame command and button not working for single frame increments.
* Fixed bugs with recording keyframes on string fields.
* Fixed bugs with window selection context: Ex. selecting keyframes in the track view and pressing Delete was deleting the selected channel instead of the keyframes.
* Fixed bug with Vector4 keyframes losing w attribute value.
* Fixed errors that occur when disabling the main Timeflow component.
* Fixed bug with a new Game View window sometimes being spawned when entering play mode.
* Fixed bug with marquee selection and editing keyframes in a locked Graph View affecting selected channels not visible in the view.
* Fixed null reference error using 'Destroy All Timeflow Behaviors'.
* Keyboard shortcut changes...
 * Changed keyboard shortcut for Solo Mode to Alt + S to resolve conflict with Timeflow context command Alt + O 'Set End of Selected Tracks'.
 * Use Alt + Shift + S for 'Solo Mode (Append)' to add selected objects to the current solo state.Changed keyboard shortcut for 'Save Scene Backup' to Control + Alt + S to resolve conflict with 'Solo Mode (Append)'Fixed 'Set End of Selected Tracks' - was mapped incorrectly to set the in point.
 * Added 'Display Nothing' to menu. 
 * Use the keyboard shortcut Control + ` (backquote) to clear the view.
 * Use ` (backquote) to deselect all.


## 1.6.0 - 2024-10-09

### New Features

* Samples have been reorganized into separate packages that may be installed optionally from the Package Manager.
  * Updated URP and HDRP samples.
* Asset paths are now based on GUIDs so that Timeflow may be moved to any location within the Assets directory.
* Remapped all Timeflow [shortcut commands](../user-guide/menus-and-shortcuts/keyboard-shortcuts/) (both contextual and global) to use Unity's built-in Shortcuts Manager for customization and exposed many additional commands.
  * Timeflow shortcuts can now be exported and imported as a CSV file, to backup your shortcuts or maintain settings for different workflows and projects.
* Added keyboard shortcut Control + Shift + Alt + T to open the Timeflow window.
* Added new Timeflow preferences settings:
  * '[Default Track Mode](../user-guide/menus-and-shortcuts/preferences/tracks.md#default-track-mode)' sets the visibility mode for new tracks.
  * '[Default Locked](../user-guide/menus-and-shortcuts/preferences/tracks.md#default-locked)' sets the default locked state for new tracks.
  * '[Default Drag Time Offset](../user-guide/menus-and-shortcuts/preferences/tracks.md#default-drag-time-offset)' determines whether new tracks default with Drag Time Offset enabled.
  * '[Show Track Colors In Inspector](../user-guide/menus-and-shortcuts/preferences/tracks.md#show-track-colors-in-inspector)' displays selected objects and channels highlighted with a color outline.
  * '[Show Component Icons](../user-guide/menus-and-shortcuts/preferences/objects-and-channels.md#show-behavior-icons)' to display the component type icon for each channel.
  * Changed the default setting of '[Enforce End Time](../user-guide/menus-and-shortcuts/preferences/time.md#enforce-start-end-time)' to false, to make track dragging more intuitive by allowing full length tracks to be moved when unlocked.
  * Added Shortcut buttons: Import, Export, Reset to Default, and Shortcuts Manager...
* Toggling the keyframe button (to set or remove a keyframe), now applies to all selected keyframe channels without having to hold the Alt key.&#x20;
  * Hold the Alt key to toggle a keyframe for a single channel, ignoring the selection.
* Solo mode (Alt + O) can now be used to add new objects to the Timeflow view.
  * If nothing is selected, then this command toggles solo mode on and off in the view.
* The [Info Panel](../user-guide/timeflow-view/info-panel.md) now shows tracks and keyframes separately for easier editing.
* [Keyframe Tools](../user-guide/timeflow-view/keyframe-tools.md) now has a Randomize option to apply randomization to keyframe times and/or values.

### Fixes

* Improved dragging to adjust heights for selected channels and objects.
* Improved keyboard navigation of Timeflow hierarchy view using keyboard arrows.
* Changed keyboard shortcut to cycle Display Lists to Alt + Left Bracket \[ and Alt + Right Bracket ] to resolve conflict with arrow key navigation.
* Fixed issues with dragging object order and reparenting in the Timeflow view.
* Fixed issues with drag limits when dragging tracks and keyframes.
* Fixed behavior of toggling keyframe in the Timeflow view.
* Fixed display colors of RGBA channels in the graph view.
* Simplified Transform property lists to remove ambiguity between 'Position', 'Local Position', 'World Position', and 'Global Position'.

{% hint style="danger" %}
**Unity 2020 Deprecated**

Timeflow 1.5.9 is the last version to support Unity 2020. Timeflow 1.6 requires Unity 2021.3 or later. Earlier Unity versions will no longer be supported and compatibility is not guaranteed.
{% endhint %}

## 1.5.9 - 2024-09-04

#### [Patch 1.5.9.1](https://www.dropbox.com/scl/fi/ida746uq2dysih34p7ylr/TimeflowPatch\_v1.5.9.1\_2024-09-04\_FixAudioSkip.unitypackage?rlkey=jykizbzczwjxh3sxdaqefinlr\&dl=0) - Fixes audio skipping on playback start

#### [Patch 1.5.9.2](https://www.dropbox.com/scl/fi/srd28bikrbzfhfkkgj811/TimeflowPatch\_v1.5.9.2\_2024-09-07\_FixDisableOverrides.unitypackage?rlkey=lvyr2aqkt6q1g347z7i1x9z0k\&dl=0) - Fixes enabling/disabling Transform Override

#### [Patch 1.5.9.3](https://www.dropbox.com/scl/fi/zb74bbqho6norbyzeh8cy/TimeflowPatch\_v1.5.9.3\_2024-09-07\_FixKeyframeDragLimit.unitypackage?rlkey=8h6aeh7izfldjoj7oox2rmr37\&dl=0) - Fixes drag limit affecting keyframes in the track view

\*\* Please note that this is the last version to support Unity 2020.&#x20;

\*\* Unity 2021.3 or later will be required for Timeflow version 1.6.0&#x20;

### New Features

* Revamped switches column and icons to improve visibility
* Added new [preferences](../user-guide/menus-and-shortcuts/preferences/)
  * [Tracks Select Objects](../user-guide/menus-and-shortcuts/preferences/tracks.md#tracks-select-objects): When enabled, selecting tracks and keyframes in the track view also selects the channels and objects they belong to.
  * [Objects Select Tracks](../user-guide/menus-and-shortcuts/preferences/tracks.md#tracks-select-objects): When enabled, selecting objects also selects their tracks.
    * To select keys in channel double-click the channel (hold Shift to select multiple)
  * [Delete Action](../user-guide/menus-and-shortcuts/preferences/objects-and-channels.md#delete-action): to customize the Delete (or Backspace) action (default: Remove from View)
    * Control + Delete Action: a secondary action option (default: Delete Game Object)
  * [Copy Locked Tracks and Keys](../user-guide/menus-and-shortcuts/preferences/tracks.md#copy-locked-tracks-and-keys): off by default to make keyframe editing easier by ignoring locked tracks and keyframes when copying and pasting. &#x20;
* [Transform Override](editor-overrides/transform-editor-override.md) - click on Position, Rotation, or Scale to open/collapse foldout
* Added menu item Tools/Timeflow/Display/[Toggle Hidden](../user-guide/menus-and-shortcuts/tools-menu.md#toggle-hidden) with shortcut Shift + \` to hide/show objects in the Timeflow view.&#x20;
  * Use this command to hide selected objects (without removing them)
  * If nothing is selected, it clears the display filter and reveals all objects in the view.
  * The keyboard shortcut may be customized in the Shortcuts Manager
* Track colors may now be assigned to all objects and channels in the view (except locked)
  * Track color changes are now undoable
  * Added hue, saturation, and lightness adjustment to globally control display colors
  * Added keyboard hotkeys for color picker:
    * A : Automatic color assignment
    * S : Sequential color
    * R : Random color
    * 1-9 : Assign color by order in list
* Audio Track now has the option to draw waveforms in mono or stereo and a scaling factor to control the vertical size displayed in the track view.
  * Audio Track channels can now be renamed in the Timeflow view (defaulting to the clip name)

### Fixes

* Script updates for Unity 6 compatibility
* Fixed null reference errors in TimeflowBehavior.cs&#x20;
* Fixed keyframe copy, paste, and duplication
* Fixed bug with splitting tracks not retaining right half
* Fixed Tween interpolation when BPM changes
* Fixed bug with unexpected snapping when dragging keyframe loop handles
* Fixed bug with SkinnedMeshRenderer blendshape properties list
* Fixed bugs with collapsing/expanding foldouts in Timeflow view
* Disabled double-clicking in the switches column to prevent accidental foldout collapse/expand
* Fixed null references occurring in MotionPath.Editor.cs
* Fixed null references when Timeflow or objects are in a prefab
* Fixed null references occurring with Transform Override when there is no active Timeflow
* Improved behavior of button painting in switches column
* Improved display of locked item state
* Keyframe bounding box now only encompasses unlocked keyframes

## 1.5.8 - 2024-08-27

### Patch 1.5.8.1 fixes keyframe copy-paste and duplication.

Note that this also update the Spine integration so those scripts will need to be imported secondarily

{% embed url="https://www.dropbox.com/scl/fi/ahrwnsw8pgtypcp4hyfsb/TimeflowPatch_v1.5.8.1_2024-08-28_KeyframeCopyPasteFix.unitypackage?rlkey=0u2pre9xxeylf414dcawmqdas&dl=0" %}

### New Features

* [Transform Override](editor-overrides/transform-editor-override.md) now has a Solo button to view the object by itself in the Timeflow view.
  * Use the shortcut key [Alt + O to toggle solo mode](../user-guide/menus-and-shortcuts/tools-menu.md#solo-selected) for the selected Timeflow objects.
* There is a new preference '[Add Objects to Top of List](../user-guide/menus-and-shortcuts/preferences/objects-and-channels.md#add-objects-to-top-of-list)' which controls the behavior of adding objects to the Timeflow view, determining whether new objects are added first or last in the view list.
* Added button  'Open Timeflow Window' to Timeflow inspector, if the window is not already open.

### Fixes

* Fixed ProperitesOfVolumes.cs errors when using URP or HDRP.
* Fixed [Transform Override](editor-overrides/transform-editor-override.md) UI flickering and layout issues.
  * Improved colored channels display when using the option "Show in Color".
  * Fixed lock button on separate X, Y, Z fields.
  * Fixed slider min max defaults and reset button.
  * Fixed issue with object becoming deselected when adding a new animation channel.
  * Improved button behaviors for showing, hiding, and adding items to Timeflow.
    * Hold the Control key to show the 'Remove Timeflow' button, which removes all Timeflow behavior components from the object.
* Fixed compile errors in Unity 2020.
* Fixed a bug when dragging to expand the height of selected channels.
* Fixed null reference errors when deleting Motion Path channels or keyframes.

## 1.5.7 - 2024-08-20

### New Features

* [Transform Override](editor-overrides/transform-editor-override.md) editor provides an optional custom inspector for Transform components with extended features for working with objects and animation in Timeflow. This can be turned on/off from the Transform component menu or the Timeflow Preferences.
* Revised display modes replacing Active Selection with [Selected Object](../user-guide/timeflow-editor/display-lists.md#selected-object-group) and [Selected Group](../user-guide/timeflow-editor/display-lists.md#selected-object-group) modes, to view the selected object, or the selected hierarchy group.
* Added a toggle button to the footer bar of the Timeflow view to enable/disable keyframe [alignment tools](../user-guide/timeflow-view/alignment-tools.md) and the bounding box for selected keyframes.
  * Use the keyboard shortcut T to toggle the keyframe alignment tools and bounding box.&#x20;
* Double-clicking an object in the Timeflow view now opens/closes its hierarchy foldout.
* Selected tracks are now drawn with a subtle bar pattern to make selections more visible.
* Revamped [Timeflow Preferences](../user-guide/menus-and-shortcuts/preferences/) window with better organization and links to resources.
  * Preferences are now automatically saved and applied, so there is no longer an 'Apply Settings' button.
* Added new preference Enforce Time Limits to restrict track and keyframe times to the start and end time of the timeline. This prevents dragging items into negative time.
* Added new context menu item 'Sort Alphabetically' applied to selected objects and their channels. Or applies to all objects and channels in the view if nothing is selected. This replaces the previous 'Sort View' option.
* Graph mode is now displayed with a darker background color to help visually distinguish it from the track view.

### Fixes

* Fixed bug with color palette not switching to List mode.
* Fixed bug with Animation Clips not honoring tracks. Clips now only plays during active track sections.
* Fixed error 'InvalidCastException: Specified cast is not valid.' occurring in SpineAnimator.cs.
  * \*\*NOTE: Please Reimport SpineIntegration package for this update.
* Improved Motion Path velocity graph behavior and fixed keyframe value labels.
* Improved keyframe controls and channel/object selection behavior when clicked.
* Fixed unimplemented warning when duplicating Blend channel keyframes.
* Fixed Blend channel playing through empty track sections.
* Improved keyframe icons and selection highlighting.
* Improved behavior and fixed issues with bulk applying switches and clicking the foldout arrow.
  * Object selections now remain unchanged when interacting with switches and foldouts, with the exception of locking items which prevents selection.
* Fixed a bug with the demo app not correctly reading the scenes list from the build settings.
* Fixed a bug with duplicated channels not updating.

## 1.5.6 - 2024-07-30

### New Features

* [Spine animation is now supported](add-ons/spine-animation.md)!
  * A new add-on package is included to integrate Spine with Timeflow.
  * Animate Spine creations with multiple tracks and animation blending.
  * Example scenes included.
* [Color label assignments](../user-guide/menus-and-shortcuts/context-menus/track-colors.md) are now controlled using a configurable asset assigned in the [Timeflow Preferences](../user-guide/menus-and-shortcuts/preferences/#track-color-mode).
  * Define any set of colors for tracks and channels with optional associations by component and/or channel type.
  * Track colors are now displayed as swatches in the column left of the object name.
  * Click the color swatch to open the new color popup, displayed either as a palette or list of colors.
  * Colors can be applied using Automatic mode to assign by type, Sequential to apply colors in order, or Random to apply colors randomly.
* Added the Auto Play setting 'Startup Frame Buffer'. This improves animation playback when a scene starts up at runtime by waiting for the scene to fully initialize before Timeflow is started.
* Added a new button in the lower toolbar to fit the graph or track view. This works the same as using the keyboard shortcut key F. Hold Shift to fit the time of the selected keyframes (or full duration if none selected).
* When holding the Shift key, the prev/next frame buttons (or Page Up/Page Down) now go to the prev/next keyframe displayed. Previously, it only navigated to selected keyframes and channels, but now ignores the selection and includes all displayed objects and channels.
* Double-clicking an Animation Clip channel in Timeflow now opens the clip in the Animation window.
* Improved object and channel selection highlighting.

### Fixes

* Improved channel height adjustment controls
  * Added a subtle line divider to visually separate tracks and channels.
  * Hover the mouse over the divider line below the object/channel name in the Timeflow hierarchy until the cursor changes to the vertical move icon, then click and drag to adjust the height of the selected channels.
* Improved visibility of playhead marker making it slightly larger.
* Keyframe tick marks are now drawn in black to make them easier to see.
* Object/channel names and foldout icons in the Timeflow hierarchy panel are now displayed without color tinting to make them easier to read.
* Fixed display of keyframe values for uniform values (such as scale)
* Fixed keyframe value labels getting clipped and not showing full value.
* Fixed bug selecting objects when foldout is closed (channels collapsed).
* Fixed bugs with object/channel selection causing hidden objects (collapsed under parent) to be selectable with marquee and drag operations.
* Fixed bug with dragging time offset resetting when track time is locked.
* Fixed bug with audio waveforms drawing stretched when time offset is applied.

## 1.5.5 - 2024-07-09

### New Features

* The Timeflow Preference '[Show Key Values In Graph](../user-guide/menus-and-shortcuts/preferences/#show-key-value-labels)' has been removed and replaced with a toggle switch in the Timeflow window toolbar, next to the grid time display. This allows keyframe values to be toggled on/off in both the graph and track views as needed.
* Noise behavior now has an Override Output option. This allows each attribute (XYZW) of the final noise to be set to a fixed value or controlled by animation.
* A new preference setting '[Draw Linked Channel Data](../user-guide/menus-and-shortcuts/preferences/#draw-linked-channel-data)' has been added to display ghosted keyframes or animation curve for linked channels to help visualize how the data is being replicated. The ghosted channel data is view-only and can only be edited in the original channel.
* 'Expand Looped Keyframes' is now available from the context menu for selected channels. This replicates the keyframes in the looped area to fill the work area or duration of the timeline. Use this to quickly replicate and then edit looped keyframes.
* A preference setting has been added '[Expand Looped Keyframes Overwrite](../user-guide/menus-and-shortcuts/preferences/#expand-looped-keyframes-overwrite)' to specify whether you wish to overwrite keyframes in the target time range, or keep them.
* Linked channels now show ghosted keyframes to visually depict where animation from the linked channels is being replicated. This also aids in visualizing the time offset.
* When working with the Time Offset column, any channels with active links show the link's time offset rather than the channel's time offset.
* A new interface ITimeflowPlayback has been added to implement basic playback events for custom scripts without having to use any of the Timeflow base classes. This allows any script to receive events to Play, Stop, Rewind, and Update in sync with a Timeflow instance.

### Fixes

* Fixed missing reference error for Spine.Unity.
* Fixed issues with VideoPlayerUpdate not working and improved video scrubbing and playback accuracy.
* Fixed runtime bug with OnRewind() not being invoked when Timeflow loops.
* Improved visibility of foldout icons to twirl open/closed track channels.
* Improved visibility of keyframe tick marks, indicating keyframe positions in the channels below the track.

## 1.5.4 - 2024-07-02

### New Features

* [Auto Keyframing](../user-guide/timeflow-view/toolbar/auto-keyframing.md) has been implemented and can be toggled using the shortcut Alt + K. When enabled, any changes made to an animated channel property are detected and result in a new keyframe being added at the current time.&#x20;
* A full list of all behaviors has been added to the main menu under Tools/Timeflow/Add Behaviors.
* Keyboard shortcuts have been added for [Tween](behaviors/animation/tween.md), [Blend](behaviors/animation/blend.md), [Motion Path](behaviors/animation/motion-path.md), and [Flyby ](behaviors/automation/flyby.md)(Alt + Shift + T|B|M|F). Shortcuts may be customized and new ones added using the built-in shortcut manager.
* All components in the Timeflow system are now listed in the Add Component menu under Timeflow as an additional way to add new behaviors.
* [Motion Path](behaviors/animation/motion-path.md)
  * When working with nodes in the Motion Path inspector, clicking the + button now inserts a new keyframe halfway between the current and next keyframe.
  * New keyframes are now added with auto tangent calculation enabled to help create smoother interpolation by default.
* Revised [Keyboard Shortcuts Cheatsheet](../user-guide/menus-and-shortcuts/keyboard-shortcuts/)

### Fixes

* Fixed bug with channel links not appearing when opening objects in prefab edit mode.
* Fixed bug with marquee select tool not selecting keyframes in track or graph view.
* Improved behavior of channel names to automatically update the name whenever the property mapping is changed, unless the channel name has been customized.
* Fixed PrefabStageUtility error upon import into Unity 2020.
* Motion Path
  * Fixed the lock toggle icon button in the Scene view gizmos to correctly show the locked status of the currently selected keyframe(s).
  * Path display color has been unified with the channel display color in the Timeflow view.
* Fixed the Timeflow info panel color picker to apply to the selected channels.&#x20;
* Fixed display issue with Timeflow channel names disappearing intermittently when scrolling.
* Fixed StackOverflowException occurring with nested Timeflows
* Fixed a bug causing duplicate instances of objects affecting runtime efficiency&#x20;

## 1.5.3 - 2024-06-10

### Fixes

* Fixed bug with Timeflow Objects not initializing in the view when working in prefab mode.&#x20;
* Fixed bug with added new behaviors not appearing in the view nor being initialized correctly.
* Improved performance of UI when displaying a long list of objects and channels at once.
* Fixed bug with Blend editor resulting in a argument out of range exceptions to occur when deleting a blend set.
* Fixed error occurring when using the command '[Destroy All Timeflow Behaviors](../user-guide/menus-and-shortcuts/context-menus/object-menu.md#destroy-all-timeflow-behaviors)'
* Fixed Look At behavior channel initializing as 'Unnamed'
* Fixed null reference error occurring when adding Property Link behavior
* Fixed ArgumentNullException when deleting channels
* Fixed NullReferenceException occurring in TimeflowChannel.BuildVectorPath

### New Features

* The Graph View now has a [lock feature](../user-guide/timeflow-view/graph-view.md#graph-locking) to keep one or multiple channel graphs displayed. When enabled, no other channels are visible in the graph view unless explicitly added.
  * Hold the Shift key when clicking the curve icon to disable graph drawing in the track view.
* Added the context menu commands [Work Area/Reveal All Tracks | Keyframes](../user-guide/menus-and-shortcuts/context-menus/work-area-menu.md#reveal-all-tracks-keyframes)&#x20;
* Blend: added Re-Capture button to sets shown in Active tab

## 1.5.2 - 2024-05-21

### Fixes

* Fixed automatic assignment of Director (when using Timeline integration) to find the top-most parent Director if multiple exist in the scene.
* Fixed Hilighter "item not found" warning and related error "EndLayoutGroup: BeginLayoutGroup must be called first."&#x20;
* Fixed mouse scroll holding Shift key to pan the timeline view.&#x20;
* Changed the main time display to make it easier to read. In dark skin mode, time is displayed in white, and in light skin mode it is displayed in black.
* Fixed initialization issue affecting channel links and FlyBy behaviors in runtime builds. &#x20;

### New Features

* Added the ability to play Timeflow in reverse.
* Added vertical scrollbar to Timeflow view.
* Added a gear icon to the Timeflow view and inspector (upper right) to open the Timeflow preferences window.&#x20;
* Added preference setting 'Unified Time Display'. When enabled, the unit of time (seconds, frames, timecode, measures) is the same for all time fields and the grid display. This option may be disabled to set the grid units independently.&#x20;
* Lower-left info panel now displays editable time fields for keyframes, markers, and track start-end times in the time display format selected (seconds, frames, timecode, measures). Supports bulk editing for tracks and keyframes.

## 1.5.1 - 2024-05-08

### Fixes

* Fixed missing InputSystem reference in Timeflow assembly definition.
* Fixed mouse scrolling vertically in hierarchy view (was conflicting with Alt key preference).
* Fixed bug with channel selection resulting in different objects than clicked being selected &#x20;

### New Features

* MidiReceiver
  * Now supports Minis by Keijiro for live MIDI input (as an alternative to MidiJack)
  * Added support for QWERTY keyboard input in lieu of or in addition to a MIDI device.
  * Added 'All Octaves' option to map the same note in all octaves.
  * Added example scenes for both legacy input (MidiJack) and new Input System (Minis)
* Added Code Example scene and script demonstrating how to programmatically add behaviors and animation to objects in Timeflow.

## 1.5.0 -2024-05-06

This version was deprecated due to some compile errors affecting some users.

## 1.4.6 - 2024-04-25

### Fixes

* Fixed script compile errors caused by MidiCloner.cs when building.

## 1.4.5 - 2024-04-23

### Fixes

* Fixed null reference errors and broken Timeflow view if display lists contain missing or null objects.
* When zooming out using mouse scroll wheel, the focal point now remains fixed in place - rather than shifting unpredictably.

### New Features

* The Timeflow timeline can now be panned by dragging with the middle mouse button, performing the same action as the original input Left Mouse Drag + Alt key.
* A preference has been added to 'Scroll Time Centered On Mouse', allowing the mouse position to be the focal point of zooming in and out rather than the current time playhead.
* Added new demo scene 'WheelAlongMotionPath' demonstrating the use of a custom script to rotate wheels moving along a motion path, matching the rotation rate to the distance traveled.

## 1.4.4 - 2024-03-20

### Fixes

* Fixed a bug causing the info panel and display list to disappear when clicking the foldout icon.

### New Features

* Added an example scene 'AudioToKeyframes' showing how to convert audio amplitude to keyframes using Audio Sample and channel link.

## 1.4.3 - 2024-03-19

### New Features

* Command key (instead of Control) can now be used on macOS for Timeflow window shortcuts.
* Added preference 'Scroll Time Without Alt Key' to allow scrolling the time view without the scroll wheel alone.
* Added MidiCloner, and experimental feature for cloning prefabs based on MIDI channel data.

## 1.4.2  - 2024-03-10

### Fixes

* Fixed bug with Timeline integration not playing during runtime or when Timeline window is closed.
* Fixed null reference error with TimeValue setting used in Tween (using Timeflow Duration).

## 1.4.1 - 2023-11-01

### New Features

### RenderToDisk:

* Added ‘Perfect Loop’ option which skips rendering the last frame for perfect loop animations to prevent rendering duplicate frames (when the first frame matches the last frame).
* Added button ‘Abort After Current Render’ to stop rendering after the current range finishes. This also stops the RenderQueue from continuing. Use this to interrupt a render while allowing the current range to finish.

### Fixes

* Fixed potential stack overflow when re-initializing Timeflow in edit mode.
* Fixed a memory leak with internal object lists.
* Fixed issue with irregular update potentially causing jitter in edit mode playback.
* Fixed crash that occurs entering play mode on a demo app scene. A warning is displayed in the console if the demo app scenes have not been configured properly.
* Render To Disk:
  * Fixed bug with disabled render range still rendering if it is first in the range.
    * Fixed repeated ‘Render Queue Finished’ log entries when waiting for video encodings at end of render.
* Animation Clips:
  * Channel value is now properly displayed in the values column.
* Motion Path:
  * Fixed Velocity channel calculation and output value.

## 1.4.0 - 2023-10-24

### **New Features**

* Timeflow Context Menu:
  * Added options to set keyframe tangents Linear Left and Linear Right.
* RenderToDisk:
  * Time ranges can now be set with multiple options including markers, seconds, and more.
  * Increased fulldome tilt range from 0 to 360 degrees.
  * Game view size is only set if the component is enabled and active in the hierarchy.
  * Added filename prefix and option to use scene name.
  * Added Edit Mode Resolution to reduce the resolution of the game view when not rendering.
  * Added ability to chain RenderToDisk instances to render multiple setups in the same scene, by assigning the Render Next field.

### **Fixes**

* Render To Disk:
  * Fixed memory leak when using Screen Capture mode - image buffer was not being disposed of properly resulting in eventual crash.
  * Fixed bug with starting frame numbers being offset by 1.
* TimeValue:
  * Changed name of timing mode ‘Never’ to ‘End’ (used for repeating operations such as Tween).
  * Added ‘Frames’ mode to enter specific frame numbers.
  * Fixed modes ‘Object Start’ and ‘Object End’ to save TimeflowObject reference.
* Delete Time Global:
  * Fixed bug with marker times being adjusted incorrectly.
* Channel Links:
  * Links now re-enable when selecting a link mode other than ‘Off’.\


## 1.3.2 - 2023-10-11

### Fixes

* Fixed script errors when TextMeshPro is not installed - it is only used in the demo app and is not required.
* &#x20;Replaced all instances of deprecated FindObjectsOfType with FindObjectsByType.

## 1.3.1 - 2023-09-06

### New Features

* Render To Disk:
* Added EXR and TGA image formats for render output.
* Added HDR option to use floating point render textures for output (supported by EXR).
* Added Stop and Stop All buttons when encoding videos to kill current or all processes. This will abort the encoding in progress.

### Fixes

* Render To Disk:
* Removed the Dome Resolution option since the render textures and output size determine the final resolution (use any supported by your graphics card)
* Fixed Cubemap Faces drop down - was only showing for stereoscopic setups
* Fixed Metadata field - was not allowing direct input when Auto checkbox is off.

## 1.3.0 - 2023-09-06

### New Features

* Render To Disk:
  * Fully revamped!
  * Optimized render speed with much faster render times!
  * Use RenderToDisk to render any scene even when not using Timeflow.
  * Video encoding is now supported!
  * Encodings automatically run in the background using ffmpeg.
  * Fully customizable settings for any encoding type using scriptable objects.
  * Queue any number of renders and encodings to be processed in order.
  * Stereoscopic rendering is now supported for all render modes.
  * Implemented post processing for VR180 and fulldome stereoscopic modes with examples included.
  * Revamped all examples in Timeflow/Examples/Scenes/Rendering.
  * All render setups now have camera rig prefabs that can easily be added to any scene.
  * Rendering now automatically sets the game view to the correct size and scale.
  * Added drop down menu with common render sizes for quick setup.
  * Replaced TextMeshPro overlay options with property mappings for use with any display type.
  * Directory paths can now be defined using the wildcards $PROJECT or $ASSETS to create paths relative to the current Unity project, or $CUSTOM to define your own path in the Timeflow Preferences.
* Realtime fulldome and fisheye camera rigs are now included as prefabs
* Optimized Behaviors: Added updating option to only update when Timeflow is playing (instead of always updating).

### Fixes

* RenderToDisk:
  * Fulldome Shader: Fixed alpha values rendered in soft edge mask.
  * Improved fulldome rendering by reducing texture memory allocation.
  * Improved render quality with reduced banding by setting all render textures to float space.
  * Fixed Cubemap Faces mask dropdown menu to select which directions are rendered.
  * Improved inspector GUI layout and information display.
  * Automatically sets Game View scale to avoid scaling issues with renders.
  * Fixed render Pause button.
  * Fixed estimated completion time displayed in console log.
* Timeflow View:
  * Fixed null reference errors in Timeflow view when the only active instance is deactivated.
  * Fixed issue with keyframes set to linear not being draggable in the graph view.
  * Fixed channel loop state not being saved when set on prefab objects.
  * Fixed bug with TimeflowObject component being added to prefabs when opened to edit.
* Visual Effect Graph: Fixed missing int and bool property mappings.
* Fixed initialization issue with objects outside of Timeflow hierarchy not updating in editor upon scene opening.
* Renamed AxonGenesis assembly definition to Timeflow and moved it in the Timeflow folder.

## 1.2.2 - 2023-08-27

### New Features

* RenderToDisk:
  * Now supports fisheye 180 VR stereoscopic rendering!
  * Added more example scenes.
  * Added sliders to provide a soft edge mask that is either circular or rounded rectangle (for Fulldome & 180)

### Fixes

* Fixed framerate calculation to work correctly when rendering with Unity Recorder.
* Align Tools: Fixed distribute time for selected keyframes and tracks.
* Timeflow View:
  * Fixed bug dragging track in and out handles, was snapping to grid when snap was turned off.
  * Prohibited selecting keyframes on locked or disabled channels.
  * Prohibited setting new keyframes on locked or disabled channels.
  * Fixed inserting keyframes in graph view on nearest curve - was adding keys on multiple curves at once.
  * Fixed ‘Add Selected’ and ‘Show Selected Only’ buttons - was being intercepted by view click.
  * Fixed rounding issue with track states evaluating on or off at the start and end of track sections.
* MotionPath:
  * Expose Nodes option is now serialized so that it doesn’t reset.
  * Context menu now simply shows ‘Add Animation/Motion Path’ and can only be added once per object.
  * Fixed miscellaneous path editing UI issues in the scene view.
  * Fixed component menu Reset to reset back to the default starting path.
  * Fixed drag-and-dropping Motion Path channel to copy it to another game object.
  * Fixed cleanup issues with hidden node container game objects.
  * Added warning when MotionPath is applied to an object with an Animator component to avoid transform conflicts.

## 1.2.1 - 2023-08-23

### **Fixes**

* **Removed extraneous using directives causing compile errors.**
* **MotionPath: Fixed issue with Look Ahead rotation spinning at end of path.**
* **Removed examples URP package as it was redundant with assets already included.**
* **Added** [**Timeflow Controller**](timeflow-controller.md) **documentation, for creating cutscenes and interactive playback.**

## 1.2.0 - 2023-08-21

### New Components!

* [Property Link](behaviors/tools/property-link.md):
  * Copies the value from one property to multiple properties across many objects.
  * Right click on an object in the Timeflow view and selected Add Tool/Property Link.
* Field Values:
  * Added a collection of simple components with basic data types useful for exposing and working with animated properties in the editor and via scripting.
  * To add a new field value, right click the game object and selected Timeflow/Add Field Value.
* Optimized Behaviors:
  * Lightweight behaviors operating independently of Timeflow for simple animations.
  * Use these instead of Tween for improved performance.
  * OptimizedRotation: applies constant rotation on 1 axis
  * OptimizedLookAt: continuous look at behavior

### New Features

* Internal: Optimized timing and track states, reducing calculations each frame.
* Timeflow View: Selecting the Track Mode in the Timeflow view now applies to all selected objects.
* Rotator: Added the ability to lock individual axes.
* Properties: A warning is now displayed in the console when a property mapping is not assigned. Clicking the warning selects the object in question to quickly locate and fix any properties which have been disconnected or left unassigned.
* Timeline Integration
  * Added an icon button in the Timeflow view to toggle on and off Timeline Director syncing.
  * Improved integration with timing being driven by Timeline or Timeflow depending on which one is played.
  * Fixed issues with control clip timing in Timeline syncing with Timeflow.
* MidiTween:
  * Added ‘Any’ note option when targeting multiple objects.
  * ‘Add Selected’ button to add multiple selected game objects as property targets.
* FPSCounter:
  * Renamed script (was previously DebugText).
  * Uses property mapping (instead of object references) to output string or int value for frame count.
  * Added ability to set color for low and high framerates based on a target FPS value.
  * Improved accuracy of FPS calculation, independent of time scale.
* Graph:
  * Added a lock feature to prevent data from being cleared or overwritten.
  * Added the ability to export and import JSON data, helpful for capturing data at runtime.
* GameObject Menu:
  * Timeflow menu has been reorganized with cleaner submenus.
  * Added a new menu command to ‘Join Adjacent Tracks’ merging all track sections with ends touching.
  * New ‘Add Field Value’ to add fields for basic data types (see Field Values below).
  * Added additional mesh tools in ‘GameObject/Timeflow/Mesh/’.
    * Get Renderer Size: outputs the size of the rendered graphic in world space.
    * Get Bounding Box: computes and outputs the bounding box of the selected game objects to the console.
    * Get Polycount: displays the number of vertices and triangles for the selected game object hierarchy in the console.
    * Combine Meshes: combines the meshes of selected objects into a new asset and game object.
    * Freeze Mesh: freezes the current mesh with transforms and any modifications into a new asset file and game object.

### Examples & Documentation

* Added Cutscenes example scene and tutorial video ‘Creating Cutscenes’.
* Added [FPS Counter](behaviors/tools/fps-counter.md) documentation page (in Behaviors/Tools).
* Added [Optimization ](../user-guide/getting-started/optimization.md)page (in Getting Started) with guidelines for optimizing scenes for performance.

### Fixes

* Timeflow View:
  * Fixed and improved drag and drop operations for objects and channels.
  * Fixed bug when selecting and editing keyframes - was confusing selection with keys in previous view
  * Fixed issues with mouse events when renaming objects, display lists, and channels.
  * Fixed undo after dragging marker with related keys (holding Shift key)
  * Fixed timeline display issues with vertical markers drawing over hierarchy.
  * Fixed null reference error in TimeflowView.OnHierarchyChange.
  * Fixed refresh behavior upon script recompile and forcing refresh.
  * Fixed NaN and Infinity errors occurring with some channel links.
  * Fixed property field not being displayed in Info Panel for Tween channels.
  * Fixed single clicking keyframes in the graph view to select them (affecting multi-numeric types)
  * Fixed channel links being inadvertently disabled for certain channel types.
  * Fixed Alt+Drag so that it only pans the view when clicking in the timeline area.
  * Fixed channels providing current property value on non-animated channels.
  * Fixed Auto Full Length tracks from shifting when adjusting Time Offset.
  * Fixed audio waveform drawing over the toolbar on initial loading of Timeflow view.
* Menu Commands:
  * Fixed ‘Reset Tracks’ command not applying to all selected tracks.
  * Fixed null reference errors when removing a TimeflowObject.
  * Fixed dependency error when using Destroy All Timeflow Behaviors due to execution order.
* Rotator: Fixed rotation bug when scene view is active causing rotations to loop chaotically.
* MidiFile: Fixed Start Time offset to correctly offset midi notes in time.
* Graph: Fixed issues removing destroyed objects (causing errors in Graph.FindGraph).
* Flyby: Fixed runtime startup error in editor: ‘Some objects were not cleaned up when closing the scene’.
* Fixed channel looping behavior to respect loop in and out enabled states.
* PlaceOnSurface: Fixed null reference errors with scene view gizmos when component is disabled.
* Preferences: Fixed issue with settings not updating globally when applied.

## 1.1.0 - 2023-07-27

## Improvements

* An Assembly Definition has been added to speed up compilation and improve package compatibility.
  * The Player Scripting Define Symbols USING\_URP and USING\_HDRP are no longer required and may be removed from Player Settings, since this is now handled automatically by the assembly definition.
  * The MidiJack plugin has been included in Timeflow/AddOns as a separate package. The MIDIJACK scripting define symbol is still required to use this add-on.&#x20;
    * Please note that MidiJack is included under the license terms provided on GitHub: [https://github.com/keijiro/MidiJack](https://github.com/keijiro/MidiJack)   &#x20;
* Revised usage of public, private, protected, and internal keywords to improve code clarity and reduce complexity for integration.
* Sealed all classes which should not be derived from for improved performance.
* Optimized calls to set shader globals using IDs instead of strings.
* Optimized use of Camera.main
* Optimized update call behavior for significant improvements to runtime speed.

### New Features

* Added support for animating Rigidbody2D MovePosition, MoveRotation, and AddForce.
  * 2D and 3D rigidbodies are supported by Keyframer, Follow, Noise, and PlaceOnSurface.&#x20;
* Hold the Alt key while dragging the vertical dividers for the Values and Offsets columns to move them independently, preserving the layout positions of the other panels.
* AudioClips can now be dragged and dropped from the Project view into the Timeflow view, automatically setting up a new game object with the necessary components.
* Flyby channels can now be duplicated to other objects (including Velocity and Rotation channels if in use) by dragging and dropping the channel in Timeflow.

### Bug Fixes

* Fixed null reference and GUIClip errors after recompiling scripts.
* Timeflow Demo App: Fixed audio feedback with live microphone input for audio reactive demo.
* Fixed calculation error with musical measures notation affecting accuracy of display.
* Fixed object selection when using the up and down arrow keys in the object display list.
* Fixed darker rendering with VR 360 format due to internal gamma issues.&#x20;

## 1.0.2 - 2023-06-05

### Bug Fixes

* Unable to drag keyframe values in Graph View on newly created channel
* Null reference error trying to select keyframes after undo.
* Adjustments to Time Offset are now undoable.
* Fixed null reference error with Keyframe Tools when no events or keyframes are selected.
* Fixed property listings for Visual Effects Graph exposed properties.
* Fixed drag-and-drop copy of AudioTrack channel to correctly assign AudioSource reference
* Removed extra using statements causing compile errors (when not using URP).

### Examples

* [Demo App](examples/demo-app.md) : An error message is now displayed in the editor if the Build Settings are not configured properly, to notify users of the setup required.
* [VFX ](examples/visual-effect-graph-vfx.md): Added example scene showing how to animate properties of Visual Effect Graph.

### Feature Updates

* [Render to Disk](behaviors/rendering/render-to-disk.md) : Added ‘Generate Marker Ranges’ to create frame ranges from marker sections
* [Auto Bank](behaviors/automation/auto-bank.md) : Added support for Flyby paths as movement input
* MidiAssetPostprocessor : Files with the .mid or .midi extension are now automatically renamed with .bytes
* [Preferences ](../user-guide/menus-and-shortcuts/preferences/):
  * Added option "[Auto Rename Midi Files .bytes](../user-guide/menus-and-shortcuts/preferences/#auto-rename-midi-files-.bytes)” to enable/disable the above feature
  * Added option "[Show Looped Keyframes](../user-guide/menus-and-shortcuts/preferences/#show-looped-keyframes)” to display ghosted keyframes on looped keyframe channels as a visual aid.
* [Keyframe Tools](../user-guide/timeflow-view/keyframe-tools.md) :
  * Added ability to apply time transforms to markers
* [Channel Menu](../user-guide/menus-and-shortcuts/context-menus/channel-menu.md) :
  * Renamed ‘Convert to Keyframes’ to ‘[Bake Keyframes](../user-guide/menus-and-shortcuts/context-menus/channel-menu.md#bake-keyframes)’ to more clearly describe intent.
  * Added feature ‘[Freeze Time Offset](../user-guide/menus-and-shortcuts/context-menus/channel-menu.md#freeze-time-offset)’ to apply time offset to all keyframes in the channel, then resetting the time offset to 0.
  * Added feature '[Expand Looped Keyframes](../user-guide/menus-and-shortcuts/context-menus/channel-menu.md#expand-looped-keyframes)' to convert a loop into editable keyframes.
  * Added 'Linked To/[Insert Data Channel](../user-guide/menus-and-shortcuts/context-menus/channel-menu.md#insert-data-channel)' to insert a new data channel between linked channels.
* [Info Panel](../user-guide/timeflow-view/info-panel.md) :&#x20;
  * Improved multi-edit capability for all numeric forms.
  * Added undo support for changes to any value fields.

## 1.0.1 - 2023-05-29

* Added [preset functionality](../user-guide/menus-and-shortcuts/presets.md) to most behaviors, with a set of presets included.
* Fixed a bug with the audio toggle in the Demo App.
* Enabled channel looping features for Flyby.
* The version number is now displayed in the Timeflow [Preferences](../user-guide/menus-and-shortcuts/preferences/).

## 1.0.0 - 2023-05-25

* Initial release

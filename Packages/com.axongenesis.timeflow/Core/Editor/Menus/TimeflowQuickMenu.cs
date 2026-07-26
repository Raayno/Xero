// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    public static class TimeflowQuickMenu
    {
        public static event Action<GenericMenu> OnMenuBuild;

        public static GUIContent GetMenuShortcut(string name, string shortcutName = null)
        {
            if (shortcutName == null) {
                shortcutName = StringUtil.RemoveEmojisAndTrim(name).Trim();
            }
            string shortcut = TimeflowShortcuts.GetShortcut(shortcutName, true);
            //Debug.Log($"{shortcutName}:{shortcut}");
            if (!string.IsNullOrEmpty(shortcut)) {
                return new GUIContent($"{name}\t{shortcut}");
            }
            return new GUIContent(name);
        }

#if TIMEFLOW_PRO
        public const string kQuickMenu = "⚡ Quick Menu";
#else
        public const string kQuickMenu = "Quick Menu";
#endif
        [Shortcut(TimeflowShortcutInfo.Path_QuickMenu, KeyCode.T, ShortcutModifiers.Shift)]
        [MenuItem(TimeflowMenu.MenuPath + kQuickMenu + TimeflowMenu.Tab + TimeflowShortcutBindings.QuickMenu, priority = 12000)]
        [MenuItem(TimeflowMenu.MenuPath2 + kQuickMenu, priority = 12000)]
        public static void ShowQuickMenu()
        {
            Vector2 mousePosition = Event.current == null ? new Vector2(500, 500) : Event.current.mousePosition;

            GenericMenu menu = new GenericMenu();

            menu.AddItem(GetMenuShortcut(TimeflowWindow.kOpenTimeflowWindow, TimeflowShortcutInfo.Path_OpenTimeflowWindow), false, () => TimeflowWindow.OpenWindow());
            menu.AddItem(GetMenuShortcut(AdvancedPresetsWindow.kOpenAdvancedPresets, TimeflowShortcutInfo.Path_OpenAdvancedPresets), false, () => AdvancedPresetsWindow.OpenWindow());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(KeyframerEdit.kAddKeyframer, KeyframerEdit.kShortcut), false, () => KeyframerEdit.AddKeyframer());
            menu.AddItem(GetMenuShortcut(TweenEdit.kAddTween, TweenEdit.kShortcut), false, () => TweenEdit.AddTween());
            menu.AddItem(GetMenuShortcut(BlendEdit.kAddBlend, BlendEdit.kShortcut), false, () => BlendEdit.AddBlend());
            menu.AddItem(GetMenuShortcut(FlybyEdit.kAddFlyby, FlybyEdit.kShortcut), false, () => FlybyEdit.AddFlyBy());
            menu.AddItem(GetMenuShortcut(MotionPathEdit.kAddMotionPath, MotionPathEdit.kShortcut), false, () => MotionPathEdit.AddMotionPath());
            menu.AddItem(GetMenuShortcut(AnimationClipsEdit.kAddAnimationClips, AnimationClipsEdit.kShortcut), false, () => AnimationClipsEdit.AddAnimationClips());
            menu.AddItem(GetMenuShortcut(AnimationSequencerEdit.kAddAnimationSequencer, AnimationClipsEdit.kShortcut), false, () => AnimationSequencerEdit.AddAnimationSequencer());

            menu.AddSeparator(TimeflowMenu.kAddBehavior + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(AlignChildrenEdit.kAddAlignChildren, AlignChildrenEdit.kShortcut), false, () => AlignChildrenEdit.AddAlignChildren());
            menu.AddItem(GetMenuShortcut(AutoBankEdit.kAddAutoBank, AutoBankEdit.kShortcut), false, () => AutoBankEdit.AddAutoBank());
            menu.AddItem(GetMenuShortcut(AutoRotateEdit.kAddAutoRotate, AutoRotateEdit.kShortcut), false, () => AutoRotateEdit.AddAutoRotate());
            menu.AddItem(GetMenuShortcut(DistanceEdit.kAddDistance, DistanceEdit.kShortcut), false, () => DistanceEdit.AddDistance());
            menu.AddItem(GetMenuShortcut(NoiseEdit.kAddNoise, NoiseEdit.kShortcut), false, () => NoiseEdit.AddNoise());
            menu.AddItem(GetMenuShortcut(FollowEdit.kAddFollow, FollowEdit.kShortcut), false, () => FollowEdit.AddFollow());
            menu.AddItem(GetMenuShortcut(LookAtEdit.kAddLookAt, LookAtEdit.kShortcut), false, () => LookAtEdit.AddLookAt());
            menu.AddItem(GetMenuShortcut(LookAtTargetEdit.kAddLookAtTarget, LookAtTargetEdit.kShortcut), false, () => LookAtTargetEdit.AddLookAtTarget());
            menu.AddItem(GetMenuShortcut(LoopTimeOffsetEdit.kLoopTimeOffset, LookAtTargetEdit.kShortcut), false, () => LoopTimeOffsetEdit.AddLoopTimeOffset());
            menu.AddItem(GetMenuShortcut(GraphEdit.kAddGraph, GraphEdit.kShortcut), false, () => GraphEdit.AddGraph());
            menu.AddItem(GetMenuShortcut(PropertyLinkEdit.kAddPropertyLink, PropertyLinkEdit.kShortcut), false, () => PropertyLinkEdit.AddPropertyLink());
            menu.AddItem(GetMenuShortcut(PlaceOnPathEdit.kAddPlaceOnPath, PlaceOnPathEdit.kShortcut), false, () => PlaceOnPathEdit.AddPlaceOnPath());
            menu.AddItem(GetMenuShortcut(PlaceOnSurfaceEdit.kAddPlaceOnSurface, PlaceOnSurfaceEdit.kShortcut), false, () => PlaceOnSurfaceEdit.AddPlaceOnSurface());

            menu.AddSeparator(TimeflowMenu.kAddBehavior + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(AudioTrackEdit.kAddAudioTrack, AudioTrackEdit.kShortcut), false, () => AudioTrackEdit.AddAudioTrack());
            menu.AddItem(GetMenuShortcut(AudioReactiveEdit.kAddAudioReactive, AudioReactiveEdit.kShortcut), false, () => AudioReactiveEdit.AddAudioReactive());
            menu.AddItem(GetMenuShortcut(AudioSampleEdit.kAddAudioSample, AudioSampleEdit.kShortcut), false, () => AudioSampleEdit.AddAudioSample());
            menu.AddItem(GetMenuShortcut(AudioSpectrumEdit.kAddAudioSpectrum, AudioSpectrumEdit.kShortcut), false, () => AudioSpectrumEdit.AddAudioSpectrum());

            menu.AddSeparator(TimeflowMenu.kAddBehavior + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(MidiFileEdit.kAddMidiFile, MidiFileEdit.kShortcut), false, () => MidiFileEdit.AddMidiFile());
            menu.AddItem(GetMenuShortcut(MidiReceiverEdit.kAddMidiReceiver, MidiReceiverEdit.kShortcut), false, () => MidiReceiverEdit.AddMidiReceiver());
            menu.AddItem(GetMenuShortcut(MidiTweenUI.kAddMidiTween, MidiTweenUI.kShortcut), false, () => MidiTweenUI.AddMidiTween());
            menu.AddItem(GetMenuShortcut(MidiClonerUI.kAddMidiCloner, MidiClonerUI.kShortcut), false, () => MidiClonerUI.AddMidiCloner());

            menu.AddSeparator(TimeflowMenu.kAddBehavior + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(ParticleSystemUpdateEdit.kAddParticleSystemUpdate, ParticleSystemUpdateEdit.kShortcut), false, () => ParticleSystemUpdateEdit.AddParticleSystemUpdate());
            menu.AddItem(GetMenuShortcut(TimeDisplayEdit.kAddTimeDisplay, TimeDisplayEdit.kShortcut), false, () => TimeDisplayEdit.AddTimeDisplay());
            menu.AddItem(GetMenuShortcut(TrailRendererUpdateEdit.kAddTrailRendererUpdate, TrailRendererUpdateEdit.kShortcut), false, () => TrailRendererUpdateEdit.AddTrailRendererUpdate());
            menu.AddItem(GetMenuShortcut(PhysicsUpdateEdit.kAddPhysicsUpdate, PhysicsUpdateEdit.kShortcut), false, () => PhysicsUpdateEdit.AddPhysicsUpdate());

//#if ANIMATIONRIGGING_1_OR_NEWER
//            menu.AddItem(GetMenuShortcut(RigUpdateEdit.kAddRigUpdate, RigUpdateEdit.kShortcut), false, () => RigUpdateEdit.AddRigUpdate());
//#endif
            menu.AddItem(GetMenuShortcut(VideoPlayerUpdateEdit.kAddVideoPlayerUpdate, VideoPlayerUpdateEdit.kShortcut), false, () => VideoPlayerUpdateEdit.AddVideoPlayerUpdate());

            menu.AddSeparator(TimeflowMenu.kAddBehavior + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(RenderToDiskEdit.kAddRenderToDisk, RenderToDiskEdit.kShortcut), false, () => RenderToDiskEdit.AddRenderToDisk());
            menu.AddItem(GetMenuShortcut(RenderQueueEditor.kAddRenderQueue, RenderQueueEditor.kShortcut), false, () => RenderQueueEditor.AddRenderToDisk());
            menu.AddItem(GetMenuShortcut(RealtimeFulldomeEdit.kAddRealtimeFulldomeCamera, RealtimeFulldomeEdit.kShortcut), false, () => RealtimeFulldomeEdit.AddRealtimeFulldome());

            OnMenuBuild?.Invoke(menu);

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kAutoKeyframing, TimeflowShortcutInfo.Path_AutoKeyframing), false, !TimeflowMenu.ValidateToggleAutoKeyframing() ? null : () => TimeflowMenu.ToggleAutoKeyframing());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kAddKeyframe, TimeflowShortcutInfo.Path_AddKeyframe), false, !TimeflowMenu.ValidateAddKeyframe() ? null : () => TimeflowMenu.AddKeyframe());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kAddNewTimeflow, TimeflowShortcutInfo.Path_AddNewTimeflow), false, () => TimeflowMenu.AddNewTimeflow());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kPrecompose, TimeflowShortcutInfo.Path_Precompose), false, !TimeflowMenu.ValidatePrecompose() ? null : () => TimeflowMenu.Precompose());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDecompose, TimeflowShortcutInfo.Path_Decompose), false, !TimeflowMenu.ValidateDecompose() ? null : () => TimeflowMenu.Decompose());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kEnterEditMode, TimeflowShortcutInfo.Path_EnterPrefabPrecompEditMode), false, !TimeflowMenu.ValidateEnterPrefabEditMode() ? null : () => TimeflowMenu.EnterPrefabEditMode());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kExitEditMode, TimeflowShortcutInfo.Path_ExitPrefabPrecompEditMode), false, !TimeflowMenu.ValidateExitPrefabEditMode() ? null : () => TimeflowMenu.ExitPrefabEditMode());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSaveSelectedPrefabs, TimeflowShortcutInfo.Path_SaveSelectedPrefabs), false, !TimeflowMenu.ValidateSavePrefabs() ? null : () => TimeflowMenu.SavePrefabs());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayNothing, TimeflowShortcutInfo.Path_DisplayNothing), false, !TimeflowMenu.ValidateDisplayNothing() ? null : () => TimeflowMenu.DisplayNothing());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayEverything, TimeflowShortcutInfo.Path_DisplayEverything), false, !TimeflowMenu.ValidateDisplayEverything() ? null : () => TimeflowMenu.DisplayEverything());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplaySelectedOnly, TimeflowShortcutInfo.Path_DisplaySelectedOnly), false, !TimeflowMenu.ValidateDisplaySelectedOnly() ? null : () => TimeflowMenu.DisplaySelectedOnly());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayAddSelectedToView, TimeflowShortcutInfo.Path_AddSelectedToView), false, !TimeflowMenu.ValidateDisplaySelectedAdd() ? null : () => TimeflowMenu.DisplaySelectedAdd());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayActiveSelectionGrouped, TimeflowShortcutInfo.Path_ActiveSelectionGrouped), false, !TimeflowMenu.ValidateDisplayActiveSelection() ? null : () => TimeflowMenu.DisplayActiveSelection());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplaySelectedObject, TimeflowShortcutInfo.Path_DisplaySelectedOnly), false, !TimeflowMenu.ValidateDisplayActiveSelectionObject() ? null : () => TimeflowMenu.DisplayActiveSelectionObject());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplaySoloSelected, TimeflowShortcutInfo.Path_SoloSelected), false, !TimeflowMenu.ValidateDisplaySoloSelected() ? null : () => TimeflowMenu.DisplaySoloSelected());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplaySoloSelectedAppend, TimeflowShortcutInfo.Path_SoloSelectedAppend), false, !TimeflowMenu.ValidateDisplaySoloSelectedAdd() ? null : () => TimeflowMenu.DisplaySoloSelectedAdd());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayToggleHidden, TimeflowShortcutInfo.Path_ToggleHidden), false, !TimeflowMenu.ValidateToggleHidden() ? null : () => TimeflowMenu.ToggleHidden());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayPrevious, TimeflowShortcutInfo.Path_DisplayPrevious), false, !TimeflowMenu.ValidateShowRecentPrev() ? null : () => TimeflowMenu.ShowRecentPrev());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDisplayNext, TimeflowShortcutInfo.Path_DisplayNext), false, !TimeflowMenu.ValidateShowRecentNext() ? null : () => TimeflowMenu.ShowRecentNext());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToFullDuration, TimeflowShortcutInfo.Path_JumpToFullDuration), false, !TimeflowMenu.ValidateJumpToFullDuration() ? null : () => TimeflowMenu.JumpToFullDuration());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker1, TimeflowShortcutInfo.Path_JumpToMarker1), false, !TimeflowMenu.ValidateJumpToMarker1() ? null : () => TimeflowMenu.JumpToMarker1());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker2, TimeflowShortcutInfo.Path_JumpToMarker2), false, !TimeflowMenu.ValidateJumpToMarker2() ? null : () => TimeflowMenu.JumpToMarker2());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker3, TimeflowShortcutInfo.Path_JumpToMarker3), false, !TimeflowMenu.ValidateJumpToMarker3() ? null : () => TimeflowMenu.JumpToMarker3());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker4, TimeflowShortcutInfo.Path_JumpToMarker4), false, !TimeflowMenu.ValidateJumpToMarker4() ? null : () => TimeflowMenu.JumpToMarker4());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker5, TimeflowShortcutInfo.Path_JumpToMarker5), false, !TimeflowMenu.ValidateJumpToMarker5() ? null : () => TimeflowMenu.JumpToMarker5());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker6, TimeflowShortcutInfo.Path_JumpToMarker6), false, !TimeflowMenu.ValidateJumpToMarker6() ? null : () => TimeflowMenu.JumpToMarker6());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker7, TimeflowShortcutInfo.Path_JumpToMarker7), false, !TimeflowMenu.ValidateJumpToMarker7() ? null : () => TimeflowMenu.JumpToMarker7());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker8, TimeflowShortcutInfo.Path_JumpToMarker8), false, !TimeflowMenu.ValidateJumpToMarker8() ? null : () => TimeflowMenu.JumpToMarker8());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJumpToMarker9, TimeflowShortcutInfo.Path_JumpToMarker9), false, !TimeflowMenu.ValidateJumpToMarker9() ? null : () => TimeflowMenu.JumpToMarker9());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDestroyAllTimeflowBehaviors, TimeflowShortcutInfo.Path_DestroyAllTimeflowBehaviors), false, !TimeflowObject.ValidateRemoveTimeflowObjects() ? null : () => TimeflowObject.RemoveTimeflowObjects());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDeleteChildren, TimeflowShortcutInfo.Path_DeleteChildren), false, !TimeflowMenu.ValidateDeleteChildren() ? null : () => TimeflowMenu.DeleteChildren());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSortChildren, TimeflowShortcutInfo.Path_SortChildren), false, !TimeflowMenu.ValidateSortChildren() ? null : () => TimeflowMenu.SortChildren());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSortChildrenReverse, TimeflowShortcutInfo.Path_SortChildrenReverse), false, !TimeflowMenu.ValidateSortChildrenReverse() ? null : () => TimeflowMenu.SortChildrenReverse());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kHideChildrenInHierarchy, TimeflowShortcutInfo.Path_HideChildrenInHierarchy), false, !TimeflowMenu.ValidateHideChildrenInHierarchy() ? null : () => TimeflowMenu.HideChildrenInHierarchy());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kShowChildrenInHierarchy, TimeflowShortcutInfo.Path_ShowChildrenInHierarchy), false, !TimeflowMenu.ValidateShowChildrenInHierarchy() ? null : () => TimeflowMenu.ShowChildrenInHierarchy());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kGroup, TimeflowShortcutInfo.Path_Group), false, !TimeflowMenu.ValidateGroupObjects() ? null : () => TimeflowMenu.GroupObjects());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kUngroup, TimeflowShortcutInfo.Path_Ungroup), false, !TimeflowMenu.ValidateUngroupObjects() ? null : () => TimeflowMenu.UngroupObjects());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kUnparent, TimeflowShortcutInfo.Path_Unparent), false, !TimeflowMenu.ValidateUnparentObjects() ? null : () => TimeflowMenu.UnparentObjects());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kFlatten, TimeflowShortcutInfo.Path_Flatten), false, !TimeflowMenu.ValidateFlattenObjectHierarchy() ? null : () => TimeflowMenu.FlattenObjectHierarchy());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kRemoveNumbering, TimeflowShortcutInfo.Path_RemoveNumbering), false, !TimeflowMenu.ValidateRemoveNumbering() ? null : () => TimeflowMenu.RemoveNumbering());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kGetRendererSize, TimeflowShortcutInfo.Path_GetRendererSize), false, !TimeflowMenu.ValidateGetRendererSize() ? null : () => TimeflowMenu.GetRendererSize());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kGetBoundingBox, TimeflowShortcutInfo.Path_GetBoundingBox), false, !TimeflowMenu.ValidateGetBoundingBox() ? null : () => TimeflowMenu.GetBoundingBox());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kGetPolycount, TimeflowShortcutInfo.Path_GetPolycount), false, !TimeflowMenu.ValidateGetMeshPolycount() ? null : () => TimeflowMenu.GetMeshPolycount());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kFreezeMesh, TimeflowShortcutInfo.Path_FreezeMesh), false, !TimeflowMenu.ValidateFreezeMesh() ? null : () => TimeflowMenu.FreezeMesh());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kCombineMeshes, TimeflowShortcutInfo.Path_CombineMeshes), false, !TimeflowMenu.ValidateCombineMeshs() ? null : () => TimeflowMenu.CombineMeshs());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kDeselectAll, TimeflowShortcutInfo.Path_DeselectAll), false, !TimeflowMenu.ValidateDeselect() ? null : () => TimeflowMenu.Deselect());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectChildren, TimeflowShortcutInfo.Path_SelectChildren), false, !TimeflowMenu.ValidateSelectChildren() ? null : () => TimeflowMenu.SelectChildren());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectDescendants, TimeflowShortcutInfo.Path_SelectDescendants), false, !TimeflowMenu.ValidateSelectDescendants() ? null : () => TimeflowMenu.SelectDescendants());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectParents, TimeflowShortcutInfo.Path_SelectParents), false, !TimeflowMenu.ValidateSelectParents() ? null : () => TimeflowMenu.SelectParents());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectAncestors, TimeflowShortcutInfo.Path_SelectAncestors), false, !TimeflowMenu.ValidateSelectAncestors() ? null : () => TimeflowMenu.SelectAncestors());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectRenderersRecursive, TimeflowShortcutInfo.Path_SelectRenderersRecursive), false, !TimeflowMenu.ValidateSelectRenderersRecursive() ? null : () => TimeflowMenu.SelectRenderersRecursive());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kSelectMainCamera, TimeflowShortcutInfo.Path_SelectMainCamera), false, () => TimeflowMenu.SelectMainCamera());

            menu.AddSeparator(TimeflowMenu.kSelect + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject1, TimeflowShortcutInfo.Path_QuickSelectObject1), false, !TimeflowMenu.ValidateQuickSelect1() ? null : () => TimeflowMenu.QuickSelect1());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject2, TimeflowShortcutInfo.Path_QuickSelectObject2), false, !TimeflowMenu.ValidateQuickSelect2() ? null : () => TimeflowMenu.QuickSelect2());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject3, TimeflowShortcutInfo.Path_QuickSelectObject3), false, !TimeflowMenu.ValidateQuickSelect3() ? null : () => TimeflowMenu.QuickSelect3());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject4, TimeflowShortcutInfo.Path_QuickSelectObject4), false, !TimeflowMenu.ValidateQuickSelect4() ? null : () => TimeflowMenu.QuickSelect4());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject5, TimeflowShortcutInfo.Path_QuickSelectObject5), false, !TimeflowMenu.ValidateQuickSelect5() ? null : () => TimeflowMenu.QuickSelect5());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject6, TimeflowShortcutInfo.Path_QuickSelectObject6), false, !TimeflowMenu.ValidateQuickSelect6() ? null : () => TimeflowMenu.QuickSelect6());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject7, TimeflowShortcutInfo.Path_QuickSelectObject7), false, !TimeflowMenu.ValidateQuickSelect7() ? null : () => TimeflowMenu.QuickSelect7());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject8, TimeflowShortcutInfo.Path_QuickSelectObject8), false, !TimeflowMenu.ValidateQuickSelect8() ? null : () => TimeflowMenu.QuickSelect8());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject9, TimeflowShortcutInfo.Path_QuickSelectObject9), false, !TimeflowMenu.ValidateQuickSelect9() ? null : () => TimeflowMenu.QuickSelect9());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject10, TimeflowShortcutInfo.Path_QuickSelectObject10), false, !TimeflowMenu.ValidateQuickSelect10() ? null : () => TimeflowMenu.QuickSelect10());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject11, TimeflowShortcutInfo.Path_QuickSelectObject11), false, !TimeflowMenu.ValidateQuickSelect11() ? null : () => TimeflowMenu.QuickSelect11());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectObject12, TimeflowShortcutInfo.Path_QuickSelectObject12), false, !TimeflowMenu.ValidateQuickSelect12() ? null : () => TimeflowMenu.QuickSelect12());

            menu.AddSeparator(TimeflowMenu.kSelect + TimeflowMenu.Sep);
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject1, TimeflowShortcutInfo.Path_QuickSelectAssignObject1), false, !TimeflowMenu.ValidateQuickSelect1Assign() ? null : () => TimeflowMenu.QuickSelect1Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject2, TimeflowShortcutInfo.Path_QuickSelectAssignObject2), false, !TimeflowMenu.ValidateQuickSelect2Assign() ? null : () => TimeflowMenu.QuickSelect2Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject3, TimeflowShortcutInfo.Path_QuickSelectAssignObject3), false, !TimeflowMenu.ValidateQuickSelect3Assign() ? null : () => TimeflowMenu.QuickSelect3Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject4, TimeflowShortcutInfo.Path_QuickSelectAssignObject4), false, !TimeflowMenu.ValidateQuickSelect4Assign() ? null : () => TimeflowMenu.QuickSelect4Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject5, TimeflowShortcutInfo.Path_QuickSelectAssignObject5), false, !TimeflowMenu.ValidateQuickSelect5Assign() ? null : () => TimeflowMenu.QuickSelect5Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject6, TimeflowShortcutInfo.Path_QuickSelectAssignObject6), false, !TimeflowMenu.ValidateQuickSelect6Assign() ? null : () => TimeflowMenu.QuickSelect6Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject7, TimeflowShortcutInfo.Path_QuickSelectAssignObject7), false, !TimeflowMenu.ValidateQuickSelect7Assign() ? null : () => TimeflowMenu.QuickSelect7Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject8, TimeflowShortcutInfo.Path_QuickSelectAssignObject8), false, !TimeflowMenu.ValidateQuickSelect8Assign() ? null : () => TimeflowMenu.QuickSelect8Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject9, TimeflowShortcutInfo.Path_QuickSelectAssignObject9), false, !TimeflowMenu.ValidateQuickSelect9Assign() ? null : () => TimeflowMenu.QuickSelect9Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject10, TimeflowShortcutInfo.Path_QuickSelectAssignObject10), false, !TimeflowMenu.ValidateQuickSelect10Assign() ? null : () => TimeflowMenu.QuickSelect10Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject11, TimeflowShortcutInfo.Path_QuickSelectAssignObject11), false, !TimeflowMenu.ValidateQuickSelect11Assign() ? null : () => TimeflowMenu.QuickSelect11Assign());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kQuickSelectAssignObject12, TimeflowShortcutInfo.Path_QuickSelectAssignObject12), false, !TimeflowMenu.ValidateQuickSelect12Assign() ? null : () => TimeflowMenu.QuickSelect12Assign());

            menu.AddSeparator("");  
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kResetTracksSelected, TimeflowShortcutInfo.Path_ResetTracksSelected), false, !TimeflowObject.ValidateResetSelectedTracksFullLength() ? null : () => TimeflowObject.ResetSelectedTracksFullLength());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kResetAllTracks, TimeflowShortcutInfo.Path_ResetAllTracks), false, !TimeflowObject.ValidateResetAllTracksFullLength() ? null : () => TimeflowObject.ResetAllTracksFullLength());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kJoinAdjacentTracks, TimeflowShortcutInfo.Path_JoinAdjacentTracks), false, !TimeflowObject.ValidateJoinAdjacentTracks() ? null : () => TimeflowObject.JoinAdjacentTracks());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TrackColorMenu.kTrackColorsOpenPalette, TimeflowShortcutInfo.Path_TrackColorsOpenPalette), false, () => TrackColorMenu.ShowPalette());
            menu.AddItem(GetMenuShortcut(TrackColorMenu.kTrackColorsAssignSequential, TimeflowShortcutInfo.Path_TrackColorsAssignSequential), false, () => TrackColorMenu.AssignSequentialTrackColors());
            menu.AddItem(GetMenuShortcut(TrackColorMenu.kTrackColorsAssignRandom, TimeflowShortcutInfo.Path_TrackColorsAssignRandom), false, () => TrackColorMenu.AssignRandomTrackColors());
            menu.AddItem(GetMenuShortcut(TrackColorMenu.kTrackColorsAssignAuto, TimeflowShortcutInfo.Path_TrackColorsAssignAuto), false, () => TrackColorMenu.AssignAutoTrackColors());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kTransformReset, TimeflowShortcutInfo.Path_TransformReset), false, !TimeflowMenu.ValidateResetTransform() ? null : () => TimeflowMenu.ResetTransform());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kTransformCopy, TimeflowShortcutInfo.Path_TransformCopy), false, !TimeflowMenu.ValidateCopyTransform() ? null : () => TimeflowMenu.CopyTransform());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kTransformPaste, TimeflowShortcutInfo.Path_TransformPaste), false, !TimeflowMenu.ValidatePasteTransform() ? null : () => TimeflowMenu.PasteTransform());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kTransformPasteResetScale, TimeflowShortcutInfo.Path_TransformPasteResetScale), false, !TimeflowMenu.ValidatePasteTransformReformScale() ? null : () => TimeflowMenu.PasteTransformReformScale());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kTransformPastePositionOnly, TimeflowShortcutInfo.Path_TransformPastePositionOnly), false, !TimeflowMenu.ValidatePastePosition() ? null : () => TimeflowMenu.PastePosition());


            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityActivate, TimeflowShortcutInfo.Path_Activate), false, !TimeflowMenu.ValidateActivate() ? null : () => TimeflowMenu.Activate());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityDeactivate, TimeflowShortcutInfo.Path_Deactivate), false, !TimeflowMenu.ValidateDeactivate() ? null : () => TimeflowMenu.Deactivate());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityActivateRecursive, TimeflowShortcutInfo.Path_ActivateRecursive), false, !TimeflowMenu.ValidateActivateRecursive() ? null : () => TimeflowMenu.ActivateRecursive());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityDeactivateRecursive, TimeflowShortcutInfo.Path_DeactivateRecursive), false, !TimeflowMenu.ValidateDeactivateRecursive() ? null : () => TimeflowMenu.DeactivateRecursive());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityEnableRenderersRecursive, TimeflowShortcutInfo.Path_EnableRenderersRecursive), false, !TimeflowMenu.ValidateEnableRenderersRecursive() ? null : () => TimeflowMenu.EnableRenderersRecursive());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kVisibilityDisableRenderersRecursive, TimeflowShortcutInfo.Path_DisableRenderersRecursive), false, !TimeflowMenu.ValidateDisableRenderersRecursive() ? null : () => TimeflowMenu.DisableRenderersRecursive());

            menu.AddSeparator("");
            menu.AddItem(GetMenuShortcut(TimeflowWindow.kToggleWindowMinimized, TimeflowShortcutInfo.Path_ToggleTimeflowWindowMinimized), false, () => TimeflowWindow.ToggleWindowMinimized());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kEditorDebugMarkLine, TimeflowShortcutInfo.Path_EditorDebugMarkLine), false, () => TimeflowMenu.DebugMarkLine());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kEditorDisableDebugAll, TimeflowShortcutInfo.Path_EditorDisableDebugAll), false, () => TimeflowMenu.DisableDebugAll());
            menu.AddItem(GetMenuShortcut(TimeflowMenu.kEditorListDependencies, TimeflowShortcutInfo.Path_EditorListDependencies), false, !TimeflowMenu.ValidateListDependencies() ? null : () => TimeflowMenu.ListDependencies());
            menu.AddItem(GetMenuShortcut(TimeflowShortcuts.kExportShortcuts, TimeflowShortcutInfo.Path_ExportShortcuts), false, () => TimeflowShortcuts.ExportShortcuts());
            menu.AddItem(GetMenuShortcut(TimeflowShortcuts.kImportShortcuts, TimeflowShortcutInfo.Path_ImportShortcuts), false, () => TimeflowShortcuts.ImportShortcuts());
            menu.AddItem(GetMenuShortcut(TimeflowShortcuts.kResetShortcuts, TimeflowShortcutInfo.Path_ResetShortcuts), false, () => TimeflowShortcuts.ResetShortcutsToDefault());
            menu.AddItem(GetMenuShortcut(TimeflowShortcuts.kOpenShortcutsManager, TimeflowShortcutInfo.Path_OpenShortcutsManager), false, () => TimeflowShortcuts.OpenShortcutsManager());
#if TIMEFLOW_PRO
            menu.AddItem(GetMenuShortcut(TimeflowEditorOverrides.kDisableTimeflow, TimeflowShortcutInfo.Path_TimeflowPro), false, () => TimeflowEditorOverrides.ToggleTimeflowPro());
#else
            menu.AddItem(GetMenuShortcut(TimeflowEditorOverrides.kEnableTimeflow, TimeflowShortcutInfo.Path_TimeflowPro), false, () => TimeflowEditorOverrides.ToggleTimeflowPro());  
#endif
            menu.AddItem(GetMenuShortcut(TimeflowPreferences.kOpenPreferences, TimeflowShortcutInfo.Path_OpenPreferences), false, () => TimeflowPreferences.Open());

            // Show the menu as a dropdown at the current mouse position.
            menu.DropDown(new Rect(mousePosition.x, mousePosition.y, 0, 0));
        }
    }

}//AxonGenesis

#endif

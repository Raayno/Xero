#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AnimationClipsComponentPreset))]
    public class AnimationClipsComponentPresetEditor : ComponentPresetEditorBase<AnimationClipsComponentPreset, AnimationClipsComponentPresetEdit> { }

    public class AnimationClipsComponentPresetEdit : TimeflowChannelComponentPresetEdit<AnimationClipsComponentPreset> 
    {
    }
}

#endif

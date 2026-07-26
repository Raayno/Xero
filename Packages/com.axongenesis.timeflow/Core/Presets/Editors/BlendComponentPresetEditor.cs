#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(BlendComponentPreset))]
    public class BlendComponentPresetEditor : ComponentPresetEditorBase<BlendComponentPreset, BlendComponentPresetEdit> { }

    public class BlendComponentPresetEdit : TimeflowChannelComponentPresetEdit<BlendComponentPreset> 
    {
    }
}

#endif

#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(MotionPathComponentPreset))]
    public class MotionPathComponentPresetEditor : ComponentPresetEditorBase<MotionPathComponentPreset, MotionPathComponentPresetEdit> { }

    public class MotionPathComponentPresetEdit : TimeflowChannelComponentPresetEdit<MotionPathComponentPreset> 
    {
    }
}

#endif

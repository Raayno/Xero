#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(FlybyComponentPreset))]
    public class FlybyComponentPresetEditor : ComponentPresetEditorBase<FlybyComponentPreset, FlybyComponentPresetEdit> { }

    public class FlybyComponentPresetEdit : TimeflowChannelComponentPresetEdit<FlybyComponentPreset> 
    {
    }
}

#endif

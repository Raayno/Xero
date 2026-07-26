#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(TimeflowObjectComponentPreset))]
    public class TimeflowObjectComponentPresetEditor : ComponentPresetEditorBase<TimeflowObjectComponentPreset, TimeflowObjectComponentPresetEdit> { }

    public class TimeflowObjectComponentPresetEdit : ComponentPresetEditBase<TimeflowObjectComponentPreset> 
    {
    }
}

#endif

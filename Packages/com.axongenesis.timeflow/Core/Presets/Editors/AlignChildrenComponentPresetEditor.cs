#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AlignChildrenComponentPreset))]
    public class AlignChildrenComponentPresetEditor : ComponentPresetEditorBase<AlignChildrenComponentPreset, AlignChildrenComponentPresetEdit> { }

    public class AlignChildrenComponentPresetEdit : ComponentPresetEditBase<AlignChildrenComponentPreset> 
    {
    }
}

#endif

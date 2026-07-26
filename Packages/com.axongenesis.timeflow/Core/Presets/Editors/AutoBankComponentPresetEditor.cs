#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AutoBankComponentPreset))]
    public class AutoBankComponentPresetEditor : ComponentPresetEditorBase<AutoBankComponentPreset, AutoBankComponentPresetEdit> { }

    public class AutoBankComponentPresetEdit : ComponentPresetEditBase<AutoBankComponentPreset> 
    {
    }
}

#endif

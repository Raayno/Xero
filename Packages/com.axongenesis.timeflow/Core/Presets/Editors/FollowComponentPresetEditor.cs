#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(FollowComponentPreset))]
    public class FollowComponentPresetEditor : ComponentPresetEditorBase<FollowComponentPreset, FollowComponentPresetEdit> { }

    public class FollowComponentPresetEdit : ComponentPresetEditBase<FollowComponentPreset> 
    {
    }
}

#endif

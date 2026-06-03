using ShaderCrew.SeeThroughShader;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShaderCrew.SeeThroughShader
{
    public class STSShaderGraphGenericEditor : SeeThroughShaderEditorAbstract
    {
        public override void DoGUI(Material material, MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            foreach (MaterialProperty materialProperty in properties)
            {
                if ((materialProperty.propertyFlags & (UnityEngine.Rendering.ShaderPropertyFlags.HideInInspector | UnityEngine.Rendering.ShaderPropertyFlags.PerRendererData)) != 0)
                    continue;

                if (!GeneralUtils.STS_SYNC_PROPERTIES_LIST.Contains(materialProperty.name) && !GeneralUtils.STS_NONSYNC_PROPERTIES_LIST.Contains(materialProperty.name))
                    materialEditor.ShaderProperty(materialProperty, materialProperty.displayName);
            }


        }
    }
}
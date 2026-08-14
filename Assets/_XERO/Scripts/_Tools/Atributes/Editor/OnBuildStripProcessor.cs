using System;
using System.Reflection;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalStripper : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null) return;

        MonoBehaviour[] allComponents = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var component in allComponents)
        {
            if (component == null) continue;

            Type type = component.GetType();
            if (type.GetCustomAttribute<StripOnBuildAttribute>() != null)
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
        }
    }
}

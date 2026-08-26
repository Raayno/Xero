using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using MoreMountains.Feedbacks;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("")]
[FeedbackHelp("Invokes a public method on a target object. Methods with no parameters are treated as instantaneous. Methods accepting a CancellationToken and returning a UniTask are tracked until completion.")]
[Serializable]
[FeedbackPath("Function")]
public class MMF_Function : MMF_Feedback
{
    [Serializable]
    public class FunctionCall
    {
        [SerializeField] private UnityEngine.Object target;
        [SerializeField] private string methodName;

        public UnityEngine.Object Target => target;
        public string MethodName => methodName;

        public MethodInfo GetMethod()
        {
            if (target == null || string.IsNullOrEmpty(methodName))
                return null;

            return target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(IsSupportedMethodByName);
        }

        private bool IsSupportedMethodByName(MethodInfo method)
        {
            return method.Name == methodName && IsSupportedMethod(method);
        }

        public static bool IsSupportedMethod(MethodInfo method)
        {
            if (method.IsSpecialName || method.IsGenericMethod)
                return false;

            ParameterInfo[] parameters = method.GetParameters();

            // Instantaneous:
            // void Foo()
            if (parameters.Length == 0)
            {
                return method.ReturnType == typeof(void);
            }

            // Tracked:
            // UniTask Foo(CancellationToken)
            if (parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(CancellationToken))
            {
                return typeof(UniTask).IsAssignableFrom(method.ReturnType);
            }

            return false;
        }
    }

    [MMFInspectorGroup("Function", true)]
    [SerializeField]
    private FunctionCall functionCall;

    private CancellationTokenSource cancellationTokenSource;
    private UniTask? trackingTask;

    public override bool IsPlaying
    {
        get
        {
            if (trackingTask == null)
                return false;

            return !trackingTask.Value.Status.IsCompleted();
        }
    }

    protected override void CustomPlayFeedback(
        Vector3 position,
        float feedbacksIntensity = 1f)
    {
        if (!Active)
            return;

        // If this feedback is played again while still running,
        // cancel the previous invocation.
        CancelCurrentInvocation();

        if (functionCall == null)
            return;

        MethodInfo method = functionCall.GetMethod();

        if (method == null)
        {
            Debug.LogWarning(
                $"MMF_Function on {(Owner != null ? Owner.name : null)} could not find method '{functionCall.MethodName}'.",
                Owner);
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();

        try
        {
            // ---------------------------------------------------------
            // Instantaneous function:
            // void Foo()
            // ---------------------------------------------------------
            if (parameters.Length == 0)
            {
                method.Invoke(functionCall.Target, null);
                return;
            }

            // ---------------------------------------------------------
            // Tracked function:
            // UniTask Foo(CancellationToken)
            // ---------------------------------------------------------
            cancellationTokenSource = new CancellationTokenSource();

            trackingTask = (UniTask)method.Invoke(
                functionCall.Target,
                new object[] { cancellationTokenSource.Token });
        }
        catch (TargetInvocationException exception)
        {
            Debug.LogException(exception.InnerException ?? exception, Owner);

            CancelCurrentInvocation();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, Owner);

            CancelCurrentInvocation();
        }
    }

    protected override void CustomStopFeedback(
        Vector3 position,
        float feedbacksIntensity = 1f)
    {
        CancelCurrentInvocation();
    }

    private void CancelCurrentInvocation()
    {
        if (cancellationTokenSource != null)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();

            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        trackingTask = null;
    }

#if UNITY_EDITOR

    // ================================================================
    // Inspector
    // ================================================================

    [CustomPropertyDrawer(typeof(FunctionCall))]
    private class FunctionCallDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f + Spacing;
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty targetProperty =
                property.FindPropertyRelative("target");

            SerializedProperty methodProperty =
                property.FindPropertyRelative("methodName");

            float lineHeight = EditorGUIUtility.singleLineHeight;

            Rect targetRect = new (
                position.x,
                position.y,
                position.width,
                lineHeight);

            Rect methodRect = new (
                position.x,
                position.y + lineHeight + Spacing,
                position.width,
                lineHeight);

            EditorGUI.PropertyField(
                targetRect,
                targetProperty,
                new GUIContent("Target"));

            UnityEngine.Object target =
                targetProperty.objectReferenceValue;

            using (new EditorGUI.DisabledScope(target == null))
            {
                MethodInfo[] methods = GetMethods(target);

                if (methods.Length == 0)
                {
                    EditorGUI.Popup(
                        methodRect,
                        "Method",
                        0,
                        new[] { "No compatible methods" });

                    return;
                }

                string[] displayNames = methods
                    .Select(FormatMethodName)
                    .ToArray();

                int selectedIndex = Array.FindIndex(
                    methods,
                    method => method.Name == methodProperty.stringValue);

                if (selectedIndex < 0)
                    selectedIndex = 0;

                int newIndex = EditorGUI.Popup(
                    methodRect,
                    "Method",
                    selectedIndex,
                    displayNames);

                if (newIndex != selectedIndex)
                {
                    methodProperty.stringValue =
                        methods[newIndex].Name;
                }
            }
        }

        private static MethodInfo[] GetMethods(
            UnityEngine.Object target)
        {
            if (target == null)
                return Array.Empty<MethodInfo>();

            return target.GetType()
                .GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(FunctionCall.IsSupportedMethod)
                .ToArray();
        }

        private static string FormatMethodName(
            MethodInfo method)
        {
            ParameterInfo[] parameters =
                method.GetParameters();

            if (parameters.Length == 0)
                return $"{method.Name}()";

            return $"{method.Name}(CancellationToken)";
        }
    }

#endif
}

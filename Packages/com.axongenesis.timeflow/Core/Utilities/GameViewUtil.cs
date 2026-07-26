// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Tools for working with the GameView in the editor.
    /// </summary>
    public static class GameViewUtil
    {
        static object gameViewSizesInstance;
        static MethodInfo getGroup;

        public enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }

        public static EditorWindow GetGameView()
        {
            Type gameViewType = Type.GetType("UnityEditor.GameView, UnityEditor");
            if (gameViewType == null) {
                Debug.LogError("Could not find GameView type.");
                return null;
            }

            // Find an existing Game View window instance
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll(gameViewType) as EditorWindow[];
            if (windows != null && windows.Length > 0) {
                return windows[0]; // Return the first Game View found
            }

            //Debug.LogWarning("No existing Game View found.");
            return null;
        }

        public static Vector2 GetSize()
        {
            Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
            MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            Vector2 size = (Vector2)GetSizeOfMainGameView.Invoke(null, null);

            return size;
        }

        public static void SetSize(int index)
        {
            Type gameViewType = GetGameViewType();
            var selectedSizeIndexProp = gameViewType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            EditorWindow gameViewWindow = GetGameViewWindow();
            if(gameViewWindow != null) selectedSizeIndexProp.SetValue(gameViewWindow, index, null);
        }

        public static void SetScale(float scale)
        {
            Vector2 size = GetSize();
            SetScale(scale, size);
        }

        public static void SetSize(int width, int height)
        {
            GameViewSizeGroupType groupType = GetCurrentGroupType();
            int size = FindSize(groupType, width, height);
            if (size == -1) {
                AddAndSelectCustomSize(GameViewSizeType.FixedResolution, groupType, width, height, width + "x" + height);
            }
            else {
                SetSize(size);
            }
        }

        public static void SetSizeAndScale(Vector2 size, float scale)
        {
            SetSize((int)size.x, (int)size.y);
            SetScale(scale, size);
        }

        public static void SetScale(float scale, Vector2 size)
        {
            Type gameViewType = GetGameViewType();
            EditorWindow gameViewWindow = GetGameViewWindow();
            if (gameViewWindow == null) return; // No game view active in the view

            float scaleX = gameViewWindow.position.width / size.x;
            float scaleY = gameViewWindow.position.height / size.y;

            scale = Mathf.Max(scale, Mathf.Min(scaleX, scaleY));

            if (gameViewWindow == null) {
                Debug.LogError("GameView is null!");
                return;
            }

            var defScaleField = gameViewType.GetField("m_defaultScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var areaField = gameViewType.GetField("m_ZoomArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var areaObj = areaField.GetValue(gameViewWindow);

            var scaleField = areaObj.GetType().GetField("m_Scale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            scaleField.SetValue(areaObj, new Vector2(scale, scale));
        }

        public static void AddAndSelectCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            AddCustomSize(viewSizeType, sizeGroupType, width, height, text);
            int idx = GameViewUtil.FindSize(GameViewSizeGroupType.Standalone, width, height);
            GameViewUtil.SetSize(idx);
        }

        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            string assemblyName = "UnityEditor.dll";
            Assembly assembly = Assembly.Load(assemblyName);
            Type gameViewSize = assembly.GetType("UnityEditor.GameViewSize");
            Type gameViewSizeType = assembly.GetType("UnityEditor.GameViewSizeType");
            ConstructorInfo ctor = gameViewSize.GetConstructor(new Type[]
                {
                 gameViewSizeType,
                 typeof(int),
                 typeof(int),
                 typeof(string)
                });
            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }

        public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
        {
            var group = GetGroup(sizeGroupType);
            var groupType = group.GetType();
            var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
            var getCustomCount = groupType.GetMethod("GetCustomCount");
            int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            var gvsType = getGameViewSize.ReturnType;
            var widthProp = gvsType.GetProperty("width");
            var heightProp = gvsType.GetProperty("height");
            var indexValue = new object[1];
            for (int i = 0; i < sizesCount; i++) {
                indexValue[0] = i;
                var size = getGameViewSize.Invoke(group, indexValue);
                int sizeWidth = (int)widthProp.GetValue(size, null);
                int sizeHeight = (int)heightProp.GetValue(size, null);
                if (sizeWidth == width && sizeHeight == height)
                    return i;
            }
            return -1;
        }

        static GameViewSizeGroupType GetCurrentGroupType()
        {
            GameViewSizeGroupType type = GameViewSizeGroupType.Standalone;
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
                type = GameViewSizeGroupType.Android;
            }
            else
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) {
                type = GameViewSizeGroupType.iOS;
            }
            return type;
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            if (gameViewSizesInstance == null) GetGameViewSizes();
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
        }

        private static void GetGameViewSizes()
        {
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup = sizesType.GetMethod("GetGroup");
            gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        private static Type GetGameViewType()
        {
            Assembly unityEditorAssembly = typeof(EditorWindow).Assembly;
            Type gameViewType = unityEditorAssembly.GetType("UnityEditor.GameView");
            return gameViewType;
        }

        private static EditorWindow GetGameViewWindow()
        {
            UnityEngine.Object[] obj = UnityEngine.Resources.FindObjectsOfTypeAll(GetGameViewType());
            if (obj.Length > 0) {
                return obj[0] as EditorWindow;
            }
            return null;
        }
    }

}//AxonGenesis
#endif

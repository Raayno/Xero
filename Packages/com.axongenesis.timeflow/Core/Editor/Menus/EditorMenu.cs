// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>  
    /// Registers utility functions in the main menu under Tools > AxonGenesis  
    /// </summary>  
    public class EditorMenu
    {
        #region SHORTCUT CONSTANTS  


        #endregion

        #region FILE  

        [Shortcut(TimeflowShortcutInfo.Path_SaveSceneBackup, KeyCode.S, ShortcutModifiers.Alt | ShortcutModifiers.Action)]
        [UnityEditor.MenuItem("File/Save Scene Backup", false, 170)]
        public static void SaveSceneBackup()
        {
            AxonTools.SaveSceneIncrementalBackup();
        }

        #endregion

        #region COMPONENT  

        [Shortcut(TimeflowShortcutInfo.Path_AddBoolField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Bool", false, 200)]
        public static void AddBoolField()
        {
            AxonTools.AddBoolField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Bool", true)]
        public static bool ValidateAddBoolField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddColorField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Color", false, 201)]
        public static void AddColorField()
        {
            AxonTools.AddColorField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Color", true)]
        public static bool ValidateAddColorField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddComponentField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Component", false, 202)]
        public static void AddComponentField()
        {
            AxonTools.AddComponentField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Component", true)]
        public static bool ValidateAddComponentField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddFloatField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Float", false, 203)]
        public static void AddFloatFieldField()
        {
            AxonTools.AddFloatField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Float", true)]
        public static bool ValidateAddFloatField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddGameObjectField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/GameObject", false, 204)]
        public static void AddGameObjectField()
        {
            AxonTools.AddGameObjectField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/GameObject", true)]
        public static bool ValidateAddGameObjectField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddRectField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Rect", false, 205)]
        public static void AddRectField()
        {
            AxonTools.AddRectField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Rect", true)]
        public static bool ValidateAddRectField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddStringField)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/String", false, 206)]
        public static void AddStringField()
        {
            AxonTools.AddStringField();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/String", true)]
        public static bool ValidateAddStringField()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddVector2Field)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector2", false, 207)]
        public static void AddVector2Field()
        {
            AxonTools.AddVector2Field();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector2", true)]
        public static bool ValidateAddVector2Field()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddVector3Field)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector3", false, 208)]
        public static void AddVector3Field()
        {
            AxonTools.AddVector3Field();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector3", true)]
        public static bool ValidateAddVector3Field()
        {
            return Selection.activeTransform;
        }

        [Shortcut(TimeflowShortcutInfo.Path_AddVector4Field)]
        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector4", false, 209)]
        public static void AddVector4Field()
        {
            AxonTools.AddVector4Field();
        }

        [UnityEditor.MenuItem("Component/Timeflow/Add Field Value/Vector4", true)]
        public static bool ValidateAddVector4Field()
        {
            return Selection.activeTransform;
        }

        #endregion
    }

}//AxonGenesis

#endif

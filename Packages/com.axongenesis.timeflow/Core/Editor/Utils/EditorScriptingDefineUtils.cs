// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;

namespace AxonGenesis
{
    /// <summary>
    /// Helper utility for getting and setting scripting define symbols in the player settings.
    /// </summary>
    public class EditorScriptingDefineUtils
    {
        /// <summary>
        /// Checks whether a specific namespace exists or not and adds or removes a scripting define symbol
        /// from the player settings.
        /// </summary>
        /// <param name="name_space"></param>
        /// <param name="symbol"></param>
        public static void UpdateSymbolIfNamespaceExists(string name_space, string symbol)
        {
            bool exists = NamespaceExists(name_space);
            if (exists) {
                AddScriptingDefineSymbol(symbol);
            }
            else {
                RemoveScriptingDefineSymbol(symbol);
            }
        }

        /// <summary>
        /// Checks whether a specific namespace exists using reflection.
        /// </summary>
        /// <param name="desiredNamespace"></param>
        /// <returns></returns>
        public static bool NamespaceExists(string desiredNamespace)
        {
            foreach (Type type in ReflectionUtil.GetTypes()) {
                if (type.Namespace == desiredNamespace) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of the scripting define symbols from the player settings of the current build
        /// target.
        /// </summary>
        public static List<string> GetScriptingDefineSymbols()
        {
#if UNITY_2022_1_OR_NEWER
            string defined = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
#else
            string defined = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            if (string.IsNullOrEmpty(defined)) {
                return new List<string>();
            }
            return defined.Split(';').ToList<string>();
        }

        /// <summary>
        /// Adds a scripting define symbol to the player settings current build target. If the symbol already
        /// exists, nohting is changed.
        /// </summary>
        public static bool HasScriptingDefineSymbol(string symbol)
        {
            List<string> defines = GetScriptingDefineSymbols();
            return defines.Contains(symbol);
        }

        /// <summary>
        /// Adds a scripting define symbol to the player settings current build target. If the symbol already
        /// exists, nohting is changed.
        /// </summary>
        public static void AddScriptingDefineSymbol(string symbol)
        {
            List<string> defines = GetScriptingDefineSymbols();
            if (!defines.Contains(symbol)) {
                defines.Add(symbol);
                SetScriptingDefineSymbols(defines);
            }
        }

        /// <summary>
        /// Removes the specified scripting define symbol from the player settings in the current build target.
        /// If the symbol is not defined, no changes are applied.
        /// </summary>
        public static void RemoveScriptingDefineSymbol(string symbol)
        {
            List<string> defines = GetScriptingDefineSymbols();
            if (defines.Contains(symbol)) {
                defines.Remove(symbol);
                SetScriptingDefineSymbols(defines);
            }
        }

        /// <summary>
        /// Sets multiply symbols provided in a list of strings. These are added to the existing scripting
        /// define symbols in the player settings of the current build target.
        /// </summary>
        /// <param name="defines"></param>
        public static void SetScriptingDefineSymbols(List<string> defines)
        {
            string newDefines = "";
            if (defines != null && defines.Count > 0) {
                newDefines = string.Join(";", defines.ToArray());
            }
#if UNITY_2022_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), newDefines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
#endif
        }

    }

}//AxonGenesis

#endif
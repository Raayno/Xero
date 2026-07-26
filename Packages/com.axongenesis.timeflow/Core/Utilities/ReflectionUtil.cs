// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Global utility methods for working with derrived classes and interfaces.
    /// </summary>
    public static class ReflectionUtil
    {
        public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            var result = new List<Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                var types = assembly.GetTypes();
                foreach (var type in types) {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }
            return result.ToArray();
        }

        public static Type[] GetAllDerivedTypes<T>(this AppDomain aAppDomain)
        {
            return GetAllDerivedTypes(aAppDomain, typeof(T));
        }

        public static Type[] GetTypesWithInterface(this AppDomain aAppDomain, Type aInterfaceType)
        {
            Assembly[] assemblies = aAppDomain.GetAssemblies();

            /// Check that the assembly types may be read to avoid breaking exception conditions. This
            /// should never occur, but may happen if there is an invalid DLL or library in the project.
            List<Assembly> searchable = new List<Assembly>();
            foreach (Assembly assembly in assemblies) {
                Type[] types = null;
                try {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex) {
                    Debug.LogError(ex.Message);
                    types = null;
                }
                if (types != null) {
                    searchable.Add(assembly);
                }
            }

            assemblies = searchable.ToArray();
            return assemblies.SelectMany(x => x.GetTypes())
                .Where(x => aInterfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x).ToArray();
        }

        public static Type[] GetTypesWithInterface<T>(this AppDomain aAppDomain)
        {
            return GetTypesWithInterface(aAppDomain, typeof(T));
        }

        /// <summary>
        /// Gets all fields from an object and its hierarchy inheritance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>All fields of the type.</returns>
        public static List<FieldInfo> GetAllFields(this Type type, BindingFlags flags)
        {
            // Early exit if Object type
            if (type == typeof(System.Object)) {
                return new List<FieldInfo>();
            }

            // Recursive call
            var fields = type.BaseType.GetAllFields(flags);
            fields.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
            return fields;
        }

        public static IEnumerable<Type> GetTypes(string nameSearch = null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(a => !a.IsDynamic && !a.FullName.StartsWith("Microsoft.GeneratedCode") &&
                     (nameSearch == null || a.FullName.ToLower().Contains(nameSearch.ToLower())))) {

                Type[] asmTypes = null;
                try {
                    asmTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex) {
                    asmTypes = ex.Types; // includes nulls
                    foreach (var loaderEx in ex.LoaderExceptions)
                        Debug.LogWarning($"[Reflection] {loaderEx.Message}");
                }

                foreach (var t in asmTypes.Where(t => t != null))
                    yield return t;
            }
        }

        public static IEnumerable<Type> GetTypes<TBase>(string nameSearch = null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(a => !a.IsDynamic && !a.FullName.StartsWith("Microsoft.GeneratedCode") &&
                     (nameSearch == null || a.FullName.ToLower().Contains(nameSearch.ToLower())))) {

                Type[] asmTypes = null;
                try {
                    asmTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex) {
                    asmTypes = ex.Types; // includes nulls
                    foreach (var loaderEx in ex.LoaderExceptions)
                        Debug.LogWarning($"[Reflection] {loaderEx.Message}");
                }

                foreach (var t in asmTypes.Where(t => t != null && typeof(TBase).IsAssignableFrom(t)))
                    yield return t;
            }
        }

        /// <summary>
        /// Perform a deep copy of the class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A deep copy of obj.</returns>
        /// <exception cref="System.ArgumentNullException">Object cannot be null</exception>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null) {
                throw new ArgumentNullException("Object cannot be null");
            }
            return (T)DoCopy(obj);
        }


        /// <summary>
        /// Does the copy.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Unknown type</exception>
        private static object DoCopy(object obj)
        {
            if (obj == null) {
                return null;
            }

            // Value type
            var type = obj.GetType();
            if (type.IsValueType || type == typeof(string)) {
                return obj;
            }

            // Array
            else if (type.IsArray) {
                Type elementType = type.GetElementType();
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++) {
                    copied.SetValue(DoCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }

            // Unity Object
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                return obj;
            }

            // Class -> Recursion
            else if (type.IsClass) {
                string typeName = obj.GetType() + "";

                /// Prevent copying cached invoke methods
                if (typeName.Contains("Cached")) return null;

                object copy = Activator.CreateInstance(obj.GetType());

                var fields = type.GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields) {
                    var fieldValue = field.GetValue(obj);
                    if (fieldValue != null) {
                        field.SetValue(copy, DoCopy(fieldValue));
                    }
                }

                return copy;
            }

            // Fallback
            else {
                throw new ArgumentException("Unknown type");
            }
        }
    }
}//AxonGenesis
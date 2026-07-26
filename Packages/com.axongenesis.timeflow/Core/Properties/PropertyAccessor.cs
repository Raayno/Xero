using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace AxonGenesis
{
    public class PropertyAccessor<TObject> : IPropertyAccessor where TObject : class
    {
        private readonly TObject instance;
        private readonly Type memberType;
        private Delegate getter;
        private Delegate setter;
        private readonly bool canGet;
        private readonly bool canSet;

        private PropertyInfo Property;
        private FieldInfo Field;
        private MemberInfo member;

        private List<string> _Warnings = new List<string>();

        // Logs warning just once per instance to avoid excessive logging
        private void LogWarning(string name, string warning)
        {
            if (!_Warnings.Contains(name)) {
                _Warnings.Add(name);
                if(instance is UnityEngine.Object unityObject) {
                    Debug.LogWarning($"{name} : {warning}", unityObject);
                    return;
                }
                Debug.LogWarning($"{name} : {warning}");
            }
        }

        /// <summary>
        /// Initializes a new instance of the PropertyAccessor class.
        /// </summary>
        /// <param name="instance">The object instance to access.</param>
        /// <param name="memberName">The name of the field or property to target.</param>
        /// <exception cref="ArgumentNullException">Thrown if instance is null.</exception>
        /// <exception cref="ArgumentException">Thrown if memberName is null, empty, or the member is not found.</exception>
        public PropertyAccessor(TObject instance, string memberName)
        {
            // Validate inputs
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

            // Get the actual runtime type of the instance
            Type actualType = instance.GetType();

            // Find the member (property or field) using reflection
            MemberInfo[] members = actualType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            member = null;
            foreach (var m in members) {
                if (m is PropertyInfo p && p.CanRead && p.CanWrite) {
                    member = m;
                    break;
                }
                else if (m is FieldInfo field && !field.IsInitOnly) // Check if field is not readonly
                {
                    member = m;
                    break;
                }
            }

            if (member == null) {
                LogWarning("acutalType.GetMember", $"No readable and writable field or property named '{memberName}' found in type '{actualType}'.");
                return;
            }

            memberType = member switch {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new InvalidOperationException("Unexpected member type.")
            };

            if (member is PropertyInfo propertyInfo) {
                Property = propertyInfo;
                if (propertyInfo.CanRead) {
                    getter = CreateGetter(propertyInfo, actualType, memberType);
                    canGet = true;
                }
                if (propertyInfo.CanWrite) {
                    setter = CreateSetter(propertyInfo, actualType, memberType);
                    canSet = true;
                }
            }
            else if (member is FieldInfo fieldInfo) {
                Field = fieldInfo;
                getter = CreateGetter(fieldInfo, actualType, memberType);
                canGet = true;
                if (!fieldInfo.IsInitOnly) {
                    setter = CreateSetter(fieldInfo, actualType, memberType);
                    canSet = true;
                }
            }

            if (!canGet && !canSet)
                LogWarning("!canGet && !canSet", "Member cannot be read or written.");

            if (getter == null)
                LogWarning("getter == null", $"{actualType}.{memberName}: Getter is null.");

            if (setter == null)
                LogWarning("setter == null", $"{actualType}.{memberName}: Setter is null.");
        }

        /// <summary>
        /// Gets the value of the targeted field or property.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <returns>The value of the field or property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the member cannot be read or if T does not match the member type.</exception>
        public T GetValue<T>()
        {
            if (!canGet) {
                LogWarning("!canGet", $"Member cannot be read.");
                return default(T);
            }
            if (memberType.IsEnum || typeof(T).IsEnum) {
                if (member is PropertyInfo propertyInfo) {
                    Enum val = (Enum)Property.GetGetMethod().Invoke(instance, null);
                    return (T)Convert.ChangeType(val, typeof(T));
                }
                else if (member is FieldInfo fieldInfo) {
                    Enum val = (Enum)Field.GetValue(instance);
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }

            if (typeof(T) != memberType) {
                LogWarning("typeof(T) != memberType", $"Expected type '{memberType}', but got '{typeof(T)}'.");
                return default(T);
            }

            try {
                var func = (Func<TObject, T>)getter;
                return func(instance);
            }
            catch (Exception ex) {
                Debug.LogError($"Error while getting value: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Sets the value of the targeted field or property.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="value">The value to set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the member cannot be written or if T does not match the member type.</exception>
        public void SetValue<T>(T value)
        {
            if (!canSet) {
                LogWarning("!canSet", "Member cannot be set.");
                return;
            }
            if (memberType.IsEnum || typeof(T).IsEnum) {
                if (member is PropertyInfo propertyInfo) {
                    Property.SetValue(instance, value);
                    return;
                }
                else if (member is FieldInfo fieldInfo) {
                    Field.SetValue(instance, value);
                    return;
                }
            }
            if (typeof(T) != memberType) {
                LogWarning("typeof(T) != memberType", $"Expected type '{memberType}', but got '{typeof(T)}'.");
                return;
            }

            try {
                var action = (Action<TObject, T>)setter;
                action(instance, value);
            }
            catch (Exception ex) {
                LogWarning("Exception", $"Error while setting value: {ex.Message}");
            }
        }

        private Delegate CreateGetter(PropertyInfo property, Type actualType, Type memberType)
        {
            var param = Expression.Parameter(typeof(TObject), "obj");
            var cast = Expression.Convert(param, actualType);
            var access = Expression.Property(cast, property);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TObject), memberType);
            var lambda = Expression.Lambda(delegateType, access, param);
            return lambda.Compile();
        }

        private Delegate CreateSetter(PropertyInfo property, Type actualType, Type memberType)
        {
            var paramObj = Expression.Parameter(typeof(TObject), "obj");
            var paramValue = Expression.Parameter(memberType, "value");
            var castObj = Expression.Convert(paramObj, actualType);
            var access = Expression.Property(castObj, property);
            var assign = Expression.Assign(access, paramValue);
            var delegateType = typeof(Action<,>).MakeGenericType(typeof(TObject), memberType);
            var lambda = Expression.Lambda(delegateType, assign, paramObj, paramValue);
            return lambda.Compile();
        }

        private Delegate CreateGetter(FieldInfo field, Type actualType, Type memberType)
        {
            var param = Expression.Parameter(typeof(TObject), "obj");
            var cast = Expression.Convert(param, actualType);
            var access = Expression.Field(cast, field);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TObject), memberType);
            var lambda = Expression.Lambda(delegateType, access, param);
            return lambda.Compile();
        }

        private Delegate CreateSetter(FieldInfo field, Type actualType, Type memberType)
        {
            var paramObj = Expression.Parameter(typeof(TObject), "obj");
            var paramValue = Expression.Parameter(memberType, "value");
            var castObj = Expression.Convert(paramObj, actualType);
            var access = Expression.Field(castObj, field);
            var assign = Expression.Assign(access, paramValue);
            var delegateType = typeof(Action<,>).MakeGenericType(typeof(TObject), memberType);
            var lambda = Expression.Lambda(delegateType, assign, paramObj, paramValue);
            return lambda.Compile();
        }
    }
}
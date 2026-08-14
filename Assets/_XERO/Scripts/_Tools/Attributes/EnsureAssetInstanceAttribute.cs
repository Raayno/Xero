using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class EnsureAssetInstanceAttribute : PropertyAttribute
{
    public string Name { get; private set; }
    public EnsureAssetInstanceAttribute(string name = null) => Name = name;
}

using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class SuffixAttribute : PropertyAttribute
{
    public string Suffix { get; private set; }
    public SuffixAttribute(string suffix) => Suffix = suffix;
}

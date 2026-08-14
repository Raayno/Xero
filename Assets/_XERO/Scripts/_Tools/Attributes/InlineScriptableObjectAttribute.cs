using System;
using UnityEngine;

public class InlineScriptableObjectAttribute : PropertyAttribute
{
    public readonly bool expandedByDefault;
    public readonly bool drawBox;
    public readonly bool hideScriptField;
    public readonly Type requiredType;

    public InlineScriptableObjectAttribute(
        bool expandedByDefault = true,
        bool drawBox = true,
        bool hideScriptField = true)
    {
        this.expandedByDefault = expandedByDefault;
        this.drawBox = drawBox;
        this.hideScriptField = hideScriptField;
        requiredType = null;
    }

    public InlineScriptableObjectAttribute(
        Type requiredType,
        bool expandedByDefault = true,
        bool drawBox = true,
        bool hideScriptField = true)
    {
        this.expandedByDefault = expandedByDefault;
        this.drawBox = drawBox;
        this.hideScriptField = hideScriptField;
        this.requiredType = requiredType;
    }
}
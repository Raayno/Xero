using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SuffixAttribute))]
public class SuffixDrawer : PropertyDrawer
{
    private const float SuffixPadding = 15f; // Padding between the field and the suffix
    private const float AvgCharWidth = 7f; // Average width of a character in pixels (approximation)
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SuffixAttribute suffixAttr = (SuffixAttribute)attribute;

        // Calculate layout positions
        Rect fieldRect = new(position.x, position.y, position.width - suffixAttr.Suffix.Length * AvgCharWidth + SuffixPadding, position.height);
        Rect labelRect = new(position.xMax - suffixAttr.Suffix.Length * AvgCharWidth, position.y, suffixAttr.Suffix.Length * AvgCharWidth, position.height);

        // Draw the standard string property field
        EditorGUI.PropertyField(fieldRect, property, label);

        // Draw the neat suffix text right next to it
        GUIStyle suffixStyle = new(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
        EditorGUI.LabelField(labelRect, suffixAttr.Suffix, suffixStyle);
    }
}
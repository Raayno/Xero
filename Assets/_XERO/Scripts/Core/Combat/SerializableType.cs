using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [Tooltip("Name of the type.\nShould match the class/file (without extension) name.\n\n', Assembly-CSharp' is added to the end of the name automatically.")]
    [Suffix(", Assembly-CSharp")][SerializeField] private string name;

    // To jest właściwość runtime'owa (nie serializuje się bezpośrednio)
    public Type Type { get; set; }

    public SerializableType() { }

    public SerializableType(Type type)
    {
        Type = type;
        name = type?.AssemblyQualifiedName;
    }

    // Wywołuje się AUTOMATYCZNIE przed zapisem pliku przez Unity
    public void OnBeforeSerialize()
    {
        if (Type != null)
        {
            name = Type.AssemblyQualifiedName;
        }
    }

    // Wywołuje się AUTOMATYCZNIE po wczytaniu pliku przez Unity
    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(name))
        {
            Type = Type.GetType(name + ", Assembly-CSharp");
        }
    }
}
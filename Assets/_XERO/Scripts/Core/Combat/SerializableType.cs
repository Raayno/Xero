using System;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [Tooltip("Name of the type. Should match file name. (for getting the type ', Assembly-CSharp' is added automatically to make it a fully qualified name)")]
    [SerializeField] private string name;

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
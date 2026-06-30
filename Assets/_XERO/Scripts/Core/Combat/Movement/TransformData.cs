using UnityEngine;

public struct TransformData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public static TransformData FromTransform(Transform transform)
    {
        return new TransformData(transform.position, transform.rotation, transform.localScale);
    }
}
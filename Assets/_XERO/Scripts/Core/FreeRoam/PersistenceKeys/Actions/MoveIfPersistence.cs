using UnityEngine;

[RequireComponent(typeof(PersistenceKey))]
public class PersistenceMover : MonoBehaviour
{
    [SerializeField] private PersistenceKey persistenceKey;
    [SerializeField, HideInInspector] private PersistenceRegistry registry;

    private void Awake()
    {
        object value = registry.GetValue(persistenceKey.Key);

        if (value == null) return;
        
        if (value is Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    private void OnDestroy()
    {
        transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        registry.SetValue(persistenceKey.Key, value: new Pose(position, rotation), isClearable: true);
    }

    void OnValidate()
    {
        if (persistenceKey == null)
        {
            persistenceKey = GetComponent<PersistenceKey>();
        }
    }
}

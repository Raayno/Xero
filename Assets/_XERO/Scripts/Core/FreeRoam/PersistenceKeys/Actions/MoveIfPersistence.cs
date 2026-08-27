using UnityEngine;

[RequireComponent(typeof(PersistenceKey))]
public class PersistenceMover : MonoBehaviour
{
    [SerializeField] private bool saveOnDestroy = false;
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
        if (saveOnDestroy) SavePose();
    }

    public void SavePose()
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

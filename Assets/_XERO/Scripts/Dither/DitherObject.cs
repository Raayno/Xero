using UnityEngine;

public class DitherObject : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private Renderer[] renderers;

    [Header("Shader")]
    [SerializeField] private string floatProperty = "_Alpha";

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 8f;

    [SerializeField] private float defaultValue = 1f;

    private MaterialPropertyBlock propertyBlock;
    private int propertyID;

    private float currentValue;
    private float targetValue;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        propertyBlock = new MaterialPropertyBlock();
        propertyID = Shader.PropertyToID(floatProperty);

        currentValue = defaultValue;
        targetValue = defaultValue;

        ApplyValue(currentValue);
    }

    private void Update()
    {
        if (Mathf.Approximately(currentValue, targetValue))
            return;

        currentValue = Mathf.MoveTowards(
            currentValue,
            targetValue,
            animationSpeed * Time.deltaTime);

        ApplyValue(currentValue);
    }

    public void SetValue(float value)
    {
        targetValue = value;
    }

    public void SetValueImmediate(float value)
    {
        currentValue = targetValue = value;
        ApplyValue(currentValue);
    }

    private void ApplyValue(float value)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(propertyID, value);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
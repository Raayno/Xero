using UnityEngine;
using MoreMountains.Tools;

public class ParryInput : MMSingleton<ParryInput>
{
    public event System.Action OnParry;
    public bool IsEnabled = false;
    [SerializeField] private bool enableDebug = false;

    protected override void Awake()
    {
        base.Awake();
        IsEnabled = false;
    }

    private void Update()
    {
        if (!IsEnabled) return;

        // TODO: Replace this with the actual input detection logic for parry
        if (Input.GetMouseButtonDown(0)) // Temporarily hardcoded left mouse button for parry input
        {
            OnParry?.Invoke();
            if (enableDebug) Debug.Log($"<color=yellow>[ParryInput]</color>Parry input detected.");
        }
    }
}
public class EyesTag : UnityEngine.MonoBehaviour
{
    [UnityEngine.Header("References for Enemy Behavior Graph (Player Eyes)")]
    public ParryThirdPersonControllerExtension ParryExtension;

    void Reset()
    {
        if (ParryExtension == null)
        {
            ParryExtension = transform.parent.GetComponentInChildren<ParryThirdPersonControllerExtension>();
        }
    }
}

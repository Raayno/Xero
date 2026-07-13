public class EyesTag : UnityEngine.MonoBehaviour
{
    [UnityEngine.Header("References for Enemy Behavior Graph (Player Eyes)")]
    public ParryThirdPersonControllerExtension ParryExtension;
    public int NumbersOfEnemiesChasingThisPlayer { get; set;} = 0;

    void Reset()
    {
        if (ParryExtension == null)
        {
            ParryExtension = transform.parent.GetComponentInChildren<ParryThirdPersonControllerExtension>();
        }
    }
}

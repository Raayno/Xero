// Stupid BlackboardVariable<T> doesn't support AnimationCurve, so we have to use a wrapper class to make it work.
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurveWrapper", menuName = "Behavior/AnimationCurveWrapper")]
public class AnimationCurveWrapper : ScriptableObject
{
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public AnimationCurve Curve => curve;
}

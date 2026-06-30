using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ParticipantMovable : MonoBehaviour
{
    protected Transform attackTransform;
    protected float duration;

    public void StartAttackMovement(TransformData targetHitTransform, float moveToTargetDuration)
    {
        if (TryGetComponent<AttackTransform>(out var attackTransformComponent))
        {
            attackTransform = attackTransformComponent.transform;
        }
        else
        {
            Debug.LogWarning($"No AttackTransform component found on {gameObject.name}. Using the current transform instead.");
            attackTransform = null;
        }

        duration = moveToTargetDuration;
        
        StartCoroutine(StartAttackMovement(targetHitTransform));
    }

    public void ReturnToOriginalPosition(float returnDuration)
    {
        duration = returnDuration;
        Continue();
    }

    private IEnumerator StartAttackMovement(TransformData targetHitTransform)
    {
        yield return MoveToTarget(targetHitTransform);
        yield return WaitForContinue();
    }

    protected virtual IEnumerator MoveToTarget(TransformData targetHitTransform)
    {
        // Get the relative position and rotation of the target transform with respect to the attack transform
        targetHitTransform.Rotation = Quaternion.Inverse(targetHitTransform.Rotation);
        if (attackTransform != null)
        {
            targetHitTransform.Position += attackTransform.position - transform.position;
            targetHitTransform.Scale = new(targetHitTransform.Scale.x/attackTransform.localScale.x, targetHitTransform.Scale.y/attackTransform.localScale.y, targetHitTransform.Scale.z/attackTransform.localScale.z);
        }

        // Move to target
        Sequence moveSequence = DOTween.Sequence(false);
        moveSequence.Append(transform.DOMove(targetHitTransform.Position, duration));
        moveSequence.Join(transform.DORotateQuaternion(targetHitTransform.Rotation, duration));
        moveSequence.Join(transform.DOScale(targetHitTransform.Scale, duration));
        
        moveSequence.SetAutoKill(false);
        moveSequence.Play();

        yield return moveSequence.WaitForCompletion();

        // Wait for continue signal
        yield return WaitForContinue();

        // Return to original position
        moveSequence.PlayBackwards();

        yield return moveSequence.WaitForRewind();
        moveSequence.Kill();
    }

    private bool isContinue = false;
    private IEnumerator WaitForContinue()
    {
        yield return new WaitUntil(() => isContinue);
        isContinue = false;
    }

    private void Continue()
    {
        isContinue = true;
    }
}
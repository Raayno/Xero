using UnityEngine;

namespace StarterAssets
{
    public class PlayerAnimationManager : MonoBehaviour
    {
        private Animator _animator;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIsJumpEnd;
        private int _animIDMotionSpeed;

        public bool HasAnimator => _animator != null;

        public void Initialize()
        {
            TryGetComponent(out _animator);
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIsJumpEnd = Animator.StringToHash("IsJumpEnd");
        }

        public void SetMovementBlend(float animationBlend, float inputMagnitude)
        {
            if (!HasAnimator)
                return;

            _animator.SetFloat(_animIDSpeed, animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        public void SetGrounded(bool grounded)
        {
            if (!HasAnimator)
                return;

            _animator.SetBool(_animIDGrounded, grounded);
        }

        public void SetJump(bool jump)
        {
            if (!HasAnimator)
                return;

            _animator.SetBool(_animIDJump, jump);
        }

        public void SetFreeFall(bool freeFall)
        {
            if (!HasAnimator)
                return;

            _animator.SetBool(_animIDFreeFall, freeFall);
        }

        public void SetJumpEnd(bool isJumpEnd)
        {
            if (!HasAnimator)
                return;

            _animator.SetBool(_animIsJumpEnd, isJumpEnd);
        }

        public void PlayAttackAnimation(string attackStateName, float crossFadeDuration)
        {
            if (!HasAnimator)
                return;

            _animator.speed = 1f;

            _animator.CrossFadeInFixedTime(
                attackStateName,
                crossFadeDuration,
                0,
                0f
            );
        }

        public void PlayIdleBlendTree(string idleBlendTreeStateName, float crossFadeDuration)
        {
            if (!HasAnimator)
                return;

            _animator.speed = 1f;

            _animator.CrossFadeInFixedTime(
                idleBlendTreeStateName,
                crossFadeDuration,
                0,
                0f
            );
        }

        public AnimatorStateInfo GetCurrentBaseLayerStateInfo()
        {
            if (!HasAnimator)
                return default;

            return _animator.GetCurrentAnimatorStateInfo(0);
        }

        public bool IsInBaseLayerTransition()
        {
            if (!HasAnimator)
                return false;

            return _animator.IsInTransition(0);
        }

        public bool IsCurrentBaseLayerState(string stateName)
        {
            if (!HasAnimator)
                return false;

            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            return currentState.IsName(stateName);
        }
    }
}